﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

using set.locale.Data.Entities;
using set.locale.Helpers;
using set.locale.Models;

namespace set.locale.Data.Services
{
    public class AppService : BaseService, IAppService
    {
        public async Task<string> Create(AppModel model)
        {
            if (model.IsNotValid() 
                || (await GetByName(model.Name)) != null)
            {
                return null;
            }

            var app = new App
            {
                UserEmail = model.Email,
                Name = model.Name,
                Url = model.Url,
                IsActive = true,
                CreatedBy = model.CreatedBy,
                Description = model.Description ?? string.Empty,
                Tokens = new List<Token>
                {
                    new Token { CreatedBy = model.CreatedBy, Key = Guid.NewGuid().ToNoDashString(), UsageCount = 0,IsAppActive = true }
                }
            };
           
            Context.Apps.Add(app);
            Context.Entry(app).State = EntityState.Added;
            if (Context.SaveChanges() > 0)
            {
                return await Task.FromResult(app.Id);
            }

            return null;
        }

        public Task<App> GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var app = Context.Apps.Include(x => x.Tokens).FirstOrDefault(x => x.Name == name);
            return Task.FromResult(app);
        }

        public Task<App> GetByUrlName(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return null;
            }

            var app = Context.Apps.Include(x => x.Tokens).FirstOrDefault(x => string.Equals(x.Name, tag, StringComparison.InvariantCultureIgnoreCase));
            return Task.FromResult(app);
        }

        public Task<bool> CreateToken(TokenModel model)
        {
            if (model.IsNotValid())
            {
                return Task.FromResult(false);
            }

            if (!Context.Apps.Any(x => x.Id == model.AppId))
            {
                return Task.FromResult(false);
            }

            var entity = new Token
            {
                CreatedBy = model.CreatedBy,
                AppId = model.AppId,
                Key = model.Token,
                UsageCount = 0,
                IsAppActive = true
            };
            Context.Tokens.Add(entity);
            Context.Entry(entity).State = EntityState.Added;

            var isAdded = Context.SaveChanges() > 0;
            return Task.FromResult(isAdded);
        }

        public Task<PagedList<App>> GetApps(int pageNumber)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            var items = Context.Apps;

            long totalCount = items.Count();
            var totalPageCount = (int)Math.Ceiling(totalCount / (double)ConstHelper.PageSize);

            if (pageNumber > totalPageCount)
            {
                pageNumber = 1;
            }

            var model = items.OrderByDescending(x => x.Id).Skip(ConstHelper.PageSize * (pageNumber - 1)).Take(ConstHelper.PageSize);

            return Task.FromResult(new PagedList<App>(pageNumber, ConstHelper.PageSize, totalCount, model));
        }

        public Task<List<App>> GetByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var apps = Context.Apps.Where(x => x.CreatedBy == userId);
            return Task.FromResult(apps.ToList());
        }

        public Task<App> Get(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                return null;
            }

            var app = Context.Apps.Include(x => x.Tokens).FirstOrDefault(x => x.Id == appId);
            return Task.FromResult(app);
        }

        public Task<bool> ChangeStatus(string appId, string updatedBy, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(updatedBy)) return Task.FromResult(false);

            var user = Context.Users.FirstOrDefault(x => x.Id == updatedBy);
            if (user == null) return Task.FromResult(false);

            var roleId = ConstHelper.BasicRoles[ConstHelper.Admin];
            var app = Context.Apps.Include(x => x.Tokens).FirstOrDefault(x => x.Id == appId && (x.CreatedBy == updatedBy
                                                                                                || user.RoleId == roleId));
            if (app == null) return Task.FromResult(false);

            foreach (var token in app.Tokens)
            {
                token.IsAppActive = !isActive;
            }

            app.IsActive = !isActive;
            Context.Entry(app).State = EntityState.Modified;
            return Task.FromResult(Context.SaveChanges() > 0);
        }

        public Task<bool> DeleteToken(string token, string deletedBy)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(false);
            }

            if (!Context.Tokens.Any(x => x.Key == token))
            {
                return Task.FromResult(false);
            }

            var item = Context.Tokens.FirstOrDefault(x => x.Key == token);
            if (item != null)
            {
                item.DeletedAt = DateTime.Now;
                item.IsDeleted = true;
                item.DeletedBy = deletedBy;
                Context.Entry(item).State = EntityState.Modified;
                return Task.FromResult(Context.SaveChanges() > 0);
            }

            return Task.FromResult(false);
        }

        public Task<bool> IsTokenValid(string token)
        {
            return Task.FromResult(Context.Tokens.Any(x => x.Key == token && x.IsAppActive));
        }
    }

    public interface IAppService
    {
        Task<string> Create(AppModel model);
        Task<PagedList<App>> GetApps(int pageNumber);
        Task<List<App>> GetByUserId(string userId);
        Task<App> Get(string appId);
        Task<App> GetByName(string name);
        Task<bool> CreateToken(TokenModel token);
        Task<bool> ChangeStatus(string appId, string updatedBy, bool isActive);
        Task<bool> DeleteToken(string token, string deletedBy);
        Task<bool> IsTokenValid(string token);
    }
}
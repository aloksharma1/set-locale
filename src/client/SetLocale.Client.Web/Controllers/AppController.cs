﻿using System;
using System.Threading.Tasks;
using System.Web.Mvc;

using SetLocale.Client.Web.Helpers;
using SetLocale.Client.Web.Models;
using SetLocale.Client.Web.Services;

namespace SetLocale.Client.Web.Controllers
{
    public class AppController : BaseController
    {
        private readonly IAppService _appService;
        public AppController(
            IUserService userService, 
            IFormsAuthenticationService formsAuthenticationService, 
            IAppService appService)
            : base(userService, formsAuthenticationService)
        {
            _appService = appService;
        }

        [HttpGet]
        public async Task<ActionResult> Detail(int id = 0)
        {
            if (id < 1)
            {
                return RedirectToHome();
            }

            var entity = await _appService.Get(id);
            if (entity == null)
            {
                return Redirect("/user/apps");
            }

            ViewBag.IsActive = entity.IsActive;

            var model = AppModel.MapFromEntity(entity);
            return View(model);
        }
        
        [HttpGet]
        public ActionResult New()
        {
            var model = new AppModel();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> New(AppModel model)
        {
            if (!model.IsValidForNew())
            {
                model.Msg = "bir sorun oluştu...";
                return View(model);
            }

            model.CreatedBy = User.Identity.GetUserId();
            model.Email = User.Identity.GetUserEmail();

            var appId = await _appService.Create(model);
            if (appId == null)
            {
                model.Msg = "bir sorun oluştu...";
                return View(model);
            }

            return Redirect("/app/detail/" + appId);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> NewToken(int appId)
        {
            var result = new ResponseModel { Ok = false };

            var token = new TokenModel
            {
                CreationDate = DateTime.Now,
                UsageCount = 0,
                Token = Guid.NewGuid().ToNoDashString(),
                AppId = appId,
                CreatedBy = User.Identity.GetUserId()
            };

            var isOk = await _appService.CreateToken(token);
            if (!isOk) return Json(result, JsonRequestBehavior.DenyGet);

            result.Ok = true;
            result.Result = token;

            return Json(result, JsonRequestBehavior.DenyGet);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> ChangeStatus(int id, bool isActive)
        {
            var model = new ResponseModel { Ok = false };
            if (id < 1)
            {
                return Json(model, JsonRequestBehavior.DenyGet);
            }

            model.Ok = await _appService.ChangeStatus(id, isActive);
            return Json(model, JsonRequestBehavior.DenyGet);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteToken( string token)
        {
            var model = new ResponseModel { Ok = false };
            if (string.IsNullOrEmpty(token))
            {
                return Json(model, JsonRequestBehavior.DenyGet);
            }

            model.Ok = await _appService.DeleteToken(token, User.Identity.GetUserId());
            return Json(model, JsonRequestBehavior.DenyGet);
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SetLocale.Client.Web.Entities;
using SetLocale.Client.Web.Helpers;
using SetLocale.Client.Web.Repositories;

namespace SetLocale.Client.Web.Services
{
    public interface ITagService
    {
        Task<PagedList<Word>> GetWords(string tagUrlName, int pageNumber);
        Task<List<Tag>> GetTags();
    }

    public class TagService : ITagService
    {
        private readonly IRepository<Word> _wordRepository;
        private readonly IRepository<Tag> _tagRepository;

        public TagService(
            IRepository<Word> wordRepository,
            IRepository<Tag> tagRepository)
        {
            _wordRepository = wordRepository;
            _tagRepository = tagRepository;
        }

        public Task<PagedList<Word>> GetWords(string tagUrlName, int pageNumber)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            var urlSlug = tagUrlName.ToUrlSlug();
            var items = _wordRepository.FindAll(x => x.Tags.Any(y => y.UrlName == urlSlug), x => x.Tags);

            long totalCount = items.Count();
            var totalPageCount = (int)Math.Ceiling(totalCount / (double)ConstHelper.PageSize);

            var itemsList = new List<Word>();
            if (pageNumber <= totalPageCount)
            {
                items = items.OrderByDescending(x => x.Id).Skip(ConstHelper.PageSize * (pageNumber - 1)).Take(ConstHelper.PageSize);
                itemsList = items.ToList();
            }

            return Task.FromResult(new PagedList<Word>(pageNumber, ConstHelper.PageSize, totalCount, itemsList));
        }


        public Task<List<Tag>> GetTags()
        {
            var tags = _tagRepository.FindAll().ToList();

            return Task.FromResult(tags);
        }
    }
}
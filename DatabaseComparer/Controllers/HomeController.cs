using DatabaseComparer.Business;
using DatabaseComparer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DatabaseComparer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly CompareManager _compareManager;

        public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache, CompareManager compareManager)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _compareManager = compareManager;
        }

        public IActionResult Index()
        {
            Dictionary<string, string> types = new Dictionary<string, string>();
            types.Add("1", "table");
            types.Add("2", "function");
            ViewBag.Types = types;
            return View();
        }

        [HttpPost]
        public PartialViewResult GetList(Filter filters = null, int page = 1, int pagesize = 50)
        {
            List<CompareResult> results;
            List<string> titles = new List<string>();
            if (!string.IsNullOrEmpty(filters.Name) || filters.Type != null)
            {
                List<CompareResult> filteredResult = new List<CompareResult>();
                if (!_memoryCache.TryGetValue("List", out results))
                {
                    results = _compareManager.GetCompares();
                    foreach (var item in results)
                    {
                        titles.Add(item.TitleLeft);
                        titles.Add(item.TitleRight);
                        break;
                    }
                    filteredResult = _compareManager.GetComparesByFilter(filters, results);
                    if (results.Count() > 0)
                    {
                        _memoryCache.Set("List", results);
                    }
                    ViewBag.TotalCount = filteredResult?.Count;
                    ViewBag.Titles = titles;
                    ViewBag.Page = page;
                    ViewBag.PageSize = pagesize;
                    filteredResult = filteredResult?.Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    return PartialView("GetList", filteredResult);
                }
                else
                {
                    foreach (var item in results)
                    {
                        titles.Add(item.TitleLeft);
                        titles.Add(item.TitleRight);
                        break;
                    }
                    filteredResult = _compareManager.GetComparesByFilter(filters, results);
                    ViewBag.TotalCount = filteredResult?.Count;
                    ViewBag.Titles = titles;
                    ViewBag.Page = page;
                    ViewBag.PageSize = pagesize;
                    filteredResult = filteredResult?.Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    return PartialView("GetList", filteredResult);
                }
            }
            else
            {
                if (!_memoryCache.TryGetValue("List", out results))
                {
                    results = _compareManager.GetCompares();
                    if (results.Count() > 0)
                    {
                        _memoryCache.Set("List", results);
                    }
                    foreach (var item in results)
                    {
                        titles.Add(item.TitleLeft);
                        titles.Add(item.TitleRight);
                        break;
                    }
                    ViewBag.Titles = titles;
                    ViewBag.TotalCount = results?.Count;
                    ViewBag.Page = page;
                    ViewBag.PageSize = pagesize;
                    results = results?.Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    return PartialView("GetList", results);
                }
                else
                {
                    foreach (var item in results)
                    {
                        titles.Add(item.TitleLeft);
                        titles.Add(item.TitleRight);
                        break;
                    }
                    ViewBag.Titles = titles;
                    ViewBag.TotalCount = results?.Count;
                    ViewBag.Page = page;
                    ViewBag.PageSize = pagesize;
                    results = results?.Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    return PartialView("GetList", results);
                }
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

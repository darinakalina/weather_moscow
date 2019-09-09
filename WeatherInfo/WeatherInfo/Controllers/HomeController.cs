using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using WeatherInfo.Models;

namespace WeatherInfo.Controllers
{
    public class HomeController : Controller
    {
        private WeatherDBEntities _db = new WeatherDBEntities();
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Visualize(int? month, int? year, int? page)
        {
            int pageSize = 10;
            int pageNumber = page.HasValue ? Convert.ToInt32(page) : 1;
            IPagedList<Weather> weathersInfo;
   
            ViewBag.month = month;
            ViewBag.year = year;

            if (year == null && month == null)
            {
                weathersInfo = _db.Weathers.ToList().ToPagedList<Weather>(pageNumber, pageSize);
            }
            else if (year == null && month != null)
            {
                // error
                throw new Exception("TODO!!!");
            } 
            else if (year != null && month == null)
            {
                weathersInfo = _db.Weathers.Where(m => m.Date.Year == year)
                    .OrderBy(m => m.Date.Year)
                    .ToPagedList(pageNumber, pageSize);
            }
            else
            {
                weathersInfo = _db.Weathers.Where(m => (m.Date.Year == year && m.Date.Month == month))
                    .OrderBy(m => m.Date.Year)
                    .ToPagedList(pageNumber, pageSize);
            }
            return View(weathersInfo);
        }

        [HttpPost]
        public ActionResult Visualize(int? month, int? year)
        {
            return RedirectToAction("Visualize", new { page=1, month=month, year=year });
        }

        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(Archives archives)
        {
            Parser parser = new Parser();
            var transaction = _db.Database.BeginTransaction();

            try
            {
                foreach (var file in archives.Files)
                {
                    if (file.ContentLength > 0)
                    {
                        _db.Configuration.ValidateOnSaveEnabled = false;
                        _db.Configuration.AutoDetectChangesEnabled = false;
                        _db.Weathers.AddRange(parser.Parse(file.InputStream));
                        _db.Configuration.AutoDetectChangesEnabled = true;
                        _db.SaveChanges();
                    }
                }
            }
            catch (WeatherParseException ex)
            {
                transaction.Rollback();
                return RedirectToAction("Upload");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return RedirectToAction("Upload");
            }
            
            transaction.Commit();
            return RedirectToAction("Upload");
        }
    }
}
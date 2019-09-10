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
        private WeatherDBEntities db = new WeatherDBEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Visualize(int? month, int? year, int? page)
        {
            int pageSize = 10;
            int pageNumber = page.HasValue ? Convert.ToInt32(page) : 1;

            WeatherInfoViewModel model = new WeatherInfoViewModel();
            model.weatherInfo = db.Weathers.Where(m => false).ToPagedList(pageNumber, pageSize);
            model.month = month;
            model.year = year;

            if (year == null && month == null)
            {
                model.weatherInfo = db.Weathers.ToList().ToPagedList<Weather>(pageNumber, pageSize);
            }
            else if (year == null && month != null)
            {
                model.errorMessage = "Specify year";
            } 
            else if (year != null && month == null)
            {
                model.weatherInfo = db.Weathers.Where(m => m.Date.Year == year)
                    .OrderBy(m => m.Date.Year)
                    .ToPagedList(pageNumber, pageSize);
            }
            else
            {
                model.weatherInfo = db.Weathers.Where(m => (m.Date.Year == year && m.Date.Month == month))
                    .OrderBy(m => m.Date.Year)
                    .ToPagedList(pageNumber, pageSize);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Visualize(int? month, int? year)
        {
            return RedirectToAction("Visualize", new { page=1, month=month, year=year });
        }

        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(Archives archives)
        {
            Parser parser = new Parser();

            var transaction = db.Database.BeginTransaction();
            try
            {
                foreach (var file in archives.Files)
                {
                    if (file.ContentLength > 0)
                    {
                        db.Configuration.ValidateOnSaveEnabled = false;
                        db.Configuration.AutoDetectChangesEnabled = false;
                        db.Weathers.AddRange(parser.Parse(file.InputStream));
                        db.Configuration.AutoDetectChangesEnabled = true;
                        db.SaveChanges();
                    }
                }
            }
            catch (WeatherParseException ex)
            {
                transaction.Rollback();
                archives.errorDescription = ex.Message.ToString();
                return View(archives);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                archives.errorDescription = ex.Message.ToString();
                return View(archives);
            }
            
            transaction.Commit();
            return View(archives);
        }
    }
}
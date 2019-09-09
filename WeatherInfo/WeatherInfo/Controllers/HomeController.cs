using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
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

        public ActionResult Visualize()
        {
            return View(_db.Weathers.ToList());
        }

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
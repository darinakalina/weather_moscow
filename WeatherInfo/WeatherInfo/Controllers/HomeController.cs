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
            return View(_db.WeatherInfo.ToList());
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
                        _db.WeatherInfo.AddRange(parser.Parse(file.InputStream));
                    }
                }
            }
            catch (WeatherParseException ex)
            {
                transaction.Rollback();
                Console.WriteLine(ex.ToString());
                return RedirectToAction("Upload");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine(ex.ToString());
                return RedirectToAction("Upload");
            }

            transaction.Commit();
            return RedirectToAction("Upload");
        }
    }
}
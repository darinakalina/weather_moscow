using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using WeatherInfo.Models;
using System.Threading.Tasks;
using System.Activities;
using System.EnterpriseServices;
using System.Transactions;

namespace WeatherInfo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Visualize(int? month, int? year, int? page)
        {
            int pageSize = 10;
            int pageNumber = page.HasValue ? Convert.ToInt32(page) : 1;

            var txOptions = new System.Transactions.TransactionOptions();
            txOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;

            using (var transaction = new TransactionScope(TransactionScopeOption.Required, txOptions))
            {
                using (var context = new WeatherDBEntities())
                {
                    WeatherInfoViewModel model = new WeatherInfoViewModel();
                    model.weatherInfo = context.Weathers.Where(m => false).ToPagedList(pageNumber, pageSize);
                    model.month = month;
                    model.year = year;

                    if (year == null && month == null)
                    {
                        model.weatherInfo = context.Weathers.ToList().ToPagedList<Weather>(pageNumber, pageSize);
                    }
                    else if (year == null && month != null)
                    {
                        model.errorMessage = "Specify year";
                    }
                    else if (year != null && month == null)
                    {
                        model.weatherInfo = context.Weathers.Where(m => m.Date.Year == year)
                            .OrderBy(m => m.Date.Year)
                            .ToPagedList(pageNumber, pageSize);
                    }
                    else
                    {
                        model.weatherInfo = context.Weathers.Where(m => (m.Date.Year == year && m.Date.Month == month))
                            .OrderBy(m => m.Date.Year)
                            .ToPagedList(pageNumber, pageSize);
                    }

                    return View(model);
                }
            }
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

            var weatherInfoList = new List<Weather>();

            try
            {
                foreach (var file in archives.Files)
                {
                    if (file.ContentLength > 0)
                    {
                        weatherInfoList.AddRange(parser.Parse(file.InputStream));
                    }
                }
            }
            catch (Exception ex)
            {
                archives.errorDescription = ex.Message.ToString();
                return View(archives);
            }

            Task.Run(() =>
            {
                using (var context = new WeatherDBEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        context.Weathers.AddRange(weatherInfoList);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
            });

            return View(archives);
        }
    }
}
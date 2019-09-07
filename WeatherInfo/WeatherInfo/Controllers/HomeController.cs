﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    }
}
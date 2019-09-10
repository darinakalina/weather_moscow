using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList.Mvc;
using PagedList;

namespace WeatherInfo.Models
{
    public class WeatherInfoViewModel
    {
        public IPagedList<Weather> weatherInfo { get; set; }
        public Nullable<int> year { get; set; }
        public Nullable<int> month { get; set; }
        public string errorMessage { get; set; }
    }
}
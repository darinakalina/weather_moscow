using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherInfo.Models
{
    public class Archives
    {
        public IEnumerable<HttpPostedFileBase> Files { get; set; }
    }
}
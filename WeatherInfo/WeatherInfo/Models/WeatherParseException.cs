using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherInfo.Models
{
    public class WeatherParseException : Exception
    {
        public WeatherParseException()
        : base() { }

        public WeatherParseException(string message)
        : base(message) { }

        public WeatherParseException(string format, params object[] args)
        : base(string.Format(format, args)) { }

        public WeatherParseException(string message, Exception innerException)
        : base(message, innerException) { }

        public WeatherParseException(string format, Exception innerException, params object[] args)
        : base(string.Format(format, args), innerException) { }
    }
}
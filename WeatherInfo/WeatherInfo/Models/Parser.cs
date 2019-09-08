using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using NPOI.HSSF.Model;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;

namespace WeatherInfo.Models
{
    public class Parser
    {
        public List<Weather> Parse(Stream stream)
        {
            XSSFWorkbook xssfwb = new XSSFWorkbook(stream);
            List<Weather> weatherInfoList = new List<Weather>();
           
            for (int sheet = 0; sheet < xssfwb.NumberOfSheets; ++sheet)
            {
                ISheet currentSheet = xssfwb.GetSheetAt(sheet);

                if (currentSheet.LastRowNum < START_ROW_IDX)
                {
                    continue;   
                }

                for (int row = START_ROW_IDX; row <= currentSheet.LastRowNum; ++row)
                {
                    var currentRow = currentSheet.GetRow(row);

                    if (currentRow != null)
                    {
                        if (currentRow.LastCellNum >= COLS_NUM - 1)
                        {
                            weatherInfoList.Add(ToWeather(currentRow));
                        }
                        else
                        {
                            throw new WeatherParseException($"Invalid number of cols: {currentRow.LastCellNum}");
                        }
                    }
                }
            }
            return weatherInfoList;
        }

        private Weather ToWeather(IRow row)
        {
            Weather weather = new Weather();
            weather.Date = ColDateValue(row, Column.Date);
            weather.Time = ColTimeValue(row, Column.Time);
            weather.Temperature = ColDoubleValue(row, Column.Temperature);
            weather.RelativeHumidity = ColDoubleValue(row, Column.RelativeHumidity);
            weather.DewPoint = ColDoubleValue(row, Column.DewPoint);
            weather.Pressure = ColIntValue(row, Column.Pressure);
            weather.WindDirection = ColStringValue(row, Column.WindDirection);
            weather.WindVelocity = ColNullableIntValue(row, Column.WindVelocity);
            weather.Claudage = ColNullableIntValue(row, Column.Claudage);
            weather.ClaudageLowBound = ColIntValue(row, Column.ClaudageLowBound);
            weather.HorizontallVisibility = ColNullableIntValue(row, Column.HorizontallVisibility);
            weather.WeatherConditions = ColStringValue(row, Column.WeatherConditions);
            return weather;
        }

        private DateTime ColDateValue(IRow row, Column col)
        {
            DateTime res;

            string strVal = ColStringValue(row, col);

            if (DateTime.TryParse(strVal, out res))
            {
                return res;
            }
            else
            {
                throw CreateInvalidFormatException("date", strVal, col);
            }
        }

        private TimeSpan ColTimeValue(IRow row, Column col)
        {
            TimeSpan res;
            string strVal = ColStringValue(row, col);

            if (TimeSpan.TryParse(strVal, out res))
            {
                return res;
            }
            else
            {
                throw CreateInvalidFormatException("time", strVal, col);
            }
        }

        private double ColDoubleValue(IRow row, Column col)
        {
            double res = 0;
            string strVal = ColStringValue(row, col);

            if (Double.TryParse(strVal, out res))
            {
                return res;
            }
            else
            {
                throw CreateInvalidFormatException("double", strVal, col);
            }
        }

        private int ColIntValue(IRow row, Column col)
        {
            Nullable<int> res = ColNullableIntValue(row, col);
           
            if (res == null)
            {
                throw new WeatherParseException($"Missing value in {col}");
            }
            else
            {
                return res.Value;
            }
        }

        private Nullable<int> ColNullableIntValue(IRow row, Column col)
        {
            int res = 0;

            if (Int32.TryParse(ColStringValue(row, col), out res))
            {
                return res;
            }
            else
            {
                return null;
            }
        }

        private string ColStringValue(IRow row, Column col)
        {
            return row.GetCell((int)col).StringCellValue;
        }

        private WeatherParseException CreateInvalidFormatException(string type, string value, Column col)
        {
            return new WeatherParseException($"Wrong {type} value ({value}) for column {col}");
        }

        private const int START_ROW_IDX = 4;
        private const int COLS_NUM = 12;

        private enum Column
        {
            Date = 0,
            Time,
            Temperature,
            RelativeHumidity,
            DewPoint,
            Pressure,
            WindDirection, 
            WindVelocity,
            Claudage,
            ClaudageLowBound, 
            HorizontallVisibility, 
            WeatherConditions 
        }
    }
}
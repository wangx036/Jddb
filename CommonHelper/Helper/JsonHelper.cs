using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CommonHelper
{
    public static class JsonHelper
    {
        public static object ToJson(this string json)
        {
            return json == null ? null : JsonConvert.DeserializeObject(json);
        }
        public static string ToJson(this object obj)
        {
            return obj.ToJson("yyyy-MM-dd HH:mm:ss");
        }
        public static string ToJson(this object obj, string datetimeformats)
        {
            var setting = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatString = datetimeformats
            };
            return JsonConvert.SerializeObject(obj, setting);
        }
        public static T ToObject<T>(this string json)
        {
            return json == null ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }
        public static List<T> ToList<T>(this string json)
        {
            return json == null ? null : JsonConvert.DeserializeObject<List<T>>(json);
        }
        public static DataTable ToTable(this string json)
        {
            return json == null ? null : JsonConvert.DeserializeObject<DataTable>(json);
        }
        public static JObject ToJObject(this string json)
        {
            return json == null ? JObject.Parse("{}") : JObject.Parse(json.Replace("&nbsp;", ""));
        }
    }


    public class DateFormat : IsoDateTimeConverter
    {
        public DateFormat()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
    }
    public class TimeFormat : IsoDateTimeConverter
    {
        public TimeFormat()
        {
            base.DateTimeFormat = "HH:mm";
        }
    }
}

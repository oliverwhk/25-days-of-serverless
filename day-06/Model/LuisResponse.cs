using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace day_06.Model
{
    public class LuisResponse
    {
        public DateTime DetectedDateTime { get; set; }
        public string DateTimeToken { get; set; }


        public static LuisResponse Parse(string jsonResponse)
        {
            var jObject = JObject.Parse(jsonResponse);
            var entities = jObject.SelectToken("prediction.entities");

            // Detect date time
            var dateTimeEntity = entities.SelectToken("datetimeV2");
            if (DateTime.TryParse(dateTimeEntity?[0]["values"]?[0]["timex"].ToString(), out var detectedDateTime) == false)
            {
                detectedDateTime = DateTime.UtcNow;
            }

            var instanceEntity = entities.SelectToken("$instance");
            var dateTimeToken = instanceEntity?["datetimeV2"][0]["text"].ToString();

            return new LuisResponse
            {
                DetectedDateTime = detectedDateTime,
                DateTimeToken = dateTimeToken
            };
        }
    }
}

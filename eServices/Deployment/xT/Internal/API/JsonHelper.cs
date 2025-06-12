using System.Text;
using Newtonsoft.Json;

namespace XT.Internal.API
{
    public static class JsonHelper
	{
        //public static bool ValidateJson(string jsonString, out object obj, out string report)
        //{
        //    var builder = new StringBuilder();
        //    obj = JsonConvert.DeserializeObject(jsonString,
        //        new JsonSerializerSettings()
        //        {
        //            Error = (sender, e) =>
        //            {
        //                builder.AppendLine(e.ErrorContext.Error.ToString());
        //            }
        //        });
        //    report = builder.ToString();
        //    return obj != null && string.IsNullOrWhiteSpace(report);
        //}
    }
}

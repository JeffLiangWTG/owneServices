using System;
using System.IO;
using System.Web;
using Enterprise.Registry.Business;

namespace Enterprise.Tracking.Web
{
	public class ClientSpecificCssHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "text/css";
			var output = GetCssOutput(context);
			if (output != null)
			{
				context.Response.Write(output);
			}
		}

		string GetCssOutput(HttpContext context)
		{
			var output = String.Empty;

			var path = context.Request.FilePath;
			if (path.EndsWith("BaseStyle.css", StringComparison.OrdinalIgnoreCase) && !path.ToUpperInvariant().Contains("App_Themes".ToUpperInvariant()))
			{
				var pathForRegistry = context.Request.Url.Host;
				var theme = WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, pathForRegistry);
				if (theme == null || theme == ThemeCodeDescriptionPairList.Codes.CUS)
				{
					output = WebDataRegistry.Instance.GetWebCustomCss(WebDataRegistry.Instance.WebTrackerCustomCss, pathForRegistry);
				}
			}
			if (String.IsNullOrEmpty(output) && File.Exists(MapPath(path)))
			{
				output = File.ReadAllText(MapPath(path));
			}

			return output;
		}

		public virtual string MapPath(string path)
		{
			return HttpContext.Current.Server.MapPath(path);
		}

		public bool IsReusable
		{
			get { return true; }
		}
	}
}

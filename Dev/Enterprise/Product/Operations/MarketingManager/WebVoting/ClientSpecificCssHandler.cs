using System;
using System.IO;
using System.Web;
using CargoWise.Data;

namespace Enterprise.MarketingManager.WebVoting
{
	public class ClientSpecificCssHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var output = string.Empty;
				var path = context.Request.FilePath;

				if (path.EndsWith("BaseStyle.css", StringComparison.OrdinalIgnoreCase) && !path.ToUpperInvariant().Contains("App_Themes".ToUpperInvariant()))
				{
					output = ClientSpecificRequestHandlerHelper.FindTheme(context)?.CSS;
				}

				if (string.IsNullOrEmpty(output))
				{
					var serverPath = HttpContext.Current.Server.MapPath(path);
					if (File.Exists(serverPath))
					{
						output = File.ReadAllText(serverPath);
					}
				}

				if (output != null)
				{
					context.Response.ContentType = "text/css";
					context.Response.Write(output);
				}
			}
		}

		public bool IsReusable
		{
			get { return true; }
		}
	}
}

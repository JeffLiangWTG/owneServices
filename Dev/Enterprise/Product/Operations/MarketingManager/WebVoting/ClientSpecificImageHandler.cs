using System.IO;
using System.Web;
using CargoWise.Data;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.MarketingManager.WebVoting
{
	public class ClientSpecificImageHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			using (Db.DisposableActionForDbConnection())
			{
				byte[] output = null;
				var path = context.Request.FilePath;

				var theme = ClientSpecificRequestHandlerHelper.FindTheme(context);
				if (theme != null)
				{
					output = theme.FindImage(Path.GetFileName(path))?.Data;
				}

				if (output == null)
				{
					var serverPath = HttpContext.Current.Server.MapPath(path);
					if (File.Exists(serverPath))
					{
						output = File.ReadAllBytes(serverPath);
					}
				}

				if (output != null)
				{
					context.Response.ContentType = context.GetImageMIMEType();
					context.Response.OutputStream.Write(output, 0, output.Length);
				}
			}
		}

		public bool IsReusable
		{
			get { return true; }
		}
	}
}

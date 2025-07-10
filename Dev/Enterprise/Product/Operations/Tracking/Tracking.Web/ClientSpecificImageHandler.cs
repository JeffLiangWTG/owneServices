using System;
using System.IO;
using System.Web;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.Tracking.Web
{
	public class ClientSpecificImageHandler : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			var output = GetImageOutput(context);
			if (output != null)
			{
				context.Response.ContentType = context.GetImageMIMEType();
				context.Response.OutputStream.Write(output, 0, output.Length);
			}
		}

		protected byte[] GetImageOutput(HttpContext context)
		{
			var path = context.Request.FilePath;
			var pathForRegistry = context.Request.Url.Host;

			byte[] output = null;
			var imageName = GetImageName(path);
			if (!string.IsNullOrEmpty(imageName))
			{
				var theme = WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, pathForRegistry);
				if (theme == null || theme == ThemeCodeDescriptionPairList.Codes.CUS)
				{
					output = WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebTrackerCustomImages, pathForRegistry, imageName);
				}

				var app = (ZGlobal)context.ApplicationInstance;
				var staticImagePath = app.MapPath(path);
				if (output == null && File.Exists(staticImagePath))
				{
					output = File.ReadAllBytes(staticImagePath);
				}
			}

			return output;
		}

		string GetImageName(string path)
		{
			try
			{
				return Path.GetFileName(path);
			}
			catch (ArgumentException)
			{
			}

			return string.Empty;
		}

		public bool IsReusable
		{
			get { return true; }
		}
	}
}

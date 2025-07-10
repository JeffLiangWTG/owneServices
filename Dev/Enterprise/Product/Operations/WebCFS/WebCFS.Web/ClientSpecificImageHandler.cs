using System.IO;
using System.Web;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.WebCFS.Web
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

		byte[] GetImageOutput(HttpContext context)
		{
			var path = context.Request.FilePath;
			var pathForRegistry = context.Request.Url.Host;

			byte[] output = null;
			var theme = WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebCFSTheme, pathForRegistry);
			if (theme == null || theme == ThemeCodeDescriptionPairList.Codes.CUS)
			{
				output = WebDataRegistry.Instance.GetWebCustomImage(WebDataRegistry.Instance.WebCFSCustomImages, pathForRegistry, Path.GetFileName(path));
			}
			if (output == null && File.Exists(MapPath(path)))
			{
				output = File.ReadAllBytes(MapPath(path));
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

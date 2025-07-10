using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.WebVoting
{
	public class WebCSSImages : IWebCSSImages
	{
		const string EmbeddedResourcePath = "Enterprise.MarketingManager.WebVoting.";

#if DEBUG
		public
#endif
		List<string> EmbeddedImagesForRegistryConfiguration()
		{
			List<string> list = new List<string>();

			list.Add("Banner.gif");
			list.Add("helpicon.png");
			list.Add("legend.jpg");
			list.Add("logo.png");
			list.Add("topbg1.gif");
			list.Add("topbg3.jpg");

			return list;
		}

		public Dictionary<string, byte[]> WebImages
		{
			get
			{
				Dictionary<string, byte[]> imageList = new Dictionary<string, byte[]>();
				Assembly executingAssembly = Assembly.GetExecutingAssembly();

				foreach (string image in EmbeddedImagesForRegistryConfiguration())
				{
					int bufferSize = 4096;
					byte[] buffer = new byte[bufferSize];

					using (Stream stream = executingAssembly.GetManifestResourceStream(EmbeddedResourcePath + "Images." + image))
					using (MemoryStream ms = new MemoryStream())
					{
						int read;
						while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
						{
							ms.Write(buffer, 0, read);
						}

						imageList.Add(image, ms.ToArray());
					}
				}

				return imageList;
			}
		}

		public ZString WebStyleSheet
		{
			get
			{
				string output = "";
				Assembly executingAssembly = Assembly.GetExecutingAssembly();

				Stream stream = null;
				try
				{
					stream = executingAssembly.GetManifestResourceStream(EmbeddedResourcePath + "BaseStyle.css");
					using (StreamReader reader = new StreamReader(stream))
					{
						stream = null;
						output = reader.ReadToEnd();
					}
				}
				finally
				{
					if (stream != null)
					{
						stream.Dispose();
					}
				}
				return output;
			}
		}
	}
}

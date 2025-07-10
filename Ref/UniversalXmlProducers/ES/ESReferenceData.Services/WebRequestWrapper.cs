using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class WebRequestWrapper : IWebRequestWrapper
	{
		public WebRequestWrapper(Encoding encoding)
		{
			this.encoding = encoding;
		}
		readonly Encoding encoding;

		static HttpWebRequest GetRequestWithCertificate(string url)
		{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			var request = (HttpWebRequest)WebRequest.Create(new  System.Uri(url));
#pragma warning restore SYSLIB0014 // Type or member is obsolete
			request.ClientCertificates.Add(CertificateConfig.GetCertificate());
			return request;
		}

		string GetStringHTMLResponse(HttpWebRequest request, bool sanitize = true)
		{
			var content = string.Empty;
			var response = (HttpWebResponse)request.GetResponse();

			using (var stream = response.GetResponseStream())
			using (var sr = new StreamReader(stream, encoding))
			{
				content = sr.ReadToEnd();
			}
			var result = sanitize ? SanitizeHtml(content) : content;
			return result;
		}

		protected static string SanitizeHtml(string html) => Regex.Replace(html, @"\p{C}+", string.Empty);

		public string GetContent(string url)
		{
			var request = GetRequestWithCertificate(url);
			return GetStringHTMLResponse(request);
		}

		public string GetContentFromPost(string url, string postData, bool sanitize = true)
		{
			var data = encoding.GetBytes(postData);
			var request = GetRequestWithCertificate(url);
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = data.Length;
			request.Method = "POST";
			using (var stream = request.GetRequestStream())
			{
				stream.Write(data, 0, data.Length);
			}

			return GetStringHTMLResponse(request, sanitize);
		}
	}
}

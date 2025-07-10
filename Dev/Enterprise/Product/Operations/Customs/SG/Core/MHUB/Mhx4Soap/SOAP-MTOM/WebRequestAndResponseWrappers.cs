using System;
using System.IO;
using System.Net;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap
{
	public class HttpWebRequestWrapper
	{
		readonly HttpWebRequest request;

		public HttpWebRequestWrapper(HttpWebRequest httpWebRequest)
		{
			this.request = httpWebRequest;
		}

		public string Accept
		{
			get { return request.Accept; }
			set { request.Accept = value; }
		}

		public string ContentType
		{
			get { return request.ContentType; }
			set { request.ContentType = value; }
		}
		public CookieContainer CookieContainer
		{
			get { return request.CookieContainer; }
			set { request.CookieContainer = value; }
		}

		public WebHeaderCollection Headers
		{
			get { return request.Headers; }
			set { request.Headers = value; }
		}
		public bool KeepAlive
		{
			get { return request.KeepAlive; }
			set { request.KeepAlive = value; }
		}
		public string Method
		{
			get { return request.Method; }
			set { request.Method = value; }
		}
		public IWebProxy Proxy
		{
			get { return request.Proxy; }
			set { request.Proxy = value; }
		}
		public string UserAgent
		{
			get { return request.UserAgent; }
			set { request.UserAgent = value; }
		}

		public virtual Stream GetRequestStream()
		{
			return request.GetRequestStream();
		}

		public virtual WebResponseWrapper GetResponse()
		{
			return new WebResponseWrapper(request.GetResponse());
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2229:ImplementSerializationConstructors")]  // STFU!
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly")]  // STFU!
	[Serializable]  // Only to make the code analyser STFU (CA2237)
	public class WebResponseWrapper : WebResponse
	{
		readonly WebResponse webResponse;

		public WebResponseWrapper(WebResponse webResponse)
		{
			this.webResponse = webResponse;
		}

		public override Stream GetResponseStream()
		{
			return webResponse.GetResponseStream();
		}
	}
}

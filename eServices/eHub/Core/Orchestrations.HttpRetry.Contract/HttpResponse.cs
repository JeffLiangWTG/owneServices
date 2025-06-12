using System;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract
{
	[Serializable]
	public class HttpResponse
	{
		[XmlIgnore]
		public bool IsSuccessful
		{
			get
			{
				return 
					string.IsNullOrEmpty(ExceptionType) && 
					string.IsNullOrEmpty(ExceptionMessage);
			}
		}

		public ushort StatusCode { get; set; }

		public string ExceptionType { get; set; }

		public string ExceptionMessage { get; set; }

		public HttpHeader[] Headers { get; set; }

		public XLANGMessage ContentMessage { get; set; }

		public string FindHeaderValue(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}

			if (Headers == null)
			{
				throw new InvalidOperationException(string.Format(
					"Key [{0}] does NOT exist due to <null> headers!",
					key));
			}

			var matchedHeaders = Headers
				.Where(header => header.Key == key)
				.ToArray();

			if (matchedHeaders.Length < 1)
			{
				throw new InvalidOperationException(string.Format(
					"Key [{0}] does NOT exist in headers!",
					key));
			}

			if (matchedHeaders.Length > 1)
			{
				throw new InvalidOperationException(string.Format(
					"Multiple headers with same key [{0}] are found!",
					key));
			}

			return matchedHeaders
				.Single()
				.Value;
		}
	}
}

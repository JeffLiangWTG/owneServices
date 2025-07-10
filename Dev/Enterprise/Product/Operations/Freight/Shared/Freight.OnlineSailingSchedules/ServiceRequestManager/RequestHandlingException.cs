using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using CargoWise.Types;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	[Serializable]
	sealed class RequestHandlingException : Exception
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception handling")]
		public RequestHandlingException(string searchParameters, HttpResponseMessage response, Exception exception) : base(exception?.Message, exception)
		{
			Data.Add("Search Parameters", searchParameters);
			if (response != null)
			{
				Data.Add("Response Header", response.Headers?.ToString());
				Data.Add("Response Request", response.RequestMessage?.ToString());
				Data.Add("Response Status", response.StatusCode);
			}

			if (exception is HttpRequestException httpRequestException)
			{
				Data.Add("Client OS Version", System.Environment.OSVersion.VersionString);
				foreach (DictionaryEntry exceptionData in httpRequestException.Data)
				{
					Data.Add(exceptionData.Key, exceptionData.Value);
				}

				if (httpRequestException.InnerException is WebException webException && webException.Response != null)
				{
					if (webException.Response?.Headers != null)
					{
						var header = new ZString(webException.Response.Headers.ToString()).SubstringSafe(0, 1000);
						Data.Add("Web Response Header", header);
					}

					var resp = new StreamReader(webException.Response.GetResponseStream()).ReadToEnd();
					var messageFromServer = new ZString(resp);
					if (!messageFromServer.IsEmpty)
					{
						Data.Add("Response Content", messageFromServer.SubstringSafe(0, 2000));
					}
				}
			}
		}

#if NETFRAMEWORK
#pragma warning disable CS0628 // New protected member declared in sealed type
		protected RequestHandlingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
#pragma warning restore CS0628 // New protected member declared in sealed type
			: base(info, context)
		{
		}
#endif
	}
}

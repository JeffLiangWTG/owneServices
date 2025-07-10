using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption.Util;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap
{
	public class SubmitRequestPost
	{
		protected virtual string CreateFormDataBoundary()
		{
			return Guid.NewGuid().ToString();
		}

		public string ExecutePostRequestSubmitPayload(Uri url, Dictionary<string, string> postData, byte[] encryptedPayloadBytes, string fileFormKey, CookieContainer cookieJar, IWebProxy proxy)
		{
			var boundary = CreateFormDataBoundary();
			HttpWebRequestWrapper request = GetNewRequest(url);
			request.Proxy = proxy;
			request.Method = "POST";
			request.KeepAlive = true;
			request.ContentType = String.Format(System.Globalization.CultureInfo.InvariantCulture, "multipart/related;start=\"<rootpart*{0}@example.jaxws.sun.com>\";type=\"application/xop+xml\";boundary=\"uuid:{0}\";start-info=\"text/xml\"", boundary);
			request.Accept = "text/xml, multipart/related";
			request.Headers.Add("SOAPAction", "\"\"");
			request.UserAgent = "JAX-WS RI 2.2.8 svn-revision#13980";
			request.CookieContainer = cookieJar;
			Stream requestStream = request.GetRequestStream();
			postData.WriteMultipartFormData(requestStream, boundary);
			if (encryptedPayloadBytes.Length > 0)
			{
				encryptedPayloadBytes.WriteMultipartFormData(requestStream, boundary, fileFormKey);
			}
			byte[] endBytes = System.Text.Encoding.ASCII.GetBytes("--uuid:" + boundary + "--");
			requestStream.Write(endBytes, 0, endBytes.Length);
			requestStream.Close();
			using (WebResponse webResponse = request.GetResponse())
			{
				using (var responseStream = webResponse.GetResponseStream())
				{
					using (StreamReader reader = new StreamReader(responseStream))
					{
						return reader.ReadToEnd();
					}
				}
			}
		}

		public byte[] ExecutePostRequestRetrievePayload(Uri url, Dictionary<string, string> postData, byte[] encryptedPayloadBytes, string fileFormKey, CookieContainer cookieJar, IWebProxy proxy)
		{
			var boundary = CreateFormDataBoundary();
			HttpWebRequestWrapper request = GetNewRequest(url);
			request.Proxy = proxy;
			request.Method = "POST";
			request.KeepAlive = true;
			request.ContentType = String.Format(System.Globalization.CultureInfo.InvariantCulture, "multipart/related;start=\"<rootpart*{0}@example.jaxws.sun.com>\";type=\"application/xop+xml\";boundary=\"uuid:{0}\";start-info=\"text/xml\"", boundary);
			request.Accept = "text/xml, multipart/related";
			request.Headers.Add("SOAPAction", "\"\"");
			request.UserAgent = "JAX-WS RI 2.2.8 svn-revision#13980";
			request.CookieContainer = cookieJar;
			Stream requestStream = request.GetRequestStream();
			postData.WriteMultipartFormData(requestStream, boundary);
			if (encryptedPayloadBytes.Length > 0)
			{
				encryptedPayloadBytes.WriteMultipartFormData(requestStream, boundary, fileFormKey);
			}
			byte[] endBytes = System.Text.Encoding.ASCII.GetBytes("--uuid:" + boundary + "--");
			requestStream.Write(endBytes, 0, endBytes.Length);
			requestStream.Close();
			byte[] payloadArray;

			using (WebResponse webResponse = request.GetResponse())
			{
				using (var responseStream = webResponse.GetResponseStream())
				{
					using (var memStream = new MemoryStream())
					{
						var totalBytesReceived = 0;
						var buffer = new byte[1024];
						int bytesRead = responseStream.Read(buffer, 0, buffer.Length);
						while (bytesRead > 0)
						{
							memStream.Write(buffer, 0, bytesRead);
							totalBytesReceived += bytesRead;
							bytesRead = responseStream.Read(buffer, 0, buffer.Length);
						}

						payloadArray = memStream.ToArray();
					}
				}
			}

			var payloadStringHeader = Encoding.ASCII.GetString(payloadArray);
			if (!payloadStringHeader.Contains("<responseCode>0</responseCode>"))
			{
				throw new NotSupportedException("Unable to download file: " + payloadStringHeader);
			}

			byte[] firstBoundaryDelimiter = Encoding.ASCII.GetBytes("\r\n"); // looking for --uuid:......\r\n boundary

			var boundaryDelimiterPositions = payloadArray.Locate(firstBoundaryDelimiter);
			byte[] boundaryArray = new byte[boundaryDelimiterPositions[0]];
			Array.Copy(payloadArray, boundaryArray, boundaryArray.Length); // found the actual value of boundary, example: --uuid:[guid]

			var boundariesLocation = payloadArray.Locate(boundaryArray); // let's check for boundaries in the reply
			var boundariesFound = boundariesLocation.Length; // how many boundaries have we found

			var lastBoundaryLocation = boundariesLocation[boundariesFound - 1]; // last boundary (after the payload)
			var previousBoundaryLocationExcludingBoundary = boundariesLocation[boundariesFound - 2] + boundaryArray.Length; // previous boundary, skipping the boundary itself
			var sizeOfXOPPayloadWithHeader = lastBoundaryLocation - previousBoundaryLocationExcludingBoundary; // data between last 2 boundaries (payload with some headers)

			byte[] actualXOPPayloadWithHeader = new byte[sizeOfXOPPayloadWithHeader];
			Array.Copy(payloadArray, previousBoundaryLocationExcludingBoundary, actualXOPPayloadWithHeader, 0, actualXOPPayloadWithHeader.Length); // getting actual payload with some headers

			var encryptedPayloadStart = actualXOPPayloadWithHeader.Locate(Encoding.ASCII.GetBytes("\r\n\r\n"))[0] + 4 + 16; // trimming headers by looking for \r\n\r\n (4) + 16 byte offset
			var encryptedPayloadFinal = new byte[actualXOPPayloadWithHeader.Length - encryptedPayloadStart - 2]; // looking for the end of the payload, skipping the last \r\n (2)

			Array.Copy(actualXOPPayloadWithHeader, encryptedPayloadStart, encryptedPayloadFinal, 0, encryptedPayloadFinal.Length); // got the final payload

			return encryptedPayloadFinal;
		}

		// We wrap HttpWebRequest so that we can subclass it for testing.
		protected virtual HttpWebRequestWrapper GetNewRequest(Uri url)
		{
#pragma warning disable SYSLIB0014
			return new HttpWebRequestWrapper((HttpWebRequest)WebRequest.Create(url));
#pragma warning restore SYSLIB0014
		}
	}
}

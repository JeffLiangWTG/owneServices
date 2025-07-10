using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	public class CVXtractorServiceClient
	{
		#region SuppressResourceStringsCheckRegion

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public CVXtractorServiceClient(string serviceUrl, string account, bool useJsonOutput = false)
		{
			ServiceUrl = serviceUrl;
			Account = account;
			UseJsonOutput = useJsonOutput;
		}

		~CVXtractorServiceClient()
		{
			awaitObjects.Clear();
			awaitObjects = null;
		}

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string ServiceUrl { get; private set; }
		public string Account { get; private set; }
		public bool UseJsonOutput { get; set; }

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int TimeoutMilliseconds { get; set; }
		public string ErrorMessage { get; protected set; }
		Dictionary<string, Task> awaitObjects = new Dictionary<string, Task>(1);
		public event EventHandler<ProcessingFinishedEventArgs> OnProcessingFinished;

		public bool IsReadyToUse => !string.IsNullOrWhiteSpace(Account) && !string.IsNullOrWhiteSpace(ServiceUrl);

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public string ProcessBatch(byte[] contents)
		{
			if (Globals.IsTest)
			{
				return string.Empty;
			}

			var result = string.Empty;
			ErrorMessage = "";

			try
			{
				var request = CreateWebRequest(string.Format(CultureInfo.InvariantCulture, "{0}.zip", ZDateTime.UtcNow.Ticks), contents, true);

				if (OnProcessingFinished != null)
				{
					string asyncID = Guid.NewGuid().ToString();
					Task task = ReadResponseAsync(request, asyncID);
					lock (awaitObjects)
					{
						awaitObjects.Add(asyncID, task);
					}
					result = asyncID;
				}
				else
				{
					result = ReadResponse(request);
				}
			}
			catch (Exception ex)
			{
				ErrorMessage = "Exception :: " + ex.Message;
			}

			return result.TrimEnd('\r', '\n');
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public virtual byte[] GetData(string token)
		{
			if (Globals.IsTest)
			{
				return null;
			}

			var fullServiceUrl = ServiceUrl.TrimEnd('/') + "/cvx/rest/api/v1/data?token=" + token;
			var uri = new Uri(fullServiceUrl);
#pragma warning disable SYSLIB0014 // WebRequest.Create(Uri) is obsolete: WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.
			WebRequest request = WebRequest.Create(uri);
#pragma warning restore SYSLIB0014
			request.Proxy = WebRequest.DefaultWebProxy;
			request.Credentials = CredentialCache.DefaultCredentials;
			request.Proxy.Credentials = CredentialCache.DefaultCredentials;
			request.Method = "GET";
			try
			{
				using (WebResponse response = request.GetResponse())
				{
					using (Stream stream = response.GetResponseStream())
					{
						List<byte> bytes = new List<byte>();
						int read = 0;
						while (read >= 0)
						{
							read = stream.ReadByte();

							if (read >= 0)
							{
								bytes.Add((byte)read);
							}
						}

						return bytes.ToArray();
					}
				}
			}
			catch (Exception ex)
			{
				ErrorMessage = "Exception :: " + ex.Message;
			}

			return null;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public string ProcessCV(string fileName, byte[] fileContent)
		{
			if (Globals.IsTest)
			{
				return string.Empty;
			}

			var result = string.Empty;
			ErrorMessage = "";

			try
			{
				var request = CreateWebRequest(fileName, fileContent, false);

				if (OnProcessingFinished != null)
				{
					string asyncID = Guid.NewGuid().ToString();
					Task task = ReadResponseAsync(request, asyncID);
					lock (awaitObjects)
					{
						awaitObjects.Add(asyncID, task);
					}
					return asyncID;
				}
				else
				{
					return ReadResponse(request);
				}
			}
			catch (Exception ex)
			{
				ErrorMessage = "Exception :: " + ex.Message;
			}

			return result;
		}

		public static ZString SanitizeFilename(string fileName)
		{
			string name = Path.GetFileNameWithoutExtension(fileName);
			string ext = Path.GetExtension(fileName);
			var sb = new StringBuilder();
			foreach (var c in name)
			{
				if (char.IsLetter(c) || char.IsNumber(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c))
				{
					sb.Append(c);
				}
				else
				{
					sb.Append("_");
				}
			}

			if (!string.IsNullOrEmpty(ext))
			{
				sb.Append(ext);
			}

			return sb.ToString();
		}

		HttpWebRequest CreateWebRequest(string fileName, byte[] fileContent, bool isBatch)
		{
			HttpWebRequest request = null;
			fileName = SanitizeFilename(fileName);

			const string boundary = "************123456789**********";
			const string twoHyphens = "--";
			const string crlf = "\r\n";

			if (string.IsNullOrWhiteSpace(fileName))
			{
				fileName = "resume_file";
			}

			/*
			  The POST request may be compressed using the gzip algorithm, in which case the HTTP request 
			  header Content­Encoding must be present and have "gzip" as value. A POST request compressed 
			  with gzip must be compressed by the client in its entirety (i.e. the whole message must be 
			  compressed, not the single parts of the multipart/form­data content). It is a client responsibility to 
			  check whether the server supports content compression, this is done by checking the value of the 
			  HTTP response header X­AlpineBits­Server­Accept­Encoding which is set to "gzip" by servers who 
			  support this feature. The so called "Housekeeping" actions must not be compressed.
			*/

			var fullServiceUrl = ServiceUrl.TrimEnd('/') + "/cvx/rest/api/v1/profile/" + (isBatch ? "batch" : "full") + "/" + ((UseJsonOutput) ? "json" : "xml");

			var uri = new Uri(fullServiceUrl);
#pragma warning disable SYSLIB0014 // WebRequest.Create(Uri) is obsolete: WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.
			request = (HttpWebRequest)WebRequest.Create(uri);
#pragma warning restore SYSLIB0014
			request.KeepAlive = true;
			request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
			request.Headers.Add("charset", "utf-8");
			request.AutomaticDecompression = DecompressionMethods.GZip;
			request.ContentType = "multipart/form-data;boundary=" + boundary;
			request.Method = "POST";
			request.Proxy = WebRequest.DefaultWebProxy;
			request.Credentials = CredentialCache.DefaultCredentials;
			request.Proxy.Credentials = CredentialCache.DefaultCredentials;

			if (TimeoutMilliseconds > 0)
			{
				request.Timeout = TimeoutMilliseconds;
			}

			byte[] compressedFileContent = CompressByteArray(fileContent);
			var requestStream = request.GetRequestStream();

			//-- file part
			WriteBytes(requestStream, twoHyphens + boundary + crlf);
			WriteBytes(requestStream, "Content-Disposition: form-data; name=\"file\"; filename=\"" + fileName + "\"" + crlf);
			WriteBytes(requestStream, "Content-Type: application/octet-stream" + crlf);
			if (compressedFileContent != null)
			{
				WriteBytes(requestStream, "Content-Transfer-Encoding: gzip" + crlf);
			}
			WriteBytes(requestStream, crlf);
			WriteBytes(requestStream, compressedFileContent ?? fileContent);
			WriteBytes(requestStream, crlf);
			//-- account part
			WriteBytes(requestStream, twoHyphens + boundary + crlf);
			WriteBytes(requestStream, "Content-Disposition: form-data; name=\"account\"" + crlf);
			WriteBytes(requestStream, "Content-Type: text/plain; charset=UTF-8" + crlf);
			WriteBytes(requestStream, crlf);
			WriteBytes(requestStream, Account);
			WriteBytes(requestStream, crlf);
			//-- closing boundary (with -- at the end) to signal end of form 
			WriteBytes(requestStream, twoHyphens + boundary + twoHyphens + crlf);
			//-- finish with thre request
			requestStream.Flush();
			requestStream.Close();

			return request;
		}

		async Task ReadResponseAsync(HttpWebRequest request, string asyncId)
		{
			try
			{
				var responseTask = await request.GetResponseAsync();
				string response = ReadResponse(responseTask as HttpWebResponse);
				string errorMessage = null;
				if (response.StartsWith("ERROR:"))
				{
					errorMessage = response;
					response = "";
				}
				var eventArgs = new ProcessingFinishedEventArgs(asyncId, response, errorMessage);
				OnProcessingFinished(this, eventArgs);
			}
			catch (Exception ex)
			{
				var eventArgs = new ProcessingFinishedEventArgs(asyncId, null, "ERROR:" + ex.Message);
				OnProcessingFinished(this, eventArgs);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		string ReadResponse(HttpWebRequest request)
		{
			string responseOutput = string.Empty;
			try
			{
				HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
				responseOutput = ReadResponse(webResponse);
				if (responseOutput.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
				{
					ErrorMessage = responseOutput;
					responseOutput = "";
				}
			}
			catch (Exception ex)
			{
				ErrorMessage = "Exception :: " + ex.Message;
			}

			return responseOutput;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static string ReadResponse(HttpWebResponse webResponse)
		{
			string output = string.Empty;
			StreamReader responseStreamReader = null;
			try
			{
				var responseStream = GetStreamForResponse(webResponse);
				responseStreamReader = new StreamReader(responseStream);
				var outputBuilder = new StringBuilder();
				string line;
				while ((line = responseStreamReader.ReadLine()) != null)
				{
					outputBuilder.AppendLine(line);
				}
				output = outputBuilder.ToString();
			}
			catch (WebException ex)
			{
				using (WebResponse response = ex.Response)
				{
					using (var data = response.GetResponseStream())
					{
						output = new StreamReader(data).ReadToEnd();
						data.Close();

						if (!output.Contains("CSERROR"))
						{
							output = "ERROR:" + ex.Message + " " + output;
						}
					}
					response.Close();
				}
			}
			catch (Exception ex)
			{
				output = "ERROR:" + ex.Message;
			}
			finally
			{
				if (responseStreamReader != null)
				{
					responseStreamReader.Close();
				}
				webResponse.Close();
			}

			return output;
		}

		static byte[] CompressByteArray(byte[] raw)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
				{
					gzipStream.Write(raw, 0, raw.Length);
				}
				return memoryStream.ToArray();
			}
		}

		static Stream GetStreamForResponse(HttpWebResponse webResponse, int readTimeOut = 0)
		{
			Stream stream;
			var responseStream = webResponse.GetResponseStream();
			switch (webResponse.ContentEncoding?.ToUpperInvariant())
			{
				case "GZIP":
					stream = new GZipStream(responseStream, CompressionMode.Decompress);
					break;
				case "DEFLATE":
					stream = new DeflateStream(responseStream, CompressionMode.Decompress);
					break;

				default:
					stream = responseStream;
					if (readTimeOut > 0)
					{
						stream.ReadTimeout = readTimeOut;
					}
					break;
			}
			return stream;
		}

		static void WriteBytes(Stream stream, string txt)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(txt);
			stream.Write(bytes, 0, bytes.Length);
		}

		static void WriteBytes(Stream stream, byte[] bytes)
		{
			stream.Write(bytes, 0, bytes.Length);
		}

		#endregion
	}

	public class ProcessingFinishedEventArgs : EventArgs
	{
		public ProcessingFinishedEventArgs(string asyncCallId, string result, string error = null)
		{
			AsyncCallId = asyncCallId;
			Result = result;
			Error = error;
		}

		public string AsyncCallId { get; private set; }
		public string Result { get; private set; }
		public string Error { get; private set; }
	}
}

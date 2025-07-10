using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.MHUB;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.MHUB
{
	public class BrokerAccountError
	{
		public BrokerAccountError(string errorMessage, ZString deactivationCode)
		{
			ErrorMessage = errorMessage;
			DeactivationCode = deactivationCode;
		}
		public string ErrorMessage;
		public ZString DeactivationCode;
	}

	public class ServerResponse
	{
		public ServerResponse()
		{
			dictionary = new Dictionary<string, string>();
		}
		readonly Dictionary<string, string> dictionary;

		public bool ContainsKey(string key)
		{
			return dictionary.ContainsKey(key);
		}

		public override string ToString()
		{
			string result = string.Empty;
			foreach (KeyValuePair<string, string> entry in dictionary)
			{
				result += entry.Key + " = " + entry.Value + " | ";
			}
			return result;
		}

		public string GetValue(string key)
		{
			return dictionary.TryGetValue(key, out var result) ? result : string.Empty;
		}

		internal void Add(string key, string value)
		{
			dictionary.Add(key, value);
		}

		internal bool TryGetValue(string key, out string errMsg)
		{
			return dictionary.TryGetValue(key, out errMsg);
		}
	}

	public class ServerResponses : IEnumerable<ServerResponse>
	{
		public ServerResponses(ZString responseMessage)
		{
			ZString formattedBody = ZString.Empty;
			var bodyTagPos = responseMessage.IndexOf("<body>");
			if (bodyTagPos >= 0)
			{
				var endBodyTagPos = responseMessage.IndexOf("</body>");
				var bodyContentLength = endBodyTagPos - bodyTagPos;
				var bodyContent = responseMessage.SubstringSafe(bodyTagPos + 6, bodyContentLength - 6).Trim();
				formattedBody = bodyContent.Replace("requestStatus", "|requestStatus");
			}

			serverResponses = ExtractServerResponsePairsWithoutHtml(formattedBody);
		}

		public static IEnumerable<ServerResponse> ExtractServerResponsePairsWithoutHtml(ZString formattedBody)
		{
			var responses = new List<ServerResponse>();
			var response = new ServerResponse();
			var keyValuePairs = formattedBody.Split('|');

			foreach (var keyValue in keyValuePairs)
			{
				if (!keyValue.IsEmpty)
				{
					string value;
					string key;

					var indexOfEquals = keyValue.IndexOf("=", System.StringComparison.OrdinalIgnoreCase);

					if (indexOfEquals > -1)
					{
						key = keyValue.Substring(0, indexOfEquals).Trim();
						value = keyValue.Substring(indexOfEquals + 1, keyValue.Length - indexOfEquals - 1).Trim();

						if (key == MHUBConstants.Parameters.requestStatus)
						{
							response = new ServerResponse();
							responses.Add(response);
						}

						AddResponseContent(response, key, value, formattedBody);
					}
					else
					{
						if (keyValue.Contains("Server Exception caught:", System.StringComparison.OrdinalIgnoreCase))
						{
							var keyValueError = keyValue.Replace("::", ":");
							var errorValues = keyValueError.Split(':');

							if (errorValues.Length == 3)
							{
								AddResponseContent(response, MHUBConstants.Parameters.ErrorCode, errorValues[1].Trim(), formattedBody);
								AddResponseContent(response, MHUBConstants.Parameters.ErrorMsg, errorValues[2].Trim(), formattedBody);
							}
						}
					}
				}
			}

			if (!responses.Any())
			{
				AddResponseContent(response, MHUBConstants.Parameters.requestStatus, string.Empty, formattedBody);
				responses.Add(response);
			}

			return responses;
		}

		static void AddResponseContent(ServerResponse response, ZString key, ZString value, ZString formattedBody)
		{
			if (response.TryGetValue(key, out var _))
			{
				ErrorReporter.ReportOnce("MHUBWebCommand - An item with the same key has already been added.", FormattableString.Invariant($"'{key}' has been added in this response. The full response body is: {formattedBody}"));
			}
			else
			{
				response.Add(key, value);
			}
		}

		public int Count
		{
			get { return serverResponses.Count(); }
		}

		internal ServerResponse GetFirst()
		{
			return serverResponses.FirstOrDefault();
		}

		readonly IEnumerable<ServerResponse> serverResponses;

		#region IEnumerable<ServerResponse> Members

		public IEnumerator<ServerResponse> GetEnumerator()
		{
			return serverResponses.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return serverResponses.GetEnumerator();
		}

		#endregion
	}

	public class WebResult
	{
		public ZString ResponseString;
		public HttpStatusCode StatusCode;
		public ZString SessionCookie;
	}

	public abstract class MHUBWebCommand
	{
		public MHUBWebCommand(string ediServlet, IMHUBSettings settingsProvider, LoggingInformation logger, bool verboseLogging)
		{
			this.ediServlet = ediServlet;
			this.settingsProvider = settingsProvider;
			this.logger = logger;
			VerboseLogging = verboseLogging;
		}

		readonly string ediServlet;
		protected readonly IMHUBSettings settingsProvider;
		public bool VerboseLogging;

		public ZString RawResponseString { get; private set; }

		public virtual bool Execute()
		{
			VerboseLog("MHUB Web Command");
			bool result = false;
			string commandString = GetCommandString(true);
			string safeCommandString = GetCommandString(false);

			VerboseLog("CommandString = " + safeCommandString);
			WebResult response = null;
			try
			{
				response = GetResponseFromMHub(commandString);
				VerboseLog("Response = " + response.ResponseString);
				RawResponseString = response.ResponseString;
				VerboseLog("Response status code = " + response.StatusCode.ToString());
			}
			catch (WebException e)
			{
				Logger.LogError("WebException: " + e.Message);
			}
			catch (UriFormatException)
			{
				Logger.LogError("Invalid URL: " + safeCommandString);
			}
			catch (SocketException e)
			{
				const int WSAETIMEDOUT = 10060; //connection timed out
				if (e.ErrorCode == WSAETIMEDOUT)
				{
					Logger.LogError("SocketException: " + e.Message);
				}
				else
				{
					VerboseLog("Response SocketException error code = " + e.ErrorCode);
					VerboseLog("Response Socket Exception desc = " + e.Message);
					throw;
				}
			}

			if (response == null)
			{
				VerboseLog("MHUB Web Command response is null");
			}

			if (response != null && response.StatusCode == HttpStatusCode.OK)
			{
				VerboseLog("MHUB Web Command successful");
				ResponseParameters = new ServerResponses(response.ResponseString);
				if (ResponseParameters.Count > 0)
				{
					statusResponseParams = ResponseParameters.GetFirst();
					if (statusResponseParams.ContainsKey(MHUBConstants.Parameters.requestStatus))
					{
						string requestStatus = statusResponseParams.GetValue(MHUBConstants.Parameters.requestStatus);
						if (requestStatus == MHUBConstants.RequestStatusFail)
						{
							if (!string.IsNullOrEmpty(statusResponseParams.GetValue(MHUBConstants.Parameters.ErrorCode)))
							{
								int.TryParse(statusResponseParams.GetValue(MHUBConstants.Parameters.ErrorCode), out var errorNumber);
								CheckForAccountError(errorNumber, statusResponseParams.GetValue(MHUBConstants.Parameters.ErrorMsg));
							}
						}
						result = (requestStatus == MHUBConstants.RequestStatusOK) || (requestStatus == MHUBConstants.RequestStatusOKNoData);
						state = requestStatus;
						VerboseLog("Request Status = " + requestStatus);
					}
				}

				if (result)
				{
					sessionCookie = response.SessionCookie;
				}
			}

			return result;
		}

		public string State
		{
			get { return state; }
		}
		string state = MHUBConstants.RequestStatusFail;

		public BrokerAccountError BrokerAccountError;
		public ServerResponses ResponseParameters;
		protected ServerResponse statusResponseParams;
		internal ZString SessionCookie
		{
			get { return sessionCookie; }
		}
		protected internal ZString sessionCookie;

		protected LoggingInformation Logger
		{
			get
			{
				if (logger == null)
				{
					logger = new LoggingInformation();
				}
				return logger;
			}
		}
		LoggingInformation logger;

		protected virtual WebResult GetResponseFromMHub(string httpRequestUrl)
		{
			var webRequest = GetHttpWebRequest(httpRequestUrl);

			WebResult result;

			if (!sessionCookie.IsEmpty)
			{
				webRequest.Headers.Add("Cookie", sessionCookie);
				webRequest.KeepAlive = true;
				webRequest.ContentType = "text/plain";
				webRequest.Headers.Add("Accept-Language", "en");
				webRequest.Headers.Add("Accept-Charset", "iso-8859-1,*,utf-8");
			}

			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;  // This just allows us to accept the certificate returned at the HTTPS layer, even if it's crap/expired/untrusted/whatever.  It's outside the HTTP layer.  (IP-->TCP-->HTTPS-->HTTP)

			RequestState requestStateObj = new RequestState();
			requestStateObj.Request = webRequest;

			result = new WebResult();
			webRequest.Timeout = settingsProvider.SynchronousTimeout;
			webRequest.ReadWriteTimeout = settingsProvider.SynchronousReadWriteTimeout;
			requestStateObj.Response = webRequest.GetResponse() as HttpWebResponse;

			if (requestStateObj.Response != null)
			{
				using (Stream responseStream = requestStateObj.Response.GetResponseStream())
				{
					StreamReader bodyReader = new StreamReader(responseStream, Encoding.ASCII);
					result.ResponseString = bodyReader.ReadToEnd();
				}
				result.StatusCode = requestStateObj.Response.StatusCode;
				result.SessionCookie = requestStateObj.Response.Headers.Get("Set-Cookie");
			}

			return result;
		}

		protected virtual HttpWebRequest GetHttpWebRequest(string httpRequestUrl)
		{
#pragma warning disable SYSLIB0014
			var webRequest = HttpWebRequest.Create(httpRequestUrl) as HttpWebRequest;
#pragma warning restore SYSLIB0014
			webRequest.Proxy = SG.MHUB.Mhx4Soap.MHAccessClient.GetProxy(settingsProvider.WebProxyAddress);
			webRequest.UserAgent = $"JAVA/{settingsProvider.JreVersion}";

			return webRequest;
		}

		public class RequestState
		{
			const int BUFFER_SIZE = 1024;
			public StringBuilder RequestData;
			public byte[] BufferRead;
			public HttpWebRequest Request;

			public HttpWebResponse Response;

			public Stream ResponseStream;
			public Decoder StreamDecode = Encoding.UTF8.GetDecoder();

			public RequestState()
			{
				BufferRead = new byte[BUFFER_SIZE];
				RequestData = new StringBuilder("");
				Request = null;
				ResponseStream = null;
			}
		}

		protected abstract Dictionary<string, object> InputParameterList { get; }
		protected abstract MHUBConstants.CommandType CommandToExecute { get; }

		protected string GetCommandString(bool showPasswords)
		{
			string result = "";
			result += ediServlet;
			if (CommandToExecute != MHUBConstants.CommandType.None)
			{
				result += "?Command=" + CommandToExecute.ToString();

				foreach (KeyValuePair<string, object> keyValuePair in InputParameterList)
				{
					string key = keyValuePair.Key;
					object value = keyValuePair.Value;
					string valueAsString;
					if (value is DateTime time)
					{
						valueAsString = time.ToString(MHUBConstants.MHubDateFormat);
					}
					else
					{
						valueAsString = value.ToString();
					}

					if (!showPasswords)
					{
						if (key == MHUBConstants.Parameters.Password
							|| key == MHUBConstants.Parameters.NewPassword)
						{
							valueAsString = "********";
						}
					}

					result += "&" + key + "=" + WebUtility.UrlEncode(valueAsString);
				}
			}

			return result;
		}

		protected void VerboseLog(string logmessage)
		{
			if (VerboseLogging)
			{
				logger.Log(logmessage);
			}
		}

		void CheckForAccountError(int errorNumber, string errorMessage)
		{
			switch (errorNumber)
			{
				case 1326: //Account Frozen
					BrokerAccountError = new BrokerAccountError(errorMessage, SGDeactivationCodes.Codes.FRZ);
					break;
				case 1307: //Password Expired
					BrokerAccountError = new BrokerAccountError(errorMessage, SGDeactivationCodes.Codes.PEX);
					break;
				case 1311: //Invalid User ID / Password
				case 1325:
					BrokerAccountError = new BrokerAccountError(errorMessage, SGDeactivationCodes.Codes.IID);
					break;
				case 1318: //User needs to change password
					BrokerAccountError = new BrokerAccountError(errorMessage, SGDeactivationCodes.Codes.PCH);
					break;
				case 1807: //Login ID doesn't exist for MHUB user
					BrokerAccountError = new BrokerAccountError(errorMessage, SGDeactivationCodes.Codes.ANE);
					break;
				case 1308: // Password is incorrect
					BrokerAccountError = new BrokerAccountError(errorMessage, SGDeactivationCodes.Codes.PIC);
					break;
				case 1803: //New password is invalid
					BrokerAccountError = new BrokerAccountError(errorMessage, SGDeactivationCodes.Codes.PCS);
					break;
				default:
					// It isn't a broker account error so do nothing
					break;
			}
		}

#if DEBUG
		public string CommandStringForTesting
		{
			get { return GetCommandString(true); }
		}
#endif
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption;
using Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption.Util;
using Enterprise.Customs.SG.MHUB.Mhx4Soap.Util;
using Enterprise.Customs.SG.V4.MHUB;
using static Enterprise.Customs.SG.V4.MHUB.MHUBConstants;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap
{
	public class MHAccessClient
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MHAccessClient(string login, string password, IMHUBSettings settingsProvider, LoggingInformation logger)
		{
			this.login = login;
			this.password = password;
			this.settingsProvider = settingsProvider;
			appId = MHUBConstants.CARGOWISE;
			vendorId = settingsProvider.VendorID;
			clientVersion = settingsProvider.ClientVersion;
			libsForDigest = settingsProvider.DigestForLibs;
			proxy = GetProxy(settingsProvider.WebProxyAddress);
			this.logger = logger;
			cookieJar = new CookieContainer();
			httpClient = InitializeClient();
			referenceTimestampString = UnixTimestampProvider.GetCurrentUnixTimestampMilliseconds();
			submissionKey = Encoding.ASCII.GetBytes(SecureRandomStringGenerator.GetRandomString()); //"Gp22P_1xTUMxeX6"; //
			submissionCipher = new SessionCipher();
		}

		public bool SubmitInterchangeFile(string file, string messageType, string interchangeNumber, bool hasXMLMessage, IEnumerable<string> attachments = null)
		{
			var loginResult = DoLogin();
			var loginContent = loginResult.Content.ReadAsStringAsync().Result;
			if (!loginContent.Contains("requestStatus=0"))
			{
				logger.LogWarning("Unable to log in: " + loginContent);
				return false;
			}

			try
			{
				var getParamResult = DoGetParam();
				var getParamResultContent = getParamResult.Content.ReadAsStringAsync().Result;

				var serverParams = new ServerParameters(getParamResultContent);
				if (serverParams.Salt.IsEmpty)
				{
					logger.LogError("Unable to get salt for submission: " + getParamResultContent);
					return false;
				}

				var submitResult = hasXMLMessage
					? DoSubmitXmlFile(file, attachments, Encoding.ASCII.GetBytes(referenceTimestampString), serverParams, messageType, interchangeNumber)
					: DoSubmitEdiFactFile(file, attachments, Encoding.ASCII.GetBytes(referenceTimestampString), serverParams, messageType, interchangeNumber);

				if (!submitResult.Contains("<responseCode>0</responseCode>"))
				{
					logger.LogError("Unable to submit file: " + submitResult);
					return false;
				}

				return true;
			}
			finally
			{
				DoLogout();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<string> RetrieveMessageStub(Action<IEnumerable<string>> callbackToSaveInterchangesToDatabaseBeforeDeletingThem)
		{
			var loginResult = DoLogin();
			var loginContent = loginResult.Content.ReadAsStringAsync().Result;
			if (!loginContent.Contains("requestStatus=0"))
			{
				logger.LogWarning("Unable to log in: " + loginContent);
				return Array.Empty<string>();
			}

			var getParamResult = DoGetParam();
			var getParamResultContent = getParamResult.Content.ReadAsStringAsync().Result;
			var serverParams = new ServerParameters(getParamResultContent);
			if (serverParams.Salt.IsEmpty)
			{
				logger.LogError("Unable to get salt for submission: " + getParamResultContent);
				return Array.Empty<string>();
			}

			var sessionId = new Regex(@"sessionId=(.*?)\|").Match(loginContent).Groups[1].Value;
			var filename = login + "_" + referenceTimestampString;
			var retrieveResultContent = DoRetrieve(sessionId, filename, serverParams).Content.ReadAsStringAsync().Result;
			var serverResponses = new ServerResponses(retrieveResultContent);
			var successOrFailure = MhxScriptSuccessChecker.ObtainSuccessOrFailureFromResponses(serverResponses, MHUBConstants.CommandType.Retrieve);

			var filesDownloaded = new List<string>();
			if (successOrFailure == MHUBConstants.ResponseStatusCodes.MailboxEmpty)
			{
				logger.Log("Mailbox empty");
			}
			else if (successOrFailure == MHUBConstants.ResponseStatusCodes.Failed)
			{
				logger.LogError("Unable to retrieve: " + retrieveResultContent);
			}
			else  // complete or partial success
			{
				var downloadAndDecryptStyle = ShouldDownload(serverResponses);
				if (downloadAndDecryptStyle != DownloadAndDecryptOptions.None)
				{
					logger.Log("About to download messages via SOAP. Listing contained " + serverResponses.Count.ToString(CultureInfo.InvariantCulture) + " records");
					filesDownloaded = DoDownloadResponse(filename, Encoding.ASCII.GetBytes(referenceTimestampString), serverParams, downloadAndDecryptStyle);
					callbackToSaveInterchangesToDatabaseBeforeDeletingThem(filesDownloaded); // We do not care whether we saved all, some, or none. We'll delete the whole batch.
					logger.Log("Downloaded " + filesDownloaded.Count.ToString(CultureInfo.InvariantCulture) + " message(s) via SOAP");
					var updateResult = DoUpdate(sessionId);
					var updateResultContent = updateResult.Content.ReadAsStringAsync().Result;
					if (!updateResultContent.Contains("requestStatus=0"))
					{
						logger.Log("Unable to update " + updateResultContent);
					}
				}
				else
				{
					logger.Log("No need to download, listing did not contain any records");
				}
			}

			DoLogout();
			return filesDownloaded;
		}

		protected DownloadAndDecryptOptions ShouldDownload(ServerResponses serverResponses)
		{
			DownloadAndDecryptOptions result = DownloadAndDecryptOptions.None;
			if (serverResponses.Any(r => r.ContainsKey(MHUBConstants.Parameters.requestStatus) && r.GetValue(MHUBConstants.Parameters.requestStatus) != MHUBConstants.RequestStatusOKNoData))  // At least one reply that's not a rerquestStatus=1 (mailbox empty) reply
			{
				result = DownloadAndDecryptOptions.DownloadOnly;
				if (serverResponses.Any(r => r.ContainsKey(MHUBConstants.Parameters.mail_type) && r.GetValue(MHUBConstants.Parameters.mail_type) == MHUBConstants.Parameters.MessageMailType))
				{
					result = DownloadAndDecryptOptions.DownloadAndDecrypt;
				}
			}
			return result;
		}

		// here we set necessary headers to behave exactly as MHAccess client
		// this is important that we maintain these headers to ensure that our direct connection appears exactly as MHAccess does
		// this may be subject to update when new MHAccess versions go out
		protected HttpClient InitializeClient()
		{
			var httpClientHandler = new HttpClientHandler
			{
				Proxy = proxy,
				UseDefaultCredentials = true,
				UseProxy = true,
				CookieContainer = cookieJar
			};
			var httpClientWithADifferentNameToMakeTheAnalyserSTFU = GetHttpClient(httpClientHandler);
			httpClientWithADifferentNameToMakeTheAnalyserSTFU.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "no-cache");
			httpClientWithADifferentNameToMakeTheAnalyserSTFU.DefaultRequestHeaders.TryAddWithoutValidation("Pragma", "no-cache");
			httpClientWithADifferentNameToMakeTheAnalyserSTFU.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Java/" + settingsProvider.JreVersion);
			httpClientWithADifferentNameToMakeTheAnalyserSTFU.DefaultRequestHeaders.TryAddWithoutValidation("Host", GetServiceUri("/").Host);
			httpClientWithADifferentNameToMakeTheAnalyserSTFU.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2");
			httpClientWithADifferentNameToMakeTheAnalyserSTFU.DefaultRequestHeaders.TryAddWithoutValidation("Content-type", "application/x-www-form-urlencoded");
			return httpClientWithADifferentNameToMakeTheAnalyserSTFU;
		}

		protected virtual HttpClient GetHttpClient(HttpClientHandler httpClientHandler)
		{
			return new HttpClient(httpClientHandler);
		}

		// login, password, digestForLibs values are encrypted with Triple DES encryption, with a known key
		// digest for libs is to be updated when the new version of MHAccess comes out (we can use the same value as MHAccess client sends as part of login request, since the value is not changing between sessions)
		// other values are subject to change depending on the MHAccess version
		protected virtual HttpResponseMessage DoLogin()
		{
			Dictionary<string, string> loginValues;

			using (var loginCipher = new LoginCipher(settingsProvider.EncryptionKey))
			{
				loginValues = new Dictionary<string, string>
				{
					{ "Command", "Login" },
					{ "Userid", Convert.ToBase64String(loginCipher.Encrypt(Encoding.ASCII.GetBytes(login))) },
					{ "Password", Convert.ToBase64String(loginCipher.Encrypt(Encoding.ASCII.GetBytes(password))) },
					{ "AppId", appId },
					{ "DigestForLibs", Convert.ToBase64String(loginCipher.Encrypt(Encoding.ASCII.GetBytes(libsForDigest))) },
					{ "Encrypted", "true" }, // true if we encrypt login info
					{ MHUBConstants.Parameters.ClientID, MHUBConstants.MHXWIN }, // true if we encrypt login info
					{ "CurrentVersion", clientVersion },
					{ "VndId", vendorId },
				};
			}

			var content = new FormUrlEncodedContent(loginValues);
			return httpClient.PostAsync(GetServiceUri(settingsProvider.EndpointPartialPathForServlet), content).Result;
		}

		/// <summary>
		/// getParam establishes session encryption parameters, a random string is generated and is encrypted with a username as key and timestamp as password, and hence seed is produced
		/// seed is sent to server side, and server side can decrypt the seed to get the random string using already known username and timestamp
		/// random string (seed) can be considered as session encryption key
		/// timestamp is sent along with seed and is used on server side for decryption of seed to get the random string generated by client
		/// server side responds to this request with a "salt" parameter
		/// </summary>
		HttpResponseMessage DoGetParam()
		{
			var seed = submissionCipher.Encrypt(submissionKey, Encoding.ASCII.GetBytes(login), Encoding.ASCII.GetBytes(referenceTimestampString));
			var getParamValues = new Dictionary<string, string>
			{
				{ "Command", "getParam" },
				{ "CurrentVersion", clientVersion },
				{ "TimeStamp", referenceTimestampString }, //unix epoch time in milliseconds //"1462143375656"
				{ "Seed", Convert.ToBase64String(seed) } //encrypted value that depends on TimeStamp and secureRandomString above //"apKVi6O7y+4q3Ab97ALU4A=="
			};
			var content2 = new FormUrlEncodedContent(getParamValues);
			return httpClient.PostAsync(GetServiceUri(settingsProvider.EndpointPartialPathForServlet), content2).Result;
		}

		HttpResponseMessage DoRetrieve(string sessionId, string filename, ServerParameters serverParams)
		{
			var retrieveValues = new Dictionary<string, string>
			{
				{ "Command", "Retrieve" },
				{ "filename", "file://WS://" + serverParams.FullDownloadPathCalculated(login, settingsProvider.SoapDownloadDirectory) + filename },
				{ "loc","I" },
				{ "split","Y" },
				{ "destroy","N" },
				{ "no_of_msg","50" },
				{ "zipfile","file://" + serverParams.FullDownloadPathCalculated(login, settingsProvider.SoapDownloadDirectory) + filename + ".zip" },
				{ "retr_session",sessionId },
				{ "CurrentVersion", clientVersion },
				{ "Encrypted", "true" }, // true if we encrypt login info
				{ "VndId", vendorId }
			}; // Command=Retrieve&filename=file://WS:///fshome/sftphome/v13t002/mhxdownload/2&loc=I&split=Y&destroy=N&no_of_msg=50&zipfile=file:///fshome/sftphome/v13t002/mhxdownload/2.zip&retr_session_id=C024889571&CurrentVersion=4.0.2.1&Encrypted=true&VndId=A7

			var content2 = new FormUrlEncodedContent(retrieveValues);
			return httpClient.PostAsync(GetServiceUri(settingsProvider.EndpointPartialPathForServlet), content2).Result;
		}

		string DoSubmitEdiFactFile(string file, IEnumerable<string> attachments, byte[] referenceTimestamp, ServerParameters serverParams, string messageType, string interchangeNumber)
		{
			var fileInfo = new FileInfo(file);
			var fileUUID = GetFileUUID();

			var submissionTimestamp = UnixTimestampProvider.GetCurrentUnixTimestampMicroseconds();
			var zippedFilenamePostfix = string.Format(CultureInfo.InvariantCulture, "_{0}.1", submissionTimestamp);

			var hasAttachmentText = attachments != null && attachments.Any() ? "<hasAttachment>true</hasAttachment>" : string.Empty;

			var payloadStringEdiFact = "<?xml version='1.0' encoding='UTF-8'?>" +
									   "<S:Envelope xmlns:S=\"http://schemas.xmlsoap.org/soap/envelope/\"><S:Body>" +
									   "<ns2:uploadRequest xmlns:ns2=\"http://service.wsvc.mhb.crimsonlogic.com/wsdl\">" +
									   "<userID>{0}</userID>" + // login
									   "<fileName>{1}</fileName>" + // unix epoch in microseconds
									   "<version>{2}</version>" + // client version
									   "<uuid>{3}</uuid>" + //WSUUID
									   "<payload>" +
									   "<xop:Include xmlns:xop=\"http://www.w3.org/2004/08/xop/include\" href=\"cid:{4}@example.jaxws.sun.com\"/>" + // XOP link
									   "</payload>" +
									   "<recipients>{5}</recipients>" + // recipient DCST401
									   "<contentType>E</contentType>" +
									   "<docType>{6}</docType>" + // doctype CUSDEC
									   "<zipFile>{1}</zipFile>" + // unix epoch time in microseconds
									   "<notification>N</notification>" +
									   "<contentId>{7}</contentId>" + // content id (36)
									   "{9}" + //has attachments? <hasAttachment>true</hasAttachment>
									   "<ediContentId>{7}</ediContentId>" +
									   "<ediDocType>{6}</ediDocType>" + // doctype CUSDEC
									   "<ediRecipient>{5}</ediRecipient>" + // recipient DCST401
									   "<ediZipFileName>{8}.zip</ediZipFileName>" + // filename zipped
									   "<encrypted>true</encrypted>" +
									   "<clientId>MHXWIN</clientId>" +
									   "<vndId>{10}</vndId>" +
									   "</ns2:uploadRequest></S:Body>" +
									   "</S:Envelope>";

			var payloadText = string.Format
			(
				CultureInfo.InvariantCulture,
				payloadStringEdiFact,
				login, submissionTimestamp, clientVersion, serverParams.WsUuid, fileUUID, settingsProvider.RecipientID, messageType, interchangeNumber, fileInfo.Name + zippedFilenamePostfix, hasAttachmentText, vendorId
			);

			return DoSubmitFileCore(fileInfo, zippedFilenamePostfix, attachments, referenceTimestamp, serverParams, payloadText, fileUUID);
		}

		string DoSubmitXmlFile(string file, IEnumerable<string> attachments, byte[] referenceTimestamp, ServerParameters serverParams, string messageType, string interchangeNumber)
		{
			var hasAttachment = attachments != null && attachments.Any();
			var hasAttachmentText = hasAttachment ? "<hasAttachment>true</hasAttachment>" : string.Empty;

			var fileInfo = new FileInfo(file);
			var fileName = hasAttachment ? $"inner{fileInfo.Name}.zip" : fileInfo.Name;
			var fileUUID = GetFileUUID();

			var submissionTimestamp = UnixTimestampProvider.GetCurrentUnixTimestampMicroseconds();

			var payloadStringXml = "<?xml version='1.0' encoding='UTF-8'?>" +
								   "<S:Envelope xmlns:S=\"http://schemas.xmlsoap.org/soap/envelope/\"><S:Body>" +
								   "<ns2:uploadRequest xmlns:ns2=\"http://service.wsvc.mhb.crimsonlogic.com/wsdl\">" +
								   "<userID>{0}</userID>" + // login
								   "<fileName>{1}</fileName>" + // unix epoch in microseconds
								   "<localPath>{2}.zip</localPath>" + // filename
								   "<version>{3}</version>" + // client version
								   "<uuid>{4}</uuid>" + //WSUUID
								   "<payload>" +
								   "<xop:Include xmlns:xop=\"http://www.w3.org/2004/08/xop/include\" href=\"cid:{5}@example.jaxws.sun.com\"/>" + // XOP link
								   "</payload>" +
								   "<recipients>{6}</recipients>" + // recipient DCST401
								   "<contentType>B</contentType>" +
								   "<docType>{7}</docType>" +
								   "<zipFile>{1}</zipFile>" + // unix epoch time in microseconds
								   "<notification>N</notification>" +
								   "<contentId>{8}</contentId>" +
								   "{9}" + //has attachments? <hasAttachment>true</hasAttachment>
								   "<encrypted>true</encrypted>" +
								   "<clientId>MHXWIN</clientId>" +
								   "<vndId>{10}</vndId>" +
								   "</ns2:uploadRequest></S:Body>" +
								   "</S:Envelope>";

			var payloadText = string.Format
			(
				CultureInfo.InvariantCulture,
				payloadStringXml,
				login, submissionTimestamp, fileName, clientVersion, serverParams.WsUuid, fileUUID, settingsProvider.RecipientID, messageType, interchangeNumber, hasAttachmentText, vendorId
			);

			return DoSubmitFileCore(fileInfo, string.Empty, attachments, referenceTimestamp, serverParams, payloadText, fileUUID);
		}

		string DoSubmitFileCore(FileInfo fileInfo, string zippedFilenamePostfix, IEnumerable<string> attachments, byte[] referenceTimestamp, ServerParameters serverParams, string payloadText, string fileUUID)
		{
			var submissionSalt = GetSubmissionSalt(referenceTimestamp, serverParams);
			var bytesZip = GetZippedBytes(fileInfo, attachments, zippedFilenamePostfix);

			var encryptedPayloadBytes = GetEncryptedPayloadBytes(submissionSalt, bytesZip);

			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("SOAPAction", @"");
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "JAX-WS RI 2.2.8 svn-revision#13980");
			httpClient.DefaultRequestHeaders.ExpectContinue = false;

			var s = GetNewSubmitPostRequest();

			var dicPost = new Dictionary<string, string>();
			dicPost.Add(string.Empty, payloadText);

			return s.ExecutePostRequestSubmitPayload(GetServiceUri(settingsProvider.EndpointPartialPathForUpload), dicPost, encryptedPayloadBytes, fileUUID, cookieJar, proxy);
		}

		protected virtual string GetFileUUID() => Guid.NewGuid().ToString();

		protected virtual byte[] GetZippedBytes(FileInfo fileInfo, IEnumerable<string> attachments, string zippedFilenamePostfix)
		{
			var zip = new InMemoryZip();

			return attachments != null && attachments.Any()
				? zip.GetZippedBytesWithAttachments(fileInfo, attachments.Select(f => new FileInfo(f)), zippedFilenamePostfix)
				: zip.GetZippedBytes(fileInfo, zippedFilenamePostfix);
		}

		protected virtual byte[] GetEncryptedPayloadBytes(byte[] submissionSalt, byte[] bytesZip)
		{
			return submissionCipher.Encrypt(bytesZip, submissionKey, submissionSalt);
		}

		List<string> DoDownloadResponse(string filename, byte[] referenceTimestamp, ServerParameters serverParams, DownloadAndDecryptOptions downloadAndDecryptStyle)
		{
			byte[] submissionSalt = GetSubmissionSalt(referenceTimestamp, serverParams);

			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("SOAPAction", @"");
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "JAX-WS RI 2.2.8 svn-revision#13980");
			httpClient.DefaultRequestHeaders.ExpectContinue = false;
			var s = GetNewSubmitPostRequest();
			var dicPost = new Dictionary<string, string>();

			var payloadString = "<?xml version='1.0' encoding='UTF-8'?>" +
								"<S:Envelope xmlns:S=\"http://schemas.xmlsoap.org/soap/envelope/\"><S:Body>" +
								"<ns2:downloadRequest xmlns:ns2=\"http://service.wsvc.mhb.crimsonlogic.com/wsdl\">" +
								"<userID>{0}</userID>" +
								"<fileName>{1}.zip</fileName>" +
								"<version>{2}</version>" +
								"<uuid>{3}</uuid>" +
								"<vndId>{4}</vndId>" +
								"</ns2:downloadRequest>" +
								"</S:Body></S:Envelope>";

			var fileUUID = Guid.NewGuid().ToString();
			var payloadText = string.Format(CultureInfo.InvariantCulture, payloadString, login, filename, clientVersion, serverParams.WsUuid, vendorId);
			dicPost.Add("", payloadText);
			var encryptedPayload = s.ExecutePostRequestRetrievePayload(GetServiceUri(settingsProvider.EndpointPartialPathForDownload), dicPost, Array.Empty<byte>(), fileUUID, cookieJar, proxy);
			List<string> interchangeStrings = downloadAndDecryptStyle == DownloadAndDecryptOptions.DownloadAndDecrypt ? DecryptPayloadAndUnzip(submissionSalt, encryptedPayload) : new List<string>();
			return interchangeStrings;
		}

		protected virtual byte[] GetSubmissionSalt(byte[] referenceTimestamp, ServerParameters serverParams)
		{
			return submissionCipher.Decrypt(Convert.FromBase64String(serverParams.Salt), submissionKey, referenceTimestamp);
		}

		protected virtual List<string> DecryptPayloadAndUnzip(byte[] submissionSalt, byte[] encryptedPayload)
		{
			var decrypted = submissionCipher.Decrypt(encryptedPayload, submissionKey, submissionSalt);
			var interchangeStrings = new List<string>();
			using (var zippedDataStream = new MemoryStream(decrypted))
			{
				Stream unzippedStream;
				var archive = new ZipArchive(zippedDataStream);
				foreach (var fileEntry in archive.Entries)
				{
					using (unzippedStream = fileEntry.Open())
					{
						using (var reader = new StreamReader(unzippedStream)) // assumes text not binary - but being UNOA EDIFACT this this OK
						{
							string text = reader.ReadToEnd();
							interchangeStrings.Add(text);
						}
					}
				}
			}

			return interchangeStrings;
		}

		protected virtual SubmitRequestPost GetNewSubmitPostRequest()
		{
			return new SubmitRequestPost();
		}

		HttpResponseMessage DoUpdate(string sessionId)
		{
			Dictionary<string, string> updateValues;
			updateValues = new Dictionary<string, string>
			{
				{ "Command", "Update" },
				{ "session_id", sessionId + ".1" },
				{ "destroy", "Y" }
			};
			var content = new FormUrlEncodedContent(updateValues);
			return httpClient.PostAsync(GetServiceUri(settingsProvider.EndpointPartialPathForServlet), content).Result;
		}

		HttpResponseMessage DoLogout()
		{
			Dictionary<string, string> logoutValues;
			logoutValues = new Dictionary<string, string>
			{
				{ "Command", "Logout" },
			};
			var content = new FormUrlEncodedContent(logoutValues);
			return httpClient.PostAsync(GetServiceUri(settingsProvider.EndpointPartialPathForServlet), content).Result;
		}

		Uri GetServiceUri(string partialPath)
		{
			return new Uri(settingsProvider.WebAddress + partialPath);
		}

		internal static IWebProxy GetProxy(string webProxyAddress)
		{
			IWebProxy result = WebRequest.DefaultWebProxy;

			if (!string.IsNullOrEmpty(webProxyAddress))
			{
				// The proxy, and IS's attitude towards it, can bite me.
				var proxyHost = Regex.Split(webProxyAddress + ":", ":")[0];
				var proxyPort = Regex.Split(webProxyAddress + ":", ":")[1];
				result = new WebProxy(proxyHost, int.Parse(proxyPort, System.Globalization.CultureInfo.InvariantCulture));
			}
			return result;
		}

		readonly LoggingInformation logger;
		readonly string libsForDigest;
		readonly string login;
		readonly string password;
		readonly string appId;
		readonly string vendorId;
		readonly string clientVersion;
		readonly IWebProxy proxy;
		protected HttpClient httpClient;
		readonly CookieContainer cookieJar;
		readonly string referenceTimestampString;
		readonly byte[] submissionKey;
		readonly SessionCipher submissionCipher;
		readonly IMHUBSettings settingsProvider;
	}
}

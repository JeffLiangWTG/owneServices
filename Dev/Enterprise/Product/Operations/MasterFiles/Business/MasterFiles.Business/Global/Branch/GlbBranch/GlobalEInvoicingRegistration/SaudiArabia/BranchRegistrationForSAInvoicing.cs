using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia
{
	public class BranchRegistrationForSAInvoicing : IRegisterBranchForGlobalEInvoicing
	{
		public BranchRegistrationForSAInvoicing(GlbBranch branch, OrgHeader debtor, ICSRGenerator csrGenerator, IHttpClientProvider httpClientProvider, ILogger logger)
		{
			Branch = Argument.NotNull(branch, nameof(branch));
			Debtor = Argument.NotNull(debtor, nameof(debtor));
			HttpClientProvider = Argument.NotNull(httpClientProvider, nameof(httpClientProvider));
			CSRGenerator = Argument.NotNull(csrGenerator, nameof(csrGenerator));
			Logger = Argument.NotNull(logger, nameof(logger));
			//Intentionally initializing these prior to using them in any async calls, to ensure data is retrieved from DB on the same thread as tests.
			Supplier = new Supplier(Branch);
			Customer = new Customer(Debtor);
		}

		IHttpClientProvider HttpClientProvider { get; }

		ICSRGenerator CSRGenerator { get; }

		GlbBranch Branch { get; }

		OrgHeader Debtor { get; }

		Supplier Supplier { get; }

		Customer Customer { get; }

		ILogger Logger { get; }

		public string OTP { get; set; }

		void IRegisterBranchForGlobalEInvoicing.Register()
		{
			HttpStatusCode httpResponseCode;
			var httpResponseContent = string.Empty;
			var requestId = string.Empty;
			var binarySecurityToken = string.Empty;
			var secret = string.Empty;
			var invoiceHash = string.Empty;
			var creditNoteHash = string.Empty;

			try
			{
				var (privateKey, csrData) = CSRGenerator.Generate();
				var privateKeyToDipslay = privateKey.Replace("\n", "\r\n");
				var csrDataToDisplay = csrData.Replace("\n", "\r\n");
				Logger.Log(LogType.Information, string.Format("{0}\r\n", privateKeyToDipslay));

				//Calls the compliance API
				Logger.Log(LogType.Information, string.Format("Submitting to {0}\r\n", API_Compliance_URI));
				Logger.Log(LogType.Information, string.Format("OTP: {0}\r\n", OTP));
				Logger.Log(LogType.Information, csrDataToDisplay);
				var task = Task.Run(() => SendRegistrationComplianceAPI(OTP, csrData));
				(httpResponseCode, httpResponseContent) = task.GetAwaiter().GetResult();
				if (!ProcessRegistrationResponse(httpResponseCode, httpResponseContent, ref requestId, ref binarySecurityToken, ref secret))
				{
					RaiseFaileRegistrationEvent();
					return;
				}

				var debtorInformation = $"Debtor {Debtor.CompanyData.Code} using address {Customer.AddressCode}\r\n";

				//Calls the test invoice API
				Logger.Log(LogType.Information, string.Format("\r\n\r\nSubmitting test Standard Invoice\r\n"));
				Logger.Log(LogType.Information, debtorInformation);
				Logger.Log(LogType.Information, string.Format("{0}/invoices\r\n", API_Compliance_URI));
				var transactionTask = Task.Run(() => SendTrialInvoiceAPI(binarySecurityToken, secret));
				(httpResponseCode, httpResponseContent, invoiceHash) = transactionTask.GetAwaiter().GetResult();
				if (!ProcessValidationResponse(httpResponseCode, httpResponseContent))
				{
					RaiseFaileRegistrationEvent();
					return;
				}

				//Calls the test credit note API
				Logger.Log(LogType.Information, string.Format("\r\nSubmitting test Standard Credit Note\r\n"));
				Logger.Log(LogType.Information, debtorInformation);
				Logger.Log(LogType.Information, string.Format("{0}/invoices\r\n", API_Compliance_URI));
				transactionTask = Task.Run(() => SendTrialCreditNoteAPI(binarySecurityToken, secret, invoiceHash));
				(httpResponseCode, httpResponseContent, creditNoteHash) = transactionTask.GetAwaiter().GetResult();
				if (!ProcessValidationResponse(httpResponseCode, httpResponseContent))
				{
					RaiseFaileRegistrationEvent();
					return;
				}

				//Calls the test debit note API
				Logger.Log(LogType.Information, string.Format("\r\nSubmitting test Standard Debit Note\r\n"));
				Logger.Log(LogType.Information, debtorInformation);
				Logger.Log(LogType.Information, string.Format("{0}/invoices\r\n", API_Compliance_URI));
				transactionTask = Task.Run(() => SendTrialDebitNoteAPI(binarySecurityToken, secret, creditNoteHash));
				(httpResponseCode, httpResponseContent, _) = transactionTask.GetAwaiter().GetResult();
				if (!ProcessValidationResponse(httpResponseCode, httpResponseContent))
				{
					RaiseFaileRegistrationEvent();
					return;
				}

				//Calls the onboarding API
				Logger.Log(LogType.Information, string.Format("\r\n\r\nSubmitting to {0}\r\n", API_Onboarding_URI));
				task = Task.Run(() => SendRegistrationOnboardingAPI(binarySecurityToken, secret, requestId));
				(httpResponseCode, httpResponseContent) = task.GetAwaiter().GetResult();
				if (!ProcessRegistrationResponse(httpResponseCode, httpResponseContent, ref requestId, ref binarySecurityToken, ref secret))
				{
					RaiseFaileRegistrationEvent();
					return;
				}

				if (registrationSuccessful != null)
				{
					registrationSuccessful.Invoke(this, new SARegistrationEventArgs { BinarySecurityToken = binarySecurityToken, RequestId = requestId, Secret = secret });
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (httpResponseContent.IsNullOrEmpty())
				{
					var messages = new List<string>();
					do
					{
						messages.Add(ex.Message);
						ex = ex.InnerException;
					}
					while (ex != null);
					var message = string.Join("\r\n", messages);
					Logger.Log(LogType.Information, string.Format("\r\n{0}", message));
				}
				else
				{
					Logger.Log(LogType.Information, string.Format("\r\nResponse: {0}\r\n", httpResponseContent));
				}

				RaiseFaileRegistrationEvent();
			}

			void RaiseFaileRegistrationEvent()
			{
				if (registrationFailed != null)
				{
					registrationFailed.Invoke(this, EventArgs.Empty);
				}
			}
		}

		event EventHandler IRegisterBranchForGlobalEInvoicing.RegistrationSuccessful
		{
			add { registrationSuccessful += value; }
			remove { registrationSuccessful -= value; }
		}
		event EventHandler registrationSuccessful;

		event EventHandler IRegisterBranchForGlobalEInvoicing.RegistrationFailed
		{
			add { registrationFailed += value; }
			remove { registrationFailed -= value; }
		}
		event EventHandler registrationFailed;

		#region ProcessRegistrationResponse

		bool ProcessRegistrationResponse(HttpStatusCode responseStatusCode, string responseContent, ref string requestId, ref string binarySecurityToken, ref string secret)
		{
			Logger.Log(LogType.Information, string.Format("\r\nReceived response: {0} {1}\r\n", (int)responseStatusCode, responseStatusCode.ToString()));
			if (responseStatusCode == HttpStatusCode.OK)
			{
				ProcessGoodRegistrationResponse(responseContent, out binarySecurityToken, out secret, out requestId);
				return true;
			}
			else
			{
				ProcessBadRegistrationResponse(responseContent);
				return false;
			}
		}

		void ProcessGoodRegistrationResponse(string responseContent, out string binarySecurityToken, out string secret, out string requestId)
		{
			var bo = ConvertRegistrationWebResponseToGoodResponseData(responseContent);
			requestId = bo.RequestID;
			secret = bo.Secret;
			binarySecurityToken = bo.BinarySecurityToken;
			var decodedToken = Base64EncoderDecoder.Decode(binarySecurityToken);
			Logger.Log(LogType.Information, string.Format("requestID: {0}\r\n", requestId));
			Logger.Log(LogType.Information, string.Format("secret: {0}\r\n", secret));
			var chunkedStrings = SplitStringWithFixedLength(decodedToken, 64);
			var tokenText = string.Join("\r\n", chunkedStrings);

			Logger.Log(LogType.Information, string.Format(@"binarySecurityToken:
-----BEGIN CERTIFICATE-----
{0}
-----END CERTIFICATE-----", tokenText));
		}

		void ProcessBadRegistrationResponse(string responseContent)
		{
			var badResponse = ConvertRegistrationWebResponseToBadResponseData(responseContent);
			if (badResponse == null || badResponse.Code == null)
			{
				var errorList = ConvertRegistrationWebResponseToBadResponseDataList(responseContent);
				if (errorList?.Errors?.Count > 0)
				{
					badResponse = errorList.Errors[0];
				}
			}
			if (badResponse != null && badResponse.Code != null)
			{
				Logger.Log(LogType.Information, string.Format("code: {0}\r\n", badResponse.Code));
				Logger.Log(LogType.Information, string.Format("message: {0}\r\n", badResponse.Message));
			}
			else
			{
				Logger.Log(LogType.Information, string.Format("\r\nResponse: {0}\r\n", responseContent));
			}
		}

		#endregion

		#region ProcessValidationResponse

		bool ProcessValidationResponse(HttpStatusCode responseStatusCode, string responseContent)
		{
			Logger.Log(LogType.Information, string.Format("Received response: {0} {1}\r\n", (int)responseStatusCode, responseStatusCode.ToString()));
			ProcessValidationResponseCore(responseContent);
			return responseStatusCode == HttpStatusCode.OK || responseStatusCode == HttpStatusCode.Accepted;
		}

		void ProcessValidationResponseCore(string responseContent)
		{
			var response = ConvertValidationWebResponseToResponseData(responseContent);
			if (response != null && response.ValidationResults != null)
			{
				if (response.ValidationResults.ErrorMessages != null && response.ValidationResults.ErrorMessages.Any())
				{
					foreach (var error in response.ValidationResults.ErrorMessages)
					{
						Logger.Log(LogType.Information, string.Format("code: {0}\r\n", error.Code));
						Logger.Log(LogType.Information, string.Format("message: {0}\r\n", error.Message));
					}
				}
				else if (response.ValidationResults.WarningMessages != null && response.ValidationResults.WarningMessages.Any())
				{
					foreach (var warning in response.ValidationResults.WarningMessages)
					{
						Logger.Log(LogType.Information, string.Format("code: {0}\r\n", warning.Code));
						Logger.Log(LogType.Information, string.Format("message: {0}\r\n", warning.Message));
					}
				}
			}
			else
			{
				Logger.Log(LogType.Information, string.Format("\r\nResponse: {0}\r\n", responseContent));
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html elements")]
		protected async Task<(HttpStatusCode ResponseCode, string ResponseContent)> SendRegistrationComplianceAPI(string otpCode, string csrData)
		{
			var headerElements = new (string Name, string Value)[]
										{
											("accept", "application/json")
											, ("OTP", otpCode)
											, ("Accept-Version", "V2")
										};
			var encodedCSRData = Base64EncoderDecoder.Encode(csrData);
			var content = string.Format("{{\"csr\":\"{0}\"}}", encodedCSRData);

			using (var httpClient = HttpClientProvider.GetHttpClient())
			using (var request = HttpHelper.GetJsonPostHttpRequestMessage(API_Compliance_URI, headerElements, content))
			{
				var response = await HttpHelper.SendAsync(httpClient, request);
				return (response.StatusCode, response.ResponseText);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html elements")]
		protected async Task<(HttpStatusCode ResponseCode, string ResponseContent)> SendRegistrationOnboardingAPI(string token, string secret, string requestId)
		{
			var authorizationData = string.Format("{0}:{1}", token, secret);
			var encodedData = Base64EncoderDecoder.Encode(authorizationData);
			var headerElements = new (string Name, string Value)[]
										{
											("accept", "application/json")
											, ("Accept-Version", "V2")
											, ("Authorization", string.Format("Basic {0}", encodedData))
										};
			var content = string.Format("{{\"compliance_request_id\":\"{0}\"}}", requestId);

			using (var httpClient = HttpClientProvider.GetHttpClient())
			{
				using (var request = HttpHelper.GetJsonPostHttpRequestMessage(API_Onboarding_URI, headerElements, content))
				{
					var response = await HttpHelper.SendAsync(httpClient, request);
					return (response.StatusCode, response.ResponseText);
				}
			}
		}

		protected async Task<(HttpStatusCode ResponseCode, string ResponseContent, string TransactionHash)> SendTrialInvoiceAPI(string token, string secret)
		{
			var trialInvoice = TrialTransaction.Invoice(Supplier, Customer);
			return await SendTrialTransactionAPI(token, secret, trialInvoice);
		}

		protected async Task<(HttpStatusCode ResponseCode, string ResponseContent, string TransactionHash)> SendTrialCreditNoteAPI(string token, string secret, string previousTransactionHash)
		{
			var trialCreditNote = TrialTransaction.CreditNote(Supplier, Customer, previousTransactionHash);
			return await SendTrialTransactionAPI(token, secret, trialCreditNote);
		}

		protected async Task<(HttpStatusCode ResponseCode, string ResponseContent, string TransactionHash)> SendTrialDebitNoteAPI(string token, string secret, string previousTransactionHash)
		{
			var trialDebitNote = TrialTransaction.DebitNote(Supplier, Customer, previousTransactionHash);
			return await SendTrialTransactionAPI(token, secret, trialDebitNote);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html elements")]
		async Task<(HttpStatusCode ResponseCode, string ResponseContent, string TransactionHash)> SendTrialTransactionAPI(string token, string secret, TrialTransaction testTransaction)
		{
			var testTransactionURI = string.Format("{0}/invoices", API_Compliance_URI);
			var authorizationData = string.Format("{0}:{1}", token, secret);
			var encodedData = Base64EncoderDecoder.Encode(authorizationData);
			var headerElements = new (string Name, string Value)[]
										{
											("accept", "application/json")
											, ("Accept-Language", "en")
											, ("Accept-Version", "V2")
											, ("Authorization", string.Format("Basic {0}", encodedData))
										};

			var transactionXml = testTransaction.ToXml();
			var invoiceHash = GenerateInvoiceHash(transactionXml);
			var transactionData = Base64EncoderDecoder.Encode(transactionXml);
			var content = string.Format("{{\"invoiceHash\":\"{0}\",\n\"uuid\":\"{1}\",\n\"invoice\":\"{2}\" }}", invoiceHash, testTransaction.Uuid, transactionData);

			using (var httpClient = HttpClientProvider.GetHttpClient())
			using (var request = HttpHelper.GetJsonPostHttpRequestMessage(testTransactionURI, headerElements, content))
			{
				var response = await HttpHelper.SendAsync(httpClient, request);
				return (response.StatusCode, response.ResponseText, invoiceHash);
			}
		}

		protected ValidationResponseData ConvertValidationWebResponseToResponseData(string responseText)
		{
			var businessObject = JsonConvert.DeserializeObject<ValidationResponseData>(responseText);
			return businessObject;
		}

		protected RegistrationGoodResponseData ConvertRegistrationWebResponseToGoodResponseData(string responseText)
			=> JsonConvert.DeserializeObject<RegistrationGoodResponseData>(responseText)
				?? throw new ApplicationException(string.Format(@"Unable to retrieve response {0} - object is null.
Response: {1}", nameof(RegistrationGoodResponseData), responseText));

		protected RegistrationBadResponseData ConvertRegistrationWebResponseToBadResponseData(string responseText)
		{
			var businessObject = JsonConvert.DeserializeObject<RegistrationBadResponseData>(responseText);
			return businessObject;
		}

		protected RegistrationBadResponseDataList ConvertRegistrationWebResponseToBadResponseDataList(string responseText)
		{
			var businessObject = JsonConvert.DeserializeObject<RegistrationBadResponseDataList>(responseText);
			return businessObject;
		}
		
		string GenerateInvoiceHash(string xmlInvoice)
		{
			var transformedXml = RemoveRedundantElementsFromTheXmlInvoice(xmlInvoice);
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(transformedXml)))
			{
				var transformer = new XmlDsigC14NTransform(false);
				transformer.LoadInput(ms);
				using (var sha256 = SHA256.Create())
				{
					var invoiceHashBytes = transformer.GetDigestedOutput(sha256);
					return Convert.ToBase64String(invoiceHashBytes);
				}
			}
		}

		string RemoveRedundantElementsFromTheXmlInvoice(string invoiceXml)
		{
			var embededXsl = Assembly.GetExecutingAssembly().GetManifestResourceStream("SA.Data.invoice");
			var transformedXml = string.Empty;
			using (var ms = new MemoryStream())
			{
				using (var streamWriter = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 32768, leaveOpen: true))
				{
					streamWriter.Write(invoiceXml);
				}
				ms.Seek(0, SeekOrigin.Begin);
				var reader = XmlReader.Create(ms);
				var xmlWriterSettings = new XmlWriterSettings();
				xmlWriterSettings.OmitXmlDeclaration = true;
				xmlWriterSettings.Encoding = Encoding.UTF8;
				xmlWriterSettings.Indent = false;
				transformedXml = ApplyXSLT(reader, xmlWriterSettings, embededXsl);
			}

			return transformedXml;
		}

		string ApplyXSLT(XmlReader inputXmlReader, XmlWriterSettings xmlWriterSettings, Stream xslResourceStream)
		{
			var stringBuilder = new StringBuilder();
			using (var results = XmlWriter.Create(stringBuilder, xmlWriterSettings))
			{
				xslResourceStream.Seek(0L, SeekOrigin.Begin);
				var stylesheet = XmlReader.Create(xslResourceStream);
				var xslCompiledTransform = new XslCompiledTransform();
				xslCompiledTransform.Load(stylesheet);
				xslCompiledTransform.Transform(inputXmlReader, results);
			}
			return stringBuilder.ToString();
		}
		
		IEnumerable<string> SplitStringWithFixedLength(string str, int n)
		{
			if (string.IsNullOrEmpty(str) || n < 1)
			{
				throw new ArgumentException(null, nameof(str));
			}

			for (var i = 0; i < str.Length; i += n)
			{
				yield return str.Substring(i, Math.Min(n, str.Length - i));
			}
		}

		ZString API_Compliance_URI => AccountingMasterFilesRegistry.Instance.SaudiArabiaEInvoicingCSIDAPIEndPoint.Value;

		ZString API_Onboarding_URI => AccountingMasterFilesRegistry.Instance.SaudiArabiaEInvoicingOnboardingAPIEndPoint.Value;
	}
}

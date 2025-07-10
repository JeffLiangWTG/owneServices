using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.MHUB.Mhx4Soap.Util;
using Enterprise.Customs.SG.V4.MHUB;
using static Enterprise.Customs.SG.V4.MHUB.MHUBConstants;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Testing
{
	public sealed class MHAccessClientForTest : MHAccessClient
	{
		public MHAccessClientForTest(string login, string password, IMHUBSettings settingsProvider, LoggingInformation logger, bool runScenarioWithOnlyAnNdnToDownload = false, bool pukeOnLoginForTest = false, string cannedResponse = null) : base(login, password, settingsProvider, logger)
		{
			SubmitRequest = new SubmitRequestPostForTest();
			this.cannedResponse = cannedResponse;
			this.pukeOnLoginForTest = pukeOnLoginForTest;
			this.runScenarioWithOnlyAnNdnToDownload = runScenarioWithOnlyAnNdnToDownload;
			if (runScenarioWithOnlyAnNdnToDownload || cannedResponse != null)
			{
				httpClient = InitializeClient();
			}
		}

		protected override HttpResponseMessage DoLogin()
		{
			if (pukeOnLoginForTest)
			{
				throw new AggregateException("Some stupid async thing", new WebException("Web sez no", new System.Net.Sockets.SocketException(10060)));
			}
			else
			{
				return base.DoLogin();
			}
		}

		protected override byte[] GetEncryptedPayloadBytes(byte[] submissionSalt, byte[] bytesZip) => Encoding.ASCII.GetBytes("I am now encrypted");

		protected override SubmitRequestPost GetNewSubmitPostRequest() => SubmitRequest;

		protected override string GetFileUUID() => LastFileUUID = base.GetFileUUID();

		protected override byte[] GetSubmissionSalt(byte[] referenceTimestamp, ServerParameters serverParams) => Array.Empty<byte>();

		protected override byte[] GetZippedBytes(FileInfo fileInfo, IEnumerable<string> attachments, string zippedFilenamePostfix)
		{
			LastAttachments = attachments;
			return base.GetZippedBytes(fileInfo, attachments, zippedFilenamePostfix);
		}

		protected override List<string> DecryptPayloadAndUnzip(byte[] submissionSalt, byte[] encryptedPayload)
		{
			if (runScenarioWithOnlyAnNdnToDownload)
			{
				throw new Exception("In the scenario whereby we have only NDNs to download, we should never be asked to DecryptPayloadAndUnzip().  If we are asked, the test fails.");
			}

			return new List<string>()
			{ Encoding.ASCII.GetString(encryptedPayload) };
		}

		internal SubmitRequestPostForTest SubmitRequest { get; private set; }

		protected override HttpClient GetHttpClient(HttpClientHandler httpClientHandler)
		{
			if (!cannedResponse.IsEmpty)
			{
				return new HttpClientForTestingCustomResponse(httpClientHandler)
				{ CannedResponse = cannedResponse };
			}
			else if (runScenarioWithOnlyAnNdnToDownload)
			{
				return new HttpClientForTestForOnlyNdnDownload(httpClientHandler);
			}
			else
			{
				return new HttpClientForTest(httpClientHandler);
			}
		}

		internal HttpClientForTest HttpClientForTest
		{
			get
			{
				return (HttpClientForTest)base.httpClient;
			}
		}

		internal DownloadAndDecryptOptions ShouldDownloadExposed(ServerResponses serverResponses)
		{
			return ShouldDownload(serverResponses);
		}

		internal IEnumerable<string> LastAttachments { get; set; }

		internal string LastFileUUID { get; set; }

		readonly bool runScenarioWithOnlyAnNdnToDownload;
		readonly bool pukeOnLoginForTest;
		readonly ZString cannedResponse;
	}

	sealed class SubmitRequestPostForTest : SubmitRequestPost
	{
		public string CannedDataToSendBack { get; set; }

		public HttpWebRequestWrapperForTest Wrapper { get; private set; }

		protected override string CreateFormDataBoundary() => "DELIMITMEBABY";

		protected override HttpWebRequestWrapper GetNewRequest(Uri url)
		{
#pragma warning disable SYSLIB0014
			Wrapper = new HttpWebRequestWrapperForTest((HttpWebRequest)WebRequest.Create(url.AbsoluteUri));
#pragma warning restore SYSLIB0014
			Wrapper.CannedDataToSendBack = CannedDataToSendBack;
			return Wrapper;
		}
	}

	sealed class HttpWebRequestWrapperForTest : HttpWebRequestWrapper
	{
		public HttpWebRequestWrapperForTest(HttpWebRequest httpWebRequest) : base(httpWebRequest)
		{
			MemoryStreamRequest = new MemoryStream();
		}

		public override Stream GetRequestStream() => MemoryStreamRequest;

		public override WebResponseWrapper GetResponse() => new WebResponseWrapperForTest(CannedDataToSendBack);

		public MemoryStream MemoryStreamRequest { get; private set; }

		public string CannedDataToSendBack { get; set; }
	}

	sealed class WebResponseWrapperForTest : WebResponseWrapper
	{
		public WebResponseWrapperForTest() : base(null)
		{
		}

		public WebResponseWrapperForTest(string cannedDataToSendBack) : this()
		{
			this.cannedDataToSendBack = cannedDataToSendBack;
		}

		public override Stream GetResponseStream() => new MemoryStream(System.Text.Encoding.ASCII.GetBytes(cannedDataToSendBack));

		readonly string cannedDataToSendBack = "";
	}

	sealed class HttpClientForTestForOnlyNdnDownload : HttpClientForTest
	{
		public HttpClientForTestForOnlyNdnDownload(HttpMessageHandler handler) : base(handler)
		{
		}

		protected override string GetMainPayloadCannedResponse()
		{
			return @"NDN-only test: For Retrieve listing (download) or for logout (upload): <body>\r\nrequestStatus=0|msgId=M200505241242023319|mail_type=4|</body>"; // type=4 --> NDN
		}
	}

	sealed class HttpClientForTestingCustomResponse : HttpClientForTest
	{
		public HttpClientForTestingCustomResponse(HttpMessageHandler handler) : base(handler)
		{
		}

		protected override string GetMainPayloadCannedResponse() => CannedResponse;

		public string CannedResponse { get; set; }
	}

	internal class HttpClientForTest : HttpClient
	{
		public HttpClientForTest(HttpMessageHandler handler) : base(handler)
		{
		}

		public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var replies = new List<string>()
			{ "for login:  requestStatus=0|sessionId=123456|", @"for getParam:
															<body>
																<FSUSERID>Daniel</FSUSERID>
																<FSUSERHOME>/milton/keynes/</FSUSERHOME>
																<FSUPLOAD>/mhxupload/</FSUPLOAD>
																<FSDOWNLOAD>/mhxdownload/</FSDOWNLOAD>
																<SALT>34GQLuw39OUM6iOnkWNO/g==</SALT>
																<WSUUID>UNHDLRgVNzF/B6suNswlZErL/Ycpoyif56hlYrR7cqXDAcel74P4saZavIIpE7tW</WSUUID>
															</body>", GetMainPayloadCannedResponse(), "for update/destroy:  requestStatus=0", "for logout (download only):  requestStatus=0", };
			RequestsMade.Add(request.Content.ReadAsStringAsync().Result);
			var response = new HttpResponseMessage(HttpStatusCode.OK);
			response.Content = new ByteArrayContent(System.Text.Encoding.ASCII.GetBytes(replies[requestIndex]));
			requestIndex++;
			return Task.FromResult(response);
		}

		protected virtual string GetMainPayloadCannedResponse()
		{
			return @"For Retrieve listing (download) or for logout (upload): <body>\r\nrequestStatus=0|msgId=M200505241242023319|cont_id=91390863|nfn=N|recip_id=scl10b5|SubmDate=24052005124200|Size=332|subj=null|doc_type=APERAK|subm_session=1116909721976.1|sender_email_id=null|recipient_email_id=null|folder=INBOX|cont_type=E|DeliveryDate=24052005124202|loc=A|mail_type=1|sender_id=dcs2001|path=/fshome/mhbsdt01/mhub/20050524/Msg_O200505241242023319|status=N|state= |fetchedDate=24052005124210|archivalDate=24052005124222|restoreDate=null|reasonCode=null|appId=MHB|encType=A|senderDomain=null|retr_session=1116909729740.1|foreignMsgId=null|</body>";
		}

		public List<string> RequestsMade = new List<string>();
		int requestIndex;
	}
}

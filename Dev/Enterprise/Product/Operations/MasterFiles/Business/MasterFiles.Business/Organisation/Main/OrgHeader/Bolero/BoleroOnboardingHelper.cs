using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using WTG.Foundation.Http;

namespace Enterprise.MasterFiles.Business
{
	public class BoleroOnBoardingHelper
	{
		readonly OrgHeader orgHeader;

		public BoleroOnBoardingHelper(OrgHeader orgHeader)
		{
			this.orgHeader = orgHeader;
		}

		BusinessObjectFactory factory;
		BusinessObjectFactory Facotry
		{
			get
			{
				return factory ??= new BusinessObjectFactory();
			}
		}

		public async Task<string> SendEnrollmentRequestAsync(OnboardingRequestDTO requestDTO, CancellationToken cancellationToken)
		{
			AddEvent(AutoEvents.MessageSentCode);

			var errorMessage = string.Empty;

			try
			{
				var requestUrl = GetRequestUrl();

				using var client = GetHttpClient();
				var postBody = JsonConvert.SerializeObject(requestDTO);
				var response = await client.PostAsync(requestUrl, new StringContent(postBody, Encoding.UTF8, "application/json"), cancellationToken);

				var responseString = await response.Content.ReadAsStringAsync();
				var result = JsonConvert.DeserializeObject<ResponseDTO>(responseString);

				switch (result.OpCode)
				{
					case HttpStatusCode.OK:
						// TODO: Add MPP Event and other logic
						break;
					case HttpStatusCode.Conflict:
						// TODO: Add MPP Event and other logic
						break;
					case HttpStatusCode.Gone:
						// TODO: Add MAA Event and other logic
						break;
					case HttpStatusCode.InternalServerError:
						// TODO: Add MRJ Event and other logic
						break;
					default:
						break;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errorMessage = HandleBoleroRequestException(ex);
			}

			return errorMessage;
		}

		public bool IsOverriddenRequest()
		{
			var lastLog = orgHeader.Logs
				.GetAllLogsByEventOrderByPostedTimeUtcDESC(
					AutoEvents.MessageSent,
					AutoEvents.MessagePendingProcessing,
					AutoEvents.MessageAccepted,
					AutoEvents.InterchangeRejected,
					AutoEvents.MessageRejected)
				.FirstOrDefault(l => l.SL_Reference.Contains((NoResString)"DEP=Bolero"));

			if (lastLog != null && (lastLog.SL_SE_NKEvent == AutoEvents.MessageSent.Code || lastLog.SL_SE_NKEvent == AutoEvents.MessagePendingProcessing.Code))
			{
				return true;
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "url")]
		protected virtual string GetRequestUrl()
		{
			const string endPoint = "onboarding/enrolment/rest/enrolment-management/enrol";
			var baseUrl = OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.Value.GetGalileoEndPointUrl();
			var requestUrl = new Uri(new Uri(baseUrl), endPoint).AbsoluteUri;

			return requestUrl;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected HttpClient GetHttpClient()
		{
			var httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromSeconds(OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.Value.GetTimeout());
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Add("User-Agent", "CargoWise Next");
			httpClient.DefaultRequestHeaders.Authorization = TokenManager.GetInstance(MDMProductCodes.BOL).GetAuthorizationHeaderValue();

			return httpClient;
		}

		void AddEvent(string eventCode, string dep = "Bolero", string rid = "", string rejReason = "", string location = "")
		{
			var eventLog = Facotry.New<StmALog>();

			using (((IUpdateFieldsLock)eventLog).LockForUpdatingKeyFields())
			{
				eventLog.SL_Parent = orgHeader.PK;
				eventLog.SL_Table = OrgHeaderSchema.Constants.TableName;
				eventLog.SL_Reference = $"|DEP={dep}|MST=Enrolment Request{(string.IsNullOrEmpty(rid) ? string.Empty : $"|RFN={rid}")}{(string.IsNullOrEmpty(location) ? string.Empty : $"|LOC={location}")}{(string.IsNullOrEmpty(rejReason) ? string.Empty : $"|RES={rejReason}")}";
				eventLog.SL_SE_NKEvent = eventCode;
			}

			Facotry.Save();
		}

		string HandleBoleroRequestException(Exception ex)
		{
			string errorMessage;
			if (ex is AuthenticationException)
			{
				errorMessage = Res.GetString("38B6378F-837C-45BA-806F-A62DF9556F37", "Failed to authenticate. Error Message: {0}", ex.Message);
			}
			else if (ex is WebException || ex is HttpRequestException)
			{
				errorMessage = Res.GetString(
					"3322C38E-F384-49C5-98F2-6585A84DFCCE",
					"Failed to communicate successfully to Bolero Enrollment Web Service.\r\nPlease contact your I.T. person and confirm that the service URIs are up to date.\r\nError Message: {0}",
					ex.Message);
			}
			else if (ex is HttpTimeoutException || ex is TaskCanceledException || (ex is AggregateException aggregateException && aggregateException.InnerException is TaskCanceledException))
			{
				AddEvent(AutoEvents.MessageRejectedCode, dep: (NoResString)"CargoWise", rejReason: (NoResString)"In case of timeout");
				errorMessage = Res.GetString(
					"DE3734D9-AC8B-420F-8F53-4B8E1E035544",
					"The request to Bolero Service is timed out, please try again later. You can also increase the timeout value for calling the web service (current value is {0} seconds). If the issue persists please raise a Customer Service Incident.",
					OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.Value.GetTimeout());
			}
			else
			{
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
				}

				errorMessage = Res.GetString("1047D39A-0011-40D4-B8F4-E5FF093693E2", "Error Message: {0}", ex.Message);
			}

			return errorMessage;
		}
	}
}

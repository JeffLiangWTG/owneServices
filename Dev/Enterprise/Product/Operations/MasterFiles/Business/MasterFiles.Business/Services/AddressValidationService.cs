using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using WTG.AddressCleansing.Common;
using WTG.Foundation.Http;
using ILogger = Enterprise.Integration.ILogger;

namespace Enterprise.MasterFiles.Business
{
	public static class ValidationResultStatusCode
	{
		public const string NotChecked = "NTC";
		public const string PrivateAddressNotChecked = "PAN";

		public const string Error = "ERR";

		public const string Invalid = "INV";
		public const string PrivateAddressInvalid = "PAI";

		public const string CityClose = "CCL";
		public const string CityExact = "CET";
		public const string StreetClose = "SCL";
		public const string StreetExact = "SET";
		public const string PointClose = "PCL";
		public const string PrivateAddressValid = "PAV";
		public const string PointExact = "PET";
		public const string CountryDataNotAvailable = "CNA";
	}

	public class WebAddressValidationResult
	{
		public ValidationResultItem ResultAddress { get; set; }
		public string Message { get; set; }
		public List<ValidationResultItem> SuggestedResults { get; set; }

		public bool HasTopRecommendedAddress
		{
			get
			{
				//If we get a PCL result OR If we get a SCL result + Available Data = 2
				//THEN we should save that result and display it at the top of the suggestions Control. We should mark this option as a "Top Pick"
				return ResultAddress != null &&
					   (ResultAddress.ResultStatusCode == ValidationResultStatusCode.PointClose ||
						(ResultAddress.ResultStatusCode == ValidationResultStatusCode.StreetClose &&
						 ResultAddress.AvailableData == AvailableData.Street));
			}
		}

		public HttpStatusCode? StatusCode { get; set; }

		public Exception ExceptionToReport { get; internal set; }

		public ValidationResultItem TopRecommendedAddress
		{
			get
			{
				if (topRecommendedAddress == null)
				{
					if (HasTopRecommendedAddress)
					{
						topRecommendedAddress = ResultAddress;
						topRecommendedAddress.Group = AddressValidationService.Constants.RecommendedAddress;
					}
				}
				return topRecommendedAddress;
			}
		}
		ValidationResultItem topRecommendedAddress;
	}

	public static class AddressValidationService
	{
		internal static bool HasNoWebServiceError(AddressCleansingResultItem result)
		{
			return result.ValidationResultItem != null && result.ValidationResultItem.ResultStatusCode != ValidationResultStatusCode.Error;
		}

		internal static Task<AddressValidationServiceUri> GetAvailableWebServiceAddressAsync()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (string.IsNullOrEmpty(availableWebServiceAddress.Uri))
				{
					availableWebServiceAddress = GetAvailableWebServiceAddress();
				}

				return Task.FromResult(availableWebServiceAddress);
			}

#endif
			return Task.Factory.StartNew(() => { return GetAvailableWebServiceAddressSafe(); }, CancellationToken.None, TaskCreationOptions.AttachedToParent, ObjectFactory.Get<TaskScheduler>());
		}

#if DEBUG
		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static AddressValidationServiceUri availableWebServiceAddress = new ();

		public static void SetAvailableWebServiceAddress(string value = "")
		{
			availableWebServiceAddress = new AddressValidationServiceUri { Uri = value };
		}
#endif

		internal static AddressValidationServiceUri GetAvailableWebServiceAddressSafe()
		{
			using (Db.DisposableActionForDbConnection())
			{
				return GetAvailableWebServiceAddress();
			}
		}

		public static AddressValidationServiceUri AvailableBackgroundWebServiceAddress
		{
			get
			{
				return AddressValidationServiceUrisProvider.GetAddressValidationServiceUris().Background;
			}
		}

		#region SuppressResourceStringsCheckRegion

		public static class Constants
		{
			public static string ExceedLengthOfAdditionalAddress
			{
				get
				{
					return Res.GetString("4000BF3E-F103-445A-94AD-1D02092EA5C5", $"The validated address' additional address information details could not be added to the existing additional address information field as the value is longer than 50 characters.");
				}
			}

			internal static string RecommendedAddress
			{
				get
				{
					return Res.GetString("4A6F3773-F028-4BB0-B254-5DC8083E8914", "Recommended Address");
				}
			}

			internal static string AddressWithPrefix
			{
				get
				{
					return Res.GetString("D09E10FA-6D76-46A2-820C-AB6FDC34E136", "Address with Prefix");
				}
			}
			public const string ValidationServiceName = "CleanseAddress";
			internal const string CheckServiceStatusName = "GetServiceStatus";
			internal const string GetCityTown = "GetCandidateCityTowns";
			internal const string GetCountryDataAvailability = "GetCountryDataAvailability";
		}

		#region Validation Web Service Monitoring
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static IMessageWriter MessageWriter { get; set; }

		public static void WriteMessage(string message)
		{
			if (MessageWriter != null)
			{
				MessageWriter.WriteMessage(message);
			}
		}

		static string ConstructRequestMessage(string uri)
		{
			string message = string.Format(CultureInfo.CurrentCulture, @"Sending request at {0}
URI
{1}
" + System.Environment.NewLine, ZDateTime.Now, uri);
			return message;
		}

		static string ConstructRequestBody(string body)
		{
			string message = string.Format(CultureInfo.CurrentCulture, @"Body
{0}
" + System.Environment.NewLine, body);
			return message;
		}

		static string ConstructValidationResponseMessage(string response)
		{
			string message = string.Format(CultureInfo.CurrentCulture, @"Received response at {0}
Reponse as text:
{1}
" + System.Environment.NewLine, ZDateTime.Now, response);
			return message;
		}

		static string ConstructSuggestionResponseMessage(string response, int numberOfSuggestedAddress)
		{
			string message = string.Format(CultureInfo.CurrentCulture, @"Received response at {0}
{1} suggested addresses returned
Reponse as text:
{2}
" + System.Environment.NewLine, ZDateTime.Now, numberOfSuggestedAddress, response);
			return message;
		}

		static string ConstructErrorMessage(string error)
		{
			string message = string.Format(CultureInfo.CurrentCulture, @"Received error message at {0}
{1}
" + System.Environment.NewLine, ZDateTime.Now, error);
			return message;
		}

		static string ConstructCancellationMessage()
		{
			string message = string.Format(CultureInfo.CurrentCulture, @"Web request has been cancelled." + System.Environment.NewLine);
			return message;
		}
		#endregion

		public static bool IsAddressInValidStatus(ISupportWebAddressValidation address)
		{
			if (address != null && address.ValidationStatus == AddressValidationStatus.NotRequired)
			{
				return false;
			}

			return !IsAddressNeedValidation(address) || address.ValidationStatus == AddressValidationStatus.ManuallyVerified || address.ValidationStatus == AddressValidationStatus.CountryNotAvailable;
		}

		public static bool IsAddressNeedValidation(ISupportWebAddressValidation address)
		{
			return
				address != null &&
				address.ValidationStatus != AddressValidationStatus.Verified &&
				address.ValidationStatus != AddressValidationStatus.VerifiedToStreet &&
				address.ValidationStatus != AddressValidationStatus.CountryNotAvailable &&
				address.ValidationStatus != AddressValidationStatus.NotRequired;
		}

		public static WebAddressValidationResult ValidateAddress(ISupportWebAddressValidation address, CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
		{
			return ValidateAddress(address, GetAvailableWebServiceAddress(), cancellationToken, true, cleanseAction);
		}

		public static WebAddressValidationResult ValidateAddressViaBackgroundEndpoint(ISupportWebAddressValidation address, CancellationTokenSource cancellationToken)
		{
			return ValidateAddress(address, AvailableBackgroundWebServiceAddress, cancellationToken, true, CleanseAction.QuickValidate);
		}

		public static async Task<WebAddressValidationResult> ValidateAddressAsync(ISupportWebAddressValidation address, CancellationTokenSource cancellationToken, bool hasRetried = false, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
		{
			var serviceUri = await GetAvailableWebServiceAddressAsync();

			if (!Globals.IsTest && (SynchronizationContext.Current == null || SynchronizationContext.Current.GetType() == typeof(SynchronizationContext)))
			{
				throw new InvalidOperationException("Need to provide a valid sync context in order for ValidateAddressAsync to be thread safe");
			}

			address.IsExactPointFound = false;
			var validationResult = new WebAddressValidationResult();
			var requestContent = CreateRequestContent(address, serviceUri.Uri, cleanseAction, validationResult, out string uriFull);
			if (requestContent == null)
			{
				return validationResult;
			}

			try
			{
				using var client = GetHttpClient(serviceUri.EnableS2STAuth, true);
				string responseData = null;
				AddressCleansingResultItem result = null;

				var response = await Task.Factory.StartNew(() =>
				{
					return client.PostAsync(uriFull, requestContent, cancellationToken.Token).Result;
				}, cancellationToken.Token, TaskCreationOptions.None, ObjectFactory.Get<TaskScheduler>());

				validationResult.StatusCode = response.StatusCode;

				if (response.IsSuccessStatusCode)
				{
					// Read the result (the deserialization is automatic)
					result = await Task.Factory.StartNew(() =>
					{
						return (response.Content.ReadAsAsync<AddressCleansingResult>().Result).Items[0];
					}, cancellationToken.Token, TaskCreationOptions.None, ObjectFactory.Get<TaskScheduler>());

					responseData = HasNoWebServiceError(result) ? response.Content.ReadAsStringAsync().Result : null;
					address.IsExactPointFound = result.ValidationResultItem?.ResultStatusCode == ValidationResultStatusCode.PointExact;
				}

				ProcessAddressValidationResponse(address, validationResult, response, result, responseData);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				await HandleAddressValidationExceptionAsync(ex, validationResult, address, cancellationToken, hasRetried, serviceUri.Uri).ConfigureAwait(false);
			}

			return validationResult;
		}

		static WebAddressValidationResult ValidateAddress(ISupportWebAddressValidation address, AddressValidationServiceUri serviceUri, CancellationTokenSource cancellationToken, bool hasRetried, CleanseAction cleanseAction)
		{
			address.IsExactPointFound = false;
			var validationResult = new WebAddressValidationResult();
			var requestContent = CreateRequestContent(address, serviceUri.Uri, cleanseAction, validationResult, out string uriFull);
			if (requestContent == null)
			{
				return validationResult;
			}

			try
			{
				using var client = GetHttpClient(serviceUri.EnableS2STAuth, true);
				string responseData = null;
				AddressCleansingResultItem result = null;
				var response = client.PostAsync(uriFull, requestContent, cancellationToken.Token).Result;
				validationResult.StatusCode = response.StatusCode;
				if (response.IsSuccessStatusCode)
				{
					// Read the result (the deserialization is automatic)
					result = (response.Content.ReadAsAsync<AddressCleansingResult>()).Result.Items[0];
					responseData = HasNoWebServiceError(result) ? response.Content.ReadAsStringAsync().Result : null;
					address.IsExactPointFound = result.ValidationResultItem?.ResultStatusCode == ValidationResultStatusCode.PointExact;
				}

				ProcessAddressValidationResponse(address, validationResult, response, result, responseData);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleAddressValidationExceptionAsync(ex, validationResult, address, cancellationToken, hasRetried, serviceUri.Uri).Wait();
			}

			return validationResult;
		}

		static HttpContent CreateRequestContent(ISupportWebAddressValidation address, string endpointUrlAddress, CleanseAction cleanseAction, WebAddressValidationResult validationResult, out string uriFull)
		{
			uriFull = null;
			HttpContent result = null;

			if (string.IsNullOrEmpty(endpointUrlAddress))
			{
				validationResult.Message = Res.GetString("CFAC2742-2C07-45F8-BC83-CE675B2E9F32", "None of the Address Validation services is available at the moment, please try again later. You can also increase the timeout value for calling the web service (current value is {0} seconds). If the issue persists please raise a Customer Service Incident.", Env.Instance.Registry.AddressValidationWebServiceTimeout);
			}
			else if (address != null && !address.IsRowDeletedOrDetachedOrNull)
			{
				// Body parameter (List of requests)
				var request = new ValidationRequest();

				// Request 1
				var requestItem = new AddressItem
				{
					Addressee = address.Addressee,
					AddressRecordGUID = address.AddressRecordGUID,
					AddressSourceTable = address.AddressSourceTable,
					Address1 = address.Address1,
					Address2 = address.Address2,
					City = address.City,
					State = address.State,
					Postcode = address.Postcode,
					CountryCode = address.CountryCodeISO2,
					CompanyName = address.CompanyName
				};
				request.Items.Add(requestItem);
				request.RequestID = Guid.NewGuid().ToString();

				uriFull = GetQueryStringForValidationService(endpointUrlAddress, address.Language, cleanseAction);
				var postBody = Newtonsoft.Json.JsonConvert.SerializeObject(request);

				WriteMessage(ConstructRequestMessage(uriFull));
				WriteMessage(ConstructRequestBody(postBody));

				result = new StringContent(postBody, Encoding.UTF8, "application/json");
			}

			return result;
		}

		internal static void ProcessAddressValidationResponse(ISupportWebAddressValidation address, WebAddressValidationResult validationResult, HttpResponseMessage response, AddressCleansingResultItem result, string responseData)
		{
			if (response.IsSuccessStatusCode)
			{
				if (address != null && !address.IsRowDeletedOrDetachedOrNull)
				{
					if (HasNoWebServiceError(result))
					{
						if (address.Language.StartsWith(Core.SharedConstants.Languages.English, StringComparison.OrdinalIgnoreCase))
						{
							result.TransliterateAddressToEnglish();
						}

						result.AdjustAddressCase(address, Env.Registry.OrgAllowMixedCase)
							.FilterUnparsedInformation();

						if (!string.IsNullOrEmpty(result.ValidationResultItem.ErrorMessage))
						{
							validationResult.Message = Res.GetString("6983A795-CFFD-440B-AB0E-CDE0510632E4", "Error message returned from Address Validation Web Service: {0}", result.ValidationResultItem.ErrorMessage);
						}
						validationResult.ResultAddress = result.ValidationResultItem;
						SetCoordinatesAndValidationStatus(address, validationResult.ResultAddress);

						if (address.ValidationStatus == AddressValidationStatus.Invalid && result.Suggestions.Count > 0)
						{
							validationResult.SuggestedResults = result.Suggestions;
							WriteMessage(ConstructSuggestionResponseMessage(responseData, result.Suggestions.Count));
						}
						else
						{
							WriteMessage(ConstructValidationResponseMessage(responseData));
						}
					}
				}
			}
			else
			{
				var errorResponseBuilder = new StringBuilder();
				if (response.StatusCode == HttpStatusCode.BadRequest)
				{
					string responseString = null;
					try
					{
						responseString = response.Content.ReadAsStringAsync().Result;
						var responseJObject = JObject.Parse(responseString);
						var title = responseJObject.Property("title");
						if (title != null)
						{
							errorResponseBuilder.AppendLine(title.Value.ToString());
						}

						var errors = responseJObject["errors"];
						if (errors != null)
						{
							foreach (var property in errors.Children<JProperty>())
							{
								errorResponseBuilder.AppendLine(property.Name + ": " + property.Value[0]);
							}
						}
					}
					catch (Exception ex)
					{
						ErrorReporter.ReportOnce("Error occurred when ProcessAddressValidationResponse", ex);
					}
					finally
					{
						if (string.IsNullOrEmpty(errorResponseBuilder.ToString()) && !string.IsNullOrEmpty(responseString))
						{
							errorResponseBuilder.AppendLine(responseString);
						}
					}
				}

				var statusCode = (int)response.StatusCode;
				if (!string.IsNullOrEmpty(errorResponseBuilder.ToString()))
				{
					validationResult.Message = Res.GetString("3F8C7C70-10FC-4B8E-95F3-834C20692A2B", "The remote server returned an error: ({0})", statusCode);
					validationResult.Message += System.Environment.NewLine + errorResponseBuilder;
				}
				else
				{
					validationResult.Message = Res.GetString("D1506F9B-C285-443D-8BB1-BB767646CF0D",
						"CargoWise Address Validation Web Service responses with errors. Please contact your I.T. person and confirm that the service Uris are up to date. The remote server returned an error: ({0})", statusCode);
				}

				var requestDetails = string.Join(System.Environment.NewLine, response.RequestMessage.ToString().Split(','));
				var errorMessage = string.Format(CultureInfo.CurrentCulture, "REQUEST:\n{0}\n\nERROR:\n{1}\n\nCODE:\n{2}", requestDetails, response.ReasonPhrase, statusCode);
				WriteMessage(ConstructErrorMessage(errorMessage));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		static async Task HandleAddressValidationExceptionAsync(
			Exception ex,
			WebAddressValidationResult validationResult,
			ISupportWebAddressValidation address,
			CancellationTokenSource cancellationToken,
			bool hasRetried,
			string unavailableUrl)
		{
			validationResult.ExceptionToReport = ex;
			if (ex is AuthenticationException)
			{
				validationResult.Message = Res.GetString("703AEC7B-68C0-4F9D-84AC-5ECAC645B4CB", "Failed to authenticate. Error Message: {0}", ex.Message);

				return;
			}

			if (ex is WebException || ex is HttpRequestException)
			{
				validationResult.Message = Res.GetString(
					"8767A331-C617-4DC6-A48F-81756E24CFAC",
					"Failed to communicate successfully to CargoWise Address Validation Web Service. " +
					"Please contact your I.T. person and confirm that the service URIs are up to date. " +
					"Error Message: {0}",
					ex.Message);

				return;
			}

			TaskCanceledException taskCanceledException;
			if (ex is AggregateException && ex.InnerException != null)
			{
				if ((taskCanceledException = ex.InnerException as TaskCanceledException) != null)
				{
					await HandleTaskCanceledExceptionAsync(address, validationResult, cancellationToken, hasRetried, taskCanceledException, unavailableUrl);
				}
				else
				{
					validationResult.Message = Res.GetString("17C4CC2B-5F0A-46B4-89FB-C9B873389D1B", "Aggregate Exception raised when validating address using web service.\r\nInner Error Message: {0}", ex.InnerException.Message);
				}
			}
			else if ((taskCanceledException = ex as TaskCanceledException) != null)
			{
				await HandleTaskCanceledExceptionAsync(address, validationResult, cancellationToken, true, taskCanceledException, unavailableUrl);
			}
			else
			{
				TargetInvocationException targetInvocationException = ex as TargetInvocationException;
				IndexOutOfRangeException indexOutOfRangeException;
				if (targetInvocationException != null)
				{
					Globals.Message.ShowDeveloperException("17C4CC2B-5b4A-46B4-89FB-C9B873389D1B", "Exception raised when validating address using web service.\r\nTargetInvocationException: {0}", ex);
				}
				else if ((indexOutOfRangeException = ex as IndexOutOfRangeException) != null)
				{
					Globals.Message.ShowDeveloperException("17C4CC2B-5c5A-46B4-89FB-C9B873389D1B", "Exception raised when validating address using web service.\r\nIndexOutOfRangeException: {0}", ex);
				}
				validationResult.Message = Res.GetString("B910D161-67D4-4B3A-B66F-9CECA9C6DD54", "Exception raised when validating address using web service.\r\nError Message: {0}", ex.GetInnermostException().Message);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static async Task HandleTaskCanceledExceptionAsync(
			ISupportWebAddressValidation address,
			WebAddressValidationResult validationResult,
			CancellationTokenSource cancellationToken,
			bool hasRetried,
			TaskCanceledException ex,
			string unavailableUrl)
		{
			if (!cancellationToken.IsCancellationRequested)
			{
				if (!hasRetried)
				{
					try
					{
						validationResult = await ValidateAddressAsync(address, cancellationToken, true);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						if (ex.InnerException != null)
						{
							validationResult.Message = Res.GetString("17C4CC2B-5F0A-46B4-89FB-C9B873389D1B", "Aggregate Exception raised when validating address using web service.\r\nInner Error Message: {0}", ex.InnerException.Message);
						}
						else
						{
							validationResult.Message = Res.GetString("C642AC6D-ED47-4C92-875D-BBC5CD7412D7", "Aggregate Exception raised when validating address using web service.\r\nError Message: {0}", ex.Message);
						}
					}
				}
				else
				{
					validationResult.Message = Res.GetString("2184256D-C09D-4266-85EA-239FAD6E86D4", "The Address Validation service has not responded in a timely manner. Please try again in a few minutes. If the issue persists please raise a Customer Service Incident.");
				}
			}
			else
			{
				validationResult.Message = Res.GetString("1E4A49B9-9168-4CCA-903B-6197F0F72261", "The Address Validation services at {0} is timed out, please try again later. You can also increase the timeout value for calling the web service (current value is {1} seconds). If the issue persists please raise a Customer Service Incident.", unavailableUrl, Env.Instance.Registry.AddressValidationWebServiceTimeout);
			}
		}

		public static void SetSuggestedAddressToAddressForValidation(ValidationResultItem suggestedAddress, bool verifyAsEntered, ISupportWebAddressValidation addressForValidation, bool shouldUpdateGeoLocationOnly = false)
		{
			try
			{
				addressForValidation.IsValidatingAddress = true;

				if (suggestedAddress != null && !verifyAsEntered)
				{
					if (!shouldUpdateGeoLocationOnly && !(addressForValidation.IsTSAKnownAddress || addressForValidation.IsMIDAddress))
					{
						// Do not update values if the same to avoid firing unnecessary change events which could fire calls to the validation web service

						addressForValidation.Address1 = GetAddressPropertySafe(
							suggestedAddress.Address1,
							addressForValidation.Address1,
							addressForValidation.Address1_MaxLength);

						addressForValidation.Address2 = GetAddressPropertySafe(
							suggestedAddress.Address2,
							addressForValidation.Address2,
							addressForValidation.Address2_MaxLength);

						addressForValidation.City = GetAddressPropertySafe(
							suggestedAddress.City,
							addressForValidation.City,
							addressForValidation.City_MaxLength);

						var suggestedPostcode = GetAddressPropertySafe(
								suggestedAddress.Postcode,
								addressForValidation.Postcode,
								addressForValidation.Postcode_MaxLength);

						if (!string.IsNullOrWhiteSpace(suggestedPostcode)
							|| addressForValidation.Country == null
							|| addressForValidation.Country.RN_PostcodeValidationRule != CountryAddressValidationRuleList.Codes.MustBeEntered
							|| (suggestedAddress.ResultStatusCode != ValidationResultStatusCode.PointExact
								&& suggestedAddress.ResultStatusCode != ValidationResultStatusCode.PointClose))
						{
							addressForValidation.Postcode = suggestedPostcode;
						}

						addressForValidation.UnrestrictedAdditionalAddressInformation = ConsolidateAddressAdditionalInformation(
							addressForValidation.UnrestrictedAdditionalAddressInformation,
							suggestedAddress.UnparsedAddressInformation);

						if (addressForValidation.Country != null && addressForValidation.Country.IsStateMustNotBeEntered)
						{
							addressForValidation.State = string.Empty;
						}
						else
						{
							UpdateSuggestedState(suggestedAddress, addressForValidation);
						}
					}

					var suggestedAddressGeoLocation = ZGeography.CreatePoint(suggestedAddress.Longitude, suggestedAddress.Latitude);

					if (addressForValidation.GeoLocation != suggestedAddressGeoLocation)
					{
						addressForValidation.GeoLocation = suggestedAddressGeoLocation;
					}

					SetClosestPort(addressForValidation);

					SetAddressMap(addressForValidation, suggestedAddress);

					if (addressForValidation.ValidationStatus != AddressValidationStatus.Verified)
					{
						addressForValidation.ValidationStatus = AddressValidationStatus.Verified;
					}
				}
				else if (verifyAsEntered)
				{
					// If verify as entered and suggest address not null we have a top suggestion and we want to
					// use the lat/long from this top suggestion
					if (suggestedAddress != null)
					{
						var suggestedAddressGeoLocation = ZGeography.CreatePoint(suggestedAddress.Longitude, suggestedAddress.Latitude);
						if (addressForValidation.GeoLocation != suggestedAddressGeoLocation)
						{
							addressForValidation.GeoLocation = suggestedAddressGeoLocation;
						}

						SetClosestPort(addressForValidation);
					}
					else
					{
						addressForValidation.GeoLocation = ZGeography.Empty;
					}

					if (addressForValidation.ValidationStatus != AddressValidationStatus.ManuallyVerified)
					{
						addressForValidation.ValidationStatus = AddressValidationStatus.ManuallyVerified;
					}
				}
			}
			finally
			{
				addressForValidation.IsValidatingAddress = false;
			}
		}

		#region Set Closest Port

		static List<RefUNLOCO> RetrieveUnlocosByCountryStateCity(BusinessObjectFactory factory, string country, RefCountryStates state, string city)
		{
			var searchUnlocoQuery = new ZQuery(RefUNLOCOSchema.RL_RW, state.PK);
			searchUnlocoQuery.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, country);
			searchUnlocoQuery.AddToFilter(RefUNLOCOSchema.RL_IsActive, true);

			var searchUnlocoSubQuery = new ZQuery(RefUNLOCOSchema.RL_PortName, city);
			searchUnlocoSubQuery.AddToFilter(JoinCondition.Or, RefUNLOCOSchema.RL_NameWithDiacriticals, city);
			searchUnlocoQuery.AddToFilter(searchUnlocoSubQuery);
			AddAvailableIdentifiersQuery(searchUnlocoQuery);

			return factory.Load<RefUNLOCO>(searchUnlocoQuery).ToList();
		}

		static List<RefUNLOCO> RetrieveUnlocosByCountryState(BusinessObjectFactory factory, string country, RefCountryStates state)
		{
			var searchUnlocoQuery = new ZQuery(RefUNLOCOSchema.RL_RW, state.PK);
			searchUnlocoQuery.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, country);
			searchUnlocoQuery.AddToFilter(RefUNLOCOSchema.RL_IsActive, true);
			AddAvailableIdentifiersQuery(searchUnlocoQuery);

			return factory.Load<RefUNLOCO>(searchUnlocoQuery).ToList();
		}

		static List<RefUNLOCO> RetrieveUnlocosByCountryCity(BusinessObjectFactory factory, string country, string city)
		{
			var searchUnlocoQuery = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, country);
			searchUnlocoQuery.AddToFilter(RefUNLOCOSchema.RL_IsActive, true);

			var searchUnlocoSubQuery = new ZQuery(RefUNLOCOSchema.RL_PortName, city);
			searchUnlocoSubQuery.AddToFilter(JoinCondition.Or, RefUNLOCOSchema.RL_NameWithDiacriticals, city);
			searchUnlocoQuery.AddToFilter(searchUnlocoSubQuery);
			AddAvailableIdentifiersQuery(searchUnlocoQuery);

			return factory.Load<RefUNLOCO>(searchUnlocoQuery).ToList();
		}

		static RefCountryStates RetrieveState(BusinessObjectFactory factory, string country, string stateCode)
		{
			var searchStateQuery = new ZDBOnlyQuery(typeof(RefCountryStates));
			searchStateQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, country);
			searchStateQuery.AddToFilter(RefCountryStatesSchema.RW_Code, stateCode);
			searchStateQuery.AddToFilter(RefCountryStatesSchema.RW_IsActive, true);
			return factory.LoadTop1<RefCountryStates>(searchStateQuery);
		}

		static void AddAvailableIdentifiersQuery(ZQuery searchRefUNLOCOQuery)
		{
			var identifierQuery = GetAvailableIdentifiersQuery(true);
			if (!string.IsNullOrEmpty(identifierQuery))
			{
				var registryConstrainsQuery = new ZDBOnlyQuery(typeof(RefUNLOCO));
				registryConstrainsQuery.AddFilterAndZSQLParameterCollection(identifierQuery, null);
				searchRefUNLOCOQuery.AddToFilter(registryConstrainsQuery);
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		static void RetrieveClosestPortByGeographyData(ISupportWebAddressValidation addressForValidation, List<RefUNLOCO> availablePorts = null)
		{
			var query = $@"
DECLARE @g geography = geography::STGeomFromText('POINT('+ @Longitude +' '+ @Latitude +')', 4326)

SELECT TOP 1 RL_Code, RL_RN_NKCountryCode
FROM dbo.RefUNLOCO
JOIN dbo.GenSpatialData ON RL_PK = SPD_ParentID
WHERE SPD_Geography.STDistance(@g) IS NOT NULL AND RL_IsActive = 1 AND SPD_ParentTableCode = 'RL' AND SPD_Column = 'RL_GeoLocation'
{GetAvailablePortsQuery(availablePorts)}
{GetAvailableIdentifiersQuery()}
ORDER BY SPD_Geography.STDistance(@g)";

			string port = null;
			string country = null;
			using (var cmd = Db.Connection.Command(query))
			{
				cmd.AddParameter("@Longitude", SqlDbType.VarChar, addressForValidation.GeoLocation.Longitude);
				cmd.AddParameter("@Latitude", SqlDbType.VarChar, addressForValidation.GeoLocation.Latitude);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						port = reader[RefUNLOCOSchema.Constants.RL_Code]?.ToString();
						country = reader[RefUNLOCOSchema.Constants.RL_RN_NKCountryCode]?.ToString();
					}
				}

				if (!string.IsNullOrEmpty(port) && country == addressForValidation.Country.Code)
				{
					addressForValidation.ClosestPort = port;
				}
			}
		}

		static string GetAvailablePortsQuery(List<RefUNLOCO> availablePorts)
		{
			var availablePortsQuery = string.Empty;
			if (availablePorts != null && availablePorts.Any())
			{
				var portCodes = availablePorts.Select(x => x.RL_Code).ToList();

				availablePortsQuery = $"AND RL_Code IN ('{string.Join("' , '", portCodes)}')";
			}

			return availablePortsQuery;
		}

		static string GetAvailableIdentifiersQuery(bool forZQuery = false)
		{
			var queryStr = string.Empty;

			if (!OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.Value)
			{
				var identifiers = OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.Value.Cast<CodeDescriptionBoolDisallowNewWithDefaultDisabled>();
				var applyingIdentifiers = identifiers.Where(x => x.Bool).Select(x => x.Code).ToArray();

				if (applyingIdentifiers.Any())
				{
					var requireAllUNLOCOConditionsToBeMet = OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.Value;
					var builder = new StringBuilder();

					if (!forZQuery)
					{
						builder.AppendLine("AND");
					}

					builder.AppendLine("(");

					foreach (var identifier in applyingIdentifiers)
					{
						builder.AppendLine("\t" + identifier + " = 1");

						if (requireAllUNLOCOConditionsToBeMet)
						{
							builder.AppendLine("\tAND");
						}
						else
						{
							builder.AppendLine("\tOR");
						}
					}

					builder = builder.Remove(builder.ToString().Length - 7, 7);
					builder.AppendLine();
					builder.AppendLine(")");
					queryStr = builder.ToString();
				}
			}

			return queryStr;
		}

		public static void SetClosestPort(ISupportWebAddressValidation addressForValidation)
		{
			var addressForValidationIsOrgAddress = addressForValidation.GetType() == typeof(OrgAddress);
			var addressForValidationIsSubClassOfOrgAddress = addressForValidation.GetType().IsSubclassOf(typeof(OrgAddress));

			if ((addressForValidationIsOrgAddress || addressForValidationIsSubClassOfOrgAddress) &&
				addressForValidation.ClosestPort.IsEmpty &&
				!addressForValidation.GeoLocation.IsEmpty &&
				addressForValidation.GeoLocation.Latitude >= -90 && 90 >= addressForValidation.GeoLocation.Latitude &&
				addressForValidation.Country != null &&
				OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.Value)
			{
				var factory = new BusinessObjectFactory();
				var countryCode = addressForValidation.Country.Code;
				var city = addressForValidation.City;
				var state = string.IsNullOrEmpty(addressForValidation.StateCode) ? null : RetrieveState(factory, addressForValidation.Country.Code, addressForValidation.StateCode);

				List<RefUNLOCO> availablePorts = null;
				if (state != null && !city.IsEmpty)
				{
					availablePorts = RetrieveUnlocosByCountryStateCity(factory, countryCode, state, city);
				}
				else if (state == null && !city.IsEmpty)
				{
					availablePorts = RetrieveUnlocosByCountryCity(factory, countryCode, city);
				}
				else if (state != null && city.IsEmpty)
				{
					availablePorts = RetrieveUnlocosByCountryState(factory, countryCode, state);
				}

				if (availablePorts?.Count == 1)
				{
					addressForValidation.ClosestPort = availablePorts.First().RL_Code;
				}
				else
				{
					RetrieveClosestPortByGeographyData(addressForValidation, availablePorts);
				}
			}
		}

		#endregion

#if DEBUG
		internal
#endif
		static void UpdateSuggestedState(ValidationResultItem suggestedAddress, ISupportWebAddressValidation addressForValidation)
		{
			var country = addressForValidation.Country;

			if (country == null)
			{
				return;
			}

			try
			{
				var suggestedState = string.Empty;

				if (string.IsNullOrEmpty(suggestedAddress.State))
				{
					suggestedState = string.Empty;
				}
				else if (country.States.Cast<RefCountryStates>().Any(s => s.RW_Code == suggestedAddress.State))
				{
					suggestedState = suggestedAddress.State;
				}
				else
				{
					var matchedRefState = country
						.States
						.Cast<RefCountryStates>()
						.Where(state => state.RW_IsActive)
						.FirstOrDefault(state => string.Equals(state.RW_DescriptionMultilingual, suggestedAddress.State, StringComparison.OrdinalIgnoreCase));

					if (matchedRefState != null)
					{
						suggestedState = matchedRefState.RW_Code;
					}
					else
					{
						matchedRefState = country
							.States
							.Cast<RefCountryStates>()
							.Where(state => state.RW_IsActive)
							.FirstOrDefault(state => string.Equals(state.RW_DescriptionMultilingual.GetLocalizedValue(addressForValidation.Language).ToString(), suggestedAddress.State, StringComparison.OrdinalIgnoreCase));

						if (matchedRefState != null)
						{
							suggestedState = matchedRefState.RW_Code;
						}
						else
						{
							var normalizedState = NormalizePunctuations.Normalize(StripDiacritics.RemoveDiacritics(suggestedAddress.State, country.RN_Code));

							matchedRefState = country
								.States
								.Cast<RefCountryStates>()
								.Where(state => state.RW_IsActive)
								.FirstOrDefault(state => string.Equals(NormalizePunctuations.Normalize(StripDiacritics.RemoveDiacritics(state.RW_DescriptionMultilingual, country.RN_Code)), normalizedState, StringComparison.OrdinalIgnoreCase));

							if (matchedRefState != null)
							{
								suggestedState = matchedRefState.RW_Code;
							}
							else if (Res.IsEnglish(addressForValidation.Language))
							{
								suggestedState = normalizedState;
							}
							else
							{
								suggestedState = suggestedAddress.State;
							}
						}
					}
				}

				addressForValidation.State = !string.IsNullOrEmpty(suggestedState) && suggestedState.Length > addressForValidation.State_MaxLength
					? suggestedState.Substring(0, addressForValidation.State_MaxLength)
					: suggestedState;
			}
			catch (NullReferenceException e)
			{
				Globals.Message.ShowDeveloperException("This should be fixed by WI00169625, please contact the ROPE team. The NullReferenceException was caught from UpdateSuggestedState().", e);
			}
		}

		static ZString GetAddressPropertySafe(string suggestedAddressProperty, ZString addressForValidationProperty, int maxLength)
		{
			if (suggestedAddressProperty != null)
			{
				if (suggestedAddressProperty.Length > maxLength)
				{
					addressForValidationProperty = suggestedAddressProperty.Substring(0, maxLength);
				}
				else
				{
					addressForValidationProperty = suggestedAddressProperty;
				}
			}
			else
			{
				addressForValidationProperty = string.Empty;
			}

			return addressForValidationProperty;
		}

		public static bool Check_UnrestrictedAdditionalAddressInformationExceedMaxLength(ISupportWebAddressValidation currentAddress)
		{
			var maxLength = OrgAddressAdditionalInfoSchema.OAI_AdditionalInfo.MaxLength;

			return currentAddress.UnrestrictedAdditionalAddressInformation.Length > maxLength;
		}

		static string ConsolidateAddressAdditionalInformation(string existingAdditionalInformation, string unparsedAddressInformation)
		{
			if (string.IsNullOrEmpty(unparsedAddressInformation))
			{
				return existingAdditionalInformation;
			}

			if (string.IsNullOrEmpty(existingAdditionalInformation))
			{
				return unparsedAddressInformation;
			}

			return existingAdditionalInformation.Contains(unparsedAddressInformation)
				? existingAdditionalInformation
				: string.Join(", ", existingAdditionalInformation, unparsedAddressInformation);
		}

		public static void SetSuggestedCityTownToForm(CandidateCityTown cityTown, ISupportWebAddressValidation address)
		{
			try
			{
				address.IsUpdatingCityTown = true;
				// Do not update values if the same to avoid firing unneccessary change events which could fire calls to the validation web service
				if (cityTown != null && cityTown.City != null && cityTown.Postcode != null && cityTown.State != null)
				{
					if (cityTown.City.ToUpper(CultureInfo.InvariantCulture) != address.City)
					{
						address.City = cityTown.City;
					}

					if (cityTown.Postcode.ToUpper(CultureInfo.InvariantCulture) != address.Postcode)
					{
						address.Postcode = cityTown.Postcode.ToUpper(CultureInfo.InvariantCulture);
					}

					if (cityTown.State.ToUpper(CultureInfo.InvariantCulture) != address.StateCode && cityTown.State.ToUpper(CultureInfo.InvariantCulture) != address.State)
					{
						if (address.Country != null && address.Country.IsStateMustNotBeEntered)
						{
							address.State = String.Empty;
						}
						else
						{
							address.State = cityTown.State.ToUpper(CultureInfo.InvariantCulture);
						}
					}
				}
			}
			finally
			{
				address.IsUpdatingCityTown = false;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Required format")]
		internal static string GetQueryStringForValidationService(string endpointUrlAddress, string languageCode, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
		{
			// URI parameters
			var systemCode = new EnterpriseInformationRetriever().LicenceCode.Replace(" - ", "");
			var user = Uri.EscapeDataString(Env.CurrentUser.FullName);
			var sessionStartDateTimeFormatted = string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd hh:mm:ss tt}", DateTime.UtcNow); // Required format
			var sessionStartDateTime = Uri.EscapeDataString(sessionStartDateTimeFormatted);
			var sessionGUID = Uri.EscapeDataString(Guid.NewGuid().ToString());
			var action = (int)cleanseAction;

			// Base URI
			var uriRequest = string.Format(CultureInfo.InvariantCulture, "{0}{1}", AddSlashSuffixIfNecessary(endpointUrlAddress), Constants.ValidationServiceName);
			// URI with parameters
			return string.Format(CultureInfo.InvariantCulture, "{0}?systemCode={1}&userName={2}&sessionID={4}&sessionStartDateTimeUTC={3}&action={5}&suggestionRequiredThreshold={6}&suggestionQualityThreshold={7}&languageCode={8}",
				uriRequest, systemCode, user, sessionStartDateTime, sessionGUID, action, ValidationResultStatusValues.BestAvailableClose.Code, ValidationResultStatusValues.BestAvailableExact.Code, GetTranslatedLanguageCode(languageCode));
		}

		static string AddSlashSuffixIfNecessary(string uri)
		{
			if (!string.IsNullOrEmpty(uri) && !uri.EndsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				uri += "/";
			}
			return uri;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static AddressValidationServiceUri GetAvailableWebServiceAddress()
		{
			try
			{
				var serviceUrisFromProvider = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris();
				var serviceUris = new List<AddressValidationServiceUri>();
				if (!string.IsNullOrEmpty(serviceUrisFromProvider.Primary?.Uri))
				{
					serviceUris.Add(serviceUrisFromProvider.Primary);
				}

				if (!string.IsNullOrEmpty(serviceUrisFromProvider.Secondary?.Uri))
				{
					serviceUris.Add(serviceUrisFromProvider.Secondary);
				}

				if (serviceUris.Count == 0)
				{
					return new AddressValidationServiceUri();
				}

				using var client = new HttpClient();
				// Perform the GET request (the serialization is automatic)
				client.Timeout = TimeSpan.FromSeconds(Env.Instance.Registry.AddressValidationWebServiceTimeout);
				foreach (var serviceUri in serviceUris)
				{
					var uriFull = serviceUri.Uri + Constants.CheckServiceStatusName;
					try
					{
						WriteMessage(ConstructRequestMessage(uriFull));
						var response = client.GetAsync(uriFull).Result;
						if (response.IsSuccessStatusCode)
						{
							WriteMessage(ConstructValidationResponseMessage(response.Content.ReadAsStringAsync().Result));
							var serviceStatus = response.Content.ReadAsAsync<ServiceStatus>().Result;
							if (!serviceStatus.ServiceAvailable)
							{
								continue;
							}

							return serviceUri;
						}

						var requestDetails = string.Join(System.Environment.NewLine, response.RequestMessage.ToString().Split(','));
						var errorMessage = string.Format(CultureInfo.CurrentCulture, "REQUEST:\n{0}\n\nERROR:\n{1}", requestDetails, response.ReasonPhrase);
						WriteMessage(ConstructErrorMessage(errorMessage));
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var exceptionMessage = string.Format(CultureInfo.CurrentCulture, "Exception thrown while accessing Address Validation service {0}\r\n{1}", uriFull, ex is AggregateException && ex.InnerException != null ? ex.InnerException.Message : ex.Message);
						WriteMessage(ConstructErrorMessage(exceptionMessage));
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var exceptionMessage = string.Format(CultureInfo.CurrentCulture, "Exception thrown while trying to get Address Validation WebService urls from registry item. \r\n{0}", ex.Message);
				WriteMessage(ConstructErrorMessage(exceptionMessage));
			}

			return new AddressValidationServiceUri();
		}

		public static bool IsServiceAvailable(ILogger serviceLogger)
		{
			using var client = GetHttpClient(AvailableBackgroundWebServiceAddress.EnableS2STAuth, false);
			var result = false;
			client.Timeout = TimeSpan.FromSeconds(Env.Instance.Registry.AddressValidationWebServiceTimeout);
			var baseUri = new Uri(AvailableBackgroundWebServiceAddress.Uri);
			var uriFull = new Uri(baseUri, Constants.CheckServiceStatusName + "/").AbsoluteUri;
			try
			{
				var response = client.GetAsync(uriFull).Result;
				if (response.IsSuccessStatusCode)
				{
					var serviceStatus = response.Content.ReadAsAsync<ServiceStatus>().Result;
					result = serviceStatus.ServiceAvailable;
				}
			}
			catch (Exception ex)
			{
				var exceptionChain = ex.FlattenInnerExceptions();
				var lastOrDefaultException = exceptionChain.LastOrDefault();
				if (lastOrDefaultException is WebException || lastOrDefaultException is SocketException || lastOrDefaultException is TaskCanceledException)
				{
					var messageBuilder = new StringBuilder();
					var exceptionLevel = 0;
					exceptionChain.ForEach(o =>
					{
						if (exceptionLevel == 0)
						{
							messageBuilder.Append(ConstructErrorMessage(ex.Message));
							messageBuilder.AppendLine($"At {nameof(AddressValidationService)}.{nameof(IsServiceAvailable)}");
						}
						else
						{
							messageBuilder.AppendLine(
								FormattableString.Invariant($"Inner Exception{exceptionLevel}(Type: {o.GetType()}):"));
							messageBuilder.AppendLine(o.Message);
						}

						exceptionLevel++;
					});

					var errorMessage = messageBuilder.ToString();
					WriteMessage(errorMessage);
					serviceLogger?.Log(LogType.Debug, errorMessage);
				}
				else
				{
					throw;
				}
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static async Task<CandidateCityTown[]> GetCityTownAsync(ISupportWebAddressValidation address, CancellationTokenSource cancellationToken)
		{
			var serviceUri = await GetAvailableWebServiceAddressAsync();

			if (address == null || address.IsRowDeletedOrDetachedOrNull)
			{
				return Array.Empty<CandidateCityTown>();
			}

			if (!string.IsNullOrEmpty(serviceUri.Uri))
			{
				var result = new List<CandidateCityTown>();
				var uriRequest = serviceUri.Uri + Constants.GetCityTown;
				var uriFull = string.Format(
					CultureInfo.InvariantCulture,
					"{0}?city={1}&state={2}&postCode={3}&countryCode={4}&languageCode={5}",
					uriRequest,
					address.City,
					address.State,
					address.Postcode,
					address.CountryCodeISO2,
					GetTranslatedLanguageCode(address.Language));

				WriteMessage(ConstructRequestMessage(uriFull));

				using var client = GetHttpClient(serviceUri.EnableS2STAuth, true);
				try
				{
					var response = await client.GetAsync(uriFull, cancellationToken.Token); //.Result;
					if (response.IsSuccessStatusCode)
					{
						result = response.Content.ReadAsAsync<List<CandidateCityTown>>().Result;

						var responseData = response.Content.ReadAsStringAsync().Result;
						ProcessGetCityTownResponse(address, response, result, responseData);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is TaskCanceledException && cancellationToken.IsCancellationRequested)
					{
						WriteMessage(ConstructCancellationMessage());
					}
					else
					{
						var exceptionMessage = string.Format(CultureInfo.CurrentCulture, "Exception thrown while accessing GetCityTown service {0}\r\n{1}", uriFull, ex is AggregateException && ex.InnerException != null ? ex.InnerException.Message : ex.Message);
						WriteMessage(ConstructErrorMessage(exceptionMessage));
					}
				}

				return result.ToArray();
			}

			return null;
		}

		internal static void ProcessGetCityTownResponse(ISupportWebAddressValidation address, HttpResponseMessage response, List<CandidateCityTown> result, string responseData)
		{
			if (response.IsSuccessStatusCode)
			{
				if (result != null && !address.IsRowDeletedOrDetachedOrNull)
				{
					// When importing, we often don't have a language, so in that case, assume English and transliterate
					if (string.IsNullOrEmpty(address.Language) || address.Language.StartsWith(Core.SharedConstants.Languages.English, StringComparison.OrdinalIgnoreCase))
					{
						AddressCleansingResultItemExtensions.TransliterateCityTownsToEnglish(result);
					}
					AddressCleansingResultItemExtensions.GetApplicableCityCasing(result, address, Env.Registry.OrgAllowMixedCase);
				}
				WriteMessage(ConstructValidationResponseMessage(responseData));
			}
		}

		internal static string GetTranslatedLanguageCode(string languageCode)
		{
			string translatedLanguageCode;

			switch (languageCode)
			{
				case Core.SharedConstants.Languages.English:
					translatedLanguageCode = "ENG";
					break;
				case Core.SharedConstants.Languages.EnglishAmerican:
					translatedLanguageCode = "EUS";
					break;
				case Core.SharedConstants.Languages.EnglishBritish:
					translatedLanguageCode = "EGB";
					break;
				default:
					translatedLanguageCode = languageCode ?? string.Empty;
					break;
			}

			return translatedLanguageCode;
		}

		internal static void SetCoordinatesAndValidationStatus(ISupportWebAddressValidation address, ValidationResultItem result)
		{
			SetCoordinatesAndValidationStatusCore(address, result);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Switch use is justifiable")]
		static void SetCoordinatesAndValidationStatusCore(ISupportWebAddressValidation address, ValidationResultItem result)
		{
			address.GeoLocation = ZGeography.CreatePoint(result.Longitude, result.Latitude);

			if (result.QueryType == QueryType.AsEntered || result.QueryType == QueryType.AllCombined)
			{
				switch (result.ResultStatusCode)
				{
					case ValidationResultStatusCode.PointExact:
						SetSuggestedAddressToAddressForValidation(
							result,
							false,
							address,
							OrganisationsDataRegistry.Instance.ShouldPreventUpdateWhenGettingPointExact);
						break;
					case ValidationResultStatusCode.PrivateAddressValid:
						address.ValidationStatus = AddressValidationStatus.Verified;
						SetAddressMap(address, result);
						break;
					case ValidationResultStatusCode.StreetExact:
						if (result.AvailableData == AvailableData.Street)
						{
							address.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
							SetAddressMap(address, result);
						}
						else
						{
							address.ValidationStatus = AddressValidationStatus.Invalid;
						}
						break;
					case ValidationResultStatusCode.Error:
						address.ValidationStatus = AddressValidationStatus.ToBeVerified;
						break;
					case ValidationResultStatusCode.PrivateAddressNotChecked:
						address.ValidationStatus = AddressValidationStatus.Invalid;
						break;
					case ValidationResultStatusCode.CountryDataNotAvailable:
						address.ValidationStatus = AddressValidationStatus.CountryNotAvailable;
						break;
					case ValidationResultStatusCode.Invalid:
					case ValidationResultStatusCode.PrivateAddressInvalid:
					case ValidationResultStatusCode.PointClose:
					case ValidationResultStatusCode.StreetClose:
					case ValidationResultStatusCode.CityClose:
					case ValidationResultStatusCode.CityExact:
					default:
						address.ValidationStatus = AddressValidationStatus.Invalid;
						break;
				}
			}
			else
			{
				if (result.AvailableData == AvailableData.NotAvailable)
				{
					address.ValidationStatus = AddressValidationStatus.CountryNotAvailable;
				}
				else
				{
					address.ValidationStatus = AddressValidationStatus.Invalid;
				}
			}
		}

		const string UnmatchedCode = "US";
		const string ApartmentCode = "A";
		const string StreetNumberCode = "SN";
		const string StreetCode = "S";

		static void SetAddressMap(ISupportWebAddressValidation address, ValidationResultItem result)
		{
			var unmatched = SetAddressMapForElement(UnmatchedCode, result.UnmatchedApartmentPrefix, address);
			var apartment = SetAddressMapForElement(ApartmentCode, result.Apartment, address);
			var streetNumber = SetAddressMapForElement(StreetNumberCode, result.StreetNumber, address);
			var streetName = SetAddressMapForElement(StreetCode, result.Street, address);

			address.AddressMap = string.Join("", unmatched, apartment, streetNumber, streetName).Trim();
		}

		static string SetAddressMapForElement(string code, string value, ISupportWebAddressValidation address)
		{
			var result = string.Empty;
			if (!string.IsNullOrEmpty(value))
			{
				if (address.Address1.Contains(value, StringComparison.OrdinalIgnoreCase))
				{
					int index = address.Address1.IndexOf(value, StringComparison.OrdinalIgnoreCase);
					result = string.Format(CultureInfo.InvariantCulture, "{0}A1[{1}-{2}]", code, index, index + value.Length - 1);
				}
				else if (address.Address2.Contains(value, StringComparison.OrdinalIgnoreCase))
				{
					int index = address.Address2.IndexOf(value, StringComparison.OrdinalIgnoreCase);
					result = string.Format(CultureInfo.InvariantCulture, "{0}A2[{1}-{2}]", code, index, index + value.Length - 1);
				}
			}
			return result;
		}

		public static string GetApartment(ISupportWebAddressValidation address)
		{
			return GetAddressMapElement(address, ApartmentCode);
		}

		public static string GetStreetNumber(ISupportWebAddressValidation address)
		{
			return GetAddressMapElement(address, StreetNumberCode);
		}

		public static string GetStreet(ISupportWebAddressValidation address)
		{
			return GetAddressMapElement(address, StreetCode);
		}

		static string GetAddressMapElement(ISupportWebAddressValidation address, string code)
		{
			if (!string.IsNullOrEmpty(address.AddressMap))
			{
				Regex regex = new Regex(code + @"A(?<line>[1-2])\[(?<position>\d+-\d+)]");
				Match match = regex.Match(address.AddressMap);

				if (match.Success)
				{
					string addressLine = match.Groups["line"].Value == "1" ? address.Address1 : address.Address2;
					var positions = Array.ConvertAll(match.Groups["position"].Value.Split('-'), p => Convert.ToInt32(p, CultureInfo.InvariantCulture));
					if (positions.Length == 2 && positions[0] >= 0 && positions[0] < addressLine.Length && positions[0] <= positions[1] && positions[1] < addressLine.Length)
					{
						return addressLine.Substring(positions[0], positions[1] - positions[0] + 1);
					}
				}
			}

			return string.Empty;
		}
		#endregion

		#region GetHttpClient

		static HttpClient GetHttpClient(bool enableSysToSysTrust, bool needAuthorization)
		{
			var httpClientFactory = ObjectFactory.Get<IHttpClientFactory>();
			var httpClient = httpClientFactory
				.CreateNew(
					new HttpClientHandlerWithDiagnostics(new CookieContainer()),
					TimeSpan.FromSeconds(Env.Instance.Registry.AddressValidationWebServiceTimeout));

			if (needAuthorization)
			{
				var helper = new AddressValidationServiceHelper();
				httpClient.DefaultRequestHeaders.Authorization = helper.GetAuthenticationHeaderValue(enableSysToSysTrust);
			}

			return httpClient;
		}

		#endregion
	}
}

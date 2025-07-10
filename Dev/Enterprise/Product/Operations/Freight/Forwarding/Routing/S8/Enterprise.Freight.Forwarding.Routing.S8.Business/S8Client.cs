using System;
using System.Net;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Testing;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	class S8Client : IS8Client
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only")]
		public const string SignInInvalidOrExpired = "Sign-in invalid or expired";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only")]
		public const string LoginMutexExceptionMessage = "Unable to retrieve Login Mutex.";
		public const int MaximumLoginMutexRetryAttempts = 20;

		protected delegate string ServiceCall(IFlightScheduleClient client, string token);
		protected delegate MethodCallResult<T> Parse<T>(string response);

		string WebServiceSessionToken
		{
			get
			{
				return FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.Value.Equals(FreightDataRegistry.Instance.S8CargoWebServiceURL_Primary.Value, StringComparison.OrdinalIgnoreCase)
					? FreightDataRegistry.Instance.S8CargoWebServiceSessionToken.Value
					: FreightDataRegistry.Instance.S8CargoWebServiceSessionFailOverToken.Value;
			}
			set
			{
				if (FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.Value.Equals(FreightDataRegistry.Instance.S8CargoWebServiceURL_Primary.Value, StringComparison.OrdinalIgnoreCase))
				{
					FreightDataRegistry.Instance.S8CargoWebServiceSessionToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					FreightDataRegistry.Instance.S8CargoWebServiceSessionFailOverToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		ZGlobalMutex LoginMutex => loginMutex ?? (loginMutex = new ZGlobalMutex(MutexIDs.S8CargoLoginTokenRetrieval));

		ZGlobalMutex loginMutex;

		int loginMutexTryAttempts;

		bool switchedToDifferentServer;

		FlightScheduleClient flightScheduleClient;
		readonly Action<string> updateStatus;
		readonly IServiceRequestManager serviceRequestManager;

		public S8Client(Action<string> updateStatus = null, IServiceRequestManager serviceRequestManager = null)
		{
			this.updateStatus = updateStatus;
			this.serviceRequestManager = serviceRequestManager ?? new S8ServiceRequestManager();

			var primaryWebServiceURL = FreightDataRegistry.Instance.S8CargoWebServiceURL_Primary.Value;
			var failoverWebServiceURL = FreightDataRegistry.Instance.S8CargoWebServiceURL_Faillover.Value;
			var current = FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.Value;

			if (string.IsNullOrEmpty(current) ||
				(!current.Equals(primaryWebServiceURL, StringComparison.OrdinalIgnoreCase) &&
				 !current.Equals(failoverWebServiceURL, StringComparison.OrdinalIgnoreCase)))
			{
				FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, primaryWebServiceURL);
			}

			flightScheduleClient = new FlightScheduleClient(new Uri(FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.Value));

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public void Dispose()
		{
			flightScheduleClient.Dispose();
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		public MethodCallResult<FlightInformation> GetFlight(FlightRequest request)
		{
			string serviceCall(IFlightScheduleClient client, string token) => client.GetFlight(token, "SSIM", request.Date, request.Airline, request.FlightNumber);
			return RequestAndParse(serviceCall, FlightInformation.Parse, serviceRequestManager, true);
		}

		public MethodCallResult<string> SolveRouting(RoutingRequest request)
		{
			Argument.NotNull(request, nameof(request));

			var requestString = request.GenerateRequestString();
			string serviceCall(IFlightScheduleClient client, string token) => client.SolveRouting(token, requestString);

			MethodCallResult<string> Parse(string response) => ParseRoutingResponse(response, request);

			var result = RequestAndParse(serviceCall, Parse, serviceRequestManager, false);
			return result;
		}

		#region Implementation

		MethodCallResult<string> ParseRoutingResponse(string response, RoutingRequest request)
		{
			var parseErrorHandler = new ServerParseErrorHandler(response, request);
			if (parseErrorHandler.ResponseContainsParseError)
			{
				return new MethodCallResult<string>(parseErrorHandler.Message, shouldBeReported: parseErrorHandler.ShouldReportError);
			}

			return new MethodCallResult<string>(response, string.Empty);
		}

		protected MethodCallResult<T> RequestAndParse<T>(ServiceCall serviceCall, Parse<T> parse, IServiceRequestManager requestManager, bool useSuppression)
		{
			try
			{
				if (useSuppression && requestManager.IsSuppressed())
				{
					return new MethodCallResult<T>(default, (NoResString)"Login is suppressed"); // Developer Only
				}

				var response = parse(Request(serviceCall));

				requestManager.OnSuccessfulRequest();
				return response;
			}
			catch (OperationCanceledException ex)
			{
				return new MethodCallResult<T>(ex.Message, shouldBeReported: false) { WasOperationCancelledByUser = true };
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				requestManager.HandleException(ex);

				if (ShouldBeReported(ex))
				{
					return new MethodCallResult<T>(ex);
				}
				else
				{
					return new MethodCallResult<T>(
						Res.GetString("4d059925-03f2-4ab9-b384-fd4a893ac6fe", @"Please try again. If the issue persists, please raise an eRequest. Issue details: {0}", ex.Message), shouldBeReported: false);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Developer Only")]
		bool ShouldBeReported(Exception ex)
		{
			if (ex is WebException || (ex is S8ClientException s8Exception && s8Exception.Message == LoginMutexExceptionMessage)) // Developer Only
			{
				return false;
			}

			return true;
		}

		string Request(ServiceCall serviceCall)
		{
			var response = new S8Response(GetResponse(serviceCall));

			if (response.Status == ResponseStatus.SignInInvalidOrExpiredExpiredError)
			{
				InitialiseAndLogin();
				return Request(serviceCall);
			}
			else if (!response.IsValid)
			{
				InitialiseAndLogin();
				response = new S8Response(GetResponse(serviceCall));

				if (!response.IsValid)
				{
					if (updateStatus != null)
					{
						updateStatus(response.Message);
						return response.Message;
					}
					else
					{
						throw new S8ClientException(response.Message);
					}
				}
			}

			return response.Content;
		}

		#region Login

		protected virtual void InitialiseAndLogin()
		{
			loginMutexTryAttempts = 0;
			if (LoginMutex.Lock())
			{
				try
				{
					var sessionToken = Login();
					var response = new S8Response(sessionToken);
					if (response.IsValid)
					{
						WebServiceSessionToken = sessionToken.Substring(0, 8);
					}
					else
					{
						throw new S8ClientException(response.Message);
					}
				}
				finally
				{
					if (LoginMutex.HasLock)
					{
						LoginMutex.Unlock();
					}
				}
			}
			else
			{
				while (LoginMutex.IsLocked)
				{
					if (loginMutexTryAttempts < MaximumLoginMutexRetryAttempts) // 10 seconds
					{
						loginMutexTryAttempts++;
						Thread.Sleep(500);
					}
					else
					{
						throw new S8ClientException(LoginMutexExceptionMessage);
					}
				}

				return;
			}
		}

		string Login()
		{
			if (updateStatus != null)
			{
				var status = Res.GetString("03f2a2f3-c006-4795-aa4f-5f838f014949",
					"Getting routes from {0} web service...",
					FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.Value == FreightDataRegistry.Instance.S8CargoWebServiceURL_Primary.Value ? (NoResString)"primary" : (NoResString)"fail-over");

				updateStatus(status);
			}

			string result;
			try
			{
				result = LoginServiceCall();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (switchedToDifferentServer)
				{
					throw;
				}
				SwitchWebServiceURLIfNecessary();
				result = Login();
			}

			return result;
		}

		protected virtual string LoginServiceCall()
		{
			return flightScheduleClient.LogInS8C(
					 FreightDataRegistry.Instance.S8CargoWebServiceUserID.Value,
					 FreightDataRegistry.Instance.S8CargoWebServicePassword.Value,
					 string.Empty,
					 EnvProxy.Instance.CurrentCompany.GetLicenceCode("-"),
					 Constants.ProductName,
					 string.Empty);
		}

		#endregion

		#region GetResponse

		string GetResponse(ServiceCall serviceCall)
		{
			ResetErrorState();

			if (string.IsNullOrEmpty(WebServiceSessionToken))
			{
				InitialiseAndLogin();
			}
			return GetResponseCore(serviceCall);
		}

		string GetResponseCore(ServiceCall serviceCall)
		{
			string result;

			try
			{
				result = serviceCall(flightScheduleClient, WebServiceSessionToken);
				ResetErrorState();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (switchedToDifferentServer)
				{
					throw;
				}

				SwitchWebServiceURLIfNecessary();

				if (string.IsNullOrEmpty(WebServiceSessionToken))
				{
					InitialiseAndLogin();
				}

				result = GetResponseCore(serviceCall);
			}

			return result;
		}

		#endregion

		#region Helper methods

		void ResetErrorState()
		{
			switchedToDifferentServer = false;
		}

		void SwitchWebServiceURLIfNecessary()
		{
			if (!switchedToDifferentServer)
			{
				var primaryWebServiceURL = FreightDataRegistry.Instance.S8CargoWebServiceURL_Primary.Value;
				var failoverWebServiceURL = FreightDataRegistry.Instance.S8CargoWebServiceURL_Faillover.Value;
				var currentURL = FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.Value;

				WebServiceSessionToken = string.Empty;
				FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentURL == primaryWebServiceURL ? failoverWebServiceURL : primaryWebServiceURL);
				switchedToDifferentServer = true;
				flightScheduleClient = new FlightScheduleClient(new Uri(FreightDataRegistry.Instance.S8CargoWebServiceURL_Current.Value));
			}
		}

		#endregion

		public enum ResponseStatus
		{
			SignInInvalidOrExpiredExpiredError,
			Error,
			Successful
		}

		internal class S8Response
		{
			public S8Response(string content)
			{
				Content = content;
				Status = GetStatus(content);
			}

			ResponseStatus GetStatus(string content)
			{
				if (Content == null)
				{
					return ResponseStatus.Error;
				}
				if (content.StartsWith("#", StringComparison.Ordinal))
				{
					if (content.Contains(SignInInvalidOrExpired))
					{
						return ResponseStatus.SignInInvalidOrExpiredExpiredError;
					}
					return ResponseStatus.Error;
				}
				return ResponseStatus.Successful;
			}

			public ResponseStatus Status { get; private set; }

			public bool IsValid => Status == ResponseStatus.Successful;

			public string Content { get; private set; }

			public string Message => Content?.Replace("#", "") ?? string.Empty;
		}

		#endregion
	}
}

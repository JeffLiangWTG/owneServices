using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterData.ServiceTask.AddressValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	BackgroundAddressValidationServiceTask.Code,
	BackgroundAddressValidationServiceTask.FriendlyName,
	"SYS",
	typeof(BackgroundAddressValidationServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	BackgroundAddressValidationServiceTask.Code,
	OrgAddressSchema.Constants.TableName,
	new[]
	{
		OrgAddressSchema.Constants.OA_IsActive + "=Y",
		OrgAddressSchema.Constants.OA_ValidationStatus + "=" + AddressValidationStatus.ToBeVerified
	},
	BackgroundAddressValidationServiceTask.FriendlyName + " - " + OrgAddressSchema.Constants.TableName)]

[assembly: HostedServiceBusinessObjectBinding(
	BackgroundAddressValidationServiceTask.Code,
	JobDocAddressSchema.Constants.TableName,
	new[]
	{
		JobDocAddressSchema.Constants.E2_ValidationStatus + "=" + AddressValidationStatus.ToBeVerified
	},
	BackgroundAddressValidationServiceTask.FriendlyName + " - " + JobDocAddressSchema.Constants.TableName)]

namespace Enterprise.MasterData.ServiceTask.AddressValidation
{
	class BackgroundAddressValidationServiceTask : ServiceProviderImpl
	{
		readonly INotificationHandler notifier = new IdleNotificationHandler();

		ZQuery orgAddressQuery;
		ZQuery jobDocAddressQuery;
		const int TotalTryCount = 3;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate")]
		const string ErrorKey = "Error occurred when processing background address validation. If this warning persists please raise a CR4 incident and our support team will assist.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate")]
		const string WarningKey = "Address Validation services are unavailable at the moment, please try again later. If the issue persists please raise a Customer Service Incident.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate")]
		const string AuthenticationWarningKey = "Authorization failed. If the issue persists, please raise a Customer Service Incident.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate")]
		const string AddressInformationLogString = @"Address Type:{0}
PK: {1}
Line 1: {2}
Line 2: {3}
City: {4}
State: {5}
Postcode: {6}
Country: {7}
New Status: {8}
";
		public const string Code = "BAV";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate")]
		public const string FriendlyName = "Background Address Validation Service";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to translate")]
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			using (Environment.DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var factoryProvider = new BusinessObjectFactoryProvider();
				var addressValidationServiceTokenSource = CancellationTokenSource.CreateLinkedTokenSource(youMustReactToThisToken);
				var maximumRows = OrganisationsDataRegistry.Instance.BackgroundValidationServiceTaskBatchSize.Value;

				try
				{
					var shouldContinue = true;
					while (shouldContinue)
					{
						youMustReactToThisToken.ThrowIfCancellationRequested();
						var addresses = GetUnvalidatedAddresses(factoryProvider.Current, maximumRows);
						var hasAddressesAndServiceAvailable = addresses.Length > 0 && AddressValidationService.IsServiceAvailable(ServiceLogger);
						if (hasAddressesAndServiceAvailable)
						{
							var tableName = addresses[0].IsJobDocAddress ? JobDocAddressSchema.Constants.TableName : OrgAddressSchema.Constants.TableName;
							if (addresses.Length > 0)
							{
								ServiceLogger?.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Background Address Validation batch has started, found {0} {1}(es).", addresses.Length, tableName));
							}

							foreach (var address in addresses)
							{
								youMustReactToThisToken.ThrowIfCancellationRequested();

								var serviceResult = ValidateAddressWithRetry(address, addressValidationServiceTokenSource);
								if (serviceResult.ExceptionToReport != null)
								{
									ServiceLogger?.Log(LogType.Warning, serviceResult.ExceptionToReport is AuthenticationException ? AuthenticationWarningKey : WarningKey);
									ServiceLogger?.Log(LogType.Debug, $"{nameof(BackgroundAddressValidationServiceTask)}.{nameof(RunTask)}", serviceResult.ExceptionToReport);

									shouldContinue = false;
									break;
								}

								if (serviceResult.StatusCode != HttpStatusCode.OK)
								{
									if (serviceResult.StatusCode == HttpStatusCode.BadRequest)
									{
										address.ValidationStatus = AddressValidationStatus.Invalid;
										ServiceLogger?.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, serviceResult.Message));
									}
									else
									{
										var message = WarningKey;
										if (serviceResult.StatusCode != null)
										{
											message += System.Environment.NewLine + string.Format(CultureInfo.InvariantCulture, "The remote server returned an error: ({0})", (int)serviceResult.StatusCode);
										}
										else if (!string.IsNullOrEmpty(serviceResult.Message))
										{
											message = serviceResult.Message;
										}

										ServiceLogger?.Log(LogType.Warning, message);
										shouldContinue = false;
										break;
									}
								}

								if (address.ValidationStatus == AddressValidationStatus.ToBeVerified)
								{
									address.IsValidatedByBackgroundService = false;
									address.ValidationStatus = AddressValidationStatus.Unverifiable;
									ServiceLogger?.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "This address is unverifiable."));
								}
								else if (address.ValidationStatus == AddressValidationStatus.Invalid || address.ValidationStatus == AddressValidationStatus.Verified)
								{
									address.IsValidatedByBackgroundService = true;
								}

								ServiceLogger?.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, AddressInformationLogString, tableName, address.EntityPK, address.Address1, address.Address2, address.City, address.State, address.Postcode, address.CountryCodeISO2, address.ValidationStatus));
							}

							using (ProcessTask.Loader.SuppressTemplateApplication())
							{
								factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
							}
						}
						else
						{
							if (addresses.Length > 0)
							{
								ServiceLogger?.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, WarningKey));
							}
							break;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException() && !(ex is OperationCanceledException))
				{
					if (ex is ZSaveConcurrencyException)
					{
						// The reason we put the handling here is because we don't want BAV stuck in a loop when merging failed
						try
						{
							ZExceptionReporting.ProcessWithSaveExceptionHandling(factoryProvider.Current.Save, null, notifier: notifier);
						}
						catch (ZSaveConcurrencyException)
						{
							//e.g. can't merge due to Strict field like WD_WP. just log and let it be tried again later
							ServiceLogger?.Log(LogType.Warning, ErrorKey, ex);
						}
					}
					else if (ex is ZCannotSaveException || ex is SqlException)
					{
						ServiceLogger?.Log(LogType.Warning, ErrorKey, ex);
					}
					else
					{
						ErrorReporter.ReportOnce(ErrorKey, ex);
					}
				}
				finally
				{
					addressValidationServiceTokenSource.Dispose();
				}
			}
		}

		static WebAddressValidationResult ValidateAddressWithRetry(ISupportWebAddressValidation address, CancellationTokenSource addressValidationServiceTokenSource)
		{
			var tryCount = 0;

			while (true)
			{
				addressValidationServiceTokenSource.Token.ThrowIfCancellationRequested();

				tryCount++;
				var result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, addressValidationServiceTokenSource);
				if (result.ExceptionToReport == null && result.StatusCode == HttpStatusCode.OK || tryCount == TotalTryCount)
				{
					return result;
				}
			}
		}

		ISupportWebAddressValidation[] GetUnvalidatedAddresses(BusinessObjectFactory factory, int maximumRows)
		{
			if (orgAddressQuery == null)
			{
				orgAddressQuery = new ZQuery { MaximumRows = maximumRows };
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_ValidationStatus, AddressValidationStatus.ToBeVerified);
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_IsActive, true);
			}

			var orgAddresses = factory.Load<OrgAddress>(orgAddressQuery);

			if (orgAddresses.Length > 0)
			{
				return orgAddresses.ToArray<ISupportWebAddressValidation>();
			}

			if (jobDocAddressQuery == null)
			{
				jobDocAddressQuery = new ZQuery { MaximumRows = maximumRows };
				jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ValidationStatus, AddressValidationStatus.ToBeVerified);
			}

			return factory.Load<JobDocAddress>(jobDocAddressQuery).ToArray<ISupportWebAddressValidation>();
		}

		class IdleNotificationHandler : INotificationHandler
		{
			public void ReportInformation(string message, string caption)
			{
			}

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.Foundation.Http;
using WTG.StaticAnalysis.Annotation;
using IHttpClientFactory = WTG.Foundation.Http.IHttpClientFactory;

[assembly: HostedService(
	BorderWiseBatchTariffClassificationServiceTask.Code,
	BorderWiseBatchTariffClassificationServiceTask.Description,
	BorderWiseBatchTariffClassificationServiceTask.Category,
	typeof(BorderWiseBatchTariffClassificationServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "5Minutes",
	DefaultScheduleRunEvery = "5Minutes",
	ActiveByDefault = true
	)]
namespace Enterprise.Customs.ServiceTasks
{
	public class BorderWiseBatchTariffClassificationServiceTask : ServiceProviderImpl
	{
		internal const string Code = "BBT";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "service task")]
		internal const string Description = "BorderWise Batch Tariff Classification";
		internal const string Category = "CUS";

		bool IsEnabled => IsBorderWiseMultiLineClassificationEnabled && (IsProductionSystem() || ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassificationServiceTask);

		public HttpMessageHandler HttpMessageHandler { get; set; }

		public override void RunTask(CancellationToken cancellationToken)
		{
			if (IsEnabled)
			{
				RetrieveAndProcessTariffClassificationFromBorderWise();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		bool RetrieveAndProcessTariffClassificationFromBorderWise()
		{
			var result = false;
			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var companyCode = EnvProxy.Instance.CurrentCompany.Code;
			var cw1ClientId = $"{productRegistrationKey.EnterpriseCode} - {companyCode} - {productRegistrationKey.ServerCode}";
			var borderWiseAddress = ZArchitecture.Environment.DataRegistry.Instance.BorderWiseWebAddress.TrimEnd('/');

			using (var client = HttpMessageHandler == null ? GetHttpClient() : new HttpClient(HttpMessageHandler))
			{
				try
				{
					ServiceLogger.Log(LogType.Information, "Started retrieving completed tariff classification jobs from BorderWise");

					var requestUriToGetJobs = new Uri($"{borderWiseAddress}{RelativeUriToGetJobs}/{cw1ClientId}");
					var task = client.GetAsync(requestUriToGetJobs, CancellationToken.None);

					if (!task.Result.IsSuccessStatusCode)
					{
						LogHttpErrorResponse(task.Result, "Failed to retrieve tariff classification jobs from BorderWise.");
						return false;
					}

					var jobs = DeserializeFromJson<List<BorderWiseTariffExchangeModelV2>>(task.Result.Content.ReadAsStringAsync().Result);
					ServiceLogger.Log(LogType.Information, $"Successfully retrieved {jobs.Count} completed jobs from BorderWise");

					foreach (var job in jobs)
					{
						ServiceLogger.Log(LogType.Information, $"Retrieving job details from BorderWise for JobNumber: {job.JobNumber}, JobPK: {job.JobPk}");
						var requestUriToGetJobDetails = new Uri($"{borderWiseAddress}{RelativeUriToGetJobDetails}/{job.JobPk}");
						task = client.GetAsync(requestUriToGetJobDetails, CancellationToken.None);

						if (!task.Result.IsSuccessStatusCode)
						{
							LogHttpErrorResponse(task.Result, $"Failed to retrieve job details for JobPK: {job.JobPk} from BorderWise.");
						}
						else
						{
							var jobDetail = DeserializeFromJson<BorderWiseTariffExchangeModelV2>(task.Result.Content.ReadAsStringAsync().Result);

							if (jobDetail?.BorderWiseInvoiceLines.Count > 0)
							{
								var requestUriToUpdateJobStatus = new Uri($"{borderWiseAddress}{RelativeUriToUpdateJobStatus}");
								var jsonContent = SerializeAsJson(new
								{
									JobPk = job.JobPk,
									Status = StatusProcessingInCW1,
									Message = "Processing started",
									IsLocked = true
								});

								var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
								task = client.PostAsync(requestUriToUpdateJobStatus, content);

								if (!task.Result.IsSuccessStatusCode)
								{
									LogHttpErrorResponse(task.Result, $"Failed to update job status for JobPK: {job.JobPk} to 'Processing In CW1' in BorderWise.");
								}
								else
								{
									ServiceLogger.Log(LogType.Information, $"Updated job status for JobPK: {job.JobPk} to 'Processing In CW1' in BorderWise.");

									var processed = (jobDetail.JobType == "Commercial Invoice") ? ProcessCommercialInvoice(jobDetail) : ProcessCustomsDeclaration(jobDetail);

									if (processed.success)
									{
										ServiceLogger.Log(LogType.Information, $"{processed.message} JobNumber: {job.JobNumber}, JobPK: {job.JobPk}");

										var requestUriToDeleteJob = new Uri($"{borderWiseAddress}{RelativeUriToDeleteJob}/{job.JobPk}/{true}");
										task = client.PostAsync(requestUriToDeleteJob.ToString(), null);
										ServiceLogger.Log(LogType.Information, $"Deleting job from BorderWise for JobPK: {job.JobPk}");

										if (!task.Result.IsSuccessStatusCode)
										{
											LogHttpErrorResponse(task.Result, $"Failed to delete job with JobPK: {job.JobPk} in BorderWise.");
										}

										ServiceLogger.Log(LogType.Information, $"Completed updating invoice lines for JobNumber {job.JobNumber} with JobPK: {job.JobPk}.");
									}
									else
									{
										jsonContent = SerializeAsJson(new
										{
											JobPk = job.JobPk,
											Status = StatusUnableToProcessInCW1,
											Message = processed.message,
											IsLocked = false
										});

										content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
										task = client.PostAsync(requestUriToUpdateJobStatus, content);

										if (!task.Result.IsSuccessStatusCode)
										{
											LogHttpErrorResponse(task.Result, $"Failed to update status for job with JobPK: {job.JobPk} in BorderWise.");
										}
									}
								}
							}
							else
							{
								ServiceLogger.Log(LogType.Information, $"No invoice lines to update for job with JobPK: {job.JobPk}.");
							}
						}
					}
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					if (exception is TaskCanceledException)
					{
						return false;
					}

					ErrorReporter.ReportOnce("Batch mode operation for BorderWise failed.", exception);
				}
				return result;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		(bool success, string message) ProcessCustomsDeclaration(BorderWiseTariffExchangeModelV2 borderWiseTariffExchangeModel)
		{
			try
			{
				using (Environment.DisposableEnvironment.ForBranch(borderWiseTariffExchangeModel.BranchPk))
				{
					var declarationQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
					declarationQuery.AddToFilter(JobDeclarationSchema.PK, borderWiseTariffExchangeModel.JobPk);

					var factory = new BusinessObjectFactory() { RefreshEnabled = false };
					var baseJobDeclaration = factory.Load<BaseJobDeclaration>(declarationQuery).FirstOrDefault();

					if (baseJobDeclaration != null)
					{
						if (!baseJobDeclaration.HasWHSTransaction && !baseJobDeclaration.DeclarationMessagesHaveBeenSent())
						{
							using (Db.DisposableActionForDbConnection())
							{
								foreach (var tariffSelected in borderWiseTariffExchangeModel.BorderWiseInvoiceLines)
								{
									ZGuid.TryParse(tariffSelected.InvoiceLinePK, out ZGuid invoiceLinePK);
									var invoiceLine = baseJobDeclaration.InvoiceLines.FindByPK(invoiceLinePK) as BaseJobComInvoiceLine;

									if (invoiceLine != null)
									{
										ZGuid.TryParse(tariffSelected.InvoiceNumberPk, out ZGuid invoiceNumberPk);
										var tariff = invoiceLine.FormatTariffForSaving($"{tariffSelected.TariffCode} {tariffSelected.StatCode}".Trim());

										if (invoiceLine.JI_Tariff != tariff)
										{
											invoiceLine.JI_Tariff = tariff;
										}

										if (invoiceLine.JI_Description != tariffSelected.Description)
										{
											invoiceLine.JI_Description = tariffSelected.Description;
										}
									}
								}
							}

							Save(factory);

							return (true, $"InvoiceLines updated successfully for {borderWiseTariffExchangeModel.JobType}");
						}
						else
						{
							var message = string.Empty;

							if (baseJobDeclaration.HasWHSTransaction)
							{
								message = "Job declaration has WHSTransaction.";
							}

							if (baseJobDeclaration.DeclarationMessagesHaveBeenSent())
							{
								message += " Job declaration message have been sent.";
							}

							ServiceLogger.Log(LogType.Information, $"{message} Skipping invoice lines update for JobPK: {baseJobDeclaration.PK}.");

							return (false, message);
						}
					}
				}

				return (false, $"{borderWiseTariffExchangeModel.JobType} does not exist.");
			}
			catch (Exception ex)
			{
				ServiceLogger.Log(LogType.Error, $"An error occurred while processing tariff classifications for {borderWiseTariffExchangeModel.JobType} JobPK: {borderWiseTariffExchangeModel.JobPk}.", ex);
				return (false, $"An error occurred while processing tariff classifications for {borderWiseTariffExchangeModel.JobType}");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		(bool success, string message) ProcessCommercialInvoice(BorderWiseTariffExchangeModelV2 borderWiseTariffExchangeModel)
		{
			try
			{
				using (Environment.DisposableEnvironment.ForBranch(borderWiseTariffExchangeModel.BranchPk))
				{
					var declarationQuery = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
					declarationQuery.AddToFilter(JobComInvoiceHeaderSchema.PK, borderWiseTariffExchangeModel.JobPk);

					var factory = new BusinessObjectFactory() { RefreshEnabled = false };
					var baseJobComInvoiceHeader = factory.Load<BaseJobComInvoiceHeader>(declarationQuery).FirstOrDefault();

					if (baseJobComInvoiceHeader != null)
					{
						using (Db.DisposableActionForDbConnection())
						{
							foreach (var tariffSelected in borderWiseTariffExchangeModel.BorderWiseInvoiceLines)
							{
								ZGuid.TryParse(tariffSelected.InvoiceLinePK, out ZGuid invoiceLinePK);
								var invoiceLine = baseJobComInvoiceHeader.InvoiceLines.FindByPK(invoiceLinePK) as BaseJobComInvoiceLine;

								if (invoiceLine != null)
								{
									var tariff = invoiceLine.FormatTariffForSaving($"{tariffSelected.TariffCode} {tariffSelected.StatCode}".Trim());

									if (invoiceLine.JI_Tariff != tariff)
									{
										invoiceLine.JI_Tariff = tariff;
									}

									if (invoiceLine.JI_Description != tariffSelected.Description)
									{
										invoiceLine.JI_Description = tariffSelected.Description;
									}
								}
							}

							Save(factory);
							return (true, "InvoiceLines updated successfully for Commercial Invoice");
						}
					}
				}

				return (false, "Commercial Invoice does not exist.");
			}
			catch (Exception ex)
			{
				ServiceLogger.Log(LogType.Error, $"An error occurred while processing tariff classifications for Commercial Invoice with JobPK: {borderWiseTariffExchangeModel.JobPk}.", ex);
				return (false, "An error occurred while processing tariff classifications for Commercial Invoice.");
			}
		}

		void Save(BusinessObjectFactory factory)
		{
			try
			{
				factory.Save();
			}
			catch (ZSaveException sEx)
			{
				ZExceptionReporting.HandleSaveException(sEx);
				throw;
			}
		}

		void LogHttpErrorResponse(HttpResponseMessage httpResponseMessage, string message)
		{
			var statusCode = (int)httpResponseMessage.StatusCode;
			var reasonPhrase = httpResponseMessage.ReasonPhrase;
			var errorMessage = $"{message}. HTTP Status Code: {statusCode}, Reason: {reasonPhrase}";
			ServiceLogger.Log(httpResponseMessage.StatusCode == HttpStatusCode.BadGateway ? LogType.Warning : LogType.Error, errorMessage);
		}

		HttpClient GetHttpClient()
		{
			var httpClientFactory = ObjectFactory.Get<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateNew(new HttpClientHandlerWithDiagnostics(new CookieContainer()), TimeSpan.FromSeconds(20));
			var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(ZArchitecture.Environment.DataRegistry.Instance.BorderWiseApiKey));
			var licenseCode = GlbCompany.CurrentCompany.GetLicenceKeyIdentifier(string.Empty);
			httpClient.DefaultRequestHeaders.Add(Authentication, ComputeSha256Hash(decodedString + licenseCode));
			httpClient.DefaultRequestHeaders.Add(LicenseCode, licenseCode);
			return httpClient;
		}

		[ThreadSafe]
		static readonly JsonSerializerOptions JsonSerializerOptions = new()
		{
			// Web API properties are using camel case
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			// Deserializing get only properties like BorderWiseTariffExchangeModelV2.BorderWiseInvoiceLines
			PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
		};

		internal static T DeserializeFromJson<T>(string text)
		{
			return JsonSerializer.Deserialize<T>(text, JsonSerializerOptions);
		}

		internal static string SerializeAsJson<T>(T value)
		{
			return JsonSerializer.Serialize(value, JsonSerializerOptions);
		}

		static string ComputeSha256Hash(string rawData)
		{
			using var sha256Hash = SHA256.Create();
			return Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData)));
		}

		bool IsProductionDatabase()
		{
#if DEBUG
			if (OverriddenValue_ForTest.IsOverriden)
			{
				return OverriddenValue_ForTest.Value;
			}
#endif
			return ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production
				&& !Globals.IsTest && !Globals.IsDebugMode;
		}

		bool IsProductionSystem()
		{
			try
			{
#if DEBUG
				if (OverriddenValue_ForTest.IsOverriden)
				{
					return OverriddenValue_ForTest.Value;
				}
#endif
				return !ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem() && IsProductionDatabase();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return false;
			}
		}

		static bool IsBorderWiseMultiLineClassificationEnabled => ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient
															&& ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassification;

#if DEBUG
		internal readonly Overridable<bool> OverriddenValue_ForTest = new Overridable<bool>();
#endif

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Job status")]
		const string StatusProcessingInCW1 = "Processing In CW1";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Job status")]
		const string StatusUnableToProcessInCW1 = "Unable to Process In CW1";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Web Address")]
		const string RelativeUriToGetJobs = "/api/tariff-classification-job/completed-tariff-exchange-jobs";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Web Address")]
		const string RelativeUriToGetJobDetails = "/api/tariff-classification-job/tariff-exchange-job";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Web Address")]
		const string RelativeUriToUpdateJobStatus = "/api/tariff-classification-job/update-job-status";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Web Address")]
		const string RelativeUriToDeleteJob = "/api/tariff-classification-job/delete-job";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Authentication")]
		const string Authentication = "Authentication";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "LicenseCode")]
		const string LicenseCode = "LicenseCode";
	}
}

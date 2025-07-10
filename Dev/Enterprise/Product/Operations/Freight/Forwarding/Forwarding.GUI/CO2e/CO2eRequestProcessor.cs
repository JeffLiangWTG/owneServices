using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class CO2eRequestProcessor
	{
		readonly ICO2eCalculationManager calculationManager;
		readonly IAddressesValidationManager addressValidationManager;
		readonly ProgressFormManager progressFormManager;

		public CO2eRequestProcessor(ICO2eCalculationManager calculationManager = null, IAddressesValidationManager addressValidationManager = null, ProgressFormManager progressFormManager = null)
		{
			this.calculationManager = calculationManager;
			this.addressValidationManager = addressValidationManager;
			this.progressFormManager = progressFormManager;
		}

		#region Send Request

		public CO2eProcessResult SendRequest(ICO2eCalculationSupporter calculationSupporter, INotifications notifications)
		{
			var result = CO2eHelper.IsApiEnabled
				? SendApiRequest(calculationSupporter)
				: SendEhubRequest(calculationSupporter, notifications);

			return result;
		}

		public async Task<CO2eProcessResult> SendRequestAsync(ICO2eCalculationSupporter calculationSupporter, INotifications notifications)
		{
			var result = CO2eHelper.IsApiEnabled
				? await SendApiRequestAsync(calculationSupporter)
				: SendEhubRequest(calculationSupporter, notifications);

			return result;
		}

		CO2eProcessResult SendEhubRequest(ICO2eCalculationSupporter calculationSupporter, INotifications notifications)
		{
			return CO2eManualRequestHelper.SendRequestManually(calculationSupporter, notifications, recalculationChecker: calculationSupporter.Factory.GetValue<ICO2eRecalculationChecker>());
		}

		#region SendApiRequest

		internal CO2eProcessResult SendApiRequest(ICO2eCalculationSupporter calculationSupporter)
		{
			var (newFactory, newBizo) = CreateNewFactoryAndImportCalculationSupporter((BusinessObject)calculationSupporter, recalculationChecker: calculationSupporter.Factory.GetValue<ICO2eRecalculationChecker>());
			using (newFactory.AddDisposableService())
			{
				var result = SendApiRequestCore((ICO2eCalculationSupporter)newBizo);
				return ProcessAPIResultAndSave(newFactory, result);
			}
		}

		internal CO2eProcessResult SendApiRequestCore(ICO2eCalculationSupporter hostSupporter)
		{
			using var manager = calculationManager ?? new CO2eCalculationManager(new CO2eApiClient());

			HookCalculationManagerEvents(manager, hostSupporter);

			var supporterBOsToSend = GetSupportersToSend(hostSupporter);

			if (supporterBOsToSend.Length == 0)
			{
				return CO2eProcessResult.NotRequired;
			}

			using var progressForm = progressFormManager ?? new CO2eProgressForm(manager.Cancel);
			var results = supporterBOsToSend
				.Select((supporterBO, index) => (ValidateAddressesAndSendRequest(supporterBO, manager, progressForm, index == 0 ? null : hostSupporter), supporterBO))
				.ToArray();

			return ProcessResults(results);
		}

		CO2eProcessResult ValidateAddressesAndSendRequest(ICO2eCalculationSupporter supporter, ICO2eCalculationManager manager, ProgressFormManager progressForm, ICO2eCalculationSupporter hostSupporter)
		{
			var previousCO2eValue = (TotalCO2e: supporter.GetTotalCO2e(),
									Transports: Enumerable.Empty<BusinessObject>());
			if (supporter is IAddressesValidation cO2eAddressValidation && cO2eAddressValidation.AddressesToValidate.Length > 0)
			{
				progressForm.UpdateStatus(GetStatus(supporter, (NoResString)"Validating Addresses"), 0);
				(addressValidationManager ?? new CO2eAddressValidationManager(cO2eAddressValidation.AddressesToValidate)).Validate();
			}

			progressForm.UpdateStatus(GetStatus(supporter, (NoResString)"Calculating"), 0);

			var result = manager.GetEmission(supporter as BusinessObject, hostSupporter);

			progressForm.UpdateStatus(GetStatus(supporter, (NoResString)"Processing"), 0);

			return ProcessEmissionResult(result, manager, progressForm, supporter, supporter.Factory, previousCO2eValue);
		}

		#endregion

		#region SendApiRequestAsync

		internal async Task<CO2eProcessResult> SendApiRequestAsync(ICO2eCalculationSupporter calculationSupporter)
		{
			var (newFactory, newBizo) = CreateNewFactoryAndImportCalculationSupporter((BusinessObject)calculationSupporter, recalculationChecker: calculationSupporter.Factory.GetValue<ICO2eRecalculationChecker>());
			using (newFactory.AddDisposableService())
			{
				var result = await SendApiRequestCoreAsync((ICO2eCalculationSupporter)newBizo);
				return ProcessAPIResultAndSave(newFactory, result);
			}
		}

		internal async Task<CO2eProcessResult> SendApiRequestCoreAsync(ICO2eCalculationSupporter hostSupporter)
		{
			using var manager = calculationManager ?? new CO2eCalculationManager(new CO2eApiClient());

			HookCalculationManagerEvents(manager, hostSupporter);

			var supporterBOsToSend = GetSupportersToSend(hostSupporter);

			if (supporterBOsToSend.Length == 0)
			{
				return CO2eProcessResult.NotRequired;
			}

			using var progressForm = progressFormManager ?? new CO2eProgressForm(manager.Cancel);
			var supporterTaskTuples = supporterBOsToSend.Select((supporter, index) => (supporter,
			task: ValidateAddressesAndSendRequestAsync(supporter, manager, progressForm, index == 0 ? null : hostSupporter))).ToArray();

			var results = await Task.WhenAll(supporterTaskTuples.Select(t => t.task));
			return ProcessResults(results.Select((result, idx) => (result, supporterTaskTuples[idx].supporter)).ToArray());
		}

		async Task<CO2eProcessResult> ValidateAddressesAndSendRequestAsync(ICO2eCalculationSupporter supporter, ICO2eCalculationManager manager, ProgressFormManager progressForm, ICO2eCalculationSupporter hostSupporter)
		{
			var previousCO2eValue = (TotalCO2e: supporter.GetTotalCO2e(),
									Transports: Enumerable.Empty<BusinessObject>());
			if (supporter is IAddressesValidation cO2eAddressValidation && cO2eAddressValidation.AddressesToValidate.Length > 0)
			{
				progressForm.UpdateStatus(GetStatus(supporter, (NoResString)"Validating Addresses"), 0);
				var addressValidationToken = new CancellationTokenSource();
				progressForm.Cancelled += delegate
				{
					addressValidationToken.Cancel();
					addressValidationToken.Dispose();
				};
				await (addressValidationManager ?? new CO2eAddressValidationManager(cO2eAddressValidation.AddressesToValidate))
					.ValidateAsync(addressValidationToken);
			}

			progressForm.UpdateStatus(GetStatus(supporter, (NoResString)"Calculating"), 0);

			var result = await manager.GetEmissionAsync(supporter as BusinessObject, hostSupporter);

			progressForm.UpdateStatus(GetStatus(supporter, (NoResString)"Processing"), 0);

			return ProcessEmissionResult(result, manager, progressForm, supporter, supporter.Factory, previousCO2eValue);
		}

		#endregion

		#region Common

		CO2eProcessResult ProcessAPIResultAndSave(BusinessObjectFactory factory, CO2eProcessResult result)
		{
			if (result.Type != CO2eResultType.ApiFail && result.Type != CO2eResultType.ApiError)
			{
				try
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, () => { }, true, true);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ReportError(ex);
					return CO2eProcessResult.ApiError(ex);
				}
			}

			return result;
		}

		(BusinessObjectFactory, BusinessObject) CreateNewFactoryAndImportCalculationSupporter(BusinessObject supporter, ICO2eRecalculationChecker recalculationChecker = null)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "CO2e Api Calculation Request Sender" };

			if (recalculationChecker != null)
			{
				factory.SetValue(() => recalculationChecker);
			}

			var newBizo = factory.ImportFromAnotherFactory(supporter);
			return (factory, newBizo);
		}

		void HookCalculationManagerEvents(ICO2eCalculationManager manager, ICO2eCalculationSupporter hostSupporter)
		{
			manager.Cancelled += (s, _) => { hostSupporter.RecordLog(CO2eEventType.Rejected, (NoResString)"Request cancelled by user."); };
			manager.Timeout += (s, _) => { hostSupporter.RecordLog(CO2eEventType.Rejected, (NoResString)"Request timeout."); };
		}

		internal ICO2eCalculationSupporter[] GetSupportersToSend(ICO2eCalculationSupporter hostSupporter)
		{
			if (!ShouldRecalculate(hostSupporter))
			{
				return Array.Empty<ICO2eCalculationSupporter>();
			}

			return hostSupporter.AdditionalCalculationSupporters
				.Where(x => x.Supporter.GetCO2eStatus() != CO2eStatusList.Codes.Current)
				.Select(x => x.Supporter)
				.Prepend(hostSupporter)
				.ToArray();
		}

		internal bool ShouldRecalculate(ICO2eCalculationSupporter supporter)
		{
			if (supporter.GetCO2eStatus() != CO2eStatusList.Codes.Current)
			{
				return true;
			}

			return supporter.Factory.GetValue<ICO2eRecalculationChecker>()?.ShouldRecalculate(supporter) ?? false;
		}

		#endregion

		#endregion

		#region Process results

		internal static CO2eProcessResult ProcessResults((CO2eProcessResult result, ICO2eCalculationSupporter supporter)[] resultSupporterTuples)
		{
			if (resultSupporterTuples.Length == 1)
			{
				return resultSupporterTuples[0].result;
			}

			if (resultSupporterTuples.All(t => t.result.Type is CO2eResultType.ApiSuccess))
			{
				return new CO2eProcessResult(CO2eResultType.ApiSuccess);
			}

			if (resultSupporterTuples.Any(t => t.result.Type is CO2eResultType.Unauthorized))
			{
				return CO2eProcessResult.Unauthorized;
			}

			var message = new ZStringBuilder();
			message.Append(string.Empty);
			foreach (var (result, supporter) in resultSupporterTuples)
			{
				if (result.Type is CO2eResultType.ApiError)
				{
					var errorMessage = Res.GetString("343a78cc-dab3-4b97-87c0-a482c180cdd8", "Error requesting greenhouse gas emissions calculation service.");
					message.Append(((IBusiness)supporter).HumanReadableName + " - " + errorMessage);
				}
				else if (result.Type is CO2eResultType.ApiFail)
				{
					var failMessage = Res.GetString("f9e40e88-e541-4e6f-80ce-132234d8cd6d", "Error requesting greenhouse gas emissions calculation: {0}", result.Message);
					message.Append(((IBusiness)supporter).HumanReadableName + " - " + failMessage);
				}
			}

			var messageString = message.ToStringWithNewLineBetweenAppends();
			return string.IsNullOrWhiteSpace(messageString) ? CO2eProcessResult.Empty : CO2eProcessResult.ApiFail(messageString);
		}

		CO2eProcessResult ProcessEmissionResult(EmissionResult result, ICO2eCalculationManager manager, ProgressFormManager progressForm, ICO2eCalculationSupporter supporter, BusinessObjectFactory factory, (decimal TotalCO2e, IEnumerable<BusinessObject> Transports) previousCO2eValue)
		{
			if (manager.Token.IsCancellationRequested)
			{
				return CO2eProcessResult.ApiCancelled;
			}

			var logProvider = supporter as IStmALogProvider;

			if (!result.HasResult)
			{
				if (result.Error != null)
				{
					if (result.Error.Message.Contains((NoResString)"Unauthorized"))
					{
						supporter.LogGHGEvent(CO2eEventType.Unauthorized, (NoResString)"Unauthorized Request: Ensure you are using a registered version of CargoWise");
						return CO2eProcessResult.Unauthorized;
					}
					else if (result.Error.Message.Contains((NoResString)"Service Unavailable"))
					{
						supporter.LogGHGEvent(CO2eEventType.ServiceUnavailable, (NoResString)"Issue communicating with server. Please try again later.");
						return CO2eProcessResult.ServiceUnavailable;
					}
					else
					{
						ReportError(result.Error);
						return CO2eProcessResult.ApiError(result.Error);
					}
				}
				else
				{
					return CO2eProcessResult.Empty;
				}
			}

			if (result.JsonContent != null)
			{
				return CO2eProcessResult.ApiFail(result.JsonContent.Message);
			}

			supporter.OnRequested();

			AddRequestLogs(result.Request, logProvider, factory);

			if (result.IsUShipment)
			{
				var importer = supporter.GetResponseImporter(factory);
				importer?.ImportGreenHouseGasEmission(result.UXml.Left, supporter, previousCO2eValue: previousCO2eValue);
				AddResponseLogs_UShimpent(result, logProvider, factory, importer?.LogMessages);

				progressForm.UpdateStatus(GetStatus(supporter, (NoResString)"Updated"), 0);
				return CO2eProcessResult.ApiSuccess(result);
			}
			else
			{
				result.UXml.Right.OnCO2eRejectionEvent(supporter);
				supporter.OnCalculated(false);
				var rejectReason = GetUEventRejectReason(result.UXml.Right);
				AddResponseLogs_UEvent(result, logProvider, factory, rejectReason);

				progressForm.UpdateStatus(GetStatus(supporter, (NoResString)"Rejected"), 0);
				return CO2eProcessResult.ApiRejected(result, rejectReason);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception message")]
		static void ReportError(Exception ex)
		{
			ExceptionReporter.Instance.ReportDeveloperException("Error requesting greenhouse gas emissions calculation service.", ex);
		}

		static void AddResponseLogs_UShimpent(EmissionResult result, IStmALogProvider logProvider, BusinessObjectFactory factory, string logMessages)
		{
			var dataImportLog = logProvider.Logs.AddNew(AutoEvents.DataImport);

			var interchange = CreateInterchange(result, factory);

			if (!string.IsNullOrEmpty(result.UXmlString))
			{
				var message = CreateEDIMessage(result, interchange);

				message.AddUniversalDataLink(dataImportLog);

				if (message is IStmNoteParent noteParent)
				{
					noteParent.Notes.AddNew(false, PredefinedNoteTypes.Instance.DataImportLogNote.Description, logMessages);
				}

				message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			}
		}

		static void AddResponseLogs_UEvent(EmissionResult result, IStmALogProvider logProvider, BusinessObjectFactory factory, string failureReason)
		{
			var parameters = new Dictionary<string, string> { { Params.Reason, failureReason } };
			var interchangeRejectedLog = logProvider.Logs.AddNew(AutoEvents.InterchangeRejected, parameters.ToArray());

			var interchange = CreateInterchange(result, factory);

			if (!string.IsNullOrEmpty(result.UXmlString))
			{
				var message = CreateEDIMessage(result, interchange);

				message.AddUniversalDataLink(interchangeRejectedLog);

				message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			}
		}

		static string GetUEventRejectReason(UniversalDataBuss.DataObjects.Universal.Event uexml)
		{
			return uexml.ContextCollection?.FirstOrDefault(context => string.Equals(context.Type, nameof(UniversalDataBuss.DataObjects.Universal.Event.ContextTypes.FailureReason), StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
		}

		static void AddRequestLogs(EmissionRequest request, IStmALogProvider logProvider, BusinessObjectFactory factory)
		{
			var dataExportLog = logProvider.Logs.AddNew(AutoEvents.DataExport);

			var interchange = CreateInterchange(request, factory);

			if (!string.IsNullOrEmpty(request.UXmlString))
			{
				var message = CreateEDIMessage(request, interchange);

				message.AddUniversalDataLink(dataExportLog);
			}
		}

		static IXmlEDIInterchange CreateInterchange(IEmissonXml emissionXml, BusinessObjectFactory factory)
		{
			var interchange = factory.New<IXmlEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_InterchangeType = EDIMessageTypeList.Codes.XDC;

			if (emissionXml is EmissionRequest request)
			{
				interchange.EI_From = Env.CurrentCompany.GetLicenceCode();
				interchange.EI_To = CO2eHelper.CO2eCalculationID;
				interchange.EI_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
				interchange.EI_BodyText = request.InterchangeString;
			}
			else if (emissionXml is EmissionResult result)
			{
				interchange.EI_From = CO2eHelper.CO2eCalculationID;
				interchange.EI_To = Env.CurrentCompany.GetLicenceCode();
				interchange.EI_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
				interchange.EI_BodyText = result.InterchangeString;
			}

			return interchange;
		}

		static IXmlEDIMessage CreateEDIMessage(IEmissonXml emissionXml, IXmlEDIInterchange interchange)
		{
			var message = interchange.AddNeweHubMessage();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;

			if (emissionXml is EmissionRequest request)
			{
				message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				message.EM_Status = EDIMessageStatusList.Codes.Sent;

				message.EM_MessageText = request.UXmlString;
				message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			}
			else if (emissionXml is EmissionResult result)
			{
				message.EM_ReceiveTransmit = EDICommunicationsModeCommsDirectionList.Codes.Receive;
				message.EM_Status = EDIMessageStatusList.Codes.Received;

				message.EM_MessageText = result.UXmlString;
				message.EM_MessageSubType = result.UXml.IsLeft ? EDIMessageSubTypeList.Codes.XmlUniversalShipment : EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			}

			return message;
		}

		#endregion

		#region Progress Form status

		string GetStatus(ICO2eCalculationSupporter supporter, string status)
		{
			return $"{status} {CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)supporter)}";
		}

		#endregion
	}
}

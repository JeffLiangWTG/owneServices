using System;
using System.Globalization;
using System.Threading;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.DataTransfer
{
	public abstract class ShipmentStatusUpdatedLogProcessor
	{
		public void Process(ILogger logger, IQueuedLog log)
		{
			if (Enable())
			{
				var logParameters = StmALog.GetParametersFromReference(log.SJ_Reference);

				var @event = GetEvent(logParameters);
				if (!IsEventAndParentTableMatchProcessor(@event, log.SJ_ParentTableCode))
				{
					return;
				}

				var logParent = GetLogParent(log);
				if (logParent != null && ShouldProcess(logParent))
				{
					var logBranch = log.Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, log.SJ_GB_NKBranch);
					using (logBranch != null ? Environment.DisposableEnvironment.ForBranch(logBranch.PK.ToGuid()) : null)
					{
						var nameSpace = GetNameSpace(logParameters);
						var recipientRole = new[] { RecipientRoleType.NVO }.ToRecipientRoleDetails();
						var actionInfo = new ActionInfo(recipientRole, logParent);

						var xmlWriter = new ShipmentUXmlReWriterHackBeingReplacedSoon(nameSpace);
						var bookingStatusDataObjectWriter = GetShipmentStatusDataObjectWriter(logParameters, @event, logParent, nameSpace);

						SendUniversalShipmentXML(logger, actionInfo, logParent, bookingStatusDataObjectWriter, xmlWriter);
					}
				}
			}
		}

		protected virtual bool Enable()
		{
			return true;
		}

		protected virtual bool IsEventAndParentTableMatchProcessor(Event @event, string logParentTableCode)
		{
			return @event != null;
		}

		protected abstract BusinessObject GetLogParent(IQueuedLog log);

		protected abstract bool ShouldProcess(BusinessObject logParent);

		ITopLevelDataObjectWriter GetShipmentStatusDataObjectWriter(ObservableDictionary<string, string> logParameters, Event @event, BusinessObject logParent, string nameSpace)
		{
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.NVO, logParent), schema: UniversalXmlSchema.Version_2012_11_DO_NOT_USE);
			var dataContextDocumentName = GetDataContextDocumentName(nameSpace);
			var rejectionReason = (@event == AutoEvents.MessageRejected) ? GetParameterValue(logParameters, Params.Reason) : string.Empty;

			return GetShipmentStatusDataObjectWriterCore(writeManager, logParent, @event, dataContextDocumentName, rejectionReason, dataContextDocumentName == DocumentNameBookingConfirmation);
		}

		protected abstract ITopLevelDataObjectWriter GetShipmentStatusDataObjectWriterCore(DataWritingManager writeManager, BusinessObject logParent,
			Event @event, string dataContextDocumentName, string rejectionReason, bool shouldPopulateTransportLegCollection);

		void SendUniversalShipmentXML(ILogger logger, ActionInfo actionInfo, BusinessObject logParent, ITopLevelDataObjectWriter dataObjectWriter, ShipmentUXmlReWriterHackBeingReplacedSoon xmlWriter)
		{
			var processor = UniversalXmlWorkflowProcessorBuilder.New(
				actionInfo,
				new UniversalXmlCommunicationModeProvider(() => (CommunicationModes, null)),
				(outboundSessionTracker) => dataObjectWriter,
				logParent,
				null,
				xmlWriter,
				UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

			var messageLogger = new NotificationBuffer();
			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			processor.Process(messageLogger, replaceThisTokenEventuallyQuestionMarkExclamationMark);

			if (messageLogger.HasErrors)
			{
				var messageError = messageLogger.GetEventsByType(ErrorType.Error).ToMessageListString();
				var msg = string.Format(CultureInfo.InvariantCulture, (NoResString)"Error sending shipment status update message:-\r\n\r\n{0}", messageError);
				logger.Log(LogType.Error, msg);
			}
			else
			{
				logger.Log(LogType.Information, "Universal Event queued for sending.");
			}
		}

		Event GetEvent(ObservableDictionary<string, string> logParameters)
		{
			Event @event = null;
			if (CheckNewShipmentStatus(logParameters, ShipmentStatusList.Codes.Confirmed, ShipmentStatusList.Codes.Booked))
			{
				@event = AutoEvents.MessageAccepted;
			}
			if (IsBookingWithdrawAccepted(logParameters))
			{
				@event = AutoEvents.MessageWithdrawCancelAccepted;
			}
			else if (CheckNewShipmentStatus(logParameters, ShipmentStatusList.Codes.BookingRejected, ShipmentStatusList.Codes.SIRejected) || IsBookingWithdrawRejected(logParameters))
			{
				@event = AutoEvents.MessageRejected;
			}

			return @event;
		}

		IEDICommunicationsMode[] CommunicationModes
		{
			get
			{
				if (communicationModes == null)
				{
					communicationModes = new IEDICommunicationsMode[]
					{
						new NonPersistentEDICommunicationMode
						{
							EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
							EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment,
							EK_Destination = eHubRecipientID
						}
					};
				}
				return communicationModes;
			}
		}
		IEDICommunicationsMode[] communicationModes;

		string GetDataContextDocumentName(string nameSpace)
		{
			var result = string.Empty;
			if (nameSpace == BLDataNamespace)
			{
				result = DocumentNameBLData;
			}
			else if (nameSpace == BookingConfirmationNamespace)
			{
				result = DocumentNameBookingConfirmation;
			}

			return result;
		}

		string GetNameSpace(ObservableDictionary<string, string> logParameters)
		{
			var result = string.Empty;
			if (CheckNewShipmentStatus(logParameters, ShipmentStatusList.Codes.Confirmed, ShipmentStatusList.Codes.SIRejected))
			{
				result = BLDataNamespace;
			}
			else if (CheckNewShipmentStatus(logParameters, ShipmentStatusList.Codes.Booked, ShipmentStatusList.Codes.BookingRejected)
				|| IsBookingWithdrawAccepted(logParameters)
				|| IsBookingWithdrawRejected(logParameters))
			{
				result = BookingConfirmationNamespace;
			}

			return result;
		}

		bool CheckNewShipmentStatus(ObservableDictionary<string, string> logParameters, params string[] statusToCheck)
		{
			foreach (var status in statusToCheck)
			{
				if (HasParameterEqualTo(logParameters, Params.New, status)
					&& HasParameterEqualTo(logParameters, Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus))
				{
					return true;
				}
			}

			return false;
		}

		bool IsBookingWithdrawAccepted(ObservableDictionary<string, string> logParameters)
		{
			return HasParameterEqualTo(logParameters, Params.New, ShipmentStatusList.Codes.BookingCancelled)
				&& HasParameterEqualTo(logParameters, Params.Old, ShipmentStatusList.Codes.EBookingCancellationRequest)
				&& HasParameterEqualTo(logParameters, Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus);
		}

		bool IsBookingWithdrawRejected(ObservableDictionary<string, string> logParameters)
		{
			return (HasParameterEqualTo(logParameters, Params.New, ShipmentStatusList.Codes.Booked) || HasParameterEqualTo(logParameters, Params.New, ShipmentStatusList.Codes.ElectronicBooking))
				&& HasParameterEqualTo(logParameters, Params.Old, ShipmentStatusList.Codes.EBookingCancellationRequest)
				&& HasParameterEqualTo(logParameters, Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus);
		}

		string GetParameterValue(ObservableDictionary<string, string> logParameters, string key)
		{
			var result = string.Empty;
			logParameters?.TryGetValue(key, out result);

			return result;
		}

		bool HasParameterEqualTo(ObservableDictionary<string, string> logParameters, string key, string expectedValue)
		{
			var parameterValue = GetParameterValue(logParameters, key);
			return !string.IsNullOrWhiteSpace(parameterValue) && parameterValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
		}

		const string eHubRecipientID = "SHIPPING_INSTRUCTION";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded document name.")]
		const string DocumentNameBookingConfirmation = "Booking Confirmation";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded document name.")]
		const string DocumentNameBLData = "BL Data";
		const string BookingConfirmationNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/BookingConfirmation/1";
		const string BLDataNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/BLData/1";
	}
}

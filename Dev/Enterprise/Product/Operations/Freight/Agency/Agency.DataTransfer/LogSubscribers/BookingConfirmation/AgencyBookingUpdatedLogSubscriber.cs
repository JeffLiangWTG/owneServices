using System;
using System.Globalization;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer
{
	[Serializable]
	public sealed class AgencyBookingUpdatedLogSubscriber : LogSubscriber
	{
		public override string Name => nameof(AgencyBookingUpdatedLogSubscriber);

		public override string FriendlyName => FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber;

		public override string[] EventTypes => new[] { Events.EditedARecordCode };

		public override string[] TableNames => new[] { JobShipmentSchema.Constants.TableName };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
			{
				var xmlWriter = new ShipmentUXmlReWriterHackBeingReplacedSoon(BookingConfirmationNamespace);

				foreach (var log in queuedLogs)
				{
					var logParent = log.Factory.Load(JobShipmentSchema.Constants.Prefix, log.SJ_ParentID);
					var logParameters = StmALog.GetParametersFromReference(log.SJ_Reference);

					if (logParent != null && logParameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.LogSubscriber, out var logSubscriber)
						&& logSubscriber == FreightConstants.LogSubscriber.AgencyBookingUpdatedLogSubscriber)
					{
						var logBranch = log.Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, log.SJ_GB_NKBranch);
						using (logBranch != null ? Environment.DisposableEnvironment.ForBranch(logBranch.PK.ToGuid()) : null)
						{
							var recipientRole = new[] { RecipientRoleType.NVO }.ToRecipientRoleDetails();
							var actionInfo = new ActionInfo(recipientRole, logParent);

							SendUniversalShipmentXML(DefaultLogger, actionInfo, logParent, GetAgencyBookingUpdatedDataObjectWriter(logParent), xmlWriter);
						}
					}
				}
			}
		}

		ITopLevelDataObjectWriter GetAgencyBookingUpdatedDataObjectWriter(BusinessObject logParent)
		{
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.NVO, logParent), schema: UniversalXmlSchema.Version_2012_11_DO_NOT_USE);
			return new AgencyBookingUpdatedDataObjectWriter(writeManager);
		}

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
				var msg = string.Format(CultureInfo.InvariantCulture, (NoResString)"Error sending shipment status update message:-\r\n\r\n{0}", messageError); // Just a log string
				logger.Log(LogType.Error, msg);
			}
			else
			{
				logger.Log(LogType.Information, "Universal Event queued for sending."); // Just a log string
			}
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

		const string eHubRecipientID = "SHIPPING_INSTRUCTION"; // Event Type Parameter
		const string BookingConfirmationNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/BookingConfirmation/1";
	}
}

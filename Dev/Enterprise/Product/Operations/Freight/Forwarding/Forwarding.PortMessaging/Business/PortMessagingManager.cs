using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public abstract class PortMessagingManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected PortMessagingManager(BusinessObject messageOriginator)
			: base(messageOriginator.Factory)
		{
			MessageOriginator = Argument.NotNull(messageOriginator, "messageOriginator");
		}

		public enum MessageType
		{
			PortOrderWithHDS,
			PortOrderWithHDSCancellationBecauseOfErrors,
			PortOrderWithHDSCancellationOnExit,
			PortOrderWithHDSForwardingCancellation,
			PortOrderInbound,
			PortOrderOutbound,
			StopRequest,
			GatePass,
			RequestForPortServices,
			CertificateOfObligation,
			RequestForRailDischarge
		}

		public const string DakosyRecipientCode = "DAKOSYHAM";

		public static bool IsHDS(MessageType messageType)
		{
			return messageType == MessageType.PortOrderWithHDS || messageType == MessageType.PortOrderWithHDSCancellationBecauseOfErrors
					|| messageType == MessageType.PortOrderWithHDSCancellationOnExit || messageType == MessageType.PortOrderWithHDSForwardingCancellation;
		}

		public static string GetPurposeCode(MessageType messageType, bool isCancellation, bool hasDGs)
		{
			string purposeCode = GetPurposeCode(messageType);

			if (IsHDS(messageType))
			{
				return purposeCode;
			}

			char prefix;

			if (isCancellation)
			{
				prefix = PurposeCodes.CancellationPrefix;
			}
			else
			{
				prefix = hasDGs ? PurposeCodes.DGPrefix : PurposeCodes.OrderPrefix;
			}

			return prefix + purposeCode;
		}

		static string GetPurposeCode(MessageType messageType)
		{
			switch (messageType)
			{
				case MessageType.PortOrderWithHDS:
					return PurposeCodes.PortOrderWithHDS;
				case MessageType.PortOrderWithHDSCancellationBecauseOfErrors:
				case MessageType.PortOrderWithHDSCancellationOnExit:
				case MessageType.PortOrderWithHDSForwardingCancellation:
					return PurposeCodes.PortOrderWithHDSCancellation;
				case MessageType.PortOrderInbound:
					return PurposeCodes.PortOrderInbound;
				case MessageType.PortOrderOutbound:
					return PurposeCodes.PortOrderOutbound;
				case MessageType.StopRequest:
					return PurposeCodes.StopRequest;
				case MessageType.GatePass:
					return PurposeCodes.GatePass;
				case MessageType.RequestForPortServices:
					return PurposeCodes.RequestForPortServices;
				case MessageType.CertificateOfObligation:
					return PurposeCodes.CertificateOfObligation;
				case MessageType.RequestForRailDischarge:
					return PurposeCodes.RequestForRailDischarge;
				default:
					throw new ArgumentException("Message purpose not accounted for: " + messageType.ToString());
			}
		}

		class PurposeCodes
		{
			public const string PortOrderWithHDS = "HDS";
			public const string PortOrderWithHDSCancellation = "S01";

			public const char OrderPrefix = 'A';
			public const char DGPrefix = 'G';
			public const char CancellationPrefix = 'S';

			public const string PortOrderInbound = "08";
			public const string PortOrderOutbound = "09";
			public const string StopRequest = "10";
			public const string GatePass = "06";
			public const string RequestForPortServices = "15";
			public const string CertificateOfObligation = "18";
			public const string RequestForRailDischarge = "22";
		}

		public BusinessObject MessageOriginator { get; private set; }
		public abstract PortMessagingData Data { get; }

		[ResourceStringData("PortMessagingManager|SZBNumber", Caption = "SZB Number")]
		public abstract ZString SZBNumber { get; }

		[ResourceStringData("PortMessagingManager|SZBInformation", Caption = "Release Status")]
		public abstract ZString SZBInformation { get; }

		[ResourceStringData("PortMessagingManager|SZBIssueDate", Caption = "SZB Received")]
		public abstract ZDateTime SZBIssueDate { get; }

		public abstract bool ShouldShowPortMessagingForDakosy { get; }
		public abstract ZString CheckPortMessagingAvailability();

		public abstract bool IsExport { get; }
		public abstract bool IsImport { get; }

		public abstract bool HasDG { get; }

		public event EventHandler DataChanged;

		#region Shipment / PackLines

		public ShipmentPortMessaging PortMessaging
		{
			get { return portMessaging ?? (portMessaging = GetShipmentPortMessaging()); }
		}
		ShipmentPortMessaging portMessaging;

		protected virtual ShipmentPortMessaging GetShipmentPortMessaging()
		{
			return Factory.GetNull<ShipmentPortMessaging>();
		}

		public ForwardingPackLineWithPortMessagingCollection PackLines
		{
			get
			{
				if (packLines == null)
				{
					packLines = CreatePackLinesCollection();
					packLines.Load();
				}

				return packLines;
			}
		}
		ForwardingPackLineWithPortMessagingCollection packLines;

		protected virtual ForwardingPackLineWithPortMessagingCollection CreatePackLinesCollection()
		{
			return new ForwardingPackLineWithPortMessagingCollection(Factory.GetNull<ForwardingShipment>());
		}

		#endregion

		#region Validation

		public ZString RunPreSendDataValidation(MessageType messageType, bool isCancellation = false, bool confirmCancellation = true)
		{
			var portMessagingAvailability = CheckPortMessagingAvailability();
			if (!portMessagingAvailability.IsEmpty)
			{
				return portMessagingAvailability;
			}

			var prerequisiteErrorMessage = RunCommonPreSendDataValidation(messageType);
			if (!prerequisiteErrorMessage.IsEmpty)
			{
				return prerequisiteErrorMessage;
			}

			prerequisiteErrorMessage = RunPreSendDataValidationCore(messageType);
			if (!prerequisiteErrorMessage.IsEmpty)
			{
				return prerequisiteErrorMessage;
			}

			if (Data == null)
			{
				return Res.GetString("9612c8d4-a128-4728-9bdc-0789d688f2f7", "There is no message data to send.");
			}

			if (!SZBNumber.IsEmpty
				&& messageType == MessageType.PortOrderWithHDS
				&& !StatusRetriever.IsMessageCancellationStillPending(messageType))
			{
				return Res.GetString("71d4eebc-2da0-47fc-a9ca-692a41159adb", "SZB number has already been received back from Dakosy. You must cancel the previous order before you can resend.");
			}

			if (MessageOriginator.HasChanges || !MessageOriginator.IsInDatabase)
			{
				return Res.GetString("362b3cdb-dedf-45af-ae91-7558c30fd0fa", "{0} must be saved first before you can send Port Order.", MessageOriginator.HumanReadableName);
			}

			if (HasDataValidationErrors)
			{
				return Res.GetString("8d9ae3b6-c113-473b-9858-5c1088a1e32a", "You must fix validation errors before sending a Port Order or Cancellation.");
			}

			if (StatusRetriever.IsWaitingForReply(messageType))
			{
				return Res.GetString("51194fbb-4ba8-4809-ba90-7bd91923bd32", "A reply has not been received for the last message to Dakosy.\r\nReplies must be received from Dakosy before you can send additional messages.");
			}

			if (StatusRetriever.IsMessageStillPending(messageType))
			{
				return Res.GetString("6b7c8195-41b2-4136-b229-5d9d34fd5b11", "Dakosy status is Pending.\r\nAn Acceptance or Rejection must be received before you can send additional messages to Dakosy.");
			}

			if (isCancellation && !confirmCancellation)
			{
				return Res.GetString("8283CDEA-8A99-4350-A561-47E31EA7B0B5", "Cancellation message aborted.");
			}

			return ZString.Empty;
		}

		protected ZString RunCommonPreSendDataValidation(MessageType messageType)
		{
			var containerValidationErrorsForEntryTypeSAC = CheckContainersForEntryTypeSAC();
			if (!containerValidationErrorsForEntryTypeSAC.IsEmpty)
			{
				return containerValidationErrorsForEntryTypeSAC;
			}

			if (messageType == MessageType.PortOrderWithHDS)
			{
				var validationErrorForEori = CheckConsolSendingForwarderHasEori();
				if (!validationErrorForEori.IsEmpty)
				{
					return validationErrorForEori;
				}
			}

			return ZString.Empty;
		}

		protected ZString CheckContainersForEntryTypeSAC()
		{
			if (Data.IsSACEntryType())
			{
				var allowedContainerModes = new ZString[]
				{
						Constants.ContainerModes.FCL,
						Constants.ContainerModes.BuyersConsol,
						Constants.ContainerModes.Groupage,
				};

				var currentConsol = GetCurrentConsol();
				if (currentConsol != null && !allowedContainerModes.Contains(currentConsol.JK_ConsolMode))
				{
					return Res.GetString("2244b692-0ac5-4227-b2ca-6b85cd6edea5", "{0}: Allowed container modes for entry type SAC are {1}.",
						currentConsol.HumanReadableName, string.Join(", ", allowedContainerModes));
				}

				Func<IPortMessaging, bool> isShipmentEntryTypeSAC = (shipment) =>
				{
					return shipment.EntryType == EntryTypeList.Codes.ConsolidatedContainer;
				};

				var topLevelShipments = GetTopLevelShipments();

				var shipmentsWithEntryTypeSAC =
					topLevelShipments
					.Where(shipment => PortMessagingHelper.CheckPortMessagingForShipment(shipment, isShipmentEntryTypeSAC))
					.ToArray();

				if (shipmentsWithEntryTypeSAC.Any())
				{
					var shipmentWithUnpackedPackline = shipmentsWithEntryTypeSAC
						.FirstOrDefault(shipment => shipment.OuterPackLines.Cast<ForwardingPackLine>().Any(packline => packline.GetContainer(currentConsol) == null));

					if (shipmentWithUnpackedPackline != null)
					{
						return Res.GetString("973fce0f-9873-4d0a-b153-68e929f7585f", "{0}: All packlines must be packed into containers for entry type SAC.",
							shipmentWithUnpackedPackline.HumanReadableName, string.Join(", ", allowedContainerModes));
					}

					var allShipmentContainers = shipmentsWithEntryTypeSAC
						.SelectMany(shipment => shipment.Containers)
						.Distinct()
						.ToArray();

					if (allShipmentContainers.Any(container => container.JC_ContainerNum.IsEmpty))
					{
						return Res.GetString("4f68b764-51df-4354-831a-28726ba860ce", "All containers are required to have container number for entry type SAC.");
					}

					var containerWithIncorrectMode = allShipmentContainers.FirstOrDefault(container => !allowedContainerModes.Contains(container.JC_ContainerMode));
					if (containerWithIncorrectMode != null)
					{
						return Res.GetString("2244b692-0ac5-4227-b2ca-6b85cd6edea5", "{0}: Allowed container modes for entry type SAC are {1}.",
							containerWithIncorrectMode.HumanReadableName, string.Join(", ", allowedContainerModes));
					}
				}
			}

			return ZString.Empty;
		}

		public ZString CheckPreSendWarningsRequiringConfirmation(MessageType messageType)
		{
			return ZString.Empty;
		}

		ZString CheckConsolSendingForwarderHasEori()
		{
			var topLevelShipments = GetTopLevelShipments();

			Func<IPortMessaging, bool> predicate = (messaging) =>
				(!PortMessagingHelper.IsEORIAndLRNEffectiveDate() && messaging.EntryType == EntryTypeList.Codes.AE1ExportDeclaration)
				|| messaging.EntryType == EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities
				|| messaging.EntryType == EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;

			var shipmentsWithExpectedEntryTypes = topLevelShipments
				.Where(shipment => PortMessagingHelper.CheckPortMessagingForShipment(shipment, predicate));

			if (shipmentsWithExpectedEntryTypes.Any())
			{
				var consol = GetCurrentConsol();

				if (consol?.SendingForwarder == null
					|| !consol.SendingForwarder.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).Any())
				{
					return PortMessagingHelper.IsEORIAndLRNEffectiveDate()
						? Res.GetString("5016a147-666c-4955-aec5-fc58da13b327",
						"When Entry Type is {0} or {1}, the Consol > Sending Forwarder’s EORI number is mandatory.",
						EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities,
						EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN)
						: Res.GetString("d00ea5dd-bbc3-4543-8b0f-3d03a67d7b11",
						"When Entry Type is {0}, {1} or {2}, the Consol > Sending Forwarder’s EORI number is mandatory.",
						EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities,
						EntryTypeList.Codes.AE1ExportDeclaration,
						EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN);
				}
			}

			return ZString.Empty;
		}

		public abstract ForwardingConsol GetCurrentConsol();
		protected abstract IEnumerable<ForwardingShipment> GetTopLevelShipments();

		protected virtual ZString RunPreSendDataValidationCore(MessageType messageType)
		{
			return ZString.Empty;
		}

		bool HasDataValidationErrors
		{
			get
			{
				ValidateAllData();

				return Data.HasMessageErrors()
					|| Data.HasErrors()
					|| HasDataValidationErrorsCore;
			}
		}

		protected virtual bool HasDataValidationErrorsCore
		{
			get { return false; }
		}

		void ValidateAllData()
		{
			Data.ValidateAll();
			ValidateAllDataCore();
		}

		protected virtual void ValidateAllDataCore()
		{
		}

		protected string HarmonizedCodeMandatoryForAUSError
		{
			get
			{
				return Res.GetString("72cfa35b-b66f-450d-b47b-f8bae60c0563", "All Pack Lines must have Harmonized Code entered for Entry Type '{0}' ({1})",
					EntryTypeList.Codes.EmergencyConcept, EntryTypeList.Descriptions.EmergencyConcept);
			}
		}

		protected bool HasInvalidUNDGDataItems(ForwardingShipment shipment)
		{
			var forwardingPackLines = shipment.OuterPackLines.Cast<ForwardingPackLine>();
			return forwardingPackLines.Any(packLine => packLine.UNDGs.Count > 1 && packLine.UNDGs.Any(c => c.PackType == null || c.DI_PackageCount.IsEmpty));
		}

		#endregion

		#region Implementation

		protected void NotifyDataChanged()
		{
			var handler = DataChanged;

			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Messaging Status Retriever

		public PortMessagingStatusRetriever StatusRetriever
		{
			get { return statusRetriever ?? (statusRetriever = new PortMessagingStatusRetriever(MessageOriginator)); }
		}
		PortMessagingStatusRetriever statusRetriever;

		#endregion
	}
}

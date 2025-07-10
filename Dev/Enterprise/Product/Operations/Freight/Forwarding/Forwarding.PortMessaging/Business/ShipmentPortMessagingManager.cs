using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class ShipmentPortMessagingManager : PortMessagingManager
	{
		public ShipmentPortMessagingManager(ForwardingShipment shipment)
			: base(shipment)
		{
			Shipment.Consols.CountChanged += OnCountChanged;
		}

		ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)MessageOriginator; }
		}

		public override PortMessagingData Data
		{
			get
			{
				ForwardingConsol consol = GetFirstValidConsolForDakosyPortMessaging(Shipment);
				if (data == null && consol != null)
				{
					data = new ShipmentPortMessagingData(consol, Shipment);
				}

				return data;
			}
		}

		PortMessagingData data;

		#region SZB

		public override ZString SZBNumber
		{
			get { return Shipment.JS_SZB; }
		}

		public override ZString SZBInformation
		{
			get { return Shipment.JS_SZBInformation; }
		}

		public override ZDateTime SZBIssueDate
		{
			get { return Shipment.JS_SZBIssueDate; }
		}

		#endregion

		public override bool ShouldShowPortMessagingForDakosy => IsValidShipmentForDakosyPortMessaging(Shipment);

		public static bool IsValidShipmentForDakosyPortMessaging(ForwardingShipment shipment)
		{
			return shipment != null && shipment.IsSea && GetFirstValidConsolForDakosyPortMessaging(shipment) != null;
		}

		public override ZString CheckPortMessagingAvailability()
		{
			if (Shipment == null)
			{
				return Res.GetString("494aaa8c-ed49-4628-b80f-d8c385f78753", "Could not load this Shipment for Port Messaging. Please try closing and reopening this Shipment to access Port Messaging.");
			}
			else if (!Env.Security.PortMessaging.IsAllowed)
			{
				return Env.Security.PortMessaging.ErrorMessageForNotAllowed;
			}
			else if (!Shipment.IsSea || GetFirstValidConsolForDakosyPortMessaging(Shipment) == null)
			{
				return Res.GetString("fa9a18e4-8e56-495d-b883-f0b6ffbdd70b", "Port Order to DAKOSY can only be sent from a sea Shipment linked to Consol with a sea leg loading or discharging in Hamburg.");
			}
			else if (Shipment.IsAssemblyMaster)
			{
				return Res.GetString("e6993f3c-8505-48ed-84b7-81342631210c", "Port Order to DAKOSY can't be sent from an Assembly master shipment.");
			}
			else if (Shipment.HasMaster(Constants.ShipmentTypes.CoLoadMaster))
			{
				return Res.GetString("b16d57ba-9637-4f58-a952-128fc575d697", "Port Order to DAKOSY can't be sent from a Co-Load sub-shipment.");
			}

			return ZString.Empty;
		}

		#region Implementation

		protected override ShipmentPortMessaging GetShipmentPortMessaging()
		{
			var portMessaging = ShipmentPortMessaging.LoadOrCreate(Shipment);
			Shipment.RegisterEditableChildObject(portMessaging);

			return portMessaging;
		}

		protected override ForwardingPackLineWithPortMessagingCollection CreatePackLinesCollection()
		{
			var packlines = new ForwardingPackLineWithPortMessagingCollection(Shipment);
			packlines.CurrentConsol = GetFirstValidConsolForDakosyPortMessaging(Shipment);
			RegisterEditableChildObject(packlines);

			return packlines;
		}

		public override ForwardingConsol GetCurrentConsol()
		{
			return GetFirstValidConsolForDakosyPortMessaging(Shipment);
		}

		protected override IEnumerable<ForwardingShipment> GetTopLevelShipments()
		{
			return new[] { Shipment };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override ZString RunPreSendDataValidationCore(MessageType messageType)
		{
			var isHDS = IsHDS(messageType);

			if (isHDS)
			{
				if (DoesEntryTypeNeedMRN
					&& PortMessaging.JSM_MovementReferenceNumber.IsEmpty
					&& PackLines.OfType<ForwardingPackLineWithPortMessaging>().All(p => p.PortMessaging.JLM_MovementReferenceNumber.IsEmpty))
				{
					return Res.GetString("3a9e1ac8-9b96-4319-bd75-74224ac866ae", "MRN number must be entered before you can send Port Order.");
				}

				if (PortMessaging.JSM_EntryType.IsEmpty
					&& PackLines.OfType<ForwardingPackLineWithPortMessaging>().Any(p => p.PortMessaging.JLM_EntryType.IsEmpty))
				{
					return Res.GetString("d11df565-4fcc-4667-870c-fe35d28282a0", "Entry Type must be entered on either shipment or all pack lines before you can send Port Order.");
				}
				if (IsExport && IsShipmentLCL)
				{
					var currentConsolidation = GetCurrentConsol();
					if (currentConsolidation != null)
					{
						if (currentConsolidation.IsCoLoad)
						{
							if (currentConsolidation.JK_CoLoadBookingReference.IsEmpty && Shipment.BKGNumber.IsEmpty)
							{
								Data.BookingReferenceInfo.AddMessageError(Res.GetString(
									"97f07e65-169d-4e1d-b0b8-10f78826a8bd",
									"Co-Load Booking Reference is required for LCL shipments when sending the Port Order with HDS and Consol type is Co-Load. Consol -> Co-Load Booking Reference."));
							}
						}
						else
						{
							if (currentConsolidation.JK_BookingReference.IsEmpty)
							{
								Data.BookingReferenceInfo.AddMessageError(Res.GetString(
									"494e6eab-7546-456b-9681-0077a7151b43",
									"Carrier Booking Reference is required for LCL shipments when sending the Port Order with HDS. Consol -> Carrier Booking Reference."));
							}
						}
					}
				}
			}

			if (messageType == MessageType.PortOrderWithHDSForwardingCancellation && PortMessaging.JSM_ForwardingCustomsOfficeCode.IsEmpty)
			{
				return Res.GetString("c486c7ee-a4e5-4c04-ba55-0d5b21bab0c9", "Forwarding Office ID must be entered before you can send Port Order.");
			}

			if ((messageType == MessageType.PortOrderWithHDSForwardingCancellation
				|| messageType == MessageType.PortOrderWithHDSCancellationOnExit
				|| messageType == MessageType.PortOrderWithHDSCancellationBecauseOfErrors)
				&& SZBNumber.IsEmpty)
			{
				return Res.GetString("1b922d03-f289-4fa2-948c-9acb6ec00587", "Cancellation or Forwarding may only be sent if the SZB number has already been received.");
			}
			if (!PortMessaging.JSM_MovementReferenceNumberComplete)
			{
				if (messageType == MessageType.PortOrderWithHDSCancellationOnExit
					&& !PackLines.Cast<ForwardingPackLineWithPortMessaging>()
						.Any(p => p.PortMessaging.JLM_MovementReferenceNumberComplete))
				{
					return Res.GetString("1a5ddfac-adb2-43bd-aba1-df7f4c474066", "At least one MRN must be marked as complete before you can send Cancellation on Exit.");
				}

				if (messageType == MessageType.PortOrderWithHDSForwardingCancellation
					&& !PackLines.Cast<ForwardingPackLineWithPortMessaging>()
						.All(p => p.PortMessaging.JLM_MovementReferenceNumberComplete))
				{
					return Res.GetString("1ec87a3b-5169-4745-a9eb-7ea927f99cf6", "All MRNs being sent must be marked as complete before you can send Forwarding Cancellation.");
				}
			}

			if (Data.IsAUSEntryType() && PackLines.Cast<ForwardingPackLineWithPortMessaging>().Any(x => x.JL_HarmonisedCode.IsEmpty))
			{
				return HarmonizedCodeMandatoryForAUSError;
			}

			if (HasInvalidUNDGDataItems(Shipment))
			{
				return Res.GetString("621e4bbe-6d36-456d-847c-51a0d2896305",
					"Multiple Dangerous Goods are recorded against a pack line on a shipment {0}, please enter Package Count and Package Type for each DG recorded via Shipment > Packing > Pack line > Right click menu > Dangerous Goods",
					Shipment.JS_UniqueConsignRef);
			}

			return ZString.Empty;
		}

		IEnumerable<string> EntryTypeNeedsMRN
		{
			get
			{
				yield return EntryTypeList.Codes.AESExportDeclaration;
				yield return EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities;
				yield return EntryTypeList.Codes.ExitSummaryDeclaration;
			}
		}

		bool DoesEntryTypeNeedMRN
		{
			get { return EntryTypeNeedsMRN.Any(n => n == PortMessaging.JSM_EntryType) || (Data.IsMITEntryType() && DoesMITNeedMRN); }
		}

		bool IsShipmentLCL => Shipment.JS_PackingMode == Constants.ContainerModes.LCL;

		IEnumerable<string> Annex30ATypesThatRequireMRNForMIT
		{
			get
			{
				yield return Annex30ATypeList.Codes.AlreadyCompleted;
				yield return Annex30ATypeList.Codes.SeparateEntry;
				yield return Annex30ATypeList.Codes.TransportDeclaration;
			}
		}

		bool DoesMITNeedMRN
		{
			get { return Annex30ATypesThatRequireMRNForMIT.Any(n => n == PortMessaging.JSM_Annex30AType); }
		}

		protected override void ValidateAllDataCore()
		{
			PortMessaging.Validation.ValidateAll();
			PortMessaging.RefreshBinding();

			foreach (ForwardingPackLineWithPortMessaging packline in PackLines)
			{
				packline.PortMessaging.Validation.ValidateAll();
				packline.PortMessaging.RefreshBinding();
			}
		}

		protected override bool HasDataValidationErrorsCore
		{
			get
			{
				return PortMessaging.HasErrors()
					|| PortMessaging.HasMessageErrors()
					|| PackLines.HasErrors()
					|| PackLines.HasMessageErrors();
			}
		}

		static ForwardingConsol GetFirstValidConsolForDakosyPortMessaging(ForwardingShipment shipment)
		{
			return shipment.Consols.Cast<ForwardingConsol>().FirstOrDefault(c => ConsolPortMessagingManager.IsValidConsolForDakosyPortMessaging(c));
		}

		void OnCountChanged(object sender, CollectionCountChangedEventArgs collectionCountChangedEventArgs)
		{
			if (data != null && Shipment != null && Shipment.Consols.Contains(data.Consol))
			{
				data = null;
				NotifyDataChanged();
			}
		}

		public override bool IsExport
		{
			get { return Shipment.Consols.Cast<ForwardingConsol>().Any(c => ConsolPortMessagingManager.IsExportConsolForDakosy(c)); }
		}

		public override bool IsImport
		{
			get { return Shipment.Consols.Cast<ForwardingConsol>().Any(c => ConsolPortMessagingManager.IsImportConsolForDakosy(c)); }
		}

		public override bool HasDG
		{
			get { return Shipment.OuterPackLines.HasDangerousGoods; }
		}

		#endregion
	}
}

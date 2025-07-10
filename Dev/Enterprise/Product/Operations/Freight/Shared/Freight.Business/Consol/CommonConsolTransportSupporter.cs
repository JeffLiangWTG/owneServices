using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public class CommonConsolTransportSupporter<T> : TransportSupporter<T>
		where T : CommonConsol
	{
		public CommonConsolTransportSupporter(T consol)
			: base(consol) { }

		#region Container Mode

		public override ZString ContainerMode
		{
			get { return Parent.JK_ConsolMode; }
		}

		public override bool IsArrivalContainerModeFCLorULD
		{
			get
			{
				return base.IsArrivalContainerModeFCLorULD || Parent.JK_ConsolMode == Constants.ContainerModes.BuyersConsol;
			}
		}

		#endregion

		public override void SetConsignmentRefIfNotSet()
		{
			Parent.PopulateJK_UniqueConsignRefIfNeeded();
		}

		public override ZString Description
		{
			get { return Parent.JK_UniqueConsignRef; }
		}

		public override ZString ConsignmentRef
		{
			get { return Parent.JK_UniqueConsignRef; }
		}

		public override ZString TransportMode
		{
			get { return Parent.JK_TransportMode; }
		}

		public override ZString BillOfLading
		{
			get { return Parent.JK_MasterBillNum; }
		}

		public override bool BillOfLadingHasChanges
		{
			get { return !Parent.IsInDatabase || Parent.JK_MasterBillNumInfo.HasChanges; }
		}

		public override ZGuid ShippingLine
		{
			get { return Parent.ShippingLinePK; }
			set { Parent.SetDefaultShippingLineAddress(value); }
		}

		public override JobConsolTransportValidation GetNewTransportValidator(Transport transport)
		{
			return new ConsolTransportValidation(transport);
		}
		protected override void ETASetFromSailingCore(Transport transport, ZDateTime oldValue, ZDateTime newValue)
		{
			base.ETASetFromSailingCore(transport, oldValue, newValue);

			if (oldValue != newValue && transport.PK == Parent.Transports.ArrivalTransport.PK)
			{
				Parent.Shipments.UpdateShipmentsETA(newValue, oldValue);
			}
		}
		protected override void ETDSetFromSailingCore(Transport transport, ZDateTime oldValue, ZDateTime newValue)
		{
			base.ETDSetFromSailingCore(transport, oldValue, newValue);

			if (oldValue != newValue && transport.PK == Parent.Transports.DepartureTransport.PK)
			{
				Parent.Shipments.UpdateShipmentsETD(newValue, oldValue);
			}
		}

		protected override void NotifySailingChangedCore(Transport transport, ZGuid previousValue)
		{
			base.NotifySailingChangedCore(transport, previousValue);

			if (transport.JW_IsLinked)
			{
				if (transport.JW_RL_NKLoadPort == Parent.JK_RL_NKLoadPort)
				{
					Parent.DefaultDepartureCTOAddressFromSailing(transport.Sailing);
				}

				if (transport.JW_RL_NKDiscPort == Parent.JK_RL_NKDischargePort)
				{
					Parent.DefaultArrivalCTOAddressFromSailing(transport.Sailing);
				}

				if (transport.IsSea)
				{
					SetShippingLine(transport);
				}
			}
		}

		protected override void NotifyLoadChangedCore(Transport transport, ZString previousValue)
		{
			Parent.JK_JX_JA_RL_NKPortOfLoadingInfo.RefreshBinding();
		}

		protected override void NotifyDischargeChangedCore(Transport transport, ZString previousValue)
		{
			Parent.JK_JX_JB_RL_NKPortOfDischargeInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForLastImportTransportInfo.RefreshBinding();

			foreach (CommonContainer container in Parent.Containers)
			{
				ContainerEventDataVendor.Instance.NotifyDischargePortChanged(container);
			}
		}

		protected override void NotifyVesselChangedCore(Transport transport, ZString previousValue)
		{
			Parent.JK_JX_JV_NKVesselInfo.RefreshBinding();
			Parent.JK_VesselOfLastImportTransportInfo.RefreshBinding();

			SetShippingLine(transport);

			foreach (CommonContainer container in Parent.Containers)
			{
				ContainerEventDataVendor.Instance.NotifyVesselChanged(container);
			}
		}

		protected override void NotifyVoyageFlightChangedCore(Transport transport, ZString previousValue)
		{
			Parent.JK_JX_JV_VoyageFlightInfo.RefreshBinding();
			Parent.JK_VoyageOfLastImportTransportInfo.RefreshBinding();

			if (Parent.IsAir && !transport.JW_VoyageFlight.IsEmpty && AirlinePrefixChanged(previousValue, transport.JW_VoyageFlight))
			{
				if (Parent.ShouldDefaultFlightDetailsFrom(transport) && !IsAnyFlightMatchesConsolCarrier())
				{
					SetShippingLineForAirline();
				}
				else if (transport.IsAir)
				{
					RefAirline airline = GetAirlineFromFlightNumber(Parent.Factory, transport.JW_VoyageFlight);
					if (airline != null)
					{
						Parent.UpdateTransportCarrierFromAirline(transport, airline);
					}
				}
			}

			foreach (CommonContainer container in Parent.Containers)
			{
				ContainerEventDataVendor.Instance.NotifyVoyageChanged(container);
			}
		}

		protected override void NotifyETDChangedCore(Transport transport, ZDateTime previousValue)
		{
			Parent.JK_JX_JA_E_DEPInfo.RefreshBinding();

			if (transport.JW_ETD.IsValid)
			{
				bool isFirstETD = true;
				foreach (Transport otherTransport in Parent.Transports)
				{
					if (otherTransport.JW_ETD.IsValid && otherTransport.JW_ETD < transport.JW_ETD && otherTransport.PK != transport.PK)
					{
						isFirstETD = false;
					}
				}

				if (isFirstETD)
				{
					Parent.Shipments.NotifyETDChanged(previousValue, transport.JW_ETD);
				}
			}

			foreach (CommonContainer container in Parent.Containers)
			{
				ContainerEventDataVendor.Instance.NotifyETDChanged(container, transport.JW_ETDInfo);
			}
		}

		protected override void NotifyETAChangedCore(Transport transport, ZDateTime previousValue)
		{
			Parent.JK_JX_JB_E_ARVInfo.RefreshBinding();

			if (transport.JW_ETA.IsValid)
			{
				bool isLastETA = true;
				foreach (Transport otherTransport in Parent.Transports)
				{
					if (otherTransport.JW_ETA.IsValid && otherTransport.JW_ETA > transport.JW_ETA && otherTransport.PK != transport.PK)
					{
						isLastETA = false;
					}
				}

				if (isLastETA)
				{
					Parent.Shipments.NotifyETAChanged(previousValue, transport.JW_ETA);
				}
			}

			foreach (CommonContainer container in Parent.Containers)
			{
				ContainerEventDataVendor.Instance.NotifyETAChanged(container, transport.JW_ETAInfo);
			}
		}

		protected override void NotifyATDChangedCore(Transport transport, ZDateTime previousValue)
		{
			Parent.JK_JX_JA_A_DEPInfo.RefreshBinding();

			foreach (CommonContainer container in Parent.Containers)
			{
				container.ContainerPenaltyCalculateHandlers.ForEach(handler => handler.HandleTransportATDChanged());
			}
		}

		protected override void NotifyATAChangedCore(Transport transport, ZDateTime previousValue)
		{
			Parent.JK_JX_JB_A_ARVInfo.RefreshBinding();

			foreach (CommonContainer container in Parent.Containers)
			{
				container.ContainerPenaltyCalculateHandlers.ForEach(handler => handler.HandleTransportATAChanged());
			}
		}

		protected override void NotifyTerminalAvailabilityDateChangedCore(Transport transport, ZDateTime previousValue)
		{
			foreach (CommonContainer container in Parent.Containers)
			{
				container.JC_FCLAvailableInfo.RefreshBinding();
			}

			foreach (CommonContainer container in Parent.Containers)
			{
				container.ContainerPenaltyCalculateHandlers.ForEach(handler => handler.HandleContainerAdded());
			}
		}

		protected override void NotifyTerminalStorageDateChangedCore(Transport transport, ZDateTime previousValue)
		{
			foreach (CommonContainer container in Parent.Containers)
			{
				container.ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.TransportDepotStorageDate);
				container.JC_ArrivalCTOStorageStartDateInfo.RefreshBinding();
			}
		}

		protected override void NotifyDepotAvailabilityDateChangedCore(Transport transport, ZDateTime previousValue)
		{
			foreach (CommonContainer container in Parent.Containers)
			{
				container.JC_LCLAvailableInfo.RefreshBinding();
			}
		}

		protected override void NotifyDepotStorageDateChangedCore(Transport transport, ZDateTime previousValue)
		{
			foreach (CommonContainer container in Parent.Containers)
			{
				container.JC_LCLStorageCommencesInfo.RefreshBinding();
			}
		}

		protected override void NotifyCarrierAddressChangedCore(Transport transport, ZGuid previousValue)
		{
			if (Parent.JK_OA_ShippingLineAddress.IsEmpty && !((ISupportDataImporting)Parent).IsImportingData)
			{
				Parent.JK_OA_ShippingLineAddress = transport.JW_OA_CarrierAddress;
			}

			Parent.Validation.ValidateJK_OA_ShippingLineAddress();
		}

		protected override void NotifyCarrierBookingRefChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyCarrierBookingRefChangedCore(transport, previousValue);

			if (Parent.JK_BookingReference.IsEmpty)
			{
				Parent.JK_BookingReference = transport.JW_CarrierBookingReference;
			}
		}

		protected override void NotifyVoyageUpdatedCore(Transport transport)
		{
			foreach (CommonContainer container in Parent.Containers)
			{
				container.Logs.CreateRecreateOrUpdateEventLog(
					Events.CargoAvailable,
					EstimateActual.Actual,
					container.AvailableDate.ToOffset(),
					ZString.Empty,
					container.GetParametersForEvent(Events.CargoAvailable).ToArray());
			}

			foreach (CommonShipment shipment in Parent.Shipments)
			{
				if (shipment.DocsAndCartage != null)
				{
					shipment.Logs.CreateRecreateOrUpdateEventLog(
						Events.CargoAvailable,
						EstimateActual.Actual,
						shipment.DocsAndCartage.AvailableDate.ToOffset(),
						ZString.Empty,
						shipment.GetParametersForEvent(Events.CargoAvailable).ToArray());
				}
			}
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		#region Implementation

		protected bool IsAnyFlightMatchesConsolCarrier()
		{
			if (Parent.ShippingLine != null && Parent.ShippingLine.MiscServ != null && Parent.ShippingLine.MiscServ.Airline != null)
			{
				ZString airlinePrefix = Parent.ShippingLine.MiscServ.Airline.RM_TwoCharacterCode;
				ZString carrierCountry = Parent.ShippingLine.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin;

				if (GlbBranch.CurrentBranch.Country.RN_Code == carrierCountry)
				{
					foreach (Transport transport in Parent.Transports)
					{
						if (transport.IsAir && transport.JW_VoyageFlight.SubstringSafe(0, 2) == airlinePrefix)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		protected bool AirlinePrefixChanged(ZString oldValue, ZString newValue)
		{
			return oldValue.SubstringSafe(0, 2) != newValue.SubstringSafe(0, 2);
		}

		protected void SetShippingLine(Transport transport)
		{
			if (!((ISupportDataImporting)Parent).IsImportingData)
			{
				Transport psudoMain = Parent.Transports.FirstTransportWithTransportMode(Parent.JK_TransportMode);
				if (psudoMain != null && transport.PK == psudoMain.PK)
				{
					OrgHeader shippingLine = null;
					if (transport.Voyage != null)
					{
						shippingLine = transport.Voyage.Line;
					}
					else if (transport.Vessel != null && Parent.ShippingLinePK.IsEmpty)
					{
						shippingLine = transport.Vessel.Header;
					}

					if (shippingLine != null && Parent.ShippingLinePK != shippingLine.PK)
					{
						Parent.SetDefaultShippingLineAddress(shippingLine);
					}
				}
			}
		}

		RefAirline GetAirlineFromFlightNumber(BusinessObjectFactory factory, ZString flightNumber)
		{
			string voyagePrefix = flightNumber.SubstringSafe(0, 2);
			return RefAirline.LoadFromAirline2LetterCode(factory, voyagePrefix);
		}

		void SetShippingLineForAirline()
		{
			RefAirline airline = GetAirlineFromFlightNumber(Parent.Factory, Parent.JK_JX_JV_VoyageFlight);
			if (airline != null)
			{
				Parent.SetShippingLineBaseOnAirline(airline);
			}
		}

		#endregion
	}
}

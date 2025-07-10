using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolTransportSupporter<T> : CommonConsolTransportSupporter<T>
		where T : ForwardingConsol
	{
		public ForwardingConsolTransportSupporter(T consol)
			: base(consol) { }

		void UpdateLoadPortAndDischargePort(Transport transport)
		{
			if (Parent.AirCargoSynchroniser != null)
			{
				Parent.AirCargoSynchroniser.UpdateLoadPort(transport);
				Parent.AirCargoSynchroniser.UpdateDischargePort(transport);
			}
		}

		protected override void NotifyLegOrderChangedCore(Transport transport, ZByte previousValue)
		{
			base.NotifyLegOrderChangedCore(transport, previousValue);
			Parent.JK_RL_NKLoadForFirstImportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForFirstImportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForLastImportTransportInfo.RefreshBinding();
			UpdateLoadPortAndDischargePort(transport);
		}

		protected override void NotifyTransportTypeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyTransportTypeChangedCore(transport, previousValue);

			bool isTypeChangedToFlight1 = (transport.JW_TransportType == Constants.TransportPlanningType.Flight1) && (previousValue != Constants.TransportPlanningType.Flight1);
			if (isTypeChangedToFlight1 && ShouldDefaultFlightDetailsFrom(transport) && !IsAnyFlightMatchesMAWB())
			{
				DefaultMasterBillAirlinePrefix();
			}
		}

		protected override void NotifyVoyageFlightChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyVoyageFlightChangedCore(transport, previousValue);

			if (ShouldDefaultFlightDetailsFrom(transport))
			{
				if (!IsAnyFlightMatchesMAWB() && Parent.MasterBillMAWB.IsEmpty)
				{
					DefaultMasterBillAirlinePrefix();
				}
				if (Parent.AirCargoSynchroniser != null)
				{
					Parent.AirCargoSynchroniser.FlightNumber = transport.JW_VoyageFlight;
				}
				Parent.DefaultSpecialHandlingItems();
			}

			Parent.AutoFillCRN(transport);

			Parent.Validation.ValidateMasterBillAirlinePrefix();
		}

		protected override void NotifyLoadChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyLoadChangedCore(transport, previousValue);

			Parent.JK_RL_NKLoadForExportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKLoadForImportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKLoadForFirstImportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForFirstImportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForLastImportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForExportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForImportTransportInfo.RefreshBinding();

			if (Parent.ShouldDefaultFlightDetailsFrom(transport))
			{
				if (!IsAnyFlightMatchesMAWB())
				{
					if (!ImportExportHelper.IsBranchCountry(transport.JW_RL_NKLoadPort) && !ImportExportHelper.IsExport(transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort))
					{
						Parent.JK_IsNeutralMaster = false;
					}

					Parent.MAWBAllocation.MarkForReallocation(checkIsNeutralAndNotPrinted: true, checkIsInDatabase: true);
					Parent.JK_IsNeutralMasterInfo.RefreshBinding();
				}
			}

			UpdateLoadPortAndDischargePort(transport);
			Parent.AutoFillCRN(transport);

			if (previousValue.SubstringSafe(0, 2) != transport.JW_RL_NKLoadPort.SubstringSafe(0, 2))
			{
				RecalculateShipmentInspectionTypes(Res.GetString("659a44cc-5d1c-4c24-b914-d810b5e7069d", "{0} has been changed", Parent.JK_RL_NKLoadPortInfo.HumanReadableName));
			}
		}

		protected override void NotifyTransportModeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyTransportModeChangedCore(transport, previousValue);

			if (previousValue == TransportModes.Air || transport.JW_TransportMode == TransportModes.Air)
			{
				RecalculateShipmentInspectionTypes(Res.GetString("5a4ed2cd-9a61-4e06-8946-963dcdd8a130", "{0} has been changed", Parent.JK_TransportModeInfo.HumanReadableName));
			}
		}

		protected override void NotifySailingChangedCore(Transport transport, ZGuid previousValue)
		{
			base.NotifySailingChangedCore(transport, previousValue);

			if (ShouldDefaultFlightDetailsFrom(transport)
				&& transport.Sailing != null
				&& !IsImporting(transport.Sailing))
			{
				if (!IsAnyFlightMatchesMAWB() && Parent.MasterBillMAWB.IsEmpty)
				{
					DefaultMasterBillAirlinePrefix();
				}

				if (Parent.AirCargoSynchroniser != null)
				{
					Parent.AirCargoSynchroniser.FlightNumber = transport.JW_VoyageFlight;
				}
			}

			Parent.AutoFillCRN(transport);
		}

		protected override void NotifyVesselChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyVesselChangedCore(transport, previousValue);

			Parent.AutoFillCRN(transport);
		}

		protected override void NotifyDischargeChangedCore(Transport transport, ZString previousValue)
		{
			base.NotifyDischargeChangedCore(transport, previousValue);

			Parent.JK_RL_NKLoadForExportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKLoadForImportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKLoadForFirstImportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForFirstImportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForLastImportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForExportTransportInfo.RefreshBinding();
			Parent.JK_RL_NKDiscForImportTransportInfo.RefreshBinding();

			UpdateLoadPortAndDischargePort(transport);
			Parent.AutoFillCRN(transport);
		}

		protected override void NotifyETAChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyETAChangedCore(transport, previousValue);

			if (Parent.ShouldDefaultFlightDetailsFrom(transport))
			{
				if (Parent.AirCargoSynchroniser != null)
				{
					Parent.AirCargoSynchroniser.ArrivalDate = transport.JW_ETA;
				}
			}

			Parent.AutoFillCRN(transport);
		}

		protected override void NotifyETDChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyETDChangedCore(transport, previousValue);

			if (transport.Parent is ForwardingConsol consol && consol.Validation != null)
			{
				consol.Validation.ValidateJK_RCA_AllocationLine();
			}

			Parent.AutoFillCRN(transport);
			if (Parent.AirCargoSynchroniser != null)
			{
				Parent.AirCargoSynchroniser.DepartureDate = Parent.JK_DepartureForTheFirstInternationalLeg;
			}

			RecalculateShipmentInspectionTypes(Res.GetString("746aa78a-307f-48d9-8bfc-b1ac16bd4718", "{0} has been changed", transport.JW_ETDInfo.HumanReadableName));
		}

		protected override void NotifyATAChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyATAChangedCore(transport, previousValue);
			Parent.AutoFillCRN(transport);
		}

		protected override void NotifyATDChangedCore(Transport transport, ZDateTime previousValue)
		{
			base.NotifyATDChangedCore(transport, previousValue);
			Parent.AutoFillCRN(transport);
			if (Parent.AirCargoSynchroniser != null)
			{
				Parent.AirCargoSynchroniser.DepartureDate = Parent.JK_DepartureForTheFirstInternationalLeg;
			}
		}

		#region Implementation

		bool ShouldDefaultFlightDetailsFrom(Transport transport)
		{
			return Parent.ShouldDefaultFlightDetailsFrom(transport)
				&& Parent.IsAir
				&& !Parent.MAWBAllocation.IsMAWBPrinted
				&& !IsImporting(Parent);
		}

		bool IsAnyFlightMatchesMAWB()
		{
			if (!Parent.MasterBillAirlinePrefix.IsEmpty)
			{
				RefAirline airline = RefAirline.LoadFromAirlinePrefix(Parent.Factory, Parent.MasterBillAirlinePrefix);
				if (airline != null)
				{
					foreach (Transport transport in Parent.Transports)
					{
						if (transport.IsAir && transport.JW_VoyageFlight.SubstringSafe(0, 2) == airline.RM_TwoCharacterCode)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		void DefaultMasterBillAirlinePrefix()
		{
			if (Parent.IsMultiAWBMaster)
			{
				Parent.MasterBillAirlinePrefix = ZString.Empty;
				return;
			}

			ZString code = "";

			foreach (Transport transport in Parent.Transports)
			{
				if (transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1)
				{
					if (code == "")
					{
						code = transport.JW_VoyageFlight.SubstringSafe(0, 2);
					}
					else if (!transport.JW_VoyageFlight.StartsWith(code, StringComparison.Ordinal))
					{
						code = "";
						break;
					}
				}
			}

			if (!code.IsEmpty)
			{
				RefAirline airline = RefAirline.LoadFromAirline2LetterCode(Parent.Factory, code);
				if (airline != null && Parent.MasterBillAirlinePrefix != airline.RM_EagleAddedAirlinePrefixOrAccountingCode)
				{
					Parent.MasterBillAirlinePrefix = airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
				}
			}
		}

		void RecalculateShipmentInspectionTypes(ZString reason)
		{
			if (!IsImporting(Parent)
				&& Parent.JK_TransportMode == TransportModes.Air)
			{
				foreach (ForwardingShipment shipment in Parent.Shipments)
				{
					if (shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(shipment.JS_InspectionTypeCode))
					{
						shipment.SetApprovedShipperStatus(reason);
						shipment.ReDefaultPackLineInspectionTypeCodes();
					}
				}
			}
		}

		bool IsImporting(ISupportDataImporting bo)
		{
			return bo.IsImportingData;
		}

		#endregion
	}
}

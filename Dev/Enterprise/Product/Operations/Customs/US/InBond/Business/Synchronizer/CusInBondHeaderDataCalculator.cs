using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	abstract class CusInBondHeaderDataCalculator
	{
		protected readonly CusInBondHeader header;

		public CusInBondHeaderDataCalculator(CusInBondHeader header)
		{
			this.header = Argument.NotNull(header, "header");
		}

		protected JobDeclaration Declaration => header.Declaration;

		public abstract ForwardingConsol RelevantConsol { get; }

		protected abstract BusinessObjectFactory SourceFactory { get; }

		#region MasterBill

		public ZString MasterBill
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;
				if (declaration != null)
				{
					result = declaration.JE_MasterBill;
				}
				else
				{
					var consol = RelevantConsol;
					if (consol != null)
					{
						result = consol.JK_MasterBillNum;
						if (ShouldSplitBillNum(result))
						{
							result = result.GetBillNumberTrimSCAC();
						}
					}
				}

				return result.KeepAlphanumericCharacters();
			}
		}

		bool ShouldSplitBillNum(ZString billNumber)
		{
			var validSCACs = ((IBillDetails)RelevantConsol).SCACIssuers.GetValidSCACs(RelevantConsol.TransportMode);
			return InBondTransportModeCodes.IsSeaOrRail(InBondTransportMode)
					&& billNumber.ShouldTrimSCACFromBills(validSCACs);
		}

		protected abstract ZString InBondTransportMode { get; }

		#endregion

		#region MasterBillIssuerCode

		public ZString MasterBillIssuerCode
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;
				if (declaration != null)
				{
					result = declaration.JE_MasterBillIssuerSCAC;
				}
				else
				{
					var consol = RelevantConsol;
					if (consol != null)
					{
						var billNumber = consol.JK_MasterBillNum;
						var validSCACs = consol.GetValidSCACIssuerCodes(consol.TransportMode);
						result = billNumber.GetSCAC(validSCACs);
					}
				}
				return result;
			}
		}

		#endregion

		#region BH_ImportTransportMode

		public ZString GetInBondModeFromTransportAndPacking()
		{
			var result = ZString.Empty;
			(var transportMode, var packingMode) = GetTransportAndPacking();
			switch (transportMode)
			{
				case Constants.TransportModes.Air:
				case Constants.TransportModes.AirSea:
					result = InBondTransportModeCodes.Codes.AirNonContainer;
					break;

				case Constants.TransportModes.Sea:
				case Constants.TransportModes.SeaAir:
					result = IsContainerizedPacking(packingMode) ? InBondTransportModeCodes.Codes.VesselContainer : InBondTransportModeCodes.Codes.VesselNonContainer;
					break;

				case Constants.TransportModes.Rail:
					result = IsContainerizedPacking(packingMode) ? InBondTransportModeCodes.Codes.RailContainer : InBondTransportModeCodes.Codes.RailNonContainer;
					break;

				case Constants.TransportModes.Road:
					result = IsContainerizedPacking(packingMode) ? InBondTransportModeCodes.Codes.TruckContainer : InBondTransportModeCodes.Codes.TruckNonContainer;
					break;

				default:
					result = ZString.Empty;
					break;
			}

			return result;
		}

		protected abstract (ZString transportMode, ZString packingMode) GetTransportAndPacking();

		bool IsContainerizedPacking(string packingMode)
		{
			return packingMode == Constants.ContainerModes.FCL || packingMode == Constants.ContainerModes.LCL || packingMode == Constants.ContainerModes.Containerised;
		}

		#endregion

		#region BH_CarrierSCAC

		protected abstract bool IsSourceAir { get; }

		protected abstract ForwardingConsol ArrivalConsol { get; }

		public ZString GetCarrierSCAC()
		{
			var result = ZString.Empty;
			var declaration = Declaration;
			if (declaration != null)
			{
				result = declaration.US_UI_NKCarrierSCAC;
			}
			else
			{
				if (IsSourceAir)
				{
					var flightNo = ZString.Empty;
					var transport = ConsolLegsToUS.FirstOrDefault(x => x.IsAir);
					if (transport != null)
					{
						flightNo = transport.JW_VoyageFlight;
					}
					var masterBillNum = ZString.Empty;
					var consol = RelevantConsol;
					if (consol != null)
					{
						masterBillNum = consol.JK_MasterBillNum;
					}
					result = GetAirCarrierCode(flightNo, masterBillNum);
				}

				if (result.IsEmpty)
				{
					result = CarrierOrg.USLocalCustomsCarrierCode(header.IsTruck);
				}
			}
			return result;
		}

		ZString GetAirCarrierCode(ZString flightNo, ZString masterBill)
		{
			var result = ZString.Empty;
			var scac = flightNo.SubstringSafe(0, 2);
			if (!scac.IsEmpty)
			{
				var query = new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, scac);
				query.AddToFilter(RefAirlineSchema.RM_MembershipFlagIATA, true);
				var isValidSCAC = SourceFactory.LoadTop1<RefAirline>(query) != null;
				if (isValidSCAC)
				{
					result = scac;
				}
			}
			if (result.IsEmpty)
			{
				var numericAirLineCode = masterBill.SubstringSafe(0, 3);
				if (!numericAirLineCode.IsEmpty)
				{
					var query = new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, numericAirLineCode);
					query.AddToFilter(RefAirlineSchema.RM_MembershipFlagIATA, true);
					var airline = SourceFactory.LoadTop1<RefAirline>(query);
					if (airline != null)
					{
						result = airline.RM_TwoCharacterCode;
					}
				}
			}

			return result;
		}

		OrgHeader CarrierOrg
		{
			get
			{
				if (carrierOrg == null)
				{
					var arrivalConsol = ArrivalConsol;
					if (arrivalConsol != null && arrivalConsol.JK_OA_ShippingLineAddress.IsValid)
					{
						var carrierOrgAddress = arrivalConsol.ShippingLineAddress;
						if (carrierOrgAddress != null)
						{
							carrierOrg = carrierOrgAddress.Header;
						}
					}
				}

				return carrierOrg;
			}
		}
		OrgHeader carrierOrg;

		#endregion

		#region BH_ImportConveyanceName

		public ZString GetConveyanceName()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				return declaration.JE_VesselName;
			}
			else
			{
				var transport = ConsolLegsToUS.FirstOrDefault();
				return transport != null ? transport.JW_Vessel : ZString.Empty;
			}
		}

		#endregion

		#region BH_VoyageNumber

		protected abstract ForwardingConsol InBondParentConsol { get; }

		public ZString GetVoyageFlight()
		{
			var result = ZString.Empty;
			var declaration = Declaration;
			if (declaration != null)
			{
				result = declaration.JE_VoyageFlightNo;
				if (declaration.IsAir)
				{
					result = GetFlightNumberWithoutSCAC(result);
				}
			}
			else
			{
				result = GetVoyageNumber();
			}
			return result;
		}

		ZString GetVoyageNumber()
		{
			var result = ZString.Empty;
			var consol = InBondParentConsol;
			if (consol != null && consol.Transports.Count > 0)
			{
				var transport = ConsolLegsToUS.FirstOrDefault();
				if (transport != null)
				{
					result = transport.JW_VoyageFlight;
					if (transport.IsAir)
					{
						result = GetFlightNumberWithoutSCAC(result);
					}
				}
			}

			return result;
		}

		ZString GetFlightNumberWithoutSCAC(ZString flightNumber)
		{
			var result = flightNumber;
			var scac = result.SubstringSafe(0, 2);
			var containsSCAC = false;
			if (!scac.IsEmpty)
			{
				containsSCAC = SourceFactory.Load<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, scac)).Length > 0;
			}
			if (containsSCAC)
			{
				result = result.SubstringSafe(2).PadLeft(3, '0');
			}
			return result;
		}

		#endregion

		internal IEnumerable<Transport> ConsolLegsToUS
		{
			get
			{
				var consol = RelevantConsol;
				if (consol != null)
				{
					consol.Transports.Load();
					foreach (var leg in consol.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder))
					{
						if (!leg.JW_RL_NKLoadPort.StartsWith(Constants.CountryCodes.UnitedStates) && leg.JW_RL_NKDiscPort.StartsWith(Constants.CountryCodes.UnitedStates))
						{
							yield return leg;
						}
					}
				}
			}
		}

		#region BH_ImportLoadPortKCode

		public ZString GetImportLoadingPort()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				return declaration.US_SchDLoading;
			}
			else
			{
				return USScheduleResolver.GetScheduleCode(Schedule.K, SourceLoadingPort, SourceTransportMode, SourceFactory);
			}
		}

		protected abstract ZString SourceTransportMode { get; }
		protected abstract ZString SourceLoadingPort { get; }

		#endregion

		#region BH_SailingDate

		public abstract ZDateTime GetSailingDate();

		#endregion

		#region BH_ETA

		public abstract ZDateTime GetETADate();

		#endregion

		#region BH_RN_NKFirstExportCountry

		public ZString GetExportCountry()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				return declaration.US_UC_NKCountryOfExport;
			}
			else
			{
				return SourceLoadingPort.SubstringSafe(0, 2);
			}
		}

		#endregion

		#region BH_FIRMS

		public ZString GetFIRMSCode()
		{
			var result = ZString.Empty;
			var declaration = Declaration;
			var consol = RelevantConsol;
			if (declaration != null)
			{
				result = declaration.US_US_NKLocationOfGoods;
			}
			else if (consol != null)
			{
				var firmsAddress = consol.UnpackDepotAddress ?? consol.ArrivalCTOAddress;
				if (firmsAddress != null)
				{
					result = firmsAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);
				}
			}
			return result;
		}

		#endregion

		#region BH_PortUnladingDCode

		public ZString GetPortUnladingDCode()
		{
			var result = ZString.Empty;
			var mostInterestingLeg = GetMostInterestingLeg();
			if (mostInterestingLeg != null)
			{
				result = USScheduleResolver.GetScheduleCode(Schedule.D, mostInterestingLeg.JW_RL_NKDiscPort, mostInterestingLeg.JW_TransportMode, header.Factory);
			}
			return result.Left(CusInBondHeader.Schema.BH_PortUnladingDCodeMaxLength);
		}

		Transport GetMostInterestingLeg()
		{
			var transports = GetSortedTransports();

			Transport mostInterestingLeg = null;
			foreach (var transport in transports)
			{
				if (mostInterestingLeg == null && transport.JW_RL_NKDiscPort.Left(2) == Constants.CountryCodes.UnitedStates)
				{
					mostInterestingLeg = transport;
				}

				if (mostInterestingLeg != null &&
					transport.JW_RL_NKLoadPort == mostInterestingLeg.JW_RL_NKDiscPort &&
					transport.JW_RL_NKDiscPort.Left(2) == Constants.CountryCodes.UnitedStates &&
					GetFallBackDate(transport.JW_ATD, transport.JW_ETD) >= GetFallBackDate(mostInterestingLeg.JW_ATA, mostInterestingLeg.JW_ETA))
				{
					mostInterestingLeg = transport;
				}
			}
			return mostInterestingLeg;
		}

		protected abstract List<Transport> Transports { get; }

		public List<Transport> GetSortedTransports()
		{
			var transports = Transports;
			transports.Sort((x, y) =>
			{
				return GetFallBackDate(x.JW_ATD, x.JW_ETD) > GetFallBackDate(y.JW_ATD, y.JW_ETD) ? 1 : -1;
			});
			return transports;
		}

		ZDateTime GetFallBackDate(ZDateTime actual, ZDateTime estimate)
		{
			return actual.IsEmpty ? estimate : actual;
		}

		#endregion
	}
}

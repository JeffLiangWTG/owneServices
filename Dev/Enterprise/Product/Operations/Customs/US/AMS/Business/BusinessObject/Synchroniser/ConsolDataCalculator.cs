using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class ConsolDataCalculator : Customs.Business.ConsolDataCalculator
	{
		public ConsolDataCalculator(ForwardingConsol consol, CusInBondHeader header)
			: base(consol, Core.Constants.CountryCodes.UnitedStates)
		{
			this.header = header;
			HookToHeaderEventsAffectingFirstCountryBoundTransportOrFirstTransportWithTransportModeCalculation();
		}
		readonly CusInBondHeader header;

		public ZString GetSCAC(OrgHeader org)
		{
			return org?.GetSCAC(consol.TransportMode) ?? ZString.Empty;
		}

		public ZString OrgProxySCAC
		{
			get
			{
				var result = ZString.Empty;

				var branch = header.Branch;
				var orgProxy = branch != null ? Factory.Load<OrgHeader>(branch.GB_OH_OrgProxy) : null;
				if (orgProxy != null)
				{
					result = GetSCAC(orgProxy);
				}
				if (result == ZString.Empty)
				{
					orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					result = orgProxy != null ? GetSCAC(orgProxy) : ZString.Empty;
				}
				return result;
			}
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingOrgProxySCAC()
		{
			yield return header.BH_GBInfo;
		}

		public ZString SendingForwarderSCAC
		{
			get { return GetSCAC(consol.SendingForwarder); }
		}

		public ZString BillOfLadingIssuerSCAC
		{
			get { return consol.IsCoLoad ? SendingForwarderSCAC : OrgProxySCAC; }
		}

		public ZString CarrierSCAC
		{
			get
			{
				var result = GetSCAC(consol.MasterBillIssuingParty);
				if (result.IsEmpty)
				{
					result = GetSCAC(consol.ShippingLine);
				}
				return result;
			}
		}

		public RefUNLOCO LastForeignPortOfLoading
		{
			get
			{
				var lastForeignPortOfLoad = consol.LastForeignPort;
				var firstUSBoundTransportOrDepartureTransport = FirstCountryBoundTransportOrFirstTransportWithTransportMode;
				if (firstUSBoundTransportOrDepartureTransport != null && (lastForeignPortOfLoad == null ||
					IsPortInTheCountry(lastForeignPortOfLoad) ||
					IsUSBoundTransportMoreValid(firstUSBoundTransportOrDepartureTransport)))
				{
					lastForeignPortOfLoad = firstUSBoundTransportOrDepartureTransport.LoadPort;
				}
				return lastForeignPortOfLoad;
			}
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingLastForeignPortOfLoading()
		{
			yield return consol.JK_RL_NKLastForeignPortInfo;
			yield return consol.JK_DateLastForeignPortInfo;

			foreach (var info in GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in GetTransportsInfos(Transport.Schema.JW_ETD, Transport.Schema.JW_ATD))
			{
				yield return info;
			}
		}

		public override IEnumerable<ZPropertyInfo> GetInfosAffectingTransportsOrder()
		{
			yield return header.BH_ImportTransportModeInfo;
			foreach (var info in base.GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}
		}

		public Transport LoadTransportForUSBoundVessel
		{
			get
			{
				var allTransportsForUSBoundVessel = AllTransportsForUSBoundVessel;
				return allTransportsForUSBoundVessel.Length > 0 ? allTransportsForUSBoundVessel[0] : null;
			}
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingLoadTransportForUSBoundVessel()
		{
			foreach (var info in GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in GetTransportsInfos(Transport.Schema.JW_Vessel))
			{
				yield return info;
			}
		}

		public Transport FirstCarrierContractualTransport
		{
			get
			{
				var result = FirstCountryBoundTransportOrFirstTransportWithTransportMode;
				if (result != null)
				{
					var carrierAddressPK = result.JW_OA_CarrierAddress;
					if (carrierAddressPK.IsValid)
					{
						var currentLeg = result.JW_LegOrder;
						var previousLeg = consol.Transports.OfType<Transport>()
							   .Where(x => x.JW_LegOrder < currentLeg && x.JW_OA_CarrierAddress == carrierAddressPK).OrderBy(x => x.JW_LegOrder).FirstOrDefault();

						if (previousLeg != null)
						{
							result = previousLeg;
						}
					}
					else
					{
						var usBoundVessel = LoadTransportForUSBoundVessel;
						if (usBoundVessel != null)
						{
							result = usBoundVessel;
						}
					}
				}
				return result;
			}
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingFirstCarrierContractualTransport()
		{
			foreach (var info in GetTransportsInfos(Transport.Schema.JW_OA_CarrierAddress, Transport.Schema.JW_Vessel))
			{
				yield return info;
			}

			foreach (var info in GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}
		}

		public ZString BillOfLadingStatusCode
		{
			get
			{
				var result = BillOfLadingStatusIndicatorList.Codes.HouseBill;

				var allTransportsForUSBoundVessel = AllTransportsForUSBoundVessel;
				if (allTransportsForUSBoundVessel.Length > 0)
				{
					var dischargeTransportForUSBoundVessel = allTransportsForUSBoundVessel[allTransportsForUSBoundVessel.Length - 1];
					if (!IsPortInTheCountry(dischargeTransportForUSBoundVessel.DiscPort))
					{
						result = (header != null && header.IsNVOCCHeader) ? BillOfLadingStatusIndicatorList.Codes.FROB : BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard;
					}
				}
				return result;
			}
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingBillOfLadingStatusCode()
		{
			foreach (var info in GetInfosAffectingTransportsOrder())
			{
				yield return info;
			}

			foreach (var info in GetTransportsInfos(Transport.Schema.JW_Vessel))
			{
				yield return info;
			}
		}

		#region Implementation

		bool IsUSBoundTransportMoreValid(Transport firstUSBoundTransportOrDepartureTransport)
		{
			return firstUSBoundTransportOrDepartureTransport != null && ((firstUSBoundTransportOrDepartureTransport.JW_ATD.IsEmpty ? firstUSBoundTransportOrDepartureTransport.JW_ETD : firstUSBoundTransportOrDepartureTransport.JW_ATD) > consol.JK_DateLastForeignPort &&
						!IsPortInTheCountry(firstUSBoundTransportOrDepartureTransport.LoadPort));
		}

		Transport[] AllTransportsForUSBoundVessel
		{
			get
			{
				var list = new List<Transport>();
				var firstUSBoundTransportOrDepartureTransport = FirstCountryBoundTransportOrFirstTransportWithTransportMode;
				if (firstUSBoundTransportOrDepartureTransport != null)
				{
					list.AddRange(GetAllTransportsForVessel(FirstCountryBoundTransportOrFirstTransportWithTransportMode.JW_Vessel));
				}
				return list.ToArray();
			}
		}

		void HookToHeaderEventsAffectingFirstCountryBoundTransportOrFirstTransportWithTransportModeCalculation()
		{
			header.BH_ImportTransportModeInfo.ValueChanged -= TransportsOrderAffecting_ValueChanged;
			header.BH_ImportTransportModeInfo.ValueChanged += TransportsOrderAffecting_ValueChanged;
		}

		void UnHookHeaderToEventsAffectingFirstCountryBoundTransportOrFirstTransportWithTransportModeCalculation()
		{
			header.BH_ImportTransportModeInfo.ValueChanged -= TransportsOrderAffecting_ValueChanged;
		}

		Transport[] GetAllTransportsForVessel(ZString vessel)
		{
			var result = new List<Transport>();
			foreach (var transport in TransportsInLegOrder)
			{
				if (transport.JW_Vessel == vessel)
				{
					result.Add(transport);
				}
			}
			return result.ToArray();
		}

		#endregion

		protected override ZString GetConsolTransportMode()
		{
			return TransportTypeList.GetFreightTransportType(header.BH_ImportTransportMode);
		}

		public override void Dispose()
		{
			base.Dispose();
			UnHookHeaderToEventsAffectingFirstCountryBoundTransportOrFirstTransportWithTransportModeCalculation();
		}
	}
}

using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class CarrierMessagingExtensions
	{
		public static bool IsForwardAirCarrierOnConsol(this ForwardingConsol consol)
		{
			var result = false;

			if (consol != null)
			{
				var carrier = consol.ShippingLine;
				if (carrier != null)
				{
					result = carrier.CustomsCodes
						.Cast<OrgCusCode>()
						.Any(c => c.OK_CodeType == OrgCusCode.CodeTypes.EHubOrganisationID
								&& c.OK_CustomsRegNo == OrgConstants.NumberFountains.Code.ForwardAirBillNumbers);
				}
			}

			return result;
		}

		public static bool IsSuitableForForwardAirMessage(this ForwardingConsol consol)
		{
			return IsUSOrCanadaExportOnConsol(consol)
				&& (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates
				|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);
		}

		static bool IsUSOrCanadaExportOnConsol(this ForwardingConsol consol)
		{
			return consol != null
					&& ((IsLoadPortUSOrCanada(consol.JK_RL_NKLoadPort) && consol.IsRoad)
					|| (consol.Transports.Cast<Transport>().Any(transport => IsLoadPortUSOrCanada(transport.JW_RL_NKLoadPort)
					&& transport.IsRoad)));
		}

		static bool IsLoadPortUSOrCanada(ZString loadPort)
		{
			return loadPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.UnitedStates || loadPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Canada;
		}
	}
}

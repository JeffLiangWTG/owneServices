using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	internal static class AWBRateLineHelper
	{
		public static SummarizeULDSLACConfig GetSummarizeULDSLACConfig(ZString dischargePortCountryCode)
		{
			var configCollection = FreightDataRegistry.Instance.SummarizeULDSLACs.Value;

			if (string.IsNullOrEmpty(dischargePortCountryCode))
			{
				return null;
			}

			return configCollection?.Cast<SummarizeULDSLACConfig>().FirstOrDefault(c => c.DestinationCountry.EqualsIgnoringCase(dischargePortCountryCode));
		}

		public static bool IsFreightChargeCode(this AccChargeCode chargeCode)
		{
			if (chargeCode == null || chargeCode.Company == null)
			{
				return false;
			}

			// We need to look for a freight charge code withing the company where the charge is created
			// instead of using the current company. In most cases it will be the current company,
			// but in some cases it may be different company, for example when we load charges autorated
			// by another company to display them in AWB.
			var company = chargeCode.Company.PK.ToGuid();
			var freightChargeCode = Env.Registry.GetFreightChargeCode(company);
			return chargeCode.PK == freightChargeCode;
		}
	}
}
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	partial class TRIncotermChargeCodeList
	{
		public static bool IsLocalCharge(ZString chargeCode)
		{
			return chargeCode == Codes.LBC
				|| chargeCode == Codes.LSC
				|| chargeCode == Codes.LDC
				|| chargeCode == Codes.LPC
				|| chargeCode == Codes.LocalCultureCharge
				|| chargeCode == Codes.LocalResourceUtilizationSupportFundCharge
				|| chargeCode == Codes.LocalEnvironmentCharge
				|| chargeCode == Codes.LOT;
		}

		public static bool IsForeignCharge(ZString chargeCode)
		{
			return chargeCode == Codes.COM
				|| chargeCode == Codes.DEM
				|| chargeCode == Codes.ROY
				|| chargeCode == Codes.INT
				|| chargeCode == Codes.OTH
				|| chargeCode == Codes.Observation
				|| chargeCode == Codes.Surveillance;
		}

		public static bool IsTotalCharge(ZString chargeCode) => chargeCode == Codes.LocalTotalCharges || chargeCode == Codes.TotalForeignCharges;
	}

	public static class TRIncotermChargeCodeListStatic
	{
		public static bool IsLocalCharge(this JobComInvCharge charge) => TRIncotermChargeCodeList.IsLocalCharge(charge.J7_ChargeType);

		public static bool IsForeignCharge(this JobComInvCharge charge) => TRIncotermChargeCodeList.IsForeignCharge(charge.J7_ChargeType);

		public static bool IsTotalCharge(this JobComInvCharge charge) => TRIncotermChargeCodeList.IsTotalCharge(charge.J7_ChargeType);

		public static bool IsOtherChargeThanLocalOrForeign(this JobComInvCharge charge) => !(charge.IsLocalCharge() || charge.IsForeignCharge());
	}
}

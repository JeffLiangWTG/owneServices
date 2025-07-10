namespace Enterprise.Customs.US.Business
{
	public class DrawbackOtherFeeTypesList : DrawbackFeeTypesList
	{
		public DrawbackOtherFeeTypesList()
		{
			RemoveCode(Codes.DrawbackDuty);
			RemoveCode(Codes.DrawbackTaxes);
			RemoveCode(Codes.DrawbackHMF);
			RemoveCode(Codes.DrawbackMPF);
		}

		public static bool IsGrandTotalDutyAmountFee(string code)
		{
			return code == Codes.DrawbackDuty
				|| code == Codes.PRDrawbackDuty;
		}

		public static bool IsGrandTotalIRTaxAmountFee(string code)
		{
			return code == Codes.DrawbackTaxes
				|| code == Codes.OilSpillTax
				|| code == Codes.DomesticTax
				|| code == Codes.DrawbackSuperfundTax;
		}

		public static bool IsOtherFee(string code)
		{
			return code != Codes.DrawbackDuty
				&& code != Codes.DrawbackTaxes
				&& code != Codes.DrawbackHMF
				&& code != Codes.DrawbackMPF
				&& !IsPuertoRicoFee(code);
		}

		public static bool IsPuertoRicoFee(string code)
		{
			return code == Codes.PRDrawbackDuty;
		}
	}
}

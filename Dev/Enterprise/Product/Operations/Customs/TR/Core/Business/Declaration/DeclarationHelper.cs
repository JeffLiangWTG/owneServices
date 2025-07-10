namespace Enterprise.Customs.TR.Business.Declaration
{
	public static class DeclarationHelper
	{
		public static class NationalVatType
		{
			public const string Code = "40";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised string")]
			public const string Description = "Katma Değer Vergisi";
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive("Used In MergeManager")]
		public static class StampDutyConstants
		{
			public const string ChargeType = "89";
			public const decimal ChargeAmount = 624.10m;
			public const string MethodOfPayment = "P";
			public const string RateOverrideReasonCode = "OVR";
			public const string Source = "CW1";
		}
	}
}

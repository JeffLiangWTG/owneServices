using CargoWise.Types;

namespace Enterprise.Customs.SG.Registry
{
	public sealed class EntryChargeTypeList : Enterprise.Registry.Business.Customs.EntryChargeTypeList
	{
		public static class Codes
		{
			public const string Duty = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
			public const string Excise = "EXC";
			public const string GST = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;
			public const string OtherTax = "OTH";
		}

		public static class Descriptions
		{
			public const string Duty = "Duty";
			public const string Excise = "Excise";
			public const string GST = "GST";
			public const string OtherTax = "Other Tax";
		}

		public EntryChargeTypeList()
		{
			Add(Codes.Duty, Descriptions.Duty, true, ZString.Empty);
			Add(Codes.Excise, Descriptions.Excise, true, ZString.Empty);
			Add(Codes.GST, Descriptions.GST, true, ZString.Empty);
			Add(Codes.OtherTax, Descriptions.OtherTax, true, ZString.Empty);
		}

		public override string DutyCode => Codes.Duty;

		public override string TaxCode => Codes.GST;
	}
}

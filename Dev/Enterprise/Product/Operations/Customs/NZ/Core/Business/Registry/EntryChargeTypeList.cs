// This file was originally auto-generated BUT IS NOW MANUALLY MAINTAINED.

using CargoWise.Types;

namespace Enterprise.Customs.NZ.Registry
{
	#region SuppressResourceStringsCheckRegion

	public class EntryChargeTypeList : Enterprise.Registry.Business.Customs.EntryChargeTypeList
	{
		public static class Codes
		{
			public const string ACCFuelLevy = "ACC";
			public const string ACCLevyCredit = "CCR";
			public const string ALACLevy = "ALC";
			public const string ALACLevyCredit = "ACR";
			public const string AntiDumpingDuty = "ADD";
			public const string CountervailingDuty = "CVD";
			public const string DepositRefund = "DFD";
			public const string Duty = "DTY";
			public const string DutyCredit = "DDB";
			public const string EntryFee = "ENF";
			public const string EntryFeeGST = "EFG";
			public const string ExciseDutyCredit = "EDC";
			public const string GST = "GST";
			public const string GSTCredit = "GCR";
			public const string HERALevy = "HER";
			public const string HERALevyCredit = "HCR";
			public const string PFMLFuelLevy = "PML";
			public const string PFMLFuelLevyCredit = "PCR";
			public const string SGGLevy = "SGG";
		}

		public static class Descriptions
		{
			public const string ACCFuelLevy = "ACC Fuel Levy";
			public const string ACCLevyCredit = "Accident Compensation Levy Credit";
			public const string ALACLevy = "ALAC Levy";
			public const string ALACLevyCredit = "ALAC Levy Credit";
			public const string AntiDumpingDuty = "Anti Dumping Duty";
			public const string CountervailingDuty = "Countervailing Duty";
			public const string DepositRefund = "Deposit Refund";
			public const string Duty = "Duty";
			public const string DutyCredit = "Duty Credit";
			public const string EntryFee = "Entry Fee";
			public const string EntryFeeGST = "Entry Fee GST";
			public const string ExciseDutyCredit = "Excise Duty Credit";
			public const string GST = "GST";
			public const string GSTCredit = "GST Credit";
			public const string HERALevy = "HERA Steel Levy";
			public const string HERALevyCredit = "HERA Steel Levy Credit";
			public const string PFMLFuelLevy = "PFML Fuel Levy";
			public const string PFMLFuelLevyCredit = "PFML Fuel Levy Credit";
			public const string SGGLevy = "Synthetic Greenhouse Gases Levy";
		}

		public EntryChargeTypeList()
		{
			Add(Codes.ACCFuelLevy, Descriptions.ACCFuelLevy, true, ZString.Empty);
			Add(Codes.ACCLevyCredit, Descriptions.ACCLevyCredit, false, ZString.Empty);
			Add(Codes.ALACLevy, Descriptions.ALACLevy, true, ZString.Empty);
			Add(Codes.ALACLevyCredit, Descriptions.ALACLevyCredit, false, ZString.Empty);
			Add(Codes.AntiDumpingDuty, Descriptions.AntiDumpingDuty, true, ZString.Empty);
			Add(Codes.CountervailingDuty, Descriptions.CountervailingDuty, true, ZString.Empty);
			Add(Codes.DepositRefund, Descriptions.DepositRefund, false, ZString.Empty);
			Add(Codes.Duty, Descriptions.Duty, true, ZString.Empty);
			Add(Codes.DutyCredit, Descriptions.DutyCredit, true, ZString.Empty);
			Add(Codes.EntryFee, Descriptions.EntryFee, true, ZString.Empty);
			Add(Codes.EntryFeeGST, Descriptions.EntryFeeGST, true, Codes.EntryFee);
			Add(Codes.ExciseDutyCredit, Descriptions.ExciseDutyCredit, false, ZString.Empty);
			Add(Codes.GST, Descriptions.GST, true, ZString.Empty);
			Add(Codes.GSTCredit, Descriptions.GSTCredit, false, ZString.Empty);
			Add(Codes.HERALevy, Descriptions.HERALevy, true, ZString.Empty);
			Add(Codes.HERALevyCredit, Descriptions.HERALevyCredit, false, ZString.Empty);
			Add(Codes.PFMLFuelLevy, Descriptions.PFMLFuelLevy, true, ZString.Empty);
			Add(Codes.PFMLFuelLevyCredit, Descriptions.PFMLFuelLevyCredit, false, ZString.Empty);
			Add(Codes.SGGLevy, Descriptions.SGGLevy, true, ZString.Empty);
		}

		public override string DutyCode => Codes.Duty;

		public override string TaxCode => Codes.GST;

		protected override void FilterChargeTypesForRegistryCore()
		{
			RemoveWhere(c => c.IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing);
		}

		public static bool IsLevy(string chargeCode)
		{
			return chargeCode == Codes.ACCFuelLevy
					|| chargeCode == Codes.ALACLevy
					|| chargeCode == Codes.HERALevy
					|| chargeCode == Codes.PFMLFuelLevy
					|| chargeCode == Codes.SGGLevy;
		}
	}

	#endregion
}

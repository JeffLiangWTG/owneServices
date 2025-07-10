using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IAntiDumpingC3
	{
		ZString ISOCountryCode { get; set; }

		ZString CaseNumber { get; set; }

		ZString RelatedCaseNumber { get; set; }

		ZString ManufacturerIDCode { get; set; }

		ZString ShipperID { get; set; }

		ZString CaseStatus { get; set; }

		ZDate CaseStatusDate { get; set; }

		ZString BondCashIndicator { get; set; }
	}

	public interface IAntiDumpingC4
	{
		ZString CaseNumber { get; set; }

		ZString Contact { get; set; }

		ZString Phone { get; set; }

		ZDate LiquidationSuspensionDate { get; set; }

		ZString ShortDescription { get; set; }
	}

	public interface IAntiDumpingC5
	{
		ZString CaseNumber { get; set; }

		ZString RateIndicator { get; set; }

		ZString DepositRate1 { get; set; }

		ZDecimal DepositRate2 { get; set; }

		ZDecimal DepositRate3 { get; set; }

		ZDate EffectiveEntryDate { get; set; }

		ZDate EffectiveExportDate { get; set; }

		ZString BondCashIndicator1 { get; set; }

		ZDate BondCashDate1 { get; set; }

		ZString BondCashIndicator2 { get; set; }

		ZDate BondCashDate2 { get; set; }

		ZString BondCashIndicator3 { get; set; }

		ZDate BondCashDate3 { get; set; }

		ZString BondCashIndicator4 { get; set; }

		ZDate BondCashDate4 { get; set; }

		ZString BondCashIndicator5 { get; set; }

		ZDate BondCashDate5 { get; set; }
	}

	public interface IAntiDumpingC6
	{
		ZString TariffNumber { get; set; }

		ZString TariffNumber1 { get; set; }

		ZString TariffNumber2 { get; set; }

		ZString TariffNumber3 { get; set; }

		ZString TariffNumber4 { get; set; }

		ZString TariffNumber5 { get; set; }

		ZString TariffNumber6 { get; set; }
	}

	public interface IAntiDumpingC7
	{
		ZString CaseNumber { get; set; }

		ZString ManufacturerName { get; set; }
	}

	public interface IAntiDumpingC8
	{
		ZString CaseNumber { get; set; }

		ZString Shipper { get; set; }
	}

	public interface IAntiDumpingC9
	{
		ZString NarrativeMessage { get; set; }

		ZString MessageIDCode { get; set; }
	}
}

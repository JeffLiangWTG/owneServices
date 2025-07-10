using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface ISecondaryTariffLine : IPGAGovernmentAgenciesCommon
	{
		new ZString Tariff { get; }
		ZString SpecialProgramsIndicatorSecondary { get; }
		ZDecimal Duty { get; }
		ZDecimal Quantity1 { get; }
		ZString UQ1 { get; }
		ZDecimal Quantity2 { get; }
		ZString UQ2 { get; }
		ZDecimal Quantity3 { get; }
		ZString UQ3 { get; }
		ZDecimal ValueInUSD { get; }
		ZDecimal SupCustomsValue { get; }
		ZString SpecialProgramsIndicatorPrimaryOrCountry { get; }
		ZDecimal GrossWeightInKilograms { get; }
		bool IsCombineLine { get; }

		bool IsDisclaimSanction { get; }
	}
}

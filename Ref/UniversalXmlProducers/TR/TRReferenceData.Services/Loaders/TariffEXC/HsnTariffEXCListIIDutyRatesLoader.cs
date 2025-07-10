namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public class HsnTariffEXCListIIDutyRatesLoader : HsnTariffEXCListDutyRatesLoaderBase
	{
		#region Overrides of HsnTariffEXCListDutyRatesLoaderBase

		public override string InputFileName => "OTV Liste II.xlsx";

		protected override int UomCU3ColumnNumber => default;

		protected override int ExemptedTariffCodesColumnNumber => 4;

		protected override int AdditionalCodeColumnNumber => 5;

		protected override int RateTypeColumnNumber => 6;

		protected override int RateCodeColumnNumber => 7;

		protected override int RateFormulaColumnNumber => 8;

		protected override int StartDateColumnNumber => 9;

		protected override int EndDateColumnNumber => 10;

		#endregion
	}
}

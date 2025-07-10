namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public class HsnTariffEXCListIIIDutyRatesLoader : HsnTariffEXCListDutyRatesLoaderBase
	{
		#region Overrides of HsnTariffEXCListDutyRatesLoaderBase

		public override string InputFileName => "OTV Liste III.xlsx";
		protected override int UomCU4ColumnNumber => 6;
		protected override int UomCU5ColumnNumber => 7;

		protected override int ExemptedTariffCodesColumnNumber => 8;

		protected override int AdditionalCodeColumnNumber => 9;

		protected override int RateTypeColumnNumber => 10;

		protected override int RateCodeColumnNumber => 11;

		protected override int RateFormulaColumnNumber => 12;

		protected override int StartDateColumnNumber => 15;

		protected override int EndDateColumnNumber => 16;

		#endregion
	}
}

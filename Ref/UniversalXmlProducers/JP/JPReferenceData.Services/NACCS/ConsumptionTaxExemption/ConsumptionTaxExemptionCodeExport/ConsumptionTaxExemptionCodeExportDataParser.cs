using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	internal class ConsumptionTaxExemptionCodeExportDataParser : BaseConsumptionTaxExemptionRefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.ConsumptionTaxExemptionCodeExport;
		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z]$");
	}
}

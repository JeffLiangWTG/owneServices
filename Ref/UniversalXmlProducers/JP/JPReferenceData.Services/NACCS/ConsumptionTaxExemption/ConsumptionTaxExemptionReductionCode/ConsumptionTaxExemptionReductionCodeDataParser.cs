using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{ 
	public class ConsumptionTaxExemptionReductionCodeDataParser : BaseConsumptionTaxExemptionRefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.ConsumptionTaxExemptionReductionCode;
		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z0-9]{3}$");
	}
}

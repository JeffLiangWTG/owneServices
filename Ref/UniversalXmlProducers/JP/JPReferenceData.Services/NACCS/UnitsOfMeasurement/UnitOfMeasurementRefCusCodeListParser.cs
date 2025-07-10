using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class UnitOfMeasurementRefCusCodeListParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.UnitOfMeasurement;
		protected override string GetZZD_Code(string[] columns) => columns[1];
		protected override string GetZZD_Description(string[] columns) => columns[3];
		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z]{2,4}$");
		protected override bool CustomisedRowValidate(string[] columns)
		{
			return !(string.IsNullOrWhiteSpace(columns[0]) || string.IsNullOrWhiteSpace(columns[2]) || string.IsNullOrWhiteSpace(columns[3]));
		}
		protected override RefCusCodeListLanguageParser GetRefCusCodeListLanguageParserCore() => new UnitOfMeasurementLanguageParser();
	}
}

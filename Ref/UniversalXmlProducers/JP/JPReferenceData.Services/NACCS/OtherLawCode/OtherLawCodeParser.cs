using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class OtherLawCodeParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.OtherLawCode;
		protected override string GetZZD_Code(string[] columns) => columns[2];
		protected override string GetZZD_Description(string[] columns) => columns[1]; 
		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z]{2}$");
		protected override RefCusCodeListAttributeParser GetRefCusCodeListAttributeParserCore() => new OtherLawCodeAttributeParser();
		protected override bool CustomisedRowValidate(string[] columns) => !string.IsNullOrWhiteSpace(columns[1]);
	}
}

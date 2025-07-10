using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class SpecialCargoCodeParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.SpecialCargoCode;
		protected override string GetZZD_Code(string[] columns) => columns[2];
		protected override string GetZZD_Description(string[] columns) => columns[1];
		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z]{3}$");
		protected override bool CustomisedRowValidate(string[] columns) => !string.IsNullOrWhiteSpace(columns[1]);
	}
}

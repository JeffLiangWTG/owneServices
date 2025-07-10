using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class BeforePermitApplicationReasonCodeParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => "BPARC";
		protected override string GetZZD_Code(string[] columns) => columns[0];
		protected override string GetZZD_Description(string[] columns) => columns[1];
		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z0-9]{2}$");
	}
}

using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ImportTradeControlOrdinanceAppendixRefCusCodeListParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.ImportTradeControlOrdinanceAppendix;
		protected override string GetZZD_Code(string[] columns) => columns[1];
		protected override string GetZZD_Description(string[] columns) => columns[0];
		protected override Regex ZZD_CodeRegex => new Regex("^[0-9]{4}$");
	}
}

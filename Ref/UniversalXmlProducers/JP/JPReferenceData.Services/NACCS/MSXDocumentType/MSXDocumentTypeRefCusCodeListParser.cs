using System.Text;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class MSXDocumentTypeRefCusCodeListParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.MSXDocumentType;

		protected override string GetZZD_Code(string[] columns) => columns[0].Normalize(NormalizationForm.FormKC);

		protected override string GetZZD_Description(string[] columns) => columns[1];

		protected override Regex ZZD_CodeRegex => new Regex("^[A-Z]{2}$");
	}
}

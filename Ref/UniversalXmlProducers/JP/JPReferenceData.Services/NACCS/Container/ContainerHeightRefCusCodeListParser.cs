using System.Text;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ContainerHeightRefCusCodeListParser : RefCusCodeListParser
	{
		bool isValidRow;

		const string TripleQuotes = "\"\"\"";

		const string DoubleQuotesFollowingSingleQuotes = "'\"";

		public override string CurrentCodeType => Constants.CodeType.ContainerHeight;

		protected override string GetZZD_Code(string[] columns) => columns[0].Normalize(NormalizationForm.FormKC);

		protected override string GetZZD_Description(string[] columns) => (columns[1] + columns[2].Replace(TripleQuotes, "\"")).Replace(DoubleQuotesFollowingSingleQuotes, "'");

		protected override Regex ZZD_CodeRegex => new Regex("^[0-9]{1}$");

		protected override bool CustomisedRowValidate(string[] columns)
		{
			isValidRow = isValidRow || columns[0].StartsWith("2桁目", System.StringComparison.OrdinalIgnoreCase);
			return isValidRow;
		}
	}
}

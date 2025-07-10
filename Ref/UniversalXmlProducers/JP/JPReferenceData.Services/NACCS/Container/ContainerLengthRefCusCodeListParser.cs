using System.Text;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ContainerLengthRefCusCodeListParser : RefCusCodeListParser
	{
		bool isValidRow;

		public override string CurrentCodeType => Constants.CodeType.ContainerLength;

		protected override string GetZZD_Code(string[] columns) => columns[0].Normalize(NormalizationForm.FormKC);

		protected override string GetZZD_Description(string[] columns) => columns[1];

		protected override Regex ZZD_CodeRegex => new Regex("^[0-9]{1}$");

		protected override bool CustomisedRowValidate(string[] columns)
		{
			if (columns[0].StartsWith("1桁目", System.StringComparison.OrdinalIgnoreCase))
			{
				isValidRow = true;
			}
			else if (columns[0].StartsWith("2桁目", System.StringComparison.OrdinalIgnoreCase))
			{
				isValidRow = false;
			}
			return isValidRow;
		}
	}
}

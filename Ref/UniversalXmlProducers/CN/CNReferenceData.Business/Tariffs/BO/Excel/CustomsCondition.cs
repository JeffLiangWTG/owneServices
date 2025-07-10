using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class CustomsCondition
	{
		public string Code { get; }
		public string Description { get; }
		public bool IsImport { get; }
		public bool IsExport { get; }

		public string ConditionType
		{
			get
			{
				switch (Code)
				{
					case "08":
						return "EXPPH";
					case "06":
					case "09":
						return "IMPPH";
					default:
						return CNDOC;
				}
			}
		}

		public static string ConditionClass => CTRL;

		public string ConditionValueType
		{
			get
			{
				switch (Code)
				{
					case "08":
					case "09":
					case "06":
						return "PROH";
					default:
						return DOC;
				}
			}
		}

		public CustomsCondition(string code, string description, bool isImport, bool isExport)
		{
			Code = code;
			Description = description;
			IsImport = isImport;
			IsExport = isExport;
		}

		public static IEnumerable<CustomsCondition> Extract(string codes)
		{
			return codes.Where(x => char.IsLetterOrDigit(x)).Select(x => CustomsConditionList.Get(x));
		}

		public bool IsIgnorable => CustomsCode == 'A';

		public char CustomsCode => Code.LastOrDefault();

		public const string CNDOC = "CNDOC";

		public const string CTRL = "CTRL";

		public const string DOC = "DOC";

		public static bool IsImportPermit(char c) => c == '1' || c == '2';

		public static bool IsExportPermit(char c) => c == '3' || c == '4' || c == '5' || c == 'x' || c == 'y' || c == 'G';

		public bool IsProhibit => Code == "06" || Code == "08" || Code == "09";
		public bool RequiresLineNumberOptional => Code == "1F" || Code == "1R" || Code == "1J" || Code == "1E";
		public bool RequiresLineNumberMandatory => Code == "1Y";
		public bool IsLicense => IsImportPermit(CustomsCode) || IsExportPermit(CustomsCode);

		public string RequirementName => IsImport ? "ImportCUSRequirement" : IsExport ? "ExportCUSRequirement" : null;
	}
}

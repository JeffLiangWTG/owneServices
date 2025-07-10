using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class DesignatedSupervisionSite
	{
		public string CustomsCode { get; set; }
		public string Type { get; set; }
		public string CustomsDistrict { get; set; }
		public string Name { get; set; }

		public (string code, string suffix) ParsedCustomsCode
		{
			get
			{
				if (parsedCustomsCode == default)
				{
					if (!new Regex("[^_0-9a-zA-Z]").IsMatch(CustomsCode))
					{
						parsedCustomsCode.code = CustomsCode;
						parsedCustomsCode.suffix = string.Empty;
					}
					else
					{
						var underscoreIndex = CustomsCode.IndexOf('_');
						parsedCustomsCode.code = CustomsCode.Substring(0, underscoreIndex);
						parsedCustomsCode.suffix = CustomsCode.Substring(underscoreIndex);
					}
				}

				return parsedCustomsCode;
			}
		}
		(string code, string suffix) parsedCustomsCode;
	}
}

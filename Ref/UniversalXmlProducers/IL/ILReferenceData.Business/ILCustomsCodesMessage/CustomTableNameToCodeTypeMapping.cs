using System.Collections.Generic;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public static class CustomTableNameToCodeTypeMapping
	{
		public static IEnumerable<string> GetCodeType(string customTableName, XElement codeElement)
		{
			switch (customTableName)
			{
				case "1091":
					if (codeElement.Element("IsForManifest")?.Value.Trim() == "true")
					{
						yield return "MPKG";
					}
					if (codeElement.Element("IsForDeclaration")?.Value.Trim() == "true")
					{
						yield return "PKG";
					}
					break;

				case "2012":
					yield return "FAC";
					break;

				case "1307":
					yield return "C1307";
					break;

				case "13":
					yield return "ILDOC";
					break;

				case "1339":
					yield return "C1339";
					break;

				case "2192":
					yield return "DISCH";
					break;

				case "23774":
					yield return "23774";
					break;

				default:
					throw new UnknownTableException($"Unknown table name: {customTableName}");
			}
		}
	}
}

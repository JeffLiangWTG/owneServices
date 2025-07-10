using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Nomenclature
{
	public static class NomenclatureParserNO
	{
		static StringBuilder errorBuilder;
		static List<RefCusNomenclatureGroup> nomenclatureList = new List<RefCusNomenclatureGroup>();

		public static List<RefCusNomenclatureGroup> CreateNomenclature(string tariffStructureXml)
		{
			nomenclatureList = nomenclatureList ?? new List<RefCusNomenclatureGroup>();
			nomenclatureList.Clear();

			errorBuilder = errorBuilder ?? new StringBuilder();
			errorBuilder.Clear();

			if (string.IsNullOrEmpty(tariffStructureXml))
			{
				return null;
			}

			var tariff = new XmlDocument();
			tariff.LoadXml(tariffStructureXml);

			var xmlNodeList = tariff.SelectNodes("/tolltariffstruktur");
			foreach (XmlNode xmlNode in xmlNodeList)
			{
				TraverseXml(xmlNode);
			}

			return nomenclatureList;
		}

		static string TraverseXml(XmlNode xmlNodeList, string nomenclatur = "")
		{
			foreach (XmlNode xmlNode in xmlNodeList)
			{
				var subNodeName = new string[] { "underposisjon", "underkapittel" };
				var divisionsNodeName = new string[] { "inndelinger", "oppdelinger" };
				var idNode = xmlNode["id"]?.InnerText ?? string.Empty;
				var itemNode = xmlNode["vareslag"]?.InnerText ?? string.Empty;
				var hsNumNode = xmlNode["hsNummer"]?.InnerText ?? string.Empty;
				var descNode = xmlNode["beskrivelse"]?.InnerText ?? string.Empty;

				var haveId = !string.IsNullOrWhiteSpace(idNode);
				var havehsNum = !string.IsNullOrWhiteSpace(hsNumNode);

				var parentNodeName = xmlNode.ParentNode.Name;
				var nodeName = xmlNode.Name;

				if (haveId || havehsNum)
				{
					var nomString = nomenclatur;

					if (divisionsNodeName.Contains(parentNodeName) && !subNodeName.Contains(nodeName) || !string.IsNullOrWhiteSpace(nomString))
					{
						nomString += ".";
					}

					var newValue = ExtractNomenclatureData(xmlNode, idNode, hsNumNode, havehsNum);

					nomString += newValue;
					XmlAddRecord(nomString, hsNumNode, !string.IsNullOrWhiteSpace(descNode) ? descNode : itemNode);

					TraverseXml(xmlNode, nomString);
				}
				else
				{
					if (divisionsNodeName.Contains(parentNodeName) && nodeName == "vare")
					{
						nomenclatur += "..";
					}
					nomenclatur = TraverseXml(xmlNode, nomenclatur);
				}
			}
			return nomenclatur;
		}

		static string ExtractNomenclatureData(XmlNode xmlNode, string idNode, string hsNumNode, bool havehsNum)
		{
			var subNodeName = new string[] { "underposisjon", "underkapittel" };
			string newValue;

			if (havehsNum && xmlNode.Name == "vare")
			{
				var parentHsNum = GetOneParentValue(xmlNode.ParentNode, "hsNummer");
				var parentIdNum = GetOneParentValue(xmlNode.ParentNode, "id");

				if (parentHsNum == hsNumNode && idNode.Contains(hsNumNode))
				{
					newValue = idNode.Removestart(hsNumNode);
				}
				else if (!string.IsNullOrWhiteSpace(parentHsNum) && hsNumNode.Contains(parentHsNum))
				{
					newValue = hsNumNode.Removestart(parentHsNum);
				}
				else if (idNode.Contains(hsNumNode))
				{
					newValue = idNode.Removestart(hsNumNode);
					if (newValue.Replace("0", "").Length == 0)
					{
						newValue = hsNumNode.Removestart(parentIdNum);
					}
				}
				else if (!string.IsNullOrWhiteSpace(parentIdNum) && hsNumNode.Contains(parentIdNum))
				{
					newValue = hsNumNode.Removestart(parentIdNum);
				}
				else
				{
					newValue = GetAllParentValues(xmlNode.ParentNode, hsNumNode, "id");
				}
			}
			else
			{
				var remove = GetOneParentValue(xmlNode.ParentNode, "id");
				var identity = !string.IsNullOrWhiteSpace(hsNumNode) ? hsNumNode : idNode;
				newValue = identity.Removestart(remove);

				if (newValue != identity && !subNodeName.Contains(xmlNode.Name))
				{
					newValue = "." + newValue;
				}
			}

			return newValue;
		}

		static string GetAllParentValues(XmlNode xmlNode, string fullString, string idType)
		{
			var parent = xmlNode?.ParentNode?.Name;
			if (parent != "avsnitt" && parent != null)
			{
				var idNode = xmlNode[idType]?.InnerText ?? string.Empty;
				if (!string.IsNullOrWhiteSpace(idNode))
				{
					return fullString.Removestart(idNode);
				}
			}
			return GetAllParentValues(xmlNode.ParentNode, fullString, idType);
		}

		static string GetOneParentValue(XmlNode xmlNode, string idType)
		{
			var removeThis = string.Empty;

			if (xmlNode?.Name != "avsnitt" && !string.IsNullOrWhiteSpace(xmlNode?.Name))
			{
				var result = xmlNode[idType]?.InnerText ?? string.Empty;

				if (!string.IsNullOrWhiteSpace(result))
				{
					return result;
				}
				removeThis += GetOneParentValue(xmlNode?.ParentNode, idType);
			}
			return removeThis;
		}

		static string Removestart(this string input, string removeThis)
		{
			var index = input.IndexOf(removeThis, StringComparison.InvariantCultureIgnoreCase);
			return index < 0 ? input : input.Remove(index, removeThis.Length);
		}

		static void XmlAddRecord(string compositeKey, string value, string description)
		{
			if (!string.IsNullOrWhiteSpace(compositeKey) && !string.IsNullOrWhiteSpace(description))
			{
				var group = new RefCusNomenclatureGroup
				{
					ZZ5_CompositeKey = compositeKey,
					ZZ5_Value = value,
					ZZ5_Description = description
				};
				nomenclatureList.Add(group);
			}
			else
			{
				errorBuilder.AppendLine("Unable to parse Nomenclature due to empty key, or empty description.");
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.UniversalXmlParser
{
	public class UniversalXmlTransformerValidator : IUniversalXmlTransformerValidator
	{
		public TransformMode GetTransformMode(XmlDocument xmlDoc)
		{
			var nodeUpdateType = xmlDoc.SelectSingleNode("/UniversalReferenceData/UpdateType");
			if (nodeUpdateType != null
				&& nodeUpdateType.InnerText.Equals("Deletion", StringComparison.OrdinalIgnoreCase))
			{
				return TransformMode.None;
			}
			var parents = FindParentsWithRefCusApplicability(xmlDoc);
			var result = TransformMode.None;

			if (parents.Contains(nameof(RefCusRate)))
			{
				result |= TransformMode.Transform_RateAndApp;
			}
			if (parents.Contains(nameof(RefCusCondition)))
			{
				result |= TransformMode.Transform_CondAndApp;
			}
			if (parents.Contains(nameof(RefCusTariffAdditionalCode)) && result != TransformMode.None)
			{
				result |= TransformMode.Transform_KeepApp;
			}
			return result;
		}

		static List<string> FindParentsWithRefCusApplicability(XmlDocument xDoc)
		{
			var parentNames = new List<string>();
			var appNode = xDoc.SelectSingleNode("//EntityType[@Name='RefCusApplicability']");
			if (appNode == null)
			{
				return parentNames;
			}
			var xpath = @"
				//EntityType[
					Key/PropertyRef[@Name='RefCusApplicability'] 
					and Property[
						@Name='RefCusApplicability' 
						and @Type='RefCusApplicability'
					]
				]/@Name";
			var parentNodes = xDoc.SelectNodes(xpath);
			if (parentNodes != null)
			{
				foreach (XmlNode parent in parentNodes)
				{
					if (parent.Value != null)
					{
						parentNames.Add(parent.Value);
					}
				}
			}
			return parentNames;
		}
	}
}

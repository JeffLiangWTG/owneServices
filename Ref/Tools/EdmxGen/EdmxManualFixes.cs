using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public static class EdmxManualFixes
	{
		public static void RenameTablesWithPluralizationProblem(Tuple<string, string>[] tablesToRename, XElement conceptual, XElement mapping)
		{
			Argument.NotNull(mapping, nameof(mapping));
			Argument.NotNull(conceptual, nameof(conceptual));
			Argument.NotNull(tablesToRename, nameof(tablesToRename));

			foreach (var tbl in tablesToRename)
			{
				var entitySetElm = conceptual.Descendants().FirstOrDefault(o => o.Name.LocalName == Constants.Elements.EntitySet && o.Attribute(XName.Get("Name")).Value == tbl.Item2);
				if (entitySetElm != null)
				{
					var entityTypeAtt = entitySetElm.Attribute(XName.Get("EntityType"));
					entityTypeAtt.Value = entityTypeAtt.Value.Replace(tbl.Item1, tbl.Item2);
				}

				var entityTypeElm = conceptual.Descendants().FirstOrDefault(o => o.Name.LocalName == Constants.Elements.EntityType && o.Attribute(XName.Get("Name")).Value == tbl.Item1);
				if (entityTypeElm != null)
				{
					var nameAtt = entityTypeElm.Attribute(XName.Get("Name"));
					nameAtt.Value = nameAtt.Value.Replace(tbl.Item1, tbl.Item2);
				}

				var associationEndElm = conceptual.Descendants().Where(o => o.Name.LocalName == Constants.Elements.End && o.Attribute(XName.Get("Role"))?.Value == tbl.Item2);
				if (associationEndElm.Any())
				{
					foreach (var ends in associationEndElm)
					{
						var typeAtt = ends.Attribute(XName.Get("Type"));
						if (typeAtt != null)
						{
							typeAtt.Value = typeAtt.Value.Replace(tbl.Item1, tbl.Item2);
						}
					}
				}

				var entityTypeMapping = mapping.Descendants().FirstOrDefault(o => o.Name.LocalName == Constants.Elements.EntityTypeMapping && o.FirstAttribute.Value.EndsWith(tbl.Item1, StringComparison.OrdinalIgnoreCase));
				if (entityTypeMapping != null)
				{
					entityTypeMapping.FirstAttribute.Value = entityTypeMapping.FirstAttribute.Value.Replace(tbl.Item1, tbl.Item2);
				}
			}
		}

		public static void RefTimeZoneSetNavigationPropertiesFix(XElement conceptualModels)
		{
			Argument.NotNull(conceptualModels, nameof(conceptualModels));

			var navigationProperties = conceptualModels.Descendants().Where(o => o.Name.LocalName == Constants.Elements.NavigationProperty && o.FirstAttribute.Value.StartsWith("RefTimeZone", StringComparison.OrdinalIgnoreCase));
			foreach (var navProp in navigationProperties)
			{
				var relationshipAtt = navProp.Attribute(XName.Get("Relationship"));
				var relationshipName = navProp.Attribute(XName.Get("Name"));
				if (relationshipAtt != null)
				{
					if (relationshipAtt.Value.Contains("FK_RefTimeZoneSet_R3_R2_DaylightSavingZone_RefTimeZone"))
					{
						if (navProp.Parent?.FirstAttribute?.Value == "RefTimeZoneSet")
						{
							relationshipName.Value = "RefTimeZoneDaylightSavingZone";
						}
						else if (navProp.Parent?.FirstAttribute?.Value == "RefTimeZone")
						{
							relationshipName.Value = "RefTimeZoneSetDaylightSavingZone";
						}
					}
					else if (relationshipAtt.Value.Contains("FK_RefTimeZoneSet_R3_R2_StandardZone_RefTimeZone"))
					{
						if (navProp.Parent?.FirstAttribute?.Value == "RefTimeZoneSet")
						{
							relationshipName.Value = "RefTimeZoneStandardZone";
						}
						else if (navProp.Parent?.FirstAttribute?.Value == "RefTimeZone")
						{
							relationshipName.Value = "RefTimeZoneSetStandardZone";
						}
					}
				}
			}
		}
	}
}

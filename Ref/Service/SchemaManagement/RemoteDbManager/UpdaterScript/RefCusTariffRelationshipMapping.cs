using System.Collections.Generic;
using CargoWise.RefDbRepo.Common;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class RefCusTariffRelationshipMapping
	{
		public static DataTableMapping Mapping => new DataTableMapping
		{
			TableName = "#TempRefCusTariffRelationship",
			RelatedFKColumnNames = new Dictionary<string, string>()
			{
				{ "RefCusTariffType", "ZZH_ZZI_TariffType" }
			},
			RelatedTableNames = new Dictionary<string, DataTableMapping>()
			{
				{
					"RefCusTariffType", new DataTableMapping { TableName = "#TempRefCusTariffType" }
				}
			}
		};

		public static DataTableMapping Mapping1
		{
			get
			{
				var mapping = new DataTableMapping();
				mapping.TableName = Mapping.TableName;
				mapping.RelatedFKColumnNames = Mapping.RelatedFKColumnNames;
				mapping.RelatedFKColumnNames.Add("RefCusApplicabilities", "ZZT_ZZH_TariffRelationship");

				mapping.RelatedTableNames = Mapping.RelatedTableNames;
				mapping.RelatedTableNames.Add("RefCusApplicabilities", RefCusApplicabilityMapping.Mapping2);
				return mapping;
			}
		}
	}
}

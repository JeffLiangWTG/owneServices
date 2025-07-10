using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class PatternMatchingRemover
	{
		public static void DeleteAll(BusinessObject bizo)
		{
			if (!bizo.IsDeleted)
			{
				var factory = bizo.Factory;
				var tablePrefix = bizo.TablePrefix;
				var pk = bizo.PK;
				var isUltimateParent = bizo is OrgHeader || bizo is GlbPerson;

				if (!isUltimateParent)
				{
					foreach (var schema in GetPatternMatchingSchemas())
					{
						var query = new ZQuery(schema.parentId, pk)
						.AddToFilter(JoinCondition.And, schema.parentTableCode, tablePrefix);
						factory.Load(schema.patternTableType, query).DeleteAll();
					}
				}
				else
				{
					foreach (var schema in GetPatternMatchingUltimateParentSchemas(bizo))
					{
						var query = new ZQuery(schema.ultimateParent, pk);
						factory.Load(schema.patternTableType, query).DeleteAll();
					}
					DeletePatternMatchingResults(bizo);
				}
			}
		}

		static List<(SchemaGuidColumn parentId, SchemaStringColumn parentTableCode, System.Type patternTableType)> GetPatternMatchingSchemas()
		{
			var patternTableSchemas = new List<(SchemaGuidColumn, SchemaStringColumn, System.Type)>();
			patternTableSchemas.Add((PatternMatchingAddressSchema.PMA_ParentId, PatternMatchingAddressSchema.PMA_ParentTableCode, typeof(PatternMatchingAddress)));
			patternTableSchemas.Add((PatternMatchingEmailSchema.PME_ParentId, PatternMatchingEmailSchema.PME_ParentTableCode, typeof(PatternMatchingEmail)));
			patternTableSchemas.Add((PatternMatchingDomainSchema.PMD_ParentId, PatternMatchingDomainSchema.PMD_ParentTableCode, typeof(PatternMatchingDomain)));
			patternTableSchemas.Add((PatternMatchingNameSchema.PMN_ParentId, PatternMatchingNameSchema.PMN_ParentTableCode, typeof(PatternMatchingName)));
			patternTableSchemas.Add((PatternMatchingPhoneSchema.PMP_ParentId, PatternMatchingPhoneSchema.PMP_ParentTableCode, typeof(PatternMatchingPhone)));
			patternTableSchemas.Add((PatternMatchingRegCodeSchema.PMR_ParentId, PatternMatchingRegCodeSchema.PMR_ParentTableCode, typeof(PatternMatchingRegCode)));
			return patternTableSchemas;
		}

		static List<(SchemaGuidColumn ultimateParent, System.Type patternTableType)> GetPatternMatchingUltimateParentSchemas(BusinessObject bizo)
		{
			var isOrgHeader = bizo is OrgHeader;
			var patternTableSchemas = new List<(SchemaGuidColumn, System.Type)>();
			patternTableSchemas.Add((isOrgHeader ? PatternMatchingAddressSchema.PMA_OH : PatternMatchingAddressSchema.PMA_PER, typeof(PatternMatchingAddress)));
			patternTableSchemas.Add((isOrgHeader ? PatternMatchingEmailSchema.PME_OH : PatternMatchingEmailSchema.PME_PER, typeof(PatternMatchingEmail)));
			patternTableSchemas.Add((isOrgHeader ? PatternMatchingDomainSchema.PMD_OH : PatternMatchingDomainSchema.PMD_PER, typeof(PatternMatchingDomain)));
			patternTableSchemas.Add((isOrgHeader ? PatternMatchingNameSchema.PMN_OH : PatternMatchingNameSchema.PMN_PER, typeof(PatternMatchingName)));
			patternTableSchemas.Add((isOrgHeader ? PatternMatchingPhoneSchema.PMP_OH : PatternMatchingPhoneSchema.PMP_PER, typeof(PatternMatchingPhone)));
			patternTableSchemas.Add((isOrgHeader ? PatternMatchingRegCodeSchema.PMR_OH : PatternMatchingRegCodeSchema.PMR_PER, typeof(PatternMatchingRegCode)));
			return patternTableSchemas;
		}

		static void DeletePatternMatchingResults(BusinessObject bizo)
		{
			var query = new ZQuery(PatternMatchingResultSchema.PMT_MasterPK, bizo.PK);
			bizo.Factory.Load<PatternMatchingResult>(query).DeleteAll();

			query = new ZQuery(PatternMatchingResultSchema.PMT_TargetPK, bizo.PK);
			bizo.Factory.Load<PatternMatchingResult>(query).DeleteAll();
		}
	}
}

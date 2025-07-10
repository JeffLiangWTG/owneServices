using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class SchemaInfo : Client.SqlServer.SchemaInfo
	{
		public SchemaInfo(IDbConnection conn)
			: base(conn)
		{ }

		protected override void AddAdditionalFKs(List<ForeignKeyRelationship> foreignKeyRelationshipList)
		{
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IRefCusTaxOrFeeType)))
			{
				foreignKeyRelationshipList.Add(new ForeignKeyRelationship
				{
					ReferencedTable = typeof(IRefCusTaxOrFeeType),
					Table = typeof(IRefCusTaxOrFee),
					Column = nameof(IRefCusTaxOrFee.ZZF_ZX0_NKTaxOrFeeType),
					ReferencedColumn = nameof(IRefCusTaxOrFeeType.ZX0_TaxOrFeeType),
					ColumnPartOfUniqueIndex = false
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IUNDGSubstanceADR) && o.Table == typeof(IUNDGAttributeZZ)))
			{
				foreignKeyRelationshipList.Add(new ForeignKeyRelationship
				{
					ReferencedTable = typeof(IUNDGSubstanceADR),
					ReferencedColumn = nameof(IUNDGSubstanceADR.ADR_PK),
					Table = typeof(IUNDGAttributeZZ),
					Column = nameof(IUNDGAttributeZZ.DAZ_ParentPK),
					ColumnPartOfUniqueIndex = true
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IUNDGSubstanceRID) && o.Table == typeof(IUNDGAttributeZZ)))
			{
				foreignKeyRelationshipList.Add(new ForeignKeyRelationship
				{
					ReferencedTable = typeof(IUNDGSubstanceRID),
					ReferencedColumn = nameof(IUNDGSubstanceRID.RID_PK),
					Table = typeof(IUNDGAttributeZZ),
					Column = nameof(IUNDGAttributeZZ.DAZ_ParentPK),
					ColumnPartOfUniqueIndex = true
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IUNDGSubstanceADN) && o.Table == typeof(IUNDGAttributeZZ)))
			{
				foreignKeyRelationshipList.Add(new ForeignKeyRelationship
				{
					ReferencedTable = typeof(IUNDGSubstanceADN),
					ReferencedColumn = nameof(IUNDGSubstanceADN.ADN_PK),
					Table = typeof(IUNDGAttributeZZ),
					Column = nameof(IUNDGAttributeZZ.DAZ_ParentPK),
					ColumnPartOfUniqueIndex = true
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IUNDGSubstanceJTT) && o.Table == typeof(IUNDGAttributeZZ)))
			{
				foreignKeyRelationshipList.Add(new ForeignKeyRelationship
				{
					ReferencedTable = typeof(IUNDGSubstanceJTT),
					ReferencedColumn = nameof(IUNDGSubstanceJTT.JTT_PK),
					Table = typeof(IUNDGAttributeZZ),
					Column = nameof(IUNDGAttributeZZ.DAZ_ParentPK),
					ColumnPartOfUniqueIndex = true
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IUNDGSubstanceCFR) && o.Table == typeof(IUNDGAttributeZZ)))
			{
				foreignKeyRelationshipList.Add(new ForeignKeyRelationship
				{
					ReferencedTable = typeof(IUNDGSubstanceCFR),
					ReferencedColumn = nameof(IUNDGSubstanceCFR.CFR_PK),
					Table = typeof(IUNDGAttributeZZ),
					Column = nameof(IUNDGAttributeZZ.DAZ_ParentPK),
					ColumnPartOfUniqueIndex = true
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IRefCusTariffAdditionalCode) && o.Table == typeof(IRefCusTariffAdditionalCodeCategory)))
			{
				foreignKeyRelationshipList.AddRange(new ForeignKeyRelationship[] {
					new ForeignKeyRelationship
					{
						Table = typeof(IRefCusTariffAdditionalCode),
						Column = nameof(IRefCusTariffAdditionalCode.ZY2_ZY3_NKCategory),
						ReferencedTable = typeof(IRefCusTariffAdditionalCodeCategory),
						ReferencedColumn = nameof(IRefCusTariffAdditionalCodeCategory.ZY3_Category),
						ColumnPartOfUniqueIndex = true
					},
					new ForeignKeyRelationship
					{
						Table = typeof(IRefCusTariffAdditionalCode),
						Column = nameof(IRefCusTariffAdditionalCode.ZY2_ZZZ_NKDataGrouping),
						ReferencedTable = typeof(IRefCusTariffAdditionalCodeCategory),
						ReferencedColumn = nameof(IRefCusTariffAdditionalCodeCategory.ZY3_ZZZ_NKDataGrouping),
						ColumnPartOfUniqueIndex = true
					}
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IRefCusCodeType) && o.Table == typeof(IRefCusCodeList)))
			{
				foreignKeyRelationshipList.AddRange(new ForeignKeyRelationship[] {
					new ForeignKeyRelationship
					{
						Table = typeof(IRefCusCodeList),
						Column = nameof(IRefCusCodeList.ZZD_ZZK_NKCodeType),
						ReferencedTable = typeof(IRefCusCodeType),
						ReferencedColumn = nameof(IRefCusCodeType.ZZK_CodeType),
						ColumnPartOfUniqueIndex = true
					},
					new ForeignKeyRelationship
					{
						Table = typeof(IRefCusCodeList),
						Column = nameof(IRefCusCodeList.ZZD_ZZZ_NKDataGrouping),
						ReferencedTable = typeof(IRefCusCodeType),
						ReferencedColumn = nameof(IRefCusCodeType.ZZK_ZZZ_NKDataGrouping),
						ColumnPartOfUniqueIndex = true
					},
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IRefCusCodeType) && o.Table == typeof(IRefCusCodeListAttributeName)))
			{
				foreignKeyRelationshipList.AddRange(new ForeignKeyRelationship[] {
					new ForeignKeyRelationship
					{
						Table = typeof(IRefCusCodeListAttributeName),
						Column = nameof(IRefCusCodeListAttributeName.ZXE_ZZK_NKCodeType),
						ReferencedTable = typeof(IRefCusCodeType),
						ReferencedColumn = nameof(IRefCusCodeType.ZZK_CodeType),
						FKNameOrId = "CodeType",
						ColumnPartOfUniqueIndex = true
					},
					new ForeignKeyRelationship
					{
						Table = typeof(IRefCusCodeListAttributeName),
						Column = nameof(IRefCusCodeListAttributeName.ZXE_ZZZ_NKDataGrouping),
						ReferencedTable = typeof(IRefCusCodeType),
						ReferencedColumn = nameof(IRefCusCodeType.ZZK_ZZZ_NKDataGrouping),
						FKNameOrId = "CodeType",
						ColumnPartOfUniqueIndex = true
					},
					new ForeignKeyRelationship
					{
						Table = typeof(IRefCusCodeListAttributeName),
						Column = nameof(IRefCusCodeListAttributeName.ZXE_ZZK_NKCodeTypeForValueList),
						ReferencedTable = typeof(IRefCusCodeType),
						ReferencedColumn = nameof(IRefCusCodeType.ZZK_CodeType),
						IsNullable = true,
						FKNameOrId = "CodeTypeValueList",
						ColumnPartOfUniqueIndex = false
					},
					new ForeignKeyRelationship
					{
						Table = typeof(IRefCusCodeListAttributeName),
						Column = nameof(IRefCusCodeListAttributeName.ZXE_ZZZ_NKDataGrouping),
						ReferencedTable = typeof(IRefCusCodeType),
						ReferencedColumn = nameof(IRefCusCodeType.ZZK_ZZZ_NKDataGrouping),
						FKNameOrId = "CodeTypeValueList",
						ColumnPartOfUniqueIndex = true
					},
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IRefUNLOCO) && o.Table == typeof(IRefUNLOCOUtcOffset)))
			{
				foreignKeyRelationshipList.Add(new ForeignKeyRelationship
				{
					ReferencedTable = typeof(IRefUNLOCO),
					ReferencedColumn = nameof(IRefUNLOCO.RL_Code),
					Table = typeof(IRefUNLOCOUtcOffset),
					Column = nameof(IRefUNLOCOUtcOffset.RLO_RL_NKCode),
					ColumnPartOfUniqueIndex = true
				});
			}
			if (!foreignKeyRelationshipList.Any(o => o.ReferencedTable == typeof(IRefUNLOCO) && o.Table == typeof(IRefUNLOCORelatedPort)))
			{
				foreignKeyRelationshipList.Add(new ForeignKeyRelationship
				{
					ReferencedTable = typeof(IRefUNLOCO),
					ReferencedColumn = nameof(IRefUNLOCO.RL_Code),
					Table = typeof(IRefUNLOCORelatedPort),
					Column = nameof(IRefUNLOCORelatedPort.RLR_RL_NKRelatedPort),
					ColumnPartOfUniqueIndex = true
				});
			}
		}

		protected override IEnumerable<Type> GetAllStorageTypes()
		{
			var localTypes = Assembly.GetExecutingAssembly().GetTypes();
			var sharedTypes = typeof(IDBHelper).Assembly.GetTypes().Where(x => !localTypes.Any(y => y.Name == x.Name));
			return localTypes.Concat(sharedTypes).Where(x => typeof(IDataSetStorage).IsAssignableFrom(x));
		}
	}
}

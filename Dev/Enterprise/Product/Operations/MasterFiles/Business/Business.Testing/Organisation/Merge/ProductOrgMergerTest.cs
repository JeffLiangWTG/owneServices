using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Organisation.Merge
{
	[TestedType(typeof(ProductOrgMerger))]
	sealed class ProductOrgMergerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProductOrgMerger(new MergeOrgHeader(Factory, null));
		}

		public void TestMergeWithSelf()
		{
			var mergeOrgHeader = new MergeOrgHeader(Factory, org2, org2);
			var merger = new ProductOrgMerger(mergeOrgHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Count (single)", 0, merger.SingleRelationDuplicateCount);
				AssertEquals("Count (multi)", 0, merger.MultiRelationDuplicateCount);
				AssertEquals("Count (total)", 0, merger.TotalDuplicateCount);
				AssertEquals("Part Numbers", "", string.Join(", ", merger.SingleRelationDuplicatePartNumbers));
				AssertEquals("Part Numbers", "", string.Join(", ", merger.MultiRelationDuplicatePartNumbers));
			});

			merger.DeactivateSingleRelationDuplicates();
			merger.DeactivateMultiRelationDuplicates();
			merger.DeleteMultiRelationDuplicateRelations();

			AssertMultilineASCIIEquals(
				"nothing should be changed",
				string.Join("\r\n", new string[]
				{
					"PROD11, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD11, MERGE-TEST (target non-duplicate), 1, OWN, TESTORG3",
					"PROD11, MERGE-TEST (target non-duplicate), 1, SUP, TESTORG1",
					"PROD12, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD12, MERGE-TEST (source non-duplicate), 1, SUP, TESTORG1",
					"PROD13, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD13, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG3",
					"PROD21, MERGE-TEST (source orphan. dupl.), 1, OWN, TESTORG2",
					"PROD21, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1",
					"PROD22, MERGE-TEST (source orphan. dupl.), 1, BTH, TESTORG2",
					"PROD22, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1",
					"PROD31, MERGE-TEST (source dup. rel org1), 1, OWN, TESTORG2",
					"PROD31, MERGE-TEST (source dup. rel org1), 1, SUP, TESTORG1",
					"PROD31, MERGE-TEST (target dupl. product), 1, BTH, TESTORG1",
					"PROD41, MERGE-TEST (source dup. rel org3), 1, OWN, TESTORG2",
					"PROD41, MERGE-TEST (source dup. rel org3), 1, OWN, TESTORG3",
					"PROD41, MERGE-TEST (target dupl. product), 1, BTH, TESTORG1",
					"PROD42, MERGE-TEST (source dup. rel org4), 1, OWN, TESTORG2",
					"PROD42, MERGE-TEST (source dup. rel org4), 1, OWN, TESTORG4",
					"PROD42, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1"
				}),
				string.Join("\r\n", FetchRelations())
			);

			AssertMultilineASCIIEquals(
				"",
				string.Join("\r\n", FetchLogs())
			);
		}

		public void TestSingleRelationDuplicates()
		{
			var mergeOrgHeader = new MergeOrgHeader(Factory, org2, org1);
			var merger = new ProductOrgMerger(mergeOrgHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, merger.SingleRelationDuplicateCount);
				AssertEquals("Total", 5, merger.TotalDuplicateCount);
				AssertEquals("Part Numbers", "PROD21, PROD22", string.Join(", ", merger.SingleRelationDuplicatePartNumbers));
			});

			merger.DeactivateSingleRelationDuplicates();

			AssertMultilineASCIIEquals(
				"single-relation duplicates are deactivated",
				string.Join("\r\n", new string[]
				{
					"PROD11, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD11, MERGE-TEST (target non-duplicate), 1, OWN, TESTORG3",
					"PROD11, MERGE-TEST (target non-duplicate), 1, SUP, TESTORG1",
					"PROD12, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD12, MERGE-TEST (source non-duplicate), 1, SUP, TESTORG1",
					"PROD13, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD13, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG3",
					"PROD21, MERGE-TEST (source orphan. dupl.), 0, OWN, TESTORG2",
					"PROD21, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1",
					"PROD22, MERGE-TEST (source orphan. dupl.), 0, BTH, TESTORG2",
					"PROD22, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1",
					"PROD31, MERGE-TEST (source dup. rel org1), 1, OWN, TESTORG2",
					"PROD31, MERGE-TEST (source dup. rel org1), 1, SUP, TESTORG1",
					"PROD31, MERGE-TEST (target dupl. product), 1, BTH, TESTORG1",
					"PROD41, MERGE-TEST (source dup. rel org3), 1, OWN, TESTORG2",
					"PROD41, MERGE-TEST (source dup. rel org3), 1, OWN, TESTORG3",
					"PROD41, MERGE-TEST (target dupl. product), 1, BTH, TESTORG1",
					"PROD42, MERGE-TEST (source dup. rel org4), 1, OWN, TESTORG2",
					"PROD42, MERGE-TEST (source dup. rel org4), 1, OWN, TESTORG4",
					"PROD42, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1"
				}),
				string.Join("\r\n", FetchRelations())
			);

			AssertMultilineASCIIEquals(
				string.Join("\r\n", new string[]
				{
					$"PROD21, MERGE-TEST (source orphan. dupl.), OrgSupplierPart, EDT, {GlbStaff.CurrentUser.GS_Code}, Deactivated Duplicate Part (Merge organization TESTORG2 into TESTORG1)",
					$"PROD22, MERGE-TEST (source orphan. dupl.), OrgSupplierPart, EDT, {GlbStaff.CurrentUser.GS_Code}, Deactivated Duplicate Part (Merge organization TESTORG2 into TESTORG1)"
				}),
				string.Join("\r\n", FetchLogs())
			);

			merger.ReloadStats();

			CombineAssertions(() =>
			{
				AssertEquals("Count", 0, merger.SingleRelationDuplicateCount);
				AssertEquals("Total", 3, merger.TotalDuplicateCount);
				AssertEquals("Part Numbers", "", string.Join(", ", merger.SingleRelationDuplicatePartNumbers));
			});
		}

		public void TestMultiRelationDuplicates_Deactivate()
		{
			var mergeOrgHeader = new MergeOrgHeader(Factory, org2, org1);
			var merger = new ProductOrgMerger(mergeOrgHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Count", 3, merger.MultiRelationDuplicateCount);
				AssertEquals("Total", 5, merger.TotalDuplicateCount);
				AssertEquals("Part Numbers", "PROD31, PROD41, PROD42", string.Join(", ", merger.MultiRelationDuplicatePartNumbers));
			});

			merger.DeactivateMultiRelationDuplicates();

			AssertMultilineASCIIEquals(
				"duplicate products with more than one relation should be deactivated",
				string.Join("\r\n", new string[]
				{
					"PROD11, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD11, MERGE-TEST (target non-duplicate), 1, OWN, TESTORG3",
					"PROD11, MERGE-TEST (target non-duplicate), 1, SUP, TESTORG1",
					"PROD12, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD12, MERGE-TEST (source non-duplicate), 1, SUP, TESTORG1",
					"PROD13, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD13, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG3",
					"PROD21, MERGE-TEST (source orphan. dupl.), 1, OWN, TESTORG2",
					"PROD21, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1",
					"PROD22, MERGE-TEST (source orphan. dupl.), 1, BTH, TESTORG2",
					"PROD22, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1",
					"PROD31, MERGE-TEST (source dup. rel org1), 0, OWN, TESTORG2",
					"PROD31, MERGE-TEST (source dup. rel org1), 0, SUP, TESTORG1",
					"PROD31, MERGE-TEST (target dupl. product), 1, BTH, TESTORG1",
					"PROD41, MERGE-TEST (source dup. rel org3), 0, OWN, TESTORG2",
					"PROD41, MERGE-TEST (source dup. rel org3), 0, OWN, TESTORG3",
					"PROD41, MERGE-TEST (target dupl. product), 1, BTH, TESTORG1",
					"PROD42, MERGE-TEST (source dup. rel org4), 0, OWN, TESTORG2",
					"PROD42, MERGE-TEST (source dup. rel org4), 0, OWN, TESTORG4",
					"PROD42, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1"
				}),
				string.Join("\r\n", FetchRelations())
			);

			AssertMultilineASCIIEquals(
				string.Join("\r\n", new string[]
				{
					$"PROD31, MERGE-TEST (source dup. rel org1), OrgSupplierPart, EDT, {GlbStaff.CurrentUser.GS_Code}, Deactivated Duplicate Part (Merge organization TESTORG2 into TESTORG1)",
					$"PROD41, MERGE-TEST (source dup. rel org3), OrgSupplierPart, EDT, {GlbStaff.CurrentUser.GS_Code}, Deactivated Duplicate Part (Merge organization TESTORG2 into TESTORG1)",
					$"PROD42, MERGE-TEST (source dup. rel org4), OrgSupplierPart, EDT, {GlbStaff.CurrentUser.GS_Code}, Deactivated Duplicate Part (Merge organization TESTORG2 into TESTORG1)"
				}),
				string.Join("\r\n", FetchLogs())
			);

			merger.ReloadStats();

			CombineAssertions(() =>
			{
				AssertEquals("Count", 0, merger.MultiRelationDuplicateCount);
				AssertEquals("Total", 2, merger.TotalDuplicateCount);
				AssertEquals("Part Numbers", "", string.Join(", ", merger.MultiRelationDuplicatePartNumbers));
			});
		}

		public void TestMultiRelationDuplicates_DeleteRelation()
		{
			var mergeOrgHeader = new MergeOrgHeader(Factory, org2, org1);
			var merger = new ProductOrgMerger(mergeOrgHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Count", 3, merger.MultiRelationDuplicateCount);
				AssertEquals("Total", 5, merger.TotalDuplicateCount);
				AssertEquals("Part Numbers", "PROD31, PROD41, PROD42", string.Join(", ", merger.MultiRelationDuplicatePartNumbers));
			});

			merger.DeleteMultiRelationDuplicateRelations();

			AssertMultilineASCIIEquals(
				"relations from duplicate products with more than one relation should be deleted",
				string.Join("\r\n", new string[]
				{
					"PROD11, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD11, MERGE-TEST (target non-duplicate), 1, OWN, TESTORG3",
					"PROD11, MERGE-TEST (target non-duplicate), 1, SUP, TESTORG1",
					"PROD12, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD12, MERGE-TEST (source non-duplicate), 1, SUP, TESTORG1",
					"PROD13, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD13, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG3",
					"PROD21, MERGE-TEST (source orphan. dupl.), 1, OWN, TESTORG2",
					"PROD21, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1",
					"PROD22, MERGE-TEST (source orphan. dupl.), 1, BTH, TESTORG2",
					"PROD22, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1",
					"PROD31, MERGE-TEST (source dup. rel org1), 1, SUP, TESTORG1",
					"PROD31, MERGE-TEST (target dupl. product), 1, BTH, TESTORG1",
					"PROD41, MERGE-TEST (source dup. rel org3), 1, OWN, TESTORG3",
					"PROD41, MERGE-TEST (target dupl. product), 1, BTH, TESTORG1",
					"PROD42, MERGE-TEST (source dup. rel org4), 1, OWN, TESTORG4",
					"PROD42, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1"
				}),
				string.Join("\r\n", FetchRelations())
			);

			AssertMultilineASCIIEquals(
				string.Join("\r\n", new string[]
				{
					$"PROD31, MERGE-TEST (source dup. rel org1), OrgSupplierPart, EDT, {GlbStaff.CurrentUser.GS_Code}, Remove Relations from Duplicate Part (Merge organization TESTORG2 into TESTORG1)",
					$"PROD41, MERGE-TEST (source dup. rel org3), OrgSupplierPart, EDT, {GlbStaff.CurrentUser.GS_Code}, Remove Relations from Duplicate Part (Merge organization TESTORG2 into TESTORG1)",
					$"PROD42, MERGE-TEST (source dup. rel org4), OrgSupplierPart, EDT, {GlbStaff.CurrentUser.GS_Code}, Remove Relations from Duplicate Part (Merge organization TESTORG2 into TESTORG1)"
				}),
				string.Join("\r\n", FetchLogs())
			);

			merger.ReloadStats();

			CombineAssertions(() =>
			{
				AssertEquals("Count", 0, merger.MultiRelationDuplicateCount);
				AssertEquals("Total", 2, merger.TotalDuplicateCount);
				AssertEquals("Part Numbers", "", string.Join(", ", merger.MultiRelationDuplicatePartNumbers));
			});
		}

		OrgHeader org1, org2, org3, org4;

		protected override void SetUp()
		{
			base.SetUp();

			org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TESTORG1";

			org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TESTORG2";

			org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "TESTORG3";

			org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "TESTORG4";

			var product011 = Factory.New<OrgSupplierPart>();
			product011.OP_PartNum = "PROD11";
			product011.OP_Desc = "MERGE-TEST (target non-duplicate)";
			product011.RelatedOrganisations.AddOwner(org3);
			product011.RelatedOrganisations.AddSupplier(org1);

			var product021 = Factory.New<OrgSupplierPart>();
			product021.OP_PartNum = "PROD21";
			product021.OP_Desc = "MERGE-TEST (target dupl. product)";
			product021.RelatedOrganisations.AddOwner(org1);

			var product022 = Factory.New<OrgSupplierPart>();
			product022.OP_PartNum = "PROD22";
			product022.OP_Desc = "MERGE-TEST (target dupl. product)";
			product022.RelatedOrganisations.AddOwner(org1);

			var product031 = Factory.New<OrgSupplierPart>();
			product031.OP_PartNum = "PROD31";
			product031.OP_Desc = "MERGE-TEST (target dupl. product)";
			product031.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Both);

			var product041 = Factory.New<OrgSupplierPart>();
			product041.OP_PartNum = "PROD41";
			product041.OP_Desc = "MERGE-TEST (target dupl. product)";
			product041.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, OrgPartRelation.RelationshipTypes.Both);

			var product042 = Factory.New<OrgSupplierPart>();
			product042.OP_PartNum = "PROD42";
			product042.OP_Desc = "MERGE-TEST (target dupl. product)";
			product042.RelatedOrganisations.AddOwner(org1);

			var product11 = Factory.New<OrgSupplierPart>();
			product11.OP_PartNum = "PROD11";
			product11.OP_Desc = "MERGE-TEST (source non-duplicate)";
			product11.RelatedOrganisations.AddOwner(org2);

			var product12 = Factory.New<OrgSupplierPart>();
			product12.OP_PartNum = "PROD12";
			product12.OP_Desc = "MERGE-TEST (source non-duplicate)";
			product12.RelatedOrganisations.AddSupplier(org1);
			product12.RelatedOrganisations.AddOwner(org2);

			var product13 = Factory.New<OrgSupplierPart>();
			product13.OP_PartNum = "PROD13";
			product13.OP_Desc = "MERGE-TEST (source non-duplicate)";
			product13.RelatedOrganisations.AddOwner(org2);
			product13.RelatedOrganisations.AddOwner(org3);

			var product21 = Factory.New<OrgSupplierPart>();
			product21.OP_PartNum = "PROD21";
			product21.OP_Desc = "MERGE-TEST (source orphan. dupl.)";
			product21.RelatedOrganisations.AddOwner(org2);

			var product22 = Factory.New<OrgSupplierPart>();
			product22.OP_PartNum = "PROD22";
			product22.OP_Desc = "MERGE-TEST (source orphan. dupl.)";
			product22.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, OrgPartRelation.RelationshipTypes.Both);

			var product31 = Factory.New<OrgSupplierPart>();
			product31.OP_PartNum = "PROD31";
			product31.OP_Desc = "MERGE-TEST (source dup. rel org1)";
			product31.RelatedOrganisations.AddSupplier(org1);
			product31.RelatedOrganisations.AddOwner(org2);

			var product41 = Factory.New<OrgSupplierPart>();
			product41.OP_PartNum = "PROD41";
			product41.OP_Desc = "MERGE-TEST (source dup. rel org3)";
			product41.RelatedOrganisations.AddOwner(org2);
			product41.RelatedOrganisations.AddOwner(org3);

			var product42 = Factory.New<OrgSupplierPart>();
			product42.OP_PartNum = "PROD42";
			product42.OP_Desc = "MERGE-TEST (source dup. rel org4)";
			product42.RelatedOrganisations.AddOwner(org2);
			product42.RelatedOrganisations.AddOwner(org4);

			Factory.Save();

			AssertMultilineASCIIEquals(
				"(pre-condition) test setup",
				string.Join("\r\n", new string[]
				{
					"PROD11, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD11, MERGE-TEST (target non-duplicate), 1, OWN, TESTORG3",
					"PROD11, MERGE-TEST (target non-duplicate), 1, SUP, TESTORG1",
					"PROD12, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD12, MERGE-TEST (source non-duplicate), 1, SUP, TESTORG1",
					"PROD13, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG2",
					"PROD13, MERGE-TEST (source non-duplicate), 1, OWN, TESTORG3",
					"PROD21, MERGE-TEST (source orphan. dupl.), 1, OWN, TESTORG2",
					"PROD21, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1",
					"PROD22, MERGE-TEST (source orphan. dupl.), 1, BTH, TESTORG2",
					"PROD22, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1",
					"PROD31, MERGE-TEST (source dup. rel org1), 1, OWN, TESTORG2",
					"PROD31, MERGE-TEST (source dup. rel org1), 1, SUP, TESTORG1",
					"PROD31, MERGE-TEST (target dupl. product), 1, BTH, TESTORG1",
					"PROD41, MERGE-TEST (source dup. rel org3), 1, OWN, TESTORG2",
					"PROD41, MERGE-TEST (source dup. rel org3), 1, OWN, TESTORG3",
					"PROD41, MERGE-TEST (target dupl. product), 1, BTH, TESTORG1",
					"PROD42, MERGE-TEST (source dup. rel org4), 1, OWN, TESTORG2",
					"PROD42, MERGE-TEST (source dup. rel org4), 1, OWN, TESTORG4",
					"PROD42, MERGE-TEST (target dupl. product), 1, OWN, TESTORG1"
				}),
				string.Join("\r\n", FetchRelations())
			);
		}

		static List<string> FetchRelations()
		{
			var relations = new List<string>();
			Db.Connection.ExecuteReader
			(
				@"
					select OP_PartNum, OP_Desc, OP_IsActive, OU_Relationship, OH_Code 
					from dbo.OrgSupplierPart
					left join dbo.OrgPartRelation on OU_OP=OP_PK
					left join dbo.OrgHeader on OH_PK=OU_OH
					where OP_Desc like 'MERGE-TEST%'
					order by OP_PartNum, OP_Desc, OU_Relationship, OH_Code
				", reader => relations.Add(
					$"{reader["OP_PartNum"]}, {reader["OP_Desc"]}, {((bool)reader["OP_IsActive"] ? 1 : 0)}, {reader["OU_Relationship"]}, {reader["OH_Code"]}"
				)
			);
			return relations;
		}

		static List<string> FetchLogs()
		{
			var logs = new List<string>();
			Db.Connection.ExecuteReader
			(
				@"
					select OP_PartNum, OP_Desc, SL_Table, SL_SE_NKEvent, SL_GS_NKUser, SL_Reference
					from dbo.OrgSupplierPart
					inner join dbo.StmALog on SL_Parent=OP_PK
					where OP_Desc like 'MERGE-TEST%' and SL_Reference like '%Merge%'
					order by OP_PartNum, SL_Table, SL_SE_NKEvent, SL_GS_NKUser, SL_Reference
				", reader => logs.Add(
					$"{reader["OP_PartNum"]}, {reader["OP_Desc"]}, {reader["SL_Table"]}, {reader["SL_SE_NKEvent"]}, {((string)reader["SL_GS_NKUser"])?.Trim()}, {reader["SL_Reference"]}"
				)
			);
			return logs;
		}
	}
}

using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierBulkRelationshipChanger))]
	sealed class OrgSupplierBulkRelationshipChangerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertEquals(typeof(OrgHeaderCollection), BusinessObjectForTest.OrganisationList.GetType());

			AssertEquals(typeof(CodeDescriptionPairList), BusinessObjectForTest.RelationshipTypeList.GetType());
			AssertEquals(4, BusinessObjectForTest.RelationshipTypeList.Count);
			Assert(BusinessObjectForTest.RelationshipTypeList.ContainsCode(OrgPartRelation.RelationshipTypes.Both));
			Assert(BusinessObjectForTest.RelationshipTypeList.ContainsCode(OrgPartRelation.RelationshipTypes.Owner));
			Assert(BusinessObjectForTest.RelationshipTypeList.ContainsCode(OrgPartRelation.RelationshipTypes.Supplier));
			Assert(BusinessObjectForTest.RelationshipTypeList.ContainsCode(OrgPartRelation.RelationshipTypes.WarehouseConsignee));
		}

		public void TestValidationFromOrganisation()
		{
			BusinessObjectForTest.FromOrganisationPK = ZGuid.Invalid;

			Assert(BusinessObjectForTest.FromOrganisationPKInfo.HasErrors());
			AssertEquals(2, BusinessObjectForTest.FromOrganisationPKInfo.GetErrors().Count());

			OrganisationForTest1.OH_IsActive = false;
			BusinessObjectForTest.FromOrganisationPK = OrganisationForTest1.PK;

			Assert(BusinessObjectForTest.FromOrganisationPKInfo.HasErrors());
			AssertEquals(1, BusinessObjectForTest.FromOrganisationPKInfo.GetErrors().Count());
			AssertEquals((NoResString)"This Organization is marked as inactive, please enter a valid Organization.", BusinessObjectForTest.FromOrganisationPKInfo.GetErrors().GetFirstMessage());
		}

		public void TestValidationFromRelationship()
		{
			BusinessObjectForTest.FromRelationship = "fff";

			Assert(BusinessObjectForTest.FromRelationshipInfo.HasErrors());
			AssertEquals(1, BusinessObjectForTest.FromRelationshipInfo.GetErrors().Count());

			OrganisationForTest1.OH_IsActive = true;
			BusinessObjectForTest.FromOrganisationPK = OrganisationForTest1.PK;
			BusinessObjectForTest.FromRelationship = OrgPartRelation.RelationshipTypes.Both;

			Assert(!BusinessObjectForTest.FromRelationshipInfo.HasErrors());
		}

		public void TestValidationToOrganisation()
		{
			BusinessObjectForTest.ToOrganisationPK = ZGuid.Invalid;

			Assert(BusinessObjectForTest.ToOrganisationPKInfo.HasErrors());
			AssertEquals(3, BusinessObjectForTest.ToOrganisationPKInfo.GetErrors().Count());

			OrganisationForTest1.OH_IsActive = false;
			BusinessObjectForTest.ToOrganisationPK = OrganisationForTest1.PK;

			Assert(BusinessObjectForTest.ToOrganisationPKInfo.HasErrors());
			AssertEquals(1, BusinessObjectForTest.ToOrganisationPKInfo.GetErrors().Count());
			AssertEquals((NoResString)"This Organization is marked as inactive, please enter a valid Organization.", BusinessObjectForTest.ToOrganisationPKInfo.GetErrors().GetFirstMessage());

			OrganisationForTest1.OH_IsActive = true;
			BusinessObjectForTest.ToOrganisationPK = OrganisationForTest1.PK;

			Assert(!BusinessObjectForTest.ToOrganisationPKInfo.HasErrors());
		}

		public void TestValidationToRelationship()
		{
			BusinessObjectForTest.ToRelationship = "fff";

			Assert(BusinessObjectForTest.ToRelationshipInfo.HasErrors());
			AssertEquals(1, BusinessObjectForTest.ToRelationshipInfo.GetErrors().Count());

			BusinessObjectForTest.ToRelationship = ZString.Empty;

			Assert(BusinessObjectForTest.ToRelationshipInfo.HasErrors());
			AssertEquals(1, BusinessObjectForTest.ToRelationshipInfo.GetErrors().Count());
			AssertEquals("Please enter valid relationship.", BusinessObjectForTest.ToRelationshipInfo.GetErrors().GetFirstMessage());

			BusinessObjectForTest.ToRelationship = OrgPartRelation.RelationshipTypes.Both;

			Assert(!BusinessObjectForTest.ToRelationshipInfo.HasErrors());
		}

		public void TestValidationGeneral()
		{
			OrganisationForTest1.OH_IsActive = true;
			OrganisationForTest2.OH_IsActive = true;

			SetValueBusinessObjectForTest(OrganisationForTest1.PK, OrganisationForTest1.PK, OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.Owner);

			Assert(BusinessObjectForTest.FromOrganisationPKInfo.HasErrors());
			AssertEquals(1, BusinessObjectForTest.FromOrganisationPKInfo.GetErrors().Count());
			AssertEquals((NoResString)"The From and To Organizations are the same, leave the To Organization blank or enter a different Organization.", BusinessObjectForTest.FromOrganisationPKInfo.GetErrors().GetFirstMessage());
			Assert(BusinessObjectForTest.ToOrganisationPKInfo.HasErrors());
			AssertEquals(1, BusinessObjectForTest.ToOrganisationPKInfo.GetErrors().Count());
			AssertEquals((NoResString)"The From and To Organizations are the same, leave the To Organization blank or enter a different Organization.", BusinessObjectForTest.ToOrganisationPKInfo.GetErrors().GetFirstMessage());

			SetValueBusinessObjectForTest(OrganisationForTest1.PK, OrganisationForTest2.PK, OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.Owner);

			Assert(!BusinessObjectForTest.FromOrganisationPKInfo.HasErrors());
			Assert(!BusinessObjectForTest.ToRelationshipInfo.HasErrors());
		}

		#region Implementation

		#region OrganisationForTest1

		OrgHeader OrganisationForTest1
		{
			get
			{
				if (organisationForTest1 == null)
				{
					organisationForTest1 = Factory.NewWithValidTestData<OrgHeader>();
				}

				return organisationForTest1;
			}
		}
		OrgHeader organisationForTest1;

		#endregion

		#region OrganisationForTest2

		OrgHeader OrganisationForTest2
		{
			get
			{
				if (organisationForTest2 == null)
				{
					organisationForTest2 = Factory.NewWithValidTestData<OrgHeader>();
				}

				return organisationForTest2;
			}
		}
		OrgHeader organisationForTest2;

		#endregion

		#region BusinessObjectForTest

		OrgSupplierBulkRelationshipChanger BusinessObjectForTest
		{
			get
			{
				if (businessObjectForTest == null)
				{
					businessObjectForTest = new OrgSupplierBulkRelationshipChanger(Factory);
				}
				return businessObjectForTest;
			}
		}
		OrgSupplierBulkRelationshipChanger businessObjectForTest;

		#endregion

		void SetValueBusinessObjectForTest(ZGuid fromOrganisationPK, ZGuid toOrganisationPK, ZString fromRelationship, ZString toRelationship)
		{
			BusinessObjectForTest.FromOrganisationPK = fromOrganisationPK;
			BusinessObjectForTest.FromRelationship = fromRelationship;
			BusinessObjectForTest.ToOrganisationPK = toOrganisationPK;
			BusinessObjectForTest.ToRelationship = toRelationship;
		}

		#endregion

		public void TestChangeRelatedOrganisations1()
		{
			OrgHeader organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "Test1";
			organisation1.MainAddress.OA_Address1 = "Address 1";

			OrgHeader organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "Test2";
			organisation2.MainAddress.OA_Address1 = "Address 2";

			OrgHeader organisation3 = Factory.New<OrgHeader>();
			organisation3.OH_Code = "Test3";
			organisation3.MainAddress.OA_Address1 = "Address 3";

			OrgSupplierPart part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "1";
			OrgPartRelation relationPart1 = part1.RelatedOrganisations.AddNew();
			relationPart1.OU_OH = organisation1.PK;
			relationPart1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			OrgSupplierPart part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "2";
			OrgPartRelation relationPart2 = part2.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = organisation1.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			OrgPartRelation relationPart2_1 = part2.RelatedOrganisations.AddNew();
			relationPart2_1.OU_OH = organisation2.PK;
			relationPart2_1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			OrgSupplierPart part3 = OrgSupplierPart.New(Factory);
			part3.OP_PartNum = "3";
			OrgPartRelation relationPart3 = part3.RelatedOrganisations.AddNew();
			relationPart3.OU_OH = organisation1.PK;
			relationPart3.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			OrgSupplierPartCollection parts = new OrgSupplierPartCollection(Factory);
			parts.Load();

			AssertEquals(3, parts.Count);
			Assert(parts.Contains(part1.PK));
			Assert(parts.Contains(part2.PK));
			Assert(parts.Contains(part3.PK));

			ZQuery query = new ZQuery();
			query.AddToFilter(OrgSupplierPartSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.NewZGuid()); // This doesn't have to make business sense, it just has to be a filter that contains a GUID in order for us to check that we generate TSQL-formatted WHERE clauses and not ADO-formatted ones. Before this tweak, we generated ADO-formatted SQL, which for a module query with a GUID in it (e.g. limiting by orgs) borked at the DB execute level. 

			OrgSupplierBulkRelationshipChanger changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.FromOrganisationPK = organisation1.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer.ToOrganisationPK = organisation2.PK;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Both;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 2, 0, 0);

			parts = new OrgSupplierPartCollection(new BusinessObjectFactory());
			parts.Load();

			AssertEquals(1, ((OrgSupplierPart)parts.FindByPK(part1.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part1.PK)), organisation2, OrgPartRelation.RelationshipTypes.Both);

			AssertEquals(1, ((OrgSupplierPart)parts.FindByPK(part2.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part2.PK)), organisation2, OrgPartRelation.RelationshipTypes.Both);

			AssertEquals(1, ((OrgSupplierPart)parts.FindByPK(part3.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part3.PK)), organisation1, OrgPartRelation.RelationshipTypes.Supplier);

			changer = new OrgSupplierBulkRelationshipChanger(Factory);
			changer.FromOrganisationPK = organisation2.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Both;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Supplier;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 2, 0, 0);

			parts = new OrgSupplierPartCollection(new BusinessObjectFactory());
			parts.Load();

			AssertEquals(1, ((OrgSupplierPart)parts.FindByPK(part1.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part1.PK)), organisation2, OrgPartRelation.RelationshipTypes.Supplier);

			AssertEquals(1, ((OrgSupplierPart)parts.FindByPK(part2.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part2.PK)), organisation2, OrgPartRelation.RelationshipTypes.Supplier);

			AssertEquals(1, ((OrgSupplierPart)parts.FindByPK(part3.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part3.PK)), organisation1, OrgPartRelation.RelationshipTypes.Supplier);

			changer = new OrgSupplierBulkRelationshipChanger(Factory);
			changer.ToOrganisationPK = organisation1.PK;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Owner;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 2, 1, 0, 0);

			parts = new OrgSupplierPartCollection(new BusinessObjectFactory());
			parts.Load();

			AssertEquals(2, ((OrgSupplierPart)parts.FindByPK(part1.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part1.PK)), organisation2, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part1.PK)), organisation1, OrgPartRelation.RelationshipTypes.Owner);

			AssertEquals(2, ((OrgSupplierPart)parts.FindByPK(part2.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part2.PK)), organisation2, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part2.PK)), organisation1, OrgPartRelation.RelationshipTypes.Owner);

			AssertEquals(1, ((OrgSupplierPart)parts.FindByPK(part3.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part3.PK)), organisation1, OrgPartRelation.RelationshipTypes.Owner);

			changer = new OrgSupplierBulkRelationshipChanger(Factory);
			changer.FromOrganisationPK = organisation1.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Both;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 3, 0, 0);

			parts = new OrgSupplierPartCollection(new BusinessObjectFactory());
			parts.Load();

			AssertEquals(2, ((OrgSupplierPart)parts.FindByPK(part1.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part1.PK)), organisation2, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part1.PK)), organisation1, OrgPartRelation.RelationshipTypes.Both);

			AssertEquals(2, ((OrgSupplierPart)parts.FindByPK(part2.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part2.PK)), organisation2, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part2.PK)), organisation1, OrgPartRelation.RelationshipTypes.Both);

			AssertEquals(1, ((OrgSupplierPart)parts.FindByPK(part3.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part3.PK)), organisation1, OrgPartRelation.RelationshipTypes.Both);

			changer = new OrgSupplierBulkRelationshipChanger(Factory);
			changer.FromOrganisationPK = organisation1.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Both;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Supplier;

			query.AddToFilter(Enterprise.ZArchitecture.Schema.OrgSupplierPartSchema.OP_PartNum, "2");
			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 1, 0, 0);

			parts = new OrgSupplierPartCollection(new BusinessObjectFactory());
			parts.Load();

			AssertEquals(2, ((OrgSupplierPart)parts.FindByPK(part1.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part1.PK)), organisation2, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part1.PK)), organisation1, OrgPartRelation.RelationshipTypes.Both);

			AssertEquals(2, ((OrgSupplierPart)parts.FindByPK(part2.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part2.PK)), organisation2, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part2.PK)), organisation1, OrgPartRelation.RelationshipTypes.Supplier);

			AssertEquals(1, ((OrgSupplierPart)parts.FindByPK(part3.PK)).RelatedOrganisations.Count);
			AssertPartRelationsAreCorrect(((OrgSupplierPart)parts.FindByPK(part3.PK)), organisation1, OrgPartRelation.RelationshipTypes.Both);
		}

		void AssertRecordsAffected(OrgSupplierBulkRelationshipChanger changer, int addedCount, int updatedCount, int failedCount, int skippedCount, int deleteCount = 0)
		{
			CombineAssertions(() =>
				{
					AssertEquals("Expected this many relationships added", addedCount, changer.RelationshipsAdded);
					AssertEquals("Expected this many relationships updated", updatedCount, changer.RelationshipsUpdated);
					AssertEquals("Expected this many relationships failed", failedCount, changer.RelationshipsThatFailedValidaton);
					AssertEquals("Expected this many products skipped", skippedCount, changer.ProductsSkipped); //- We don't really care about this
					AssertEquals("Expected this many relationships deleted", deleteCount, changer.RelationshipsDeleted);
				});
		}

		string FormatRelations(OrgSupplierPart part)
		{
			return string.Join(", ",
				part.RelatedOrganisations
					.Cast<OrgPartRelation>()
					.Select(r => $"{r.OU_Relationship}: {r.Organisation.OH_Code}")
					.OrderBy(t => t)
			);
		}

		public void TestChangeRelatedOrganisations2()
		{
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "Test1";

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "Test2";

			var part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "duplicate";
			var relationPart1 = part1.RelatedOrganisations.AddNew();
			relationPart1.OU_OH = organisation1.PK;
			relationPart1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "duplicate";
			var relationPart2 = part2.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = organisation2.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.FromOrganisationPK = organisation1.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Supplier;
			changer.ToOrganisationPK = organisation2.PK;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Supplier;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 0, 1, 0);

			part1 = new BusinessObjectFactory().Load<OrgSupplierPart>(part1.PK);
			part2 = new BusinessObjectFactory().Load<OrgSupplierPart>(part2.PK);

			AssertEquals(organisation1.PK, part1.RelatedOrganisations[0].OU_OH);
			AssertEquals(organisation2.PK, part2.RelatedOrganisations[0].OU_OH);
		}

		public void TestChangeRelatedOrganisations3()
		{
			var organisationA = Factory.New<OrgHeader>();
			organisationA.OH_Code = "TestA";

			var organisationB = Factory.New<OrgHeader>();
			organisationB.OH_Code = "TestB";

			var part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "duplicate";
			var relationPart1a = part1.RelatedOrganisations.AddNew();
			relationPart1a.OU_OH = organisationA.PK;
			relationPart1a.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var relationPart1b = part1.RelatedOrganisations.AddNew();
			relationPart1b.OU_OH = organisationB.PK;
			relationPart1b.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "duplicate";
			var relationPart2 = part1.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = organisationB.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.FromOrganisationPK = organisationB.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer.ToOrganisationPK = ZGuid.Empty;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Supplier;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 0, 1, 0);
		}

		public void TestChangeRelatedOrganisations4()
		{
			var organisationA = Factory.New<OrgHeader>();
			organisationA.OH_Code = "TestA";

			var organisationB = Factory.New<OrgHeader>();
			organisationB.OH_Code = "TestB";

			var part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "duplicate";
			var relationPart1 = part1.RelatedOrganisations.AddNew();
			relationPart1.OU_OH = organisationA.PK;
			relationPart1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "duplicate";
			var relationPart2 = part1.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = organisationB.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.FromOrganisationPK = organisationA.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Supplier;
			changer.ToOrganisationPK = organisationB.PK;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 1, 0, 0);
		}

		public void TestChangeRelatedOrganisations5()
		{
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "Test1";

			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "Test2";

			var part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "duplicate";
			var relationPart1 = part1.RelatedOrganisations.AddNew();
			relationPart1.OU_OH = organisation1.PK;
			relationPart1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "duplicate";
			var relationPart2 = part2.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = organisation2.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.ToOrganisationPK = organisation2.PK;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Supplier;

			changer.ChangeRelatedOrganisations(query);

			AssertRecordsAffected(changer, 0, 0, 1, 1);

			part1 = new BusinessObjectFactory().Load<OrgSupplierPart>(part1.PK);
			part2 = new BusinessObjectFactory().Load<OrgSupplierPart>(part2.PK);

			AssertEquals("Unchanged", organisation1.PK, part1.RelatedOrganisations[0].OU_OH);
			AssertEquals("Unchanged", organisation2.PK, part2.RelatedOrganisations[0].OU_OH);
		}

		public void TestChangeRelatedOrganisationsWithDuff6()
		{
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "Test1";

			var part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "duplicate";
			var relationPart1 = part1.RelatedOrganisations.AddNew();
			relationPart1.OU_OH = organisation1.PK;
			relationPart1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "duplicate";
			var relationPart2 = part2.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = organisation1.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.ToOrganisationPK = organisation1.PK;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Owner;

			changer.ChangeRelatedOrganisations(query);

			AssertRecordsAffected(changer, 0, 0, 1, 1);

			part1 = new BusinessObjectFactory().Load<OrgSupplierPart>(part1.PK);
			part2 = new BusinessObjectFactory().Load<OrgSupplierPart>(part2.PK);

			AssertEquals("Unchanged", organisation1.PK, part1.RelatedOrganisations[0].OU_OH);
			AssertEquals("Unchanged", organisation1.PK, part2.RelatedOrganisations[0].OU_OH);
		}

		public void TestChangeRelatedOrganisations_DontDeleteWarehouseConsigneeWhenFixingDuplicateOwners()
		{
			var organisationA = Factory.New<OrgHeader>();
			organisationA.OH_Code = "OrgA";

			var part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "PART1";
			var relationPart1a = part1.RelatedOrganisations.AddNew();
			relationPart1a.OU_OH = organisationA.PK;
			relationPart1a.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var relationPart1b = part1.RelatedOrganisations.AddNew();
			relationPart1b.OU_OH = organisationA.PK;
			relationPart1b.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var relationPart1c = part1.RelatedOrganisations.AddNew();
			relationPart1c.OU_OH = organisationA.PK;
			relationPart1c.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;

			var part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "PART2";
			var relationPart2 = part2.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = organisationA.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.FromOrganisationPK = organisationA.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer.ToOrganisationPK = ZGuid.Empty;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Both;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 1, 0, 0, 1);

			part1 = new BusinessObjectFactory().Load<OrgSupplierPart>(part1.PK);
			AssertEquals("BTH: OrgA, WCN: OrgA", FormatRelations(part1));

			part2 = new BusinessObjectFactory().Load<OrgSupplierPart>(part2.PK);
			AssertEquals("BTH: OrgA", FormatRelations(part2));
		}

		public void TestChangeRelatedOrganisations_DontCreateDuplicateProductsFromWarehouseConsignee()
		{
			var organisationA = Factory.New<OrgHeader>();
			organisationA.OH_Code = "OrgA";

			var part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "P1-SELF-DUP";
			var relationPart1a = part1.RelatedOrganisations.AddNew();
			relationPart1a.OU_OH = organisationA.PK;
			relationPart1a.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var relationPart1b = part1.RelatedOrganisations.AddNew();
			relationPart1b.OU_OH = organisationA.PK;
			relationPart1b.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;

			var part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "P2-FINE";
			var relationPart2 = part2.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = organisationA.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;

			var part31 = OrgSupplierPart.New(Factory);
			part31.OP_PartNum = "P3-DUP";
			var relationPart31 = part31.RelatedOrganisations.AddNew();
			relationPart31.OU_OH = organisationA.PK;
			relationPart31.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;

			var part32 = OrgSupplierPart.New(Factory);
			part32.OP_PartNum = "P3-DUP";
			var relationPart32 = part32.RelatedOrganisations.AddNew();
			relationPart32.OU_OH = organisationA.PK;
			relationPart32.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;

			var part41 = OrgSupplierPart.New(Factory);
			part41.OP_PartNum = "P4-HALF-DUP";
			var relationPart41 = part41.RelatedOrganisations.AddNew();
			relationPart41.OU_OH = organisationA.PK;
			relationPart41.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			var part42 = OrgSupplierPart.New(Factory);
			part42.OP_PartNum = "P4-HALF-DUP";
			var relationPart42 = part42.RelatedOrganisations.AddNew();
			relationPart42.OU_OH = organisationA.PK;
			relationPart42.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.FromOrganisationPK = organisationA.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			changer.ToOrganisationPK = ZGuid.Empty;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Owner;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 1, 4, 0);

			var factory2 = new BusinessObjectFactory();

			AssertEquals("failure", "BTH: OrgA, WCN: OrgA", FormatRelations(factory2.Load<OrgSupplierPart>(part1.PK)));
			AssertEquals("success", "OWN: OrgA", FormatRelations(factory2.Load<OrgSupplierPart>(part2.PK)));
			AssertEquals("failure", "WCN: OrgA", FormatRelations(factory2.Load<OrgSupplierPart>(part31.PK)));
			AssertEquals("failure", "WCN: OrgA", FormatRelations(factory2.Load<OrgSupplierPart>(part32.PK)));
			AssertEquals("- n/a -", "BTH: OrgA", FormatRelations(factory2.Load<OrgSupplierPart>(part41.PK)));
			AssertEquals("failure", "WCN: OrgA", FormatRelations(factory2.Load<OrgSupplierPart>(part42.PK)));
		}

		public void TestChangeRelatedOrganisations_DontCreateDuplicateRecords()
		{
			var organisationA = Factory.New<OrgHeader>();
			organisationA.OH_Code = "OrgA";

			var part = OrgSupplierPart.New(Factory);
			part.OP_PartNum = "TEST-PART";
			var relationPart1a = part.RelatedOrganisations.AddNew();
			relationPart1a.OU_OH = organisationA.PK;
			relationPart1a.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var relationPart1b = part.RelatedOrganisations.AddNew();
			relationPart1b.OU_OH = organisationA.PK;
			relationPart1b.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.FromOrganisationPK = organisationA.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			changer.ToOrganisationPK = ZGuid.Empty;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Supplier;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 0, 1, 0);

			var factory2 = new BusinessObjectFactory();

			AssertEquals("failure", "SUP: OrgA, WCN: OrgA", FormatRelations(factory2.Load<OrgSupplierPart>(part.PK)));
		}

		public void TestChangeRelatedOrganisationsSlow_DontCreateDuplicateRecords()
		{
			var orgA = Factory.New<OrgHeader>();
			orgA.OH_Code = "OrgA";
			var orgB = Factory.New<OrgHeader>();
			orgB.OH_Code = "OrgB";
			var orgC = Factory.New<OrgHeader>();
			orgC.OH_Code = "OrgC";
			var orgD = Factory.New<OrgHeader>();
			orgD.OH_Code = "OrgD";

			var part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "Part";
			var part1ToA = part1.RelatedOrganisations.AddNew();
			part1ToA.OU_OH = orgA.PK;
			part1ToA.OU_Relationship = "SUP";
			var partToB = part1.RelatedOrganisations.AddNew();
			partToB.OU_OH = orgB.PK;
			partToB.OU_Relationship = "OWN";

			var part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "Part";
			var part2ToA = part2.RelatedOrganisations.AddNew();
			part2ToA.OU_OH = orgA.PK;
			part2ToA.OU_Relationship = "SUP";
			var part2ToC = part2.RelatedOrganisations.AddNew();
			part2ToC.OU_OH = orgC.PK;
			part2ToC.OU_Relationship = "OWN";

			AssertEquals("Precondition", "OWN: OrgB, SUP: OrgA", FormatRelations(Factory.Load<OrgSupplierPart>(part1.PK)));
			AssertEquals("Precondition", "OWN: OrgC, SUP: OrgA", FormatRelations(Factory.Load<OrgSupplierPart>(part2.PK)));

			Factory.Save();

			var changer = new OrgSupplierBulkRelationshipChanger(Factory)
			{
				FromOrganisationPK = orgA.PK,
				FromRelationship = "SUP",
				ToOrganisationPK = orgD.PK,
				ToRelationship = "OWN"
			};
			changer.ChangeRelatedOrganisations(new ZQuery());
			AssertRecordsAffected(changer, 0, 1, 1, 0);

			var newFactory = new BusinessObjectFactory();

			var part1Relations = FormatRelations(newFactory.Load<OrgSupplierPart>(part1.PK));
			var part2Relations = FormatRelations(newFactory.Load<OrgSupplierPart>(part2.PK));
			if (part1Relations == "OWN: OrgB, OWN: OrgD")
			{
				AssertEquals("OWN: OrgC, SUP: OrgA", part2Relations);
			}
			else
			{
				AssertEquals("OWN: OrgB, SUP: OrgA", part1Relations);
				AssertEquals("OWN: OrgC, OWN: OrgD", part2Relations);
			}
		}

		public void TestChangeRelatedOrganisations_DontDeleteOwnersOnNonOwnerChanges()
		{
			var organisationA = Factory.New<OrgHeader>();
			organisationA.OH_Code = "OrgA";

			var part = OrgSupplierPart.New(Factory);
			part.OP_PartNum = "TEST-PART";
			var relationPart1a = part.RelatedOrganisations.AddNew();
			relationPart1a.OU_OH = organisationA.PK;
			relationPart1a.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var relationPart1b = part.RelatedOrganisations.AddNew();
			relationPart1b.OU_OH = organisationA.PK;
			relationPart1b.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var relationPart1c = part.RelatedOrganisations.AddNew();
			relationPart1c.OU_OH = organisationA.PK;
			relationPart1c.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.FromOrganisationPK = organisationA.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Supplier;
			changer.ToOrganisationPK = ZGuid.Empty;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;

			changer.ChangeRelatedOrganisations(query);
			AssertRecordsAffected(changer, 0, 1, 0, 0);

			var factory2 = new BusinessObjectFactory();

			AssertEquals("success", "BTH: OrgA, OWN: OrgA, WCN: OrgA", FormatRelations(factory2.Load<OrgSupplierPart>(part.PK)));
		}

		public void TestChangeRelatedOrganisationsForDuplicatedRelationships()
		{
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "Test1";

			var part = OrgSupplierPart.New(Factory);
			part.OP_PartNum = "duplicate";

			var relationPart1 = part.RelatedOrganisations.AddNew();
			relationPart1.OU_OH = organisation1.PK;
			relationPart1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var relationPart2 = part.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = organisation1.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.ToOrganisationPK = organisation1.PK;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer.ChangeRelatedOrganisations(query);

			AssertRecordsAffected(changer, 0, 0, 0, 0, 1);

			part = new BusinessObjectFactory().Load<OrgSupplierPart>(part.PK);
			AssertEquals("OWN: Test1", FormatRelations(part));

			var part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "duplicate";

			var relationPart3 = part2.RelatedOrganisations.AddNew();
			relationPart3.OU_OH = organisation1.PK;
			relationPart3.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var relationPart4 = part2.RelatedOrganisations.AddNew();
			relationPart4.OU_OH = organisation1.PK;
			relationPart4.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.Save();
			}

			changer = new OrgSupplierBulkRelationshipChanger(Factory);
			changer.FromOrganisationPK = organisation1.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Both;
			changer.ChangeRelatedOrganisations(query);

			AssertRecordsAffected(changer, 0, 0, 1, 0, 1);

			part = new BusinessObjectFactory().Load<OrgSupplierPart>(part.PK);
			AssertEquals("OWN: Test1", FormatRelations(part));

			part2 = new BusinessObjectFactory().Load<OrgSupplierPart>(part2.PK);
			AssertEquals("BTH: Test1", FormatRelations(part2));

			changer = new OrgSupplierBulkRelationshipChanger(Factory);
			changer.FromOrganisationPK = organisation1.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			changer.ChangeRelatedOrganisations(query);

			part = new BusinessObjectFactory().Load<OrgSupplierPart>(part.PK);
			AssertEquals("WCN: Test1", FormatRelations(part));
		}

		#region TestChangeRelatedOrganisationsForDuplicatedRelationships_ExactMatch

		public void TestChangeRelatedOrganisationsForDuplicatedRelationships_ExactMatch_BothAndOwner()
		{
			TestChangeRelatedOrganisationsForDuplicatedRelationships_ExactMatchCore(hasOwner: true, hasBoth: true, (changer) => AssertRecordsAffected(changer, 0, 0, 0, 0, 1));
		}

		public void TestChangeRelatedOrganisationsForDuplicatedRelationships_ExactMatch_OwnerOnly()
		{
			TestChangeRelatedOrganisationsForDuplicatedRelationships_ExactMatchCore(hasOwner: true, hasBoth: false, (changer) => AssertRecordsAffected(changer, 0, 0, 0, 1, 0));
		}

		public void TestChangeRelatedOrganisationsForDuplicatedRelationships_ExactMatch_BothOnly()
		{
			TestChangeRelatedOrganisationsForDuplicatedRelationships_ExactMatchCore(hasOwner: false, hasBoth: true, (changer) => AssertRecordsAffected(changer, 0, 1, 0, 0, 0));
		}

		void TestChangeRelatedOrganisationsForDuplicatedRelationships_ExactMatchCore(bool hasOwner, bool hasBoth, Action<OrgSupplierBulkRelationshipChanger> expectedAssertion)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = helper.CreateClient("Org1");
			var org2PK = helper.CreateClient("Org2");
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var product = (OrgSupplierPart)helper.CreateProduct(orgPK, "Pro01");
			helper.CreateWhsReceiveWithInventory(orgPK, warehouse.PK, "R1", product.PK, 10m);

			if (hasOwner)
			{
				var relationPart1 = product.RelatedOrganisations.AddNew();
				relationPart1.OU_OH = org2PK;
				relationPart1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			}
			if (hasBoth)
			{
				var relationPart2 = product.RelatedOrganisations.AddNew();
				relationPart2.OU_OH = org2PK;
				relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			}

			helper.CreateWhsReceiveWithInventory(org2PK, warehouse.PK, "R2", product.PK, 10m);

			Factory.Save();

			var query = new ZQuery();
			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.ToOrganisationPK = org2PK;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer.ChangeRelatedOrganisations(query);

			expectedAssertion(changer);
		}

		#endregion

		#region TestChangeRelatedOrganisations_ChecksIfProductHasExistingStockWithCurrentOwner

		public void TestChangeRelatedOrganisations_ChecksIfProductHasExistingStockWithCurrentOwner()
		{
			// Organisations
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "Test1";
			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "Test2";

			// Products
			var part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "GUITARS";
			part1.RelatedOrganisations.AddOwner(organisation1);
			var part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "PIANOS";
			part2.RelatedOrganisations.AddOwner(organisation1);
			var part3 = OrgSupplierPart.New(Factory);
			part3.OP_PartNum = "FLUTES";
			part3.RelatedOrganisations.AddOwner(organisation1);
			part3.RelatedOrganisations.AddSupplier(organisation2);
			var part4 = OrgSupplierPart.New(Factory);
			part4.OP_PartNum = "VIOLINS";
			part4.RelatedOrganisations.AddOwner(organisation2);

			// Warehouse Transactions
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var receive = helper.CreateWhsReceive(organisation1.PK, warehouse.PK, "R1", new ZArchitecture.NotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive, part1.PK, 10m, ZGuid.Empty);
			helper.CreateWhsReceiveInventoryLine(receive, part3.PK, 10m, ZGuid.Empty);
			Factory.Save();

			// Part 1 should fail update, Part 2 should update, Part 3 should fail update, Part 4 should be skipped
			var query = new ZQuery();
			var changer1 = new OrgSupplierBulkRelationshipChanger(Factory);
			changer1.FromOrganisationPK = organisation1.PK;
			changer1.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer1.ToOrganisationPK = organisation2.PK;
			changer1.ToRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer1.ChangeRelatedOrganisations(query);
			Factory.Save();
			AssertRecordsAffected(changer1, addedCount: 0, updatedCount: 1, failedCount: 2, skippedCount: 0);
			AssertPartRelationsAreCorrect(part1, organisation1, OrgPartRelation.RelationshipTypes.Owner);
			AssertPartRelationsAreCorrect(part2, organisation2, OrgPartRelation.RelationshipTypes.Owner);
			AssertPartRelationsAreCorrect(part3, organisation1, OrgPartRelation.RelationshipTypes.Owner);
			AssertPartRelationsAreCorrect(part3, organisation2, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(part4, organisation2, OrgPartRelation.RelationshipTypes.Owner);

			// clean up
			ResetPart2Relations(organisation1, part2);

			// Part 1 should fail update, Part 2 should update, Part 3 should fail update, Part 4 should be skipped
			var changer2 = new OrgSupplierBulkRelationshipChanger(Factory);
			changer2.FromOrganisationPK = organisation1.PK;
			changer2.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer2.ToRelationship = OrgPartRelation.RelationshipTypes.Supplier;
			changer2.ChangeRelatedOrganisations(query);
			Factory.Save();
			AssertRecordsAffected(changer2, addedCount: 0, updatedCount: 1, failedCount: 2, skippedCount: 0);
			AssertPartRelationsAreCorrect(part1, organisation1, OrgPartRelation.RelationshipTypes.Owner);
			AssertPartRelationsAreCorrect(part2, organisation1, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(part3, organisation1, OrgPartRelation.RelationshipTypes.Owner);
			AssertPartRelationsAreCorrect(part3, organisation2, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(part4, organisation2, OrgPartRelation.RelationshipTypes.Owner);

			// clean up
			ResetPart2Relations(organisation1, part2);

			// Part 1 should fail update, Part 2 should update, Part 3 should fail update, Part 4 should have one added
			var changer3 = new OrgSupplierBulkRelationshipChanger(Factory);
			changer3.ToOrganisationPK = organisation1.PK;
			changer3.ToRelationship = OrgPartRelation.RelationshipTypes.Supplier;
			changer3.ChangeRelatedOrganisations(query);
			Factory.Save();
			AssertRecordsAffected(changer3, addedCount: 1, updatedCount: 1, failedCount: 2, skippedCount: 0);
			AssertPartRelationsAreCorrect(part1, organisation1, OrgPartRelation.RelationshipTypes.Owner);
			AssertPartRelationsAreCorrect(part2, organisation1, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(part3, organisation1, OrgPartRelation.RelationshipTypes.Owner);
			AssertPartRelationsAreCorrect(part3, organisation2, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(part4, organisation1, OrgPartRelation.RelationshipTypes.Supplier);
			AssertPartRelationsAreCorrect(part4, organisation2, OrgPartRelation.RelationshipTypes.Owner);
		}

		static void ResetPart2Relations(OrgHeader organisation1, OrgSupplierPart part2)
		{
			var otherFactory = new BusinessObjectFactory();
			var part2InOtherFactory = otherFactory.Load<OrgSupplierPart>(part2.PK);
			part2InOtherFactory.RelatedOrganisations.RemoveAndDeleteAll();
			part2InOtherFactory.RelatedOrganisations.AddOwner(organisation1);
			otherFactory.Save();
		}

		void AssertPartRelationsAreCorrect(OrgSupplierPart part, OrgHeader organisation, ZString relationship)
		{
			AssertNotNull(string.Format("Could not find Relation with Organisation '{0}' and Relationship '{1}' on Part '{2}'", part.OP_PartNum, organisation.OH_Code, relationship),
				new BusinessObjectFactory().Load<OrgSupplierPart>(part.PK).RelatedOrganisations.FindByOrganisationAndRelationship(organisation, relationship));
		}

		#endregion

		#region TestChangeRelatedOrganisations_ChecksIfProductHasExistingStockWithCurrentOwner_WithInTransitQty_FastQuery

		public void TestChangeRelatedOrganisations_ChecksIfProductHasExistingStockWithCurrentOwner_WithInTransitQty_FastQuery()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("ORG1"));
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P2");
			AssertEquals("Precondition: Should *not* have Stock On Hand.", false, part1.HasStockOnHandOrInTransit);
			AssertEquals("Precondition: Should *not* have Stock On Hand.", false, part2.HasStockOnHandOrInTransit);

			// Add some stock on hand
			var whsPK = helper.CreateWarehouse("1", "A").PK;
			Factory.Save();

			var receiveLine = (IWhsDocketLine)helper.CreateStock(whsPK, client1.PK, part1.PK, 10m);
			Factory.Save();
			AssertEquals("Precondition: Should have Stock On Hand.", true, part1.HasStockOnHandOrInTransit);
			AssertEquals("Precondition: Should *not* have Stock On Hand.", false, part2.HasStockOnHandOrInTransit);

			var orderPK = helper.CreateWhsOrder(client1.PK, whsPK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, part1.PK, 10);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });

			AssertEquals("Precondition: Stock On Hand exists.", 10m, receiveLine.WE_StockOnHand);
			var pickLine = helper.GetPickLines(pickPK).Single();
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
			AssertEquals("Precondition: Stock On Hand reduced.", 0m, receiveLine.WE_StockOnHand);
			Factory.Save();

			AssertEquals("Precondition: Should have Stock In-Transit.", true, part1.HasStockOnHandOrInTransit);
			AssertEquals("Precondition: Should *not* have Stock In-Transit.", false, part2.HasStockOnHandOrInTransit);

			var changer1 = new OrgSupplierBulkRelationshipChanger(Factory);
			changer1.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer1.ToRelationship = OrgPartRelation.RelationshipTypes.Supplier;
			changer1.FromOrganisationPK = client1.PK;
			changer1.ChangeRelatedOrganisations(new ZQuery());
			Factory.Save();

			// Part 1 should not be updated, Part 2 should be
			AssertRecordsAffected(changer1, addedCount: 0, updatedCount: 1, failedCount: 1, skippedCount: 0);
			AssertPartRelationsAreCorrect(part1, client1, OrgPartRelation.RelationshipTypes.Owner);
			AssertPartRelationsAreCorrect(part2, client1, OrgPartRelation.RelationshipTypes.Supplier);

			helper.FinaliseDocketWithoutUserConfirmation(orderPK);
			helper.FinalisePick(pickPK);
			Factory.Save();
			AssertEquals("Precondition: Should *not* have Stock On Hand or In-Transit.", false, part1.HasStockOnHandOrInTransit);
			AssertEquals("Precondition: Should *not* have Stock On Hand or In-Transit.", false, part2.HasStockOnHandOrInTransit);

			var changer2 = new OrgSupplierBulkRelationshipChanger(Factory);
			changer2.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer2.ToRelationship = OrgPartRelation.RelationshipTypes.Supplier;
			changer2.FromOrganisationPK = client1.PK;
			changer2.ChangeRelatedOrganisations(new ZQuery());
			Factory.Save();

			// Part 1 should *not* be updated as there are transactions
			AssertRecordsAffected(changer2, addedCount: 0, updatedCount: 0, failedCount: 1, skippedCount: 0);
			AssertPartRelationsAreCorrect(part1, client1, OrgPartRelation.RelationshipTypes.Owner);
			AssertPartRelationsAreCorrect(part2, client1, OrgPartRelation.RelationshipTypes.Supplier);
		}

		public void TestChangeRelatedOrganisations_ChecksIfProductHasExistingStockWithCurrentOwner_FastQuery_Supplier()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("ORG1"));
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");
			var part1Relation = (OrgPartRelation)part1.RelatedOrganisations.Single();
			AssertEquals("Precondition: Should *not* have Stock On Hand.", false, part1.HasStockOnHandOrInTransit);

			// Add some stock on hand
			var whsPK = helper.CreateWarehouse("1", "A").PK;
			Factory.Save();

			var receiveLine = (IWhsDocketLine)helper.CreateStock(whsPK, client1.PK, part1.PK, 10m);
			part1Relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier; // Invalid
			Factory.Save();
			AssertEquals("Precondition: Should have Stock On Hand.", true, part1.HasStockOnHandOrInTransit);

			var changer1 = new OrgSupplierBulkRelationshipChanger(Factory);
			changer1.FromOrganisationPK = client1.PK;
			changer1.FromRelationship = OrgPartRelation.RelationshipTypes.Supplier;
			changer1.ToRelationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
			changer1.ChangeRelatedOrganisations(new ZQuery());
			Factory.Save();

			// Part 1 should be updated as StockOnHand for Supplier is irrelevant (WD_OH_Client is relevant for Owner/Both relationship)
			AssertRecordsAffected(changer1, addedCount: 0, updatedCount: 1, failedCount: 0, skippedCount: 0);
			AssertPartRelationsAreCorrect(part1, client1, OrgPartRelation.RelationshipTypes.WarehouseConsignee);
		}

		public void TestChangeRelatedOrganisations_ChecksIfProductHasExistingStockWithCurrentOwner_FastQuery_OwnerToBoth()
		{
			TestChangeRelatedOrganisations_ChecksIfProductHasExistingStockWithCurrentOwner_FastQuery_OwnerAndBothCore(OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both);
		}

		public void TestChangeRelatedOrganisations_ChecksIfProductHasExistingStockWithCurrentOwner_FastQuery_BothToOwner()
		{
			TestChangeRelatedOrganisations_ChecksIfProductHasExistingStockWithCurrentOwner_FastQuery_OwnerAndBothCore(OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.Owner);
		}

		public void TestChangeRelatedOrganisations_ChecksIfProductHasExistingStockWithCurrentOwner_FastQuery_OwnerAndBothCore(string fromType, string toType)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = Factory.Load<OrgHeader>(helper.CreateClient("ORG1"));
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1.PK, "P1");
			var part1Relation = (OrgPartRelation)part1.RelatedOrganisations.Single();
			part1Relation.OU_Relationship = fromType;
			AssertEquals("Precondition: Should *not* have Stock On Hand.", false, part1.HasStockOnHandOrInTransit);

			// Add some stock on hand
			var whsPK = helper.CreateWarehouse("1", "A").PK;
			Factory.Save();

			var receiveLine = (IWhsDocketLine)helper.CreateStock(whsPK, client1.PK, part1.PK, 10m);
			Factory.Save();
			AssertEquals("Precondition: Should have Stock On Hand.", true, part1.HasStockOnHandOrInTransit);

			var changer1 = new OrgSupplierBulkRelationshipChanger(Factory);
			changer1.FromOrganisationPK = client1.PK;
			changer1.FromRelationship = fromType;
			changer1.ToRelationship = toType;
			changer1.ChangeRelatedOrganisations(new ZQuery());
			Factory.Save();

			// Part 1 should be updated
			AssertRecordsAffected(changer1, addedCount: 0, updatedCount: 1, failedCount: 0, skippedCount: 0);
			AssertPartRelationsAreCorrect(part1, client1, toType);
		}

		#endregion
	}
}

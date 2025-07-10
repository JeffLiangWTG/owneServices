using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	sealed class OrgSupplierBulkRelationshipChangerNonTransactionalTest : TestCase
	{
		public void TestCanRunOutsideOfTransaction()
		{
			var factory = new BusinessObjectFactory();

			var org = factory.New<OrgHeader>();
			org.OH_Code = "Test1";

			var part = OrgSupplierPart.New(factory);
			part.OP_PartNum = "TEST-PRODUCT";
			part.RelatedOrganisations.AddSupplier(org);

			AssertEquals("pre-condition", 1, part.RelatedOrganisations.Count);

			factory.Save();

			OrgSupplierBulkRelationshipChanger changer = new OrgSupplierBulkRelationshipChanger(new BusinessObjectFactory());

			changer.FromOrganisationPK = org.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Supplier;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Owner;

			changer.ChangeRelatedOrganisations(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "TEST-PRODUCT"));

			var factory2 = new BusinessObjectFactory();
			var part2 = factory2.Load<OrgSupplierPart>(part.PK);

			AssertEquals(1, part2.RelatedOrganisations.Count);
			AssertEquals(org.PK, part2.RelatedOrganisations[0].OU_OH);
			AssertEquals(OrgPartRelation.RelationshipTypes.Owner, part2.RelatedOrganisations[0].OU_Relationship);
		}
	}
}

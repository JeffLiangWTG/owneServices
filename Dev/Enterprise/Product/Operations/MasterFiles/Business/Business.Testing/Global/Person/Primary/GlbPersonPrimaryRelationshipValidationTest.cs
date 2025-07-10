using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbPersonPrimaryRelationshipValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidatePPR_PER()
		{
			var person = Factory.New<GlbPerson>();
			var contact = Factory.New<OrgContact>();

			person.SetPrimaryRelationship(contact);

			person.PrimaryRelationship.Validation.ValidatePPR_PER();
			AssertHasError(person.PrimaryRelationship.PPR_PERInfo, "The primary's person does not match the primary relationship's person. This must be fixed before saving.");
		}

		public void TestValidatePPR_PrimaryId()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_PER = person.PK;

			person.SetPrimaryRelationship(contact);
			person.PrimaryRelationship.PPR_PrimaryTableCode = "GS";

			person.PrimaryRelationship.Validation.ValidatePPR_PrimaryId();
			AssertHasError(person.PrimaryRelationship.PPR_PrimaryIdInfo, FormattableString.Invariant($"The primary Id ({contact.PK}) does not match the primary table code (GS)."));
		}

		public void TestValidatePPR_PrimaryTableCode()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_PER = person.PK;

			person.SetPrimaryRelationship(contact);
			person.PrimaryRelationship.PPR_PrimaryTableCode = "GO";

			person.PrimaryRelationship.Validation.ValidatePPR_PrimaryTableCode();
			AssertHasError(person.PrimaryRelationship.PPR_PrimaryTableCodeInfo, FormattableString.Invariant($"The primary table code must be either {OrgContactSchema.Constants.Prefix} or {GlbStaffSchema.Constants.Prefix}."));
		}
	}
}

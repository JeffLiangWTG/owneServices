using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;
using Loader = Enterprise.MasterFiles.Business.OrgRelatedParty.Loader;

namespace Enterprise.MasterFiles.Business.Testing;

[TestedType(typeof(Loader))]
sealed class OrgRelatedPartLoaderTest : LoaderTestCase
{
	public void TestLoadPartiesWithRelatedOrgAndType() => CombineAssertions(() =>
	{
		var org1 = Factory.New<OrgHeader>();
		org1.OH_Code = "ORG1";

		var org2 = Factory.New<OrgHeader>();
		org2.OH_Code = "ORG2";

		var org3 = Factory.New<OrgHeader>();
		org3.OH_Code = "ORG3";

		var relatedParty1 = CreateRelation(org1, org3, "ABC");
		var relatedParty2 = CreateRelation(org2, org3, "ABC");
		var relatedParty3 = CreateRelation(org3, org2, "DEF");

		var loader = new Loader(Factory);
		var loadResult = loader.LoadPartiesWithRelatedOrgAndType(org3.PK, "ABC");
		AssertContainsExactElementsInAnyOrder("For ORG3, ABC", new OrgRelatedParty[] { relatedParty1, relatedParty2 }, loadResult);

		loadResult = loader.LoadPartiesWithRelatedOrgAndType(org2.PK, "DEF");
		AssertEquals("For ORG2, ABC", relatedParty3, loadResult.Single());

		loadResult = loader.LoadPartiesWithRelatedOrgAndType(org1.PK, "ABC");
		AssertEquals("No match for org ORG1", 0, loadResult.Length);

		loadResult = loader.LoadPartiesWithRelatedOrgAndType(org3.PK, "DEF");
		AssertEquals("No match for ORG3, DEF", 0, loadResult.Length);
	});

	OrgRelatedParty CreateRelation(OrgHeader org, OrgHeader relatedOrg, string relationType)
	{
		var relatedParty = org.AllRelatedParties.AddNew();
		relatedParty.PR_OH_RelatedParty = relatedOrg.PK;
		relatedParty.PR_PartyType = relationType;
		return relatedParty;
	}

	protected override BusinessObject.Loader GetNewLoaderToTest() => new Loader(Factory);
}

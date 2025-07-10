using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrgHeaderDeduplicationProviderTest : TestCaseWithFactory
	{
		public void TestDeduplicationProvider()
		{
			var provider = new OrgHeaderDeduplicationProvider();

			AssertEquals(DeduplicationDisplayMode.Detailed, provider.DisplayModeForType);
			AssertEquals(DeduplicationProvider.Constants.Organisations, provider.GroupNameForType);
			AssertEquals(typeof(IOrgHeader), provider.GlowType);
			AssertEquals(typeof(OrgHeader), provider.BusinessObjectType);
			AssertEquals(OrgHeaderSchema.Constants.Prefix, provider.TablePrefix);
		}

		public void TestGetComparisonSource()
		{
			var provider = new OrgHeaderDeduplicationProvider();
			var orgInDB = Factory.NewWithValidTestData<OrgHeader>();

			orgInDB.OH_Code = "ABZ";
			Factory.Save();
			var deduporgheader = new DeduplicationOrgHeader(orgInDB);
			var source = provider.GetComparisonSource(deduporgheader, orgInDB.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(orgInDB.PK, ((DeduplicationOrgHeader)source).OH_PK);

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_Code = "CCA";
			var deduporgheadernew = new DeduplicationOrgHeader(newOrg);
			source = provider.GetComparisonSource(deduporgheadernew, newOrg.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(newOrg.PK, ((DeduplicationOrgHeader)source).OH_PK);

			var orgContact = newOrg.Contacts.AddNew();
			var glbPerson = GlbPerson.CreateFromContact(Factory, orgContact);
			var dedupPerson = new MasterDataProvider().GetDeduplicationGlbPerson(glbPerson) as DeduplicationGlbPerson;
			source = provider.GetComparisonSource(new[] { dedupPerson }, newOrg.PK.ToGuid(), null);
			AssertNotNull("Source is not null", source);
			AssertEquals(newOrg.PK, ((DeduplicationOrgHeader)source).OH_PK);
		}

		public void TestGetHeading_WhenSourceIsKnown()
		{
			var provider = new OrgHeaderDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "CCE";
			var deduporgheader = new DeduplicationOrgHeader(org);
			var header = provider.GetHeading(deduporgheader);

			AssertEquals("CCE", header);
		}

		public void TestGetHeading_WhenSourceIsEitherNewOrExisting()
		{
			var provider = new OrgHeaderDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "CEA";
			org.OH_FullName = "Capitol Hill";
			Factory.Save();

			var deduporgheader = new List<IDeduplicationGlowObject>() { new DeduplicationOrgHeader(org) };
			var shortHeader = provider.GetHeading(deduporgheader, org.PK.ToGuid(), HeaderType.Short);
			var longHeader = provider.GetHeading(deduporgheader, org.PK.ToGuid(), HeaderType.Long);

			AssertEquals("CEA", shortHeader);
			AssertEquals("CEA: Capitol Hill", longHeader);
		}

		public void TestGetChildObject()
		{
			var provider = new OrgHeaderDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var childObj = provider.GetChildObject(new DeduplicationOrgHeader(org), org.PK.ToGuid());
			AssertNotNull(childObj);
			Assert(childObj is DeduplicationOrgHeader);
			AssertEquals(org.PK, ((DeduplicationOrgHeader)childObj).OH_PK);

			var orgContact = org.Contacts.AddNew();
			var glbPerson = GlbPerson.CreateFromContact(Factory, orgContact);
			childObj = provider.GetChildObject(glbPerson.CreateIGlbPerson() as IDeduplicationMaster, org.PK.ToGuid());
			AssertNotNull(childObj);
			Assert(childObj is DeduplicationOrgHeader);
			AssertEquals(org.PK, ((DeduplicationOrgHeader)childObj).OH_PK);

			glbPerson = Factory.New<GlbPerson>();
			childObj = provider.GetChildObject(glbPerson.CreateIGlbPerson() as IDeduplicationMaster, org.PK.ToGuid());
			AssertNull(childObj);
		}
	}
}

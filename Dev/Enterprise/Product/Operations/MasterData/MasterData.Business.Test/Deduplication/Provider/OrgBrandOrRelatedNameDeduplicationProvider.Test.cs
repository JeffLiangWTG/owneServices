using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrgBrandOrRelatedNameDeduplicationProviderTest : TestCaseWithFactory
	{
		public void TestDeduplicationProvider()
		{
			var provider = new OrgBrandOrRelatedNameDeduplicationProvider();

			AssertEquals(DeduplicationDisplayMode.Undefined, provider.DisplayModeForType);
			AssertEquals("", provider.GroupNameForType);
			AssertEquals(typeof(IOrgBrandOrRelatedName), provider.GlowType);
			AssertEquals(typeof(OrgBrandOrRelatedName), provider.BusinessObjectType);
			AssertEquals(OrgBrandOrRelatedNameSchema.Constants.Prefix, provider.TablePrefix);
		}

		public void TestGetComparisonSource()
		{
			var provider = new OrgBrandOrRelatedNameDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var brand = org.BrandsOrRelatedNames.AddNew();

			brand.P1_RelatedName = "Branded";
			Factory.Save();
			var deduporg = new DeduplicationOrgHeader(org);
			var source = provider.GetComparisonSource(deduporg, brand.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(brand.PK, ((DeduplicationOrgBrandOrRelatedName)source).P1_PK);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var brand2 = org2.BrandsOrRelatedNames.AddNew();

			brand2.P1_RelatedName = "Branded";
			var deduporg2 = new DeduplicationOrgHeader(org2);
			source = provider.GetComparisonSource(deduporg2, brand2.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(brand2.PK, ((DeduplicationOrgBrandOrRelatedName)source).P1_PK);

			var orgContact = org2.Contacts.AddNew();
			var glbPerson = GlbPerson.CreateFromContact(Factory, orgContact);
			var dedupePerson = new MasterDataProvider().GetDeduplicationGlbPerson(glbPerson) as DeduplicationGlbPerson;
			source = provider.GetComparisonSource(new[] { dedupePerson }, brand2.PK.ToGuid(), null);
			AssertNotNull("Source is not null", source);
			AssertEquals(brand2.PK, ((DeduplicationOrgBrandOrRelatedName)source).P1_PK);

			source = provider.GetComparisonSource(dedupePerson, brand2.PK.ToGuid(), null);
			AssertNotNull("Source is not null", source);
			AssertEquals(brand2.PK, ((DeduplicationOrgBrandOrRelatedName)source).P1_PK);
		}

		public void TestGetComparisonSource_ShouldNotThrowNullReferenceException()
		{
			var provider = new OrgBrandOrRelatedNameDeduplicationProvider();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var dedupePerson = new DeduplicationGlbPerson(person);
			AssertNoExceptionThrown(() => provider.GetComparisonSource(new List<IDeduplicationGlowObject>() { dedupePerson }, Guid.Empty, null));
		}

		public void TestGetHeading_WhenSourceIsKnown()
		{
			var provider = new OrgBrandOrRelatedNameDeduplicationProvider();
			var brand = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();

			brand.P1_RelatedName = "Branded";
			var dedupname = new DeduplicationOrgBrandOrRelatedName(brand, null);
			var header = provider.GetHeading(dedupname);

			AssertEquals("Branded", header);
		}

		public void TestGetHeading_WhenSourceIsEitherNewOrExisting()
		{
			var provider = new OrgBrandOrRelatedNameDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var brand = org.BrandsOrRelatedNames.AddNew();

			brand.P1_RelatedName = "Branded";
			Factory.Save();
			var deduporg = new List<IOrgHeader>() { new DeduplicationOrgHeader(org) };
			var header = provider.GetHeading(deduporg, brand.PK.ToGuid());

			AssertEquals("Branded", header);
		}

		[ExpectNoExceptions]
		public void TestGetComparisonSource_DoesNotThrowException_WhenTargetPKIsInSecondOrgHeader()
		{
			var provider = new OrgBrandOrRelatedNameDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "OAA";
			org.OH_FullName = "Toll Australia";
			org2.OH_Code = "OAB";
			org2.OH_FullName = "Toll New Zealand";
			var dedupeOrg = new MasterDataProvider().GetDeduplicationOrgHeader(org) as DeduplicationOrgHeader;
			var targets = new List<DeduplicationOrgHeader> { dedupeOrg, new DeduplicationOrgHeader(org2) };
			var source = provider.GetComparisonSource(targets, Guid.Empty, null);
		}

		[ExpectNoExceptions]
		public void TestGetHeading_DoesNotThrowException_WhenTargetPKIsInSecondOrgHeader()
		{
			var provider = new OrgBrandOrRelatedNameDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "OAA";
			org.OH_FullName = "Toll Australia";
			org2.OH_Code = "OAB";
			org2.OH_FullName = "Toll New Zealand";

			var targets = new List<IOrgHeader> { new DeduplicationOrgHeader(org), new DeduplicationOrgHeader(org2) };
			var source = provider.GetHeading(targets, Guid.Empty);
		}

		public void TestGetChildObject()
		{
			var provider = new OrgBrandOrRelatedNameDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var brand = org.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "Branded";

			var childObj = provider.GetChildObject(new DeduplicationOrgHeader(org), brand.PK.ToGuid());
			AssertNotNull(childObj);
			AssertEquals(brand.PK, ((DeduplicationOrgBrandOrRelatedName)childObj).P1_PK);

			var orgContact = org.Contacts.AddNew();
			var glbPerson = GlbPerson.CreateFromContact(Factory, orgContact);
			childObj = provider.GetChildObject(glbPerson.CreateIGlbPerson() as IDeduplicationMaster, brand.PK.ToGuid());
			AssertNotNull(childObj);
			AssertEquals(brand.PK, ((DeduplicationOrgBrandOrRelatedName)childObj).P1_PK);
		}
	}
}

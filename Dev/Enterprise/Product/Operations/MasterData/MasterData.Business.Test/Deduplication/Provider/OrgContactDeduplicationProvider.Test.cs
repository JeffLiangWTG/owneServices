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
	public class OrgContactDeduplicationProviderTest : TestCaseWithFactory
	{
		public void TestDeduplicationProvider()
		{
			var provider = new OrgContactDeduplicationProvider();

			AssertEquals(DeduplicationDisplayMode.Detailed, provider.DisplayModeForType);
			AssertEquals(DeduplicationProvider.Constants.Contacts, provider.GroupNameForType);
			AssertEquals(typeof(IOrgContact), provider.GlowType);
			AssertEquals(typeof(OrgContact), provider.BusinessObjectType);
			AssertEquals(OrgContactSchema.Constants.Prefix, provider.TablePrefix);
		}

		public void TestGetComparisonSource()
		{
			var provider = new OrgContactDeduplicationProvider();
			var orgInDB = Factory.NewWithValidTestData<OrgHeader>();
			var contact = orgInDB.Contacts.AddNew();

			orgInDB.OH_Code = "ABZ";
			contact.OC_ContactName = "Alex";
			Factory.Save();

			var deduporgheader = new DeduplicationOrgHeader(orgInDB);
			var source = provider.GetComparisonSource(deduporgheader, contact.PK.ToGuid(), null);
			AssertNotNull("Source is not null", source);
			AssertEquals(contact.PK, ((DeduplicationOrgContact)source).OC_PK);

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = newOrg.Contacts.AddNew();
			newOrg.OH_Code = "CCA";
			contact1.OC_ContactName = "Martin";
			Factory.Save();
			var deduporgheadernew = new DeduplicationOrgHeader(newOrg);
			source = provider.GetComparisonSource(deduporgheadernew, contact1.PK.ToGuid(), null);
			AssertNotNull("Source is not null", source);
			AssertEquals(contact1.PK, ((DeduplicationOrgContact)source).OC_PK);

			source = provider.GetComparisonSource(new[] { deduporgheader, deduporgheadernew }, contact.PK.ToGuid(), null);
			AssertNotNull("Source is not null", source);
			AssertEquals(contact.PK, ((DeduplicationOrgContact)source).OC_PK);

			var glbPerson = GlbPerson.CreateFromContact(Factory, contact);
			var dedupGlbPerson = new MasterDataProvider().GetDeduplicationGlbPerson(glbPerson) as DeduplicationGlbPerson;
			source = provider.GetComparisonSource(new[] { dedupGlbPerson }, contact.PK.ToGuid(), null);
			AssertNotNull("Source is not null", source);
			AssertEquals(contact.PK, ((DeduplicationOrgContact)source).OC_PK);

			source = provider.GetComparisonSource(dedupGlbPerson, contact.PK.ToGuid(), null);
			AssertNotNull("Source is not null", source);
			AssertEquals(contact.PK, ((DeduplicationOrgContact)source).OC_PK);
		}

		public void TestGetHeading_WhenSourceIsKnown()
		{
			var provider = new OrgContactDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			org.OH_Code = "CCE";
			contact.OC_ContactName = "Edward";
			var deduporgcontact = new DeduplicationOrgContact(contact, false);
			var header = provider.GetHeading(deduporgcontact);

			AssertEquals("Edward", header);
		}

		public void TestGetHeading_WhenSourceIsEitherNewOrExisting()
		{
			var provider = new OrgContactDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			org.OH_Code = "CEA";
			org.OH_FullName = "Capitol Hill";
			contact.OC_ContactName = "Edward";
			Factory.Save();

			var deduporgheader = new List<IOrgHeader>() { new DeduplicationOrgHeader(org) };
			var header = provider.GetHeading(deduporgheader, contact.PK.ToGuid(), HeaderType.Short);

			AssertEquals("Edward - Capitol Hill (CEA)", header);
		}

		[ExpectNoExceptions]
		public void TestGetComparisonSource_DoesNotThrowException_WhenTargetPKIsInSecondOrgHeader()
		{
			var provider = new OrgContactDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "OAA";
			org.OH_FullName = "Toll Australia";
			org2.OH_Code = "OAB";
			org2.OH_FullName = "Toll New Zealand";

			var targets = new List<IDeduplicationGlowObject> { new DeduplicationOrgHeader(org), new DeduplicationOrgHeader(org2) };
			var source = provider.GetComparisonSource(targets, Guid.Empty, null);
		}

		[ExpectNoExceptions]
		public void TestGetHeading_DoesNotThrowException_WhenTargetPKIsInSecondOrgHeader()
		{
			var provider = new OrgContactDeduplicationProvider();
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
			var provider = new OrgContactDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			Factory.Save();
			AssertNull(provider.GetChildObject(new DeduplicationOrgHeader(org), Guid.NewGuid()));
			AssertNotNull(provider.GetChildObject(new DeduplicationOrgHeader(org), contact.PK.ToGuid()));

			var glbPerson = GlbPerson.CreateFromContact(Factory, contact);
			AssertNotNull(provider.GetChildObject(glbPerson.CreateIGlbPerson() as IDeduplicationMaster, contact.PK.ToGuid()));

			AssertNull(provider.GetChildObject(new object() as IDeduplicationMaster, contact.PK.ToGuid()));
			AssertNull(provider.GetChildObject(null, contact.PK.ToGuid()));
		}
	}
}

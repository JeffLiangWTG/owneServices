using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrgWebURLDeduplicationProviderTest : TestCaseWithFactory
	{
		public void TestDeduplicationProvider()
		{
			var provider = new OrgWebUrlDeduplicationProvider();

			AssertEquals(DeduplicationDisplayMode.Detailed, provider.DisplayModeForType);
			AssertEquals(DeduplicationProvider.Constants.Websites, provider.GroupNameForType);
			AssertEquals(typeof(IOrgWebURL), provider.GlowType);
			AssertEquals(typeof(OrgWebURL), provider.BusinessObjectType);
			AssertEquals(OrgWebURLSchema.Constants.Prefix, provider.TablePrefix);
		}

		public void TestGetComparisonSource()
		{
			var provider = new OrgWebUrlDeduplicationProvider();
			var orgInDB = Factory.NewWithValidTestData<OrgHeader>();
			var webUrl = orgInDB.OrgWebURLs.AddNew();

			orgInDB.OH_Code = "ABZ";
			webUrl.PU_URL = "http://www.wisetechglobal.com";
			Factory.Save();
			var deduporgheader = new DeduplicationOrgHeader(orgInDB);
			var source = provider.GetComparisonSource(deduporgheader, webUrl.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(webUrl.PK, ((DeduplicationOrgWebURL)source).PU_PK);

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var webUrlForNewOrg = newOrg.OrgWebURLs.AddNew();
			newOrg.OH_Code = "CCA";
			webUrlForNewOrg.PU_URL = "http://www.wisecloud.net";
			var deduporgheadernew = new DeduplicationOrgHeader(newOrg);
			source = provider.GetComparisonSource(deduporgheadernew, webUrlForNewOrg.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(webUrlForNewOrg.PK, ((DeduplicationOrgWebURL)source).PU_PK);
		}

		public void TestGetHeading_WhenSourceIsKnown()
		{
			var provider = new OrgWebUrlDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var webUrl = org.OrgWebURLs.AddNew();

			org.OH_Code = "CCE";
			webUrl.PU_URL = "http://www.wisetechglobal.com";
			var deduporgheader = new DeduplicationOrgHeader(org);
			var header = provider.GetHeading(deduporgheader.OrgWebURLs.First(x => x.PU_PK == webUrl.PK) as IDeduplicationGlowObject);

			AssertEquals("http://www.wisetechglobal.com", header);
		}

		public void TestGetHeading_WhenSourceIsEitherNewOrExisting()
		{
			var provider = new OrgWebUrlDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var webUrl = org.OrgWebURLs.AddNew();

			org.OH_Code = "CEA";
			org.OH_FullName = "Capitol Hill";
			webUrl.PU_URL = "http://www.wisetechglobal.com";
			Factory.Save();

			var deduporgheader = new List<IOrgHeader>() { new DeduplicationOrgHeader(org) };
			var header = provider.GetHeading(deduporgheader, webUrl.PK.ToGuid(), HeaderType.Short);

			AssertEquals("http://www.wisetechglobal.com", header);
		}

		[ExpectNoExceptions]
		public void TestGetComparisonSource_DoesNotThrowException_WhenTargetPKIsInSecondOrgHeader()
		{
			var provider = new OrgWebUrlDeduplicationProvider();
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
			var provider = new OrgWebUrlDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "OAA";
			org.OH_FullName = "Toll Australia";
			org2.OH_Code = "OAB";
			org2.OH_FullName = "Toll New Zealand";

			var targets = new List<IOrgHeader> { new DeduplicationOrgHeader(org), new DeduplicationOrgHeader(org2) };
			var source = provider.GetHeading(targets, Guid.Empty);
		}
	}
}

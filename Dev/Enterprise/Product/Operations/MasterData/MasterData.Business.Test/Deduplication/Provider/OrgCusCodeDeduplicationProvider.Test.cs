using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrgCusCodeDeduplicationProviderTest : TestCaseWithFactory
	{
		public void TestDeduplicationProvider()
		{
			var provider = new OrgCusCodeDeduplicationProvider();

			AssertEquals(DeduplicationDisplayMode.List, provider.DisplayModeForType);
			AssertEquals(DeduplicationProvider.Constants.RegistrationCodes, provider.GroupNameForType);
			AssertEquals(typeof(IOrgCusCode), provider.GlowType);
			AssertEquals(typeof(OrgCusCode), provider.BusinessObjectType);
			AssertEquals(OrgCusCodeSchema.Constants.Prefix, provider.TablePrefix);
		}

		public void TestGetComparisonSource()
		{
			ZString refCountryCode = "ZA";
			var provider = new OrgCusCodeDeduplicationProvider();
			var orgInDB = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode cusCode1 = orgInDB.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "XZX";
			cusCode1.OK_RN_NKCodeCountry = refCountryCode;
			cusCode1.OK_CustomsRegNo = "88754";
			cusCode1.OK_OA_PremisesAddress = ZGuid.Empty;

			orgInDB.OH_Code = "ABZ";
			Factory.Save();
			var deduporgheader = new DeduplicationOrgHeader(orgInDB);
			var source = provider.GetComparisonSource(deduporgheader, cusCode1.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(cusCode1.PK, ((DeduplicationOrgCusCode)source).OK_PK);

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode cusCode2 = newOrg.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "XZX";
			cusCode2.OK_RN_NKCodeCountry = refCountryCode;
			cusCode2.OK_CustomsRegNo = "88744";
			cusCode2.OK_OA_PremisesAddress = ZGuid.Empty;
			newOrg.OH_Code = "CCA";
			var deduporgheadernew = new DeduplicationOrgHeader(newOrg);
			source = provider.GetComparisonSource(deduporgheadernew, cusCode2.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(cusCode2.PK, ((DeduplicationOrgCusCode)source).OK_PK);
		}

		public void TestGetHeading_WhenSourceIsKnown()
		{
			ZString refCountryCode = "ZA";
			var provider = new OrgCusCodeDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode cusCode1 = org.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "XZX";
			cusCode1.OK_RN_NKCodeCountry = refCountryCode;
			cusCode1.OK_CustomsRegNo = "88754";
			cusCode1.OK_OA_PremisesAddress = ZGuid.Empty;

			org.OH_Code = "CCE";
			var deduporgheader = new DeduplicationOrgHeader(org);
			var header = provider.GetHeading(deduporgheader.CusCodes.First(x => x.OK_PK == cusCode1.PK) as IDeduplicationGlowObject);

			AssertEquals("ZA XZX 88754", header);
		}

		public void TestGetHeading_WhenSourceIsEitherNewOrExisting()
		{
			ZString refCountryCode = "ZA";
			var provider = new OrgCusCodeDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode cusCode1 = org.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "XZX";
			cusCode1.OK_RN_NKCodeCountry = refCountryCode;
			cusCode1.OK_CustomsRegNo = "88754";
			cusCode1.OK_OA_PremisesAddress = ZGuid.Empty;

			org.OH_Code = "CEA";
			org.OH_FullName = "Capitol Hill";
			Factory.Save();

			var deduporgheader = new List<IOrgHeader>() { new DeduplicationOrgHeader(org) };
			var header = provider.GetHeading(deduporgheader, cusCode1.PK.ToGuid(), HeaderType.Short);

			AssertEquals("ZA XZX 88754", header);
		}

		[ExpectNoExceptions]
		public void TestGetComparisonSource_DoesNotThrowException_WhenTargetPKIsInSecondOrgHeader()
		{
			var provider = new OrgCusCodeDeduplicationProvider();
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
			var provider = new OrgCusCodeDeduplicationProvider();
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

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	public class OrgAddressDeduplicationProviderTest : TestCaseWithFactory
	{
		public void TestDeduplicationProvider()
		{
			var provider = new OrgAddressDeduplicationProvider();

			AssertEquals(DeduplicationDisplayMode.Detailed, provider.DisplayModeForType);
			AssertEquals(DeduplicationProvider.Constants.Addresses, provider.GroupNameForType);
			AssertEquals(typeof(IOrgAddress), provider.GlowType);
			AssertEquals(typeof(OrgAddress), provider.BusinessObjectType);
			AssertEquals(OrgAddressSchema.Constants.Prefix, provider.TablePrefix);
		}

		public void TestGetComparisonSource()
		{
			var provider = new OrgAddressDeduplicationProvider();
			var orgInDB = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgInDB.Addresses.AddNew();

			orgInDB.OH_Code = "ABZ";
			address.OA_Address1 = "Alexandria";
			Factory.Save();

			var deduporg = new DeduplicationOrgHeader(orgInDB);
			var source = provider.GetComparisonSource(deduporg, address.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(address.PK, ((DeduplicationOrgAddress)source).OA_PK);

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = newOrg.Addresses.AddNew();
			newOrg.OH_Code = "CCA";
			address1.OA_Address1 = "Botany";
			Factory.Save();
			var deduporgnew = new DeduplicationOrgHeader(newOrg);
			source = provider.GetComparisonSource(deduporgnew, address1.PK.ToGuid(), null);

			AssertNotNull("Source is not null", source);
			AssertEquals(address1.PK, ((DeduplicationOrgAddress)source).OA_PK);
		}

		public void TestGetHeading_WhenSourceIsKnown()
		{
			var provider = new OrgAddressDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();

			org.OH_Code = "CCE";
			address.OA_Address1 = "Address 1";
			address.OA_Code = "CODEA";
			Factory.Save();
			var deduporgheader = new DeduplicationOrgHeader(org);

			var header = provider.GetHeading(deduporgheader.OrgAddresses.First(x => x.OA_PK == address.PK) as IDeduplicationGlowObject);

			AssertEquals("CODEA", header);
		}

		public void TestGetHeading_WhenSourceIsEitherNewOrExisting()
		{
			var provider = new OrgAddressDeduplicationProvider();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();

			org.OH_Code = "CEA";
			org.OH_FullName = "Capitol Hill";
			address.OA_Address1 = "Address 1";
			address.OA_Code = "CODEA";
			Factory.Save();
			var deduporgheader = new List<IOrgHeader>() { new DeduplicationOrgHeader(org) };
			var header = provider.GetHeading(deduporgheader, address.PK.ToGuid(), HeaderType.Short);

			AssertEquals("CODEA", header);
		}

		[ExpectNoExceptions]
		public void TestGetComparisonSource_DoesNotThrowException_WhenTargetPKIsInSecondOrgHeader()
		{
			var provider = new OrgAddressDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "OAA";
			org.OH_FullName = "Toll Australia";
			org2.OH_Code = "OAB";
			org2.OH_FullName = "Toll New Zealand";
			Factory.Save();

			var targets = new List<IDeduplicationGlowObject> { new DeduplicationOrgHeader(org), new DeduplicationOrgHeader(org2) };
			var source = provider.GetComparisonSource(targets, Guid.Empty, null);
		}

		[ExpectNoExceptions]
		public void TestGetHeading_DoesNotThrowException_WhenTargetPKIsInSecondOrgHeader()
		{
			var provider = new OrgAddressDeduplicationProvider();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "OAA";
			org.OH_FullName = "Toll Australia";
			org2.OH_Code = "OAB";
			org2.OH_FullName = "Toll New Zealand";

			var targets = new List<IOrgHeader> { new DeduplicationOrgHeader(org), new DeduplicationOrgHeader(org2) };
			var source = provider.GetHeading(targets, Guid.Empty);
		}

		public void TestValidationStatusIsNTCWhenAddressValidationServiceIsDisabled()
		{
			var rawEnableAddressValidationWebServiceValue = Env.Instance.Registry.EnableAddressValidationWebService;
			try
			{
				var address = Factory.New<OrgAddress>();

				Env.Instance.Registry.EnableAddressValidationWebService = true;
				address.OA_RN_NKCountryCode = "";
				address.ValidationStatus = "VAD";
				var deduplicationOrgAddress = new DeduplicationOrgAddress(address, null);
				AssertEquals("NTC", deduplicationOrgAddress.OA_ValidationStatus);

				var countryCode = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
				AssertNotNull("Precondition", countryCode);

				using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(countryCode.PK, disabledForOrgAddress: true)))
				{
					address.OA_RN_NKCountryCode = "AU";
					address.ValidationStatus = "VAD";
					deduplicationOrgAddress = new DeduplicationOrgAddress(address, null);
					AssertEquals("NTC", deduplicationOrgAddress.OA_ValidationStatus);
				}

				deduplicationOrgAddress = new DeduplicationOrgAddress(address, null);
				AssertEquals("VAD", deduplicationOrgAddress.OA_ValidationStatus);

				Env.Instance.Registry.EnableAddressValidationWebService = false;
				deduplicationOrgAddress = new DeduplicationOrgAddress(address, null);
				AssertEquals("NTC", deduplicationOrgAddress.OA_ValidationStatus);
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawEnableAddressValidationWebServiceValue;
			}
		}
	}
}

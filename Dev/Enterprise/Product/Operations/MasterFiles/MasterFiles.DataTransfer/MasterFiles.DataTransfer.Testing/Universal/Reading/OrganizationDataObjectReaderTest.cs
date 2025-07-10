using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class OrganizationDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestOrganizationMatchesEvenIfOwningCompanyIsADifferentLanguage()
		{
			var org = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			org.OH_Language = "GRM";
			org.OH_FullName = "HANS VON MANGOLDT GMBH & CO. KG";
			var addr1 = org.Addresses.AddNew();
			var addr2 = org.Addresses.AddNew();

			addr1.FillWithValidTestData();
			addr2.FillWithValidTestData();
			addr1.OA_CompanyNameOverride = "DHL GLOBAL FORWARDING GMBH";
			addr2.OA_CompanyNameOverride = "HANS VON MANGOLDT GMBH & CO. KG";
			addr1.OA_Language = "GRM";
			addr2.OA_Language = "ENG";
			addr1.OA_Address1 = "FF SCHILDCHEN 2";
			addr2.OA_Address1 = "AM SCHILDCHEN 1";

			Factory.SaveForTesting();
			var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Address1 = "AM SCHILDCHEN 1",
				CompanyName = "HANS VON MANGOLDT GMBH & CO. KG",
				AddressType = "Supplier",
			};

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressData, logger, Factory);
			CombineAssertions(() =>
			{
				var updatedAddress = reader.GetMatched();
				AssertEquals(addr2.PK, updatedAddress.PK);
			});
		}

		public void TestGetMatchedWithLongCompanyName()
		{
			var logger = new TestErrorLogger();
			var addressData = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<Address1>Some Address Name</Address1>
	<CompanyName>SHENZHEN JOY LUCK INTERNATIONAL FREIGHT FORWARDING LIMITEDROOM 903 SECTION B TAIPINGYANG COMMERCIAL TRADING BUILDING NO 4028 JIABIN ROAD LUOHU DISTRICT SHENZHEN CHINA</CompanyName>
</OrganizationAddress>", logger);

			var reader = new OrganisationDataObjectReader(addressData, logger, Factory);
			AssertNoExceptionThrown(() => reader.GetMatched(true));
		}

		public void TestPhoneNumberInOrganizationAddressShouldBeStandardizedAfterImport()
		{
			var phoneNumberFormatter = new PhoneNumberFormatter();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);

			var invalidPhoneFormatAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			invalidPhoneFormatAddress.Phone = "1234";
			var reader = new OrganisationDataObjectReader(invalidPhoneFormatAddress, logger, Factory);
			CombineAssertions(() =>
			{
				var updatedAddress = reader.GetMatchedOrNew(parentBO);
				AssertEquals("1234", updatedAddress.E2_Phone);
			});

			var validRawPhoneNumber = "2 8545 5624";
			var expectedFormattedPhoneNumber = phoneNumberFormatter.FormatE164(validRawPhoneNumber, "AU");
			invalidPhoneFormatAddress.Phone = validRawPhoneNumber;
			CombineAssertions(() =>
			{
				var updatedAddress = reader.GetMatchedOrNew(parentBO);
				AssertEquals(expectedFormattedPhoneNumber, updatedAddress.E2_Phone);
			});

			var validPhoneFormatAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			validPhoneFormatAddress.AddressShortCode = "TST";
			validPhoneFormatAddress.Phone = validRawPhoneNumber;
			reader = new OrganisationDataObjectReader(validPhoneFormatAddress, logger, Factory);
			CombineAssertions(() =>
			{
				var updatedAddress = reader.GetMatchedOrNew(parentBO);
				AssertEquals(expectedFormattedPhoneNumber, updatedAddress.E2_Phone);
			});

			var formattedPhoneFormatAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			formattedPhoneFormatAddress.Phone = expectedFormattedPhoneNumber;
			reader = new OrganisationDataObjectReader(formattedPhoneFormatAddress, logger, Factory);
			CombineAssertions(() =>
			{
				var updatedAddress = reader.GetMatchedOrNew(parentBO);
				AssertEquals(expectedFormattedPhoneNumber, updatedAddress.E2_Phone);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_AddressTypeOnly_NotCreateAndDelete()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			Factory.SaveForTesting();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should not create empty docaddress", 0, parentBO.DocAddresses.Count);
			});

			var consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_State = "NSW";
			consignor.E2_Address1 = "Old Test Address 1";
			consignor.E2_Address2 = "Old Test Address 2";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertEquals("PRE: Should create docaddress of with address overriden", true, consignor.E2_AddressOverride);
			AssertEquals("PRE: Should create docaddress of with test address 1", "Ye Olde Test Company", consignor.E2_CompanyName);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should delete to empty docaddress", 0, parentBO.DocAddresses.Count);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByWrongCodeOnly_NotCreateAndDelete()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
	<OrganizationCode>WRONGCODE</OrganizationCode>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should not create empty docaddress", 0, parentBO.DocAddresses.Count);
			});

			var consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "Address 1";
			consignor.E2_Address2 = "Address 2";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertEquals("PRE: Should create docaddress of with address overriden", true, consignor.E2_AddressOverride);
			AssertEquals("PRE: Should create docaddress of with test address 1", "Ye Olde Test Company", consignor.E2_CompanyName);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should delete empty docaddress", 0, parentBO.DocAddresses.Count);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_SecurityCheckpoint()
		{
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);

			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>CustomsDepotAddress</AddressType>
	<OrganizationCode>DUMMYCODE</OrganizationCode>
	<CompanyName>TROJAN BOND</CompanyName>
	<Address1>1 BUMBORAH POINT ROAD</Address1>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNull("Does not populate CustomsDepotAddress due to DeniedSecurityCheckpoint", reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should not create a docaddress", 0, parentBO.DocAddresses.Count);
			});

			var addressDataObject2 = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
	<OrganizationCode>DUMMYCODE</OrganizationCode>
	<CompanyName>TROJAN BOND</CompanyName>
	<Address1>1 BUMBORAH POINT ROAD</Address1>
</OrganizationAddress>", logger);
			var reader2 = new OrganisationDataObjectReader(addressDataObject2, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNotNull("Populates ConsignorDocumentaryAddress due to NoneSecurityCheckpoint", reader2.GetMatchedOrNew(parentBO));
				AssertEquals("Should create a docaddress", 1, parentBO.DocAddresses.Count);

				var consignorDocumentaryAddress = parentBO.DocAddresses.FindDocAddressesByType(DocAddressType.ConsignorDocumentaryAddress).First();
				Assert("Address is marked overrridden", consignorDocumentaryAddress.E2_AddressOverride);
				AssertEquals("Populated doc address from XML", "TROJAN BOND", consignorDocumentaryAddress.E2_CompanyName);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByWrongCodeWithOverridenFlag_NotCreateAndDelete()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			Factory.SaveForTesting();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
	<OrganizationCode>WRONGCODE</OrganizationCode>
	<AddressOverride>true</AddressOverride>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should not create empty docaddress", 0, parentBO.DocAddresses.Count);
			});

			var consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "Address 1";
			consignor.E2_Address2 = "Address 2";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertEquals("PRE: Should create docaddress of with address overriden", true, consignor.E2_AddressOverride);
			AssertEquals("PRE: Should create docaddress of with test address 1", "Ye Olde Test Company", consignor.E2_CompanyName);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should not create empty docaddress", 0, parentBO.DocAddresses.Count);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByWrongCodeWithoutOverridenFlagWithEmptyField_NotCreateAndDelete()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
	<OrganizationCode>WRONGCODE</OrganizationCode>
	<Address1></Address1>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should not create empty docaddress", 0, parentBO.DocAddresses.Count);
			});

			var consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "Old Test Address 1";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertEquals("PRE: Should create docaddress of with address overriden", true, consignor.E2_AddressOverride);
			AssertEquals("PRE: Should create docaddress of with test address 1", "Ye Olde Test Company", consignor.E2_CompanyName);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should delete empty docaddress", 0, parentBO.DocAddresses.Count);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByWrongCodeWithOverridenFlagWithEmptyField_NotCreateAndDelete()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
	<OrganizationCode>WRONGCODE</OrganizationCode>
	<AddressOverride>true</AddressOverride>
	<Address1></Address1>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should not create empty docaddress", 0, parentBO.DocAddresses.Count);
			});

			var consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "Old Test Address 1";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, parentBO.DocAddresses[0].DocAddressType);
			AssertEquals("PRE: Should create docaddress of with address overriden", true, consignor.E2_AddressOverride);
			AssertEquals("PRE: Should create docaddress of with test address 1", "Ye Olde Test Company", consignor.E2_CompanyName);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should delete empty docaddress", 0, parentBO.DocAddresses.Count);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByWrongCodeWithoutOverridenFlagWithSomeField_CreateAndUpdate()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			Factory.SaveForTesting();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
	<OrganizationCode>WRONGCODE</OrganizationCode>
	<Address1>New Test Address 1</Address1>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			AssertNotNull(reader.GetMatchedOrNew(parentBO));
			AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
			var consignor = parentBO.DocAddresses[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, parentBO.DocAddresses[0].DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", true, consignor.E2_AddressOverride);
				AssertEquals("Should create docaddress of with test address 1", "New Test Address 1", consignor.E2_Address1);
			});

			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "Old Test Address 1";
			consignor.E2_Address2 = "Old Test Address 2";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertEquals("Pre: Should not have docaddress with test address 1", "Old Test Address 1", consignor.E2_Address1);
			AssertEquals("Pre: Should not have docaddress with test address 2", "Old Test Address 2", consignor.E2_Address2);

			CombineAssertions(() =>
			{
				AssertNotNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
				consignor = parentBO.DocAddresses[0];
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", "New Test Address 1", consignor.E2_Address1);
				AssertEquals("Should create docaddress of with test address 2", "Old Test Address 2", consignor.E2_Address2);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByWrongCodeWithOverridenFlagWithSomeField_CreateAndUpdate()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			Factory.SaveForTesting();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
	<OrganizationCode>WRONGCODE</OrganizationCode>
	<AddressOverride>true</AddressOverride>
	<Address1>New Test Address 1</Address1>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			AssertNotNull(reader.GetMatchedOrNew(parentBO));
			AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
			var consignor = parentBO.DocAddresses[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, parentBO.DocAddresses[0].DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", true, consignor.E2_AddressOverride);
				AssertEquals("Should create docaddress of with test address 1", "New Test Address 1", consignor.E2_Address1);
			});

			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "Old Test Address 1";
			consignor.E2_Address2 = "Old Test Address 2";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertEquals("Pre: Should not have docaddress with test address 1", "Old Test Address 1", consignor.E2_Address1);
			AssertEquals("Pre: Should not have docaddress with test address 2", "Old Test Address 2", consignor.E2_Address2);

			CombineAssertions(() =>
			{
				AssertNotNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
				consignor = parentBO.DocAddresses[0];
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", "New Test Address 1", consignor.E2_Address1);
				AssertEquals("Should create docaddress of with test address 2", "Old Test Address 2", consignor.E2_Address2);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByRightCodeWithoutOverridenFlag_CreateAndUpdate()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
	<OrganizationCode>RIGHTCODE</OrganizationCode>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			parentBO.DocAddresses.ToArray().DeleteAll();
			Factory.SaveForTesting();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			AssertNotNull("reader.GetMatchedOrNew(parentBO)", reader.GetMatchedOrNew(parentBO));
			AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
			var consignor = parentBO.DocAddresses[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", "Test Address 1", consignor.E2_Address1);
			});

			parentBO.DocAddresses.ToArray().DeleteAll();
			consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "NOT Test Address 1";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertNotEquals("PRE: Parent should have docaddress without test address 1", "Test Address 1", consignor.E2_Address1);

			CombineAssertions(() =>
			{
				AssertNotNull("reader.GetMatchedOrNew(parentBO)", reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should have 1 docaddress", 1, parentBO.DocAddresses.Count);
				consignor = parentBO.DocAddresses[0];
				AssertEquals("Should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
				AssertEquals("Should have docaddress of with test address 1", "Test Address 1", consignor.E2_Address1);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByRightCodeWithOverridenFlag_NotCreateAndDelete()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			Factory.SaveForTesting();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<OrganizationCode>RIGHTCODE</OrganizationCode>
				<AddressOverride>true</AddressOverride>
			</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should delete empty docaddress", 0, parentBO.DocAddresses.Count);
			});

			parentBO.DocAddresses.ToArray().DeleteAll();
			var consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "Old Test Address 1";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertNotEquals("PRE: Parent should have docaddress without test address 1", "Test Address 1", consignor.E2_Address1);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should have no docaddress", 0, parentBO.DocAddresses.Count);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByRightCodeWithoutOverridenFlagWithEmptyField_CreateAndUpdateWithOriginalAddress()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<OrganizationCode>RIGHTCODE</OrganizationCode>
				<Address1></Address1>
			</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNotNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, parentBO.DocAddresses[0].DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", "Test Address 1", parentBO.DocAddresses[0].E2_Address1);
			});

			parentBO.DocAddresses.ToArray().DeleteAll();
			var consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "Old Test Address 1";
			consignor.E2_AddressOverride = true;
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertNotEquals("PRE: Parent should have docaddress with some address", string.Empty, consignor.E2_Address1);

			CombineAssertions(() =>
			{
				AssertNotNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
				consignor = parentBO.DocAddresses[0];
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", "Test Address 1", consignor.E2_Address1);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByRightCodeWithOverridenFlagWithEmptyField_NotCreateAndDelete()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<OrganizationCode>RIGHTCODE</OrganizationCode>
				<AddressOverride>true</AddressOverride>
				<Address1></Address1>
			</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should delete empty docaddress", 0, parentBO.DocAddresses.Count);
			});

			parentBO.DocAddresses.ToArray().DeleteAll();
			var consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "Old Test Address 1";
			consignor.E2_Address2 = "Old Test Address 2";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertEquals("PRE: Parent should have docaddress without test address 1", "Old Test Address 1", consignor.E2_Address1);
			AssertEquals("PRE: Parent should have docaddress without test address 2", "Old Test Address 2", consignor.E2_Address2);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByRightCodeWithoutOverridenFlagWithSomeField_CreateAndUpdateWithOriginalAddress()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<OrganizationCode>RIGHTCODE</OrganizationCode>
				<Address1>New Test Address 1</Address1>
			</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			AssertNotNull(reader.GetMatchedOrNew(parentBO));
			AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
			var consignor = parentBO.DocAddresses[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, parentBO.DocAddresses[0].DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", "Test Address 1", parentBO.DocAddresses[0].E2_Address1);
			});

			parentBO.DocAddresses.ToArray().DeleteAll();
			consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_CompanyName = "Ye Olde Test Company";
			consignor.E2_Address1 = "Old Test Address 1";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertNotEquals("PRE: Parent should have docaddress with some address", string.Empty, consignor.E2_Address1);

			CombineAssertions(() =>
			{
				AssertNotNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
				consignor = parentBO.DocAddresses[0];
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", "Test Address 1", consignor.E2_Address1);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchByRightCodeWithOverridenFlagWithSomeField_CreateAndUpdateWithOriginalAddress()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			Factory.SaveForTesting();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<OrganizationCode>RIGHTCODE</OrganizationCode>
				<AddressOverride>true</AddressOverride>
				<Address1>New Test Address 1</Address1>
			</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			AssertNotNull(reader.GetMatchedOrNew(parentBO));
			AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
			var consignor = parentBO.DocAddresses[0];
			CombineAssertions(() =>
			{
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", "New Test Address 1", consignor.E2_Address1);
			});

			parentBO.DocAddresses.ToArray().DeleteAll();
			consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_Address1 = "Old Test Address 1";
			consignor.E2_Address2 = "Old Test Address 2";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			consignor = parentBO.DocAddresses[0];
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
			AssertEquals("Should create docaddress of with test address 1", "Old Test Address 1", consignor.E2_Address1);
			AssertEquals("Should create docaddress of with test address 2", "Old Test Address 2", consignor.E2_Address2);

			CombineAssertions(() =>
			{
				AssertNotNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should create 1 docaddress", 1, parentBO.DocAddresses.Count);
				consignor = parentBO.DocAddresses[0];
				AssertEquals("Should create docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, consignor.DocAddressType);
				AssertEquals("Should create docaddress of with test address 1", "New Test Address 1", consignor.E2_Address1);
				AssertEquals("Should create docaddress of with test address 2", "Old Test Address 2", consignor.E2_Address2);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchWithoutCodeWithOverridenFlagWithEmptyField_NotCreateAndDelete()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			Factory.SaveForTesting();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
	<AddressOverride>true</AddressOverride>
	<Address1></Address1>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should not create empty docaddress", 0, parentBO.DocAddresses.Count);
			});

			var consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_Address1 = "Test Address 1";
			consignor.E2_Address2 = "Test Address 2";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, parentBO.DocAddresses[0].DocAddressType);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should have no docaddress", 0, parentBO.DocAddresses.Count);
			});
		}

		public void TestDontCreateEmptyJobDocAddresses_MatchWithoutCodeWithOverridenFlagWithEmptyField_NotCreateAndDelete_EXT()
		{
			SetupRightCodeAddressBOInDBForTesting();
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();

			parentBO.DocAddresses.ToArray().DeleteAll();
			Factory.SaveForTesting();
			AssertEquals("PRE: Parent should have 0 docaddress", 0, parentBO.DocAddresses.Count);
			var addressDataObject = GetOrganizationAddressFromXMLString(@"
<OrganizationAddress>
	<AddressType>ConsignorDocumentaryAddress</AddressType>
	<AddressOverride>true</AddressOverride>
</OrganizationAddress>", logger);
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should not create empty docaddress", 0, parentBO.DocAddresses.Count);
			});

			var consignor = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			consignor.E2_AddressOverride = true;
			consignor.E2_Address1 = "Test Address 1";
			consignor.E2_Address2 = "Test Address 2";
			Factory.SaveForTesting();

			AssertEquals("PRE: Parent should have 1 docaddress", 1, parentBO.DocAddresses.Count);
			AssertEquals("PRE: Parent should have docaddress of ConsignorDocumentaryAddress", DocAddressType.ConsignorDocumentaryAddress, parentBO.DocAddresses[0].DocAddressType);

			CombineAssertions(() =>
			{
				AssertNull(reader.GetMatchedOrNew(parentBO));
				AssertEquals("Should delete empty docaddress", 0, parentBO.DocAddresses.Count);
			});
		}

		public void TestJobDocAddressMatchUsesDocAddressOverrideOverSuppliedAddressType()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD("Divine Address");
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO, OrganisationTypes.Consignor, null, DocAddressType.ConsignorDocumentaryAddress);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_OA_Address", addressBOForSetup.PK, jobDocAddressBO.E2_OA_Address);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", false, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.PK", jobDocInCollection.PK, jobDocAddressBO.PK);

			var organisationBO = jobDocAddressBO.Organisation;

			AssertNotNull(organisationBO);
			AssertOrgHeaderContents(organisationBO);
		}

		public void TestDelayUpdatingOrCreatingJobDocAddress()
		{
			var organisation1 = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var organisation2 = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			Factory.SaveForTesting();

			var addressDataObject1 = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ContainerLegPickupAddress));
			var addressDataObject2 = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.TransportCompanyDocumentaryAddress));

			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			parentBO.SupportedAddressTypesForTesting = new[] { DocAddressType.ContainerLegPickupAddress, DocAddressType.CustomsTreatmentProviderAddress };
			parentBO.GetRegistrationNumberForTesting = (x) => new Business.RegistrationNumber() { Number = x == DocAddressType.ContainerLegPickupAddress ? "55555" : "TAXYOU", NumberType = "GST" };
			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ContainerLegPickupAddress);
			var logger = new TestErrorLogger();
			var delaySetters = new Dictionary<string, ValueSetter>();
			OrganisationDataObjectReader.MatchedOrNew(parentBO, addressDataObject1, logger, Factory, delaySetters);
			OrganisationDataObjectReader.MatchedOrNew(parentBO, addressDataObject2, logger, Factory, delaySetters, DocAddressType.CustomsTreatmentProviderAddress);
			AssertEquals("jobDocInCollection.IsEmpty", true, jobDocInCollection.IsEmpty);
			delaySetters[OrganizationDataObjectValueSetter.GetKey(parentBO.PK, nameof(DocAddressType.ContainerLegPickupAddress))].SetValue();
			jobDocAddressCollection.Load();
			AssertEquals("jobDocAddressCollection.Count", 1, jobDocAddressCollection.Count);

			OrganizationAddressTestHelper.AssertJobDocAddressContentMatches_INTHEMSYD(jobDocInCollection);
			AssertEquals("jobDocAddressBO.E2_OA_Address", organisation1.MainAddress.PK, jobDocInCollection.E2_OA_Address);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", false, jobDocInCollection.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_AddressType", DocAddressTypes.Codes.ContainerLegPickupAddress, jobDocInCollection.E2_AddressType);

			jobDocInCollection.Delete();
			jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ContainerLegPickupAddress);
			OrganisationDataObjectReader.MatchedOrNew(parentBO, addressDataObject1, logger, Factory, null);
			OrganisationDataObjectReader.MatchedOrNew(parentBO, addressDataObject2, logger, Factory, null, DocAddressType.CustomsTreatmentProviderAddress);
			AssertEquals("jobDocInCollection.IsEmpty", false, jobDocInCollection.IsEmpty);
			jobDocAddressCollection.Load();
			AssertEquals("jobDocAddressCollection.Count", 2, jobDocAddressCollection.Count);

			var docAddress1 = jobDocAddressCollection[0];
			var docAddress2 = jobDocAddressCollection[1];
			if (docAddress2.PK == jobDocInCollection.PK)
			{
				docAddress1 = jobDocAddressCollection[1];
				docAddress2 = jobDocAddressCollection[0];
			}

			OrganizationAddressTestHelper.AssertJobDocAddressContentMatches_INTHEMSYD(docAddress1);
			AssertEquals("docAddress1.E2_OA_Address", organisation1.MainAddress.PK, docAddress1.E2_OA_Address);
			AssertEquals("docAddress1.E2_AddressOverride", false, docAddress1.E2_AddressOverride);
			AssertEquals("docAddress1.E2_AddressType", DocAddressTypes.Codes.ContainerLegPickupAddress, docAddress1.E2_AddressType);
			AssertEquals("docAddress1", jobDocInCollection, docAddress1);

			OrganizationAddressTestHelper.AssertJobDocAddressContentMatches_CRAHOLSYD(docAddress2);
			AssertEquals("docAddress2.E2_OA_Address", organisation2.MainAddress.PK, docAddress2.E2_OA_Address);
			AssertEquals("docAddress2.E2_AddressOverride", false, docAddress2.E2_AddressOverride);
			AssertEquals("docAddress2.E2_AddressType", DocAddressTypes.Codes.CustomsTreatmentProviderAddress, docAddress2.E2_AddressType);
		}

		public void TestJobDocAddressUpdateGetsCountryCodeFromIATACodeIfCountryIsNotPresent()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.Country = null;
			addressDataObject.Port.Code = "Mel";
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();
			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", false, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", addressBOForSetup.PK, jobDocAddressBO.E2_OA_Address);

			var organisationBO = jobDocAddressBO.Organisation;

			AssertNotNull(organisationBO);
			AssertOrgHeaderContents(organisationBO);
		}

		public void TestJobDocAddressUpdateGetsCountryCodeFromPortIfCountryIsNotPresent()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.Country = null;
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", false, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", addressBOForSetup.PK, jobDocAddressBO.E2_OA_Address);

			var organisationBO = jobDocAddressBO.Organisation;

			AssertNotNull(organisationBO);
			AssertOrgHeaderContents(organisationBO);
		}

		public void TestWhenLinkingToExistingOrgHeaderWithNoPortUsesCountryCode()
		{
			var addressData = GetNewAddressData_INTHEMSYD("Fooey");
			addressData.Port = null;

			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressData, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressData, logger, new UniversalObjectFactory());
			var addressBO = reader.GetMatchedOrNewForTesting();

			AssertNotNull("addressBO", addressBO);
			CombineAssertions(delegate
			{
				AssertEquals("addressBO.IsInDatabase", true, addressBO.IsInDatabase);
				AssertEquals("addressBO.HasChanges", false, addressBO.HasChanges);
				AssertEquals("addressBO.PK", addressBOForSetup.PK, addressBO.PK);
			});

			AssertEquals("addressBO.OA_RL_NKRelatedPortCode", "AU", addressBO.OA_RL_NKRelatedPortCode);
		}

		public void TestWhenLinkingToExistingOrgHeaderGetsCountryCodeFromPortIfCountryIsNotPresent()
		{
			var addressData = GetNewAddressData_INTHEMSYD("Fooey");
			addressData.Country = null;

			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressData, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressData, logger, new UniversalObjectFactory());
			var addressBO = reader.GetMatchedOrNewForTesting();

			AssertNotNull("addressBO", addressBO);
			CombineAssertions(delegate
			{
				AssertEquals("addressBO.IsInDatabase", true, addressBO.IsInDatabase);
				AssertEquals("addressBO.HasChanges", false, addressBO.HasChanges);
				AssertEquals("addressBO.PK", addressBOForSetup.PK, addressBO.PK);
			});
			AssertAddressContentMatches_INTHEMSYD(addressBO);
		}

		public void TestJobDocAddressGetsCountryCodeFromPortIfCountryIsNotPresent()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.Country = null;
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew();
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", true, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBO.E2_OA_Address);
			AssertNotEquals("jobDocAddressBO.PK", jobDocInCollection.PK, jobDocAddressBO.PK);
		}

		public void TestReadOrganizationWithNoPortUsesCountryCode()
		{
			var addressData = GetNewAddressData_INTHEMSYD("Fooey");
			addressData.Port = null;

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressData, logger, Factory);

			var addressBO = reader.GetMatchedOrNewForTesting();

			AssertNotNull("addressBO", addressBO);
			var organizationBO = addressBO.Header;
			AssertNotNull("organizationBO", organizationBO);
			AssertEquals("organizationBO.IsInDatabase", false, organizationBO.IsInDatabase);

			AssertEquals("addressBO.OA_RL_NKRelatedPortCode", "AU", addressBO.OA_RL_NKRelatedPortCode);
		}

		public void TestReadOrganizationWithNoCountryUsesPortCode()
		{
			var addressData = GetNewAddressData_INTHEMSYD("Fooey");
			addressData.Country = null;

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressData, logger, Factory);

			var addressBO = reader.GetMatchedOrNewForTesting();

			AssertNotNull("addressBO", addressBO);
			var organizationBO = addressBO.Header;
			AssertNotNull("organizationBO", organizationBO);
			AssertEquals("organizationBO.IsInDatabase", false, organizationBO.IsInDatabase);

			AssertAddressContentMatches_INTHEMSYD(addressBO);
			AssertEquals("addressBO.OA_Code", "THEMOMENT", addressBO.OA_Code);
		}

		public void TestReadE2_IsResidential()
		{
			var addressData = GetNewAddressData_INTHEMSYD("Fooey");
			addressData.IsResidential = true;

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressData, logger, Factory);
			var addressParent = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var jobDocAddress = reader.GetMatchedOrNew(addressParent, DocAddressType.ConsignorDocumentaryAddress);
			AssertNotNull("JobDocAddress was populated", jobDocAddress);
			Assert(AutoJobDocAddress.Schema.E2_IsResidential + " was not populated", !jobDocAddress.E2_IsResidential);

			reader.PopulateIsResidential = true;
			jobDocAddress = reader.GetMatchedOrNew(addressParent, DocAddressType.ConsignorDocumentaryAddress);
			AssertNotNull("JobDocAddress was populated", jobDocAddress);
			Assert(AutoJobDocAddress.Schema.E2_IsResidential + " was populated", jobDocAddress.E2_IsResidential);
		}

		public void TestReadOrganization()
		{
			var addressData = GetNewAddressData_INTHEMSYD("Fooey");

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressData, logger, Factory);

			var addressBO = reader.GetMatchedOrNewForTesting();

			AssertNotNull("addressBO", addressBO);
			var organizationBO = addressBO.Header;
			AssertNotNull("organizationBO", organizationBO);
			AssertEquals("organizationBO.IsInDatabase", false, organizationBO.IsInDatabase);

			AssertAddressContentMatches_INTHEMSYD(addressBO);
			AssertEquals("addressBO.OA_Code", "THEMOMENT", addressBO.OA_Code);
		}

		public void TestGetMatchedOrNew_ReadAdditionalAddressInformation_WhenAddressNotExists()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.AddressOverride = false;

			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var reader = new OrganisationDataObjectReader(addressDataObject, new TestErrorLogger(), Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);
			AssertEquals(true, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("AAI", jobDocAddressBO.AdditionalAddressInformation);
		}

		public void TestGetMatchedOrNew_ReadAdditionalAddressInformation_WhenAddressExists()
		{
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.AddressOverride = false;

			var header = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = (ZString)addressDataObject.OrganizationCode;
			header.OH_FullName = (ZString)addressDataObject.CompanyName;

			var address = header.Addresses.AddNew();
			address.OA_Address1 = (ZString)addressDataObject.Address1;

			var additionalInfo = address.AdditionalInfos.AddNew();
			additionalInfo.OAI_AdditionalInfo = "Additional Address Info";
			additionalInfo.OAI_IsPrimary = true;

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				header.OH_OverrideAdditionalAddressInformation = false;

				Factory.SaveForTesting();

				var reader = new OrganisationDataObjectReader(addressDataObject, new TestErrorLogger(), Factory);
				var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

				Assert("OH_OverrideAdditionalAddressInformation is false", !header.OH_OverrideAdditionalAddressInformation);
				AssertEquals(false, jobDocAddressBO.E2_AddressOverride);
				AssertEquals(string.Empty, ((INeedRow)jobDocAddressBO).Row[JobDocAddressSchema.E2_AdditionalAddressInformation.Name]);
				AssertEquals("Fallback to use org address additional information", "Additional Address Info", jobDocAddressBO.E2_AdditionalAddressInformation);

				addressDataObject.AdditionalAddressInformation = "ADDITIONAL ADDRESS INFO";
				jobDocAddressBO = reader.GetMatchedOrNew(parentBO);
				AssertEquals(false, jobDocAddressBO.E2_AddressOverride);
				AssertEquals("Has matched additional info but registry is disabled", string.Empty, ((INeedRow)jobDocAddressBO).Row[JobDocAddressSchema.E2_AdditionalAddressInformation.Name]);
				AssertEquals("Fallback to use org address additional information", "Additional Address Info", jobDocAddressBO.E2_AdditionalAddressInformation);
			}

			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new OrganisationDataObjectReader(addressDataObject, new TestErrorLogger(), Factory);
				var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);
				AssertEquals(false, jobDocAddressBO.E2_AddressOverride);
				AssertEquals("Has matched additional info and registry is enabled", "ADDITIONAL ADDRESS INFO", ((INeedRow)jobDocAddressBO).Row[JobDocAddressSchema.E2_AdditionalAddressInformation.Name]);
				AssertEquals("Use it's own value", "ADDITIONAL ADDRESS INFO", jobDocAddressBO.E2_AdditionalAddressInformation);

				header.OH_OverrideAdditionalAddressInformation = true;
				Factory.SaveForTesting();

				jobDocAddressBO = reader.GetMatchedOrNew(parentBO);
				jobDocAddressBO.AdditionalAddressInformation = "AAI";
				Assert("OH_OverrideAdditionalAddressInformation is true", header.OH_OverrideAdditionalAddressInformation);
				AssertEquals(false, jobDocAddressBO.E2_AddressOverride);
				AssertEquals("AAI", ((INeedRow)jobDocAddressBO).Row[JobDocAddressSchema.E2_AdditionalAddressInformation.Name]);
				AssertEquals("AAI", jobDocAddressBO.E2_AdditionalAddressInformation);
			}
		}

		public void TestGetMatchedOrNew_ReadAdditionalAddressInformation_OverrideIsTrue()
		{
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.AddressOverride = true;

			var header = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = (ZString)addressDataObject.OrganizationCode;
			header.OH_FullName = (ZString)addressDataObject.CompanyName;

			var address = header.Addresses.AddNew();
			address.OA_Address1 = (ZString)addressDataObject.Address1;

			Factory.SaveForTesting();

			var reader = new OrganisationDataObjectReader(addressDataObject, new TestErrorLogger(), Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertEquals(true, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("AAI", jobDocAddressBO.E2_AdditionalAddressInformation);
		}

		public void TestJobDocAddressGetsAddressShortCodeWhenReadingIn()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", false, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", addressBOForSetup.PK, jobDocAddressBO.E2_OA_Address);

			var addressBO = jobDocAddressBO.Address;
			AssertEquals("addressBO.OA_Code", "THEMOMENT", addressBO.OA_Code);

			var organisationBO = jobDocAddressBO.Organisation;

			AssertNotNull(organisationBO);
			AssertOrgHeaderContents(organisationBO);
		}

		public void TestNoMatchAddsNewOrgHeader()
		{
			var addressData = GetNewAddressData_INTHEMSYD("Fooey");

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressData, logger, Factory);

			var addressBO = reader.GetMatchedOrNewForTesting();

			AssertNotNull("addressBO", addressBO);
			var organizationBO = addressBO.Header;
			AssertNotNull("organizationBO", organizationBO);
			AssertEquals("organizationBO.IsInDatabase", false, organizationBO.IsInDatabase);

			AssertAddressContentMatches_INTHEMSYD(addressBO);

			AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
				".Trim(), logger.Logs);
		}

		public void TestMatchLinksToExistingOrgHeader()
		{
			var addressData = GetNewAddressData_INTHEMSYD("Fooey");

			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressData, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressData, logger, new UniversalObjectFactory());
			var addressBO = reader.GetMatchedOrNewForTesting();

			AssertNotNull("addressBO", addressBO);
			CombineAssertions(delegate
			{
				AssertEquals("addressBO.IsInDatabase", true, addressBO.IsInDatabase);
				AssertEquals("addressBO.HasChanges", false, addressBO.HasChanges);
				AssertEquals("addressBO.PK", addressBOForSetup.PK, addressBO.PK);
			});
			AssertAddressContentMatches_INTHEMSYD(addressBO);
		}

		public void TestJobDocAddressUpdateUpdatesIfExistsByTypeAndOrgAddressHasAMatch()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", false, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", addressBOForSetup.PK, jobDocAddressBO.E2_OA_Address);

			var organisationBO = jobDocAddressBO.Organisation;

			AssertNotNull(organisationBO);
			AssertOrgHeaderContents(organisationBO);
		}

		public void TestJobDocAddressUpdateOverridesIfFlagSetIfExistsByTypeEvenIfOrgAddressHasAMatch()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.AddressOverride = true;

			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", true, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBO.E2_OA_Address);
			AssertEquals("jobDocAddressBO.Address.OA_Code", "OFC: NO ADDRESS SPECIFIED", jobDocAddressBO.Address.OA_Code);
		}

		public void TestJobDocAddressUpdateUpdatesIfExistsByTypeAndOrgAddressHasNoMatch()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", true, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBO.E2_OA_Address);
			AssertEquals("jobDocAddressBO.PK", jobDocInCollection.PK, jobDocAddressBO.PK);
		}

		public void TestJobDocAddressUpdateCreatesNewIfNotExistingAndOrgAddressHasNoMatch()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew();
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", true, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBO.E2_OA_Address);
			AssertNotEquals("jobDocAddressBO.PK", jobDocInCollection.PK, jobDocAddressBO.PK);
		}

		public void TestJobDocAddressUpdateCreatesNewIfNotExistingAndOrgAddressHasAMatch()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew();
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", false, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", addressBOForSetup.PK, jobDocAddressBO.E2_OA_Address);
			AssertNotEquals("jobDocAddressBO.PK", jobDocInCollection.PK, jobDocAddressBO.PK);
		}

		public void TestJobDocAddressMatchLinksToExistingOrgHeaderWithValidAddressType()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);

			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_OA_Address", addressBOForSetup.PK, jobDocAddressBO.E2_OA_Address);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", false, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.PK", jobDocInCollection.PK, jobDocAddressBO.PK);

			var organisationBO = jobDocAddressBO.Organisation;

			AssertNotNull(organisationBO);
			AssertOrgHeaderContents(organisationBO);
		}

		public void TestJobDocAddressMatchLogsWarningWithInvalidAddressType()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD("Divine Address");
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew();
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNull(jobDocAddressBO);

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Matching 'Divine Address':- Matched to 'INTHEMSYD' by code, address 'THEMOMENT' (only address).
Warning - Unknown Address Type [Divine Address] found. Job Document Address not imported.
			".Trim(), logger.Logs);
		}

		public void TestAddAdditionalUnmatchedNoteWhenUnmatched()
		{
			var addressDataObject = OrganizationDataObjectReaderTest.GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var org = Factory.New<OrgHeader>();
			AssertEquals("Precondition: No Unmatched note.", 0, org.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description).Length);

			reader.AddAdditionalUnmatchedNoteWhenUnmatched(org, OrganisationTypes.Consignee, ZGuid.NewZGuid());
			AssertEquals("Should still be no Unmatched note unless the Unmatched org is used.", 0, org.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description).Length);

			reader.AddAdditionalUnmatchedNoteWhenUnmatched(org, OrganisationTypes.Consignee, OrgHeader.UnmatchedOrganisationPK);

			var foundNotes = org.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("There should be one UNMATCHED note", 1, foundNotes.Length);
			ZString serialisedNoteText = @"Organisation Type: Consignee
Owner Code: 
EDI Code: INTHEMSYD
Organisation Name: In The Moment
Address Line 1: Unit 12, Level 3
Address Line 2: 233 Here St
City: ThereVille
Post Code: 1233
State or Province: OfBliss
Country: AU
Doc Address Type: 
 ";
			AssertMultilineASCIIEquals("Note text should describe DUMMY CONSIGNEE", serialisedNoteText, foundNotes[0].ST_NoteText);
		}

		public void TestJobDocOverrideAddressUpdateIfFlagNullIfOrgAddressHasAMatch()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.AddressOverride = true;

			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var jobDocAddressCollection = parentBO.DocAddresses;
			var jobDocInCollection = jobDocAddressCollection.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);
			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", true, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBO.E2_OA_Address);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.Address.OA_Code", "OFC: NO ADDRESS SPECIFIED", jobDocAddressBO.Address.OA_Code);

			addressDataObject.AddressOverride = null;
			jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertNotNull(jobDocAddressBO);
			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressOverride", false, jobDocAddressBO.E2_AddressOverride);
			AssertEquals("jobDocAddressBO.E2_OA_Address", addressBOForSetup.PK, jobDocAddressBO.E2_OA_Address);
			AssertEquals("jobDocAddressBO.E2_AddressType", "CRD", jobDocAddressBO.E2_AddressType);
			AssertEquals("jobDocAddressBO.PK", jobDocInCollection.PK, jobDocAddressBO.PK);
		}

		public void TestOrganizationDataObjectValueSetter_AddressTypeOverride()
		{
			var declaration = Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var logger = new TestErrorLogger();
			var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress), Address1 = "ADD" };
			var docAddresses = (IDocAddresses)declaration;
			var collection = docAddresses.DocAddresses;
			collection.RemoveAndDeleteAll();
			AssertEquals("ConsigneeDocumentaryAddress is not supported", false, docAddresses.SupportedAddressTypes.Contains(DocAddressType.ConsigneeDocumentaryAddress));
			var setter = new OrganizationDataObjectValueSetter(docAddresses, addressData, logger, Factory);
			AssertEquals("setter.MatchingKey", OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.ConsigneeDocumentaryAddress)), setter.MatchingKey);
			setter.SetValue();
			AssertEquals("no docAddress added", 0, collection.Count);
			AssertEquals("ImporterDocumentaryAddress is supported", true, docAddresses.SupportedAddressTypes.Contains(DocAddressType.ImporterDocumentaryAddress));
			setter = new OrganizationDataObjectValueSetter(docAddresses, addressData, logger, Factory, DocAddressType.ImporterDocumentaryAddress);
			AssertEquals("setter.MatchingKey", OrganizationDataObjectValueSetter.GetKey(declaration.PK, nameof(DocAddressType.ImporterDocumentaryAddress)), setter.MatchingKey);
			setter.SetValue();
			AssertEquals("1 docAddress was added", 1, collection.Count);
			var docAddress = collection[0];
			AssertEquals("docAddress.E2_AddressType", DocAddressTypes.Codes.ImporterDocumentaryAddress, docAddress.E2_AddressType);
		}

		public void TestGetMatchedOrNew_AddressTypeOverride()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			parentBO.SupportedAddressTypesForTesting = new[] { DocAddressType.ConsignorDocumentaryAddress, DocAddressType.SupplierDocumentaryAddress };
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO, DocAddressType.SupplierDocumentaryAddress);
			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", DocAddressTypes.Codes.SupplierDocumentaryAddress, jobDocAddressBO.E2_AddressType);

			jobDocAddressBO.Delete();
			jobDocAddressBO = reader.GetMatchedOrNew(parentBO);
			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_AddressType", DocAddressTypes.Codes.ConsignorDocumentaryAddress, jobDocAddressBO.E2_AddressType);
		}

		public void TestGetMatchedOrNew_WhenGettingOverriddenAddressWithSuppresion_ShouldUpdateSuppressAddressValidationToTrue()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.AddressOverride = true;
			addressDataObject.SuppressAddressValidationError = true;

			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertEquals(
				"jobDocAddressBO.E2_SuppressAddressValidationError",
				ZBool.True,
				jobDocAddressBO.E2_SuppressAddressValidationError);
		}

		public void TestGetMatchedOrNew_WhenGettingOverriddenAddressWithoutSuppression_ShouldUpdateSuppressAddressValidationToFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.AddressOverride = true;
			addressDataObject.SuppressAddressValidationError = false;

			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertEquals(
				"jobDocAddressBO.E2_SuppressAddressValidationError",
				ZBool.False,
				jobDocAddressBO.E2_SuppressAddressValidationError);
		}

		public void TestGetMatchedOrNew_WhenGettingOverriddenAddressWithNoSuppressionDefined_ShouldUpdateSuppressAddressValidationToFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.AddressOverride = true;

			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertEquals(
				"jobDocAddressBO.E2_SuppressAddressValidationError",
				ZBool.False,
				jobDocAddressBO.E2_SuppressAddressValidationError);
		}

		public void TestGetMatchedOrNew_WhenGettingNonOverriddenAddress_ShouldUpdateSuppressAddressValidationToFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.AddressOverride = false;
			addressDataObject.SuppressAddressValidationError = true;

			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObject, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);

			AssertEquals(
				"jobDocAddressBO.E2_SuppressAddressValidationError",
				ZBool.False,
				jobDocAddressBO.E2_SuppressAddressValidationError);
		}

		public void TestGetMatchedOrNew_WhenMatchedOrgAddressIsGiven()
		{
			var addressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "TRA" };
			var orgAddress = Factory.BOFactory.NewWithValidTestData<OrgAddress>();

			var parentBO = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			parentBO.SupportedAddressTypesForTesting = new DocAddressType[] { DocAddressType.TransportCompanyDocumentaryAddress };

			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO, orgAddress, DocAddressType.TransportCompanyDocumentaryAddress);

			AssertNotNull("Precondition: jobDocAddressBO", jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_OA_PK", orgAddress.PK, jobDocAddressBO.E2_OA_Address);
			AssertEquals("jobDocAddressBO.E2_ParentId", parentBO.PK, jobDocAddressBO.E2_ParentID);
		}

		public void TestAddDocAddressNumberWhenAddressOverrideIsTrue()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			addressDataObject.AddressOverride = true;
			var parentBO = new JobDocAddressParentWithSupportsDocAddressNumbersIsTrueForTesting(Factory.BOFactory);
			var logger = new TestErrorLogger();
			var reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			var jobDocAddressBO = reader.GetMatchedOrNew(parentBO);
			var docAddressNumbers = jobDocAddressBO.DocAddressNumbers;
			AssertEquals(2, docAddressNumbers.Count);
			AssertEquals("DocAddressNumber.E2N_NumberType", "GST", docAddressNumbers[0].E2N_NumberType);
			AssertEquals("DocAddressNumber.E2N_Number", "55555", docAddressNumbers[0].E2N_Number);
			AssertEquals("DocAddressNumber.CountryCode", "AU", docAddressNumbers[0].E2N_RN_NKCountryCode);
			AssertEquals("DocAddressNumber.E2N_NumberType", "atf", docAddressNumbers[1].E2N_NumberType);
			AssertEquals("DocAddressNumber.E2N_Number", "1234F", docAddressNumbers[1].E2N_Number);
			AssertEquals("DocAddressNumber.CountryCode", "NZ", docAddressNumbers[1].E2N_RN_NKCountryCode);

			addressDataObject = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
				AddressShortCode = "THEMOMENT",
				AddressOverride = false,

				OrganizationCode = "INTHEMSYD",
				CompanyName = "In The Moment",
				Address1 = "Unit 12, Level 3",
				Address2 = "233 Here St",
				City = "ThereVille",
				State = "OfBliss",
				Postcode = "1233",
				Port = new UNLOCO { Code = "AUMel", Name = "Melbourne" },
				Country = new Country { Code = "au", Name = "Australia" },

				Contact = "Starshine Moonbeam",
				Email = "s.m@moment.com.au",
				Fax = "234098234",
				Mobile = "234098293",
				Phone = "1239813209",

				ScreeningStatus = new CodeDescriptionPair { Code = "Unk", Description = "Unknown" },
				UniversalOfficeCode = "454",
				UniversalNettingCode = "545",
				AdditionalAddressInformation = "AAI"
			};
			addressDataObject.SetRegistrationNumberCollection(() => new List<RegistrationNumber>(new[]
				{
					new RegistrationNumber
					{
						Type = new RegistrationNumberType { Code = "TPC", Description = "Tax payment on account business identifier, coded" },
						CountryOfIssue = new Country { Code = "NZ", Name = "New Zealand" },
						Value = "6666A",
					},
					new RegistrationNumber
					{
						Type = new RegistrationNumberType { Code = "FTZ", Description = "Free Trade Zone" },
						CountryOfIssue = new Country { Code = "CN", Name = "Chinese" },
						Value = "8888A",
					},
					new RegistrationNumber
					{
						Type = new RegistrationNumberType { Code = "AEO", Description = "Authorized Economic Operator" },
						CountryOfIssue = new Country { Code = "AU", Name = "Australia" },
						Value = "9999Z",
					}
				}));
			addressDataObject.AddressOverride = true;
			parentBO = new JobDocAddressParentWithSupportsDocAddressNumbersIsTrueForTesting(Factory.BOFactory);
			var consignorDocumentaryAddress = parentBO.DocAddresses.AddNew(DocAddressType.ConsignorDocumentaryAddress);
			var cbfAddressNumber = consignorDocumentaryAddress.DocAddressNumbers.FindOrCreate("CBF", "TW");
			cbfAddressNumber.E2N_Number = "1111C";
			reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			jobDocAddressBO = reader.GetMatchedOrNew(parentBO);
			docAddressNumbers = jobDocAddressBO.DocAddressNumbers;
			AssertEquals("DocAddressNumbers should have 3 records nor 4 records becuase the CBF was removed.", 3, docAddressNumbers.Count);
			AssertEquals("DocAddressNumber.E2N_NumberType", "TPC", docAddressNumbers[0].E2N_NumberType);
			AssertEquals("DocAddressNumber.E2N_Number", "6666A", docAddressNumbers[0].E2N_Number);
			AssertEquals("DocAddressNumber.CountryCode", "NZ", docAddressNumbers[0].E2N_RN_NKCountryCode);
			AssertEquals("DocAddressNumber.E2N_NumberType", "FTZ", docAddressNumbers[1].E2N_NumberType);
			AssertEquals("DocAddressNumber.E2N_Number", "8888A", docAddressNumbers[1].E2N_Number);
			AssertEquals("DocAddressNumber.CountryCode", "CN", docAddressNumbers[1].E2N_RN_NKCountryCode);
			AssertEquals("DocAddressNumber.E2N_NumberType", "AEO", docAddressNumbers[2].E2N_NumberType);
			AssertEquals("DocAddressNumber.E2N_Number", "9999Z", docAddressNumbers[2].E2N_Number);
			AssertEquals("DocAddressNumber.CountryCode", "AU", docAddressNumbers[2].E2N_RN_NKCountryCode);

			var parentBOWithSupportsDocAddressNumbersIsFalse = new JobDocAddressParentForTestingExtended(Factory.BOFactory);
			reader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			jobDocAddressBO = reader.GetMatchedOrNew(parentBOWithSupportsDocAddressNumbersIsFalse);
			docAddressNumbers = jobDocAddressBO.DocAddressNumbers;
			Assert("DocAddressNumbers is empty because the property 'SupportsDocAddressNumbers' is false.", !docAddressNumbers.Any());
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenValidNoOverrideAndAddressPKMatchesThenShouldBeTrue()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			addressDataObject.Contact = string.Empty;
			addressDataObject.AdditionalAddressInformation = string.Empty;
			var (jobDocAddress, orgAddress) = GetNewJobDocAddressWithOrgAddress();
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return true as data object address override is off and linked E2_OA_Address matches OrgAddress PK", true, orgReader.IsJobDocAddressMatchingOrgAddress(orgAddress, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenValidNullOverrideAndAddressPKMatchesThenShouldBeTrue()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			addressDataObject.AddressOverride = null;
			addressDataObject.Contact = string.Empty;
			addressDataObject.AdditionalAddressInformation = string.Empty;
			var (jobDocAddress, orgAddress) = GetNewJobDocAddressWithOrgAddress();
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return true as data object address override is null and linked E2_OA_Address matches OrgAddress PK", true, orgReader.IsJobDocAddressMatchingOrgAddress(orgAddress, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenValidNoOverrideButAddressPKDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			addressDataObject.Contact = string.Empty;
			addressDataObject.AdditionalAddressInformation = string.Empty;
			var (jobDocAddress, orgAddress) = GetNewJobDocAddressWithOrgAddress();
			jobDocAddress.E2_OA_Address = ZGuid.NewZGuid();
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is off but linked E2_OA_Address does not match OrgAddress PK", false, orgReader.IsJobDocAddressMatchingOrgAddress(orgAddress, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenValidNullOverrideButAddressPKDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			addressDataObject.AddressOverride = null;
			addressDataObject.Contact = string.Empty;
			addressDataObject.AdditionalAddressInformation = string.Empty;
			var (jobDocAddress, orgAddress) = GetNewJobDocAddressWithOrgAddress();
			jobDocAddress.E2_OA_Address = ZGuid.NewZGuid();
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is null but linked E2_OA_Address does not match OrgAddress PK", false, orgReader.IsJobDocAddressMatchingOrgAddress(orgAddress, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenDataObjectHasNoAddressOverrideButJobDocAddressHasOverrideThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			addressDataObject.Contact = string.Empty;
			addressDataObject.AdditionalAddressInformation = string.Empty;
			var (jobDocAddress, orgAddress) = GetNewJobDocAddressWithOrgAddress();
			jobDocAddress.E2_OA_Address = ZGuid.Empty;
			jobDocAddress.E2_AddressOverride = true;
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is off but E2_AddressOverride is true", false, orgReader.IsJobDocAddressMatchingOrgAddress(orgAddress, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenDataObjectHasNullOverrideButJobDocAddressHasOverrideThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			addressDataObject.AddressOverride = null;
			addressDataObject.Contact = string.Empty;
			addressDataObject.AdditionalAddressInformation = string.Empty;
			var (jobDocAddress, orgAddress) = GetNewJobDocAddressWithOrgAddress();
			jobDocAddress.E2_OA_Address = ZGuid.Empty;
			jobDocAddress.E2_AddressOverride = true;
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is null but E2_AddressOverride is true", false, orgReader.IsJobDocAddressMatchingOrgAddress(orgAddress, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenDataObjectHasNoAddressOverrideButOrgAddressIsNullThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			addressDataObject.Contact = string.Empty;
			addressDataObject.AdditionalAddressInformation = string.Empty;
			var (jobDocAddress, _) = GetNewJobDocAddressWithOrgAddress();
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is off but OrgAddress passed in is null", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenDataObjectHasNullOverrideButOrgAddressIsNullThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			addressDataObject.AddressOverride = null;
			addressDataObject.Contact = string.Empty;
			addressDataObject.AdditionalAddressInformation = string.Empty;
			var (jobDocAddress, orgAddress) = GetNewJobDocAddressWithOrgAddress();
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is null but OrgAddress passed in is null", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenValidNoOverrideButAdditionalAddressInformationDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			addressDataObject.Contact = string.Empty;
			addressDataObject.AdditionalAddressInformation = "different info";
			var (jobDocAddress, orgAddress) = GetNewJobDocAddressWithOrgAddress();
			orgAddress.OA_AdditionalAddressInformation = "some info";
			AssertEquals("Precondition: JobDocAddress.E2_AdditionalAddressInformation has non-empty value 'some info' that is different to addressDataObject.AdditionalAddressInformation - E2_AdditionalAddressInformation comes from linked OrgAddress.OA_AdditionalAddressInformation", "some info", jobDocAddress.E2_AdditionalAddressInformation);

			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is off but E2_AdditionalAddressInformation does not match data object AdditionalAddressInformation", false, orgReader.IsJobDocAddressMatchingOrgAddress(orgAddress, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenValidNullOverrideButAdditionalAddressInformationDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			addressDataObject.AddressOverride = null;
			addressDataObject.Contact = string.Empty;
			addressDataObject.AdditionalAddressInformation = "different info";
			var (jobDocAddress, orgAddress) = GetNewJobDocAddressWithOrgAddress();
			orgAddress.OA_AdditionalAddressInformation = "some info";
			AssertEquals("Precondition: JobDocAddress.E2_AdditionalAddressInformation has non-empty value 'some info' that is different to addressDataObject.AdditionalAddressInformation - E2_AdditionalAddressInformation comes from linked OrgAddress.OA_AdditionalAddressInformation", "some info", jobDocAddress.E2_AdditionalAddressInformation);

			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);

			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is null but E2_AdditionalAddressInformation does not match data object AdditionalAddressInformation", false, orgReader.IsJobDocAddressMatchingOrgAddress(orgAddress, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndAddress1DoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_Address1 = "Different Address1";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_Address1 does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndAddress2DoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_Address2 = "Different Address2";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_Address2 does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndCityDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_City = "Different City";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_City does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndCompanyNameDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_CompanyName = "Different Company Name";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_CompanyName does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndEmailDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_Email = "Different Email";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_Email does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndFaxDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_Fax = "Different Fax";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_Fax does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndGovRegNumDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_GovRegNum = "Different GovRegNum";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_GovRegNum does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndMobileDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_Mobile = "Different Mobile";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_Mobile does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndPhoneDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_Phone = "Different Phone";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_Phone does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndPostcodeDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_Postcode = "XXXXX";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_Postcode does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndStateDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_State = "Different State";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_State does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndCountryCodeDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_RN_NKCountryCode = "XX";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_RN_NKCountryCode does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndContactDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_Contact = "Different Contact";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_Contact does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndAdditionalAddressInformationDoesNotMatchThenShouldBeFalse()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			jobDocAddress.E2_AdditionalAddressInformation = "Different Additional Address Information";
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return false as data object address override is on but E2_AdditionalAddressInformation does not match", false, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenOverrideAndAllAddressDetailsMatchThenShouldBeTrue()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var jobDocAddress = GetNewJobDocAddressWithOverride_INTHEMSYD();
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return true as data object address override is on and all address details match", true, orgReader.IsJobDocAddressMatchingOrgAddress(null, jobDocAddress));
		}

		public void TestIsJobDocAddressMatchingOrgAddressWhenValidNoOverrideAndAddressPKMatchesEvenIfContactNotBlankThenShouldBeTrue()
		{
			var addressDataObject = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			var (jobDocAddress, orgAddress) = GetNewJobDocAddressWithOrgAddress();
			orgAddress.OA_AdditionalAddressInformation = addressDataObject.AdditionalAddressInformation.Value;
			var logger = new TestErrorLogger();
			var orgReader = new OrganisationDataObjectReader(addressDataObject, logger, Factory);
			AssertEquals("IsJobDocAddressMatchingOrgAddress() should return true as data object address override is off and E2_OA_Address matches OrgAddress PK despite E2_Contact not matching data object Contact", true, orgReader.IsJobDocAddressMatchingOrgAddress(orgAddress, jobDocAddress));
		}

		#region Implementation

		void SetupRightCodeAddressBOInDBForTesting()
		{
			OrganizationAddressTestHelper.SetUseUnmatchedOrganisationForMatchingRegistry(false);
			var addressDataObjectFotSetup = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
				Address1 = "Test Address 1",
				AddressShortCode = "THEMOMENT",
				AddressOverride = false,
				OrganizationCode = "RIGHTCODE",
			};

			var loggerForSetup = new TestErrorLogger();
			var readerForSetup = new OrganisationDataObjectReader(addressDataObjectFotSetup, loggerForSetup, Factory);
			var addressBOForSetup = readerForSetup.GetMatchedOrNewForTesting();

			AssertNotNull("Precondition: addressBOForSetup", addressBOForSetup);
			Factory.SaveForTesting();
			AssertEquals("Precondition: addressBOForSetup.IsInDatabase", true, addressBOForSetup.IsInDatabase);
		}

		OrganizationAddress GetOrganizationAddressFromXMLString(string xmlString, IXmlImportLogger logger)
		{
			var result = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			using (var memoryStream = (SubStreamableStream)new MemoryStream(System.Text.Encoding.Default.GetBytes(xmlString)))
			{
				new UniversalDataBuss.XmlIO.XmlReading.XmlReader().ReadXML(result, memoryStream, logger);
			}
			return result;
		}

		#region JobDocAddressDependentCollectionForTesting
		class JobDocAddressDependentCollectionForTesting : JobDocAddressDependentCollection
		{
			public JobDocAddressDependentCollectionForTesting(IDocAddresses parent)
			: base(parent)
			{
			}

			public new JobDocAddressForTesting this[int index]
			{
				get { return (JobDocAddressForTesting)base[index]; }
			}

			public new JobDocAddressForTesting AddNew()
			{
				return (JobDocAddressForTesting)base.AddNew();
			}

			public new JobDocAddressForTesting AddNew(DocAddressType docAddressType)
			{
				return (JobDocAddressForTesting)base.AddNew(docAddressType);
			}

			public new JobDocAddressForTesting AddNew(OrgAddress orgAddress)
			{
				return (JobDocAddressForTesting)base.AddNew(orgAddress);
			}

			public new JobDocAddressForTesting AddNew(DocAddressType docAddressType, int sequence)
			{
				return (JobDocAddressForTesting)base.AddNew(docAddressType, sequence);
			}

			public new JobDocAddressForTesting AddNew(OrgAddress orgAddress, DocAddressType docAddressType)
			{
				return (JobDocAddressForTesting)base.AddNew(orgAddress, docAddressType);
			}

			public new JobDocAddressForTesting CreateWithAddressType(DocAddressType docAddressType)
			{
				return (JobDocAddressForTesting)base.AddNew(docAddressType);
			}

			public new JobDocAddressForTesting CreateWithRequirement(JobDocAddressRequirement requirement)
			{
				return (JobDocAddressForTesting)base.CreateWithRequirement(requirement);
			}

			public new JobDocAddressForTesting FindByDocAddressType(DocAddressType docAddressType)
			{
				return (JobDocAddressForTesting)base.FindByDocAddressType(docAddressType);
			}

			public new JobDocAddressForTesting FindByDocAddressType(DocAddressType docAddressType, int sequence)
			{
				return (JobDocAddressForTesting)base.FindByDocAddressType(docAddressType, sequence);
			}

			public new JobDocAddressForTesting[] FindDocAddressesByType(DocAddressType docAddressType)
			{
				return new List<JobDocAddressForTesting>(new TypedEnumerable<JobDocAddressForTesting>(base.FindDocAddressesByType(docAddressType))).ToArray();
			}

			public JobDocAddressForTesting[] FindDocAddressesByType(DocAddressType docAddressType, JobDocAddressForTesting ignoreDocAddress)
			{
				return new List<JobDocAddressForTesting>(new TypedEnumerable<JobDocAddressForTesting>(base.FindDocAddressesByType(docAddressType, ignoreDocAddress))).ToArray();
			}

			public new JobDocAddressForTesting FindOrCreateDummyAddress(int sequence)
			{
				return (JobDocAddressForTesting)base.FindOrCreateDummyAddress(sequence);
			}

			public new JobDocAddressForTesting FindOrCreateWithDocAddressType(DocAddressType docAddressType)
			{
				return (JobDocAddressForTesting)base.FindOrCreateWithDocAddressType(docAddressType);
			}

			public new JobDocAddressForTesting FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType)
			{
				return (JobDocAddressForTesting)base.FindOrCreateWithDocAddressType(orgAddressPK, docAddressType);
			}

			public new JobDocAddressForTesting FindOrCreateWithRequirement(JobDocAddressRequirement requirement)
			{
				return (JobDocAddressForTesting)base.FindOrCreateWithRequirement(requirement);
			}

			public new JobDocAddressForTesting FindOrCreateWithRequirement(JobDocAddressRequirement requirement, int sequence)
			{
				return (JobDocAddressForTesting)base.FindOrCreateWithRequirement(requirement, sequence);
			}
		}
		#endregion

		public class JobDocAddressForTesting : JobDocAddress
		{
			public JobDocAddressForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row) { }

			public override bool SupportsDocAddressNumbers => true;
		}

		class JobDocAddressParentWithSupportsDocAddressNumbersIsTrueForTesting : JobDocAddressParentForTestingExtended
		{
			internal JobDocAddressParentWithSupportsDocAddressNumbersIsTrueForTesting(BusinessObjectFactory factory)
			  : base(factory)
			{
			}

			public override JobDocAddressDependentCollection DocAddresses
			{
				get
				{
					if (fDocAddresses == null)
					{
						fDocAddresses = new JobDocAddressDependentCollectionForTesting(this);
						fDocAddresses.Load();
					}
					return fDocAddresses;
				}
			}
			JobDocAddressDependentCollectionForTesting fDocAddresses;
		}

		class JobDocAddressParentForTestingExtended : JobDocAddressParentForTesting, IDocAddresses
		{
			internal JobDocAddressParentForTestingExtended(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
			{
				if (docAddress.DocAddressType == DocAddressType.CustomsDepotAddress)
				{
					return new DeniedSecurityCheckpoint();
				}

				return Environment.Env.Security.None;
			}

			IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => SupportedAddressTypesForTesting ?? new DocAddressType[]
					{
						DocAddressTypes.GetDocAddressTypeFromCode(Factory, DocAddressTypes.Codes.ConsignorDocumentaryAddress),
						DocAddressTypes.GetDocAddressTypeFromCode(Factory, DocAddressTypes.Codes.CustomsDepotAddress)
					};
			public DocAddressType[] SupportedAddressTypesForTesting;

			JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
			{
				var requirement = new JobDocAddressRequirement();
				requirement.GetRegistrationNumberResult = (JobDocAddress docAddress) => { return GetRegistrationNumber(docAddress, addressType); };
				return requirement;
			}

			RegistrationNumberResult GetRegistrationNumber(JobDocAddress docAddress, DocAddressType addressType)
			{
				var registrationNumber = GetRegistrationNumberForTesting == null ? new Business.RegistrationNumber() { Number = "55555", NumberType = "GST" } : GetRegistrationNumberForTesting(addressType);
				return new RegistrationNumberResult(docAddress.Factory, true, delegate
				{ return registrationNumber; });
			}

			public Func<DocAddressType, Business.RegistrationNumber> GetRegistrationNumberForTesting;
		}

		ZGuid MiscOrgAddressPK
		{
			get { return OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress; }
		}

		static void AssertJobDocAddressContentMatches_INTHEMSYD(JobDocAddress jobDocAddressBO)
		{
			CombineAssertions(delegate
			{
				AssertEquals("jobDocAddressBO.E2_Address1", "Unit 12, Level 3", jobDocAddressBO.E2_Address1);
				AssertEquals("jobDocAddressBO.E2_Address2", "233 Here St", jobDocAddressBO.E2_Address2);
				AssertEquals("jobDocAddressBO.E2_CompanyName", "In The Moment", jobDocAddressBO.E2_CompanyName);
				AssertEquals("jobDocAddressBO.E2_City", "ThereVille", jobDocAddressBO.E2_City);
				AssertEquals("jobDocAddressBO.E2_State", "OfBliss", jobDocAddressBO.E2_State);
				AssertEquals("jobDocAddressBO.E2_Postcode", "1233", jobDocAddressBO.E2_Postcode);
				AssertEquals("jobDocAddressBO.E2_RN_NKCountryCode", "AU", jobDocAddressBO.E2_RN_NKCountryCode);
				AssertEquals("jobDocAddressBO.E2_Contact", "Starshine Moonbeam", jobDocAddressBO.E2_Contact);
				AssertEquals("jobDocAddressBO.E2_Phone", "1239813209", jobDocAddressBO.E2_Phone);
				AssertEquals("jobDocAddressBO.E2_Fax", "234098234", jobDocAddressBO.E2_Fax);
				AssertEquals("jobDocAddressBO.E2_Email", "s.m@moment.com.au", jobDocAddressBO.E2_Email);
				AssertEquals("jobDocAddressBO.E2_Mobile", "234098293", jobDocAddressBO.E2_Mobile);
				AssertEquals("jobDocAddressBO.E2_GovRegNum", "55555", jobDocAddressBO.E2_GovRegNum);
				AssertEquals("jobDocAddressBO.E2_GovRegNumType", "GST", jobDocAddressBO.E2_GovRegNumType);
			});
		}

		static void AssertOrgHeaderContents(OrgHeader organizationBO)
		{
			AssertEquals("organizationBO.OH_RL_NKClosestPort", "AUMEL", organizationBO.OH_RL_NKClosestPort);
			AssertEquals("organizationBO.OH_Code", "INTHEMSYD", organizationBO.OH_Code);
			AssertEquals("organizationBO.OH_FullName", "In The Moment", organizationBO.OH_FullName);

			organizationBO.Contacts.Sort(OrgContact.Schema.OC_ContactName);
			var contacts = new List<string>();
			foreach (OrgContact contact in organizationBO.Contacts)
			{
				contacts.Add(string.Format("Name[{0}] Email[{1}] Phone[{2}] Fax[{3}] Mobile[{4}]", contact.OC_ContactName, contact.OC_Email, contact.OC_Phone, contact.OC_Fax, contact.OC_Mobile));
			}
			AssertMultilineASCIIEquals("organizationBO.Contacts", @"
Name[Starshine Moonbeam] Email[s.m@moment.com.au] Phone[1239813209] Fax[234098234] Mobile[234098293]
				".Trim(), string.Join("\r\n", contacts.ToArray()));

			organizationBO.CustomsCodes.Sort(OrgCusCode.Schema.OK_CodeType);
			var registrationNumbers = new List<string>();
			foreach (OrgCusCode customsCode in organizationBO.CustomsCodes)
			{
				registrationNumbers.Add(string.Format("Country[{0}] Type[{1}] Number[{2}] Address[{3}]", customsCode.OK_RN_NKCodeCountry, customsCode.OK_CodeType, customsCode.OK_CustomsRegNo, customsCode.PremisesAddress == null ? ZString.Empty : customsCode.PremisesAddress.OA_Address2));
			}
			AssertMultilineASCIIEquals("organizationBO.CustomsCodes", @"
Country[NZ] Type[ATF] Number[1234F] Address[233 Here St]
Country[AU] Type[GST] Number[55555] Address[]
Country[AU] Type[UNC] Number[545] Address[]
Country[AU] Type[UOC] Number[454] Address[]
				".Trim(), string.Join("\r\n", registrationNumbers.ToArray()));
		}

		static void AssertAddressContentMatches_INTHEMSYD(OrgAddress addressBO)
		{
			var organizationBO = addressBO.Header;
			CombineAssertions(delegate
			{
				AssertEquals("addressBO.OA_Address1", "Unit 12, Level 3", addressBO.OA_Address1);
				AssertEquals("addressBO.OA_Address2", "233 Here St", addressBO.OA_Address2);
				AssertEquals("addressBO.OA_City", "ThereVille", addressBO.OA_City);
				AssertEquals("addressBO.OA_State", "OfBliss", addressBO.OA_State);
				AssertEquals("addressBO.OA_PostCode", "1233", addressBO.OA_PostCode);
				AssertEquals("addressBO.OA_RL_NKRelatedPortCode", "AUMEL", addressBO.OA_RL_NKRelatedPortCode);
				AssertEquals("addressBO.OA_Phone", "1239813209", addressBO.OA_Phone);
				AssertEquals("addressBO.OA_Fax", "234098234", addressBO.OA_Fax);
				AssertEquals("addressBO.OA_Email", "s.m@moment.com.au", addressBO.OA_Email);
				AssertEquals("addressBO.OA_Mobile", "234098293", addressBO.OA_Mobile);
				AssertOrgHeaderContents(organizationBO);
			});
		}

		static OrganizationAddress GetNewAddressData_INTHEMSYD(ZString addressType)
		{
			var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = addressType,
				AddressShortCode = "THEMOMENT",
				AddressOverride = false,

				OrganizationCode = "INTHEMSYD",
				CompanyName = "In The Moment",
				Address1 = "Unit 12, Level 3",
				Address2 = "233 Here St",
				City = "ThereVille",
				State = "OfBliss",
				Postcode = "1233",
				Port = new UNLOCO { Code = "AUMel", Name = "Melbourne" },
				Country = new Country { Code = "au", Name = "Australia" },

				Contact = "Starshine Moonbeam",
				Email = "s.m@moment.com.au",
				Fax = "234098234",
				Mobile = "234098293",
				Phone = "1239813209",

				ScreeningStatus = new CodeDescriptionPair { Code = "Unk", Description = "Unknown" },

				GovRegNum = "55555",
				GovRegNumType = new RegistrationNumberType { Code = "GST", Description = "GST Code" },
				UniversalOfficeCode = "454",
				UniversalNettingCode = "545",
				AdditionalAddressInformation = "AAI"
			};

			addressData.SetRegistrationNumberCollection(() => new List<RegistrationNumber>(new[]
				{
					new RegistrationNumber
					{
						Type = new RegistrationNumberType { Code = "atf", Description = "Approved Transitional Facility" },
						CountryOfIssue = new Country { Code = "nz", Name = "New Zealand" },
						Value = "1234F",
					},
				}));
			return addressData;
		}

		(JobDocAddress jobDocAddress, OrgAddress orgAddress) GetNewJobDocAddressWithOrgAddress()
		{
			var jobDocAddress = Factory.BOFactory.New<JobDocAddress>();
			jobDocAddress.E2_AddressType = "CNE";
			var orgAddress = Factory.BOFactory.New<OrgAddress>();
			jobDocAddress.E2_OA_Address = orgAddress.PK;
			var orgHeader = Factory.BOFactory.New<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			PopulateOrgAddress_INTHEMSYD(orgAddress);

			return (jobDocAddress, orgAddress);
		}

		void PopulateOrgAddress_INTHEMSYD(OrgAddress orgAddress)
		{
			orgAddress.OA_Code = "THEMOMENT";
			orgAddress.OA_CompanyNameOverride = "In The Moment";
			orgAddress.OA_Address1 = "Unit 12, Level 3";
			orgAddress.OA_Address2 = "233 Here St";
			orgAddress.OA_City = "ThereVille";
			orgAddress.OA_State = "OfBliss";
			orgAddress.OA_PostCode = "1233";
			orgAddress.OA_RL_NKRelatedPortCode = "AUMel";
			orgAddress.OA_RN_NKCountryCode = "au";
			orgAddress.OA_Email = "s.m@moment.com.au";
			orgAddress.OA_Fax = "234098234";
			orgAddress.OA_Mobile = "234098293";
			orgAddress.OA_Phone = "1239813209";
		}

		JobDocAddress GetNewJobDocAddressWithOverride_INTHEMSYD()
		{
			var jobDocAddress = Factory.BOFactory.New<JobDocAddress>();
			jobDocAddress.E2_AddressType = "CNE";
			jobDocAddress.E2_AddressOverride = true;

			jobDocAddress.E2_CompanyName = "In The Moment";
			jobDocAddress.E2_Address1 = "Unit 12, Level 3";
			jobDocAddress.E2_Address2 = "233 Here St";
			jobDocAddress.E2_City = "ThereVille";
			jobDocAddress.E2_State = "OfBliss";
			jobDocAddress.E2_Postcode = "1233";
			jobDocAddress.E2_RN_NKCountryCode = "AU";
			jobDocAddress.E2_Email = "s.m@moment.com.au";
			jobDocAddress.E2_GovRegNum = "55555";
			jobDocAddress.E2_GovRegNumType = "GST";
			jobDocAddress.E2_Fax = "234098234";
			jobDocAddress.E2_Mobile = "234098293";
			jobDocAddress.E2_Phone = "1239813209";
			jobDocAddress.E2_Contact = "Starshine Moonbeam";
			jobDocAddress.E2_AdditionalAddressInformation = "AAI";

			return jobDocAddress;
		}

		#endregion
	}
}

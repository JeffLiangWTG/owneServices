using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Organisation.OrgImport.Testing;
namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class SimpleModuleDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestFlattenedProperties()
		{
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));
			var properties = processor.FlattenedProperties_Exposed.OrderBy(x => x.Name).Select(x => x.Name).ToArray();
			AssertCollectionContains("XX_HeaderString", properties);
			AssertCollectionContains("Child_ChildString", properties);
			AssertCollectionContains("HomeAddress_OH_Code", properties);
			AssertCollectionContains("HomeAddress_E2_Address1", properties);
		}

		public void TestGetHeader()
		{
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));
			var header = processor.GetHeader_Exposed("ZUBIN", x => x.XX_HeaderString);
			header.XX_HeaderString = "ZUBIN";
			AssertNotNull(header);

			var header2 = processor.GetHeader_Exposed("ZUBIN2", x => x.XX_HeaderString);
			AssertNotNull(header2);
			AssertNotEquals(header, header2);

			var header3 = processor.GetHeader_Exposed("ZUBIN", x => x.XX_HeaderString);
			AssertNotNull(header3);
			AssertEquals(header, header3);

			AssertEquals(true, header.HasBeenSetup);
		}

		public void TestCopyIdenticalProperties()
		{
			var flattened = new DummyFlattened();
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));
			var header = new DummyHeader();

			flattened.XX_HeaderDate = new ZDate(2015, 5, 7);
			flattened.XX_HeaderString = "Testing123";

			processor.CopyIdenticallyNamedProperties_Exposed(header, flattened, "XX");
			AssertEquals(new ZDate(2015, 5, 7), header.XX_HeaderDate);
			AssertEquals("Testing123", header.XX_HeaderString);
		}

		public void TestCopyIdenticalProperties_Child()
		{
			var flattened = new DummyFlattened();
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));
			var header = new DummyHeader();

			flattened.Child_ChildString = "Testing123";

			processor.CopyIdenticallyNamedProperties_Exposed(header, flattened, "Child", "Child");
			AssertEquals("Testing123", header.ChildString);
		}

		public void TestImport()
		{
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));
			var saveCalled = false;
			Factory.Saved += delegate
			{ saveCalled = true; };
			processor.Import();
			AssertEquals(true, saveCalled);
		}

		public void TestImport_DoNotSaveWhenCanceled()
		{
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));

			var saveCalled = false;
			Factory.Saved += delegate
			{ saveCalled = true; };
			processor.IsCanceled = true;
			processor.Import();
			AssertEquals(false, saveCalled);
		}

		public void TestGetAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ZUBIN";
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "ADDY1";
			address.OA_City = "Newington";

			var flattened = new DummyFlattened();
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));

			flattened.HomeAddress_OH_Code = "ZUBIN";
			flattened.HomeAddress_E2_Address1 = "ADDY1";
			flattened.HomeAddress_E2_City = "Newington";
			var result = processor.GetAddress_Exposed(flattened, "Home", false);
			AssertEquals(address, result);

			flattened.HomeAddress_E2_City = "Bella Vista";
			result = processor.GetAddress_Exposed(flattened, "Home", false);
			AssertNull(result);

			result = processor.GetAddress_Exposed(flattened, "Home", true);
			AssertEquals(orgHeader.MainAddress, result);
		}

		public void TestGetAddress_EmptyAddressFields()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ZUBIN";
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "ADDY1";
			address.OA_City = "Newington";

			var flattened = new DummyFlattened();
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));

			flattened.HomeAddress_OH_Code = "ZUBIN";
			var result = processor.GetAddress_Exposed(flattened, "Home", false);
			AssertEquals(orgHeader.MainAddress, result);

			flattened.HomeAddress_E2_City = "Bella Vista";
			result = processor.GetAddress_Exposed(flattened, "Home", false);
			AssertNull(result);

			result = processor.GetAddress_Exposed(flattened, "Home", true);
			AssertEquals(orgHeader.MainAddress, result);
		}

		public void TestGetAddress_LegacyCode()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "RAKHSH";

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "ZUBIN", ZString.Empty);

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "ADDY1";
			address.OA_City = "Newington";

			var flattened = new DummyFlattened();
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));

			flattened.HomeAddress_OH_Code = "ZUBIN";
			flattened.HomeAddress_E2_Address1 = "ADDY1";
			flattened.HomeAddress_E2_City = "Newington";
			var result = processor.GetAddress_Exposed(flattened, "Home", false);
			AssertEquals(address, result);

			flattened.HomeAddress_E2_City = "Bella Vista";
			result = processor.GetAddress_Exposed(flattened, "Home", false);
			AssertNull(result);

			result = processor.GetAddress_Exposed(flattened, "Home", true);
			AssertEquals(orgHeader.MainAddress, result);
		}

		public void TestSetJobDocAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ZUBIN";
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "ADDY1";
			address.OA_City = "Newington";

			var flattened = new DummyFlattened();
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));

			var dummyAddress = Factory.New<JobDocAddress>();
			flattened.HomeAddress_OH_Code = "ZUBIN";
			flattened.HomeAddress_E2_Address1 = "ADDY1";
			flattened.HomeAddress_E2_City = "Newington";
			processor.SetJobDocAddress_Exposed("Home", flattened, dummyAddress);
			AssertEquals(false, dummyAddress.E2_AddressOverride);
			AssertEquals(address.PK, dummyAddress.E2_OA_Address);

			dummyAddress = Factory.New<JobDocAddress>();
			flattened.HomeAddress_E2_City = "Bella Vista";
			processor.SetJobDocAddress_Exposed("Home", flattened, dummyAddress);
			AssertEquals(true, dummyAddress.E2_AddressOverride);
			AssertEquals("ADDY1", dummyAddress.E2_Address1);
			AssertEquals("Bella Vista", dummyAddress.E2_City);
		}

		public void TestSetJobDocAddress_EmptyAddress()
		{
			var flattened = new DummyFlattened();
			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));

			var dummyAddress = Factory.New<JobDocAddress>();
			processor.SetJobDocAddress_Exposed("Home", flattened, dummyAddress);
			AssertEquals(false, dummyAddress.E2_AddressOverride);
			AssertEquals(true, dummyAddress.IsEmpty);
			AssertEquals(ZString.Empty, dummyAddress.E2_Address1);
			AssertEquals(ZString.Empty, dummyAddress.E2_City);
		}

		public void TestSetOrgReference()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ZUBIN";

			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));

			OrgHeader testValue = null;
			processor.SetOrgReference_Exposed("ZUBIN", x => testValue = x);
			AssertEquals(orgHeader, testValue);
		}

		public void TestSetOrgReference_LegacyCode_CountrySpecific()
		{
			AssertOrgReference(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public void TestSetOrgReference_LegacyCode_BlankCountry()
		{
			AssertOrgReference(ZString.Empty);
		}

		void AssertOrgReference(string countryCodeForValidLegacyCode)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ALEERA";

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "RYLAN", "IN");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "ZUBIN", countryCodeForValidLegacyCode);

			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));

			OrgHeader testValue = null;
			processor.SetOrgReference_Exposed("ZUBIN", x => testValue = x);
			AssertEquals(orgHeader, testValue);
		}

		public void TestSetOrgReference_NoMatch()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ALEERA";

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "ZAYDEN", "IN");

			var collection = new DummyHeaderCollection(Factory);
			var processor = new SimpleModuleDataTransferProcessorForTest(collection, new ImportCollectionInfoForDummyFlattened(new DummyFlattenedCollection(Factory)));

			OrgHeader testValue = null;
			processor.SetOrgReference_Exposed("ZUBIN", x => testValue = x);
			AssertNull(testValue);
		}
	}
}

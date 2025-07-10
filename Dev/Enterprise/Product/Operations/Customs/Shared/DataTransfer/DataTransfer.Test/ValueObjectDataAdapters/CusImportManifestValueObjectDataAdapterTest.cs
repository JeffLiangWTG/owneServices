using System.IO;
using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(CusImportManifestValueObjectDataAdapter))]
	sealed class CusImportManifestValueObjectDataAdapterTest : Enterprise.DataTransfer.Xml.Testing.ValueObjectDataAdapterTest<CusSeaManTranHead, Xsd.CusImportManifest>
	{
		public void TestImportFromXmlFile()
		{
			var serializer = new XmlValueObjectSerializer(typeof(Xsd.CusImportManifests));
			var pathToXmlDocSample = TestFileHelper.GetPathForTesting("PopulatedImportManifest.xml");

			var exampleDoc = new XmlDocument();

			try
			{
				using (var reader = new StreamReader(pathToXmlDocSample))
				{
					exampleDoc.Load(reader);
				}
			}
			catch (XmlException)
			{
				throw new AssertionFailedError("Could not load xml document. it may be malformed.");
			}

			var reader1 = new XmlNodeReader(exampleDoc);
			var values = (IValueObject)serializer.Deserialize(reader1);

			Assert(values is Xsd.CusImportManifests);

			var xmlImportManifests = (Xsd.CusImportManifests)values;

			var dataAdapter = new CusImportManifestValueObjectDataAdapter();
			var tranHead = GetEmptyTranHead();

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			dataAdapter.ImportFromValueObject(tranHead, xmlImportManifests.CusImportManifest[0], context);

			AssertEquals("Voyage Number", "12345", tranHead.BT_VoyageNum);
			AssertEquals("Vessel", "ADMIRALENGRACHT", tranHead.BT_VesselName);

			AssertEquals("OceanBills", 1, tranHead.OceanBills.Count);
			var oceanBill = tranHead.OceanBills[0];

			AssertEquals("OceanBill Number", "OBL100", oceanBill.BO_OceanBill);

			AssertEquals("Details", 2, oceanBill.Details.Count);
			var detail = oceanBill.Details[0];
			var detail2 = oceanBill.Details[1];

			AssertEquals("Container Number", "AAAA1111115", detail.BD_ContainerNumber);
			AssertEquals("FCL", detail.BD_LineCargoType);
			AssertEquals("0987654321A123456789", detail.BD_SealNo);

			AssertEquals("Container Number", ZString.Empty, detail2.BD_ContainerNumber);
			AssertEquals("B/B", detail2.BD_LineCargoType);
			AssertEquals(ZString.Empty, detail2.BD_ContainerSizeOrISOCode);
			AssertEquals(ZString.Empty, detail2.BD_TypeOfContainer);
		}

		public void TestGetVesselCode()
		{
			var dataAdapter = new CusImportManifestValueObjectDataAdapter();
			var notify = new NotificationBuffer();
			var importContext = new ValueObjectImportContext(Factory, notify);

			var vessel = RefVessel.New(Factory);
			vessel.RV_Code = "TESTVESSEL";
			vessel.RV_LloydsNumber = "12345";

			AssertEquals("TESTVESSEL", dataAdapter.GetVesselCode(Factory, "TESTVESSEL", ZString.Empty, importContext));
			AssertEquals("TESTVESSEL", dataAdapter.GetVesselCode(Factory, ZString.Empty, "12345", importContext));
			AssertEquals("TESTVESSEL", dataAdapter.GetVesselCode(Factory, "TESTVESSEL", "12345", importContext));

			var newVesselCode = dataAdapter.GetVesselCode(Factory, "FOO", "321", importContext);
			AssertEquals("FOO", newVesselCode);

			var newVessel = RefVessel.LookupVesselByCode(newVesselCode, Factory);
			AssertEquals(false, newVessel.IsInDatabase);
			AssertEquals("FOO", newVessel.RV_Code);
			AssertEquals("321", newVessel.RV_LloydsNumber);
		}

		public void TestXmlOrgHasAddress()
		{
			var org = new Xsd.Organisation();
			var dataAdapter = new CusImportManifestValueObjectDataAdapter();
			AssertEquals(false, dataAdapter.XmlOrgHasAddresses(org));

			var address = org.OrganisationDetails.Addresses.AddNew();
			AssertEquals(false, dataAdapter.XmlOrgHasAddresses(org));

			address.AddressLine1 = "FOO";
			AssertEquals(true, dataAdapter.XmlOrgHasAddresses(org));

			address.AddressLine1 = ZString.Empty;
			address.AddressLine2 = "BAR";
			AssertEquals(true, dataAdapter.XmlOrgHasAddresses(org));
		}

		public void TestNoOrgMatching()
		{ // W00041171
			var org = new Xsd.Organisation();
			var dataAdapter = new CusImportManifestValueObjectDataAdapter(false);

			var xmlBills = new Xsd.CusImportManifestOceanBillCollection();
			var xmlBill = xmlBills.AddNew();

			xmlBill.Consignee.OrganisationDetails.Name = "foo";
			xmlBill.Consignee.OrganisationDetails.Addresses.AddNew();
			xmlBill.Consignee.OrganisationDetails.Addresses.GetMainAddress().AddressLine1 = "bar";
			xmlBill.Consignee.OrganisationDetails.Addresses.GetMainAddress().AddressLine2 = "spam";
			xmlBill.Consignee.OrganisationDetails.Addresses.GetMainAddress().CityOrSuburb = "bleh";
			xmlBill.Consignee.OrganisationDetails.Addresses.GetMainAddress().PostCode = "bloh";
			xmlBill.Consignee.OrganisationDetails.Addresses.GetMainAddress().Location.Country = "AU";

			xmlBill.Consignor.OrganisationDetails.Name = "foo2";
			xmlBill.Consignor.OrganisationDetails.Addresses.AddNew();
			xmlBill.Consignor.OrganisationDetails.Addresses.GetMainAddress().AddressLine1 = "bar2";
			xmlBill.Consignor.OrganisationDetails.Addresses.GetMainAddress().AddressLine2 = "spam2";
			xmlBill.Consignor.OrganisationDetails.Addresses.GetMainAddress().CityOrSuburb = "bleh2";
			xmlBill.Consignor.OrganisationDetails.Addresses.GetMainAddress().PostCode = "bloh2";
			xmlBill.Consignor.OrganisationDetails.Addresses.GetMainAddress().Location.Country = "NZ";

			var header = Factory.New<CusSeaManTranHead>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			dataAdapter.SetOceanBillDetails(header.OceanBills, xmlBills, context);

			AssertEquals(1, header.OceanBills.Count);

			var bill = header.OceanBills[0];

			AssertNull(bill.Consignee);
			AssertNull(bill.Consignor);

			AssertEquals(ZGuid.Empty, bill.BO_OH_Consignee);
			AssertEquals(ZGuid.Empty, bill.BO_OH_Consignor);

			AssertEquals("foo", bill.BO_ConsigneeName);
			AssertEquals("bar", bill.BO_ConsigneeAddress1);
			AssertEquals("spam", bill.BO_ConsigneeAddress2);
			AssertEquals("bleh", bill.BO_ConsigneeCity);
			AssertEquals("bloh", bill.BO_ConsigneePostCode);
			AssertEquals("AU", bill.BO_RN_NKConsigneeCountryCode);

			AssertEquals("foo2", bill.BO_ConsignorName);
			AssertEquals("bar2", bill.BO_ConsignorAddress1);
			AssertEquals("spam2", bill.BO_ConsignorAddress2);
			AssertEquals("bleh2", bill.BO_ConsignorCity);
			AssertEquals("bloh2", bill.BO_ConsignorPostCode);
			AssertEquals("NZ", bill.BO_RN_NKConsignorCountryCode);
		}

		public void TestPortsMatchUNLOCOBeforePortName()
		{
			var org = new Xsd.Organisation();
			var dataAdapter = new CusImportManifestValueObjectDataAdapter(false);

			var xmlBills = new Xsd.CusImportManifestOceanBillCollection();
			var xmlBill = xmlBills.AddNew();

			xmlBill.PortOfOrigin.Port.Value = "CAVAN";
			xmlBill.PortOfLoading.Port.Value = "CAVAN";
			xmlBill.PortOfDischarge.Port.Value = "CAVAN";
			xmlBill.PortOfDestination.Port.Value = "CAVAN";

			var header = Factory.New<CusSeaManTranHead>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			dataAdapter.SetOceanBillDetails(header.OceanBills, xmlBills, context);

			AssertEquals(1, header.OceanBills.Count);

			var bill = header.OceanBills[0];

			AssertEquals("CAVAN", bill.BO_RL_NKOriginPort);
			AssertEquals("CAVAN", bill.BO_RL_NKLoadPort);
			AssertEquals("CAVAN", bill.BO_RL_NKDestinationPort);
			AssertEquals("CAVAN", bill.BO_RL_NKDischargePort);
		}

		protected override string ExpectedRootCollectionElementName => "CusImportManifests";

		protected override string ExpectedRootElementName => "CusImportManifest";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyTranHead(), TestFileHelper.GetPathForTesting("EmptyImportManifest.xml"), ValidationKind.Xsd, "Empty Import Manifest");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetTranHeadWithTestData(), TestFileHelper.GetPathForTesting("PopulatedImportManifest.xml"), ValidationKind.Xsd, "Empty Import Manifest");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override ValueObjectDataAdapter<CusSeaManTranHead, Xsd.CusImportManifest> GetNewBizObjXmlDataAdapter() => new CusImportManifestValueObjectDataAdapter();

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override bool IsExportToCollectionSupported => false;

		protected override bool IsExportToValueObjectSupported => false;

		CusSeaManTranHead GetEmptyTranHead() => Factory.New<CusSeaManTranHead>();

		CusSeaManTranHead GetTranHeadWithTestData()
		{
			var tranHead = Factory.New<CusSeaManTranHead>();
			tranHead.BT_PortOfLastForeignPortATD = new ZDateTime(2005, 09, 20);
			tranHead.BT_RL_NKPortOfLastForeignPort = "NZAKL";
			tranHead.BT_VesselName = "ADMIRALENGRACHT";
			tranHead.BT_VoyageNum = "12345";

			var port = tranHead.Arrivals.AddNew();
			port.BA_IsFirstArrival = true;
			port.BA_ArrivalPortETA = new ZDateTime(2005, 09, 21);
			port.BA_BerthCode = "54321";
			port.BA_CTOEstablishmentID = "07896";
			port.BA_RL_NKArrivalPort = "AUSYD";
			port.BA_StevadoreID = "12345698701";

			var header = tranHead.OceanBills.AddNew();
			header.BO_OceanBill = "ABC1234567";
			header.BO_RN_NKGoodsCountryOfOrigin = "NZ";
			header.BO_PaymentMethod = "CC";
			header.BO_RL_NKLoadPort = "NZAKL";
			header.BO_RL_NKOriginPort = "NZAKL";
			header.BO_RL_NKDestinationPort = "AUSYD";
			header.BO_RL_NKDischargePort = "AUSYD";
			header.BO_HeaderCargoType = "I";

			header.BO_ConsigneeName = "Foo People";
			header.BO_RN_NKConsigneeCountryCode = "AU";
			header.BO_ConsignorCity = "Foosville";
			header.BO_ConsigneeAddress1 = "335 Foo Street";
			header.BO_ConsigneePostCode = "5532";

			header.BO_ConsigneeName = "Bar People";
			header.BO_RN_NKConsigneeCountryCode = "NZ";
			header.BO_ConsignorCity = "Barsland";
			header.BO_ConsigneeAddress1 = "669 Bar Avenue";
			header.BO_ConsigneePostCode = "9962";

			var detail = header.Details.AddNew();
			detail.BD_ContainerNumber = "BBOR2204927";
			detail.BD_LineCargoType = "FCL";
			detail.BD_TypeOfContainer = "GEN";
			detail.BD_ContainerSizeOrISOCode = "2008";
			detail.BD_NoOfPacks = 150;
			detail.BD_PackType = "XB";
			detail.BD_GrossWeight = 1000;
			detail.BD_GrossWeightUM = "KG";
			detail.BD_CargoVolume = 4;
			detail.BD_CargoVolumeUM = "CU";
			detail.BD_GoodsDescription = "stuff";
			detail.BD_HazardousIndicator = true;

			var slotOrg = tranHead.SlotCharterers.AddNew();
			slotOrg.BS_SlotCharterID = "93948532091";

			return tranHead;
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}

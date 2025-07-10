using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	sealed class OGADataTransferToolTest : TestCaseWithFactory
	{
		public void TestExportOGADetails()
		{
			JobComInvoiceLine invoiceLine = GetInvoiceLine();
			DOT dot = invoiceLine.DOTs.AddNew();
			DOT dot1 = invoiceLine.DOTs.AddNew();
			GetDotsDetails(dot, dot1);
			Xsd.USDOTCollection xmlDots = OGADataTransferTool.ExportDOTsDetails(invoiceLine.DOTs);
			AssertXmlDotDetails(xmlDots[0]);
			FCC fcc = invoiceLine.FCCs.AddNew();
			GetFCCDetails(fcc);
			Xsd.USFCCCollection xmlFCCs = OGADataTransferTool.ExportFCCsDetails(invoiceLine.FCCs);
			AssertXmlFCCDetails(xmlFCCs[0]);
			FDA fda = invoiceLine.FDAs.AddNew();
			GetFDADetails(fda, TestOrganization.Addresses[2].PK, TestOrganization.PK);
			Xsd.USFDACollection xmlFDAs = OGADataTransferTool.ExportFDAsDetails(invoiceLine.FDAs, new ValueObjectExportContext(Notification));
			AssertXmlFDADetails(xmlFDAs[0]);
			JobComInvoiceLine invoiceLine1 = GetInvoiceLineWithContainers();
			Business.Bill bill = invoiceLine1.Declaration.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.CU_BillNum = "BG456";
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			fda = invoiceLine1.FDAs.AddNew();
			fda.ContainersForInvoiceLine.FindByContainerNumber("MAEUHHHHHH").IsForFDALine = true;
			fda.ContainersForInvoiceLine.FindByContainerNumber("MAEUVVVVV").IsForFDALine = false;
			fda.BillsAvailable[0].IsForFDALine = true;
			xmlFDAs = OGADataTransferTool.ExportFDAsDetails(invoiceLine1.FDAs, new ValueObjectExportContext(Notification));
			AssertEquals("MAEUHHHHHH", xmlFDAs[0].Containers[0].ContainerNumber);
			AssertEquals("BG456", xmlFDAs[0].Bills[0].MasterBill);
		}

		public void TestExportOrganisation()
		{
			Xsd.USManufacturer xmlManufacturer = new Xsd.USManufacturer();
			var address = TestOrganization.Addresses[1];
			var context = new ValueObjectExportContext(Notification);
			context.SimplifiedXML = false;
			OGADataTransferTool.ExportOrganisation(xmlManufacturer, address, context);
			var organisationDetails = ((Xsd.Organisation)xmlManufacturer.Item).OrganisationDetails;
			AssertEquals("Test Org", organisationDetails.Name);
			var xmlAddress = organisationDetails.Addresses.OfType<Xsd.OrgAddress>().FirstOrDefault(x => x.Sequence == 1);
			AssertEquals("Sequence 1 AddressLine1", address.OA_Address1, xmlAddress.AddressLine1);
			AssertEquals("Sequence 1 AddressLine2", address.OA_Address2, xmlAddress.AddressLine2);
		}

		public void TestExportLaceyActData()
		{
			var invoiceLine = GetInvoiceLine();
			var pGAs = invoiceLine.LaceyActLines;
			var pga = pGAs.AddNew();
			GetLaceyActDetals(pga);
			var xmlPGAs = OGADataTransferTool.ExportLaceyActData(false, pGAs);
			AssertXmlLaceyActDetails(xmlPGAs[0]);
			AssertEquals(0, xmlPGAs[0].Containers.Count);
			var invoiceLine2 = GetInvoiceLineWithContainers();
			xmlPGAs = OGADataTransferTool.ExportLaceyActData(true, invoiceLine2.LaceyActLines);
			AssertEquals("MAEUVVVVV", xmlPGAs[0].Containers[0].ContainerNumber);
		}

		public void TestImportLaceyActDataForProduct()
		{
			var invoiceLine = GetInvoiceLine();
			var pGAs = invoiceLine.LaceyActLines;
			var pga = pGAs.AddNew();
			GetLaceyActDetals(pga);
			var xmlPGAs = new Xsd.USLaceyActCollection();
			OGADataTransferTool.ImportLaceyActDetails(xmlPGAs, pGAs, Factory, false, null);
			AssertLaceyActDetails(pGAs[0]);
			AssertEquals(0, pGAs[0].ContainersForPGALine.Count);
		}

		public void TestImportLaceyActDataForInvoiceLine()
		{
			var invoiceLine = GetInvoiceLineWithContainers();
			var xmlPGAs = new Xsd.USLaceyActCollection();
			OGADataTransferTool.ImportLaceyActDetails(xmlPGAs, invoiceLine.LaceyActLines, Factory, false, invoiceLine.ContainersForInvoiceLinesForBindingOnly);
			AssertLaceyActDetails(invoiceLine.LaceyActLines[0]);
			AssertEquals("MAEUVVVVV", ((IContainerNumber)invoiceLine.LaceyActLines[0].ContainersForPGALine[0]).ContainerEquipmentID);
		}

		public void TestImport_FDAShipperCanBeFoundByMID()
		{
			JobComInvoiceLine invoiceLine = GetInvoiceLine();
			FDA fda = invoiceLine.FDAs.AddNew();
			GetFDADetails(fda, TestOrganization.Addresses[2].PK, TestOrganization.PK);
			Xsd.USFDACollection xmlFDAs = OGADataTransferTool.ExportFDAsDetails(invoiceLine.FDAs, new ValueObjectExportContext(Notification));
			xmlFDAs[0].Shipper.Item = "SIBEREQU6LON";
			invoiceLine.FDAs.RemoveAndDeleteAll();
			OGADataTransferTool.ImportFDADetails(xmlFDAs, invoiceLine.FDAs, new ValueObjectImportContext(Factory, Notification));
			OrgHeader supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "Test Shipper"));
			OrgAddress[] addresses = (OrgAddress[])supplier.Addresses.Find(new ZQuery(OrgAddressSchema.OA_Address1, "Shipper Test 2"));
			AssertEquals(1, invoiceLine.FDAs.Count);
			AssertEquals(addresses[0].PK, invoiceLine.FDAs[0].US_FDAShipperAddress);
			Assert(!Notification.AsString.Contains(OGADataTransferTool.ValueFieldIsObseleteWarning));
		}

		public void TestValueFieldIsReplacedByInvValue()
		{
			var invoiceLine = GetInvoiceLine();
			var fda = invoiceLine.FDAs.AddNew();
			GetFDADetails(fda, TestOrganization.Addresses[2].PK, TestOrganization.PK);
			Xsd.USFDACollection xmlFDAs = OGADataTransferTool.ExportFDAsDetails(invoiceLine.FDAs, new ValueObjectExportContext(Notification));
			AssertEquals(180m, xmlFDAs[0].InvValue);
			xmlFDAs[0].Value = 190m;
			invoiceLine.FDAs.RemoveAndDeleteAll();
			OGADataTransferTool.ImportFDADetails(xmlFDAs, invoiceLine.FDAs, new ValueObjectImportContext(Factory, Notification));
			Assert(Notification.AsString.Contains(OGADataTransferTool.ValueFieldIsObseleteWarning));
			AssertEquals(180m, invoiceLine.FDAs[0].US_InvCurrFDAValue);
		}

		internal static void GetDotsDetails(DOT dot, DOT dot1)
		{
			dot.US_DOTBoxNo = "01";
			dot.US_DOTClarCode = "2";
			dot.US_DOTCommercialDesc = "comm descr";
			dot.US_DOTCountryOfOrigin = "AD";
			dot.US_DOTImpSubstStatement = true;
			dot.US_DOTPassport = "456";
			dot.US_DOTPriorApproval = false;
			dot.US_DOTBondSuretyCode = "891";
			dot.US_DOTTireBrandName = "Brand";
			dot.US_DOTTireID = "ID";
			var vehicleDetail = dot.DOTVINs.AddNew();
			vehicleDetail.US_DOTVEN = "1234";
			vehicleDetail.US_DOTMake = "make";
			vehicleDetail.US_DOTModel = "model";
			vehicleDetail.US_DOTRINo = "4573";
			vehicleDetail.US_DOTVIN = "10";
			vehicleDetail.US_DOTYear = 98;
			dot1.US_DOTBoxNo = "01";
			dot1.US_DOTClarCode = "2";
			dot1.US_DOTCommercialDesc = "comm descr";
			dot1.US_DOTCountryOfOrigin = "AD";
			dot1.US_DOTImpSubstStatement = true;
			dot1.US_DOTPassport = "456";
			dot1.US_DOTPriorApproval = false;
			dot1.US_DOTBondSuretyCode = "891";
			dot1.US_DOTTireBrandName = "Brand";
			dot1.US_DOTTireID = "ID";
		}

		internal static void GetFCCDetails(FCC fcc)
		{
			fcc.US_FCCCommercialDesc = "comm descr";
			fcc.US_FCCID = "21";
			fcc.US_FCCImpCondNo = "No";
			fcc.US_FCCImpCondNoQtyAppr = false;
			fcc.US_FCCModel = "Model";
			fcc.US_FCCQty = 151;
			fcc.US_FCCTradeName = "Trade Name";
			fcc.US_FCCWithhold = true;
		}

		internal static void GetFDADetails(FDA fda, ZGuid orgAddressPK, ZGuid orgPK)
		{
			fda.AffirmationCodes.AddNew(AffirmationCodeConstants.Codes.CCN, "CN");
			fda.US_TradeBrandName = "Trade Brand Name";
			fda.US_FDACargoStorageCode = "A";
			fda.US_FDACommercialDesc = "MALE HORSES, PUREBRED BREE";
			fda.US_UC_NKFDAProduction = "IT";
			fda.US_DimUQ = FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals;
			fda.US_ContainerDim1 = 10m;
			fda.US_ContainerDim2 = 20m;
			fda.US_ContainerDim3 = 30m;
			fda.US_FDAContainerDimType = "C";
			fda.US_FDAManufacturerAddress = orgAddressPK;
			fda.US_FDAProductCode = "12AAB01";
			fda.US_FDAQty1 = 130m;
			fda.US_FDAMeasure1 = "BOL";
			fda.US_FDAQty2 = 131m;
			fda.US_FDAMeasure2 = "AE";
			fda.US_FDAQty3 = 132m;
			fda.US_FDAMeasure3 = "AM";
			fda.US_FDAQty4 = 133m;
			fda.US_FDAMeasure4 = "AP";
			fda.US_FDAQty5 = 135m;
			fda.US_FDAMeasure5 = "BG";
			fda.US_FDAQty6 = 136m;
			fda.US_FDAMeasure6 = "BF";
			fda.US_FDAValue = 141m;
			fda.US_PFR = "food reg";
			fda.US_FME = FDAPriorNoticeExemptCodeList.Codes.M;
			fda.US_PFT = ProducerFirmTypeList.Codes.G;
			fda.US_CSH = "GT";
			fda.US_OFT = OwnerFirmTypeList.Codes.Carrier;
			fda.US_InvCurrFDAValue = 180m;
			var shipper = fda.Factory.New<OrgHeader>();
			shipper.FillWithValidTestData();
			shipper.OH_FullName = "Test Shipper";
			shipper.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ShipperRegistrationNumber, "7845", fda.Factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedStates));
			var address1 = shipper.Addresses.AddNew();
			address1.OA_Address1 = "Shipper Test 1";
			var address2 = shipper.Addresses.AddNew();
			address2.OA_Address1 = "Shipper Test 2";
			address2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "SIBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			var address3 = shipper.Addresses.AddNew();
			address3.OA_Address1 = "Shipper Test 3";
			fda.US_FDAShipperAddress = shipper.Addresses[0].PK;
			fda.US_OA_FDAFEI = orgAddressPK;
			fda.FDAFEIAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, "004568987521", Core.Constants.CountryCodes.UnitedStates);
			fda.Factory.Save();
		}

		internal static void GetLaceyActDetals(PGA pga)
		{
			pga.US_PGACommercialDescription = "commerc description";
			pga.US_PGALineValue = 600m;
			var element = pga.PG04ConstituentElements.AddNew();
			element.US_PGANameOfTheConstituentElement = "name 1";
			element.US_PGAPercentOfConstituentElement = 10.8m;
			element.US_PGAQuantityOfConstituentElement = 100m;
			element.US_PGAUnitOfMeasure = "DOZ";
			var scientificData = element.ScientificDataCollection.AddNew();
			scientificData.US_PGACountryCode = "PH";
			scientificData.US_PGAScientificGenusName = "Pine";
			scientificData.US_PGAScientificSpeciesName = "Species Name";
		}

		internal static void AssertLaceyActDetails(PGA pga)
		{
			AssertEquals("commerc description", pga.US_PGACommercialDescription);
			AssertEquals("name 1", pga.PG04ConstituentElements[0].US_PGANameOfTheConstituentElement);
			AssertEquals(10.8m, pga.PG04ConstituentElements[0].US_PGAPercentOfConstituentElement);
			AssertEquals(100m, pga.PG04ConstituentElements[0].US_PGAQuantityOfConstituentElement);
			AssertEquals("DOZ", pga.PG04ConstituentElements[0].US_PGAUnitOfMeasure);
			AssertEquals("PH", pga.PG04ConstituentElements[0].ScientificDataCollection[0].US_PGACountryCode);
			AssertEquals("Pine", pga.PG04ConstituentElements[0].ScientificDataCollection[0].US_PGAScientificGenusName);
			AssertEquals("Species Name", pga.PG04ConstituentElements[0].ScientificDataCollection[0].US_PGAScientificSpeciesName);
		}

		internal static void AssertXmlDotDetails(Xsd.USDOT xmlDOT)
		{
			AssertEquals("01", xmlDOT.BoxNumber);
			AssertEquals("2", xmlDOT.Clarification);
			AssertEquals("comm descr", xmlDOT.CommercialDesc);
			AssertEquals("AD", xmlDOT.CountryOfOrigin);
			AssertEquals(true, xmlDOT.ImporterStatement);
			AssertEquals("456", xmlDOT.PassportNo);
			AssertEquals(false, xmlDOT.PriorApproved);
			AssertEquals("891", xmlDOT.SuretyCode);
			AssertEquals("Brand", xmlDOT.Tire.BrandName);
			AssertEquals("ID", xmlDOT.Tire.ID);
			AssertEquals("1234", xmlDOT.VehicleDetails[0].EligibilityNo);
			AssertEquals("make", xmlDOT.VehicleDetails[0].Make);
			AssertEquals("model", xmlDOT.VehicleDetails[0].Model);
			AssertEquals("4573", xmlDOT.VehicleDetails[0].NHTSANumber);
			AssertEquals("10", xmlDOT.VehicleDetails[0].VIN);
			AssertEquals(98, xmlDOT.VehicleDetails[0].Year);
		}

		internal static void AssertXmlFCCDetails(Xsd.USFCC xmlFCC)
		{
			AssertEquals("comm descr", xmlFCC.CommercialDesc);
			AssertEquals("21", xmlFCC.ID);
			AssertEquals("No", xmlFCC.ImportConditionNo);
			AssertEquals(false, xmlFCC.ImportConditionQtyApproved);
			AssertEquals("Model", xmlFCC.Model);
			AssertEquals(151m, xmlFCC.Qty);
			AssertEquals("Trade Name", xmlFCC.TradeName);
			AssertEquals(true, xmlFCC.Withhold);
		}

		internal static void AssertXmlFDADetails(Xsd.USFDA xmlFDA)
		{
			AssertEquals("CCN", xmlFDA.AffirmationCodes[0].Code);
			AssertEquals("CN", xmlFDA.AffirmationCodes[0].Value);
			AssertEquals("Trade Brand Name", xmlFDA.BrandName);
			AssertEquals("A", xmlFDA.CargoStorageCode);
			AssertEquals("MALE HORSES, PUREBRED BREE", xmlFDA.CommercialDesc);
			AssertEquals("IT", xmlFDA.CountryOfProduction);
			AssertEquals(10m, xmlFDA.InnermostContainer.Dimension1);
			AssertEquals(20m, xmlFDA.InnermostContainer.Dimension2);
			AssertEquals(30m, xmlFDA.InnermostContainer.Dimension3);
			AssertEquals("C", xmlFDA.InnermostContainer.Shape);
			AssertEquals("12AAB01", xmlFDA.ProductCode);
			AssertEquals(130m, xmlFDA.Quantities.Quantity1.Value);
			AssertEquals("BOL", xmlFDA.Quantities.Quantity1.DimensionType);
			AssertEquals(131m, xmlFDA.Quantities.Quantity2.Value);
			AssertEquals("AE", xmlFDA.Quantities.Quantity2.DimensionType);
			AssertEquals(132m, xmlFDA.Quantities.Quantity3.Value);
			AssertEquals("AM", xmlFDA.Quantities.Quantity3.DimensionType);
			AssertEquals(133m, xmlFDA.Quantities.Quantity4.Value);
			AssertEquals("AP", xmlFDA.Quantities.Quantity4.DimensionType);
			AssertEquals(135m, xmlFDA.Quantities.Quantity5.Value);
			AssertEquals("BG", xmlFDA.Quantities.Quantity5.DimensionType);
			AssertEquals(136m, xmlFDA.Quantities.Quantity6.Value);
			AssertEquals("BF", xmlFDA.Quantities.Quantity6.DimensionType);
			AssertEquals("food reg", xmlFDA.ManufacturerProducerData.FoodFacilityRegistrationNumber);
			AssertEquals(Xsd.USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.M, xmlFDA.ManufacturerProducerData.FoodFacilityRegistrationExemption);
			AssertEquals(Xsd.USManufacturerProducerDataTypeProducerFirmType.G, xmlFDA.ManufacturerProducerData.ProducerFirmType);
			AssertEquals("GT", xmlFDA.CountryOfShipping);
			AssertEquals(Xsd.USFDAOwnerFirmType.C, xmlFDA.OwnerFirmType);
			AssertEquals("7845", xmlFDA.ShipperRegistrationNumber);
			AssertEquals("Test Org", ((Xsd.Organisation)xmlFDA.EstablismentID.Item).OrganisationDetails.Name);
			AssertEquals("Test Shipper", ((Xsd.Organisation)xmlFDA.Shipper.Item).OrganisationDetails.Name);
		}

		internal static void AssertXmlLaceyActDetails(Xsd.USLaceyAct laceyActData)
		{
			AssertEquals("commerc description", laceyActData.CommercialDescription);
			AssertEquals("name 1", laceyActData.ConstituentElements[0].Name);
			AssertEquals(10.8m, laceyActData.ConstituentElements[0].Percent);
			AssertEquals(100m, laceyActData.ConstituentElements[0].Quantity);
			AssertEquals("DOZ", laceyActData.ConstituentElements[0].UOM);
			AssertEquals("PH", laceyActData.ConstituentElements[0].ScientificData[0].Country);
			AssertEquals("Pine", laceyActData.ConstituentElements[0].ScientificData[0].GenusName);
			AssertEquals("Species Name", laceyActData.ConstituentElements[0].ScientificData[0].SpeciesName);
		}

		JobComInvoiceLine GetInvoiceLineWithContainers()
		{
			var invoiceLine = GetInvoiceLine();
			AddContainerForInvoiceLine(invoiceLine, "MAEUHHHHHH", true);
			AddContainerForInvoiceLine(invoiceLine, "MAEUVVVVV", true);
			AddContainerForInvoiceLine(invoiceLine, "MAEUKKKKKK", false);
			var pga = invoiceLine.LaceyActLines.AddNew();
			GetLaceyActDetals(pga);
			AssociateContainerWithPGALine(pga, "MAEUVVVVV");
			return invoiceLine;
		}

		JobComInvoiceLine GetInvoiceLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine;
		}

		void AddContainerForInvoiceLine(JobComInvoiceLine invoiceLine, ZString containerNumber, bool isForInvoiceLine)
		{
			CusContainer container = invoiceLine.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			var cont = invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerNumber);
			cont.IsForInvoiceLine = isForInvoiceLine;
		}

		void AssociateContainerWithPGALine(PGA pga, ZString containerNumber)
		{
			RelatedContainer relatedContainer = pga.ContainersForInvoiceLine.FindByContainerNumber(containerNumber);
			relatedContainer.IsForPGALine = true;
		}

		OGADataTransferTool ogaDataTransferTool;
		OGADataTransferTool OGADataTransferTool => ogaDataTransferTool ?? (ogaDataTransferTool = new OGADataTransferTool());

		NotificationBuffer notify;
		NotificationBuffer Notification => notify ?? (notify = new NotificationBuffer());

		OrgHeader testOrganization;
		OrgHeader TestOrganization
		{
			get
			{
				if (testOrganization == null)
				{
					testOrganization = Factory.New<OrgHeader>();
					testOrganization.FillWithValidTestData();
					testOrganization.OH_FullName = "Test Org";
					var countryData = Factory.New<OrgCountryData>();
					countryData.OV_OH_OrgHeader = testOrganization.PK;
					countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
					var addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
					addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
					var address1 = TestOrganization.Addresses.AddNew();
					address1.OA_Address1 = "Test 1";
					address1.OA_Address2 = "Test 1A";
					var address2 = TestOrganization.Addresses.AddNew();
					address2.OA_Address1 = "Test 2";
					address2.OA_Address2 = "Test 2a";
					Factory.Save();
				}

				return testOrganization;
			}
		}
	}
}

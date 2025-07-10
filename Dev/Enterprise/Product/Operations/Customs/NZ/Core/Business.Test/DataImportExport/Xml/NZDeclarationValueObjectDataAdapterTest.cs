using System;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.NZ.Business.Data.Testing
{
	[TestedType(typeof(NZDeclarationValueObjectDataAdapter))]
	public class NZDeclarationValueObjectDataAdapterTest : DeclarationValueObjectDataAdapterAbstractTest
	{
		protected override void AssertDTAF(Xsd.Consol toConsol)
		{
			// Overidden as this field not used in NZ Customs.
		}

		public void TestImportingSupplierImporterDefaultsPaymentMethod()
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ.OM_IMPaymentMethod = PaymentMethodList.Codes.ClientDeferred;
			consignee.OH_RL_NKClosestPort = "NZAKL";
			Factory.Save();

			var xmlDec = new Xsd.ConsolAndShipment();
			xmlDec.Shipment.ShipmentDetails.ShipmentType = Xsd.ShipmentType.IMP;
			xmlDec.Shipment.ShipmentDetails.Consignee.EDICode = consignee.OH_Code;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var declaration = Factory.New<JobDeclaration>();
			DeclarationDataAdapter.ImportFromValueObject(declaration, xmlDec, context);

			AssertEquals(PaymentMethodList.Codes.ClientDeferred, declaration.JE_PaymentMethod);
		}

		public void TestNZGetNewInvoiceAdapter()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			InvoiceValueObjectDataAdapter invoiceAdapter = DeclarationDataAdapter.GetNewInvoiceAdapter(declaration);

			AssertNotNull(invoiceAdapter);
			AssertEquals(typeof(NZInvoiceValueObjectDataAdapter), invoiceAdapter.GetType());
		}

		public new void TestImportConsolDetails_SEA()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			Xsd.Consol consolXsd = new Xsd.Consol();

			declaration.JE_TransportMode = "SEA";
			consolXsd.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();
			Xsd.ConsolIdentifier consolIdentifierXsd = consolXsd.ConsolIdentifier.AddNew();
			consolIdentifierXsd.ConsolIdentifierTypeSpecified = true;
			consolIdentifierXsd.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifierXsd.Value = "IDENT1";

			consolIdentifierXsd = consolXsd.ConsolIdentifier.AddNew();
			consolIdentifierXsd.ConsolIdentifierTypeSpecified = true;
			consolIdentifierXsd.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;
			consolIdentifierXsd.Value = "IDENT2";

			consolXsd.ConsolDetail = new Xsd.ConsolConsolDetail();
			consolXsd.ConsolDetail.PortOfLoading = new Xsd.Movement();
			consolXsd.ConsolDetail.PortOfLoading.ActualDateTime = new DateTime(2005, 3, 13);
			consolXsd.ConsolDetail.PortOfLoading.EstimatedDateTime = new DateTime(2005, 3, 14);
			consolXsd.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPort(AUMEL);
			consolXsd.ConsolDetail.PortOfDischarge = new Xsd.Movement();
			consolXsd.ConsolDetail.PortOfDischarge.ActualDateTime = new DateTime(2005, 3, 15);
			consolXsd.ConsolDetail.PortOfDischarge.EstimatedDateTime = new DateTime(2005, 3, 16);
			consolXsd.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPort(AUSYD);
			consolXsd.ConsolDetail.PortFirstArrival = new Xsd.Movement();
			consolXsd.ConsolDetail.PortFirstArrival.ActualDateTime = new DateTime(2005, 3, 17);
			consolXsd.ConsolDetail.PortFirstArrival.EstimatedDateTime = new DateTime(2005, 3, 18);
			consolXsd.ConsolDetail.PortFirstArrival.Port = Xsd.UNLOCO.FromPort(AUTES);
			consolXsd.ConsolDetail.Item = new Xsd.SailingWithVesselVoyage();
			((Xsd.SailingWithVesselVoyage)consolXsd.ConsolDetail.Item).LloydsNo = "LloydsNo";
			((Xsd.SailingWithVesselVoyage)consolXsd.ConsolDetail.Item).VesselName = "Name";
			((Xsd.SailingWithVesselVoyage)consolXsd.ConsolDetail.Item).VoyageNo = "Voyage";

			DeclarationDataAdapter.ImportConsolDetails(declaration, consolXsd, DataImportContext);

			AssertEquals("IDENT1", declaration.JE_MasterBill);
			AssertEquals("AUMEL", declaration.JE_RL_NKPortOfLoading);
			AssertEquals(new ZDateTime(2005, 3, 13), declaration.JE_ExportDate);
			AssertEquals("AUSYD", declaration.JE_RL_NKPortOfArrival);
			AssertEquals(new ZDateTime(2005, 3, 15), declaration.JE_DateOfArrival);
			AssertEquals("Voyage", declaration.JE_VoyageFlightNo);
		}

		public new void TestImportConsolDetails_AIR()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			Xsd.Consol consolXsd = new Xsd.Consol();

			declaration.JE_TransportMode = "AIR";
			consolXsd.ConsolIdentifier = new Xsd.ConsolIdentifierCollection();

			Xsd.ConsolIdentifier consolIdentifierXsd = consolXsd.ConsolIdentifier.AddNew();
			consolIdentifierXsd.ConsolIdentifierTypeSpecified = true;
			consolIdentifierXsd.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifierXsd.Value = "IDENT1";

			consolIdentifierXsd = consolXsd.ConsolIdentifier.AddNew();
			consolIdentifierXsd.ConsolIdentifierTypeSpecified = true;
			consolIdentifierXsd.ConsolIdentifierType = Xsd.ConsolIdentifierType.Other;
			consolIdentifierXsd.Value = "IDENT2";

			consolXsd.ConsolDetail = new Xsd.ConsolConsolDetail();
			consolXsd.ConsolDetail.PortOfLoading = new Xsd.Movement();
			consolXsd.ConsolDetail.PortOfLoading.ActualDateTime = new DateTime(2005, 3, 13);
			consolXsd.ConsolDetail.PortOfLoading.EstimatedDateTime = new DateTime(2005, 3, 14);
			consolXsd.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPort(AUMEL);
			consolXsd.ConsolDetail.PortOfDischarge = new Xsd.Movement();
			consolXsd.ConsolDetail.PortOfDischarge.ActualDateTime = new DateTime(2005, 3, 15);
			consolXsd.ConsolDetail.PortOfDischarge.EstimatedDateTime = new DateTime(2005, 3, 16);
			consolXsd.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPort(AUSYD);
			consolXsd.ConsolDetail.PortFirstArrival = new Xsd.Movement();
			consolXsd.ConsolDetail.PortFirstArrival.ActualDateTime = new DateTime(2005, 3, 17);
			consolXsd.ConsolDetail.PortFirstArrival.EstimatedDateTime = new DateTime(2005, 3, 18);
			consolXsd.ConsolDetail.PortFirstArrival.Port = Xsd.UNLOCO.FromPort(NZAKL);
			consolXsd.ConsolDetail.Item = new Xsd.FlightWithFlightNumber();
			((Xsd.FlightWithFlightNumber)consolXsd.ConsolDetail.Item).FlightNoJourneyNoTruckRegNo = "Flight";

			DeclarationDataAdapter.ImportConsolDetails(declaration, consolXsd, DataImportContext);

			AssertEquals("IDENT1", declaration.JE_MasterBill);
			AssertEquals("AUMEL", declaration.JE_RL_NKPortOfLoading);
			AssertEquals(new ZDateTime(2005, 3, 13), declaration.JE_ExportDate);
			AssertEquals("AUSYD", declaration.JE_RL_NKPortOfArrival);
			AssertEquals(new ZDateTime(2005, 3, 15), declaration.JE_DateOfArrival);
			AssertEquals("Flight", declaration.JE_VoyageFlightNo);
		}

		public override void TestImportHouseBillWhereEmptyHousebillAlreadyExists()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_MasterBill = "MBL1";

			Xsd.ConsolAndShipment consolAndShipmentXsd = new Xsd.ConsolAndShipment();

			Xsd.Shipment shipmentXsd = consolAndShipmentXsd.Shipment;
			Xsd.ShipmentIdentifier shipmentIdentifierXsd = shipmentXsd.ShipmentIdentifier.AddNew();
			shipmentIdentifierXsd.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifierXsd.Value = "HBL1";

			shipmentIdentifierXsd = shipmentXsd.ShipmentIdentifier.AddNew();
			shipmentIdentifierXsd.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifierXsd.Value = "HBL2";

			shipmentIdentifierXsd = shipmentXsd.ShipmentIdentifier.AddNew();
			shipmentIdentifierXsd.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifierXsd.Value = "HBL3";

			DeclarationDataAdapter.ImportFromValueObject(declaration, consolAndShipmentXsd, DataImportContext);

			AssertEquals(4, declaration.Bills.Count);
			AssertEquals("HBL1", declaration.JE_HouseBill);
			AssertEquals("MBL1", declaration.JE_MasterBill);
			AssertEquals("MBL1", declaration.Bills[0].CU_BillNum);
			AssertEquals("HBL1", declaration.Bills[1].CU_BillNum);
			AssertEquals("MBL1", declaration.Bills[1].CU_MasterBill);

			AssertEquals("HBL2", declaration.Bills[2].CU_BillNum);
			AssertEquals("MBL1", declaration.Bills[2].CU_MasterBill);

			AssertEquals("HBL3", declaration.Bills[3].CU_BillNum);
			AssertEquals("MBL1", declaration.Bills[3].CU_MasterBill);
		}

		public void TestSetMessageTypeTypeIfPortOfArrivalIsNZ()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			DeclarationDataAdapter.SetShipmentTypeDetailsCore(declaration);
			AssertEquals("Message Type for NZ Specific XML Declaration Data Adapter is incorrect", "IMP", declaration.JE_MessageType);
		}

		public void TestSetMessageTypeTypeIfPortOfArrivalIsNotNZ()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_RL_NKPortOfArrival = "USJFK";
			DeclarationDataAdapter.SetShipmentTypeDetailsCore(declaration);
			AssertEquals("Message Type for NZ Specific XML Declaration Data Adapter is incorrect", "EXP", declaration.JE_MessageType);
		}

		public void TestImportDeclarationAdditionalInfo()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();

			Xsd.AdditionalCustomsInformationCollection addCusInfoXsds = new Xsd.AdditionalCustomsInformationCollection();
			Xsd.AdditionalCustomsInformation addCusInfoXsd = addCusInfoXsds.AddNew();

			addCusInfoXsd.CustomsDetailType = "ConcessionCode";
			addCusInfoXsd.CustomsDetailValue = "UT";
			AssertEquals("Precondition: declaration.AddInfo.ZN_ConcessionCode", "", ((IHaveNZAddInfo)declaration).AddInfo.ZN_ConcessionCode);

			DeclarationDataAdapter.ImportDeclarationAdditionalInfo(declaration, addCusInfoXsds, DataImportContext);
			AssertEquals("declaration.AddInfo.ZN_ConcessionCode", "UT", ((IHaveNZAddInfo)declaration).AddInfo.ZN_ConcessionCode);

			addCusInfoXsd.CustomsDetailType = "AntiDumpingDuty";
			addCusInfoXsd.CustomsDetailValue = "123";
			AssertEquals("Precondition: declaration.AddInfo.ZN_AntiDumpingDuty", ZDecimal.Zero, ((IHaveNZAddInfo)declaration).AddInfo.ZN_AntiDumpingDuty);

			DeclarationDataAdapter.ImportDeclarationAdditionalInfo(declaration, addCusInfoXsds, DataImportContext);
			AssertEquals("declaration.AddInfo.ZN_AntiDumpingDuty", new ZDecimal(123), ((IHaveNZAddInfo)declaration).AddInfo.ZN_AntiDumpingDuty);
		}

		public void TestExportDeclarationAdditionalInfo()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			Xsd.AdditionalCustomsInformationCollection addCusInfoXsds = new Xsd.AdditionalCustomsInformationCollection();

			((IHaveNZAddInfo)declaration).AddInfo.LoadPropertiesFromString("AntiDumpingDuty=1*ProhibitedCodes=2*OtherInfos=hellotest^keepontesting=yes");
			DeclarationDataAdapter.ExportDeclarationAdditionalInfo(addCusInfoXsds, declaration, new NotificationBuffer());

			Assert("AntiDumpingDuty should be in collection", ItemInXmlAddInfoCollection(addCusInfoXsds, "AntiDumpingDuty"));
			Assert("ProhibitedCodes should be in collection", ItemInXmlAddInfoCollection(addCusInfoXsds, "ProhibitedCodes"));
			Assert("OtherInfos should be in collection", ItemInXmlAddInfoCollection(addCusInfoXsds, "OtherInfos"));

			AssertEquals("AntiDumpingDuty Value", "1", GetXmlAddInfoValue(addCusInfoXsds, "AntiDumpingDuty"));
			AssertEquals("ProhibitedCodes Value", "2", GetXmlAddInfoValue(addCusInfoXsds, "ProhibitedCodes"));
			AssertEquals("OtherInfos Value", "hellotest^keepontesting=yes", GetXmlAddInfoValue(addCusInfoXsds, "OtherInfos"));
		}

		public void TestExportMasterbillDetails()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_MasterBill = "TEST";
			Xsd.Consol consolXsd = new Xsd.Consol();
			DeclarationDataAdapter.ExportMasterbillDetails(consolXsd, declaration);
			AssertEquals("TEST", consolXsd.ConsolIdentifier.FindFirst(Xsd.ConsolIdentifierType.MasterWaybill).Value);
			AssertEquals(1, consolXsd.ConsolIdentifier.Find(Xsd.ConsolIdentifierType.MasterWaybill).Count);
		}

		public void TestImportProcessingPortUsingProcessPort()
		{
			Xsd.ConsolAndShipment consolAndShipmentXsd = new Xsd.ConsolAndShipment();
			consolAndShipmentXsd.Consol = new Xsd.Consol();
			consolAndShipmentXsd.Consol.ConsolDetail = new Xsd.ConsolConsolDetail();
			consolAndShipmentXsd.Consol.Shipments = new Xsd.ShipmentCollection();
			Xsd.Shipment shipmentXsd = consolAndShipmentXsd.Consol.Shipments.AddNew();
			shipmentXsd.ShipmentDetails = new Xsd.ShipmentShipmentDetails();
			consolAndShipmentXsd.Shipment = shipmentXsd;

			shipmentXsd.Invoices = new Xsd.InvoiceHeaderCollection();
			shipmentXsd.Declaration = new Xsd.Declaration();
			shipmentXsd.Declaration.AddCustomsDetails = new Xsd.AdditionalCustomsInformationCollection();
			Xsd.AdditionalCustomsInformation addCusInfoXsd = shipmentXsd.Declaration.AddCustomsDetails.AddNew();

			addCusInfoXsd.CustomsDetailType = "ProcessPort";
			addCusInfoXsd.CustomsDetailValue = "NZAKL";

			addCusInfoXsd = shipmentXsd.Declaration.AddCustomsDetails.AddNew();
			addCusInfoXsd.CustomsDetailType = "RL_NKProcessingPort";
			addCusInfoXsd.CustomsDetailValue = "AUMEL";

			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			DeclarationDataAdapter.ImportFromValueObject(declaration, consolAndShipmentXsd, DataImportContext);

			AssertEquals("Should load the processing port from the different name", "NZAKL", declaration.JE_RL_NKProcessingPort);
		}

		public void TestExportProcessingPortUsingProcessPort()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			declaration.JE_RL_NKProcessingPort = "NZAKL";

			Xsd.ConsolAndShipment consolAndShipmentXsd = DeclarationDataAdapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));

			Assert("Should have Addition customs information loaded", consolAndShipmentXsd.Shipment.Declaration.AddCustomsDetails.Count > 0);

			Xsd.AdditionalCustomsInformation processingPortAddCusInfoXsd = null;
			foreach (Xsd.AdditionalCustomsInformation addCusInfoXsd in consolAndShipmentXsd.Shipment.Declaration.AddCustomsDetails)
			{
				if (addCusInfoXsd.CustomsDetailType == "ProcessPort")
				{
					processingPortAddCusInfoXsd = addCusInfoXsd;
					break;
				}
			}

			Assert("Should have found the ProcessPort Item", processingPortAddCusInfoXsd != null);
			AssertEquals("processingPortAddCusInfoXsd.CustomsDetailType", "ProcessPort", processingPortAddCusInfoXsd.CustomsDetailType);
			AssertEquals("processingPortAddCusInfoXsd.CustomsDetailValue", "NZAKL", processingPortAddCusInfoXsd.CustomsDetailValue);
		}

		public void TestExportHousebillDetails2()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HouseBill 1";

			houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HouseBill 2";

			//			NZDeclarationValueObjectDataAdapter Adapter = new NZDeclarationValueObjectDataAdapter();
			Xsd.ConsolAndShipment consolAndShipmentXsd = DeclarationDataAdapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Should have all housebills exported", 2, consolAndShipmentXsd.Shipment.ShipmentIdentifier.Count);

			Xsd.ShipmentIdentifier shipmentIdentifierXsd = consolAndShipmentXsd.Shipment.ShipmentIdentifier[0];
			AssertEquals("First item should be set as a house bill", Xsd.ShipmentIdentifierType.Housebill, shipmentIdentifierXsd.ShipmentIdentifierType);
			AssertEquals("Should be the first house bill", "HOUSEBILL 1", shipmentIdentifierXsd.Value);

			shipmentIdentifierXsd = consolAndShipmentXsd.Shipment.ShipmentIdentifier[1];
			AssertEquals("Second item should be set as a house bill", Xsd.ShipmentIdentifierType.Housebill, shipmentIdentifierXsd.ShipmentIdentifierType);
			AssertEquals("Should be the second house bill", "HOUSEBILL 2", shipmentIdentifierXsd.Value);
		}

		public override void TestGetNewInvoicesGenerator()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			AssertEquals("Invoice Generator", InvoicesGeneratorType, DeclarationDataAdapter.GetNewInvoicesGenerator(declaration).GetType());
		}

		public override void TestExportBillContainerPacks()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			declaration.DisableDefaultPackingInformation = true;
			AddPackageToDeclaration(declaration, "HOUSEBILL 1", "000001", 1, "BG");
			AddPackageToDeclaration(declaration, "HOUSEBILL 2", "000002", 2, "AE");
			declaration.JE_MasterBill = "MasterBill";

			Xsd.ConsolAndShipment consolAndShipmentXsd = DeclarationDataAdapter.ExportToValueObject(declaration, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(2, consolAndShipmentXsd.Shipment.Declaration.BillContainerPacks.Count);
			Xsd.DeclarationBillContainerPack billContainerPackXsd = consolAndShipmentXsd.Shipment.Declaration.BillContainerPacks[0];
			AssertEquals("HOUSEBILL 1", billContainerPackXsd.BillNumber);
			AssertEquals("000001", billContainerPackXsd.ContainerNumber);
			AssertEquals(1M, billContainerPackXsd.PackQty.Value);
			AssertEquals("BG", billContainerPackXsd.PackQty.DimensionType);

			billContainerPackXsd = consolAndShipmentXsd.Shipment.Declaration.BillContainerPacks[1];
			AssertEquals("HOUSEBILL 2", billContainerPackXsd.BillNumber);
			AssertEquals("000002", billContainerPackXsd.ContainerNumber);
			AssertEquals(2M, billContainerPackXsd.PackQty.Value);
			AssertEquals("AE", billContainerPackXsd.PackQty.DimensionType);
		}

		public void TestSetHousebillDetails()
		{
			Xsd.ConsolAndShipment consolAndShipmentXsd = new Xsd.ConsolAndShipment();

			Xsd.ShipmentIdentifier shipmentIdentifierXsd = consolAndShipmentXsd.Shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifierXsd.Value = "HOUSEBILL 1";
			shipmentIdentifierXsd.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			shipmentIdentifierXsd = consolAndShipmentXsd.Shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifierXsd.Value = "HOUSEBILL 2";
			shipmentIdentifierXsd.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			shipmentIdentifierXsd = consolAndShipmentXsd.Shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifierXsd.Value = "OTHER";
			shipmentIdentifierXsd.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Other;

			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			DeclarationDataAdapter.ImportFromValueObject(declaration, consolAndShipmentXsd, DataImportContext);

			AssertEquals(2, declaration.Bills.Count);

			AssertEquals("HOUSEBILL 1", declaration.Bills[0].CU_HouseBill);

			AssertEquals("HOUSEBILL 2", declaration.Bills[1].CU_HouseBill);
		}

		public override void TestImportBillContainerPacks()
		{
			Xsd.ConsolAndShipment consolAndShipmentXsd = new Xsd.ConsolAndShipment();
			AddBillContainerPackToXml(consolAndShipmentXsd, "HOUSEBILL 1", "CONT 1", 11, "AE");
			AddBillContainerPackToXml(consolAndShipmentXsd, "HOUSEBILL 2", "CONT 2", 12, "BG");

			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();
			DeclarationDataAdapter.ImportFromValueObject(declaration, consolAndShipmentXsd, DataImportContext);

			Package package = declaration.Packages[0];
			AssertEquals("HB:HOUSEBILL 1", package.CW_HouseBill);
			AssertEquals("CONT 1", package.CW_ContainerNoOrEquipmentNo);
			AssertEquals(11, package.CW_PackQty);
			AssertEquals("AE", package.CW_PackType);

			package = declaration.Packages[1];
			AssertEquals("HB:HOUSEBILL 2", package.CW_HouseBill);
			AssertEquals("CONT 2", package.CW_ContainerNoOrEquipmentNo);
			AssertEquals(12, package.CW_PackQty);
			AssertEquals("BG", package.CW_PackType);
		}

		public void TestImportBillContainerPacksWhenBillNumberIsNotSpecified()
		{
			var consolAndShipmentXsd = new Xsd.ConsolAndShipment();
			AddBillContainerPackToXml(consolAndShipmentXsd, "", "CONT 1", 11, "AE");// no bill specified

			var declaration = (JobDeclaration)GetJobDeclaration();
			declaration.Bills.AddNew().CU_BillType = BillTypeList.Codes.HouseBill;
			AssertNotNull(declaration.PrimaryHouseBill);

			DeclarationDataAdapter.ImportFromValueObject(declaration, consolAndShipmentXsd, DataImportContext);

			AssertEquals(1, declaration.Bills.Count);
			var package = declaration.Packages[0];
			AssertEquals("Bill should be guesssed if there is only one bill in the job", declaration.Bills[0], package.PackingGroup.Bill);
			AssertEquals("CONT 1", package.CW_ContainerNoOrEquipmentNo);
			AssertEquals(11, package.CW_PackQty);
			AssertEquals("AE", package.CW_PackType);
		}

		public void TestExportGroupInvoices()
		{
			JobDeclaration declaration = (JobDeclaration)GetJobDeclaration();

			JobComInvoiceGroupHeader allInvoicesHeader = declaration.JobComInvoiceGroupHeaders[0];
			allInvoicesHeader.JZ_InvoiceNumber = "All Invoices";
			JobComInvoiceGroupHeader subGroupHeader = allInvoicesHeader.JobComInvoiceGroupHeaders.AddNew();
			subGroupHeader.JZ_InvoiceNumber = "GroupHeader";

			Xsd.InvoiceHeaderCollection invoiceHeaderXsds = DeclarationDataAdapter.ExportInvoiceHeaders(declaration, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("All Invoices", invoiceHeaderXsds[0].InvoiceNumber);
			AssertEquals("GroupHeader", invoiceHeaderXsds[1].InvoiceNumber);
		}

		public override void TestFindBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var declarationValue = new Xsd.ConsolAndShipment();

			var masterBillValue = declarationValue.Consol.ConsolIdentifier.AddNew();
			masterBillValue.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			var houseBillValue = declarationValue.Shipment.ShipmentIdentifier.AddNew();
			houseBillValue.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			masterBillValue.Value = "MasterBillToFind";
			houseBillValue.Value = "HouseBillToFind";
			declaration.JE_MasterBill = "MasterBillToFind";
			declaration.JE_HouseBill = "HouseBillToFind";
			Factory.Save();
			AssertBusinessObjectFound("Should find the correct declaration by master bill and house bill", declarationValue, declaration, expectMatch: true);

			masterBillValue.Value = "MasterBillToFind";
			houseBillValue.Value = "";
			declaration.JE_MasterBill = "MasterBillToFind";
			declaration.JE_HouseBill = "";
			Factory.Save();
			AssertBusinessObjectFound("Should not find the correct declaration as house bill is blank and bill has been deleted in declaration", declarationValue, declaration, expectMatch: false);
		}

		#region Implementation
		void AddPackageToDeclaration(JobDeclaration declaration, string houseBill, string containerNo, int packQty, string packType)
		{
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_HouseBill = houseBill;

			Package package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = containerNo;
			package.CW_PackQty = packQty;
			package.CW_PackType = packType;
		}

		void AddBillContainerPackToXml(Xsd.ConsolAndShipment consolAndShipmentXsd, string houseBill, string containerNo, int packQty, string packType)
		{
			Xsd.ShipmentIdentifier shipmentIdentifierXsd = consolAndShipmentXsd.Shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifierXsd.Value = houseBill;
			shipmentIdentifierXsd.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;

			Xsd.Container containerXsd = consolAndShipmentXsd.Consol.ConsolDetail.Containers.AddNew();
			containerXsd.ContainerNumber = containerNo;

			Xsd.DeclarationBillContainerPack billContainerPackXsd = consolAndShipmentXsd.Shipment.Declaration.BillContainerPacks.AddNew();
			billContainerPackXsd.BillNumber = houseBill;
			billContainerPackXsd.ContainerNumber = containerNo;
			billContainerPackXsd.PackQty.Value = packQty;
			billContainerPackXsd.PackQty.DimensionType = packType;
		}

		RefUNLOCO AUSYD => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

		RefUNLOCO AUMEL => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");

		RefUNLOCO AUTES => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUTES");

		RefUNLOCO NZAKL => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

		protected override Type InvoicesGeneratorType => typeof(NZInvoicesGeneratorFromXSD);

		protected override Customs.Business.BaseJobDeclaration NewBusinessObject()
		{
			JobDeclaration result = Factory.New<JobDeclaration>();
			result.DisableDefaultPackingInformation = true;
			result.ApportionmentDirty = false;
			result.JE_ApplicationCode = "";
			result.JE_TransactionNature = "";
			return result;
		}

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration()
		{
			return NewBusinessObject();
		}

		protected override ValueObjectDataAdapter<Customs.Business.BaseJobDeclaration, Xsd.ConsolAndShipment> GetNewBizObjXmlDataAdapter()
		{
			return new NZDeclarationValueObjectDataAdapter();
		}

		readonly string baseTestFilePath = TestFileReader.GetAssemblyName(typeof(NZDeclarationValueObjectDataAdapterTest)) + ".DataImportExport.Xml.Testing";

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyJobDeclaration(), FileReader.ExtractEmbeddedResourceToFile(baseTestFilePath, TempDir.DirectoryName, "NZEmptyDeclaration.xml"), ValidationKind.None, "Empty Declaration");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			TestJobDec.ResumeApportionment();
			return new BusinessObjectAndExpectedOutputFileName(TestJobDec, FileReader.ExtractEmbeddedResourceToFile(baseTestFilePath, TempDir.DirectoryName, "NZPopulatedDeclaration.xml"), ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Declaration");
		}

		protected override void SetUp()
		{
			base.SetUp();
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);

			TestJobDec.JE_RL_NKPortOfFirstArrivalInfo.Value = new ZString("");
			TestJobDec.JE_DateOfFirstArrival = ZDateTime.Empty;
			TestJobDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
		}

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(NZDeclarationValueObjectDataAdapterTest)));
		TestFileReader fileReader;

		protected override void TearDown()
		{
			base.TearDown();

			tempDir?.Dispose();
		}

		protected override string TestingCountry
		{
			get { return null; }
		}

		NZDeclarationValueObjectDataAdapterTestClass fDeclarationDataAdapter;
		NZDeclarationValueObjectDataAdapterTestClass DeclarationDataAdapter
		{
			get { return fDeclarationDataAdapter ?? (fDeclarationDataAdapter = new NZDeclarationValueObjectDataAdapterTestClass()); }
		}

		class NZDeclarationValueObjectDataAdapterTestClass : NZDeclarationValueObjectDataAdapter
		{
			public new void SetShipmentTypeDetailsCore(Customs.Business.BaseJobDeclaration jobDec)
			{
				base.SetShipmentTypeDetailsCore(jobDec);
			}

			public new void ImportVesselInformation(Customs.Business.BaseJobDeclaration jobDec, Xsd.SailingWithVesselVoyage sailingWithVesselVoyage, IValueObjectImportContext context)
			{
				base.ImportVesselInformation(jobDec, sailingWithVesselVoyage, context);
			}

			public new void ImportDeclarationAdditionalInfo(Customs.Business.BaseJobDeclaration jobDec, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
			{
				base.ImportDeclarationAdditionalInfo(jobDec, addCustomsDetails, context);
			}

			public new void ExportDeclarationAdditionalInfo(Xsd.AdditionalCustomsInformationCollection addCustomsDetails, Customs.Business.BaseJobDeclaration jobDec, INotifications notification)
			{
				base.ExportDeclarationAdditionalInfo(addCustomsDetails, jobDec, notification);
			}

			public new void ExportMasterbillDetails(Xsd.Consol toConsol, Customs.Business.BaseJobDeclaration jobDec)
			{
				base.ExportMasterbillDetails(toConsol, jobDec);
			}

			public new InvoiceValueObjectDataAdapter GetNewInvoiceAdapter(Customs.Business.BaseJobDeclaration jobDec)
			{
				jobDec.ResumeApportionment();
				return base.GetNewInvoiceAdapter(jobDec);
			}

			public new void ImportConsolDetails(Customs.Business.BaseJobDeclaration jobDec, Xsd.Consol consol, IValueObjectImportContext context)
			{
				base.ImportConsolDetails(jobDec, consol, context);
			}

			public new Xsd.InvoiceHeaderCollection ExportInvoiceHeaders(Customs.Business.BaseJobDeclaration jobDec, IValueObjectExportContext context)
			{
				return base.ExportInvoiceHeaders(jobDec, context);
			}

			public new InvoicesGeneratorFromXSD GetNewInvoicesGenerator(Customs.Business.BaseJobDeclaration jobDec)
			{
				return base.GetNewInvoicesGenerator(jobDec);
			}
		}

		ValueObjectImportContext fDataImportContext;
		ValueObjectImportContext DataImportContext
		{
			get { return fDataImportContext ?? (fDataImportContext = new ValueObjectImportContext(Factory, new NotificationBuffer())); }
		}
		#endregion
	}
}

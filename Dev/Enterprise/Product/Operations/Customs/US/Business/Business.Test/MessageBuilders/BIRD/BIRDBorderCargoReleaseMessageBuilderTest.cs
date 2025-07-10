using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class BIRDBorderCargoReleaseMessageBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			JobDeclaration declaration = CreateDeclaration();

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			BIRDCargoReleaseMessageBuilder builder = new BIRDCargoReleaseMessageBuilder(entry);
			MQEDIMessage message = builder.PopulateMessage();

			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.BIRDCargoRelease, message.EM_MessageSubType);
			AssertEquals("1234567890", ((BRDAA)message.MessageBlock.B).OriginatingBrokerRef);

			AssertContains("H1A8888XJ5 <E#PLCH>69-9999999JC20      81                   OTT1123401891       ", message.EM_MessageText);
			AssertContains("H2        69-9999999JC V123W0000015000                                          ", message.EM_MessageText);
			AssertContains("HA            TESTMB1                             00000130PCS        OTT1       ", message.EM_MessageText);
			AssertContains("H5001AU8471704065XYBEREQU6LON               0000015000                          ", message.EM_MessageText);

			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals(EDIMessage.Status.Acknowledged, message.EM_Status);
		}

		public void TestBuildWhenExternalBrokerExists()
		{
			JobDeclaration declaration = CreateDeclaration();
			declaration.SetExternalBrokerForTesting();

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			BIRDCargoReleaseMessageBuilder builder = new BIRDCargoReleaseMessageBuilder(entry);
			MQEDIMessage message = builder.PopulateMessage();
			AssertEquals(EDIMessage.Status.Pending, message.EM_Status);
		}

		JobDeclaration CreateDeclaration()
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "69-9999999JC");

			OrgHeader manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			declaration.US_SchDLoading = "56789";
			declaration.US_SchDArrival = "1234";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
			declaration.IOROrgPK = declaration.JE_OH_Importer;
			declaration.US_PaymentType = "";
			declaration.US_SchDEntry = "8888";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_SuretyCode = "891";
			declaration.JE_RL_NKPortOfLoading = "PERTD";
			declaration.JE_RL_NKOrigin = "PERTD";
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.JE_VesselName = "DONGSNICKNAMEISRICHI";

			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "TestMB1";
			masterBill.US_UI_NKBillIssuerSCAC = "OTT1";
			masterBill.CU_NoOfPacks = 130;
			masterBill.CU_PackType = "PCS";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 15000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;
			invoiceLine.JI_Tariff = "8471704065";
			declaration.US_BRDRefNo = "1234567890";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			return declaration;
		}
	}
}

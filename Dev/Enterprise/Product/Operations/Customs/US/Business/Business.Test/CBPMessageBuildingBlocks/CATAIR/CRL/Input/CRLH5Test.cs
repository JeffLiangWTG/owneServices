using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.US;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class CRLH5Test : BIRDLineUpdateTest
	{
		public void TestAddingManufacturer()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "69-9999999JC");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
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

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "TestMB1";
			masterBill.US_UI_NKBillIssuerSCAC = "OTT1";
			masterBill.CU_NoOfPacks = 130;
			masterBill.CU_PackType = "PCS";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 15000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");

			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;
			invoiceLine.JI_Tariff = "8471704065";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.US_BRDRefNo = "1234567890";

			var notifications = new NotificationCollection();
			var generator = new ABIInputBlockControlGenerator<Messaging.Business.MessageBuildingBlocks.BIRD.BRDAA, Messaging.Business.MessageBuildingBlocks.BIRD.BRDZZ>();

			AssertEquals(" (XYBEREQU6LON)", declaration.InvoiceLines[0].ManufacturerNameAndID);
			string messageText =
"AA3461XJ5    1234567890                 20131118205751                          " +
"H1A8888XJ5 <E#PLCH>69-9999999JC10      81                   OTT1123401891       " +
"H2        69-9999999JC V123W0000015000                     DONGSNICKNAMEISRICHI " +
"HA            TESTMB1                             00000130PCS        OTT1       " +
"H5001AU8471704065XYBEREQ111PM               0000015000                          " +
"ZZ3461000000004                                                                 ";

			generator.Deserialise(messageText);
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			AssertEquals("AUTO CREATED FROM MID (XYBEREQ111PM)", declaration.InvoiceLines[0].ManufacturerNameAndID);

			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, declaration.InvoiceLines[0].ManufacturerAddress.Header.PK);

			var message = Factory.LoadTop1<MQEDIMessage>(query);
			AssertNotNull(message);

			var messageType = ACEApplicationIdentifierCodeList.Codes.ManufacturerNameAndAddressQuery;
			AssertEquals(messageType, message.EM_MessageType);
			AssertEquals("XYBEREQ111PM", declaration.InvoiceLines[0].ManufacturerAddress.Header.MainAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates));

			var manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");

			var codes = new OrgCusCode.Loader(Factory).Load(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");
			AssertEquals("Precondition: shoul be two customs codes", 2, codes.Length);

			notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			AssertEquals("AUTO CREATED FROM MID (XYBEREQ111PM)", declaration.InvoiceLines[0].ManufacturerNameAndID);

			codes = new OrgCusCode.Loader(Factory).Load(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");
			AssertEquals("Shoul be two customs codes, no new duplicated created after import", 2, codes.Length);
		}

		public void TestUpdateManufacturerAddressByCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB", GlbCompany.CurrentCompany.Country);

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();

			var cRLH5 = new CRLH5() { ManufacturerShipper = "PHEVEAPP80SAB" };
			((IBIRDLineRecord)cRLH5).Update(invoiceLine, notifications);
			AssertEquals("Matched address found by MID", orgAddress1.PK, invoiceLine.JI_OA_ManufacturerAddress);

			notifications.Clear();
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			cRLH5 = new CRLH5() { ManufacturerShipper = "  PHEVEAPP80SAB" };
			((IBIRDLineRecord)cRLH5).Update(invoiceLine, notifications);
			AssertEquals("No matched address found or created because MID is invalid", ZGuid.Empty, invoiceLine.JI_OA_ManufacturerAddress);
			AssertContains(ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, "  PHEVEAPP80SAB"), notifications.AsString.Trim());

			notifications.Clear();
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			cRLH5 = new CRLH5() { ManufacturerShipper = "PHEVEAPP80SCD" };
			((IBIRDLineRecord)cRLH5).Update(invoiceLine, notifications);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			AssertEquals("New organization and address is created", org.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
		}

		protected override IBIRDLineRecord[] GetPopulatedLineRecords()
		{
			var h5 = new CRLH5();
			h5.RecordControlNumber = 1;
			h5.CountryOfOrigin = "AU";
			h5.TariffNumber = "0000000000";
			h5.ManufacturerShipper = "AUABCEXP6390ALE";
			h5.LineItemUltimateConsignee = "12-1234567CC";
			h5.LineItemValue = 10000m;

			return new IBIRDLineRecord[] { h5 };
		}

		protected override EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new CargoReleaseMessageBuilder(declaration.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add, false);
		}

		protected override Type GetTypeOfMessageBlock()
		{
			return typeof(CRLH5);
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine, IBIRDLineRecord lineRecord)
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUABCEXP6390ALE", GlbCompany.CurrentCompany.Country);

			var ultimateConsignee = Factory.NewWithValidTestData<OrgHeader>();
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567CC", GlbCompany.CurrentCompany.Country);

			Factory.Save();
		}
	}
}

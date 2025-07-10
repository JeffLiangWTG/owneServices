using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>))]
	sealed class EntryHeaderENS7501PrintTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2019, 02, 25)]
		public void TestPrintDutyAmountFromEntryLineFor9808003000OnHeader()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.US_SupTariff = "9808003000";
			line1.JI_Tariff = "8457100075";
			line1.JI_LinePrice = 2000m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entryHeader = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var ehp = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entryHeader, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals(84m, ehp.TotalDutyAmt);
			AssertEquals(25.67m, ehp.TotalOther);
			AssertEquals(0m, ehp.TotalEstTax);
			AssertEquals("Block40Total should not contains 9808003000 duty", 25.67m, ehp.Block40Total);
		}

		public void TestIParentDocManagerSupportMembers()
		{
			var print = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			IParentDocManagerSupport supporter = print;
			AssertEquals(Core.Constants.DocManagerCodes.CustomsEntry, supporter.DocManagerInfo.DocManagerCode);
			AssertEquals("ParentGuid", Entry.PK, supporter.ParentGuid);
			AssertEquals("ParentTableName", Entry.TableName, supporter.ParentTableName);
		}

		public void TestIDocumentDeliveredLogSupporterMembers()
		{
			var print = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			IDocumentDeliveredLogSupporter supporter = print;
			AssertEquals("BusinessObjectTypeToLogAgainst", typeof(CusEntryHeader), supporter.BusinessObjectTypeToLogAgainst);
			AssertEquals("Identifier", Entry.PK, supporter.Identifier);
		}

		public void TestFormattedEntryNumber()
		{
			Declaration.US_EntryFilerCode = "ABC";
			CusEntryHeader entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "12345678";
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Formatted Entry No", "ABC-1234567-8", ehp.FormattedEntryNumber);
		}

		[TestDate(2007, 10, 30, 6, 0, 0)]
		public void TestEntrySummaryFiledDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader ensEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary)[0];
			CusEntryHeader crlEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.CargoRelease)[0];
			var ehp1 = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			var ehp2 = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(crlEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("EntrySummaryFiledDate should not print until entry has successful response - ie in printing from message class", ZDate.Empty, ehp1.EntrySummaryFiledDate);
			AssertEquals("EntrySummaryFiledDate", ZDate.Empty, ehp2.EntrySummaryFiledDate);
		}

		public void TestLocationOfGoodsAndName()
		{
			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "LOC1", "Location", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			warehouse.OH_FullName = "Warehouse Test for FIRMS";
			warehouse.OH_IsWarehouseClient = true;
			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Declaration.US_US_NKLocationOfGoods = "LOC1";

			var ensEntry = Declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("LOC1/Location", ehp.LocationOfGoodsAndName);

			warehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "LOCA");
			Declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("LOCA/Warehouse Test for FIRMS", ehp.LocationOfGoodsAndName);

			Declaration.US_GeneralOrderNo = "483720958101";
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Location should now print General Order Number", "G.O. 483720958101", ehp.LocationOfGoodsAndName);

			Declaration.US_GeneralOrderNo = "";
			Declaration.US_US_NKLocationOfGoods = "";
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Declaration.WarehouseDocAddress.E2_OA_Address = ZGuid.Empty;
			Declaration.JE_VoyageFlightNo = "730";
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Location should now print Flight Number", "730", ehp.LocationOfGoodsAndName);

			Declaration.US_GeneralOrderNo = "2011100156200";
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Location should now print formatted 13 digit General Order Number", "GO-2011-1001-56200", ehp.LocationOfGoodsAndName);
		}

		public void TestLocationOfGoodsAndNameForBondedWarehouseEntry()
		{
			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "LOCA", "Location", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var warehouse = Factory.New<OrgHeader>();
			warehouse.FillWithValidTestData();
			warehouse.OH_FullName = "JPDuminy Bond Stores";
			warehouse.OH_IsWarehouseClient = true;
			warehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "WH01");
			Declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;

			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Declaration.US_US_NKLocationOfGoods = "LOCA";
			Declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;

			var ensEntry = declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Organisation name to be used when FIRMS Code does not exist in table", "WH01/JPDuminy Bond Stores", ehp.LocationOfGoodsAndName);
		}

		public void TestWarehouseWithdrawal()
		{
			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals("when US_EntryType is in (31,32, 34,38)", false, Declaration.IsExWarehouseEntryType);
			AssertEquals(false, ehp.WarehouseWithdrawal);

			Declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			AssertEquals(true, ehp.WarehouseWithdrawal);

			Declaration.US_EstimatedEntryDate = ZDateTime.Today.AddDays(2);

			var acePrint = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Box 7 printed", ZDateTime.Today.AddDays(2), acePrint.EstimatedEntryDate);

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EstimatedEntryDate = ZDateTime.Today.AddDays(3);
			acePrint = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Box 7 should be printed", ZDateTime.Today.AddDays(3), acePrint.EstimatedEntryDate);
		}

		public void TestDecFinalWithdrawal()
		{
			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals(false, ehp.DecFinalWithdrawal);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, ehp.DecFinalWithdrawal);

			Declaration.US_IsFinalWHS = false;
			AssertEquals(false, ehp.DecFinalWithdrawal);

			Declaration.US_IsFinalWHS = true;
			AssertEquals(true, ehp.DecFinalWithdrawal);
		}

		public void TestEntryTypeCode()
		{
			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Entry Type Code", "", ehp.EntryTypeCode);

			Declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			AssertEquals("Entry Type Code", "LIVE", ehp.EntryTypeCode);
		}

		public void TestTaxToBeDeferred()
		{
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			var ensEntry = Declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			Assert("TaxToBeDeferred", ehp.TaxToBeDeferred);

			Declaration.US_TaxDeferIndicator = "";
			Assert("TaxToBeDeferred", !ehp.TaxToBeDeferred);

			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.BulkLiquorDeferred;
			Assert("BulkLiquorTaxDeferred", ehp.BulkLiquorTaxDeferred);
			Assert("TaxToBeDeferred", !ehp.TaxToBeDeferred);
		}

		public void TestDeferredTaxToBePaidByEFT()
		{
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTaxWithEFT;
			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals(true, ehp.DeferredTaxToBePaidByEFT);

			Declaration.US_TaxDeferIndicator = "";
			AssertEquals(false, ehp.DeferredTaxToBePaidByEFT);
		}

		public void TestIORAlcoholLicence()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("", ehp.IORAlcoholLicence);

			OrgHeader importerOfRecord = Factory.New<OrgHeader>();
			OrgCusCode aLCLicense = importerOfRecord.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AlcoholImportLicence, "U3894HW");
			Declaration.IOROrgPK = importerOfRecord.PK;
			AssertEquals("U3894HW", ehp.IORAlcoholLicence);
		}

		public void TestBrokerFileNo()
		{
			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			ZString expectedResult = Declaration.JE_DeclarationReference;
			AssertEquals("Broker File No (Declaration reference)", expectedResult, ehp.BrokerFileNo);

			Declaration.JE_OwnerRef = "MWB-00395/15";
			expectedResult = declaration.JE_DeclarationReference + " / Ref: MWB-00395/15";
			AssertEquals("Broker File No (with Owner reference)", expectedResult, ehp.BrokerFileNo);

			DataRegistry.Business.USCustomsDataRegistry.Instance.DefaultOwnerRefOn7501.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			expectedResult = declaration.JE_DeclarationReference;
			AssertEquals("Broker File No should now not print with Owner reference", expectedResult, ehp.BrokerFileNo);
		}

		public void TestImportingCarrier()
		{
			var ensEntry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("For merchandise arriving in the U.S. by means of transportation other than vessel or air, leave blank.", "", ehp.ImportingCarrier);

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SUDU");

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_VesselName = "HYOGO MARU";
			Declaration.JE_OH_ShippingLine = shippingLine.PK;
			AssertEquals("For merchandise arriving in the U.S. by vessel, record the name of the vessel that transported the merchandise from the foreign port of lading to the first U.S. port of unlading. Do not record the vessel identifier code in lieu of the vessel name.", "HYOGO MARU (SUDU)", ehp.ImportingCarrier);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_VesselName = "";
			Declaration.JE_VoyageFlightNo = "SQ539";
			declaration.US_UI_NKCarrierSCAC = "SQ";
			AssertEquals("For merchandise arriving in the U.S. by air, record the two digit IATA alpha code corresponding to the name of the airline.", "SQ", ehp.ImportingCarrier);

			Declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			AssertEquals("For hand caarried, master bill will be dafault to 'HANDCARRIED' if empty, therefor, ImportingCarrier will default from declaration carrier code. ", "SUDU", ehp.ImportingCarrier);

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			Declaration.JE_MasterBill = "FTZ295";
			Declaration.US_FTZNo = "FTZ296";
			Declaration.US_UI_NKCarrierSCAC = "";
			Declaration.JE_VoyageFlightNo = "FR001";
			AssertEquals(@"For merchandise arriving in the customs territory from a U.S. Foreign Trade Zone (FTZ),
insert 'FTZ' followed by the FTZ number. Use the following format: FTZ NNNN", "FTZ 296", ehp.ImportingCarrier);
		}

		public void TestManufacturerID()
		{
			CusEntryHeader ensEntry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			OrgHeader manufacturer = Factory.New<OrgHeader>();
			manufacturer.FillWithValidTestData();
			manufacturer.OH_FullName = "Manufacturer";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");
			ensEntry.RandomHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals("Manufacturer", "XYBEREQU6LON", ehp.ManufacturerID);

			JobComInvoiceHeader invoice1 = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			OrgHeader manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.FillWithValidTestData();
			manufacturer2.OH_FullName = "Manufacturer2";
			manufacturer2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "123EREQU6LON");

			JobComInvoiceHeader invoice2 = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("ManufacturerID", USConstants.MultipleValueIndicator, ehp.ManufacturerID);
		}

		public void TestDeclarantName()
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			JobDeclaration declaration = SetUpMergedInvoices();
			declaration.JE_GS_NKCusAgent = TestBroker.GS_Code;
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Declarant Name should default to formatted broker", ContactNameHelper.GetFormattedName(TestBroker.GS_FullName, true), ehp.DeclarantName);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Declarant Name should now pick up current user", ContactNameHelper.GetFormattedName(Env.CurrentUser.FullName, true), ehp.DeclarantName);

			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("fall back to declaration broker", TestBroker.GS_FullName, entry.DeclarantName);
			}

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			var broker1 = Factory.New<GlbStaff>();
			broker1.GS_Code = "III";
			broker1.GS_FullName = "ANOTHER BROKER";
			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, broker1.PK.ToGuid());
			AssertEquals("BROKER, ANOTHER", ehp.DeclarantName);
		}

		public void TestDeclarantTitle()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Pre-condition - By default AttorneyInFact should be true", true, entry.IsAttorneyInFact);
			AssertEquals("Declarant should show Atty in fact when registy on", "ATTY-IN-FACT", ehp.DeclarantTitle);

			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, entry.IsAttorneyInFact);
			AssertEquals("Test doesn't break when no broker entered", "", ehp.DeclarantTitle);

			declaration.JE_GS_NKCusAgent = "XX";
			AssertEquals("Test doesn't break when crap is entered", "", ehp.DeclarantTitle);

			declaration.JE_GS_NKCusAgent = TestBroker.GS_Code;
			AssertEquals("Declarant Title should default to broker", TestBroker.GS_Title, ehp.DeclarantTitle);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Declarant Title should now pick up current user", Env.CurrentUser.Title, ehp.DeclarantTitle);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			var broker1 = Factory.New<GlbStaff>();
			broker1.GS_Code = "III";
			broker1.GS_Title = "TT1";
			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, broker1.PK.ToGuid());
			AssertEquals("TT1", ehp.DeclarantTitle);
		}

		public void TestBrokerFilerName()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Pre-condition - By default AttorneyInFact should be true", true, entry.IsAttorneyInFact);

			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Broker Filer Name should default to current company only", GlbCompany.CurrentCompany.OrgProxy.OH_FullName, ehp.BranchName);

			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals("Broker Filer Name should default to branch", declaration.Branch.GB_BranchName, ehp.BranchName);
		}

		public void TestBrokerFilerAddress()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			ZString addressExpected = declaration.Branch.OrgProxy.MainAddress.OA_Address1 + ", " + declaration.Branch.OrgProxy.MainAddress.OA_Address2 + " " + declaration.Branch.OrgProxy.MainAddress.OA_City + " " + declaration.Branch.OrgProxy.MainAddress.OA_PostCode;
			AssertEquals("Broker Filer Address should default to current company", addressExpected.Trim(), ehp.BranchAddress);

			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals("UNIT 3, 480 NUDGEE ROAD, HENDRA ROAD, QLD 4011 Brisbane", ehp.BranchAddress);
		}

		public void TestBrokerFilerPhoneNo()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			declaration.Branch.OrgProxy.MainAddress.OA_Phone = "PHONE";
			declaration.Branch.OrgProxy.MainAddress.OA_Fax = "FAX";
			declaration.Branch.GB_Phone = "BRANCH PHONE";
			declaration.Branch.GB_Fax = "BRANCH FAX";
			AssertEquals("PHONE: PHONE  FAX: FAX", ehp.BranchPhone);

			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals("PHONE: BRANCH PHONE  FAX: BRANCH FAX", ehp.BranchPhone);
		}

		public void TestBlock24ReferenceNumber()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			OrgHeader notifyParty = Factory.New<OrgHeader>();
			notifyParty.FillWithValidTestData();
			notifyParty.OH_FullName = "Importer";
			OrgCusCode notifyPartyEINCode = notifyParty.CustomsCodes.AddNew();
			notifyPartyEINCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			notifyPartyEINCode.OK_CustomsRegNo = "12-3456789";

			declaration.JE_OH_NotifyParty = notifyParty.PK;
			declaration.IOROrgPK = notifyParty.PK;
			declaration.JE_OH_Importer = notifyParty.PK;
			declaration.IORWrapper.ZO_NPID = "333";

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals("Block24 reference number", "12-3456789", ehp.Block24ReferenceNumber);

			notifyParty.CustomsCodes.RemoveAll();
			OrgCusCode notifyPartyCBPCode = notifyParty.CustomsCodes.AddNew();
			notifyPartyCBPCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			notifyPartyCBPCode.OK_CustomsRegNo = "YYDDPP-12345";
			AssertEquals("Block24 reference number", "YYDDPP-12345", ehp.Block24ReferenceNumber);

			notifyParty.CustomsCodes.RemoveAll();
			OrgCusCode notifyPartySSNCode = notifyParty.CustomsCodes.AddNew();
			notifyPartySSNCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			notifyPartySSNCode.OK_CustomsRegNo = "555-33-1234";
			AssertEquals("We don't print SSN on customs side", "", ehp.Block24ReferenceNumber);

			notifyParty.CustomsCodes.RemoveAll();
			AssertEquals(ZString.Empty, ehp.Block24ReferenceNumber);

			declaration.JE_OH_NotifyParty = ZGuid.Empty;
			AssertEquals("Taken from IOR", "333", ehp.Block24ReferenceNumber);
		}

		public void TestBlock24ReferenceNumberForACE()
		{
			var declaration = SetUpMergedInvoices();
			declaration.JE_ApplicationCode = "ACE";

			var entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_FullName = "Importer";

			var importerEINCode = importer.CustomsCodes.AddNew();
			importerEINCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			importerEINCode.OK_CustomsRegNo = "12-3456789";
			declaration.JE_OH_NotifyParty = importer.PK;

			AssertEquals("ACE should print EIN of Notify party", "12-3456789", ehp.Block24ReferenceNumber);
		}

		public void TestTotalEnteredValue()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 15000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceNumber = "Inv-001";

			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;

			JobComInvoiceLine invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 5000m;

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 3000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_InvoiceNumber = "Inv-002";

			JobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 3000m;

			AssertEquals("Invoice 1 entered total", 15000m, invoice1.InvoiceLineTotal);
			AssertEquals("Invoice 2 entered total", 3000m, invoice2.InvoiceLineTotal);

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader cusEntry = declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(cusEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Total Entered Value", 18000m, ehp.TotalEnteredValue);
		}

		[TestDate(2009, 6, 1)]
		public void TestTotalOtherFees()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			invoiceLine.JI_Tariff = "2402103030";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 1360m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Total Excise Fee", 1828m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals("Summary Fee 1", "499 499 Desc from DB", ehp.SummaryFeeDesc1);
			AssertEquals("Total MPF", 31.5m, ehp.SummaryFee1);
			AssertEquals("Two charges expected", 2, entry.Charges.Count);
			AssertEquals("Other Fees should not include excisable charges", 31.5m, ehp.TotalOtherFees);
		}

		[TestDate(2009, 6, 1)]
		public void TestTotalOtherFeesForWarehouseEntryType21()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_IsHMFApplicable = "Y";
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			invoiceLine.JI_Tariff = "2402103030";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 1360m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Total Excise Fee", 1828m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals("Summary Fee 1", "499 499 Desc from DB", ehp.SummaryFeeDesc1);
			AssertEquals("Total MPF", 31.5m, ehp.SummaryFee1);
			AssertEquals("Summary Fee 2", "501 501 Desc from DB", ehp.SummaryFeeDesc2);
			AssertEquals("Total HMF", 18.75m, ehp.SummaryFee2);
			AssertEquals("Three charges expected", 3, entry.Charges.Count);
			AssertEquals("Other Fees should only include HMF charges", 18.75m, ehp.TotalOtherFees);
		}

		[TestDate(2014, 6, 5)]
		public void TestTotalOtherFeesForReWarehouseEntryType22()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			invoiceLine.JI_Tariff = "2402103030";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 1360m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entry = invoiceLine.CusEntryLine.Header;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Summary Fee 1", "499 499 Desc from DB", ehp.SummaryFeeDesc1);
			AssertEquals("Total MPF", 51.96m, ehp.SummaryFee1);
			AssertEquals("Summary Fee 2", "501 501 Desc from DB", ehp.SummaryFeeDesc2);
			AssertEquals("Total HMF", 18.75m, ehp.SummaryFee2);
			AssertEquals("Nothing to pay for 22", 0m, ehp.TotalOtherFees);
		}

		[TestDate(2009, 6, 1)]
		public void TestTotalOtherFeesAndSummaryBlockIncludeADandCVD()
		{
			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C475819005";
			cvdCase.U5_ISOCountryCode = "IT";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "19021920";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 0.01m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_Tariff = "2402103030";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 1360m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 22474m;
			invoiceLine2.JI_Tariff = "1902192030";
			invoiceLine2.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine2.US_SPI = "";
			invoiceLine2.JI_CustomsQuantity = 12351m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.US_ADD_NA = true;
			invoiceLine2.US_CVDCaseNo = "C475819005";
			invoiceLine2.US_CVDDepositRateIndicator = "1";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Total Excise Fee", 1828m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals("Summary Fee 1", "013 CVD", ehp.SummaryFeeDesc1);
			AssertEquals("Total CVD", 224.74m, ehp.SummaryFee1);
			AssertEquals("Summary Fee 2", "499 499 Desc from DB", ehp.SummaryFeeDesc2);
			AssertEquals("Total MPF", 78.7m, ehp.SummaryFee2);
			AssertEquals("Two charges expected", 2, entry.Charges.Count);
			AssertEquals("Other Fees should not include excisable charges but should inlcude AD & CVD", 303.44m, ehp.TotalOtherFees);
		}

		public void TestSCACAndMBillNumber()
		{
			JobDeclaration declaration = SetUpBills();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals("APLUMasterBill", ehp.SCACAndMBillNumber);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			ehp = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals("MasterBill", ehp.SCACAndMBillNumber);
		}

		public void TestFirstBillDetails()
		{
			JobDeclaration declaration = SetUpBills();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals("APLUMasterBill", ehp.SCACAndMBillNumber);
			AssertEquals(date1.Date, ehp.FirstBillITDate);
			AssertEquals("IT12345", ehp.FirstBillITNO);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			ehp = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals(ZDateTime.Empty, ehp.FirstBillITDate);
			AssertEquals("", ehp.FirstBillITNO);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			ehp = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals("MasterBill", ehp.SCACAndMBillNumber);
		}

		public void TestFirstBillITNO()
		{
			JobDeclaration declaration = SetUpBills();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var entryHeaderForPrint = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("IT12345", entryHeaderForPrint.FirstBillITNO);

			Bill masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MasterBill 2";
			masterBill2.US_UI_NKBillIssuerSCAC = "APLU";

			Bill houseBill1_MB2 = declaration.Bills.AddNew();
			houseBill1_MB2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1_MB2.CU_CU_ParentBill = masterBill2.PK;
			houseBill1_MB2.CU_BillNum = "HouseBill1";
			houseBill1_MB2.US_UI_NKBillIssuerSCAC = "APLU";
			houseBill1_MB2.ITNumber = "V1245678910";
			AssertEquals("IT12345", entryHeaderForPrint.FirstBillITNO);
		}

		public void TestEntryPrintBills()
		{
			var declaration = SetUpBills();
			var houseBill2 = declaration.Bills.FindByBillNumberAndType("HouseBill2", Enterprise.Customs.Business.BillTypeList.Codes.HouseBill);
			houseBill2.ITAndSplitDetails.AddNew().US_ITNumber = "V12345678";
			houseBill2.ITAndSplitDetails.AddNew().US_ITNumber = "987654";

			var entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("IT1234", ehp.EntryPrintBills[0].ITNO);
			AssertEquals("HouseBill2", ehp.EntryPrintBills[0].HouseBill);
			AssertEquals("MasterBill", ehp.EntryPrintBills[0].MasterBill);
			AssertEquals("", ehp.EntryPrintBills[0].SubHouseBill);

			AssertEquals("V12345678", ehp.EntryPrintBills[1].ITNO);
			AssertEquals("HouseBill2", ehp.EntryPrintBills[1].HouseBill);
			AssertEquals("MasterBill", ehp.EntryPrintBills[1].MasterBill);

			AssertEquals("987654", ehp.EntryPrintBills[2].ITNO);
			AssertEquals("HouseBill2", ehp.EntryPrintBills[2].HouseBill);
			AssertEquals("MasterBill", ehp.EntryPrintBills[2].MasterBill);

			AssertEquals("IT12345", ehp.EntryPrintBills[3].ITNO);
			AssertEquals("HouseBill1", ehp.EntryPrintBills[3].HouseBill);
			AssertEquals("MasterBill", ehp.EntryPrintBills[3].MasterBill);

			houseBill2.ITAndSplitDetails.RemoveAndDeleteAll();
			houseBill2.US_SESplitShip = true;
			var split1 = houseBill2.ITAndSplitDetails.AddNew();
			split1.US_FlightNumber = "0613";
			split1.US_CarrierCode = "APLU";

			var split2 = houseBill2.ITAndSplitDetails.AddNew();
			split2.US_FlightNumber = "641P";
			split2.US_CarrierCode = "OTT1";

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals(ZString.Empty, ehp.EntryPrintBills[0].ITNO);
			AssertEquals("HouseBill2", ehp.EntryPrintBills[0].HouseBill);
			AssertEquals("MasterBill", ehp.EntryPrintBills[0].MasterBill);
			AssertEquals("", ehp.EntryPrintBills[0].SubHouseBill);

			AssertEquals("IT12345", ehp.EntryPrintBills[1].ITNO);
			AssertEquals("HouseBill1", ehp.EntryPrintBills[1].HouseBill);
			AssertEquals("MasterBill", ehp.EntryPrintBills[1].MasterBill);
			AssertEquals("SubHouseBill", ehp.EntryPrintBills[1].SubHouseBill);
		}

		public void TestEffectiveUltimateConsigneeCustomsRegNo()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			OrgCusCode ultimateConsigneeEINCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeEINCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			ultimateConsigneeEINCode.OK_CustomsRegNo = "12-3456789";
			entry.RandomHeader.JZ_OH_Buyer = ultimateConsignee.PK;
			declaration.IOROrgPK = ultimateConsignee.PK;
			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			AssertEquals("ultimateConsignee is Importer of Record", USConstants.Same, ehp.EffectiveUltimateConsigneeCustomsRegNo);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals("Ultimate Consignee should show value for ConsumptionFTZ entry : EIN", "12-3456789", ehp.EffectiveUltimateConsigneeCustomsRegNo);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			OrgHeader importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.FillWithValidTestData();
			importerOfRecord.OH_FullName = "IOR";
			declaration.IOROrgPK = importerOfRecord.PK;

			AssertEquals("Ultimate Consignee differs from Importer of Record : EIN", "12-3456789", ehp.EffectiveUltimateConsigneeCustomsRegNo);

			ultimateConsignee.CustomsCodes.RemoveAll();
			OrgCusCode ultimateConsigneeSSNCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeSSNCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			ultimateConsigneeSSNCode.OK_CustomsRegNo = "123-12-1234";

			AssertEquals("Ultimate Consignee differs from Importer of Record : SSN (We don't print SSN on customs side)", "", ehp.EffectiveUltimateConsigneeCustomsRegNo);

			ultimateConsignee.CustomsCodes.RemoveAll();
			OrgCusCode ultimateConsigneeCBPCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeCBPCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			ultimateConsigneeCBPCode.OK_CustomsRegNo = "YYDDPP-44999";

			AssertEquals("Ultimate Consignee differs from Importer of Record : CBP", "YYDDPP-44999", ehp.EffectiveUltimateConsigneeCustomsRegNo);
		}

		public void TestImporterOfRecordCustomsRegNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC";
			declaration.IOROrgPK = importer.PK;
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "123-12-1234");

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("We don't print SSN on customs side", "", ehp.ImporterOfRecordCustomsRegNo);

			declaration.PrintSocialSecurityNumberOnDocument = true;
			AssertEquals("We print SSN on customs side only through XXX (SSN) menu", "123-12-1234", ehp.ImporterOfRecordCustomsRegNo);
		}

		public void TestUltimateState()
		{
			JobDeclaration declaration = SetUpMergedInvoice();

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Entry - US_DestinationState", "AL", entry.US_DestinationState);
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			OrgHeader importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.FillWithValidTestData();
			importerOfRecord.OH_FullName = "IOR";
			importerOfRecord.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789");
			entry.RandomHeader.JZ_OH_Buyer = importerOfRecord.PK;
			declaration.IOROrgPK = importerOfRecord.PK;
			declaration.JE_OA_ConsigneeAddress = importerOfRecord.MainAddress.PK;

			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, ehp.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("Ultimate Consignee State should show destination state when Consignee & importer is the same", "AL", ehp.EffectiveUltimateConsigneeState);
			AssertEquals("State of ultimate destination should only print when different from Consignee", "", ehp.UltimateState);

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			ultimateConsignee.Addresses[0].OA_State = "NY";
			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertNotEquals("EffectiveUltimateConsignee is now different from IOR", USConstants.Same, ehp.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("Ultimate Consignee State", "NY", ehp.EffectiveUltimateConsigneeState);
			AssertEquals("State of ultimate destination should only print when different from Consignee", "AL", ehp.UltimateState);
		}

		public void TestWarehouseEntryNo()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			Declaration.US_WHSEntryFilerCode = "XJ5";
			Declaration.US_WHSEntryNumber = "17738949";
			CusEntryHeader ensEntry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("WarehouseEntryNo - formatted", "XJ5-1773894-9", ehp.WarehouseEntryNo);

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("WarehouseEntryNo - should not print for non Ex-Warehouse types", "", ehp.WarehouseEntryNo);
		}

		public void TestMissingDoc1()
		{
			Declaration.US_MissingDocument1 = MissingDocumentList.Codes.CBPF3291;
			CusEntryHeader ensEntry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Missing Document 1 s/b code 11", MissingDocumentList.Codes.CBPF3291, ehp.MissingDoc1);

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Missing Document 1 now not applicable", "", ehp.MissingDoc1);
		}

		public void TestMissingDoc2()
		{
			Declaration.US_MissingDocument2 = MissingDocumentList.Codes.CommInv;
			CusEntryHeader ensEntry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Missing Document 2 s/b code 01", MissingDocumentList.Codes.CommInv, ehp.MissingDoc2);

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Missing Document 2 now not applicable", "", ehp.MissingDoc2);
		}

		public void TestUniqueCountryOfOrigin()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 328m;
			invoiceLine1.JI_Tariff = "1509902000";
			invoiceLine1.JI_CustomsQuantity = 41m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders[0];

			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals(ZString.Empty, ehp.UniqueCountryOfOrigin);

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 364m;
			invoiceLine2.JI_Tariff = "1902192030";
			invoiceLine2.JI_CustomsQuantity = 180m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals(ZString.Empty, ehp.UniqueCountryOfOrigin);

			invoice.US_UC_NKCountryOfOrigin = "AU";
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("AU", ehp.UniqueCountryOfOrigin);
			invoice.US_UC_NKCountryOfOrigin = ZString.Empty;

			invoiceLine1.US_UC_NKCountryOfOrigin = "AD";
			invoiceLine2.US_UC_NKCountryOfOrigin = "IT";
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals(USConstants.MultipleValueIndicator, ehp.UniqueCountryOfOrigin);

			invoiceLine1.US_UC_NKCountryOfOrigin = "IT";
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("IT", ehp.UniqueCountryOfOrigin);
		}

		public void TestUniquePortOfLading()
		{
			var declaration = SetUpMergedInvoice();
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var invoiceLine1 = declaration.Invoices[0].InvoiceLines[0];
			invoiceLine1.US_SchDLoading = "52000";

			var invoiceLine2 = declaration.Invoices[0].InvoiceLines[1];
			invoiceLine2.US_SchDLoading = "52002";
			Assert(declaration.MergeManager.RequiresMerge);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals(USConstants.MultipleValueIndicator, ehp.UniquePortOfLading);

			invoiceLine2.US_SchDLoading = "52000";
			Assert(declaration.MergeManager.RequiresMerge);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("52000", ehp.UniquePortOfLading);
		}

		public void TestReconStatement()
		{
			CusEntryHeader ensEntry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Recon Statment should be blank", "", ehp.ReconStatement);

			Declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.Value9802Recon;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Recon Statment", "Flagged For Recon - 005 - Value/9802 Recon.", ehp.ReconStatement);

			Declaration.US_OtherReconIndicator = "";
			Declaration.US_NAFTAReconIndicator = true;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Recon Statment when NAFTA", "Flagged For Recon - FTA", ehp.ReconStatement.Trim());

			Declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(ensEntry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Recon Statment if ever both are used", "Flagged For Recon - FTA   Flagged For Recon - 006 - Class/9802 Recon.", ehp.ReconStatement);

			Declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.NotApplicable;
			Declaration.US_NAFTAReconIndicator = false;
			AssertEquals("Recon Statment should be blank when issue code is NA", "", ehp.ReconStatement);
		}

		[TestDate(2009, 2, 1)]
		public void TestHMFDeMinimisFee()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = "SEA";
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDLoading = "42879";
			declaration.US_SchDArrival = "3901";
			declaration.US_SchDEntry = "8888";
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices[0];
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 250m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_LinePrice = 250m;
			invoiceLine.JI_Tariff = "2001.10.0000";//duty is calculated off from customs quantity and MPF is exempt
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";//this makes MPF exempt
			invoiceLine.JI_InvoiceQuantity = 250m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_CustomsQuantity = 250m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("Duty is calculated", ZDecimal.Zero, entry.TotalDutyAmount);
			AssertEquals("Tax is calculated", ZDecimal.Zero, entry.TotalEstimatedTax);

			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Calculated HMF Fee at line level", 0.31m, entry.MergedLines[0].Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF).Round(2));
			AssertEquals("Adjusted HMF Fee for entry", 0m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals("Summary fee desc when HMF de minimis required", "501 HMF (De Minimus)         $0.00", ehp.SummaryFeeDesc1);
			AssertEquals("Total Fees should include HMF charge", .31m, ehp.TotalOtherFees);
		}

		[TestDate(2009, 6, 1)]
		public void TestOtherFeesSummaryBlock()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "C475819005";
			uscCase.U5_ISOCountryCode = "IT";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "1902192030";
			var rate2 = uscCase.CaseRates.AddNew();
			rate2.U6_AdValoremRate = 0.02m;
			rate2.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_RN_NKDefaultOrigin = "IT";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 328m;
			invoiceLine1.JI_Tariff = "1509902000";
			invoiceLine1.JI_CustomsQuantity = 41m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 364m;
			invoiceLine2.JI_Tariff = "1902192030";
			invoiceLine2.JI_CustomsQuantity = 180m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 1324m;
			invoiceLine3.JI_Tariff = "1509102000";
			invoiceLine3.JI_CustomsQuantity = 247m;
			invoiceLine3.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 349m;
			invoiceLine4.JI_Tariff = "2103204020";
			invoiceLine4.JI_CustomsQuantity = 164m;
			invoiceLine4.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 196m;
			invoiceLine5.JI_Tariff = "2103909091";
			invoiceLine5.JI_CustomsQuantity = 196m;
			invoiceLine5.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine6 = declaration.InvoiceLines.AddNew();
			invoiceLine6.JI_LinePrice = 22474m;
			invoiceLine6.JI_Tariff = "1902192030";
			invoiceLine6.JI_CustomsQuantity = 12351m;
			invoiceLine6.JI_CustomsUnitQty = "KG";
			invoiceLine6.US_ADD_NA = true;
			invoiceLine6.US_CVDCaseNo = "C475819005";
			invoiceLine6.US_CVDDepositRateIndicator = "1";

			JobComInvoiceLine invoiceLine7 = declaration.InvoiceLines.AddNew();
			invoiceLine7.JI_LinePrice = 8550m;
			invoiceLine7.JI_Tariff = "1902192030";
			invoiceLine7.JI_CustomsQuantity = 4077m;
			invoiceLine7.JI_CustomsUnitQty = "KG";
			invoiceLine7.US_ADD_NA = true;
			invoiceLine7.US_CVDCaseNo = "C475819005";
			invoiceLine7.US_CVDDepositRateIndicator = "1";

			JobComInvoiceLine invoiceLine8 = declaration.InvoiceLines.AddNew();
			invoiceLine8.JI_LinePrice = 942m;
			invoiceLine8.JI_Tariff = "1902112030";
			invoiceLine8.JI_CustomsQuantity = 240m;
			invoiceLine8.JI_CustomsUnitQty = "KG";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Summary Fee Desc 1", "013 CVD", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total CVD)", 620.48m, ehp.SummaryFee1);
			AssertEquals("Summary Fee 2", "499 499 Desc from DB", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total MPF)", 72.51m, ehp.SummaryFee2);
			AssertEquals("Summary Fee 3", "501 501 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total HMF)", 43.18m, ehp.SummaryFee3);
			AssertEquals("Total Other Fees should inlcude CVD", 736.17m, ehp.TotalOtherFees);
			AssertEquals("Total Other", 736.17m, ehp.TotalOther);
		}

		public void TestTotalDutyAmt()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_RN_NKDefaultOrigin = "IT";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 328m;
			invoiceLine1.JI_Tariff = "1509902000";
			invoiceLine1.JI_CustomsQuantity = 41m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 364m;
			invoiceLine2.JI_Tariff = "1902192030";
			invoiceLine2.JI_CustomsQuantity = 180m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 1324m;
			invoiceLine3.JI_Tariff = "1509102000";
			invoiceLine3.JI_CustomsQuantity = 247m;
			invoiceLine3.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 349m;
			invoiceLine4.JI_Tariff = "2103204020";
			invoiceLine4.JI_CustomsQuantity = 164m;
			invoiceLine4.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 196m;
			invoiceLine5.JI_Tariff = "2103909091";
			invoiceLine5.JI_CustomsQuantity = 196m;
			invoiceLine5.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine6 = declaration.InvoiceLines.AddNew();
			invoiceLine6.JI_LinePrice = 22474m;
			invoiceLine6.JI_Tariff = "1902192030";
			invoiceLine6.JI_CustomsQuantity = 12351m;
			invoiceLine6.JI_CustomsUnitQty = "KG";
			invoiceLine6.US_ADD_NA = true;
			invoiceLine6.US_CVDCaseNo = "C475819005";
			invoiceLine6.US_CVDDepositRateIndicator = "1";

			JobComInvoiceLine invoiceLine7 = declaration.InvoiceLines.AddNew();
			invoiceLine7.JI_LinePrice = 8550m;
			invoiceLine7.JI_Tariff = "1902192030";
			invoiceLine7.JI_CustomsQuantity = 4077m;
			invoiceLine7.JI_CustomsUnitQty = "KG";
			invoiceLine7.US_ADD_NA = true;
			invoiceLine7.US_CVDCaseNo = "C475819005";
			invoiceLine7.US_CVDDepositRateIndicator = "1";

			JobComInvoiceLine invoiceLine8 = declaration.InvoiceLines.AddNew();
			invoiceLine8.JI_LinePrice = 942m;
			invoiceLine8.JI_Tariff = "1902112030";
			invoiceLine8.JI_CustomsQuantity = 240m;
			invoiceLine8.JI_CustomsUnitQty = "KG";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Duty Line 1", 2.05m, ehp.EntryPrintLines[0].DutyAmount);
			AssertEquals("Duty Line 2", 0m, ehp.EntryPrintLines[1].DutyAmount);
			AssertEquals("Duty Line 3", 12.35m, ehp.EntryPrintLines[2].DutyAmount);
			AssertEquals("Duty Line 4", 40.48m, ehp.EntryPrintLines[3].DutyAmount);
			AssertEquals("Duty Line 5", 12.54m, ehp.EntryPrintLines[4].DutyAmount);
			AssertEquals("Duty Line 6", 0m, ehp.EntryPrintLines[5].DutyAmount);
			AssertEquals("Duty Line 7", 0m, ehp.EntryPrintLines[6].DutyAmount);
			AssertEquals("Duty Line 8", 0m, ehp.EntryPrintLines[7].DutyAmount);
			AssertEquals("Total Duty", 67.42m, ehp.TotalDutyAmt);
		}

		public void TestTotalDutyAmtWhenWarehouseEntryType21()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_RN_NKDefaultOrigin = "IT";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 328m;
			invoiceLine1.JI_Tariff = "1509902000";
			invoiceLine1.JI_CustomsQuantity = 41m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 364m;
			invoiceLine2.JI_Tariff = "1902192030";
			invoiceLine2.JI_CustomsQuantity = 180m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 1324m;
			invoiceLine3.JI_Tariff = "1509102000";
			invoiceLine3.JI_CustomsQuantity = 247m;
			invoiceLine3.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 349m;
			invoiceLine4.JI_Tariff = "2103204020";
			invoiceLine4.JI_CustomsQuantity = 164m;
			invoiceLine4.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 196m;
			invoiceLine5.JI_Tariff = "2103909091";
			invoiceLine5.JI_CustomsQuantity = 196m;
			invoiceLine5.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine6 = declaration.InvoiceLines.AddNew();
			invoiceLine6.JI_LinePrice = 22474m;
			invoiceLine6.JI_Tariff = "1902192030";
			invoiceLine6.JI_CustomsQuantity = 12351m;
			invoiceLine6.JI_CustomsUnitQty = "KG";
			invoiceLine6.US_ADD_NA = true;
			invoiceLine6.US_CVDCaseNo = "C475819005";
			invoiceLine6.US_CVDDepositRateIndicator = "1";

			JobComInvoiceLine invoiceLine7 = declaration.InvoiceLines.AddNew();
			invoiceLine7.JI_LinePrice = 8550m;
			invoiceLine7.JI_Tariff = "1902192030";
			invoiceLine7.JI_CustomsQuantity = 4077m;
			invoiceLine7.JI_CustomsUnitQty = "KG";
			invoiceLine7.US_ADD_NA = true;
			invoiceLine7.US_CVDCaseNo = "C475819005";
			invoiceLine7.US_CVDDepositRateIndicator = "1";

			JobComInvoiceLine invoiceLine8 = declaration.InvoiceLines.AddNew();
			invoiceLine8.JI_LinePrice = 942m;
			invoiceLine8.JI_Tariff = "1902112030";
			invoiceLine8.JI_CustomsQuantity = 240m;
			invoiceLine8.JI_CustomsUnitQty = "KG";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Duty Line 1 - should still print", 2.05m, ehp.EntryPrintLines[0].DutyAmount);
			AssertEquals("Duty Line 2", 0m, ehp.EntryPrintLines[1].DutyAmount);
			AssertEquals("Duty Line 3 - should still print", 12.35m, ehp.EntryPrintLines[2].DutyAmount);
			AssertEquals("Duty Line 4 - should still print", 40.48m, ehp.EntryPrintLines[3].DutyAmount);
			AssertEquals("Duty Line 5 - should still print", 12.54m, ehp.EntryPrintLines[4].DutyAmount);
			AssertEquals("Duty Line 6", 0m, ehp.EntryPrintLines[5].DutyAmount);
			AssertEquals("Duty Line 7", 0m, ehp.EntryPrintLines[6].DutyAmount);
			AssertEquals("Duty Line 8", 0m, ehp.EntryPrintLines[7].DutyAmount);
			AssertEquals("Total Duty for Warehouse entry should be 0", 0m, ehp.TotalDutyAmt);
		}

		[TestDate(2009, 6, 1)]
		public void TestSummaryBlockOverflow()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A475818001";
			addCase.U5_ISOCountryCode = "IT";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "1902192030";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 0.19m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C475819005";
			cvdCase.U5_ISOCountryCode = "IT";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "1902192030";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 0.01m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_RN_NKDefaultOrigin = "IT";
			invoice.US_UC_NKCountryOfExport = "IT";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 22474m;
			invoiceLine1.JI_Tariff = "1902192030";
			invoiceLine1.JI_CustomsQuantity = 12351m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.US_ADD_NA = true;
			invoiceLine1.US_CVDCaseNo = "C475819005";
			invoiceLine1.US_CVDDepositRateIndicator = "1";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 8550m;
			invoiceLine2.JI_Tariff = "1902192030";
			invoiceLine2.JI_CustomsQuantity = 4077m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.US_CVD_NA = true;
			invoiceLine2.US_ADDCaseNo = "A475818001";
			invoiceLine2.US_ADDDepositRateIndicator = "1";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("SummaryBlockOverflow", "", ehp.SummaryBlockOverflow);
			AssertEquals("Summary Fee Desc 1", "012 AD", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total AD)", 1624.50m, ehp.SummaryFee1);
			AssertEquals("Summary Fee Desc 2", "013 CVD", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total CVD)", 224.74m, ehp.SummaryFee2);
			AssertEquals("Summary Fee 3", "499 499 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total MPF)", 65.16m, ehp.SummaryFee3);
			AssertEquals("Summary Fee 4", "501 501 Desc from DB", ehp.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total HMF)", 38.78m, ehp.SummaryFee4);
			AssertEquals("Total Other Fees", 1953.18m, ehp.TotalOtherFees);

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 6000m;
			invoiceLine3.JI_CustomsQuantity = 150m;
			invoiceLine3.JI_CustomsUnitQty = "KG";
			invoiceLine3.JI_Tariff = "0804.40.0010";

			declaration.DoMerge();
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Avocado, 100m);
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("SummaryBlockOverflow", "+", ehp.SummaryBlockOverflow);
			AssertEquals("Summary Fee Desc 1", "012 AD", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total AD)", 1624.50m, ehp.SummaryFee1);
			AssertEquals("Summary Fee Desc 2", "013 CVD", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total CVD)", 224.74m, ehp.SummaryFee2);
			AssertEquals("Summary Fee Desc 3", "107 107 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total Avocado Fee)", 100m, ehp.SummaryFee3);
			AssertEquals("Summary Fee 4", "499 499 Desc from DB", ehp.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total MPF)", 77.76m, ehp.SummaryFee4);
			AssertEquals("Summary Fee 5", "501 501 Desc from DB", ehp.SummaryFeeDesc5);
			AssertEquals("SummaryFee5 (Total HMF)", 46.28m, ehp.SummaryFee5);
			AssertEquals("Total Other Fees should inlcude new avocado fee", 2073.28m, ehp.TotalOtherFees);
		}

		[TestDate(2009, 6, 1)]
		public void TestPrintOfExcessChargesWithADAndCVD()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A475818001";
			addCase.U5_ISOCountryCode = "IT";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "1902192030";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 0.19m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C475819005";
			cvdCase.U5_ISOCountryCode = "IT";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "1902192030";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 0.01m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_RN_NKDefaultOrigin = "IT";
			invoice.US_UC_NKCountryOfExport = "IT";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 22474m;
			invoiceLine1.JI_Tariff = "1902192030";
			invoiceLine1.JI_CustomsQuantity = 12351m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.US_ADD_NA = true;
			invoiceLine1.US_CVDCaseNo = "C475819005";
			invoiceLine1.US_CVDDepositRateIndicator = "1";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 8550m;
			invoiceLine2.JI_Tariff = "1902192030";
			invoiceLine2.JI_CustomsQuantity = 4077m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.US_CVD_NA = true;
			invoiceLine2.US_ADDCaseNo = "A475818001";
			invoiceLine2.US_ADDDepositRateIndicator = "1";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("SummaryBlockOverflow", "", ehp.SummaryBlockOverflow);
			AssertEquals("Summary Fee Desc 1", "012 AD", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total AD)", 1624.50m, ehp.SummaryFee1);
			AssertEquals("Summary Fee Desc 2", "013 CVD", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total CVD)", 224.74m, ehp.SummaryFee2);
			AssertEquals("Summary Fee 3", "499 499 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total MPF)", 65.16m, ehp.SummaryFee3);
			AssertEquals("Summary Fee 4", "501 501 Desc from DB", ehp.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total HMF)", 38.78m, ehp.SummaryFee4);
			AssertEquals("Total Other Fees", 1953.18m, ehp.TotalOtherFees);

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 6000m;
			invoiceLine3.JI_CustomsQuantity = 150m;
			invoiceLine3.JI_CustomsUnitQty = "KG";
			invoiceLine3.JI_Tariff = "0804.40.0010";

			declaration.DoMerge();
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Avocado, 100m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Beef, 200m);
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("SummaryBlockOverflow", "+", ehp.SummaryBlockOverflow);
			AssertEquals("Summary Fee Desc 1", "012 AD", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total AD)", 1624.50m, ehp.SummaryFee1);
			AssertEquals("Summary Fee Desc 2", "013 CVD", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total CVD)", 224.74m, ehp.SummaryFee2);
			AssertEquals("Summary Fee 3", "053 053 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total Beef Fee)", 200m, ehp.SummaryFee3);
			AssertEquals("Summary Fee 4", "107 107 Desc from DB", ehp.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total Avocado Fee)", 100m, ehp.SummaryFee4);
			AssertEquals("Total Other Fees should inlcude new avocado fee", 2273.28m, ehp.TotalOtherFees);

			AssertEquals(77.76m, ehp.EntryPrintExcessFees[0].SummaryFee);
			AssertEquals("499 499 Desc from DB", ehp.EntryPrintExcessFees[0].SummaryFeeDesc);
			AssertEquals(46.28m, ehp.EntryPrintExcessFees[1].SummaryFee);
			AssertEquals("501 501 Desc from DB", ehp.EntryPrintExcessFees[1].SummaryFeeDesc);
		}

		[TestDate(2009, 6, 1)]
		public void TestPrintOfExcessChargesNoSpecialCharge()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_RN_NKDefaultOrigin = "IT";
			invoice.US_UC_NKCountryOfExport = "IT";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 22474m;
			invoiceLine1.JI_Tariff = "1902192030";
			invoiceLine1.JI_CustomsQuantity = 12351m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.US_ADD_NA = true;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2000m;
			invoiceLine2.JI_CustomsQuantity = 150m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_Tariff = "0804.40.0010";

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 6000m;
			invoiceLine3.JI_CustomsQuantity = 150m;
			invoiceLine3.JI_CustomsUnitQty = "KG";
			invoiceLine3.JI_Tariff = "0804.40.0010";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Avocado, 100m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Beef, 200m);
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("SummaryBlockOverflow", "", ehp.SummaryBlockOverflow);
			AssertEquals("Summary Fee Desc 1", "053 053 Desc from DB", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total Beef Fee)", 200m, ehp.SummaryFee1);
			AssertEquals("Summary Fee Desc 2", "107 107 Desc from DB", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total Avocado Fee)", 100m, ehp.SummaryFee2);
			AssertEquals("Summary Fee 3", "499 499 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total MPF)", 64m, ehp.SummaryFee3);
			AssertEquals("Summary Fee 4", "501 501 Desc from DB", ehp.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total HMF)", 38.09m, ehp.SummaryFee4);
			AssertEquals("Total Other Fees should inlcude new avocado & beff fees", 402.09m, ehp.TotalOtherFees);

			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry, 300m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 400m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes, 500m);
			ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("SummaryBlockOverflow", "+", ehp.SummaryBlockOverflow);
			AssertEquals("Summary Fee Desc 1", "053 053 Desc from DB", ehp.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total Beef Fee)", 200m, ehp.SummaryFee1);
			AssertEquals("Summary Fee Desc 2", "056 056 Desc from DB", ehp.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total Cotton Fee)", 400m, ehp.SummaryFee2);
			AssertEquals("Summary Fee 3", "102 102 Desc from DB", ehp.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total Fresh Limes Fee)", 500m, ehp.SummaryFee3);
			AssertEquals("Summary Fee 4", "106 106 Desc from DB", ehp.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total Blueberry Fee)", 300m, ehp.SummaryFee4);
			AssertEquals("Total Other Fees should inlcude new blueberry, cotton & lime fees", 1602.09m, ehp.TotalOtherFees);

			AssertEquals("Excess Fee 1", "107 107 Desc from DB", ehp.EntryPrintExcessFees[0].SummaryFeeDesc);
			AssertEquals("ExcessFee1", 100m, ehp.EntryPrintExcessFees[0].SummaryFee);
			AssertEquals("Excess Fee 2", "499 499 Desc from DB", ehp.EntryPrintExcessFees[1].SummaryFeeDesc);
			AssertEquals("ExcessFee2", 64m, ehp.EntryPrintExcessFees[1].SummaryFee);
			AssertEquals("Excess Fee 3", "501 501 Desc from DB", ehp.EntryPrintExcessFees[2].SummaryFeeDesc);
			AssertEquals("ExcessFee3", 38.09m, ehp.EntryPrintExcessFees[2].SummaryFee);
		}

		[TestDate(2008, 08, 21)]
		public void TestTIBBondChg()
		{
			CreateTIBDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Duty", 697.60m, ehp.TIBTotalDuty);
			AssertEquals("TIB Bond CHG for entry summary printing", 1496.24m, ehp.TIBBondChg);
		}

		[TestDate(2009, 04, 07)]
		public void TestTIBBondChgForExeptionTariffs()
		{
			CreateTIBWithExemptTariffsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Duty", 312m, ehp.TIBTotalDuty);
			AssertEquals("TIB Charges", 25m, ehp.TIBTotalCharges);
			AssertEquals("TIB Bond CHG for exception tariffs should only be 110% of est duties & charges", 370.70m, ehp.TIBBondChg);
		}

		public void TestInvoiceSequencing()
		{
			declaration = null;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice1 = GetNewInvoiceHeader(Declaration, "A40583");
			AddNewInvoiceLine(invoice1, "9801001010");
			AddNewInvoiceLine(invoice1, "2402103030");
			AddNewInvoiceLine(invoice1, "2922292700");

			JobComInvoiceHeader invoice2 = GetNewInvoiceHeader(Declaration, "B40583");
			AddNewInvoiceLine(invoice2, "");

			JobComInvoiceHeader invoice3 = GetNewInvoiceHeader(Declaration, "C40583");
			AddNewInvoiceLine(invoice3, "");

			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			CusEntryHeader entry = (CusEntryHeader)invoice1.Entries[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("EntrySummary7501LineCollection count", 5, ehp.EntryPrintLines.Count);

			EntrySummary7501Line esl = ehp.EntryPrintLines[0];
			AssertEquals("Invoice contains sequence 1", "001/A40583", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = ehp.EntryPrintLines[1];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = ehp.EntryPrintLines[2];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);

			esl = ehp.EntryPrintLines[3];
			AssertEquals("Invoice contains sequence 4", "002/B40583", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);

			esl = ehp.EntryPrintLines[4];
			AssertEquals("Invoice contains sequence 4", "003/C40583", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);
		}

		public void Test7501PrintForHMFDeMinimus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var printBO = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Summary fee desc", "501 HMF (De Minimus)         $0.00", printBO.SummaryFeeDesc1);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			printBO = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("Summary fee desc", "501 HMF (De Minimus)         $0.00", printBO.SummaryFeeDesc1);
		}

		[TestDate(2008, 08, 21)]
		public void TestAdditionalLineSectionPrintingFlags()
		{
			CreateTIBDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TIB Duty", 697.60m, ehp.TIBTotalDuty);
			AssertEquals("TIB Bond CHG for entry summary printing", 1496.24m, ehp.TIBBondChg);

			AssertEquals("EntryHasADDCVDLines", false, ehp.EntryHasADDCVDLines);
			AssertEquals("EntryHasSecondaryTariffLines", true, ehp.EntryHasSecondaryTariffLines);
			AssertEquals("EntryHasAdditionalTariffLines", false, ehp.EntryHasAdditionalTariffLines);
			AssertEquals("EntryHasProRatedCalculation", false, ehp.EntryHasProRatedCalculation);
			AssertEquals("EntryHasAdValoremConversionCalculation ", false, ehp.EntryHasAdValoremConversionCalculation);
		}

		public void TestAdditionalLineSectionPrintingFlags_ProRated_AdValorem()
		{
			CreateWatchRepairsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("Entry print lines count", 3, ehp.EntryPrintLines.Count);
			AssertEquals("EntryHasADDCVDLines", false, ehp.EntryHasADDCVDLines);
			AssertEquals("EntryHasSecondaryTariffLines", true, ehp.EntryHasSecondaryTariffLines);
			AssertEquals("EntryHasAdditionalTariffLines", true, ehp.EntryHasAdditionalTariffLines);
			AssertEquals("EntryHasProRatedCalculation", true, ehp.EntryHasProRatedCalculation);
			AssertEquals("EntryHasAdValoremConversionCalculation ", true, ehp.EntryHasAdValoremConversionCalculation);

			EntrySummary7501Line entryLine1 = ehp.EntryPrintLines[0];
			AssertEquals("Line 1 (Parent) Tariff number", "9802.00.8068", entryLine1.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", entryLine1.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.8068", entryLine1.SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", entryLine1.SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.8068", entryLine1.SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", entryLine1.SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.8068", entryLine1.SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", entryLine1.SecondaryLine7FormattedTariff);

			AssertEquals("Entry should have pro-rated summary", true, entryLine1.ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", entryLine1.ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", entryLine1.ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", entryLine1.ProRatedLine3);

			AssertEquals("Line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("Line 1 Duty Percentage as String", "Free", entryLine1.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, entryLine1.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", entryLine1.SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 2 Duty to print on 7501", 0m, entryLine1.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", entryLine1.SecondaryLine2DutyPercentAsString);

			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, entryLine1.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine1.SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 4 Duty to print on 7501", 0m, entryLine1.SecondaryLine4DutyAmount);
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", entryLine1.SecondaryLine4DutyPercentAsString);

			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.30m, entryLine1.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine1.SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 6 Duty to print on 7501", 0m, entryLine1.SecondaryLine6DutyAmount);
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", entryLine1.SecondaryLine6DutyPercentAsString);

			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, entryLine1.SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine1.SecondaryLine7DutyPercentAsString);

			//Entry Line 2
			EntrySummary7501Line entryLine2 = ehp.EntryPrintLines[1];
			AssertEquals("Line 2 (Parent) Tariff number", "9802.00.4040", entryLine2.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", entryLine2.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.4040", entryLine2.SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", entryLine2.SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.4040", entryLine2.SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", entryLine2.SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.4040", entryLine2.SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", entryLine2.SecondaryLine7FormattedTariff);

			AssertEquals("Entry should have ad-valorem calculation", true, entryLine2.AdValoremConversionCalculation);
			AssertEquals("AV Watches", "1000 x $0.44 NO", entryLine2.AVWatches);
			AssertEquals("AVWatchesDuty", 440m, entryLine2.AVWatchesDuty);
			AssertEquals("AV Cases", "$2619 x 6%", entryLine2.AVCases);
			AssertEquals("AVCasesDuty", 157.14m, entryLine2.AVCasesDuty);
			AssertEquals("AV Bracelets", "$1345 x 14%", entryLine2.AVBracelets);
			AssertEquals("AVBraceletsDuty", 188.30m, entryLine2.AVBraceletsDuty);
			AssertEquals("AVBatteries", "$204 x 5.3%", entryLine2.AVBatteries);
			AssertEquals("AVBatteriesDuty", 10.81m, entryLine2.AVBatteriesDuty);
			AssertEquals("AVTotalDuty", 796.25m, entryLine2.AVTotalDuty);
			AssertEquals("AV Conversion line", "$796.25/$9426.00 (Total Entered Value) = 8.447%", entryLine2.AVLine2);

			AssertEquals("Line 2 Duty", 0m, entryLine2.DutyAmount);
			AssertEquals("Line 2 Duty Percentage as String", "Free", entryLine2.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 287.70m, entryLine2.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 2 Duty to print on 7501", 0m, entryLine2.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", entryLine2.SecondaryLine2DutyPercentAsString);

			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 135.91m, entryLine2.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 4 Duty to print on 7501", 0m, entryLine2.SecondaryLine4DutyAmount);
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", entryLine2.SecondaryLine4DutyPercentAsString);

			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 113.61m, entryLine2.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 6 Duty to print on 7501", 0m, entryLine2.SecondaryLine6DutyAmount);
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", entryLine2.SecondaryLine6DutyPercentAsString);

			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 0m, entryLine2.SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine7DutyPercentAsString);

			//Entry Line 3
			EntrySummary7501Line entryLine3 = ehp.EntryPrintLines[2];
			AssertEquals("Line 3 (Parent) Tariff number", "9802.00.4040", entryLine3.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", entryLine3.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.4040", entryLine3.SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", entryLine3.SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.4040", entryLine3.SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", entryLine3.SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.4040", entryLine3.SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", entryLine3.SecondaryLine7FormattedTariff);
			AssertEquals("Line 3 Duty", 0m, entryLine3.DutyAmount);
			AssertEquals("Line 3 Duty Percentage as String", "Free", entryLine3.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 2 Duty to print on 7501", 0m, entryLine3.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", entryLine3.SecondaryLine2DutyPercentAsString);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 4 Duty to print on 7501", 0m, entryLine3.SecondaryLine4DutyAmount);
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", entryLine3.SecondaryLine4DutyPercentAsString);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 6 Duty to print on 7501", 0m, entryLine3.SecondaryLine6DutyAmount);
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", entryLine3.SecondaryLine6DutyPercentAsString);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine7DutyAmount);

			AssertEquals("Box 37 Total Duty", 1074.47m, ehp.TotalDutyAmt);
		}

		public void TestAdditionalLineSectionPrintingFlags_ADD()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 34527m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_RN_NKDefaultOrigin = "IT";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 328m;
			invoiceLine1.JI_Tariff = "1509902000";
			invoiceLine1.JI_CustomsQuantity = 41m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 364m;
			invoiceLine2.JI_Tariff = "1902192030";
			invoiceLine2.JI_CustomsQuantity = 180m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 1324m;
			invoiceLine3.JI_Tariff = "1509102000";
			invoiceLine3.JI_CustomsQuantity = 247m;
			invoiceLine3.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 349m;
			invoiceLine4.JI_Tariff = "2103204020";
			invoiceLine4.JI_CustomsQuantity = 164m;
			invoiceLine4.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 196m;
			invoiceLine5.JI_Tariff = "2103909091";
			invoiceLine5.JI_CustomsQuantity = 196m;
			invoiceLine5.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine6 = declaration.InvoiceLines.AddNew();
			invoiceLine6.JI_LinePrice = 22474m;
			invoiceLine6.JI_Tariff = "1902192030";
			invoiceLine6.JI_CustomsQuantity = 12351m;
			invoiceLine6.JI_CustomsUnitQty = "KG";
			invoiceLine6.US_ADD_NA = true;
			invoiceLine6.US_CVDCaseNo = "C475819005";
			invoiceLine6.US_CVDDepositRateIndicator = "1";

			JobComInvoiceLine invoiceLine7 = declaration.InvoiceLines.AddNew();
			invoiceLine7.JI_LinePrice = 8550m;
			invoiceLine7.JI_Tariff = "1902192030";
			invoiceLine7.JI_CustomsQuantity = 4077m;
			invoiceLine7.JI_CustomsUnitQty = "KG";
			invoiceLine7.US_ADD_NA = true;
			invoiceLine7.US_CVDCaseNo = "C475819005";
			invoiceLine7.US_CVDDepositRateIndicator = "1";

			JobComInvoiceLine invoiceLine8 = declaration.InvoiceLines.AddNew();
			invoiceLine8.JI_LinePrice = 942m;
			invoiceLine8.JI_Tariff = "1902112030";
			invoiceLine8.JI_CustomsQuantity = 240m;
			invoiceLine8.JI_CustomsUnitQty = "KG";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			var ehp = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

			AssertEquals("EntryHasADDCVDLines", true, ehp.EntryHasADDCVDLines);
			AssertEquals("EntryHasSecondaryTariffLines", false, ehp.EntryHasSecondaryTariffLines);
			AssertEquals("EntryHasAdditionalTariffLines", false, ehp.EntryHasAdditionalTariffLines);
			AssertEquals("EntryHasProRatedCalculation", false, ehp.EntryHasProRatedCalculation);
			AssertEquals("EntryHasAdValoremConversionCalculation ", false, ehp.EntryHasAdValoremConversionCalculation);
		}

		public void TestForConsumptionFTZ()
		{
			Declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			Declaration.Invoices.RemoveAll();
			Factory.Save();

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Declaration.US_EnableENS = true;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8703330045";
			invoiceLine1.JI_LinePrice = 48000m;
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var printBO = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("UniqueCountryOfExport", Core.Constants.CountryCodes.Canada, printBO.UniqueCountryOfExport);

			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Chile;
			invoiceLine1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Mexico;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			printBO = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("UniqueCountryOfExport should be calculated for FTZ", Core.Constants.CountryCodes.Mexico, printBO.UniqueCountryOfExport);

			invoiceLine1.JI_LinePrice = 2900m;
			var invoice2 = Declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8703330045";
			invoiceLine2.JI_LinePrice = 3600m;
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine2.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Mauritania;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			printBO = new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));
			AssertEquals("When there are multiple countries of export, that the highest entered value (customs value in USD) one is shown.", Core.Constants.CountryCodes.Mauritania, printBO.UniqueCountryOfExport);
		}

		[TestDate(2018, 10, 10)]
		public void TestTotalADCVDutyPayableForCombinedLines()
		{
			#region Setup

			var caseADNumber = Factory.New<USCACCase>();
			caseADNumber.U5_CaseNumber = "A570979116";
			caseADNumber.U5_ISOCountryCode = "CN";
			caseADNumber.U5_CaseStatusDate = ZDateTime.Today.AddDays(-10);
			var caseADRate = caseADNumber.CaseRates.AddNew();
			caseADRate.U6_CaseNumber = "A570979116";
			caseADRate.U6_AdValoremRate = 0.0782;
			caseADRate.U6_EffectiveDate = ZDateTime.Today.AddDays(-10);
			var caseADTariff = caseADNumber.CaseTariffs.AddNew();
			caseADTariff.U9_CaseNumber = "A570979116";
			caseADTariff.U9_TariffNumber = "8541406025";

			var caseCVDNumber = Factory.New<USCACCase>();
			caseCVDNumber.U5_CaseNumber = "C570980043";
			caseCVDNumber.U5_ISOCountryCode = "CN";
			caseCVDNumber.U5_CaseStatusDate = ZDateTime.Today.AddDays(-10);
			var caseCVDRate = caseCVDNumber.CaseRates.AddNew();
			caseCVDRate.U6_CaseNumber = "C570980043";
			caseCVDRate.U6_AdValoremRate = 0.132m;
			caseCVDRate.U6_EffectiveDate = ZDateTime.Today.AddDays(-10);
			var caseCVDTariff = caseCVDNumber.CaseTariffs.AddNew();
			caseCVDTariff.U9_TariffNumber = "C570980043";
			caseCVDTariff.U9_TariffNumber = "8541406025";

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfExport = "CN";
			invoiceLine1.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine1.US_SupTariff = "99038802";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_UC_NKCountryOfExport = "CN";
			invoiceLine2.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine2.JI_Tariff = "8541406025";
			invoiceLine2.US_SupTariff = "99034521";
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.US_ADDCaseNo = "A570979116";
			invoiceLine2.US_ADDDepositRateIndicator = "A";
			invoiceLine2.US_CVDCaseNo = "C570980043";
			invoiceLine2.US_CVDDepositRateIndicator = "A";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var printBO = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TotalAntidumpingDutyAmountPayable", 782m, printBO.TotalAntidumpingDutyAmountPayable);
			AssertEquals("TotalCountervailingDutyPayable", 1320m, printBO.TotalCountervailingDutyPayable);
		}

		[TestDate(2025, 04, 01)]
		public void TestTotalADCVDutyPayableForInvoiceLineWithAdditionalTariffs()
		{
			#region Setup

			var caseADNumber = Factory.New<USCACCase>();
			caseADNumber.U5_CaseNumber = "A122857070";
			caseADNumber.U5_ISOCountryCode = "CA";
			caseADNumber.U5_CaseStatusDate = ZDateTime.Today.AddDays(-10);
			var caseADRate = caseADNumber.CaseRates.AddNew();
			caseADRate.U6_CaseNumber = "A122857070";
			caseADRate.U6_AdValoremRate = 0.0766m;
			caseADRate.U6_EffectiveDate = ZDateTime.Today.AddDays(-10);
			var caseADTariff = caseADNumber.CaseTariffs.AddNew();
			caseADTariff.U9_CaseNumber = "A122857070";
			caseADTariff.U9_TariffNumber = "4407130000";

			var caseCVDNumber = Factory.New<USCACCase>();
			caseCVDNumber.U5_CaseNumber = "C122858078";
			caseCVDNumber.U5_ISOCountryCode = "CA";
			caseCVDNumber.U5_CaseStatusDate = ZDateTime.Today.AddDays(-10);
			var caseCVDRate = caseCVDNumber.CaseRates.AddNew();
			caseCVDRate.U6_CaseNumber = "C122858078";
			caseCVDRate.U6_AdValoremRate = 0.0674m;
			caseCVDRate.U6_EffectiveDate = ZDateTime.Today.AddDays(-10);
			var caseCVDTariff = caseCVDNumber.CaseTariffs.AddNew();
			caseCVDTariff.U9_TariffNumber = "C122858078";
			caseCVDTariff.U9_TariffNumber = "4407130000";

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "4407130000", "1", 0m, "M3");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030126", "0", 0m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030114", "0", 0m, "");

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = "XC";
			invoiceLine.JI_FormattedTariff = "4407.13.0000";
			invoiceLine.SupTariffFormatted = "9903.01.26";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.01.14";
			invoiceLine.US_SPI = "S";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_ADDCaseNo = "A122857070";
			invoiceLine.US_ADDDepositRateIndicator = "A";
			invoiceLine.US_CVDCaseNo = "C122858078";
			invoiceLine.US_CVDDepositRateIndicator = "A";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var printBO = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entry, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals("TotalAntidumpingDutyAmountPayable", 766m, printBO.TotalAntidumpingDutyAmountPayable);
			AssertEquals("TotalCountervailingDutyPayable", 674m, printBO.TotalCountervailingDutyPayable);
		}

		protected override BusinessObject GetNewBusinessObject() => new EntryHeaderENS7501Print<ACSEntryHeaderENS7501Line>(Entry, (a, b, c) => new ACSEntryHeaderENS7501Line(a, b, c));

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			GlbBranch.CurrentBranch.SetCountry("US");
			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			EntryFiler filer = new EntryFiler();
			filer.EntryFilerCode = "XJ6";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.Invoices.AddNew();
			Declaration.InvoiceLines.AddNew();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
		}

		JobComInvoiceHeader GetNewInvoiceHeader(JobDeclaration declaration, ZString invoiceNumber)
		{
			JobComInvoiceHeader result = declaration.Invoices.AddNew();
			result.JZ_InvoiceNumber = invoiceNumber;
			result.JZ_InvoiceAmount = 15000m;
			result.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			return result;
		}

		JobComInvoiceLine AddNewInvoiceLine(JobComInvoiceHeader invoice, ZString tariff)
		{
			JobComInvoiceLine result = invoice.JobComInvoiceLines.AddNew();

			result.JI_Tariff = tariff;
			result.US_UC_NKCountryOfExport = "DE";
			result.US_UC_NKCountryOfOrigin = "PL";
			result.JI_InvoiceQuantity = 250m;
			result.JI_InvoiceUQ = "KG";
			result.JI_CustomsQuantity = 250m;

			return result;
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return declaration;
			}
		}

		CusEntryHeader entry;
		CusEntryHeader Entry => entry ?? (entry = Declaration.CustomsEntryHeaders.AddNew());

		void CreateTIBDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;
			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateTIBWithExemptTariffsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 8464m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130050";
			invoiceLine1.JI_InvoiceQuantity = 95m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 8000m;
			invoiceLine1.JI_Tariff = "8528723600";
			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_CustomsUnitQty = "NO";

			JobComInvoiceLine invoiceLine2 = Declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "98130050";
			invoiceLine2.JI_InvoiceQuantity = 5m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_LinePrice = 464m;
			invoiceLine2.JI_Tariff = "7318154000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateWatchRepairsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			Declaration.US_EnableENS = true;
			Declaration.US_SchDLoading = "55976";
			Declaration.JE_RL_NKPortOfLoading = "SGSIN";

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH_Repairs";
			invoiceHeader.JZ_InvoiceAmount = 19080m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "KR";
			invoiceHeader.US_UC_NKCountryOfExport = "SG";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				#region entry line 1
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802.00.8068";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102.11.1010";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;
				invoiceLine1.JI_CountryOfOrigin = "KR";

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802.00.8068";
				childLine2.US_98GoodsValue = 1010m;
				childLine2.JI_Tariff = "9102.11.1020";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802.00.8068";
				childLine4.JI_Tariff = "9102.11.1030";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802.00.8068";
				childLine6.US_98GoodsValue = 204m;
				childLine6.JI_Tariff = "9102.11.1040";
				childLine6.JI_CustomsQuantity = 1000m;
				childLine6.JI_CustomsUnitQty = "NO";
				#endregion

				#region entry line 2
				JobComInvoiceLine invoiceLine2 = Declaration.InvoiceLines.AddNew();
				invoiceLine2.US_SupTariff = "9802.00.4040";
				invoiceLine2.US_98GoodsValue = 1852m;
				invoiceLine2.JI_Tariff = "9102.11.1010";
				invoiceLine2.JI_CustomsQuantity = 1000m;
				invoiceLine2.JI_CustomsUnitQty = "NO";
				invoiceLine2.JI_LinePrice = 3406m;
				invoiceLine2.JI_CountryOfOrigin = "KR";

				JobComInvoiceLine il2childLine2 = invoiceLine2.AddSecondaryInvoiceLine();
				il2childLine2.US_SupTariff = "9802.00.4040";
				il2childLine2.US_98GoodsValue = 1010m;
				il2childLine2.JI_Tariff = "9102.11.1020";
				il2childLine2.JI_CustomsQuantity = 1000m;
				il2childLine2.JI_CustomsUnitQty = "NO";
				il2childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine il2childLine4 = invoiceLine2.AddSecondaryInvoiceLine();
				il2childLine4.US_SupTariff = "9802.00.4040";
				il2childLine4.JI_Tariff = "9102.11.1030";
				il2childLine4.JI_CustomsQuantity = 1000m;
				il2childLine4.JI_CustomsUnitQty = "NO";
				il2childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine il2childLine6 = invoiceLine2.AddSecondaryInvoiceLine();
				il2childLine6.US_SupTariff = "9802.00.4040";
				il2childLine6.US_98GoodsValue = 204m;
				il2childLine6.JI_Tariff = "9102.11.1040";
				il2childLine6.JI_CustomsQuantity = 1000m;
				il2childLine6.JI_CustomsUnitQty = "NO";
				#endregion

				#region entry line 3
				JobComInvoiceLine invoiceLine3 = Declaration.InvoiceLines.AddNew();
				invoiceLine3.US_SupTariff = "9802.00.4040";
				invoiceLine3.US_98GoodsValue = 1852m;
				invoiceLine3.JI_Tariff = "9102111010";
				invoiceLine3.JI_CustomsQuantity = 1000m;
				invoiceLine3.JI_CustomsUnitQty = "NO";
				invoiceLine3.JI_LinePrice = 3406m;
				invoiceLine3.JI_CountryOfOrigin = "XO";
				invoiceLine3.US_UC_NKCountryOfExport = "CA";
				invoiceLine3.US_SPI = "CA";

				JobComInvoiceLine il3childLine2 = invoiceLine3.AddSecondaryInvoiceLine();
				il3childLine2.US_SupTariff = "9802.00.4040";
				il3childLine2.US_98GoodsValue = 1010m;
				il3childLine2.JI_Tariff = "9102111020";
				il3childLine2.JI_CustomsQuantity = 1000m;
				il3childLine2.JI_CustomsUnitQty = "NO";
				il3childLine2.JI_LinePrice = 1609m;
				il3childLine2.JI_CountryOfOrigin = "XO";
				il3childLine2.US_UC_NKCountryOfExport = "CA";
				il3childLine2.US_SPI = "CA";

				JobComInvoiceLine il3childLine4 = invoiceLine3.AddSecondaryInvoiceLine();
				il3childLine4.US_SupTariff = "9802.00.4040";
				il3childLine4.JI_Tariff = "9102111030";
				il3childLine4.JI_CustomsQuantity = 1000m;
				il3childLine4.JI_CustomsUnitQty = "NO";
				il3childLine4.JI_LinePrice = 1345m;
				il3childLine4.JI_CountryOfOrigin = "XO";
				il3childLine4.US_UC_NKCountryOfExport = "CA";
				il3childLine4.US_SPI = "CA";

				JobComInvoiceLine il3childLine6 = invoiceLine3.AddSecondaryInvoiceLine();
				il3childLine6.US_SupTariff = "9802.00.4040";
				il3childLine6.US_98GoodsValue = 204m;
				il3childLine6.JI_Tariff = "9102111040";
				il3childLine6.JI_CustomsQuantity = 1000m;
				il3childLine6.JI_CustomsUnitQty = "NO";
				il3childLine6.JI_CountryOfOrigin = "XO";
				il3childLine6.US_UC_NKCountryOfExport = "CA";
				il3childLine6.US_SPI = "CA";
				#endregion
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		JobDeclaration SetUpMergedInvoices()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.DisableDefaultPackingInformation = true;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();

			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			invoice1.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			manufacturer.OH_FullName = "Manufacturer";
			manufacturer.FillWithValidTestData();
			OrgCusCode manufacturerCode = manufacturer.MainAddress.CustomsCodes.AddNew();
			manufacturerCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			manufacturerCode.OK_CustomsRegNo = "XYBEREQU6LON";
			manufacturerCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;
			JobComInvoiceHeader invoice3 = declaration.Invoices.AddNew();
			JobComInvoiceLine line3 = invoice3.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine2.PK;
			JobComInvoiceHeader invoice4 = declaration.Invoices.AddNew();
			JobComInvoiceLine line4 = invoice4.JobComInvoiceLines.AddNew();
			line4.JI_CL = entryLine2.PK;
			return declaration;
		}

		JobDeclaration SetUpMergedInvoice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.US_DestinationState = "IL";

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 8000m;
			invoiceLine2.US_DestinationState = "AL";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			return declaration;
		}

		JobDeclaration SetUpBills()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			date1 = new ZDateTime(2007, 3, 12, 3, 32, 32);
			declaration.US_ITDate = date1;
			declaration.Bills.RemoveAll();

			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MasterBill";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";

			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill.PK;
			houseBill1.CU_BillNum = "HouseBill1";
			houseBill1.US_UI_NKBillIssuerSCAC = "APLU";

			Bill subHouseBill = declaration.Bills.AddNew();
			subHouseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill.CU_BillNum = "SubHouseBill";
			subHouseBill.US_UI_NKBillIssuerSCAC = "APLU";
			subHouseBill.ITNumber = "IT12345";

			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_CU_ParentBill = masterBill.PK;
			houseBill2.ITNumber = "IT1234";
			houseBill2.CU_BillNum = "HouseBill2";
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";

			return declaration;
		}
		ZDateTime date1;

		GlbStaff TestBroker
		{
			get
			{
				Broker.GS_FullName = "Tim Brooke Broker";
				Broker.GS_Code = "TBB";
				Broker.GS_WorkPhone = "+1 2 11112222";

				GlbBranch brokerTestBranch = Factory.NewWithValidTestData<GlbBranch>();
				brokerTestBranch.GB_Address1 = "TB Addr1";
				brokerTestBranch.GB_Address2 = "TB Addr1";
				brokerTestBranch.GB_City = "New York";
				brokerTestBranch.GB_PostCode = "99999";

				Broker.GS_GB_HomeBranch = brokerTestBranch.PK;

				return Broker;
			}
		}

		GlbStaff broker;
		GlbStaff Broker => broker ?? (broker = Factory.NewWithValidTestData<GlbStaff>());
	}
}

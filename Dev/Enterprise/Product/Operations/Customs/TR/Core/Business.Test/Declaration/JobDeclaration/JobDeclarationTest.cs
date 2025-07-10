using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.TR.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : EU.Business.Declaration.Testing.JobDeclarationAbstractTest<JobDeclaration>
	{
		public void TestSetDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				AssertEquals("JE_ExportGoodsType", OrderTypesOfGoodsList.Codes._3, declaration.JE_ExportGoodsType);
				AssertEquals("JE_ValuationDate", ZDate.Today, declaration.JE_ValuationDate);
				AssertEquals("ZG_TradeType", TradeTypeList.Codes.ETD, declaration.ZG_TradeType);
			});
		}

		public void TestJE_ExportGoodsType_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();

			AssertEquals("Order Type", DataBoundResourceStrings.GetDataForProperty(declaration.JE_ExportGoodsTypeInfo).Caption);
		}

		public void TestCustomsEntryInstructions()
		{
			AssertType<EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>>(declaration.CustomsEntryInstructions);
		}

		public override void TestGetCustomsEntryInstructionProviderCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<EntryInstructionProvider>(declaration.CustomsEntryInstructionProvider);
		}

		public void TestSupportingDocuments()
		{
			AssertType<SupportingDocumentCollection>("Should be a Customs.TR.Business.Declaration.SupportingDocumentCollection", declaration.SupportingDocuments);
		}

		public void TestCreateNewSupportingDocumentCollection()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			var newSupportingDocumentCollection = declaration.CreateNewSupportingDocumentCollection();
			AssertType<SupportingDocumentCollection>("Should be a Customs.TR.Business.Declaration.SupportingDocumentCollection", newSupportingDocumentCollection);
		}

		public override void TestMergeManagerType()
		{
			AssertType<MergeManager>("Should be a Customs.TR.Business.Declaration.MergeManager", declaration.MergeManager);
		}

		public void TestRequiresMergeIsStillFalseAfterAccessingJE_DeclarationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals("RequiresMerge should be false by default ", false, declaration.MergeManager.RequiresMerge);
			_ = declaration.JE_DeclarationDate;
			AssertEquals("RequiresMerge should still be false after accessing JE_DeclarationDate", false, declaration.MergeManager.RequiresMerge);
		}

		public void TestCusEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertNull(declaration.CusEntryHeader);

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_EntrySubmittedDate = ZDateTime.Today;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_EntrySubmittedDate = ZDateTime.Today.AddDays(-1);
			AssertSame("Should be the first entry ordered by CH_EntrySubmittedDate", entry2, declaration.CusEntryHeader);
		}

		public void TestSupportsChcPivotBetweenInvoiceLineAndPacking()
		{
			AssertEquals(true, declaration.SupportsChcPivotBetweenInvoiceLineAndPacking);
		}

		public void TestTraders()
		{
			var docAddress1 = Factory.New<Trader>();
			docAddress1.E2_AddressType = "BUY";
			docAddress1.E2_ParentID = declaration.PK;
			docAddress1.E2_ParentTableCode = declaration.TablePrefix;
			var docAddress2 = Factory.New<Trader>();
			docAddress2.E2_AddressType = "SEL";
			docAddress2.E2_ParentID = declaration.PK;
			docAddress2.E2_ParentTableCode = declaration.TablePrefix;
			var docAddress3 = Factory.New<Trader>();
			docAddress3.E2_AddressType = "BOF";
			docAddress3.E2_ParentID = declaration.PK;
			docAddress3.E2_ParentTableCode = declaration.TablePrefix;
			var traders = declaration.Traders;

			AssertEquals(2, traders.Count);
			AssertCollectionContains(docAddress1, traders);
			AssertCollectionContains(docAddress2, traders);
		}

		public void TestWillThereBeMultipleEntryHeaders()
		{
			AssertEquals(false, declaration.WillThereBeMultipleEntryHeaders);
		}

		public void TestJE_DeclarationDate()
		{
			AssertEquals(ZDateTime.Empty, declaration.JE_DeclarationDate);
			declaration.CustomsEntryHeaders.AddNew();
			var entryHeader = declaration.CusEntryHeader;
			var date = new ZDateTime(2020, 11, 24);
			entryHeader.CH_EntrySubmittedDate = date;
			AssertEquals("Should be CH_EntrySubmittedDate of EntryHeader", date, declaration.JE_DeclarationDate);
		}

		public void TestJE_DeclarationExchangeRate()
		{
			AssertEquals(0m, declaration.JE_DeclarationExchangeRate);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TR0001";
			invoice.JZ_InvoiceCurrExRate = 1.6427m;
			AssertEquals(1.6427m, declaration.JE_DeclarationExchangeRate);
		}

		public void TestTotalCustomsQuantity()
		{
			using (CustomsDataRegistry.Instance.CopyPreviousLineCustomsProcedureCode.SetTemporaryValue(Guid.Empty, declaration.Branch.EntityPK.ToGuid(), Guid.Empty, false))
			{
				AssertEquals(0m, declaration.TotalCustomsQuantity);
				var invoiceLine1 = declaration.InvoiceLines.AddNew();
				invoiceLine1.JI_CustomsQuantity = 15m;
				var invoiceLine2 = declaration.InvoiceLines.AddNew();
				invoiceLine2.JI_CustomsQuantity = 2.201m;
				AssertEquals(17.201m, declaration.TotalCustomsQuantity);
			}
		}

		public void TestJE_CustomsDischargePort()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("PORT", "PortList");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "PORT1", "PORT Name 1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "PORT2", "PORT Name 2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			declaration.JE_CustomsDischargePort = "PORT1";
			Factory.Save();
			AssertEquals("PORT1", declaration.JE_CustomsDischargePort);
		}

		public void TestJE_CustomsLoadPort()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = "SEA";
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("PORT", "PortList");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "PORT1", "PORT Name 1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "PORT", "PORT2", "PORT Name 2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			declaration.JE_CustomsLoadPort = "PORT2";
			Factory.Save();
			AssertEquals("PORT2", declaration.JE_CustomsLoadPort);
		}

		public override void TestCustomsOfficeOfExit()
		{
			JobDeclaration decEcs = Factory.New<JobDeclaration>();
			decEcs.JE_MessageType = "EXP";
			AssertEquals("", decEcs.OfficeOfExit);
			decEcs.CustomsOffices.Cast<OfficeCode>().Single().CY_Data = "TR000001";
			AssertEquals("TR000001", decEcs.OfficeOfExit);

			decEcs.JE_MessageType = "1";
			decEcs.JE_ApplicationCode = "EMC";
			Assert(decEcs.IsEMCS);
		}

		public void TestCusEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertNotNull(declaration.CusEntryInstruction);

			declaration = Factory.New<JobDeclaration>();
			var instruction1 = Factory.New(typeof(CusEntryInstruction), new Guid("29168C5E-CEB2-4faa-B6BF-329BF39FA1E4"));
			var instruction2 = Factory.New(typeof(CusEntryInstruction), new Guid("19168C5E-CEB2-4faa-B6BF-329BF39FA1E4"));
			declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Add(instruction1);
			declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.Add(instruction2);
			AssertEquals(instruction2, declaration.CusEntryInstruction);
		}

		public void TestJE_EntrySubStyle()
		{
			var instruction = declaration.CusEntryInstruction;
			instruction.CEI_SubStyle = "1";
			AssertEquals("1", declaration.JE_EntrySubStyle);
		}

		public void TestJE_EntryDateForDuty()
		{
			var instruction = declaration.CusEntryInstruction;
			var date = new ZDateTime(2020, 11, 24);
			instruction.CEI_DateForDuty = date;
			AssertEquals(date, declaration.JE_EntryDateForDuty);
		}

		public void TestJE_EntryFromWarehouse()
		{
			var instruction = declaration.CusEntryInstruction;
			instruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			AssertEquals(helper.Warehouse.MainAddress.PK, declaration.JE_EntryFromWarehouse);
			AssertEquals(helper.Warehouse.MainAddress, declaration.JE_EntryFromWarehouse_ZAddress.OrgAddress);
		}
		public void TestJE_EntryToWarehouse()
		{
			var instruction = declaration.CusEntryInstruction;
			instruction.CEI_OA_Warehouse2 = helper.Warehouse2.MainAddress.PK;
			AssertEquals(helper.Warehouse2.MainAddress.PK, declaration.JE_EntryToWarehouse);
			AssertEquals(helper.Warehouse2.MainAddress, declaration.JE_EntryToWarehouse_ZAddress.OrgAddress);
		}

		protected override void SetupInvoice(BaseJobDeclaration declaration)
		{
			base.SetupInvoice(declaration);
			var dec = ((JobDeclaration)declaration);
			dec.JE_EntrySubStyle = "1";
			dec.JE_EntryDateForDuty = new ZDateTime(2020, 11, 24);
			dec.DischargeOffice = "TR210300";
			dec.DischargePlace = "Place Name";
		}

		public void TestDischargeOffice()
		{
			AssertEquals(ZString.Empty, declaration.DischargeOffice);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.DischargeOffice = "TR000001";
			AssertNotNull(EuOfficeCode.Load<OfficeCode>(declaration, EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented));
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertNotNull(EuOfficeCode.Load<OfficeCode>(declaration, EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented));
			AssertEquals("TR000001", declaration.DischargeOffice);
		}

		public void TestDischargePlace()
		{
			AssertEquals(ZString.Empty, declaration.DischargePlace);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("CUSOF", "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "CUSOF", "TR000001", "Place Name 1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "CUSOF", "TR000002", "Place Name 2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			declaration.DischargeOffice = "TR000001";
			AssertEquals("Place Name 1", declaration.DischargePlace);

			declaration.DischargePlace = "Another Place";
			declaration.DischargeOffice = "TR000002";
			AssertEquals("Another Place", declaration.DischargePlace);

			declaration.DischargePlace = ZString.Empty;
			AssertEquals("Place Name 2", declaration.DischargePlace);
		}

		public void TestEntryOffice()
		{
			AssertEquals(ZString.Empty, declaration.EntryOffice);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.EntryOffice = "TR000020";
			AssertNotNull(EuOfficeCode.Load<OfficeCode>(declaration, EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented));
			Factory.Save();
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertNotNull(EuOfficeCode.Load<OfficeCode>(declaration, EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented));
			AssertEquals("TR000020", declaration.EntryOffice);
		}

		public void TestGetCustomsOffices()
		{
			var customsOffices = declaration.CustomsOffices;
			AssertType<OfficeCodeCollection>(customsOffices);
			AssertEquals(EuOfficeCodesTypes.Codes.CodeOfTheCustomsOfficeWhereTheGoodsShallBePresented, customsOffices.DefaultPurposeCode);
		}

		public override void TestGetCusCodeDataType()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(typeof(OfficeCode), ((Integration.Customs.ICusCodeDataTypeSupporter)dec).GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.OfficeCode]);
		}

		public void TestBondedWarehouseCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "Bonded Warehouse Codes List", "TR");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "ABC", "ABC Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyBondedWarehouseCodes, "CDE", "CDE Port.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			declaration.BondedWarehouseCode = "ABC";
			Factory.Save();
			AssertEquals("ABC", declaration.BondedWarehouseCode);
		}

		public void TestFirstPortOfArrivalDefaultsWhenFinalDestinationIsSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKFinalDestination = "GBLHR";
			AssertEquals("TR is not EU, so First Port of Arrival should NOT default to the final destination", ZString.Empty, declaration.JE_RL_NKPortOfFirstArrival);
		}

		public new void TestPopulateCommercialInvoice()
		{
			//TODO
			Assert("We will handle default JI_Tariff in future work item.", true);
		}

		public void TestLookupObjectIsNotCached()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationLookups>("should not cache for TR", declaration.Lookups);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationLookups>("should not cache for TR", declaration.Lookups);
		}

		public void TestLookups_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationLookups>(declaration.Lookups);
		}

		public void TestLookups_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationLookups>(declaration.Lookups);
		}

		public void TestLookups_MiscellaneousCustoms()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationLookups>(declaration.Lookups);
		}

		public void TestValidation_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobDeclarationValidation>(declaration.Validation);
		}

		public void TestValidation_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobDeclarationValidation>(declaration.Validation);
		}

		public void TestValidation_MiscellaneousCustoms()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobDeclarationValidation>(declaration.Validation);
		}

		public void TestInvoices()
		{
			AssertType<InvoiceHeaderActiveCollection>(declaration.Invoices);
		}

		public void TestInvoiceLines()
		{
			AssertType<InvoiceLineCompleteCollection>(declaration.InvoiceLines);
		}

		public void TestJobComInvoiceGroupHeaders()
		{
			AssertType<BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>>(declaration.JobComInvoiceGroupHeaders);
		}

		JobDeclarationForTesting GetJobDeclarationForTesting()
		{
			var dec = Factory.New<JobDeclarationForTesting>();
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}

		public void TestDV1Details()
		{
			AssertType<CusDV1DetailCollection>("Should be a Customs.TR.Business.Declaration.CusDV1DetailCollection", declaration.DV1Details);
		}

		public void TestCreateNewDV1DetailsCollection()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			var newDV1DetailsCollection = declaration.CreateNewDV1DetailsCollection();
			AssertType<CusDV1DetailCollection>("Should be a Customs.TR.Business.Declaration.CusDV1DetailCollection", newDV1DetailsCollection);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals(Core.Constants.CurrencyCodes.Turkey, GetJobDeclarationForTesting().LocalCurrencyCodeCoreExposed);
		}

		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			AssertEquals(false, declaration.AreMultipleEntryInstructionsAllowed);
		}

		public void TestJE_TransportModeInland_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[25] Border M.O.T.", DataBoundResourceStrings.GetDataForProperty(declaration.JE_TransportModeInlandInfo).Caption);
		}

		public void TestJE_TransportModeInland_SetsZG_InlandTransportType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = TRTransportModeInland.Codes._30;

			AssertEquals("JE_TransportModeInland should set ZG_InlandTransportType in CusEntryInstruction to be the same value",
				"30", declaration.CusEntryInstruction.ZG_InlandTransportType);

			declaration.CusEntryInstruction.ZG_InlandTransportType = TRTransportModeInland.Codes._20;
			declaration.JE_TransportModeInland = TRTransportModeInland.Codes._16;

			AssertEquals("The value of ZG_InlandTransportType should not change after changing the value of JE_TransportModeInland at this stage",
				"20", declaration.CusEntryInstruction.ZG_InlandTransportType);
		}

		public override void TestJE_TransportMode_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Transport", DataBoundResourceStrings.GetDataForProperty(declaration.JE_TransportModeInfo).Caption);
		}

		public void TestJE_SubLocationOfGoods()
		{
			{
				var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				var yesterday = ZDateTime.Today.AddDays(-1);
				var tomorrow = ZDateTime.Today.AddDays(1);
				var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", eun);
				helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "TR Warehouses");
				helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyWarehouseCodes, "WAR1", "Warehouse1", yesterday, tomorrow);

				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_LocationOfGoods = "WAR1";

				AssertEquals("Warehouse1", declaration.JE_SubLocationOfGoods);
			}
		}

		public void TestJE_GoodsDestination_SetsZG_ExportUnionCountryCode()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var today = ZDateTime.Today;
			helper.CreateCusMapType("CNTRY", "BTH", "Country Code Mapping", true);
			helper.CreateCusMap("CNTRY", "TR", "052", startDate, endDate, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "SG", "706", startDate, endDate, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "DE", "004", startDate, endDate, Core.Constants.CountryCodes.Turkey);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GoodsDestination = "TR";

			AssertEquals("JE_GoodsDestination should map & set to ZG_ExportUnionCountryCode in CusEntryInstruction to be the same value",
						 "052",
						 declaration.CusEntryInstruction.ZG_ExportUnionCountryCode);

			declaration.CusEntryInstruction.ZG_ExportUnionCountryCode = "706";
			declaration.JE_GoodsDestination = "DE";

			AssertEquals("The value of ZG_ExportUnionCountryCode should not change after changing the value of JE_GoodsDestination at this stage",
						 "706",
						 declaration.CusEntryInstruction.ZG_ExportUnionCountryCode);
		}

		public void TestExchangeRateWhenShipmentTypeChanged()
		{
			var foreignCurrency = RefCurrency.New(Factory);
			foreignCurrency.RX_Code = "XYZ";

			var cusRate = foreignCurrency.ExchangeRates.AddNew();
			cusRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			cusRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			cusRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			cusRate.RE_SellRate = 0.8m;

			var cusSecRate = foreignCurrency.ExchangeRates.AddNew();
			cusSecRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
			cusSecRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			cusSecRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			cusSecRate.RE_SellRate = 0.7m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ValuationDate = ZDate.Today;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;

			AssertEquals(0.7m, invoice1.JZ_InvoiceCurrExRate);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals(0.8m, invoice1.JZ_InvoiceCurrExRate);
		}

		public void TestGoodsAtCustomsArea()
		{
			AssertEquals(ZBool.False, declaration.GoodsAtCustomsArea);

			declaration.CustomsEntryHeaders.AddNew();
			var header = declaration.CusEntryHeader;
			header.EUH_AreGoodsAtCustomsArea = ZBool.True;
			AssertEquals(ZBool.True, declaration.GoodsAtCustomsArea);
		}

		public void TestOverTimePaymentCompleted()
		{
			AssertEquals(ZBool.False, declaration.OverTimePaymentCompleted);

			declaration.CustomsEntryHeaders.AddNew();
			var header = declaration.CusEntryHeader;
			header.EUH_IsOverTimePaymentCompleted = ZBool.True;
			AssertEquals(ZBool.True, declaration.OverTimePaymentCompleted);
		}

		public void TestInspectionClerk()
		{
			AssertEquals(ZString.Empty, declaration.InspectionClerk);

			declaration.CustomsEntryHeaders.AddNew();
			var header = declaration.CusEntryHeader;
			header.EUH_InspectionClerk = "TEST";
			AssertEquals("TEST", declaration.InspectionClerk);
		}

		public void TestZG_IsHighValueOvrd()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("ZG_IsHighValueOvrd is true", true, declaration.ZG_IsHighValueOvrd);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("ZG_IsHighValueOvrd is false", false, declaration.ZG_IsHighValueOvrd);
			});
		}

		public void TestGetAllActiveCusSupportingInfoTypes()
		{
			CombineAssertions(() =>
			{
				AssertEquals(typeof(AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)declaration).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals(typeof(SupportingDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)declaration).GetCusSupportingInfoTypes()[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
			});
		}

		public void TestAdditionalInfos()
		{
			AssertType<AdditionalInfoCollection>(declaration.AdditionalInfos);
		}

		public void TestFilteredInvoiceLines()
		{
			AssertType<EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public void TestCalculatedAmountFields_InvoiceCount()
		{
			declaration.Invoices.AddNew();
			AssertEquals("InvoiceCount", 1, declaration.InvoiceCount);

			declaration.Invoices.AddNew();
			AssertEquals("InvoiceCount", 2, declaration.InvoiceCount);
		}

		public void TestCalculatedAmountFields_TotalInvoice()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1.1;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 2.2;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;
			AssertEquals("TotalInvoiceAmount", 3.3m, declaration.TotalInvoiceAmount);
			AssertCurrencyGuid("TotalInvoiceCurrency: Default invoice JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.Turkey, declaration.TotalInvoiceCurrency);
		}

		public void TestCalculatedAmountFields_TotalFreeOnBoard()
		{
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.Turkey, 1, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 19.33, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.EuropeanUnion, 21.41, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			declaration.Company.GC_IsReciprocal = true;
			declaration.JE_ValuationDate = ZDate.Today;
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var insuranceCharge1 = invoice.Charges.AddNew();
			insuranceCharge1.J7_ChargeType = TRIncotermChargeCodeList.Codes.DEM;
			insuranceCharge1.J7_Amount = 1;
			insuranceCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			insuranceCharge1.J7_IsDutiable = false;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice2.JZ_InvoiceAmount = 2000;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var insuranceCharge2 = invoice2.Charges.AddNew();
			insuranceCharge2.J7_ChargeType = TRIncotermChargeCodeList.Codes.DEM;
			insuranceCharge2.J7_Amount = 1;
			insuranceCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			insuranceCharge2.J7_IsDutiable = false;

			AssertEquals("TotalFreeOnBoardAmount: 1000TRY - 21.41TRY(1EUR) + 2000TRY - 19.33TRY(1USD): 2959.26", 2959.26m, declaration.TotalFreeOnBoardAmount);
			AssertCurrencyGuid("TotalFreeOnBoardCurrency: Default invoice JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.Turkey, declaration.TotalFreeOnBoardCurrency);
		}

		public void TestCalculatedAmountFields_TotalFreight()
		{
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.Turkey, 1, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.EuropeanUnion, 21.41, ZDateTime.Today, Factory, ExchangeRateType.CustomsSecondary);
			declaration.Company.GC_IsReciprocal = true;
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var freightCharge = invoice.Charges.AddNew();
			freightCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.OFT;
			freightCharge.J7_Amount = 1;
			freightCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			AssertEquals("TotalFreightAmount: total OFT: 1EUR(21.41TRY)", 1m, declaration.TotalFreightAmount);
			AssertCurrencyGuid("TotalFreightCurrency: Default invoice JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.EuropeanUnion, declaration.TotalFreightCurrency);
		}

		public void TestCalculatedAmountFields_TotalInsurance()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var insuranceCharge = invoice.Charges.AddNew();
			insuranceCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.ONS;
			insuranceCharge.J7_Amount = 1;
			insuranceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;

			AssertEquals("TotalInsuranceAmount: total ONS: 1TRY", 1.0m, declaration.TotalInsuranceAmount);
			AssertCurrencyGuid("TotalInsuranceCurrency: Default invoice JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.Turkey, declaration.TotalInsuranceCurrency);
		}

		public void TestCalculatedAmountFields_OverSeasAmount()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var totalOverseasCharge = invoice.Charges.AddNew();
			totalOverseasCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.TotalForeignCharges;
			totalOverseasCharge.J7_Amount = 2;
			totalOverseasCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;

			AssertEquals("TotalOverseasAmount: total TFC: 2TRY", 2.0m, declaration.TotalOverseasAmount);
			AssertCurrencyGuid("TotalOverseasCurrency: Default invoice JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.Turkey, declaration.TotalOverseasCurrency);
		}

		public void TestCalculatedAmountFields_DomesticAmount()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var totalLocalCharge = invoice.Charges.AddNew();
			totalLocalCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LocalTotalCharges;
			totalLocalCharge.J7_Amount = 3;
			totalLocalCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;

			AssertEquals("LocalTotalChargesAmount: total TFC: 3TRY", 3.0m, declaration.LocalTotalChargesAmount);
			AssertCurrencyGuid("LocalTotalChargesCurrency: Default invoice JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.Turkey, declaration.LocalTotalChargesCurrency);
		}

		void AssertCurrencyGuid(string message, ZString expectedCurrencyCode, ZGuid actualCurrencyGuid)
		{
			AssertEquals(
				$"{message}, Input currency Guid: {actualCurrencyGuid}, expecting currency code: {expectedCurrencyCode}",
				Factory.LoadTop1<RefCurrency>(new ZQuery().AddToFilter(RefCurrencySchema.PK, actualCurrencyGuid)).RX_Code,
				expectedCurrencyCode
			);
		}

		public void TestCalculatedAmountFields_Caching()
		{
			var declarationForTesting = Factory.New<JobDeclarationForTesting>();

			var invoice = declarationForTesting.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Turkey;

			var freightCharge = invoice.Charges.AddNew();
			freightCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.OFT;
			freightCharge.J7_Amount = 1;
			freightCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;

			var insuranceCharge = invoice.Charges.AddNew();
			insuranceCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.ONS;
			insuranceCharge.J7_Amount = 2;
			insuranceCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;

			var totalOverseasCharge = invoice.Charges.AddNew();
			totalOverseasCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.LocalTotalCharges;
			totalOverseasCharge.J7_Amount = 4;
			totalOverseasCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;

			var totalLocalCharge = invoice.Charges.AddNew();
			totalLocalCharge.J7_ChargeType = TRIncotermChargeCodeList.Codes.TotalForeignCharges;
			totalLocalCharge.J7_Amount = 8;
			totalLocalCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;

			_ = declarationForTesting.TotalInvoiceAmount;
			_ = declarationForTesting.TotalFreeOnBoardAmount;
			_ = declarationForTesting.TotalFreightAmount;
			_ = declarationForTesting.TotalInsuranceAmount;
			_ = declarationForTesting.TotalOverseasAmount;
			_ = declarationForTesting.LocalTotalChargesAmount;

			var sumCalledCount = declarationForTesting.SumChargeAmountCalled;

			_ = declarationForTesting.TotalInvoiceAmount;
			_ = declarationForTesting.TotalFreeOnBoardAmount;
			_ = declarationForTesting.TotalFreightAmount;
			_ = declarationForTesting.TotalInsuranceAmount;
			_ = declarationForTesting.TotalOverseasAmount;
			_ = declarationForTesting.LocalTotalChargesAmount;

			AssertEquals("Should use the cached value for the second call of amount values", sumCalledCount, declarationForTesting.SumChargeAmountCalled);
		}

		public void TestEntryCreationStrategy()
		{
			AssertType<EntryCreationStrategy>("Should Be Enterprise.Customs.TR.Business.Declaration.EntryCreationStrategy", declaration.CreateEntryCreationStrategy());
		}

		protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var result = Factory.New<JobDeclaration>();
			result.AutoCreateChargesBasedOnIncoTerm = false;
			return result;
		}

		public override void TestDisableResultApportionmentWithRealInvoice2()
		{
			Assert(true);
		}

		public void TestCusContainers()
		{
			AssertType<CusContainerCollection>(declaration.CusContainers);
		}

		public void TestZG_TradeType_MaxLength()
		{
			AssertEquals(3, declaration.ZG_TradeTypeInfo.MaxLength);
		}

		public void TestDeclarationExchangeRateRefresh()
		{
			var rateDate1 = ZDate.Today.AddDays(-3);
			var rateDate2 = ZDate.Today.AddDays(-1);

			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 30.33, rateDate1, Factory, ExchangeRateType.CustomsSecondary);
			CurrencyTestHelper.SetExchangeRate(Core.Constants.CurrencyCodes.UnitedStates, 40.33, rateDate2, Factory, ExchangeRateType.CustomsSecondary);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			Factory.Save();

			declaration.JE_ValuationDate = rateDate1;
			AssertEquals(30.33m, declaration.JE_DeclarationExchangeRate);

			declaration.JE_ValuationDate = rateDate2;
			AssertEquals(40.33m, declaration.JE_DeclarationExchangeRate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = (JobDeclaration)GetJobDeclaration();
			helper = new WhsDataTestHelper(Factory);
		}
		JobDeclaration declaration;
		WhsDataTestHelper helper;
	}

	sealed class JobDeclarationForTesting : JobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LocalCurrencyCodeCoreExposed => LocalCurrencyCodeCore;

		public new EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => base.CreateNewSupportingDocumentCollection();

		public new EU.Business.Declaration.CusDV1DetailCollection CreateNewDV1DetailsCollection() => base.CreateNewDV1DetailsCollection();

		public int SumChargeAmountCalled { get; private set; }

		protected internal override ZDecimal SumChargeAmount(string chargeType)
		{
			SumChargeAmountCalled++;
			return base.SumChargeAmount(chargeType);
		}
	}
}

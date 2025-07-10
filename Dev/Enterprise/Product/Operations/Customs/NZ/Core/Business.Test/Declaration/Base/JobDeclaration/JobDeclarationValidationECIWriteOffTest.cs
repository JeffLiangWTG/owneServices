using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	sealed class JobDeclarationValidationECIWriteOffTest : JobDeclarationValidationTest
	{
		[TestDate(2019, 07, 30)]
		public void TestCheckJE_ECI_InvoiceAmount()
		{
			var warnningMessage = "Tariff details are required for consignment values above NZD$400.  Please enter invoice lines.";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("LVT", 400m, "NZ", new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6), "Low Value");
			Factory.Save();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_ECI_InvoiceCurrency = Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "NZD").PK;
			Declaration.JE_ECI_InvoiceAmount = 400m;
			AssertHasWarningContaining(Declaration.JE_ECI_InvoiceAmountInfo, warnningMessage);

			Declaration.JE_ECI_InvoiceAmount = 399m;
			AssertNoWarningContaining(Declaration.JE_ECI_InvoiceAmountInfo, warnningMessage);

			Declaration.JE_ECI_InvoiceCurrency = Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "USD").PK;
			Declaration.JE_ECI_InvoiceAmount = 600m;
			AssertEquals(431.65m, Declaration.JE_ECI_InvoiceAmountInLocalCurrency);
			AssertHasWarningContaining(Declaration.JE_ECI_InvoiceAmountInfo, warnningMessage);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.Validation.ValidateJE_ECI_InvoiceAmount();
			AssertNoWarningContaining(Declaration.JE_ECI_InvoiceAmountInfo, warnningMessage);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.Validation.ValidateJE_ECI_InvoiceAmount();
			AssertNoWarningContaining(Declaration.JE_ECI_InvoiceAmountInfo, warnningMessage);
		}

		public void TestValidateFinalDestinationWhenECIAndImport()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_RL_NKPortOfLoading = "HKHKG";
			Declaration.JE_RL_NKPortOfArrival = "NZAKL";
			Declaration.JE_RL_NKOrigin = "HKHKG";
			Declaration.JE_RL_NKFinalDestination = "TOTBU";
			AssertNoNotifications("Final Destination shoud not have an error", Declaration.JE_RL_NKFinalDestinationInfo);
		}

		public void TestPaymentMethodNeverRaisesAnyValidationAsItsNotRequired()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			Declaration.JE_PaymentMethod = "";
			AssertNoMessageErrors(Declaration.JE_PaymentMethodInfo);
			Declaration.JE_PaymentMethod = "XXX";
			AssertNoMessageErrors(Declaration.JE_PaymentMethodInfo);
			Declaration.JE_PaymentMethod = "YYY";
			AssertNoMessageErrors(Declaration.JE_PaymentMethodInfo);
			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
			AssertNoMessageErrors(Declaration.JE_PaymentMethodInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			Declaration.JE_PaymentMethod = "";
			AssertNoMessageErrors(Declaration.JE_PaymentMethodInfo);
			Declaration.JE_PaymentMethod = "XXX";
			AssertNoMessageErrors(Declaration.JE_PaymentMethodInfo);
			Declaration.JE_PaymentMethod = "YYY";
			AssertNoMessageErrors(Declaration.JE_PaymentMethodInfo);
			Declaration.JE_PaymentMethod = PaymentMethodList.Codes.BrokerDeferred;
			AssertNoMessageErrors(Declaration.JE_PaymentMethodInfo);
		}

		public void TestPortOfOriginValidationForExportECIWriteOff()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_RL_NKOrigin = "";
			AssertHasMessageError(Declaration.JE_RL_NKOriginInfo, JobDeclarationValidationECIWriteOff.MessageErrorMissingOriginPortCode);
			Declaration.JE_RL_NKOrigin = "NZAKL";
			AssertHasMessageErrors(Declaration.JE_RL_NKOriginInfo);
			Declaration.JE_RL_NKOrigin = "GBTIL";
			AssertNoMessageErrors(Declaration.JE_RL_NKOriginInfo);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_RL_NKOrigin = "";
			AssertHasMessageError(Declaration.JE_RL_NKOriginInfo, JobDeclarationValidationECIWriteOff.MessageErrorMissingOriginPortCode);
			Declaration.JE_RL_NKOrigin = "NZAKL";
			AssertNoMessageErrors(Declaration.JE_RL_NKOriginInfo);
			Declaration.JE_RL_NKOrigin = "GBTIL";
			AssertNoMessageErrors(Declaration.JE_RL_NKOriginInfo);
		}

		public void TestPackagesValidationOnlyKicksInWhenUsingContainers()
		{
			Declaration.DisableDefaultPackingInformation = true;
			Declaration.JE_TotalNoOfPacks = 20;
			Declaration.JE_TotalNoOfPacksPackType = "PK";
			AssertNoMessageError(Declaration.PackagesActualPackageCountInfo, JobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage);
			Declaration.CusContainers.AddNew();
			AssertHasMessageError(Declaration.PackagesActualPackageCountInfo, JobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage);
			Declaration.CusContainers.RemoveAndDeleteAll();
			AssertNoMessageError(Declaration.PackagesActualPackageCountInfo, JobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage);
		}

		public void TestWeightValidationWhenUsingContainers()
		{
			Declaration.JE_TotalWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			Declaration.JE_TotalWeight = ZDecimal.Zero;
			Declaration.JE_HouseBill = "JAMESNEEDSAHAIRCUT";
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeEqualToContainerWeight);
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeMoreThanContainerWeight);
			CusContainer container = Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000006";
			Declaration.JE_TotalWeight = ZDecimal.Zero;
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeEqualToContainerWeight);
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeMoreThanContainerWeight);
			container.CO_Weight = 10m;
			AssertHasMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeEqualToContainerWeight);
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeMoreThanContainerWeight);
			Package loosePackage = Declaration.Packages.AddNew();
			loosePackage.CW_PackQty = 1;
			loosePackage.CW_PackType = "PK";
			loosePackage.CW_HouseBill = Declaration.PrimaryHouseBill.CU_BillUniqueCode;
			Declaration.JE_TotalWeight = 1m;
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeEqualToContainerWeight);
			AssertHasMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeMoreThanContainerWeight);
			Declaration.JE_TotalWeight = 12m;
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeEqualToContainerWeight);
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeMoreThanContainerWeight);
			Declaration.Packages.RemoveAndDeleteAll();
			Declaration.JE_TotalWeight = 11m;
			AssertHasMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeEqualToContainerWeight);
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeMoreThanContainerWeight);
			Declaration.JE_TotalWeight = 10m;
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeEqualToContainerWeight);
			AssertNoMessageErrorContaining(Declaration.JE_TotalWeightInfo, JobDeclarationValidationECIWriteOff.MessageErrorTotalWeightMustBeMoreThanContainerWeight);
		}

		[TestDate(2050, 1, 1)]
		public void TestValidECIWriteOffDoesntPickupAnyExtraValidationFromBaseChanges()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "PK");
			TestDeclarationCreator.InitialiseFlightVesselReferenceData(Factory);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			var decCreator = new TestECIWriteOffCreator(declaration);
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB(100, "AUD");
			declaration.JE_DateOfArrival = new ZDateTime(2050, 1, 1);
			declaration.JE_EntrySubmittedDate = new ZDateTime(2050, 1, 1);
			declaration.JE_DateAtFinalDestination = new ZDateTime(2050, 1, 1);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			declaration.RunPreSaveValidation();
			AssertNoNotifications(declaration);
		}

		public void TestValidatePackageType()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			refDataHelper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", "PK", "Package", UniversalReferenceConstants.UNPackTypeStartDate, ZDateTime.Today.AddDays(1));
			refDataHelper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", "VI", "Vial", UniversalReferenceConstants.UNPackTypeStartDate, ZDateTime.Today.AddDays(1));
			refDataHelper.CreateCusCodeListWithAttribute("UNE", "UNPKG", "VQ", "Bulk, liquefied gas (abnormal)", UniversalReferenceConstants.UNPackTypeStartDate, ZDateTime.Today.AddDays(1), "BULK", "BULK");
			Factory.Save();

			Declaration.JE_TotalNoOfPacksPackType = "XX";//Invalid
			AssertEquals("Invalid", true, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());

			Declaration.JE_TotalNoOfPacksPackType = Declaration.Lookups.JE_TotalNoOfPacksPackType_List[0].Code;
			AssertEquals("Valid", false, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());

			Declaration.JE_TotalNoOfPacks = 2;
			Declaration.JE_TotalNoOfPacksPackType = "VQ";//Bulk
			AssertEquals("Bulk type should only have 1 pack", true, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());

			Declaration.JE_TotalNoOfPacksPackType = "VI";
			AssertEquals("Valid, but is not a bulk type", false, Declaration.JE_TotalNoOfPacksPackTypeInfo.HasMessageErrors());
		}

		public void TestValidateInvoiceAmount()
		{
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			Declaration.JE_ECI_InvoiceAmount = 0m;
			AssertEquals("Zero dollar for full container is a message error", true, Declaration.JE_ECI_InvoiceAmountInfo.HasMessageErrors());

			Declaration.JE_ECI_InvoiceAmount = 1000m;
			AssertEquals("No message error", false, Declaration.JE_ECI_InvoiceAmountInfo.HasMessageErrors());

			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			Declaration.JE_ECI_InvoiceAmount = 0m;
			AssertEquals("Zero dollar for empty container is OK", false, Declaration.JE_ECI_InvoiceAmountInfo.HasMessageErrors());
		}

		public void TestWriteOffHouseBill()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_HouseBill = ZString.Empty;
			AssertHasMessageErrors(Declaration.JE_HouseBillInfo);
			Declaration.JE_HouseBill = "J003928";
			AssertNoMessageErrors(Declaration.JE_HouseBillInfo);
		}

		public void TestDocumentsZeroValue()
		{
			Declaration.JE_ECI_InvoiceAmount = 0m;
			AssertEquals("Precondition", true, Declaration.JE_ECI_InvoiceAmountInfo.HasMessageErrors());

			Declaration.JE_GoodsDescription = "Documents";
			Declaration.Validation.ValidateJE_ECI_InvoiceAmount();
			AssertEquals("Documents", false, Declaration.JE_ECI_InvoiceAmountInfo.HasMessageErrors());

			Declaration.JE_GoodsDescription = "Docs";
			Declaration.Validation.ValidateJE_ECI_InvoiceAmount();
			AssertEquals("Docs", false, Declaration.JE_ECI_InvoiceAmountInfo.HasMessageErrors());

			Declaration.JE_GoodsDescription = "Docsickle";
			Declaration.Validation.ValidateJE_ECI_InvoiceAmount();
			AssertEquals("Docsickle", true, Declaration.JE_ECI_InvoiceAmountInfo.HasMessageErrors());
		}

		public void TestValidateInvoiceCurrency()
		{
			Declaration.JE_ECI_InvoiceCurrency = ZGuid.Empty;
			AssertHasMessageError(Declaration.JE_ECI_InvoiceCurrencyInfo, JobDeclarationValidationECIWriteOff.MessageErrorPleaseEnterAValidCurrencyCode);

			Declaration.JE_ECI_InvoiceCurrency = Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "AED").PK;
			AssertHasMessageError(Declaration.JE_ECI_InvoiceCurrencyInfo, JobDeclarationValidationECIWriteOff.MessageErrorInvalidCurrencyForNZCustomsPurposes);

			Declaration.JE_ECI_InvoiceCurrency = Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "NZD").PK;
			AssertNoNotifications(Declaration.JE_ECI_InvoiceCurrencyInfo);
		}

		public void TestValidateWeight()
		{
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			Declaration.JE_TotalWeight = 0m;
			AssertEquals("Weight 0 for empty container is OK", false, Declaration.JE_TotalWeightInfo.HasMessageErrors());

			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			Declaration.JE_TotalWeight = 0m;
			AssertEquals("Zero Weight for full container", true, Declaration.JE_TotalWeightInfo.HasMessageErrors());

			Declaration.JE_TotalWeight = 10m;
			Container.CO_Weight = 10m;
			AssertEquals("Valid weight", false, Declaration.JE_TotalWeightInfo.HasMessageErrors());
		}

		public void TestValidateGoodsDescription()
		{
			Declaration.JE_GoodsDescription = ZString.Empty;
			AssertEquals("Empty is a message error", true, Declaration.JE_GoodsDescriptionInfo.HasMessageErrors());

			Declaration.JE_GoodsDescription = "Test Description";
			AssertEquals("Not message error", false, Declaration.JE_GoodsDescriptionInfo.HasMessageErrors());
		}

		public void TestValidateTotalNoOfPacks()
		{
			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.Empty;
			Declaration.JE_TotalNoOfPacks = 0;
			AssertEquals("Empty for empty contaner all right", false, Declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());

			Container.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			Declaration.JE_TotalNoOfPacks = 0;
			AssertEquals("Empty for full contaner is message error", true, Declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());

			Declaration.JE_TotalNoOfPacks = 2;
			AssertEquals("Empty for full contaner is message error", false, Declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
		}

		public void TestValidateConsignee()
		{
			Declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("Empty message error", true, Declaration.JE_OH_ImporterInfo.HasMessageErrors());

			Declaration.JE_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true)).PK;
			AssertEquals("Valid", false, Declaration.JE_OH_ImporterInfo.HasMessageErrors());
		}

		public void TestValidateConsignor()
		{
			Declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals("Empty message error", true, Declaration.JE_OH_SupplierInfo.HasMessageErrors());

			Declaration.JE_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true)).PK;
			AssertEquals("Valid", false, Declaration.JE_OH_SupplierInfo.HasMessageErrors());
		}

		public void TestValidateFinalDestination()
		{
			Declaration.JE_RL_NKFinalDestination = ZString.Empty;

			Declaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertHasMessageError(Declaration.JE_RL_NKFinalDestinationInfo, JobDeclarationValidationECIWriteOff.MessageErrorMustHaveDestinationPortCode);

			Declaration.JE_RL_NKFinalDestination = "ZZZZ";
			AssertHasMessageError(Declaration.JE_RL_NKFinalDestinationInfo, JobDeclarationValidationECIWriteOff.MessageErrorMustHaveDestinationPortCode);

			Declaration.JE_RL_NKFinalDestination = "NZAKL";
			AssertNoNotifications(Declaration.JE_RL_NKFinalDestinationInfo);
		}

		public void TestValidateOrigin()
		{
			Declaration.JE_RL_NKOrigin = ZString.Empty;
			AssertEquals("Empty port of loading", true, Declaration.JE_RL_NKOriginInfo.HasMessageErrors());

			Declaration.JE_RL_NKOrigin = "AUSYD";
			AssertEquals("Port of loading", false, Declaration.JE_RL_NKOriginInfo.HasMessageErrors());
		}

		public void TestValidatePortOfLoading()
		{
			Declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			AssertEquals("Empty port of loading", true, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());

			Declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Port of loading", false, Declaration.JE_RL_NKPortOfLoadingInfo.HasMessageErrors());
		}

		public void TestValidateMasterBill()
		{
			Declaration.JE_MasterBill = "";
			AssertEquals("Valid", false, Declaration.JE_MasterBillInfo.HasMessageErrors());

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MasterBill = "M123456789ABCDEFG005534322";
			AssertNoMessageErrors("MasterBill can now have the full 35 characters", Declaration.JE_MasterBillInfo);
		}

		public void TestValidateHouseBill()
		{
			Declaration.JE_HouseBill = "";
			AssertEquals("Empty House bill", true, Declaration.JE_HouseBillInfo.HasMessageErrors());

			Declaration.JE_HouseBill = "A1234";
			AssertEquals("Valid", false, Declaration.JE_HouseBillInfo.HasMessageErrors());

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_HouseBill = "123456789B123456789C123456789D12345";
			AssertNoMessageErrors("HouseBill can now have full 35 characters", Declaration.JE_HouseBillInfo);
		}

		public void TestValidateArrivalDate()
		{
			Declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals("Date of arrival is necessay for messaging", true, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());

			Declaration.JE_DateOfArrival = new ZDateTime(2001, 12, 30);
			AssertEquals("Date of arrival is necessay for messaging", false, Declaration.JE_DateOfArrivalInfo.HasMessageErrors());
		}

		public void TestValidatePortOfArrival()
		{
			Declaration.JE_RL_NKPortOfArrival = ZString.Empty;
			AssertHasMessageError(Declaration.JE_RL_NKPortOfArrivalInfo, JobDeclarationValidationECIWriteOff.MessageErrorMustHaveDischargePort);

			Declaration.JE_RL_NKPortOfArrival = "NZAKL";
			AssertNoNotifications(Declaration.JE_RL_NKPortOfArrivalInfo);

			Declaration.JE_RL_NKPortOfArrival = "AAAA";
			AssertHasMessageError(Declaration.JE_RL_NKPortOfArrivalInfo, JobDeclarationValidationECIWriteOff.MessageErrorPortCodeInvalid);
		}

		public void TestValidateFlightNumberForAIR()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "QF117", "QF117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertEquals("Empty voyage no message error", true, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			AssertEquals("Empty craft name is not an error for AIR", false, Declaration.JE_VesselNameInfo.HasMessageErrors());

			Declaration.JE_VoyageFlightNo = "QF117";
			AssertEquals("No message error", false, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
		}

		public void TestValidateVoyageNumberForSEA()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "BUNGA BIDARA", "BUNGA BIDARA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertEquals("Empty voyage no message error", true, Declaration.JE_VoyageFlightNoInfo.HasMessageErrors());

			Declaration.JE_VesselName = ZString.Empty;
			AssertEquals("Empty craft name is an error for SEA", true, Declaration.JE_VesselNameInfo.HasMessageErrors());

			Declaration.JE_VesselName = "BUNGA BIDARA";
			AssertEquals("Not empty", false, Declaration.JE_VesselNameInfo.HasMessageErrors());
		}

		public void TestValidateTransportMode()
		{
			Declaration.JE_TransportMode = "AAA";
			AssertEquals("Invalid", true, Declaration.JE_TransportModeInfo.HasErrors());

			Declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("Empty", true, Declaration.JE_TransportModeInfo.HasMessageErrors());

			Declaration.JE_TransportMode = new TransportTypeList()[0].Code;
			AssertEquals("Valid", false, Declaration.JE_TransportModeInfo.HasMessageErrors());
		}

		public void TestShippingLineValidation()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_OH_ShippingLine = ZGuid.Empty;
			AssertEquals("Has Message Error: 'Please enter a shipping line.' when Shipping Line Empty", true, Declaration.JE_OH_ShippingLineInfo.HasMessageError("Please enter a shipping line."));

			Declaration.JE_OH_ShippingLine = ZGuid.NewZGuid();
			AssertEquals("Has Message Error: 'Please enter a shipping line.' when Shipping Line Invalid", true, Declaration.JE_OH_ShippingLineInfo.HasMessageError("Please enter a shipping line."));

			OrgHeader shippingLine = Factory.New<OrgHeader>();
			Declaration.JE_OH_ShippingLine = shippingLine.PK;
			AssertEquals("Should Have No Message Errors", false, Declaration.JE_OH_ShippingLineInfo.HasMessageErrors());
		}

		public void TestValidateAll()
		{
			using (Declaration.SuspendValidationTesting())
			{
				Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
				Declaration.JE_ECI_InvoiceAmount = ZDecimal.Zero;
				Declaration.JE_ECI_InvoiceCurrency = ZGuid.Empty;
				AssertHasMessageError(Declaration.JE_ECI_InvoiceAmountInfo, JobDeclarationValidationECIWriteOff.MessageErrorPleaseEnterInvoiceAmount);
				AssertHasMessageError(Declaration.JE_ECI_InvoiceCurrencyInfo, JobDeclarationValidationECIWriteOff.MessageErrorPleaseEnterAValidCurrencyCode);
				Declaration.ClearAllNotifications();
				AssertNoMessageError(Declaration.JE_ECI_InvoiceAmountInfo, JobDeclarationValidationECIWriteOff.MessageErrorPleaseEnterInvoiceAmount);
				AssertNoMessageError(Declaration.JE_ECI_InvoiceCurrencyInfo, JobDeclarationValidationECIWriteOff.MessageErrorPleaseEnterAValidCurrencyCode);
				Declaration.Validation.ValidateAll();
				AssertHasMessageError(Declaration.JE_ECI_InvoiceAmountInfo, JobDeclarationValidationECIWriteOff.MessageErrorPleaseEnterInvoiceAmount);
				AssertHasMessageError(Declaration.JE_ECI_InvoiceCurrencyInfo, JobDeclarationValidationECIWriteOff.MessageErrorPleaseEnterAValidCurrencyCode);
			}
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			return declaration;
		}
	}
}

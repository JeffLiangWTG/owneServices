using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class CusEntryHeaderProviderTestHelper : IDisposable
	{
		public CusEntryHeaderProviderTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
			Setup();
		}
		readonly BusinessObjectFactory factory;
		GlbStaff loggedInUser;
		IDisposable tempUserContext;

		void Setup()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", false);
			helper.CreateCusMap("CNTRY", "TR", "052", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "NL", "003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "DE", "004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "GB", "006", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "AF", "660", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "IL", "624", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMap("CNTRY", "ES", "011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			helper.CreateTaxOrFee("MUAF", 0, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VAT 0%");
			helper.CreateTaxOrFee("KD1", 0.01, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VAT 1%");
			helper.CreateTaxOrFee("KD10", 0.10, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VAT 10%");
			helper.CreateTaxOrFee("KD20", 0.20, Core.Constants.CountryCodes.Turkey, 0, 0, "VAT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VAT 20%");
			factory.Save();

			loggedInUser = factory.NewWithValidTestData<GlbStaff>();
			loggedInUser.GS_Code = "ULU";
			loggedInUser.GS_FullName = "test ulukom";
			loggedInUser.GS_MobilePhone = "+905322173033";
			tempUserContext = Env.SetTemporaryUserContext(new UserContext(loggedInUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK));

			var emailAddress = loggedInUser.EmailAddresses.AddNew();
			emailAddress.GSE_EmailAddress = "ilker@ulukom.com.tr";
			emailAddress.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;

			var emailAddress2 = loggedInUser.EmailAddresses.AddNew();
			emailAddress2.GSE_EmailAddress = "mehmet@ulukom.com.tr";
			emailAddress2.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Default;

			var emailAddress3 = loggedInUser.EmailAddresses.AddNew();
			emailAddress3.GSE_EmailAddress = "yusuf@ulukom.com.tr";
			emailAddress3.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Default;

			var currencyUSD = factory.New<RefCurrency>();
			currencyUSD.RX_Code = "USD";
			var rateUSD = currencyUSD.ExchangeRates.AddNew();
			rateUSD.RE_StartDate = ZDateTime.Today;
			rateUSD.RE_ExpiryDate = ZDateTime.Today;
			rateUSD.RE_ExRateType = "CUS";
			rateUSD.RE_SellRate = 26m;

			var currencyEUR = factory.New<RefCurrency>();
			currencyEUR.RX_Code = "EUR";
			var rateEUR = currencyEUR.ExchangeRates.AddNew();
			rateEUR.RE_StartDate = ZDateTime.Today;
			rateEUR.RE_ExpiryDate = ZDateTime.Today;
			rateEUR.RE_ExRateType = "CUS";
			rateEUR.RE_SellRate = 29m;

			var currencyTRY = factory.New<RefCurrency>();
			currencyTRY.RX_Code = "TRY";
			var rateTRY = currencyTRY.ExchangeRates.AddNew();
			rateTRY.RE_StartDate = ZDateTime.Today;
			rateTRY.RE_ExpiryDate = ZDateTime.Today;
			rateTRY.RE_ExRateType = "CUS";
			rateTRY.RE_SellRate = 1m;
		}

		void IDisposable.Dispose()
		{
			tempUserContext.Dispose();
		}

		public JobDeclaration GetProviderHeader(string messageType = "IMP")
		{
			var extPassword = factory.New<GlbExternalPassword_TR>();
			extPassword.GP_PasswordType = PasswordTypesList.Codes.TRK;
			extPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			extPassword.GP_UserID = "20201224104";
			extPassword.GP_GS = loggedInUser.PK;
			extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			AddExchangeRates(messageType);

			var jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = messageType;
			jobDeclaration.JE_TransportMode = "ROA";
			jobDeclaration.DeclarationNumber = "21340300IM123456";
			jobDeclaration.JE_CustomsOffice = "TR341200";
			jobDeclaration.JE_EntrySubStyle = "1";
			jobDeclaration.JE_TotalNoOfPacks = 20;
			jobDeclaration.JE_OwnerRef = "ORD1234";
			jobDeclaration.JE_GoodsOrigin = "NL";
			jobDeclaration.JE_GoodsDestination = "DE";
			jobDeclaration.JE_RN_NKTransportNationality = "AF";
			jobDeclaration.JE_ShipmentIncoTerm = "FOB";
			jobDeclaration.JE_ShipmentIncoTermPlace = "ISTANBUL";
			jobDeclaration.JE_TransportModeInland = "30";
			jobDeclaration.DischargePlace = "ERENKÖY GÜMRÜK MÜDÜRLÜĞÜ";
			jobDeclaration.JE_SubLocationOfGoods = "ZEYTİNBURNU";
			jobDeclaration.BondedWarehouseCode = "G002";
			jobDeclaration.EntryOffice = "TR341200";
			jobDeclaration.JE_AddInfo = "İLAVE AÇIKLAMA";
			jobDeclaration.JE_PaymentMethod = "C";
			jobDeclaration.JE_DeclarationReference = "ULU-2021IM/00000005";
			jobDeclaration.JE_VesselName = "TK321";
			jobDeclaration.JE_CustomsDischargePort = "TR01M-004";
			jobDeclaration.JE_CustomsLoadPort = "TR01M-005";
			jobDeclaration.Company.GC_IsReciprocal = true;
			jobDeclaration.ZG_NumberOfDocs = 2;
			jobDeclaration.JE_AgentsReference = "24066666SB000001";

			jobDeclaration.AdditionalInfos.AddNew().CSI_Description = "desc1";
			jobDeclaration.AdditionalInfos.AddNew().CSI_Description = "desc2";
			jobDeclaration.AdditionalInfos.AddNew().CSI_Description = "desc3";

			AddOrganizations(jobDeclaration);
			AddPackages(jobDeclaration);
			AddGuarantee(jobDeclaration);
			AddManifestToOpenBills(jobDeclaration);
			AddDV1Details(jobDeclaration);

			jobDeclaration.ZG_CountryOfSupply = "NL";
			jobDeclaration.ZG_ShippingCountry = "NL";
			jobDeclaration.ZG_Box18TransportID = "TK123";
			jobDeclaration.JE_TransportMeans = "3";
			jobDeclaration.ZG_Box18TransportID = "TK123";
			jobDeclaration.ZG_Box18TransportNationality = "GB";
			jobDeclaration.ZG_BankCode = "000100007026";

			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "21340300IM123456";

			var cusEntryPayInfo = cusEntryHeader.EntryPayInfos.AddNew();
			cusEntryPayInfo.C9_PaymentReference = "ABC";
			cusEntryPayInfo.C9_IncomingPayResponseNo = "DEF";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;
			cusEntryLine.CL_AdValoremTariff = "660320000000";

			var cusEntryLine2 = cusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine2.CL_LineNumber = 2;

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2021, 3, 30);
			invoiceHeader.JZ_InvoiceNumber = "5435345345";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RelatedIndicator = "Y";
			invoiceHeader.JZ_InvoiceCurrExRate = 20m;
			invoiceHeader.JZ_IncoTerm = "FOB";

			AddInvoiceCharges(invoiceHeader);
			AddQuestionsOrWarning(cusEntryHeader);
			AddEntryLine1ToInvoiceLines(cusEntryLine, invoiceHeader);
			AddEntryLine2ToInvoiceLines(cusEntryLine2, invoiceHeader);

			return jobDeclaration;
		}

		public JobDeclaration GetExportProviderHeader()
		{
			var jobDeclaration = GetProviderHeader("EXP");
			return jobDeclaration;
		}

		public void AddExchangeRates(ZString messageType)
		{
			if (messageType == "IMP")
			{
				var currency = RefCurrency.LoadFromCurrencyCode(factory, Core.Constants.CurrencyCodes.UnitedStates);
				var exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-2);
				exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
				exchangeRate.RE_SellRate = 27m;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

				currency = RefCurrency.LoadFromCurrencyCode(factory, Core.Constants.CurrencyCodes.EuropeanUnion);
				exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-2);
				exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
				exchangeRate.RE_SellRate = 30m;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			}
			else
			{
				var currency = RefCurrency.LoadFromCurrencyCode(factory, Core.Constants.CurrencyCodes.UnitedStates);
				var exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
				exchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-2);
				exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
				exchangeRate.RE_SellRate = 27m;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

				currency = RefCurrency.LoadFromCurrencyCode(factory, Core.Constants.CurrencyCodes.EuropeanUnion);
				exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
				exchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-2);
				exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
				exchangeRate.RE_SellRate = 30m;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			}
		}

		#region Header Methods

		void AddOrganizations(JobDeclaration jobDeclaration)
		{
			var orgSupplier = factory.New<OrgHeader>();
			orgSupplier.OH_Code = "xSupplier";
			orgSupplier.OH_FullName = "xSupplier Full Name";
			orgSupplier.OH_RL_NKClosestPort = "TR";
			orgSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "1234567890123");
			orgSupplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890024379");
			orgSupplier.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, "ANKARA");
			var addressSupplier = orgSupplier.MainAddress;
			addressSupplier.OA_OH = orgSupplier.PK;
			addressSupplier.CompanyName = "xSupplier Company Name";
			addressSupplier.Address1 = "xSupplierAdress1";
			addressSupplier.Address2 = "xSupplierAdress2";
			addressSupplier.OA_Phone = "02122122691";
			addressSupplier.OA_Fax = "02122122692";
			addressSupplier.City = "ISTANBUL";
			addressSupplier.Postcode = "340301";
			addressSupplier.OA_RN_NKCountryCode = "TR";
			jobDeclaration.JE_OH_Supplier = orgSupplier.PK;

			var orgConsignee = factory.New<OrgHeader>();
			orgConsignee.OH_Code = "xConsignee";
			orgConsignee.OH_FullName = "xConsignee Full Name";
			orgConsignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890024399");
			orgConsignee.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, "IZMIR");
			var addressConsignee = orgConsignee.MainAddress;
			addressConsignee.OA_OH = orgConsignee.PK;
			addressConsignee.CompanyName = "xConsignee Company Name";
			addressConsignee.Address1 = "xConsigneeAdress1";
			addressConsignee.Address2 = "xConsigneeAdress2";
			addressConsignee.OA_Phone = "02122122693";
			addressConsignee.OA_Fax = "02122122694";
			addressConsignee.City = "IZMIR";
			addressConsignee.Postcode = "340302";
			addressConsignee.OA_RN_NKCountryCode = "TR";
			jobDeclaration.JE_OH_Importer = orgConsignee.PK;

			var orgDeclarant = factory.New<OrgHeader>();
			orgDeclarant.OH_Code = "xDeclarant";
			orgDeclarant.OH_FullName = "xDeclarant Full Name";
			orgDeclarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890024444");
			orgDeclarant.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, "ADANA");
			var addressDeclarant = orgDeclarant.MainAddress;
			addressDeclarant.OA_OH = orgDeclarant.PK;
			addressDeclarant.CompanyName = "xDeclarant Company Name";
			addressDeclarant.Address1 = "xDeclarantAdress1";
			addressDeclarant.Address2 = "xDeclarantAdress2";
			addressDeclarant.OA_Phone = "02122122695";
			addressDeclarant.OA_Fax = "02122122696";
			addressDeclarant.City = "ANKARA";
			addressDeclarant.Postcode = "340303";
			addressDeclarant.OA_RN_NKCountryCode = "TR";
			jobDeclaration.JE_OA_DeclarantAddress = addressDeclarant.PK;

			var orgAdvisor = factory.New<OrgHeader>();
			orgAdvisor.OH_Code = "xAdvisor";
			orgAdvisor.OH_FullName = "xAdvisor Full Name";
			orgAdvisor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8890024555");
			orgAdvisor.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, "VAN");
			var addressAdvisor = orgAdvisor.MainAddress;
			addressAdvisor.OA_OH = orgAdvisor.PK;
			addressAdvisor.CompanyName = "xAdvisor Company Name";
			addressAdvisor.Address1 = "xAdvisorAdress1";
			addressAdvisor.Address2 = "xAdvisorAdress2";
			addressAdvisor.OA_Phone = "02122122697";
			addressAdvisor.OA_Fax = "02122122698";
			addressAdvisor.City = "BERLIN";
			addressAdvisor.Postcode = "340304";
			addressAdvisor.OA_RN_NKCountryCode = "DE";
			jobDeclaration.JE_OA_Representative = orgAdvisor.MainAddress.PK;

			var orgTrader = factory.New<OrgHeader>();
			orgTrader.OH_Code = "xTrader";
			orgTrader.OH_FullName = "xTrader Full Name";
			orgTrader.OH_RL_NKClosestPort = "TR";
			orgTrader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "3234567890");
			orgTrader.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, "SAMSUN");
			var addressTrader = orgTrader.MainAddress;
			addressTrader.OA_OH = orgTrader.PK;
			addressTrader.CompanyName = "xTrader Company Name";
			addressTrader.Address1 = "xTraderAdress1";
			addressTrader.Address2 = "xTraderAdress2";
			addressTrader.OA_Phone = "02122122699";
			addressTrader.OA_Fax = "02122122600";
			addressTrader.City = "MADRID";
			addressTrader.Postcode = "340305";
			addressTrader.OA_RN_NKCountryCode = "ES";
			var trader = jobDeclaration.Traders.AddNew();
			trader.E2_AddressType = DocAddressTypes.Codes.BuyerDocumentaryAddress;
			trader.E2_OA_Address = addressTrader.PK;

			var orgTrader2 = factory.New<OrgHeader>();
			orgTrader2.OH_Code = "xTrader2";
			orgTrader2.OH_FullName = "xTrader2 Full Name";
			orgTrader2.OH_RL_NKClosestPort = "IL";
			orgTrader2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "4234567890");
			orgTrader2.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, "SAKARYA");
			var addressTrader2 = orgTrader2.MainAddress;
			addressTrader2.OA_OH = orgTrader2.PK;
			addressTrader2.CompanyName = "xTrader2 Company Name";
			addressTrader2.Address1 = "xTrader2Adress1";
			addressTrader2.Address2 = "xTrader2Adress2";
			addressTrader2.OA_Phone = "02122122601";
			addressTrader2.OA_Fax = "02122122602";
			addressTrader2.City = "TEL AVIV";
			addressTrader2.Postcode = "340306";
			addressTrader2.OA_RN_NKCountryCode = "IL";
			var trader2 = jobDeclaration.Traders.AddNew();
			trader2.E2_AddressType = DocAddressTypes.Codes.SellerDocumentaryAddress;
			trader2.E2_OA_Address = addressTrader2.PK;
		}

		void AddPackages(JobDeclaration jobDeclaration)
		{
			var package = jobDeclaration.Packages.AddNew();
			package.CW_PackType = "BI";
			package.CW_PackQty = 10;
			package.CW_MarksAndNos = "1111";

			var package2 = jobDeclaration.Packages.AddNew();
			package2.CW_PackType = "BI";
			package2.CW_PackQty = 10;
			package2.CW_MarksAndNos = "2222";

			var package3 = jobDeclaration.Packages.AddNew();
			package3.CW_PackType = "BI";
			package3.CW_PackQty = 8;
			package3.CW_MarksAndNos = "3333";

			var package4 = jobDeclaration.Packages.AddNew();
			package4.CW_PackType = "BI";
			package4.CW_PackQty = 7;
			package4.CW_MarksAndNos = "4444";
		}

		void AddGuarantee(JobDeclaration jobDeclaration)
		{
			var guarantees = jobDeclaration.Guarantees.AddNew();
			guarantees.PW_BondType = "GLOBAL";
			guarantees.PW_BondAmount = 100m;
			guarantees.PW_BondNumber = "string";
			guarantees.PW_GuaranteeDescription = "string";

			var guarantees2 = jobDeclaration.Guarantees.AddNew();
			guarantees2.PW_BondType = "DIGER";
			guarantees2.PW_BondAmount = 100m;
			guarantees2.PW_BondNumber = "string";
			guarantees2.PW_GuaranteeDescription = "string";
		}

		void AddInvoiceCharges(JobComInvoiceHeader invoiceHeader)
		{
			var invoiceHeaderChargeOFT = invoiceHeader.Charges["OFT"] ?? invoiceHeader.Charges.AddNew("OFT");
			invoiceHeaderChargeOFT.J7_Amount = 100m;
			invoiceHeaderChargeOFT.J7_RX_NKCurrency = "USD";

			var invoiceHeaderChargeONS = invoiceHeader.Charges["ONS"] ?? invoiceHeader.Charges.AddNew("ONS");
			invoiceHeaderChargeONS.J7_Amount = 30m;
			invoiceHeaderChargeONS.J7_RX_NKCurrency = "USD";

			#region Foreign

			var invoiceHeaderChargeCOM = invoiceHeader.Charges["COM"] ?? invoiceHeader.Charges.AddNew("COM");
			invoiceHeaderChargeCOM.J7_Amount = 20m;
			invoiceHeaderChargeCOM.J7_RX_NKCurrency = "EUR";

			var invoiceHeaderChargeDEM = invoiceHeader.Charges["DEM"] ?? invoiceHeader.Charges.AddNew("DEM");
			invoiceHeaderChargeDEM.J7_Amount = 20m;
			invoiceHeaderChargeDEM.J7_RX_NKCurrency = "EUR";

			var invoiceHeaderChargeINT = invoiceHeader.Charges["INT"] ?? invoiceHeader.Charges.AddNew("INT");
			invoiceHeaderChargeINT.J7_Amount = 20m;
			invoiceHeaderChargeINT.J7_RX_NKCurrency = "EUR";

			var invoiceHeaderChargeROY = invoiceHeader.Charges["ROY"] ?? invoiceHeader.Charges.AddNew("ROY");
			invoiceHeaderChargeROY.J7_Amount = 20m;
			invoiceHeaderChargeROY.J7_RX_NKCurrency = "EUR";

			var invoiceHeaderChargeOTH = invoiceHeader.Charges["OTH"] ?? invoiceHeader.Charges.AddNew("OTH");
			invoiceHeaderChargeOTH.J7_Amount = 20m;
			invoiceHeaderChargeOTH.J7_RX_NKCurrency = "EUR";

			#endregion

			#region Domestic

			var invoiceHeaderChargeLTC = invoiceHeader.Charges["LTC"] ?? invoiceHeader.Charges.AddNew("LTC");
			invoiceHeaderChargeLTC.J7_Amount = 350m;
			invoiceHeaderChargeLTC.J7_RX_NKCurrency = "TRY";

			var invoiceHeaderChargeLPC = invoiceHeader.Charges["LPC"] ?? invoiceHeader.Charges.AddNew("LPC");
			invoiceHeaderChargeLPC.J7_Amount = 350m;
			invoiceHeaderChargeLPC.J7_RX_NKCurrency = "TRY";

			var invoiceHeaderChargeLBC = invoiceHeader.Charges["LBC"] ?? invoiceHeader.Charges.AddNew("LBC");
			invoiceHeaderChargeLBC.J7_Amount = 350m;
			invoiceHeaderChargeLBC.J7_RX_NKCurrency = "TRY";

			var invoiceHeaderChargeLDC = invoiceHeader.Charges["LDC"] ?? invoiceHeader.Charges.AddNew("LDC");
			invoiceHeaderChargeLDC.J7_Amount = 350m;
			invoiceHeaderChargeLDC.J7_RX_NKCurrency = "TRY";

			var invoiceHeaderChargeLOT = invoiceHeader.Charges["LOT"] ?? invoiceHeader.Charges.AddNew("LOT");
			invoiceHeaderChargeLOT.J7_Amount = 350m;
			invoiceHeaderChargeLOT.J7_RX_NKCurrency = "TRY";

			var invoiceHeaderChargeLSC = invoiceHeader.Charges["LSC"] ?? invoiceHeader.Charges.AddNew("LSC");
			invoiceHeaderChargeLSC.J7_Amount = 350m;
			invoiceHeaderChargeLSC.J7_RX_NKCurrency = "TRY";

			#endregion
		}

		void AddManifestToOpenBills(JobDeclaration jobDeclaration)
		{
			var header = jobDeclaration.ManifestToOpenHeaders.AddNew();
			header.CE_EntryNum = "21067777IM123456";

			var manifestToOpenBills1 = header.Bills.AddNew();
			manifestToOpenBills1.TPD_DocumentNumber = "TS1";
			manifestToOpenBills1.TPD_IncludeAllItems = ZBool.False;
			manifestToOpenBills1.TPD_IsInWarehouse = ZBool.True;
			manifestToOpenBills1.TPD_IsOtherProcedure = ZBool.True;

			var packs1 = manifestToOpenBills1.Packs.AddNew();
			packs1.TPI_LineNumber = 1;
			packs1.TPI_WarehouseCode = "A00003";
			packs1.TPI_Quantity = 50;

			var packs2 = manifestToOpenBills1.Packs.AddNew();
			packs2.TPI_LineNumber = 2;
			packs2.TPI_WarehouseCode = "A00004";
			packs2.TPI_Quantity = 50;

			var header2 = jobDeclaration.ManifestToOpenHeaders.AddNew();
			header2.CE_EntryNum = "21067777IM123457";
			var manifestToOpenBills2 = header2.Bills.AddNew();
			manifestToOpenBills2.TPD_DocumentNumber = "TS2";
			manifestToOpenBills2.TPD_IncludeAllItems = ZBool.False;
			manifestToOpenBills2.TPD_IsInWarehouse = ZBool.True;
			manifestToOpenBills2.TPD_IsOtherProcedure = ZBool.True;

			var packs3 = manifestToOpenBills2.Packs.AddNew();
			packs3.TPI_LineNumber = 1;

			var packs4 = manifestToOpenBills2.Packs.AddNew();
			packs4.TPI_LineNumber = 1;

			var packs5 = manifestToOpenBills2.Packs.AddNew();
			packs5.TPI_LineNumber = 1;

			var manifestToOpenBills22 = header2.Bills.AddNew();
			manifestToOpenBills22.TPD_DocumentNumber = "TS2-2";
			manifestToOpenBills22.TPD_IncludeAllItems = ZBool.False;
			manifestToOpenBills22.TPD_IsInWarehouse = ZBool.True;
			manifestToOpenBills22.TPD_IsOtherProcedure = ZBool.True;

			var packs221 = manifestToOpenBills22.Packs.AddNew();
			packs221.TPI_LineNumber = 1;

			var header3 = jobDeclaration.ManifestToOpenHeaders.AddNew();
			header3.CE_EntryNum = "21067777IM123458";
			var manifestToOpenBills3 = header3.Bills.AddNew();
			manifestToOpenBills3.TPD_DocumentNumber = "TS3";
			manifestToOpenBills3.TPD_IncludeAllItems = ZBool.True;
			manifestToOpenBills3.TPD_IsInWarehouse = ZBool.True;
			manifestToOpenBills3.TPD_IsOtherProcedure = ZBool.True;
		}

		void AddDV1Details(JobDeclaration jobDeclaration)
		{
			var dv1Details = jobDeclaration.DV1Details.AddNew();
			dv1Details.Sequence = 1;
			dv1Details.DV1_ContractDate = new ZDate(2021, 4, 13);
			dv1Details.DV1_ContractNumber = "sozlesmeNo";
			dv1Details.DV1_CustomsDecisionNumber = "kararNo";
			dv1Details.DV1_CustomsDecisionDate = new ZDate(2021, 4, 21);
			dv1Details.DV1_Relationship = "Y";
			dv1Details.DV1_PriceInfluence = "Y";
			dv1Details.DV1_CloseApproximation = "Y";
			dv1Details.DV1_RelationDetails = "Alıcı Satıcı açıklaması";
			dv1Details.DV1_Restrictions = "Y";
			dv1Details.DV1_Consideration = "Y";
			dv1Details.DV1_RestrictionConsiderationDetails = "Kısıtlamalar ayrıntıları";
			dv1Details.DV1_RoyaltiesLicence = "Y";
			dv1Details.DV1_RoyaltiesLicenceDetails = "Koşullar";
			dv1Details.DV1_Resale = "Y";
			dv1Details.DV1_ResaleDetails = "Koşullar b";
			dv1Details.DV1_Place = "İSTANBUL";
		}

		void AddQuestionsOrWarning(CusEntryHeader cusEntryHeader)
		{
			var cpDecCollection = cusEntryHeader.CPDecCollection.AddNew();
			cpDecCollection.ON_QuestionType = "Q";
			cpDecCollection.ON_CPDecNum = 1024;
			cpDecCollection.ON_AnswerCode = "Y";
		}

		#endregion

		#region Entry Line Methods

		void AddEntryLine1ToInvoiceLines(CusEntryLine cusEntryLine, JobComInvoiceHeader invoiceHeader)
		{
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.JI_Tariff = "123456789";
			invoiceLine.JI_BrandName = "VOLVO";
			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_PreviousEntryNumber = "11111";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			invoiceLine.ZG_ProcessingDescription = "pre desc1";
			invoiceLine.JI_CustomsSecondUnitQty = "C62";
			invoiceLine.ZG_ReturnToOrigin = true;
			invoiceLine.ZG_SecondaryTreatedProduct = true;
			invoiceLine.ZG_InwardProcessingLicenseLineNumber = "1";
			invoiceLine.JI_CustomsSecondQuantity = 200;
			invoiceLine.JI_ZZF_NKTaxType = "KD20";
			invoiceLine.ZG_UsedGoodsCode = "K1";
			invoiceLine.ZG_CommercialPaymentCode = "11";
			invoiceLine.ZG_EntryExitPurposeCode = "01";
			invoiceLine.ZG_EntryExitPurposeDetail = "descOfPurpose";
			invoiceLine.ZG_RW_NKBorderTradeStateCode = "34";
			invoiceLine.ZG_ReturningGoodsReasonCode = "21";
			invoiceLine.ZG_ReturningGoodsReasonDetail = "descOfReturningGoodsReason";
			invoiceLine.JI_CountryOfOrigin = "NL";
			invoiceLine.JI_Weight = 200.00;
			invoiceLine.JI_NetWeight = 180.00;
			invoiceLine.JI_CustomsSecondUnitQty = "C62";
			invoiceLine.JI_CustomsSecondQuantity = 200.00;
			invoiceLine.JI_PrimaryPreference = "AT";
			invoiceLine.JI_CustomsThirdUnitQty = "AYR";
			invoiceLine.JI_CustomsThirdQuantity = 180.0000;
			invoiceLine.JI_CustomsFourthUnitQty = "KFO";
			invoiceLine.JI_CustomsFourthQuantity = 180.0000;
			invoiceLine.JI_CustomsFifthUnitQty = "GSM";
			invoiceLine.JI_CustomsFifthQuantity = 145.0000;
			invoiceLine.JI_SupplementaryCode1 = "7012";
			invoiceLine.JI_LinePrice = 3000.00;
			invoiceLine.JI_NDescription = "Şemsiye";
			invoiceLine.JI_BrandName = "ADDR";
			invoiceLine.ZG_CommercialPaymentCode = "XX";
			invoiceLine.ZG_CommercialPaymentAmount = 100;
			invoiceLine.ZG_CommercialPaymentNumber = "111";
			invoiceLine.ZG_PriceType = "01";
			invoiceLine.JI_ValuationCode = "11";
			invoiceLine.ZG_ExportUnionProductionYear = 2023;
			invoiceLine.ZG_ExportUnionThreadCode = "A0002";
			invoiceLine.ZG_ExportUnionPackCode = "A0001";

			var orgManufacturer = factory.New<OrgHeader>();
			orgManufacturer.OH_Code = "xManufactur";
			orgManufacturer.OH_FullName = "xManufacturer Company Name";
			orgManufacturer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "8899665544");
			orgManufacturer.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, "SAMSUN");
			var addressManufacturer = orgManufacturer.MainAddress;
			addressManufacturer.OA_OH = orgManufacturer.PK;
			addressManufacturer.CompanyName = "xManufacturer Company Name";
			addressManufacturer.Address1 = "xAdress1";
			addressManufacturer.Address2 = "xAdress2";
			addressManufacturer.OA_Phone = "02122122692";
			addressManufacturer.OA_Fax = "02122122692";
			addressManufacturer.City = "IST";
			addressManufacturer.Postcode = "340300";
			addressManufacturer.OA_RN_NKCountryCode = "TR";
			invoiceLine.ManufacturerOrgPK = orgManufacturer.PK;

			AddInvoiceLineSupplementaryCodes(invoiceLine);
			AddInvoiceLineCharges(invoiceLine);
			AddInvoiceLineAddInfo(invoiceLine, "ek bilgi 1");
			AddInvoiceLineAddInfo(invoiceLine, "ek bilgi 2");
			AddEntryLineQuestionsOrWarning(cusEntryLine);

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_Procedure = "4000";
			invoiceLine2.JI_Tariff = "123456789";
			invoiceLine2.JI_BrandName = "VOLVO";
			invoiceLine2.JI_LinePrice = 200m;
			invoiceLine2.JI_CustomsQuantity = 20;
			invoiceLine2.JI_PreviousEntryNumber = "11111";
			invoiceLine2.JI_PreviousEntryLineNumber = 1;
			invoiceLine2.ZG_ProcessingDescription = "pre desc2";
			invoiceLine2.ZG_CommercialPaymentCode = "XX";
			invoiceLine2.ZG_CommercialPaymentAmount = 100;
			invoiceLine2.ZG_CommercialPaymentNumber = "111";
			invoiceLine2.ZG_PriceType = "02";
			invoiceLine2.ZG_ExportUnionProductionYear = 2023;
			invoiceLine2.ZG_ExportUnionThreadCode = "A0002";
			invoiceLine2.ZG_ExportUnionPackCode = "A0001";

			AddInvoiceLineCharges(invoiceLine2);
			AddInvoiceLineAddInfo(invoiceLine2, "ek bilgi 3");
			if (!invoiceHeader.IsExport)
			{
				AddInvoiceLineVehicle(invoiceLine, invoiceLine2);
			}

			var packagePiv1 = invoiceLine.PackagesPivot.AddNew();
			packagePiv1.CHC_CW = invoiceHeader.JobDeclaration.Packages[0].PK;
			packagePiv1.CHC_NumberOfPacks = 5;

			var packagePiv2 = invoiceLine2.PackagesPivot.AddNew();
			packagePiv2.CHC_CW = invoiceHeader.JobDeclaration.Packages[1].PK;
			packagePiv2.CHC_NumberOfPacks = 4;

			AddInvoiceLineDocuments(invoiceLine, invoiceLine2);
			AddEntryLineTaxes(cusEntryLine);
			AddInvoiceLineContainers(invoiceHeader, invoiceLine, invoiceLine2);
			AddInvoiceLineAviationFuelType(invoiceLine);
		}

		void AddEntryLine2ToInvoiceLines(CusEntryLine cusEntryLine, JobComInvoiceHeader invoiceHeader)
		{
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;
			invoiceLine.JI_Procedure = "4000";
			invoiceLine.JI_Tariff = "123456789";
			invoiceLine.JI_BrandName = "VOLVO";
			invoiceLine.JI_LinePrice = 200m;
			invoiceLine.JI_CustomsQuantity = 20;
			invoiceLine.JI_PreviousEntryNumber = "11111";
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			invoiceLine.ZG_ProcessingDescription = "pre desc1";
			invoiceLine.JI_CustomsSecondUnitQty = "C62";
			invoiceLine.ZG_ReturnToOrigin = true;
			invoiceLine.ZG_SecondaryTreatedProduct = true;
			invoiceLine.ZG_InwardProcessingLicenseLineNumber = "1";
			invoiceLine.JI_CustomsSecondQuantity = 200;
			invoiceLine.JI_ZZF_NKTaxType = "KD10";
			invoiceLine.ZG_UsedGoodsCode = "K1";
			invoiceLine.ZG_CommercialPaymentCode = "11";
			invoiceLine.ZG_EntryExitPurposeCode = "01";
			invoiceLine.ZG_EntryExitPurposeDetail = "descOfPurpose";
			invoiceLine.ZG_RW_NKBorderTradeStateCode = "34";
			invoiceLine.ZG_ReturningGoodsReasonCode = "21";
			invoiceLine.ZG_ReturningGoodsReasonDetail = "descOfReturningGoodsReason";
			invoiceLine.ZG_ExportUnionProductionYear = 2023;
			invoiceLine.ZG_ExportUnionThreadCode = "A0002";
			invoiceLine.ZG_ExportUnionPackCode = "A0001";

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 4;
			invoiceLine2.JI_CL = cusEntryLine.PK;
			invoiceLine2.JI_Procedure = "4000";
			invoiceLine2.JI_Tariff = "123456789";
			invoiceLine2.JI_BrandName = "VOLVO";
			invoiceLine2.JI_LinePrice = 200m;
			invoiceLine2.JI_CustomsQuantity = 20;
			invoiceLine2.JI_PreviousEntryNumber = "22222";
			invoiceLine2.JI_PreviousEntryLineNumber = 1;
			invoiceLine2.ZG_ProcessingDescription = "pre desc4";
			invoiceLine2.ZG_ExportUnionProductionYear = 2023;
			invoiceLine2.ZG_ExportUnionThreadCode = "A0002";
			invoiceLine2.ZG_ExportUnionPackCode = "A0001";
			if (!invoiceHeader.IsExport)
			{
				AddInvoiceLineVehicle(invoiceLine, invoiceLine2);
			}

			var packagePiv1 = invoiceLine.PackagesPivot.AddNew();
			packagePiv1.CHC_CW = invoiceHeader.JobDeclaration.Packages[2].PK;
			packagePiv1.CHC_NumberOfPacks = 1;

			var packagePiv2 = invoiceLine2.PackagesPivot.AddNew();
			packagePiv2.CHC_CW = invoiceHeader.JobDeclaration.Packages[3].PK;
			packagePiv2.CHC_NumberOfPacks = 1;

			AddInvoiceLineContainers(invoiceHeader, invoiceLine, invoiceLine2);
		}

		void AddEntryLineTaxes(CusEntryLine cusEntryLine)
		{
			var tax = cusEntryLine.Fees.AddNew();
			tax.NationalFeeTypeCode = "10";
			tax.CF_ChargeAmount = 620.76;
			tax.CF_Rate = 33.59;
			tax.G4_MethodOfPayment = "P";
			tax.G4_BaseAmount = 1848.04m;

			var tax2 = cusEntryLine.Fees.AddNew();
			tax2.NationalFeeTypeCode = "40";
			tax2.CF_ChargeAmount = 100;
			tax2.CF_Rate = 10;
			tax2.G4_MethodOfPayment = "C";
			tax2.G4_BaseAmount = 1000m;
		}

		void AddEntryLineQuestionsOrWarning(CusEntryLine cusEntryLine)
		{
			var cpDecCollection1 = cusEntryLine.CPDecCollection.AddNew();
			cpDecCollection1.ON_QuestionType = "Q";
			cpDecCollection1.ON_CPDecNum = 1025;
			cpDecCollection1.ON_AnswerCode = "Y";

			var cpDecCollection2 = cusEntryLine.CPDecCollection.AddNew();
			cpDecCollection2.ON_QuestionType = "Q";
			cpDecCollection2.ON_CPDecNum = 1026;
			cpDecCollection2.ON_AnswerCode = "N";

			var cpDecCollection3 = cusEntryLine.CPDecCollection.AddNew();
			cpDecCollection3.ON_QuestionType = "W";
			cpDecCollection3.ON_CPDecNum = 1027;
		}

		#endregion

		#region Invoice Line Methods

		void AddInvoiceLineAddInfo(JobComInvoiceLine invoiceLine, ZString description)
		{
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_Description = description;
		}

		public void AddInvoiceLineVehicle(JobComInvoiceLine invoiceLine1, JobComInvoiceLine invoiceLine2)
		{
			var vehicle1 = invoiceLine1.Vehicles.AddNew();
			vehicle1.CVH_RegistrationNumber = "232423";
			vehicle1.CVH_SerialNumber = "444555232";
			vehicle1.CVH_ModelYear = "2021";
			vehicle1.CVH_ModelName = "XC60";
			vehicle1.CVH_Color = "MAVİ";
			vehicle1.CVH_VehicleIdentificationNumber = "12345678901234567";
			vehicle1.CVH_Gears = 1;
			vehicle1.CVH_IMEINo = "IMEI NO";
			vehicle1.CVH_BrandName = "BRAND";

			var engine1 = vehicle1.Engine;
			engine1.CEG_CapacityCC = 2000;
			engine1.CEG_Cylinders = 4;
			engine1.CEG_EngineType = "1";
			engine1.CEG_CapacityHP = 200;

			var vehicle2 = invoiceLine1.Vehicles.AddNew();
			vehicle2.CVH_RegistrationNumber = ZString.Empty;
			vehicle2.CVH_SerialNumber = "444555233";
			vehicle2.CVH_ModelYear = "2021";
			vehicle2.CVH_ModelName = "XC60";
			vehicle2.CVH_Color = "MAVİ";
			vehicle2.CVH_VehicleIdentificationNumber = "12345678901234568";
			vehicle2.CVH_Gears = 1;
			vehicle2.CVH_IMEINo = "IMEI NO";
			vehicle2.CVH_BrandName = "BRAND";

			var engine2 = vehicle2.Engine;
			engine2.CEG_CapacityCC = 2000;
			engine2.CEG_Cylinders = 4;
			engine2.CEG_EngineType = "1";
			engine2.CEG_CapacityHP = 200;

			var vehicle3 = invoiceLine2.Vehicles.AddNew();
			vehicle3.CVH_RegistrationNumber = "232423";
			vehicle3.CVH_SerialNumber = "444555232";
			vehicle3.CVH_ModelYear = "2021";
			vehicle3.CVH_ModelName = "XC60";
			vehicle3.CVH_Color = "MAVİ";
			vehicle3.CVH_VehicleIdentificationNumber = "12345678901234561";
			vehicle3.CVH_Gears = 1;
			vehicle3.CVH_IMEINo = "IMEI NO";
			vehicle3.CVH_BrandName = "BRAND";

			var engine3 = vehicle3.Engine;
			engine3.CEG_CapacityCC = 2000;
			engine3.CEG_Cylinders = 4;
			engine3.CEG_EngineType = "1";
			engine3.CEG_CapacityHP = 200;

			var vehicle4 = invoiceLine2.Vehicles.AddNew();
			vehicle4.CVH_RegistrationNumber = ZString.Empty;
			vehicle4.CVH_SerialNumber = "444555233";
			vehicle4.CVH_ModelYear = "2021";
			vehicle4.CVH_ModelName = "XC60";
			vehicle4.CVH_Color = "MAVİ";
			vehicle4.CVH_VehicleIdentificationNumber = "12345678901234562";
			vehicle4.CVH_Gears = 1;
			vehicle4.CVH_IMEINo = "IMEI NO";
			vehicle4.CVH_BrandName = "BRAND";

			var engine4 = vehicle4.Engine;
			engine4.CEG_CapacityCC = 2000;
			engine4.CEG_Cylinders = 4;
			engine4.CEG_EngineType = "1";
			engine4.CEG_CapacityHP = 200;
		}

		void AddInvoiceLineContainers(JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine1, JobComInvoiceLine invoiceLine2)
		{
			var container1 = invoiceHeader.JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "11111";
			container1.CO_ContainerSize = "40";
			container1.CO_Weight = 7.8m;
			container1.CO_WeightUQ = "T";
			container1.CO_RN_NKOwnerCountry = "TR";

			var container2 = invoiceHeader.JobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "22222";
			container2.CO_RN_NKOwnerCountry = "GB";

			var container3 = invoiceHeader.JobDeclaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "33333";
			container3.CO_RN_NKOwnerCountry = "DE";

			invoiceLine1.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoiceLine1.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
		}

		void AddInvoiceLineCharges(JobComInvoiceLine invoiceLine)
		{
			var invoiceLineChargeOFT = invoiceLine.Charges.AddNew();
			invoiceLineChargeOFT.J7_ChargeType = "OFT";
			invoiceLineChargeOFT.J7_Amount = 100m;
			invoiceLineChargeOFT.J7_RX_NKCurrency = "USD";
			invoiceLineChargeOFT.J7_IsStatisticalValueApplicable = true;
			invoiceLineChargeOFT.Explanation = "OFT Desc";

			var invoiceLineChargeONS = invoiceLine.Charges.AddNew();
			invoiceLineChargeONS.J7_ChargeType = "ONS";
			invoiceLineChargeONS.J7_Amount = 30m;
			invoiceLineChargeONS.J7_RX_NKCurrency = "USD";
			invoiceLineChargeONS.J7_IsStatisticalValueApplicable = true;
			invoiceLineChargeONS.Explanation = "ONS Desc";

			#region Domestic

			var invoiceLineChargeLOT = invoiceLine.Charges.AddNew();
			invoiceLineChargeLOT.J7_ChargeType = "LOT";
			invoiceLineChargeLOT.J7_Amount = 1m;
			invoiceLineChargeLOT.J7_RX_NKCurrency = "TRY";
			invoiceLineChargeLOT.Explanation = "Domestic Explanation";

			var invoiceLineChargeLBC = invoiceLine.Charges.AddNew();
			invoiceLineChargeLBC.J7_ChargeType = "LBC";
			invoiceLineChargeLBC.J7_Amount = 2m;
			invoiceLineChargeLBC.J7_RX_NKCurrency = "TRY";

			var invoiceLineChargeLSC = invoiceLine.Charges.AddNew();
			invoiceLineChargeLSC.J7_ChargeType = "LSC";
			invoiceLineChargeLSC.J7_Amount = 3m;
			invoiceLineChargeLSC.J7_RX_NKCurrency = "TRY";

			var invoiceLineChargeLDC = invoiceLine.Charges.AddNew();
			invoiceLineChargeLDC.J7_ChargeType = "LDC";
			invoiceLineChargeLDC.J7_Amount = 4m;
			invoiceLineChargeLDC.J7_RX_NKCurrency = "TRY";

			var invoiceLineChargeLPC = invoiceLine.Charges.AddNew();
			invoiceLineChargeLPC.J7_ChargeType = "LPC";
			invoiceLineChargeLPC.J7_Amount = 5m;
			invoiceLineChargeLPC.J7_RX_NKCurrency = "TRY";

			var invoiceLineChargeLCC = invoiceLine.Charges.AddNew();
			invoiceLineChargeLCC.J7_ChargeType = "LCC";
			invoiceLineChargeLCC.J7_Amount = 6m;
			invoiceLineChargeLCC.J7_RX_NKCurrency = "TRY";

			var invoiceLineChargeLRU = invoiceLine.Charges.AddNew();
			invoiceLineChargeLRU.J7_ChargeType = "LRU";
			invoiceLineChargeLRU.J7_Amount = 7m;
			invoiceLineChargeLRU.J7_RX_NKCurrency = "TRY";

			var invoiceLineChargeLTC = invoiceLine.Charges.AddNew();
			invoiceLineChargeLTC.J7_ChargeType = "LTC";
			invoiceLineChargeLTC.J7_Amount = 8m;
			invoiceLineChargeLTC.J7_RX_NKCurrency = "TRY";

			var invoiceLineChargeLEC = invoiceLine.Charges.AddNew();
			invoiceLineChargeLEC.J7_ChargeType = "LEC";
			invoiceLineChargeLEC.J7_Amount = 9m;
			invoiceLineChargeLEC.J7_RX_NKCurrency = "TRY";

			#endregion

			#region Foreign

			var invoiceLineChargeCOM = invoiceLine.Charges.AddNew();
			invoiceLineChargeCOM.J7_ChargeType = "COM";
			invoiceLineChargeCOM.J7_Amount = 10m;
			invoiceLineChargeCOM.J7_RX_NKCurrency = "USD";

			var invoiceLineChargeDEM = invoiceLine.Charges.AddNew();
			invoiceLineChargeDEM.J7_ChargeType = "DEM";
			invoiceLineChargeDEM.J7_Amount = 20m;
			invoiceLineChargeDEM.J7_RX_NKCurrency = "USD";

			var invoiceLineChargeROY = invoiceLine.Charges.AddNew();
			invoiceLineChargeROY.J7_ChargeType = "ROY";
			invoiceLineChargeROY.J7_Amount = 30m;
			invoiceLineChargeROY.J7_RX_NKCurrency = "USD";

			var invoiceLineChargeOTH = invoiceLine.Charges.AddNew();
			invoiceLineChargeOTH.J7_ChargeType = "OTH";
			invoiceLineChargeOTH.J7_Amount = 50m;
			invoiceLineChargeOTH.J7_RX_NKCurrency = "USD";
			invoiceLineChargeOTH.Explanation = "Overseas Explanation";

			var invoiceLineChargeTFC = invoiceLine.Charges.AddNew();
			invoiceLineChargeTFC.J7_ChargeType = "TFC";
			invoiceLineChargeTFC.J7_Amount = 60m;
			invoiceLineChargeTFC.J7_RX_NKCurrency = "USD";
			invoiceLineChargeTFC.Explanation = "Total Foreign";

			var invoiceLineChargeINT = invoiceLine.ApportionedCharges.AddNew();
			invoiceLineChargeINT.J7_ChargeType = "INT";
			invoiceLineChargeINT.J7_Amount = 40m;
			invoiceLineChargeINT.J7_RX_NKCurrency = "USD";

			#endregion
		}

		void AddInvoiceLineDocuments(JobComInvoiceLine invoiceLine, JobComInvoiceLine invoiceLine2)
		{
			var supportingDocuments = invoiceLine.SupportingDocuments.AddNew();
			supportingDocuments.CSI_Code = "0100";
			supportingDocuments.CSI_Status = "V";
			supportingDocuments.CSI_DateOfIssue = new ZDateTime(2021, 2, 24);
			supportingDocuments.CSI_ReferenceNumber = "544554";

			var supportingDocuments2 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocuments2.CSI_Code = "0200";
			supportingDocuments2.CSI_Status = "Y";
			supportingDocuments2.CSI_DateOfIssue = new ZDateTime(2021, 2, 25);
			supportingDocuments2.CSI_ReferenceNumber = "666666";

			var supportingDocuments3 = invoiceLine.SupportingDocuments.AddNew();
			supportingDocuments3.CSI_Code = "0300";
			supportingDocuments3.CSI_Status = "L";
			supportingDocuments3.CSI_DateOfIssue = new ZDateTime(2021, 2, 26);
			supportingDocuments3.CSI_ReferenceNumber = "777777";

			var supportingDocuments4 = invoiceLine2.SupportingDocuments.AddNew();
			supportingDocuments4.CSI_Code = "0100";
			supportingDocuments4.CSI_Status = "V";
			supportingDocuments4.CSI_DateOfIssue = new ZDateTime(2021, 2, 24);
			supportingDocuments4.CSI_ReferenceNumber = "544554";
		}

		void AddInvoiceLineSupplementaryCodes(JobComInvoiceLine invoiceLine)
		{
			var suppCode1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			suppCode1.CY_Code = "AHSKA";
			var suppCode2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			suppCode2.CY_Code = "BSİZ";
			var suppCode3 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			suppCode3.CY_Code = "AMBLI";
			var suppCode4 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			suppCode4.CY_Code = "BTC";
			var suppCode5 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			suppCode5.CY_Code = "BSGDCULKE";
		}

		void AddInvoiceLineAviationFuelType(JobComInvoiceLine invoiceLine)
		{
			var aviationFuelType = invoiceLine.AviationFuelTypeCollection.AddNew();
			aviationFuelType.CSI_ReferenceNumber2 = "1234567890";
			aviationFuelType.CSI_DateOfIssue = new ZDateTime(2021, 2, 24);
			aviationFuelType.CSI_ReferenceNumber = "5478966";
			aviationFuelType.CSI_Value = 15450m;
			aviationFuelType.CSI_Description = "uçak yakıtı";
		}

		#endregion
	}
}

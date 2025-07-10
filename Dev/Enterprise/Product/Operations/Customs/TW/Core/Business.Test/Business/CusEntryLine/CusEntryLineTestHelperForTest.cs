using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	public static class CusEntryLineTestHelperForTest
	{
		public static CusEntryHeader GetImportRorEntryHeaderTestCase1(BusinessObjectFactory factory)
		{
			GenerateTariffAndRate1(factory);
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(factory, Core.Constants.CurrencyCodes.Japan, 0.2849m, new ZDateTime(2016, 2, 5), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2016, 2, 5);
			entryInstruction.CEI_RORPaymentMethod = "ROR";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 990088m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoice.JZ_IncoTerm = "FOB";

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 16696.76m, Core.Constants.CurrencyCodes.Japan);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "84662000007";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "JP";
			invoiceLine1.JI_Procedure = "38";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "SET";
			invoiceLine1.JI_EnteredUnitPrice = 187665m;
			invoiceLine1.JI_UseOneTenthCV = false;
			invoiceLine1.JI_RAPPrice = 9383.25m;
			invoiceLine1.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "90178090007";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_CountryOfOrigin = "JP";
			invoiceLine2.JI_Procedure = "38";
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_InvoiceUQ = "SET";
			invoiceLine2.JI_EnteredUnitPrice = 63664m;
			invoiceLine2.JI_UseOneTenthCV = false;
			invoiceLine2.JI_RAPPrice = 3183.2m;
			invoiceLine2.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Tariff = "90158090009";
			invoiceLine3.JI_PrimaryPreference = "PR1";
			invoiceLine3.JI_CountryOfOrigin = "JP";
			invoiceLine3.JI_Procedure = "38";
			invoiceLine3.JI_InvoiceQuantity = 1m;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = 87792m;
			invoiceLine3.JI_UseOneTenthCV = false;
			invoiceLine3.JI_RAPPrice = 4389.6m;
			invoiceLine3.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_Tariff = "90158090009";
			invoiceLine4.JI_PrimaryPreference = "PR1";
			invoiceLine4.JI_CountryOfOrigin = "JP";
			invoiceLine4.JI_Procedure = "38";
			invoiceLine4.JI_InvoiceQuantity = 1m;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = 94412m;
			invoiceLine4.JI_UseOneTenthCV = false;
			invoiceLine4.JI_RAPPrice = 4720.6m;
			invoiceLine4.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = entryInstruction.PK;
			invoiceLine5.JI_Tariff = "90158090009";
			invoiceLine5.JI_PrimaryPreference = "PR1";
			invoiceLine5.JI_CountryOfOrigin = "JP";
			invoiceLine5.JI_Procedure = "38";
			invoiceLine5.JI_InvoiceQuantity = 1m;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = 63375m;
			invoiceLine5.JI_UseOneTenthCV = false;
			invoiceLine5.JI_RAPPrice = 3168.75m;
			invoiceLine5.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_CEI = entryInstruction.PK;
			invoiceLine6.JI_Tariff = "73269059004";
			invoiceLine6.JI_PrimaryPreference = "PR1";
			invoiceLine6.JI_CountryOfOrigin = "JP";
			invoiceLine6.JI_Procedure = "38";
			invoiceLine6.JI_InvoiceQuantity = 1m;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = 63375m;
			invoiceLine6.JI_UseOneTenthCV = false;
			invoiceLine6.JI_RAPPrice = 3168.75m;
			invoiceLine6.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "90173000007";
			invoiceLine7.JI_PrimaryPreference = "PR1";
			invoiceLine7.JI_CountryOfOrigin = "JP";
			invoiceLine7.JI_Procedure = "38";
			invoiceLine7.JI_InvoiceQuantity = 1m;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = 19282m;
			invoiceLine7.JI_UseOneTenthCV = false;
			invoiceLine7.JI_RAPPrice = 964.1m;
			invoiceLine7.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine8 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine8.JI_Tariff = "90173000007";
			invoiceLine8.JI_PrimaryPreference = "PR1";
			invoiceLine8.JI_CountryOfOrigin = "JP";
			invoiceLine8.JI_Procedure = "38";
			invoiceLine8.JI_InvoiceQuantity = 1m;
			invoiceLine8.JI_InvoiceUQ = "PCE";
			invoiceLine8.JI_EnteredUnitPrice = 23194m;
			invoiceLine8.JI_UseOneTenthCV = false;
			invoiceLine8.JI_RAPPrice = 1159.7m;
			invoiceLine8.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine9 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine9.JI_Tariff = "73269059004";
			invoiceLine9.JI_PrimaryPreference = "PR1";
			invoiceLine9.JI_CountryOfOrigin = "JP";
			invoiceLine9.JI_Procedure = "38";
			invoiceLine9.JI_InvoiceQuantity = 1m;
			invoiceLine9.JI_InvoiceUQ = "PCE";
			invoiceLine9.JI_EnteredUnitPrice = 254106m;
			invoiceLine9.JI_UseOneTenthCV = false;
			invoiceLine9.JI_RAPPrice = 12705.3m;
			invoiceLine9.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine10 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine10.JI_Tariff = "73269059004";
			invoiceLine10.JI_PrimaryPreference = "PR1";
			invoiceLine10.JI_CountryOfOrigin = "JP";
			invoiceLine10.JI_Procedure = "38";
			invoiceLine10.JI_InvoiceQuantity = 1m;
			invoiceLine10.JI_InvoiceUQ = "PCE";
			invoiceLine10.JI_EnteredUnitPrice = 92738m;
			invoiceLine10.JI_UseOneTenthCV = false;
			invoiceLine10.JI_RAPPrice = 4636.9m;
			invoiceLine10.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine11 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine11.JI_Tariff = "82041200004";
			invoiceLine11.JI_PrimaryPreference = "PR1";
			invoiceLine11.JI_CountryOfOrigin = "JP";
			invoiceLine11.JI_Procedure = "38";
			invoiceLine11.JI_InvoiceQuantity = 1m;
			invoiceLine11.JI_InvoiceUQ = "PCE";
			invoiceLine11.JI_EnteredUnitPrice = 23035m;
			invoiceLine11.JI_UseOneTenthCV = false;
			invoiceLine11.JI_RAPPrice = 1151.75m;
			invoiceLine11.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			var invoiceLine12 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine12.JI_Tariff = "82041200004";
			invoiceLine12.JI_PrimaryPreference = "PR1";
			invoiceLine12.JI_CountryOfOrigin = "JP";
			invoiceLine12.JI_Procedure = "38";
			invoiceLine12.JI_InvoiceQuantity = 1m;
			invoiceLine12.JI_InvoiceUQ = "PCE";
			invoiceLine12.JI_EnteredUnitPrice = 17450m;
			invoiceLine12.JI_UseOneTenthCV = false;
			invoiceLine12.JI_RAPPrice = 872.5m;
			invoiceLine12.JI_RAPCurr = Core.Constants.CurrencyCodes.Japan;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		public static CusEntryHeader GetImportRorEntryHeaderTestCase2(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			CreateTaxOrFee(helper);
			factory.Save();

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(factory, Core.Constants.CurrencyCodes.EuropeanUnion, 34.72m, new ZDateTime(2024, 4, 23), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2024, 4, 23);
			entryInstruction.CEI_RORPaymentMethod = "DEF";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice.JZ_IncoTerm = "EXW";

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 67.62m, Core.Constants.CurrencyCodes.EuropeanUnion);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 317.7m, Core.Constants.CurrencyCodes.EuropeanUnion);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "90132000003";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "DE";
			invoiceLine1.JI_Procedure = "38";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 10000m;
			invoiceLine1.JI_UseOneTenthCV = false;
			invoiceLine1.JI_RAPPrice = 0m;

			var lineMerger = new LineMerger(declaration);
			lineMerger.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		public static CusEntryHeader GetImportRorEntryHeaderTestCase3(BusinessObjectFactory factory)
		{
			GenerateTariffAndRate3(factory);

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(factory, Core.Constants.CurrencyCodes.UnitedStates, 32.48m, new ZDateTime(2016, 6, 14), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2016, 6, 14);
			entryInstruction.CEI_RORPaymentMethod = "ROR";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2461.86m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_IncoTerm = "CIF";

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 10.08m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 11.03m, Core.Constants.CurrencyCodes.UnitedStates);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "70200019001";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "JP";
			invoiceLine1.JI_Procedure = "38";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 2461.86m;
			invoiceLine1.JI_UseOneTenthCV = true;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		public static CusEntryHeader GetImportRorEntryHeaderTestCase4(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			CreateTaxOrFee(helper);
			factory.Save();

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(factory, Core.Constants.CurrencyCodes.Japan, 0.2962m, new ZDateTime(2014, 7, 9), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2014, 7, 9);
			entryInstruction.CEI_RORPaymentMethod = "ROR";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 21000000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
			invoice.JZ_IncoTerm = "CIF";

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 352115m, Core.Constants.CurrencyCodes.Japan);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 94077m, Core.Constants.CurrencyCodes.Japan);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "90308200007";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "JP";
			invoiceLine1.JI_Procedure = "38";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "UNT";
			invoiceLine1.JI_EnteredUnitPrice = 21000000m;
			invoiceLine1.JI_UseOneTenthCV = false;
			invoiceLine1.JI_RAPPrice = 2100000m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		public static CusEntryHeader GetImportRorEntryHeaderTestCase5(BusinessObjectFactory factory)
		{
			GenerateTariffAndRate5(factory);
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(factory, Core.Constants.CurrencyCodes.EuropeanUnion, 35.07m, new ZDateTime(2023, 8, 30), Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 8, 30);
			entryInstruction.CEI_RORPaymentMethod = "ROR";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 35560.93m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice.JZ_IncoTerm = "CFR";

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 602.5m, Core.Constants.CurrencyCodes.EuropeanUnion);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Tariff = "90262092007";
			invoiceLine1.JI_PrimaryPreference = "PR1";
			invoiceLine1.JI_CountryOfOrigin = "DE";
			invoiceLine1.JI_Procedure = "38";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "EAC";
			invoiceLine1.JI_EnteredUnitPrice = 250m;
			invoiceLine1.JI_UseOneTenthCV = false;
			invoiceLine1.JI_RAPPrice = 0m;
			invoiceLine1.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Tariff = "90262092007";
			invoiceLine2.JI_PrimaryPreference = "PR1";
			invoiceLine2.JI_CountryOfOrigin = "US";
			invoiceLine2.JI_Procedure = "38";
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_InvoiceUQ = "EAC";
			invoiceLine2.JI_EnteredUnitPrice = 250m;
			invoiceLine2.JI_UseOneTenthCV = false;
			invoiceLine2.JI_RAPPrice = 0m;
			invoiceLine2.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Tariff = "90262092007";
			invoiceLine3.JI_PrimaryPreference = "PR1";
			invoiceLine3.JI_CountryOfOrigin = "DE";
			invoiceLine3.JI_Procedure = "38";
			invoiceLine3.JI_InvoiceQuantity = 1m;
			invoiceLine3.JI_InvoiceUQ = "EAC";
			invoiceLine3.JI_EnteredUnitPrice = 2121m;
			invoiceLine3.JI_UseOneTenthCV = false;
			invoiceLine3.JI_RAPPrice = 0m;
			invoiceLine3.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction.PK;
			invoiceLine4.JI_Tariff = "90262092007";
			invoiceLine4.JI_PrimaryPreference = "PR1";
			invoiceLine4.JI_CountryOfOrigin = "DE";
			invoiceLine4.JI_Procedure = "38";
			invoiceLine4.JI_InvoiceQuantity = 1m;
			invoiceLine4.JI_InvoiceUQ = "EAC";
			invoiceLine4.JI_EnteredUnitPrice = 2135m;
			invoiceLine4.JI_UseOneTenthCV = false;
			invoiceLine4.JI_RAPPrice = 0m;
			invoiceLine4.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CEI = entryInstruction.PK;
			invoiceLine5.JI_Tariff = "84142000000";
			invoiceLine5.JI_PrimaryPreference = "PR1";
			invoiceLine5.JI_CountryOfOrigin = "DE";
			invoiceLine5.JI_Procedure = "38";
			invoiceLine5.JI_InvoiceQuantity = 1m;
			invoiceLine5.JI_InvoiceUQ = "EAC";
			invoiceLine5.JI_EnteredUnitPrice = 924m;
			invoiceLine5.JI_UseOneTenthCV = false;
			invoiceLine5.JI_RAPPrice = 0m;
			invoiceLine5.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_CEI = entryInstruction.PK;
			invoiceLine6.JI_Tariff = "90261091000";
			invoiceLine6.JI_PrimaryPreference = "PR1";
			invoiceLine6.JI_CountryOfOrigin = "DE";
			invoiceLine6.JI_Procedure = "38";
			invoiceLine6.JI_InvoiceQuantity = 1m;
			invoiceLine6.JI_InvoiceUQ = "EAC";
			invoiceLine6.JI_EnteredUnitPrice = 3114m;
			invoiceLine6.JI_UseOneTenthCV = false;
			invoiceLine6.JI_RAPPrice = 0m;
			invoiceLine6.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "90262092007";
			invoiceLine7.JI_PrimaryPreference = "PR1";
			invoiceLine7.JI_CountryOfOrigin = "DE";
			invoiceLine7.JI_Procedure = "38";
			invoiceLine7.JI_InvoiceQuantity = 1m;
			invoiceLine7.JI_InvoiceUQ = "EAC";
			invoiceLine7.JI_EnteredUnitPrice = 4400m;
			invoiceLine7.JI_UseOneTenthCV = false;
			invoiceLine7.JI_RAPPrice = 0m;
			invoiceLine7.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine8 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine8.JI_Tariff = "90262092007";
			invoiceLine8.JI_PrimaryPreference = "PR1";
			invoiceLine8.JI_CountryOfOrigin = "DE";
			invoiceLine8.JI_Procedure = "38";
			invoiceLine8.JI_InvoiceQuantity = 1m;
			invoiceLine8.JI_InvoiceUQ = "EAC";
			invoiceLine8.JI_EnteredUnitPrice = 579m;
			invoiceLine8.JI_UseOneTenthCV = false;
			invoiceLine8.JI_RAPPrice = 0m;
			invoiceLine8.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine9 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine9.JI_Tariff = "84229000005";
			invoiceLine9.JI_PrimaryPreference = "PR1";
			invoiceLine9.JI_CountryOfOrigin = "DE";
			invoiceLine9.JI_Procedure = "38";
			invoiceLine9.JI_InvoiceQuantity = 1m;
			invoiceLine9.JI_InvoiceUQ = "EAC";
			invoiceLine9.JI_EnteredUnitPrice = 246m;
			invoiceLine9.JI_UseOneTenthCV = false;
			invoiceLine9.JI_RAPPrice = 0m;
			invoiceLine9.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine10 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine10.JI_Tariff = "84229000005";
			invoiceLine10.JI_PrimaryPreference = "PR1";
			invoiceLine10.JI_CountryOfOrigin = "DE";
			invoiceLine10.JI_Procedure = "38";
			invoiceLine10.JI_InvoiceQuantity = 1m;
			invoiceLine10.JI_InvoiceUQ = "EAC";
			invoiceLine10.JI_EnteredUnitPrice = 175m;
			invoiceLine10.JI_UseOneTenthCV = false;
			invoiceLine10.JI_RAPPrice = 0m;
			invoiceLine10.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine11 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine11.JI_Tariff = "84229000005";
			invoiceLine11.JI_PrimaryPreference = "PR1";
			invoiceLine11.JI_CountryOfOrigin = "DE";
			invoiceLine11.JI_Procedure = "38";
			invoiceLine11.JI_InvoiceQuantity = 1m;
			invoiceLine11.JI_InvoiceUQ = "EAC";
			invoiceLine11.JI_EnteredUnitPrice = 274m;
			invoiceLine11.JI_UseOneTenthCV = false;
			invoiceLine11.JI_RAPPrice = 0m;
			invoiceLine11.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine12 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine12.JI_Tariff = "84141090003";
			invoiceLine12.JI_PrimaryPreference = "PR1";
			invoiceLine12.JI_CountryOfOrigin = "DE";
			invoiceLine12.JI_Procedure = "38";
			invoiceLine12.JI_InvoiceQuantity = 1m;
			invoiceLine12.JI_InvoiceUQ = "EAC";
			invoiceLine12.JI_EnteredUnitPrice = 7100m;
			invoiceLine12.JI_UseOneTenthCV = false;
			invoiceLine12.JI_RAPPrice = 0m;
			invoiceLine12.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine13 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine13.JI_Tariff = "85044020003";
			invoiceLine13.JI_PrimaryPreference = "PR1";
			invoiceLine13.JI_CountryOfOrigin = "DK";
			invoiceLine13.JI_Procedure = "38";
			invoiceLine13.JI_InvoiceQuantity = 1m;
			invoiceLine13.JI_InvoiceUQ = "EAC";
			invoiceLine13.JI_EnteredUnitPrice = 192.93m;
			invoiceLine13.JI_UseOneTenthCV = false;
			invoiceLine13.JI_RAPPrice = 0m;
			invoiceLine13.JI_RAPCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine14 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine14.JI_Tariff = "90262091008";
			invoiceLine14.JI_PrimaryPreference = "PR1";
			invoiceLine14.JI_CountryOfOrigin = "DK";
			invoiceLine14.JI_Procedure = "50";
			invoiceLine14.JI_InvoiceQuantity = 1m;
			invoiceLine14.JI_InvoiceUQ = "EAC";
			invoiceLine14.JI_EnteredUnitPrice = 13800m;
			invoiceLine14.JI_TpfPymntMthd = "CAS";
			invoiceLine14.JI_VatPymntMthd = "CAS";

			declaration.ResumeApportionment();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			return entryHeader;
		}

		static void CreateTaxOrFee(UniversalReferenceTestDataHelper helper)
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateTaxOrFee("DDF", 0.0004m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			var tpfFee = helper.CreateTaxOrFee("TPF", 0.0004m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			tpfFee.ZZF_Threshold = 101m;
			helper.CreateTaxOrFee("VAT", 0.05m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
		}

		static void GenerateTariffAndRate1(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			CreateTaxOrFee(helper);
			factory.Save();

			var tradeGroupWTO = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "WTO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddNewOrExistingCountry(tradeGroupWTO, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddNewOrExistingCountry(tradeGroupWTO, Core.Constants.CountryCodes.Japan, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			factory.Save();

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			factory.Save();
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(factory, "DTA", dutyRateType.PK);
			factory.Save();
			var preferencePR1 = helper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", Core.Constants.CountryCodes.Taiwan);
			factory.Save();

			var cusTariff84662000007 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "84662000007", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 1");
			var cusTariff90178090007 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "90178090007", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 2");
			var cusTariff90158090009 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "90158090009", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 3");
			var cusTariff73269059004 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "73269059004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 4");
			var cusTariff82041200004 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "82041200004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 5");
			factory.Save();

			var testRate1 = helper.CreateRate(cusTariff84662000007, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.025*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.025", dataGrouping: "TW");
			var testRate2 = helper.CreateRate(cusTariff90178090007, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.05*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.05", dataGrouping: "TW");
			var testRate3 = helper.CreateRate(cusTariff90158090009, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.034*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.034", dataGrouping: "TW");
			var testRate4 = helper.CreateRate(cusTariff73269059004, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.062*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.062", dataGrouping: "TW");
			var testRate5 = helper.CreateRate(cusTariff82041200004, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.1", dataGrouping: "TW");
			helper.CreateCusApplicability(testRate1, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");
			helper.CreateCusApplicability(testRate2, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");
			helper.CreateCusApplicability(testRate3, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");
			helper.CreateCusApplicability(testRate4, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");
			helper.CreateCusApplicability(testRate5, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");

			factory.Save();
		}

		static void GenerateTariffAndRate3(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			CreateTaxOrFee(helper);
			factory.Save();

			var tradeGroupWTO = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "WTO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddNewOrExistingCountry(tradeGroupWTO, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddNewOrExistingCountry(tradeGroupWTO, Core.Constants.CountryCodes.Japan, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			factory.Save();

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			factory.Save();
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(factory, "DTA", dutyRateType.PK);
			factory.Save();
			var preferencePR1 = helper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", Core.Constants.CountryCodes.Taiwan);
			factory.Save();

			var cusTariff70200019001 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "70200019001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 1");
			factory.Save();

			var testRate = helper.CreateRate(cusTariff70200019001, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.1*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.258", dataGrouping: "TW");
			helper.CreateCusApplicability(testRate, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");

			factory.Save();
		}

		static void GenerateTariffAndRate5(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			CreateTaxOrFee(helper);
			factory.Save();

			var tradeGroupWTO = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Taiwan, "WTO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.AddNewOrExistingCountry(tradeGroupWTO, Core.Constants.CountryCodes.Germany, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			factory.Save();

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			factory.Save();
			var dutyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(factory, "DTA", dutyRateType.PK);
			factory.Save();
			var preferencePR1 = helper.CreatePreferenceForCountry("PR1", "Column I Rates(WTO Member/Other Countries with reciprocal relationship)", Core.Constants.CountryCodes.Taiwan);
			factory.Save();

			var cusTariff84142000000 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "84142000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 1");
			var cusTariff84229000005 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "84229000005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 2");
			var cusTariff84141090003 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "84141090003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "test tariff 3");
			factory.Save();

			var testRate1 = helper.CreateRate(cusTariff84142000000, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.04*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.04", dataGrouping: "TW");
			var testRate2 = helper.CreateRate(cusTariff84229000005, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.025*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.025", dataGrouping: "TW");
			var testRate3 = helper.CreateRate(cusTariff84141090003, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.03*VFD", preferencePk: preferencePR1.PK, rateFormulaDeriveFrom: "0.03", dataGrouping: "TW");
			helper.CreateCusApplicability(testRate1, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");
			helper.CreateCusApplicability(testRate2, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");
			helper.CreateCusApplicability(testRate3, tradeGroupWTO, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, orderNumber: "QUOTA");

			factory.Save();
		}
	}
}

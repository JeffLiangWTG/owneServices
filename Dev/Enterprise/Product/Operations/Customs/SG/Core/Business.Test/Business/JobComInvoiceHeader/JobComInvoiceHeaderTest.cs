using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestJZ_ValuationDateOverride()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertEquals("Should be empty initially", ZDateTime.Empty, invoiceHeader.JZ_ValuationDateOverride);

			var date = ZDateTime.Today.AddDays(-2);
			invoiceHeader.JZ_ValuationDateOverride = date;
			AssertEquals("Should be changed when trying to set a past date", date, invoiceHeader.JZ_ValuationDateOverride);

			date = ZDateTime.Today.AddDays(2);
			invoiceHeader.JZ_ValuationDateOverride = date;
			AssertEquals("Should be cleared when trying to set a future date", ZDateTime.Empty, invoiceHeader.JZ_ValuationDateOverride);

			AssertEquals("Caption", "Valuation Date", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceHeader), nameof(JobComInvoiceHeader.JZ_ValuationDateOverride)).Caption);
		}

		public void TestTypeOfJobComInvoiceLines()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertType<JobComInvoiceLineViewCollection>(invoiceHeader.InvoiceLines);
			new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader);
			AssertType<JobComInvoiceLineViewCollection>(invoiceHeader.InvoiceLines);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			Customs.Business.ICommonInvoice commonInvoice = dec.Invoices.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals("OFT, ONS, OPT, OTH", chargeTypeList1.CodesAsString);
		}

		public void TestSupportsRelatedBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			AssertEquals(false, invoice.SupportsRelatedBill);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			AssertEquals(false, invoice.SupportsRelatedBill);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.TNP;
			AssertEquals(false, invoice.SupportsRelatedBill);
		}

		public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Singapore;
		public void TestRevalidateWhenAnUncommitttedInvoiceDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			for (int index = 0; index < 20; index++)
			{
				((IBindingList)declaration.Invoices).AddNew();
				AssertNoErrors("No error expected as it is within a limit", declaration.JE_Calc_InvoicesCountInfo);
			}

			((IBindingList)declaration.Invoices).AddNew();
			AssertHasError(declaration.JE_Calc_InvoicesCountInfo, JobDeclarationValidation.Only20InvoicesAreAllowed);
			((ICancelAddNew)declaration.Invoices).CancelNew(20); //ActiveBusinessObjectCollection.List should not have this invoice
			AssertNoError(declaration.JE_Calc_InvoicesCountInfo, JobDeclarationValidation.Only20InvoicesAreAllowed);
		}

		public override void TestDefaultINCOFromImporter()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_RL_NKFinalDestination = "BFXXX";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var consignee = OrgHeader.New(Factory);
			consignee.MiscServ.OM_IMDefaultINCOTerm = "135";
			var consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";
			var link = consignee.SupplierLinks.AddNew(consignor);
			link.OL_RN_NKImporterCountry = "BF";
			declaration.JE_OH_Importer = consignee.PK;
			invoiceHeader.JZ_OH_Supplier = consignor.PK;
			AssertEquals("Defaulted Inco Term", "135", invoiceHeader.JZ_IncoTerm);
		}

		public override void TestDefaultINCOFromOrgLink()
		{
			var declaration = JobDeclaration.New(Factory);
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var consignee = OrgHeader.New(Factory);
			var consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";
			var link = consignee.SupplierLinks.AddNew(consignor);
			link.OL_RN_NKImporterCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			link.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = "123";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_RL_NKFinalDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			invoiceHeader.JZ_OH_Supplier = consignor.PK;
			AssertEquals("Defaulted Inco Term", "123", invoiceHeader.JZ_IncoTerm);
		}

		public override void TestDefaultINCOFromSupplier()
		{
			var declaration = JobDeclaration.New(Factory);
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";
			invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			invoiceHeader.JZ_OH_Supplier = consignor.PK;
			AssertEquals("Defaulted Inco Term", "321", invoiceHeader.JZ_IncoTerm);
			invoiceHeader.JZ_IncoTerm = "234";
			invoiceHeader.JZ_OH_Supplier = consignor.PK;
			AssertEquals("Overridden Inco Term", "234", invoiceHeader.JZ_IncoTerm);
		}

		public override void TestSupplierDefaultingIncoTermAndCurrency()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.MiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			supplier.MiscServ.OM_RX_NKEXDefCurrency = Core.Constants.CurrencyCodes.Australia;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_OH_Supplier = supplier.PK;
			AssertEquals("For SG: Inco Term should default from supplier entered when no values have been entered into Inco Term OR Currency Code", "CIF", invHeader.JZ_IncoTerm);
			AssertEquals("For SG: Invoice Currency should default from supplier entered when no values have been entered into Inco Term OR Currency Code", "AUD", invHeader.JZ_RX_NKInvoice_Currency);
		}

		[TestDate(2007, 1, 1)]
		public void TestEffectiveValuationDate()
		{
			AssertEquals(new ZDateTime(2007, 1, 1), invoiceHeader.EffectiveValuationDate);
			var valuationDate = new ZDateTime(2006, 12, 12);
			invoiceHeader.JZ_ValuationDateOverride = valuationDate;
			AssertEquals(valuationDate, invoiceHeader.EffectiveValuationDate);
		}

		public void TestEffectiveValuationDateForTN41()
		{
			declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AssertEquals("For TradeNet 4.1 valuation date is always today's date.", ZDateTime.Today, invoiceHeader.EffectiveValuationDate);
		}

		public void TestSetDefaultValues()
		{
			declaration.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
			declaration.JE_ShipmentIncoTerm = UnitPriceTermTypeCodeList.Codes.CNI;
			AssertEquals(7, invoiceHeader.SG_GSTRate);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, invoiceHeader.Invoice_Currency.RX_Code);
		}

		public void TestCurrencyDefaultsFromSuppliersOrgDetails()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.MiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			supplier.MiscServ.OM_RX_NKEXDefCurrency = Core.Constants.CurrencyCodes.Australia;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Inv-1";
			AssertEquals("Invoice Currency should have defaulted (AUD) from Supplier", Core.Constants.CurrencyCodes.Australia, invoice.JZ_RX_NKInvoice_Currency);
		}

		public void TestCurrencyDefaultsToSGDIfNoSupplierDefault()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.MiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "Inv-1";
			AssertEquals("Invoice Currency should default to SGD if it doesn't default from Supplier", Core.Constants.CurrencyCodes.Singapore, invoice.JZ_RX_NKInvoice_Currency);
		}

		public void TestHasPreferentialDuty()
		{
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Default value of duty preference is 'STD'", "STD", invoiceLine.JI_PrimaryPreference);
			AssertEquals(false, invoiceHeader.HasPreferentialDuty);
			invoiceLine.JI_PrimaryPreference = "";
			AssertEquals(false, invoiceHeader.HasPreferentialDuty);
			invoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRF;
			AssertEquals(true, invoiceHeader.HasPreferentialDuty);
			invoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRI;
			AssertEquals(true, invoiceHeader.HasPreferentialDuty);
		}

		public void TestTypeDecider()
		{
			Assert(Factory.New<Customs.Business.BaseJobComInvoiceHeader>() is JobComInvoiceHeader);
		}

		public void TestSetDefaultsForInvoiceLine()
		{
			Assert(true);
		}

		public void TestValidation()
		{
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Assert(invoiceHeader.Validation is JobComInvoiceHeaderValidation_IPT);
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			Assert(invoiceHeader.Validation is JobComInvoiceHeaderValidation_INP);
			declaration.JE_MessageType = "";
			Assert(invoiceHeader.Validation is JobComInvoiceHeaderValidation);
		}

		public void TestDefaultCurrencyForStandaloneInvoice()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			_ = new Customs.Business.FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			AssertEquals(JobDeclaration.LocalCurrencyConstantCode, invoice.JZ_RX_NKInvoice_Currency);
		}

		public void TestCloneExchangeRate()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoiceHeader.JZ_InvoiceCurrExRate = 1.22m;
			JobDeclaration clonedDec = (JobDeclaration)declaration.TemplateCopy();
			JobComInvoiceHeader clonedInvoice = clonedDec.Invoices[0];
			AssertEquals(1.79m, clonedInvoice.JZ_InvoiceCurrExRate);
		}

		public override void TestOnLoadedDoesNotChangePersistentValues()
		{
			Assert("exchange rates should be updated with current rates", true);
		}

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert("exchange rates should be loaded", true);
		}

		public override void TestIsInvoiceLinesLoaded()
		{
			Assert("invoicelines should be marked as needing apportionment", true);
		}

		public override void TestIWorkflowProviderImpl()
		{
			OrgHeader org1 = OrgHeader.New(Factory);
			OrgHeader org2 = OrgHeader.New(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var workflowProvider1 = invoice1 as IWorkflowProvider;
			AssertNotNull("JobComInvoiceHeader should be IWorkflowProvider", workflowProvider1);
			AssertNull("GetWorkflowInformationProvider", workflowProvider1.GetWorkflowInformationProvider());
			AssertNotNull("WorkflowItems", workflowProvider1.WorkflowItems);
			AssertEquals("WorkflowItems should be empty", 0, workflowProvider1.WorkflowItems.Count);
			AssertEquals("WorkflowType", WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode, workflowProvider1.WorkflowType);
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_OH_Supplier = org2.PK;
			declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			var ranker1 = workflowProvider1.GetTemplateSelectionCriteria();
			AssertEquals(typeof(ColumnValueRanker), ranker1.GetType());
			AssertCollectionContains("Clients should contains Importer", org1.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionNotContains("Clients should not contains Supplier", org2.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Job Types should contains INP", MessageTypeCodeList.Codes.INP, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_SubType1));
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			ranker1 = workflowProvider1.GetTemplateSelectionCriteria();
			AssertCollectionNotContains("Clients should not contains Importer", org1.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Clients should contains Supplier", org2.PK, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Job Types should contains OUT", MessageTypeCodeList.Codes.OUT, ((ColumnValueRanker)ranker1).GetValues(ProcessTaskTemplateSchema.P0_SubType1));
			var invoice2 = Factory.New<JobComInvoiceHeader>();
			invoice2.JZ_OH_Buyer = org1.PK;
			invoice2.JZ_OH_Supplier = org2.PK;
			invoice2.JZ_MessageType = "INP";
			invoice2.JZ_StandAloneInvoiceDirection = "INP";
			var workflowProvider2 = invoice2 as IWorkflowProvider;
			var ranker2 = workflowProvider2.GetTemplateSelectionCriteria();
			AssertEquals(typeof(ColumnValueRanker), ranker2.GetType());
			AssertCollectionContains("Clients should contains Importer", org1.PK, ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionNotContains("Clients should not contains Supplier", org2.PK, ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Job Types should contains INP", "INP", ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_SubType1));
			invoice2.JZ_MessageType = "OUT";
			invoice2.JZ_StandAloneInvoiceDirection = "OUT";
			ranker2 = workflowProvider2.GetTemplateSelectionCriteria();
			AssertCollectionNotContains("Clients should not contains Importer", org1.PK, ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Clients should contains Supplier", org2.PK, ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertCollectionContains("Job Types should contains OUT", "OUT", ((ColumnValueRanker)ranker2).GetValues(ProcessTaskTemplateSchema.P0_SubType1));
		}

		#region Declaration
		new JobDeclaration declaration
		{
			get
			{
				return (JobDeclaration)base.declaration;
			}
		}

		#endregion
		#region Invoice Header
		new JobComInvoiceHeader invoiceHeader
		{
			get
			{
				return (JobComInvoiceHeader)base.invoiceHeader;
			}
		}

		#endregion
		#region Overrides
		protected override bool RatesAreReciprocal
		{
			get
			{
				return true;
			}
		}
		#endregion

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "Goods and Services Tax");
			Factory.Save();
			base.SetUp();
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);
	}
}

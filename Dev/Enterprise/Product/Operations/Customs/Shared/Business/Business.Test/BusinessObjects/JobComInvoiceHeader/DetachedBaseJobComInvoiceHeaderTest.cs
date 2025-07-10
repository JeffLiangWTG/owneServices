using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DetachedBaseJobComInvoiceHeaderTest : TestCaseWithFactory
	{
		public void TestImportExport()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			Assert(invoice.IsImport);
			Assert(!invoice.IsExport);

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			Assert(!invoice.IsImport);
			Assert(invoice.IsExport);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.JZ_JE = declaration.PK;
			Assert(invoice.IsImport);
			Assert(!invoice.IsExport);
		}

		public void TestLandedCostingExRateFallBackToJobExRate()
		{
			Env.Registry.LandedCostingFallbackExRatesToJobInvoicing = true;
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			invoice.JZ_InvoiceCurrLandedCostExRate = 0.3m;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			BusinessObject exRate = ((IBusinessObjectCollection)job["ExchangeRates"]).AddNew();
			RefCurrency invoiceCurrency = RefCurrency.LoadFromCurrencyCode(Factory, invoice.JZ_RX_NKInvoice_Currency);
			exRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = invoiceCurrency.RX_Code;
			exRate[JobExRateSchema.Constants.JF_BaseRate] = 0.5m;

			AssertEquals(0.3m, invoice.LandedCostingExRateFallBackToJobExRate);

			invoice.JZ_InvoiceCurrLandedCostExRate = 0m;
			AssertEquals(0.5m, invoice.LandedCostingExRateFallBackToJobExRate);

			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals(1m, invoice.LandedCostingExRateFallBackToJobExRate);

			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			invoice.JZ_InvoiceCurrLandedCostExRate = 0.3m;
			declaration.JE_GB = ZGuid.Empty;
			AssertEquals(0.3m, invoice.LandedCostingExRateFallBackToJobExRate);
		}

		/// <summary>
		/// CS00040695
		/// </summary>
		public void TestLineNumbersAreCorrectlyCalculatedForStandAloneInvoices()
		{
			BaseJobComInvoiceHeader standAloneInvoice = Factory.New<BaseJobComInvoiceHeader>();
			BaseJobDeclaration fakeDeclaration = (BaseJobDeclaration)new FakeDeclarationCreatorForInvoice(standAloneInvoice).HeaderData;

			BaseJobComInvoiceLine invoiceLine1 = standAloneInvoice.JobComInvoiceLines.AddNew();
			AssertEquals("Line Number correctly calculated", (short)1, invoiceLine1.JI_LineNo);

			BaseJobComInvoiceLine invoiceLine2 = standAloneInvoice.JobComInvoiceLines.AddNew();
			AssertEquals("Line Number correctly calculated", (short)2, invoiceLine2.JI_LineNo);

			Assert("Should Contain invoice line1", fakeDeclaration.InvoiceLines.Contains(invoiceLine1));
			Assert("Should Contain invoice line2", fakeDeclaration.InvoiceLines.Contains(invoiceLine2));
			Assert("Should Contain invoice line1", fakeDeclaration.FilteredInvoiceLines.Contains(invoiceLine1));
			Assert("Should Contain invoice line2", fakeDeclaration.FilteredInvoiceLines.Contains(invoiceLine2));
		}

		public void TestLinkingToADeclarationClearsJZ_MessageTypeNotificaiton()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = "Z@1";
			AssertHasErrors(invoice.JZ_MessageTypeInfo);
			invoice.JZ_JE = declaration.PK;
			AssertNoErrors(invoice.JZ_MessageTypeInfo);
		}

		public void TestSerialiseAndDeserialiseMessageTypeToStandaloneMessageType()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BaseJobComInvoiceHeader invoiceToBeSaved = factory1.New<BaseJobComInvoiceHeader>();
			invoiceToBeSaved.JZ_MessageType = "ABC";
			AssertEquals("invoiceToBeSaved.JZ_MessageType", "ABC", invoiceToBeSaved.JZ_MessageType);
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobComInvoiceHeader invoiceLoaded = factory2.Load<BaseJobComInvoiceHeader>(invoiceToBeSaved.PK);
			AssertEquals("invoiceLoaded.JZ_MessageType", "ABC", invoiceLoaded.JZ_MessageType);
		}

		[ExpectNoExceptions()]
		public void TestSettingJZ_OH_SupplierDoesNotGenerateException()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
		}

		public void TestGetProductCreationHelper()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertType(typeof(DeclarationForProductCreationHelper), invoice.GetProductCreationHelper());
		}

		public void TestEffectiveDateForDutyRate()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals(declaration.DateForDutyRate, invoice.EffectiveDateForDutyRate);
			invoice.JZ_JE = ZGuid.Invalid;
			AssertEquals(ZDate.Today, invoice.EffectiveDateForDutyRate);
		}

		[ExpectNoExceptions()]
		public void TestCallingEffectiveValuationDateDoesNotGenerateException()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_ValuationDateOverride = ZDateTime.Empty;
			ZDateTime chkDate = invoice.EffectiveValuationDate;
		}

		[ExpectNoExceptions()]
		public void TestSettingJZ_IncoTermDoesNotGenerateException()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.NeedToGetNewIncoTermAndChargeFactory = true;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		}

		public void TestIncludedInLinesDiscountDontAffectBalance()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			BaseJobComInvHeaderCharge discount = invoice.Charges.AddNew();
			discount.J7_ChargeType = "DIS";
			discount.J7_Amount = 200m;
			discount.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			AssertEquals("Balance with excluded-from-lines discount", 10200m, invoice.InvoiceLineTotal);

			discount.J7_IsIncludedInITOT = true;
			AssertEquals("Balance with included-in-lines discount", 10000m, invoice.InvoiceLineTotal);
		}

		public void TestUpdatesImporterOnNonPersistentDeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.JZ_OH_Buyer = ZGuid.NewZGuid();
			Assert("Doesn't update persistent declaration", invoiceHeader.JZ_OH_Buyer != declaration.JE_OH_Importer);

			declaration.MakeNonPersistent();
			invoiceHeader.JZ_OH_Buyer = ZGuid.NewZGuid();
			Assert("Updates non-persistent declaration", invoiceHeader.JZ_OH_Buyer == declaration.JE_OH_Importer);
		}

		public void TestUpdatesSupplierOnNonPersistentDeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);

			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.JZ_OH_Supplier = ZGuid.NewZGuid();
			Assert("Doesn't update persistent declaration", invoiceHeader.JZ_OH_Supplier != declaration.JE_OH_Supplier);

			declaration.MakeNonPersistent();
			invoiceHeader.JZ_OH_Supplier = ZGuid.NewZGuid();
			Assert("Updates non-persistent declaration", invoiceHeader.JZ_OH_Supplier == declaration.JE_OH_Supplier);
		}

		public void TestIsOnPersistentDeclaration()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var faker = new FakeDeclarationCreatorForInvoice(invoice);
			Assert("Fake declaration", !invoice.IsAttachedToPersistentDeclaration);
			AssertNull("PersistentDeclaration", invoice.PersistentDeclaration);
			AssertNull("GetPersistentDeclaration<BaseJobDeclaration>()", invoice.GetPersistentDeclaration<BaseJobDeclaration>((x) => true));

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			Assert("Real declaration", invoice.IsAttachedToPersistentDeclaration);
			AssertEquals("PersistentDeclaration", declaration, invoice.PersistentDeclaration);
			AssertEquals("GetPersistentDeclaration<BaseJobDeclaration>((x) => true)", declaration, invoice.GetPersistentDeclaration<BaseJobDeclaration>((x) => true));
			AssertNull("GetPersistentDeclaration<BaseJobDeclaration>((x) => x.IsExport)", invoice.GetPersistentDeclaration<BaseJobDeclaration>((x) => x.IsExport));
			AssertEquals("GetPersistentDeclaration<BaseJobDeclaration>((x) => x.IsImport)", declaration, invoice.GetPersistentDeclaration<BaseJobDeclaration>((x) => x.IsImport));
		}

		public void TestAutoLoggingIsEnabled()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.FillWithValidTestData();
			Factory.Save();
			AssertEquals("An ADD log should exist for the JobComInvoiceHeader.", 1, invoiceHeader.Logs.DatabaseCount);
		}

		public void TestMessageTypeSerialisationDoesntOccurOnPersistentDeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;

			ZString value = invoice.JZ_MessageType;
			Factory.Save();
			AssertEquals("Invoice.JZ_StandAloneInvoiceDirection", "", invoice.JZ_StandAloneInvoiceDirection);
		}

		public void TestJZ_GBIsClearedOnSavingOfAPersistentDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_GB = ZGuid.NewZGuid();
			Factory.Save();

			AssertEquals("invoice.JZ_GB", ZGuid.Empty, invoice.JZ_GB);
		}

		public void TestJZ_GBIsNotClearedOnSavingOfANonPersistentDeclaration()
		{
			BaseJobComInvoiceHeader standaloneInvoice = Factory.New<BaseJobComInvoiceHeader>();
			standaloneInvoice.JZ_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			AssertNotEquals("standaloneInvoice.JZ_GB", ZGuid.Empty, standaloneInvoice.JZ_GB);
		}

		public void TestJZ_MessageTypeGetAndSet()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			FakeDeclarationCreatorForInvoice faker = new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_MessageType = "ABC";
			AssertEquals("Get and set", "ABC", invoice.JZ_MessageType);
		}

		public void TestJZ_MessageTypeSetsHasChanges()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			Assert("Initially, has changes = false", !invoice.HasChanges);
			invoice.JZ_MessageType = "AAA";
			AssertEquals("HasChanges", true, invoice.HasChanges);
		}

		public void TestJZ_MessageTypeProxiesThroughFromDeclarationWhenItIsPersistent()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals("Proxying through", declaration.JE_MessageType, invoice.JZ_MessageType);
			declaration.JE_MessageType = "NNN";
			AssertEquals("Proxying through", declaration.JE_MessageType, invoice.JZ_MessageType);
		}

		public void TestDoNotSetJZ_MessageTypeWhenOnPersistentDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();

			ErrorReporter.Clear();

			invoice.JZ_JE = declaration.PK;
			invoice.JZ_MessageType = "AAA";
			Assert("Got error report", !string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			ErrorReporter.Clear();

			FakeDeclarationCreatorForInvoice faker = new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_MessageType = "ZZZ";
			Assert("No error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			ErrorReporter.Clear();
		}

		public void TestIsImport()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();

			invoiceHeader.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Assert("Import", invoiceHeader.IsImport);

			invoiceHeader.JZ_MessageType = "ZZZ";
			Assert("Not import", !invoiceHeader.IsImport);

			invoiceHeader.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Assert("Not import", !invoiceHeader.IsImport);
		}

		public void TestIsExport()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();

			invoiceHeader.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Assert("Export", invoiceHeader.IsExport);

			invoiceHeader.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Assert("Not Export", !invoiceHeader.IsExport);
		}

		public void TestIDocAddresses_DocAddresses()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var inv = dec.Invoices.AddNew() as IDocAddresses;
			AssertNotNull(inv.DocAddresses);
		}

		public void TestOrderNumberAddedWhenAttachedToDeclaration()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceAmount = 100m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeader.LocalCurrencyCode;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_OrderNumber = "AABBCC1";

			var invoiceLin2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLin2.JI_LinePrice = 200m;
			invoiceLin2.JI_OrderNumber = "DDEECC2";

			var invoiceLin3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLin3.JI_LinePrice = 200m;
			invoiceLin3.JI_OrderNumber = "";

			var declaration = Factory.New<BaseJobDeclaration>();
			invoiceHeader.JZ_JE = declaration.PK;

			AssertEquals("AABBCC1,DDEECC2", declaration.DocsAndCartage.JP_OrderItemsAsString);
		}

		public void TestMarkApportionmentDirtyWhenAttachedToDeclaration()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
				invoiceHeader.JZ_InvoiceAmount = 100m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = invoiceHeader.LocalCurrencyCode;
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 100m;

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				var charge = declaration.TopGroupInvoice.Charges.AddNew();
				charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				charge.J7_Amount = 50m;
				charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

				invoiceHeader.JZ_JE = declaration.PK;

				Assert("This should be marked as Dirty to resume apportionment when merge or Save happens", declaration.ApportionmentDirty);

				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

				AssertEquals(50m, invoiceLine.JI_OverseasFreight.Amount);
			}
		}

		public void TestNoExceptionWhenCalling_UpdateWhenAnInvoiceIsLinkedToADeclaration_And_TopGroupInvoiceIsNull()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			declaration.JobComInvoiceGroupHeaders.RemoveAll();
			AssertNoExceptionThrown(() => groupHeader.JobComInvoiceHeaders.AddNew());
		}
	}
}

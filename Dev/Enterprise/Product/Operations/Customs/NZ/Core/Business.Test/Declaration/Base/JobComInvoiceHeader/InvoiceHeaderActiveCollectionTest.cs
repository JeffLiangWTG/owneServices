using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System.ComponentModel;
	using Enterprise.Customs.NZ.Registry;
	using NUnit.Framework;

	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	public class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
		public void TestExportJob_NewInvoice_CurrencyIndicator_Default_To_FirstOfSameCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var indicatorOfFirstInvoice = ExchangeRateIndicatorList.Codes.Floating;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice1.JZ_ExchangeRateIndicator = indicatorOfFirstInvoice;

			for (var i = 0; i < 3; i++)
			{
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("Export job currency indicator is set to the same", indicatorOfFirstInvoice, invoice.JZ_ExchangeRateIndicator);
			}
		}

		public void TestNonExportJob_NewInvoice_CurrencyIndicator_Default_To_Empty()
		{
			var nonExportTypes = new[]
			{
				JobMessageTypeList.Codes.Import,
				JobMessageTypeList.Codes.Excise,
				JobMessageTypeList.Codes.MiscellaneousCustoms
			};

			foreach (var nonExportType in nonExportTypes)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = nonExportType;

				var indicatorOfFirstInvoice = ExchangeRateIndicatorList.Codes.Floating;
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoice1.JZ_ExchangeRateIndicator = indicatorOfFirstInvoice;

				for (var i = 0; i < 3; i++)
				{
					var invoice = declaration.Invoices.AddNew();
					Assert(
						"Non-export job currency indicator for a foreign currency is set to empty",
						invoice.JZ_ExchangeRateIndicator.IsEmpty);
				}
			}
		}

		public void TestInvoiceHeaderAddedToCollectionDefaultsMiscDetailsFromParentDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ZGuid miscOrgPK = declaration.CachedMiscOrgPK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			declaration.JE_OH_Supplier = miscOrgPK;
			declaration.MiscSupplierName = "MISC Supplier";
			declaration.JE_OH_Importer = miscOrgPK;
			declaration.MiscImporterName = "MISC Importer";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals(miscOrgPK, invoice.JZ_OH_Supplier);
			AssertEquals("MISC SUPPLIER", invoice.MiscSupplierName);
			AssertEquals(miscOrgPK, invoice.JZ_OH_Buyer);
		}

		public void TestRefreshBindingGetsCalledWhenCollectionCountChangesOnECIWriteOff()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			IBindingList declarationAsIBindingList = declaration;
			declarationAsIBindingList.ListChanged += new ListChangedEventHandler(DeclarationAsIBindingList_ListChanged);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declarationRefreshBindingCalled = false;
			declaration.Invoices.AddNew();
			AssertEquals("Declaration.RefreshBinding has been called", false, declarationRefreshBindingCalled);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declarationRefreshBindingCalled = false;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AssertEquals("Declaration.RefreshBinding has been called", true, declarationRefreshBindingCalled);

			declarationRefreshBindingCalled = false;
			invoice.Delete();
			AssertEquals("Declaration.RefreshBinding has been called", true, declarationRefreshBindingCalled);
		}

		public override void TestTotalInvoiceAmount()
		{
			RefCurrency currencyNZD = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			RefCurrency currencyAUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			RefCurrency currencyUSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			JobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceAmount = 100.00m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = currencyAUD.RX_Code;
			AssertEquals("InvoiceHeaders.TotalInvoiceAmount", Money.Empty.ToString(), declaration.Invoices.TotalInvoiceLinesAmount.ToString());
			invoiceHeader1.JobComInvoiceLines.AddNew().JI_LinePrice = 100.00m;
			AssertEquals("InvoiceHeaders.TotalInvoiceAmount", new Money(100.00m, currencyAUD), declaration.Invoices.TotalInvoiceLinesAmount);

			JobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 200.00m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = currencyAUD.RX_Code;
			invoiceHeader2.JobComInvoiceLines.AddNew().JI_LinePrice = 100.00m;
			invoiceHeader2.JobComInvoiceLines.AddNew().JI_LinePrice = 100.00m;
			AssertEquals("InvoiceHeaders.TotalInvoiceAmount", new Money(300.00m, currencyAUD), declaration.Invoices.TotalInvoiceLinesAmount);

			JobComInvoiceHeader invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_InvoiceAmount = 50.00m;
			invoiceHeader3.JZ_RX_NKInvoice_Currency = currencyUSD.RX_Code;
			invoiceHeader3.JobComInvoiceLines.AddNew().JI_LinePrice = 50.00m;
			AssertEquals("InvoiceHeaders.TotalInvoiceAmount", new Money(411.46m, currencyNZD).ToString(), declaration.Invoices.TotalInvoiceLinesAmount.ToString());
		}

		public void TestDefaultValues()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals("Buyer is Defaulted on new InvoiceHeader", declaration.JE_OH_Importer, invoiceHeader.JZ_OH_Buyer);
		}

		public void TestDeleteInvoiceAfterDeclarationIsDeleted()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			IBindingList declarationAsIBindingList = Declaration;
			declarationAsIBindingList.ListChanged += new ListChangedEventHandler(DeclarationAsIBindingList_ListChanged);
			declarationRefreshBindingCalled = false;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			declaration.Delete();
			AssertEquals("Declaration.IsDeleted", true, declaration.IsDeleted);
			AssertNoExceptionThrown(() => invoice.Delete());
		}

		#region Implementation
		bool declarationRefreshBindingCalled;
		void DeclarationAsIBindingList_ListChanged(object sender, ListChangedEventArgs e)
		{
			declarationRefreshBindingCalled = true;
		}

		protected override BaseJobDeclaration GetNewJobDeclaration()
		{
			return JobDeclaration.New(Factory);
		}

		#endregion
	}

	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	public class GroupInvoiceDirectChildInvoiceHeaderCollectionTest : Customs.Business.Testing.BaseGroupInvoiceDirectChildInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection>
	{
	}

	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	public class GroupInvoiceAllInvoiceHeaderCollectionTestTest : Customs.Business.Testing.BaseGroupInvoiceAllInvoiceHeaderCollectionTest
	{
	}
}

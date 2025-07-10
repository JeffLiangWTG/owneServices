using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseInvoiceHeaderCollectionTest<TCollection, TJobComInvoiceHeader> : ActiveBusinessObjectCollectionTestCase<TCollection>
		where TCollection : InvoiceHeaderActiveCollection
		where TJobComInvoiceHeader : BaseJobComInvoiceHeader
	{
		public void TestCreateRelationshipFilter()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice2 = declaration2.Invoices.AddNew();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobDeclaration declarationLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);
			AssertEquals(true, declarationLoaded.Invoices.Contains(factory2.Load<BaseJobComInvoiceHeader>(invoice.PK)));
			AssertEquals(false, declarationLoaded.Invoices.Contains(factory2.Load<BaseJobComInvoiceHeader>(invoice2.PK)));
		}

		public virtual void TestTotalInvoiceAmount()
		{
			BaseJobDeclaration declaration = GetNewJobDeclaration();
			BaseJobComInvoiceHeader invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceAmount = 100.00m;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals("InvoiceHeaders.TotalInvoiceAmount", Money.Empty.ToString(), declaration.Invoices.TotalInvoiceLinesAmount.ToString());
			invoiceHeader1.JobComInvoiceLines.AddNew().JI_LinePrice = 100.00m;
			AssertEquals("InvoiceHeaders.TotalInvoiceAmount", new Money(100.00m, invoiceHeader1.LocalCurrency).ToString(), declaration.Invoices.TotalInvoiceLinesAmount.ToString());

			BaseJobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 200.00m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader2.JobComInvoiceLines.AddNew().JI_LinePrice = 100.00m;
			invoiceHeader2.JobComInvoiceLines.AddNew().JI_LinePrice = 100.00m;
			AssertEquals("InvoiceHeaders.TotalInvoiceAmount", new Money(300.00m, invoiceHeader2.LocalCurrency).ToString(), declaration.Invoices.TotalInvoiceLinesAmount.ToString());

			BaseJobComInvoiceHeader invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_InvoiceAmount = 50.00m;
			invoiceHeader3.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoiceHeader3.JobComInvoiceLines.AddNew().JI_LinePrice = 50.00m;
			AssertEquals("InvoiceHeaders.TotalInvoiceAmount", new Money(350.00m, invoiceHeader2.LocalCurrency).ToString(), declaration.Invoices.TotalInvoiceLinesAmount.ToString());
		}

		public void TestDeletingChangedInvoiceMarkApportionmentDirty()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			testDec.ApportionmentDirty = false;
			testDec.Invoices.Delete(invoice);
			AssertEquals("Appportionment Dirty", true, testDec.ApportionmentDirty);
		}

		public void TestDeletingInvoiceInDBMarksApportionmentDirty()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			Factory.Save();

			testDec.ApportionmentDirty = false;
			testDec.Invoices.Delete(invoice);
			AssertEquals("Appportionment Dirty", true, testDec.ApportionmentDirty);
		}

		public void TestDetachOfPersistedObjectsSavedCorrectly()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.FillWithValidTestData();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			Factory.Save();

			Assert("No changes", !((IBusinessObjectCollection)Collection).HasChanges);
			invoice.JZ_JE = ZGuid.Empty;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AssertEquals("Change has been saved", ZGuid.Empty, factory2.Load<BaseJobComInvoiceHeader>(invoice.PK).JZ_JE);
			AssertEquals("No invoices against TestDec", 0, factory2.Load<BaseJobDeclaration>(testDec.PK).Invoices.Count);
		}

		[ExpectNoExceptions]
		public void TestDeleteInvoiceBeforeInvoiceLinesAreReferenced()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceGroupHeader subGroup = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine line = testDec.FilteredInvoiceLines.AddNew();

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			BaseJobDeclaration decLoaded = anotherFactory.Load<BaseJobDeclaration>(testDec.PK);
			BaseJobComInvoiceHeader invoiceLoaded = decLoaded.Invoices[0];
			decLoaded.Invoices.Delete(invoiceLoaded);
		}

		public void TestSetAndRemoveCollectionRelationships()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_JZ_GroupInvoiceFK = ZGuid.Empty;
			invoice.JZ_JE = ZGuid.Empty;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			ZGuid dummyGuidForEntryLine = ZGuid.NewZGuid();
			ZGuid dummyGuidForContainer = ZGuid.NewZGuid();
			ZGuid dummyGuidForOrderLine = ZGuid.NewZGuid();

			invoiceLine.JI_CL = dummyGuidForEntryLine;
			invoiceLine.JI_CO = dummyGuidForContainer;
			invoiceLine.JI_JO = dummyGuidForOrderLine;
			var containerPivot = invoiceLine.ContainersPivot.AddNew();
			containerPivot.C2_CO = dummyGuidForContainer;
			testDec.Invoices.Add(invoice);
			invoice.JZ_GB = ZGuid.Empty;
			ZGuid dummyGuidForHouseBill = ZGuid.NewZGuid();
			invoice.JZ_CU_RelatedHouseBill = dummyGuidForHouseBill;
			AssertEquals("FK to declaration set", testDec.PK, invoice.JZ_JE);
			AssertEquals("FK to GroupInvoice set when empty", testDec.JobComInvoiceGroupHeaders[0].PK, invoice.JZ_JZ_GroupInvoiceFK);
			AssertEquals("FK to House Bill still there", dummyGuidForHouseBill, invoice.JZ_CU_RelatedHouseBill);
			AssertEquals("FK to Container still there", dummyGuidForContainer, invoiceLine.JI_CO);
			AssertEquals("FK to Order Line still there", dummyGuidForOrderLine, invoiceLine.JI_JO);
			AssertEquals("FK to Entry Line unset", Guid.Empty, invoiceLine.JI_CL);
			AssertEquals("ContainersPivot collection cleared", 0, invoiceLine.ContainersPivot.Count);

			invoiceLine.JI_CL = dummyGuidForEntryLine;
			containerPivot = invoiceLine.ContainersPivot.AddNew();
			containerPivot.C2_CO = dummyGuidForContainer;
			invoice.JZ_JE = ZGuid.Empty;
			AssertEquals("FK to declaration unset", Guid.Empty, invoice.JZ_JE);
			AssertEquals("FK to GroupInvoice unset", Guid.Empty, invoice.JZ_JZ_GroupInvoiceFK);
			AssertEquals("FK to House Bill unset", Guid.Empty, invoice.JZ_CU_RelatedHouseBill);
			AssertEquals("FK to Container unset", Guid.Empty, invoiceLine.JI_CO);
			AssertEquals("FK to Order Line unset", Guid.Empty, invoiceLine.JI_JO);
			AssertEquals("FK to Entry Line still unset", Guid.Empty, invoiceLine.JI_CL);
			AssertEquals("ContainersPivot collection stll cleared", 0, invoiceLine.ContainersPivot.Count);

			Factory.Save();
			AssertEquals("FK to declaration still unset", Guid.Empty, invoice.JZ_JE);
			AssertEquals("FK to GroupInvoice still unset", Guid.Empty, invoice.JZ_JZ_GroupInvoiceFK);
			AssertEquals("FK to House Bill still unset", Guid.Empty, invoice.JZ_CU_RelatedHouseBill);
			AssertEquals("FK to Container still unset", Guid.Empty, invoiceLine.JI_CO);
			AssertEquals("FK to Order Line still unset", Guid.Empty, invoiceLine.JI_JO);
			AssertEquals("FK to Entry Line still unset", Guid.Empty, invoiceLine.JI_CL);
			AssertEquals("ContainersPivot collection stll cleared", 0, invoiceLine.ContainersPivot.Count);

			ZGuid newGuid = ZGuid.NewZGuid();
			invoice.JZ_JZ_GroupInvoiceFK = newGuid;
			testDec.Invoices.Add(invoice);
			AssertEquals("FK to GroupInvoice not set when not empty", newGuid, invoice.JZ_JZ_GroupInvoiceFK);
		}

		public virtual void TestRemovingInvoiceLeadsToReapportion()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				BaseJobDeclaration testDec = GetNewJobDeclaration();
				BaseJobComInvoiceHeader invoice1 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoice1.JZ_InvoiceAmount = 10000m;
				invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice1.JZ_IncoTerm = "FOB";

				BaseJobComInvoiceHeader invoice2 = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoice2.JZ_InvoiceAmount = 10000m;
				invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice2.JZ_IncoTerm = "FOB";

				BaseJobComInvHeaderCharge cOM = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 1000m, testDec.LocalCurrencyCode);
				testDec.ResumeApportionment();
				AssertEquals("Apportioned for Invoice1", 500m, invoice1.GroupCharges.GetCharge(cOM.ChargeKey).Amount);
				AssertEquals("Apportioned for invoice2", 500m, invoice2.GroupCharges.GetCharge(cOM.ChargeKey).Amount);

				testDec.Invoices.Delete(invoice1);
				testDec.ResumeApportionment();
				AssertEquals("Apportioned for invoice2", 1000m, invoice2.GroupCharges.GetCharge(cOM.ChargeKey).Amount);
			}
		}

		public void TestAddingInvoiceAddsLinesToDeclaration()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			AssertEquals("Initially no lines", 0, testDec.FilteredInvoiceLines.Count);

			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);
			BusinessObject line1 = invoice.JobComInvoiceLines.AddNew();
			BusinessObject line2 = invoice.JobComInvoiceLines.AddNew();

			testDec.Invoices.Add(invoice);
			AssertEquals("2 lines added", 2, testDec.FilteredInvoiceLines.Count);
			AssertEquals("2 lines added", 2, testDec.InvoiceLines.Count);
			Assert("Right lines", testDec.FilteredInvoiceLines.Contains(line1));
			Assert("Right lines", testDec.FilteredInvoiceLines.Contains(line2));
		}

		public void TestRemovingInvoiceRemovesLinesFromDeclaration()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);

			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(invoice);
			BusinessObject line1 = invoice.JobComInvoiceLines.AddNew();
			BusinessObject line2 = invoice.JobComInvoiceLines.AddNew();
			testDec.Invoices.Add(invoice);
			AssertEquals("2 lines added", 2, testDec.FilteredInvoiceLines.Count);

			BusinessObject otherLine = testDec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals("3 lines added", 3, testDec.FilteredInvoiceLines.Count);
			invoice.JZ_JE = ZGuid.Empty;
			AssertEquals("One line remaining", 1, testDec.FilteredInvoiceLines.Count);
			AssertEquals("Right lines removed", otherLine, testDec.FilteredInvoiceLines[0]);
		}

		public void TestSetMasterOfInvoice()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = testDec.Invoices.AddNew();
			invoiceHeader.JZ_Calc_GroupInvoice = groupHeader.JZ_InvoiceNumber;
			AssertEquals("Master is set", groupHeader, invoiceHeader.Master);
		}

		public void TestPopulateJobDeclarationReference()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader testItem = testDec.Invoices.AddNew();
			AssertEquals("Declaration Reference is set", testDec.PK, testItem.JZ_JE);
		}

		public void TestAddingAnInvoiceAddsLinesWhenNotLoading()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);

			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			BaseJobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("TestDec has no invoices", 0, testDec.Invoices.Count);
			AssertEquals("TestDec has no invoice lines", 0, testDec.FilteredInvoiceLines.Count);
			AssertEquals("TestDec has no invoice lines", 0, testDec.InvoiceLines.Count);
			testDec.Invoices.Add(invoice);
			AssertEquals("TestDec has one invoice", 1, testDec.Invoices.Count);
			AssertEquals("TestDec has one invoice line", 1, testDec.FilteredInvoiceLines.Count);
			AssertEquals("TestDec has one invoice lines", 1, testDec.InvoiceLines.Count);
		}

		public void TestEarliestAndLatestInvoice()
		{
			var declaration = GetNewJobDeclaration();

			var invoiceHeaderMock1 = Factory.NewMoq<TJobComInvoiceHeader>();
			var invoiceHeader1 = invoiceHeaderMock1.Object;
			invoiceHeader1.JZ_JE = declaration.PK;
			invoiceHeaderMock1.Setup(m => m.DeclarationDate).Returns(new ZDateTime(2007, 9, 01));

			var invoiceHeaderMock2 = Factory.NewMoq<TJobComInvoiceHeader>();
			var invoiceHeader2 = invoiceHeaderMock2.Object;
			invoiceHeader2.JZ_JE = declaration.PK;
			invoiceHeaderMock2.Setup(m => m.DeclarationDate).Returns(new ZDateTime(2007, 9, 02));

			var invoiceHeaderMock3 = Factory.NewMoq<TJobComInvoiceHeader>();
			var invoiceHeader3 = invoiceHeaderMock3.Object;
			invoiceHeader3.JZ_JE = declaration.PK;
			invoiceHeaderMock3.Setup(m => m.DeclarationDate).Returns(new ZDateTime(2006, 12, 31));

			AssertEquals("Earliest Invoice", invoiceHeader3, declaration.Invoices.EarliestInvoice);
			AssertEquals("Latest Invoice", invoiceHeader2, declaration.Invoices.LatestInvoice);
		}

		#region Implementation
		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetNewJobDeclaration();
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		protected virtual BaseJobDeclaration GetNewJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_JE = Declaration.PK;
			return invoice;
		}

		protected override TCollection GetCollectionToTest()
		{
			return (TCollection)Declaration.Invoices;
		}

		protected void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate, RefCurrency foreignCurrency)
		{
			SetExchangeRate(startDate, endDate, exchangeRate, foreignCurrency, Factory);
		}

		public static void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate, RefCurrency foreignCurrency, BusinessObjectFactory factory)
		{
			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, foreignCurrency.RX_Code);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, "CUS");
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, startDate.AddDays(1));
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, endDate);

			var exchangeRateDuty = factory.LoadTop1<RefExchangeRate>(sQLFilter);
			if (exchangeRateDuty != null)
			{
				exchangeRateDuty.Delete();
			}

			RefExchangeRate newOne = factory.New<RefExchangeRate>();
			newOne.RE_ExpiryDate = endDate;
			newOne.RE_ExRateType = "CUS";
			newOne.RE_GC = GlbCompany.CurrentCompany.PK;
			newOne.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			newOne.RE_StartDate = startDate;
			newOne.RE_SellRate = exchangeRate;
		}
		#endregion
	}
}

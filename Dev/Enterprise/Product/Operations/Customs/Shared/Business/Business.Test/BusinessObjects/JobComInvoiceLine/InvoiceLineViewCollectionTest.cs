using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	using System;
	using Enterprise.Customs.Common;
	using Enterprise.Customs.DataRegistry.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.DataMapping;
	using Enterprise.ZArchitecture.Schema;
	using Moq;
	using NUnit.Framework;

	public class InvoiceLineCollectionInternalTest : TestCaseWithFactory
	{
		public void TestCopyLastLineDetailsToNewLines()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;

			collection.CopyLastLineDetailsToNewLines = true;

			BaseJobComInvoiceLine line1 = collection.AddNew();
			AssertEquals((ZShort)1, line1.JI_LineNo);
			line1.JI_InvoiceQuantity = 5;
			line1.JI_InvoiceUQ = "KG";
			line1.JI_PartAttrib1 = "pa1";

			BaseJobComInvoiceLine line2 = collection.AddNew();
			AssertEquals((ZShort)2, line2.JI_LineNo);
			AssertEquals((ZDecimal)5, line2.JI_InvoiceQuantity);
			AssertEquals("KG", line2.JI_InvoiceUQ);
			AssertEquals("pa1", line2.JI_PartAttrib1);

			collection.CopyLastLineDetailsToNewLines = false;
			BaseJobComInvoiceLine line3 = collection.AddNew();
			AssertEquals((ZShort)3, line3.JI_LineNo);
			AssertEquals((ZDecimal)0, line3.JI_InvoiceQuantity);
			AssertEquals("", line3.JI_InvoiceUQ);
			AssertEquals("", line3.JI_PartAttrib1);
		}

		public void TestLoadView()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			var collection = new InvoiceLineViewCollection<BaseJobComInvoiceLine>(declaration);
			AssertEquals("Current LoadView is set to default", InvoiceLineViewCollection<BaseJobComInvoiceLine>.LoadView.All, collection.currentLoadView);

			collection.LoadFilteredLines(null);
			AssertEquals("Current LoadView is set to Filtered Only", InvoiceLineViewCollection<BaseJobComInvoiceLine>.LoadView.FilteredOnly, collection.currentLoadView);
		}

		public void TestLoadFilteredLines()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;
			var line1 = collection.AddNew();
			var line2 = collection.AddNew();
			var line3 = collection.AddNew();
			collection.LoadFilteredLines(x => x.JI_LineNo >= 2 && x.JI_LineNo <= 3);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(line2, collection);
			AssertCollectionContains(line3, collection);
			collection.LoadFilteredLines(x => x.JI_LineNo < 2);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(line1, collection);
		}

		public void TestNotificationsViewerProviderInterface()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;
			var provider = (IBusinessObjectCollectionNotificationsViewerProvider)collection;
			AssertEquals(2, provider.HumanReadableColumns.Count);
			AssertEquals("Column 1 Field Name", BaseJobComInvoiceLine.Schema.JI_Calc_Invoice, provider.HumanReadableColumns[0].FieldName);
			AssertEquals("Column 1 Caption", "Inv. No.", provider.HumanReadableColumns[0].Caption.Caption);
			AssertEquals("Column 1 Width", 200, provider.HumanReadableColumns[0].ColumnWidth);
			AssertEquals("Column 2 Field Name", BaseJobComInvoiceLine.Schema.JI_LineNo, provider.HumanReadableColumns[1].FieldName);
			AssertEquals("Column 2 Caption", "Inv. Line#", provider.HumanReadableColumns[1].Caption.Caption);
			AssertEquals("Column 2 Width", 100, provider.HumanReadableColumns[1].ColumnWidth);
		}
	}

	public abstract class InvoiceLineCollectionTest<TInvoiceLineCollection, TInvoiceLine> : BusinessObjectCollectionViewTestCase<TInvoiceLineCollection>
		where TInvoiceLineCollection : InvoiceLineViewCollection<TInvoiceLine>
		where TInvoiceLine : BaseJobComInvoiceLine
	{
		protected BaseJobDeclaration DeclarationForBizOCollectionTest
		{
			get
			{
				if (fDeclarationForBizOCollectionTest == null)
				{
					fDeclarationForBizOCollectionTest = GetDeclarationForBizOCollectionTest();
					fDeclarationForBizOCollectionTest.Invoices.AddNew();
				}
				return fDeclarationForBizOCollectionTest;
			}
		}
		BaseJobDeclaration fDeclarationForBizOCollectionTest;

		protected virtual BaseJobDeclaration GetDeclarationForBizOCollectionTest()
		{
			return Factory.New<BaseJobDeclaration>();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceLine result = Factory.New<BaseJobComInvoiceLine>();
			result.JI_JZ = DeclarationForBizOCollectionTest.Invoices[0].PK;
			return result;
		}

		public void TestIsThisPartOfTheCollectionWithFakeDeclaration()
		{
			var newFactory = NewFactory();

			var invoice = newFactory.New<BaseJobComInvoiceHeader>();
			var fakeDeclaration = (BaseJobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;

			var invoiceLines = fakeDeclaration.FilteredInvoiceLines;

			var invoiceLine = invoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			void AssertIsThisPartOfTheCollectionOnSaving(BusinessObjectFactory factory)
			{
				AssertEquals("Shoud change to empty.", ZGuid.Empty, invoice.JZ_JE);
				AssertEquals("Shoud change to the PK of fake declaration.", fakeDeclaration.PK, invoice.PreDeclarationPk);

				invoiceLines.Rebuild();
				AssertCollectionContains("The invoice line should be the part of fake declaration's FilteredInvoiceLines or it will be removed in 'SubsetBusinessObjectCollection - Rebuild'.", invoiceLine, invoiceLines);
			}

			void AssertIsThisPartOfTheCollectionOnSaved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				AssertEquals("Shoud restore to the PK of fake declaration.", fakeDeclaration.PK, invoice.JZ_JE);
				AssertEquals("Shoud change to empty.", ZGuid.Empty, invoice.PreDeclarationPk);

				invoiceLines.Rebuild();
				AssertCollectionContains("The invoice line should be the part of fake declaration's FilteredInvoiceLines or it will be removed in 'SubsetBusinessObjectCollection - Rebuild'.", invoiceLine, invoiceLines);
			}

			var action = new DisposableAction(() =>
			{
				newFactory.Saving += AssertIsThisPartOfTheCollectionOnSaving;
				newFactory.Saved += AssertIsThisPartOfTheCollectionOnSaved;
			}, () =>
			{
				newFactory.Saving -= AssertIsThisPartOfTheCollectionOnSaving;
				newFactory.Saved -= AssertIsThisPartOfTheCollectionOnSaved;
			});

			using (action)
			{
				newFactory.Save();

				AssertEquals("Shoud keep the PK of fake declaration.", fakeDeclaration.PK, invoice.JZ_JE);
				AssertEquals("Shoud change to empty.", ZGuid.Empty, invoice.PreDeclarationPk);
			}
		}

		public void TestRebuildWhenInvoiceIsDetached()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			declaration.Invoices.Add(invoice);
			AssertEquals("InvoiceLines are attached too", 2, declaration.InvoiceLines.Count);
			AssertEquals("InvoiceLines are attached too", 2, declaration.FilteredInvoiceLines.Count);

			invoice.JZ_JE = ZGuid.Empty;
			AssertEquals("InvoiceLines are detached too", 0, declaration.InvoiceLines.Count);
			AssertEquals("InvoiceLines are detached too", 0, declaration.FilteredInvoiceLines.Count);
		}

		public void TestLineNoDoesntGetOverwrittenWhenLoaded()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";

			BaseJobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			line1.JI_LinePrice = 100m;

			BaseJobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			line2.JI_LinePrice = 200m;

			AssertEquals("Line1 Line number", (short)1, line1.JI_LineNo);
			AssertEquals("Line2 Line number", (short)2, line2.JI_LineNo);

			declaration.InvoiceLines.Sort(BaseJobComInvoiceLine.Schema.JI_LinePrice, ListSortDirection.Descending);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobDeclaration loadedDeclaration = newFactory.New<BaseJobDeclaration>();
			foreach (BaseJobComInvoiceLine invoiceLine in loadedDeclaration.InvoiceLines)
			{
				if (invoiceLine.JI_LinePrice == 100m)
				{
					AssertEquals("Line1 LineNumber", (short)1, invoiceLine.JI_LineNo);
				}
				else if (invoiceLine.JI_LinePrice == 200m)
				{
					AssertEquals("Line2 LineNumber", (short)2, invoiceLine.JI_LineNo);
				}
			}
		}

		public void TestSortNotAffectLineNO()
		{
			line3.JI_InvoiceQuantity = 3;
			line2.JI_InvoiceQuantity = 2;
			line1.JI_InvoiceQuantity = 1;

			header.JobComInvoiceLines.Sort(BaseJobComInvoiceLine.Schema.JI_InvoiceQuantity, ListSortDirection.Descending);

			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line2.JI_LineNo);
			AssertEquals("Line No", (short)3, line3.JI_LineNo);
		}

		public void TestUpdateLineNumbersNewAdded()
		{
			BaseJobComInvoiceLine line4 = header.JobComInvoiceLines.AddNew();
			AssertEquals("Line No", (short)4, line4.JI_LineNo);
		}

		public void TestUpdateLineNumbersDeleted()
		{
			line2.Delete();
			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line3.JI_LineNo);
		}

		public void TestUpdateLineNumbersDeletedAfterSorting()
		{
			using (line1.InvoiceHeader.GetLineNumberRenumberingSuspender())
			{
				line3.JI_InvoiceQuantity = 3;
				line2.JI_InvoiceQuantity = 2;
				line1.JI_InvoiceQuantity = 1;
			}

			header.JobComInvoiceLines.Sort(BaseJobComInvoiceLine.Schema.JI_InvoiceQuantity, ListSortDirection.Descending);

			line2.Delete();
			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line3.JI_LineNo);
		}

		public void TestRecordWithInvoiceChangeStaysInList()
		{
			header.JZ_InvoiceNumber = "INV 1";
			BaseJobComInvoiceLine newLine = Declaration.InvoiceLines.AddNew();
			AssertEquals("Total Invoice Line Count", 4, Declaration.InvoiceLines.Count);
			BaseJobComInvoiceHeader newHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			newHeader.JZ_InvoiceNumber = "INV 2";
			newLine.JI_Calc_Invoice = newHeader.JZ_InvoiceNumber;
			AssertEquals("Total Invoice Line Count", 4, Declaration.InvoiceLines.Count);
		}

		public void TestBalance()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1000";
			invoiceHeader.JZ_RX_NKInvoice_Currency = aud.RX_Code;
			invoiceHeader.JZ_InvoiceAmount = 1000m;

			BaseJobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Calc_Invoice = invoiceHeader.JZ_InvoiceNumber;
			line1.JI_LinePrice = 1000m;
			AssertEquals("Balance of invoice header should be 0", 0m, invoiceHeader.JZ_Calc_Balance);
		}

		public void TestHasUnclassifiedLines()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";

			BaseJobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			line1.JI_Calc_Invoice = "1";
			line1.JI_Tariff = "0000.00.00";

			line2.JI_Calc_Invoice = "1";

			AssertEquals("There is one unclassified line", true, declaration.FilteredInvoiceLines.HasUnclassifiedLines);
		}

		public void TestSuspendAdditionallyForImportZeroValues()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;

			// Adds lines to the collection
			PopulateCollection(collection);

			AssertEquals("Line Number reset to 1 without suspender", (ZShort)1, collection[0].JI_LineNo);
			AssertEquals("Line Number reset to 2 without suspender", (ZShort)2, collection[1].JI_LineNo);
			AssertEquals("Line Number reset to 3 without suspender", (ZShort)3, collection[2].JI_LineNo);
			AssertEquals("Line Number reset to 4 without suspender", (ZShort)4, collection[3].JI_LineNo);
			AssertEquals("Line Number reset to 5 without suspender", (ZShort)5, collection[4].JI_LineNo);

			using (collection.SuspendAdditionallyForImport())
			{
				PopulateCollection(collection);

				AssertEquals("Line Number unaltered as 0", (ZShort)0, collection[0].JI_LineNo);
				AssertEquals("Line Number unaltered as 0", (ZShort)0, collection[1].JI_LineNo);
				AssertEquals("Line Number unaltered as 0", (ZShort)0, collection[2].JI_LineNo);
				AssertEquals("Line Number unaltered as 0", (ZShort)0, collection[3].JI_LineNo);
				AssertEquals("Line Number unaltered as 0", (ZShort)0, collection[4].JI_LineNo);
			}

			AssertEquals("Line Number reset to 1 after suspender because of zero in source", (ZShort)1, collection[0].JI_LineNo);
			AssertEquals("Line Number reset to 2 after suspender because of zero in source", (ZShort)2, collection[1].JI_LineNo);
			AssertEquals("Line Number reset to 3 after suspender because of zero in source", (ZShort)3, collection[2].JI_LineNo);
			AssertEquals("Line Number reset to 4 after suspender because of zero in source", (ZShort)4, collection[3].JI_LineNo);
			AssertEquals("Line Number reset to 5 after suspender because of zero in source", (ZShort)5, collection[4].JI_LineNo);
		}

		public void TestSuspendAdditionallyForImportNonZeroValues()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;

			// Adds lines to the collection
			PopulateCollection(collection, new ZShort[] { 37, 40, 70, 87, 93 });

			AssertEquals("Line Number reset to 1 without suspender", (ZShort)1, collection[0].JI_LineNo);
			AssertEquals("Line Number reset to 2 without suspender", (ZShort)2, collection[1].JI_LineNo);
			AssertEquals("Line Number reset to 3 without suspender", (ZShort)3, collection[2].JI_LineNo);
			AssertEquals("Line Number reset to 4 without suspender", (ZShort)4, collection[3].JI_LineNo);
			AssertEquals("Line Number reset to 5 without suspender", (ZShort)5, collection[4].JI_LineNo);

			using (collection.SuspendAdditionallyForImport())
			{
				PopulateCollection(collection, new ZShort[] { 37, 40, 70, 87, 93 });

				AssertEquals("Line Number unaltered as 37", (ZShort)37, collection[0].JI_LineNo);
				AssertEquals("Line Number unaltered as 40", (ZShort)40, collection[1].JI_LineNo);
				AssertEquals("Line Number unaltered as 70", (ZShort)70, collection[2].JI_LineNo);
				AssertEquals("Line Number unaltered as 87", (ZShort)87, collection[3].JI_LineNo);
				AssertEquals("Line Number unaltered as 93", (ZShort)93, collection[4].JI_LineNo);
			}

			AssertEquals("Line Number remains unaltered as 37", (ZShort)37, collection[0].JI_LineNo);
			AssertEquals("Line Number remains unaltered as 40", (ZShort)40, collection[1].JI_LineNo);
			AssertEquals("Line Number remains unaltered as 70", (ZShort)70, collection[2].JI_LineNo);
			AssertEquals("Line Number remains unaltered as 87", (ZShort)87, collection[3].JI_LineNo);
			AssertEquals("Line Number remains unaltered as 93", (ZShort)93, collection[4].JI_LineNo);
		}

		public void TestSuspendAdditionallyForImportZeroAndNonZeroValues()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;

			// Adds lines to the collection
			PopulateCollection(collection, new ZShort[] { 0, 20, 37, 47, 84 });

			AssertEquals("Line Number 1 without suspender", (ZShort)1, collection[0].JI_LineNo);
			AssertEquals("Line Number 2 without suspender", (ZShort)2, collection[1].JI_LineNo);
			AssertEquals("Line Number 3 without suspender", (ZShort)3, collection[2].JI_LineNo);
			AssertEquals("Line Number 4 without suspender", (ZShort)4, collection[3].JI_LineNo);
			AssertEquals("Line Number 5 without suspender", (ZShort)5, collection[4].JI_LineNo);

			using (collection.SuspendAdditionallyForImport())
			{
				PopulateCollection(collection, new ZShort[] { 0, 20, 37, 47, 84 });

				AssertEquals("Line Number unaltered as 0", (ZShort)0, collection[0].JI_LineNo);
				AssertEquals("Line Number unaltered as 20", (ZShort)20, collection[1].JI_LineNo);
				AssertEquals("Line Number unaltered as 37", (ZShort)37, collection[2].JI_LineNo);
				AssertEquals("Line Number unaltered as 47", (ZShort)47, collection[3].JI_LineNo);
				AssertEquals("Line Number unaltered as 84", (ZShort)84, collection[4].JI_LineNo);
			}

			AssertEquals("Line Number reset to 1 after suspender because of zero in source", (ZShort)1, collection[0].JI_LineNo);
			AssertEquals("Line Number reset to 2 after suspender because of zero in source", (ZShort)2, collection[1].JI_LineNo);
			AssertEquals("Line Number reset to 3 after suspender because of zero in source", (ZShort)3, collection[2].JI_LineNo);
			AssertEquals("Line Number reset to 4 after suspender because of zero in source", (ZShort)4, collection[3].JI_LineNo);
			AssertEquals("Line Number reset to 5 after suspender because of zero in source", (ZShort)5, collection[4].JI_LineNo);
		}

		public void TestSuspendAdditionallyForImport_ReadOnlyCheck()
		{
			ErrorReporter.Clear();
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;
			collection.RemoveAndDeleteAll();

			collection.ListChanged += (sender, args) =>
			{
				if (collection.Count > 0)
				{
					collection.RemoveAll();
				}
			};

			var properties = new List<IImportPropertyInfo>();

			var collectionInfo = new Mock<IImportCollectionInfo>();
			collectionInfo.Setup(m => m.Collection).Returns(collection);
			collectionInfo.Setup(m => m.Properties).Returns(properties);
			var wizard = new ImportWizard(collectionInfo.Object, null, default);
			AssertNoExceptionThrown(() => wizard.GenerateReadOnlyWarnings());
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		#region Implementation

		protected virtual BaseJobDeclaration GetNewDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		protected virtual BaseJobComInvoiceHeader GetNewInvoiceHeader()
		{
			if (Declaration != null)
			{
				return Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			}
			else
			{
				return null;
			}
		}

		BaseJobDeclaration fDeclaration;
		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetNewDeclaration();
				}
				return fDeclaration;
			}
		}
		protected BaseJobComInvoiceHeader header;
		protected BaseJobComInvoiceLine line1;
		protected BaseJobComInvoiceLine line2;
		protected BaseJobComInvoiceLine line3;
		protected RefCurrency aud;

		protected override void SetUp()
		{
			base.SetUp();
			header = GetNewInvoiceHeader();
			header.JZ_InvoiceNumber = "INV1";
			line1 = header.JobComInvoiceLines.AddNew();
			line2 = header.JobComInvoiceLines.AddNew();
			line3 = header.JobComInvoiceLines.AddNew();
			aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
		}

		void PopulateCollection(IInvoiceLineViewCollection<BaseJobComInvoiceLine> collection, ZShort[] values = null)
		{
			collection.RemoveAndDeleteAll();

			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();

			if (values != null && values.Length == 5)
			{
				collection[0].JI_LineNo = values[0];
				collection[1].JI_LineNo = values[1];
				collection[2].JI_LineNo = values[2];
				collection[3].JI_LineNo = values[3];
				collection[4].JI_LineNo = values[4];
			}
		}

		#endregion
	}

	[TestedType(typeof(InvoiceLineViewCollection<BaseJobComInvoiceLine>))]
	public class InvoiceLineCollectionTest : InvoiceLineCollectionTest<InvoiceLineViewCollection<BaseJobComInvoiceLine>, BaseJobComInvoiceLine>
	{
		protected override InvoiceLineViewCollection<BaseJobComInvoiceLine> GetCollectionToTest()
		{
			return new InvoiceLineViewCollection<BaseJobComInvoiceLine>(DeclarationForBizOCollectionTest);
		}
	}

	public class ApportionmentTest : TestCaseWithFactory
	{
		public void TestDeletingALineMarkApportionmentDirty()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = testDec.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 10m;

			testDec.ApportionmentDirty = false;
			AssertEquals("PreCondition:ApportionmentEnabled", false, testDec.ApportionmentDirty);

			testDec.FilteredInvoiceLines.RemoveAndDelete(invoiceLine);
			AssertEquals("Apportionment is dirty now", true, testDec.ApportionmentDirty);
		}

		public void TestDeletingInvoiceLineReapportionGroupCharge()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
				BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 30000m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 10000;

				BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 10000;

				BaseJobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
				line3.JI_LinePrice = 10000;

				BaseJobComInvHeaderCharge aDD = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 300m, testDec.LocalCurrencyCode);
				testDec.ResumeApportionment();

				AssertEquals("PreCondition:Line1 has an apportioned ADD", 100m, line1.ApportionedCharges.GetCharge(aDD.ChargeKey).Amount);
				AssertEquals("PreCondition:Line2 has an apportioned ADD", 100m, line2.ApportionedCharges.GetCharge(aDD.ChargeKey).Amount);
				AssertEquals("PreCondition:Line3 has an apportioned ADD", 100m, line3.ApportionedCharges.GetCharge(aDD.ChargeKey).Amount);

				testDec.FilteredInvoiceLines.RemoveAndDelete(line3);
				testDec.ResumeApportionment();

				AssertEquals("Line1 has an apportioned ADD", 150m, line1.ApportionedCharges.GetCharge(aDD.ChargeKey).Amount);
				AssertEquals("Line2 has an apportioned ADD", 150m, line2.ApportionedCharges.GetCharge(aDD.ChargeKey).Amount);
			}
		}
	}

	public class InvoiceLineCollectionDefaultTest : TestCaseWithFactory
	{
		public void TestLineNoSetForTheFirstLine()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = testDec.InvoiceLines.AddNew();

			AssertEquals("JI_JZ is set", invoice.PK, invoiceLine.JI_JZ);
			AssertEquals("JI_LineNo is set", (short)1, invoiceLine.JI_LineNo);
		}

		public void TestLineNoSetForTheSecondLine()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine line1 = testDec.InvoiceLines.AddNew();
			AssertEquals("JI_JZ is set", invoice.PK, line1.JI_JZ);
			AssertEquals("JI_LineNo is set", (short)1, line1.JI_LineNo);

			BaseJobComInvoiceLine line2 = testDec.InvoiceLines.AddNew();
			AssertEquals("JI_JZ is set for line2", invoice.PK, line2.JI_JZ);
			AssertEquals("JI_LineNo is set for line2", (short)2, line2.JI_LineNo);
		}

		public void TestSettingReferenceToInvoice()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = testDec.InvoiceLines.AddNew();
			AssertEquals("JI_JZ", invoice.PK, invoiceLine.JI_JZ);
		}

		public void TestSetDefaultsForTheFirstLine()
		{
			invoice.JZ_InvoiceNumber = "TEST";

			BaseJobComInvoiceLine invoiceLine = testCollection.AddNew();
			AssertEquals("InvoiceNumber defaulted", invoice.JZ_InvoiceNumber, invoiceLine.JI_Calc_Invoice);
			AssertEquals("JI_JZ defaulted", invoice.PK, invoiceLine.JI_JZ);
		}

		public void TestSetDefaultFromPreviousLine()
		{
			BaseJobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = testCollection.AddNew();
			AssertEquals("InvoiceLine1.JI_JZ", invoice.PK, invoiceLine1.JI_JZ);

			invoiceLine1.JI_JZ = invoice2.PK;
			invoiceLine1.JI_OrderNumber = "Order Num";
			BaseJobComInvoiceLine invoiceLine2 = testCollection.AddNew();
			AssertEquals("InvoiceLine2.JI_JZ", invoice2.PK, invoiceLine2.JI_JZ);
			AssertEquals("InvoiceLine2.JI_LineNo", (short)2, invoiceLine2.JI_LineNo);
			AssertEquals("InvoiceLine2.JI_OrderNumber", "Order Num", invoiceLine2.JI_OrderNumber);
		}

		public void TestSetDefaultForOrderNumberWhenIsAttachedOrder()
		{
			Order attachedOrder1 = testDec.AttachedOrders.AddNew();
			attachedOrder1.JD_OrderNumber = "323:000";

			Order attachedOrder2 = testDec.AttachedOrders.AddNew();
			attachedOrder2.JD_OrderNumber = "323:000";
			attachedOrder2.JD_OrderNumberSplit = 1;

			BaseJobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_OrderNumber = "323:000";

			BaseJobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			AssertEquals("Order number", ZString.Empty, invoiceLine2.JI_OrderNumber);

			invoiceLine2.JI_OrderNumber = attachedOrder2.JD_OrderNumberAndSplit;

			BaseJobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			AssertEquals("Order Number", ZString.Empty, invoiceLine3.JI_OrderNumber);

			invoiceLine3.JI_OrderNumber = "Test";
			BaseJobComInvoiceLine invoiceLine4 = invoice2.JobComInvoiceLines.AddNew();
			AssertEquals("Order Number", "Test", invoiceLine4.JI_OrderNumber);
		}

		public void TestDontDefaultLineOrigin()
		{
			var supplierFromNZ = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "NZ"));
			invoice.JZ_OH_Supplier = supplierFromNZ.PK;
			BaseJobComInvoiceLine line = testCollection.AddNew();

			AssertEquals("Line's invoice", invoice.PK, line.JI_JZ);
			AssertEquals("Line origin should not be defaulted from supplier's origin as AU clients dont want this behaviour. If you want this, please override the method in your country", "", line.JI_CountryOfOrigin);

			line.JI_CountryOfOrigin = "NZ";
			BaseJobComInvoiceLine line2 = testCollection.AddNew();
			AssertEquals("Line2 origin should not be defaulted from previous line orign as AU clients dont want this behaviour. If you want, please override the method in your country", "", line2.JI_CountryOfOrigin);
		}

		public void TestSortByMergedLineNumber()
		{
			BaseJobComInvoiceLine line1 = testCollection.AddNew();
			CusEntryLine mergedLine1 = Factory.New<CusEntryLine>();
			mergedLine1.CL_LineNumber = 39;
			line1.JI_CL = mergedLine1.PK;

			BaseJobComInvoiceLine line2 = testCollection.AddNew();
			CusEntryLine mergedLine2 = Factory.New<CusEntryLine>();
			mergedLine2.CL_LineNumber = 4;
			line2.JI_CL = mergedLine2.PK;

			testCollection.Sort(BaseJobComInvoiceLine.Schema.MergedLineNumber);
			AssertEquals("4", testCollection[0].MergedLineNumber);
			AssertEquals("39", testCollection[1].MergedLineNumber);
			testCollection.Sort(BaseJobComInvoiceLine.Schema.MergedLineNumber, ListSortDirection.Descending);
			AssertEquals("39", testCollection[0].MergedLineNumber);
			AssertEquals("4", testCollection[1].MergedLineNumber);

			line1.JI_CL = ZGuid.Empty;
			testCollection.Sort(BaseJobComInvoiceLine.Schema.MergedLineNumber, ListSortDirection.Ascending);
			AssertEquals(line1, testCollection[0]);
			AssertEquals(line2, testCollection[1]);
		}

		#region Implementation

		BaseJobDeclaration testDec;
		BaseJobComInvoiceHeader invoice;
		IInvoiceLineViewCollection<BaseJobComInvoiceLine> testCollection;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = BaseJobDeclaration.New(Factory);
			invoice = testDec.Invoices.AddNew();
			testCollection = testDec.FilteredInvoiceLines;
		}

		#endregion
	}

	sealed class InvoiceLineViewCollectionBaseOnlyTest : TestCaseWithFactory
	{
		public void TestAddingDetachInvoiceAffectWeightApportion()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 1000m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			BaseJobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			AssertEquals(1000m, invoiceLine1.JI_Weight);
			BaseJobComInvoiceLine invoiceLine2 = (BaseJobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew(); // add uncommitted invoice
			invoiceLine2.JI_LinePrice = 1000m;
			AssertEquals(500m, invoiceLine1.JI_Weight);
			AssertEquals(500m, invoiceLine2.JI_Weight);
			((ICancelAddNew)declaration.FilteredInvoiceLines).CancelNew(1);
			AssertEquals(1000m, invoiceLine1.JI_Weight);
			AssertEquals(true, invoiceLine2.IsDeleted);
		}

		public void TestInvoiceLineViewCollectionSupportAdditionalDeclarations()
		{
			var declaration1 = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration2.Invoices.Add(invoice);

			invoice.AttachToAdditionalDeclaration(declaration1);
			AssertEquals("FilteredInvoiceLines should contains one invoice line", 1, declaration1.FilteredInvoiceLines.Count);
			AssertCollectionContains("FilteredInvoiceLines should contains attached invoiceLine", invoiceLine, declaration1.FilteredInvoiceLines);
		}

		public void TestReApportionAfterImportedLines()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 10000m;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice1.JZ_InvoiceAmount = 1000m;
			var invoiceLines = declaration.InvoiceLines;

			CombineAssertions(() =>
			{
				using (declaration.FilteredInvoiceLines.SuspendAdditionallyForImport())
				{
					var invoiceLine1 = invoiceLines.AddNew();
					invoiceLine1.JI_LinePrice = 400m;
					invoiceLine1.JI_Weight = 800m;
					var invoiceLine2 = invoiceLines.AddNew();
					invoiceLine2.JI_LinePrice = 600m;
					invoiceLine2.JI_Weight = 200m;
					AssertEquals("Weight 800 added when importing", 800m, invoiceLine1.JI_Weight);
					AssertEquals("Weight 200 added when importing", 200m, invoiceLine2.JI_Weight);
				}

				AssertEquals("Weight 800 added and ReApportion", 4000m, invoiceLines[0].JI_Weight);
				AssertEquals("Weight 200 added and ReApportion", 6000m, invoiceLines[1].JI_Weight);
			});
		}
	}

	public abstract class InvoiceLineCollectionBOTest<T> : BusinessObjectCollectionViewTestCase<T> where T : IBusinessObjectCollectionView
	{
		BaseJobDeclaration fJobDeclaration;
		protected BaseJobDeclaration JobDeclaration
		{
			get
			{
				if (fJobDeclaration == null)
				{
					fJobDeclaration = BaseJobDeclaration.New(Factory);
				}
				return fJobDeclaration;
			}
		}

		BaseJobComInvoiceHeader fInvoice;
		protected BaseJobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = JobDeclaration.Invoices.AddNew();
				}
				return fInvoice;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceLine result = Factory.New<BaseJobComInvoiceLine>();
			result.JI_JZ = Invoice.PK;
			if (JobDeclaration.InvoiceLines.Contains(result))
			{
				JobDeclaration.InvoiceLines.Remove(result);
			}

			return result;
		}
	}
}

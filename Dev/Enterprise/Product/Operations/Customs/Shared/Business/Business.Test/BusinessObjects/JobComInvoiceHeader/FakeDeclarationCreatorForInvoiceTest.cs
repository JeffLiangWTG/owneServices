using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class FakeDeclarationCreatorForInvoiceTest : TestCaseWithFactory
	{
		public void TestFakeDeclarationClusterKeyShouldUpdateFromInvoice()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var declaration = (BaseJobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			Factory.Save();
			AssertNotEquals("Invoice cluster key should have been updated by cluster key architecture code", 0, invoice.JZ_ClusterKey);
			AssertEquals("Fake Declaration cluster key should be kept in sync with the Invoice, otherwise we have incorrect relationship filters for collections",
				invoice.JZ_ClusterKey, declaration.JE_ClusterKey);
		}

		public void TestFetchForLoadChildEditableObjects()
		{
			FetchStrategyTestHelper.AssertFetchHints<BaseJobComInvoiceHeader>(
				GetType(),
				Factory,
				SetupFetchForLoadChildEditableObjectsCoreTestCases,
				businessObject => new FakeDeclarationCreatorForInvoice(businessObject),
				businessObject => businessObject.LoadChildEditableObjects(),
				tablesToCollectQueriesFor: GetTablesToCollectQueriesFor()
				);
		}

		void SetupFetchForLoadChildEditableObjectsCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<BaseJobComInvoiceHeader>> testCases)
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			testCases = new List<FetchStrategyTestHelper.TestCase<BaseJobComInvoiceHeader>>
			{
				new FetchStrategyTestHelper.TestCase<BaseJobComInvoiceHeader>
				{
					BusinessObject = invoice,
					FetchStrategyExpectedHitCounts = FetchForLoadChildEditableObjectsFetchStrategyExpectedHitCounts,
					ExecuteActionExpectedHitCounts = FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts,
				}
			};
		}

		string[] GetTablesToCollectQueriesFor()
		{
			return new[]
			{
				JobComInvoiceHeaderSchema.Constants.TableName,
				JobComInvoiceLineSchema.Constants.TableName,
				GenCustomAddOnValueSchema.Constants.TableName,
				JobComInvLineRefsSchema.Constants.TableName,
				JobComInvoiceLineTaxSchema.Constants.TableName,
				JobDocAddressSchema.Constants.TableName,
				ProcessHeaderSchema.Constants.TableName,
			};
		}

		Dictionary<string, int> FetchForLoadChildEditableObjectsFetchStrategyExpectedHitCounts => new Dictionary<string, int>
		{
			{ JobComInvoiceLineSchema.Constants.TableName, 2 }
		};

		Dictionary<string, int> FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts => new Dictionary<string, int>
		{
			{ JobComInvoiceLineSchema.Constants.TableName, 0 },
			{ JobComInvHeaderChargeSchema.Constants.TableName, 1 },
			{ ProcessTasksSchema.Constants.TableName, 1 }
		};

		Dictionary<string, int> FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts => new Dictionary<string, int>
		{
			{ GenCustomAddOnValueSchema.Constants.TableName, 0 },
			{ JobComInvLineRefsSchema.Constants.TableName, 1 },
			{ JobComInvoiceLineTaxSchema.Constants.TableName, 1 },
			{ JobDocAddressSchema.Constants.TableName, 1 },
			{ ProcessHeaderSchema.Constants.TableName, 1 },
			{ CusContainerInvoiceLinePivotSchema.Constants.TableName, 1 },
			{ CusHouseContPackInvoiceLinePivotSchema.Constants.TableName, 1 },
			{ CusHouseContPackInvoiceHeaderPivotSchema.Constants.TableName, 1 },
			{ CusUnderbondDecSchema.Constants.TableName, 1 },
			{ JobComInvLineComponentInventorySchema.Constants.TableName, 1 }
		};

		public void TestSyncHasChangesChangedOnFilteredInvoiceLinesRemoveOrDelete()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var declaration = (BaseJobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;

			var state = (IBusinessObjectState)invoice;

			var invoiceLines = declaration.FilteredInvoiceLines;

			Factory.Save();
			Assert("Precondition", !state.HasChangesNotIncludingChildren);

			var isCalled = false;
			invoice.HasChangesChanged += (s, e) => { isCalled = e.ObjectJustWasChanged; };

			var invoiceLine = invoiceLines.AddNew();

			Assert("Was called when the collection is adding a new element(ClusterKey of element was changed).", isCalled);
			Assert("Should be true as the change happened at FilteredInvoiceLines and would reflect to invoice itself.", state.HasChangesNotIncludingChildren);

			isCalled = false;
			Factory.Save();

			Assert("Should not call it on saving.", !isCalled);
			Assert("Should reset to be false.", !state.HasChangesNotIncludingChildren);
			AssertEquals("Should add 1 invoice line.", 1, invoiceLines.Count);

			isCalled = false;
			invoiceLines.RemoveAndDelete(invoiceLine);

			Assert("Should force call it when the collection is removing or deleting an existing element.", isCalled);
			Assert("Should be true as the event is also hooked in ZFormPostingButtonsStrategy.", state.HasChangesNotIncludingChildren);

			isCalled = false;
			Factory.Save();

			Assert("Should not call it on saving.", !isCalled);
			Assert("Should reset to be false.", !state.HasChangesNotIncludingChildren);
			AssertEquals("Should remove and delete 1 invoice line.", 0, invoiceLines.Count);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestNullException()
		{
			new FakeDeclarationCreatorForInvoice(null);
		}

		public void TestApportionmentDirtyBeingMarked()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var faker = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)faker.HeaderData;
			AssertEquals("Apportionment should not be dirty to start with", false, declaration.ApportionmentDirty);

			Factory.Save();

			AssertEquals("Apportionment should stay as clean when saving happens", false, declaration.ApportionmentDirty);
		}

		public void TestImporterIsCopiedToFakeDeclarationWhenInvoiceIsInDB()
		{
			BaseJobComInvoiceHeader invoiceInDB = Factory.New<BaseJobComInvoiceHeader>();
			FakeDeclarationCreatorForInvoice faker = new FakeDeclarationCreatorForInvoice(invoiceInDB);
			invoiceInDB.FillWithValidTestData();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			invoiceInDB.JZ_OH_Buyer = org.PK;
			invoiceInDB.JZ_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobComInvoiceHeader invoice = factory2.Load<BaseJobComInvoiceHeader>(invoiceInDB.PK);
			FakeDeclarationCreatorForInvoice faker2 = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)faker2.HeaderData;
			AssertEquals("Declaration gets the Buyer from the invoice", invoice.JZ_OH_Buyer, declaration.JE_OH_Importer);
			AssertEquals("Invoice is import", JobMessageTypeList.Codes.Import, invoice.JZ_MessageType);
			AssertEquals("Setting a foreign importer to JE_OH_Importer should not change the message type. Declaration is import", true, declaration.IsImport);
		}

		public void TestSupplierIsNotCopiedToFakeDeclarationWhenInvoiceIsInNotDB()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			FakeDeclarationCreatorForInvoice declarationFaker = new FakeDeclarationCreatorForInvoice(invoice);
			invoice.JZ_OH_Buyer = ZGuid.NewZGuid();

			FakeDeclarationCreatorForInvoice declarationFaker2 = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)declarationFaker2.HeaderData;
			Assert("Declaration does not get the Buyer from the invoice", invoice.JZ_OH_Buyer != declaration.JE_OH_Importer);
		}

		public void TestHumanReadableName()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			FakeDeclarationCreatorForInvoice faker = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)faker.HeaderData;
			AssertNotNull("Commercial Invoice", declaration.HumanReadableName);
		}

		public void TestDeclarationGetsCreated()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			FakeDeclarationCreatorForInvoice faker = new FakeDeclarationCreatorForInvoice(invoice);
			AssertNotNull("Declaration", invoice.JobDeclaration);
		}

		public void TestGroupHeaderGetsCreated()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			FakeDeclarationCreatorForInvoice faker = new FakeDeclarationCreatorForInvoice(invoice);
			AssertNotNull("Declaration", invoice.GroupHeader);
		}

		public void TestCanSave()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.FillWithValidTestData();
			invoice.JZ_JZ_GroupInvoiceFK = ZGuid.Empty;
			invoice.JZ_JE = ZGuid.Empty;
			FakeDeclarationCreatorForInvoice faker = new FakeDeclarationCreatorForInvoice(invoice);
			invoice.RunPreSaveValidation();

			Factory.Save();
			Assert("Saved OK", invoice.IsInDatabase);
			var declaration = (BaseJobDeclaration)faker.HeaderData;
			Assert("Not persisted", !declaration.IsInDatabase);
			Assert("Not persisted", !declaration.JobComInvoiceGroupHeaders[0].IsInDatabase);
		}

		public void TestLineNumbersAreNotReset()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			FakeDeclarationCreatorForInvoice fake = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)fake.HeaderData;
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 1000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_Weight = 500m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line4 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line5 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LineNo = 1;
			AssertEquals((short)2, line1.JI_LineNo);
			AssertEquals((short)3, line2.JI_LineNo);
			AssertEquals((short)1, line3.JI_LineNo);
			AssertEquals((short)4, line4.JI_LineNo);
			AssertEquals((short)5, line5.JI_LineNo);
			AssertEquals(5, invoice.JobComInvoiceLines.Count);
			AssertEquals(5, declaration.FilteredInvoiceLines.Count);
			AssertEquals(5, declaration.InvoiceLines.Count);
			AssertEquals(500m, invoice.JZ_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, invoice.JZ_WeightUQ);
			Factory.Save();
			AssertEquals((short)2, line1.JI_LineNo);
			AssertEquals((short)3, line2.JI_LineNo);
			AssertEquals((short)1, line3.JI_LineNo);
			AssertEquals((short)4, line4.JI_LineNo);
			AssertEquals((short)5, line5.JI_LineNo);
			AssertEquals(5, invoice.JobComInvoiceLines.Count);
			AssertEquals(5, declaration.FilteredInvoiceLines.Count);
			AssertEquals(5, declaration.InvoiceLines.Count);
			AssertEquals(500m, invoice.JZ_Weight);
			AssertEquals(Core.Constants.Weight.Kilograms, invoice.JZ_WeightUQ);
		}

		public void TestHasChanges()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.FillWithValidTestData();
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_InvoiceAmount = 2000m;
			invoice.JZ_Weight = 500m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice.JZ_OH_Buyer = buyer.PK;
			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0001.01.01 1";
			line1.JI_CustomsUnitQty = "NO";
			line1.JI_InvoiceQuantity = 18m;
			line1.JI_InvoiceUQ = "DOZ";
			line1.JI_LinePrice = 150m;
			line1.JI_CustomsQuantity = ZDecimal.Zero;
			AssertEquals(ZDecimal.Zero, line1.JI_CustomsQuantity);

			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line4 = invoice.JobComInvoiceLines.AddNew();
			line4.JI_InvoiceQuantity = 1m;
			line4.JI_InvoiceUQ = "T";
			line4.JI_LinePrice = 23m;

			BaseJobComInvoiceLine line5 = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			FakeDeclarationCreatorForInvoice fake = new FakeDeclarationCreatorForInvoice(invoice);
			AssertEquals(false, line1.HasChanges);
			var declaration = (BaseJobDeclaration)fake.HeaderData;
			AssertEquals(false, declaration.HasChanges);
		}

		public void TestHeaderData_ShouldBeStandaloneCommonInvoiceProvider_WhenEnabledInHeader()
		{
			using var tempEnableNewStandaloneInvoiceData = CustomsDataRegistry.Instance.EnableNewStandaloneInvoiceData.SetTemporaryValue(default, default, default, true);

			var invoice = Factory.New<BaseJobComInvoiceHeaderForTesting>();
			invoice.UseNewStandaloneInvoiceDataCore_Exposed = true;
			var fake = new FakeDeclarationCreatorForInvoice(invoice);
			AssertType<StandaloneCommonInvoiceProvider>(fake.HeaderData);

			invoice.UseNewStandaloneInvoiceDataCore_Exposed = false;
			fake = new FakeDeclarationCreatorForInvoice(invoice);
			AssertType<BaseJobDeclaration>(fake.HeaderData);
		}

		sealed class BaseJobComInvoiceHeaderForTesting : BaseJobComInvoiceHeader
		{
			public BaseJobComInvoiceHeaderForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool UseNewStandaloneInvoiceDataCore_Exposed { get; set; }

			protected override bool UseNewStandaloneInvoiceDataCore => UseNewStandaloneInvoiceDataCore_Exposed;
		}
	}
}

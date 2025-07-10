using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccDraftInvoiceHeaderDataTest : AssemblyDataTest
	{
		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(AccDraftInvoiceHeader), DraftHeaderData.BusinessObjectType);
		}

		public void TestModuleID()
		{
			AssertNull("There isn't a module for Draft Invoice Header which is fine since it won't work in Manage > DocManager > Allocate eDocs", DraftHeaderData.ModuleID);
		}

		public void TestGetBusinessObjectCollection()
		{
			AssertNull("This is only used for the Allocate eDocs functionality, which we've disabled for Draft Invoice Header.", DraftHeaderData.GetBusinessObjectCollection(Factory));
		}

		public void TestReferenceType()
		{
			AssertEquals(Core.Constants.ReferenceTypes.Accounting, DraftHeaderData.ReferenceType);
		}

		public void TestShouldThrowExceptionForInvalidTransactionType()
		{
			var draftHeader = Factory.New<AccDraftInvoiceHeader>();
			draftHeader.AIH_TransactionType = null;
			AssertExceptionThrown<InvalidOperationException>("Should throw exception for null transaction type", "Invalid transaction type.", () => _ = DraftHeaderData.GetFriendlyName(draftHeader));

			draftHeader.AIH_TransactionType = string.Empty;
			AssertExceptionThrown<InvalidOperationException>("Should throw exception for empty transaction type", "Invalid transaction type.", () => _ = DraftHeaderData.GetFriendlyName(draftHeader));

			draftHeader.AIH_TransactionType = TransactionTypes.Journal;
			AssertExceptionThrown<InvalidOperationException>("Should throw exception for transaction type other than INV or CRD", "Invalid transaction type: JNL.", () => _ = DraftHeaderData.GetFriendlyName(draftHeader));
		}

		public void TestShouldCorrectlyDisplayDraftInvoiceWithInternalReference()
		{
			var draftHeader = Factory.New<AccDraftInvoiceHeader>();
			draftHeader.AIH_TransactionType = TransactionTypes.Invoice;
			AssertEquals("Draft Invoice", DraftHeaderData.GetFriendlyName(draftHeader));

			draftHeader.AIH_InternalReference = "11111";
			AssertEquals("Draft Invoice 11111", DraftHeaderData.GetFriendlyName(draftHeader));

			var draftHeader1 = Factory.New<AccDraftInvoiceHeader>();
			draftHeader1.AIH_TransactionType = TransactionTypes.Invoice;
			AssertEquals("Draft Invoice", DraftHeaderData.GetFriendlyName(draftHeader1));

			draftHeader1.AIH_InternalReference = "22222";
			AssertEquals("Draft Invoice 22222", DraftHeaderData.GetFriendlyName(draftHeader1));
		}

		public void TestShouldCorrectlyDisplayDraftCreditNoteWithInternalReference()
		{
			var draftHeader = Factory.New<AccDraftInvoiceHeader>();
			draftHeader.AIH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals("Draft Credit Note", DraftHeaderData.GetFriendlyName(draftHeader));

			draftHeader.AIH_InternalReference = "11111";
			AssertEquals("Draft Credit Note 11111", DraftHeaderData.GetFriendlyName(draftHeader));

			var draftHeader1 = Factory.New<AccDraftInvoiceHeader>();
			draftHeader1.AIH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals("Draft Credit Note", DraftHeaderData.GetFriendlyName(draftHeader1));

			draftHeader1.AIH_InternalReference = "22222";
			AssertEquals("Draft Credit Note 22222", DraftHeaderData.GetFriendlyName(draftHeader1));
		}

		#region Implementation

		AccDraftInvoiceHeaderData DraftHeaderData
		{
			get
			{
				if (draftInvoiceHeaderData == null)
				{
					draftInvoiceHeaderData = new AccDraftInvoiceHeaderData();
				}
				return draftInvoiceHeaderData;
			}
		}
		AccDraftInvoiceHeaderData draftInvoiceHeaderData;

		#endregion
	}
}

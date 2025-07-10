using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PGACollection))]
	public class PGACollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestType()
		{
			AssertEquals(typeof(PGA), PGACollection.AddNew().GetType());
		}

		public void TestCopyLaceyLineSetValueToZero()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var laceyline1 = invoiceLine.LaceyActLines.AddNew();
			laceyline1.US_InvCurrPGAValue = 11;

			declaration.CopyLastPGADetailsToNewLine = true;

			Factory.Save();

			var laceyline2 = invoiceLine.LaceyActLines.AddNew();
			AssertEquals(laceyline2.US_InvCurrPGAValue, decimal.Zero);
		}

		public void TestDeletdItemsAreExcluded()
		{
			var pga1 = PGACollection.AddNew();
			pga1.US_InvCurrPGAValue = 3m;
			pga1.US_PGALineValue = 3m;
			var pga2 = PGACollection.AddNew();
			pga2.US_InvCurrPGAValue = 3m;
			pga2.US_PGALineValue = 1m;
			AssertEquals("TotalPGAValue", 4m, PGACollection.TotalPGAValue);
			AssertEquals("TotalInvCurrPGAValue", 6m, PGACollection.TotalInvCurrPGAValue);
			pga1.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			AssertEquals("TotalPGAValue", 1m, PGACollection.TotalPGAValue);
			AssertEquals("TotalInvCurrPGAValue", 3m, PGACollection.TotalInvCurrPGAValue);
			pga1.US_TrackingStatus = PGATrackingStatusList.Codes.Deleting;
			AssertEquals("TotalPGAValue", 1m, PGACollection.TotalPGAValue);
			AssertEquals("TotalInvCurrPGAValue", 3m, PGACollection.TotalInvCurrPGAValue);
			pga1.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			AssertEquals("TotalPGAValue", 1m, PGACollection.TotalPGAValue);
			AssertEquals("TotalInvCurrPGAValue", 3m, PGACollection.TotalInvCurrPGAValue);
			pga1.Delete();
			AssertEquals("TotalPGAValue", 1m, PGACollection.TotalPGAValue);
			AssertEquals("TotalInvCurrPGAValue", 3m, PGACollection.TotalInvCurrPGAValue);
		}

		public void TestDefaults()
		{
			InvoiceLine.JI_Description = "TEST";
			InvoiceLine.JI_LinePrice = 20m;

			PGA pga = PGACollection.AddNew();
			AssertEquals(ZString.Empty, pga.US_PGACommercialDescription);
			AssertEquals(InvoiceLine.JI_LinePrice, pga.US_InvCurrPGAValue);
		}

		public void TestTotalPGAValue()
		{
			InvoiceLine.JI_LinePrice = 20.5m;

			var element = PGACollection.AddNew();
			element.US_InvCurrPGAValue = 15m;
			AssertEquals(15m, PGACollection.TotalInvCurrPGAValue);

			InvoiceLine.Declaration.ResumeApportionment();

			var nonCommitted = (PGA)((IBindingList)PGACollection).AddNew();
			AssertEquals("Defaulted the remaining value", 5.5m, nonCommitted.US_InvCurrPGAValue);
			AssertEquals("Declaration should not be marked as dirty", false, InvoiceLine.Declaration.ApportionmentDirty);

			((ICancelAddNew)PGACollection).CancelNew(1);
			AssertEquals("Declaration should not be marked as dirty", false, InvoiceLine.Declaration.ApportionmentDirty);

			nonCommitted = (PGA)((IBindingList)PGACollection).AddNew();
			nonCommitted.US_PGACommercialDescription = "xyz"; // BizOCollection.SetDefaultsForNewChild() will not set HasChanges on uncommitted element even if changes are made on its child
			((ICancelAddNew)PGACollection).EndNew(1);
			AssertEquals("Declaration should be marked as dirty now", true, InvoiceLine.Declaration.ApportionmentDirty);
		}

		public void TestAdd_MasterDeleted()
		{
			var collection = (PGACollection)GetCollectionToTest();
			collection.Master.Delete();
			PGA pga = collection.AddNew();
			AssertEquals("Master has been deleted", true, collection.Master.IsDeleted);
			AssertEquals(ZString.Empty, pga.US_PGACommercialDescription);
		}

		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.LaceyActLines;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.LaceyActLines;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PGACollection(InvoiceLine);
		}

		PGACollection PGACollection
		{
			get { return elements ?? (elements = InvoiceLine.LaceyActLines); }
		}
		PGACollection elements;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (invoiceHeader == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableCRL = true;
					invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				}
				return invoiceHeader;
			}
		}
		JobComInvoiceHeader invoiceHeader;

		#endregion
	}
}

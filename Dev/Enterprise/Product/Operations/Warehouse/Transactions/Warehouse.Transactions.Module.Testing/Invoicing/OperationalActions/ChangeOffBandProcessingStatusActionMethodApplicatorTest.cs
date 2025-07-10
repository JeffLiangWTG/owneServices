using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Invoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ChangeOffBandProcessingStatusActionMethodApplicator))]
	public class ChangeOffBandProcessingStatusActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction

		public void TestAction()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var today = ZDate.Today;

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, today.AddDays(-6), today);
			Helper.CreateJobCharges(invoice.JobHeader);
			Factory.Save();

			AssertEquals("Default", StorageOffBandProcessingStatus.Codes.NIQ, invoice.ET_OffBandProcessingStatus);
			Applicator.SelectedOffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			ApplyApplicator(new[] { invoice }, @"INFO: I00000001 [HL I00000001] - Processing Status Was Successfully Changed.");
			AssertEquals("Should set value.", StorageOffBandProcessingStatus.Codes.QUE, invoice.ET_OffBandProcessingStatus);
		}

		#endregion

		#region TestNotValidToChangeStatus

		public void TestNotValidToChangeStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var today = ZDate.Today;

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, today.AddDays(-6), today);
			Helper.CreateJobCharges(invoice.JobHeader);
			Factory.Save();

			AssertEquals("Default", StorageOffBandProcessingStatus.Codes.NIQ, invoice.ET_OffBandProcessingStatus);
			Applicator.SelectedOffBandProcessingStatus = "ABC";
			ApplyApplicator(new[] { invoice }, @"WARNING: I00000001 [HL I00000001] - Error - ET_OffBandProcessingStatus: Enter a valid selection.");
			AssertEquals("Should undo change.", StorageOffBandProcessingStatus.Codes.NIQ, invoice.ET_OffBandProcessingStatus);
		}

		#endregion

		#region TestNotValidUndoOnlyInvaildChange

		public void TestNotValidUndoOnlyInvaildChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var today = ZDateTime.Today;

			var invoice1 = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, today.AddDays(-13), today.AddDays(-7));
			var invoice2 = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, today.AddDays(-6), today);
			Helper.CreateJobCharges(invoice1.JobHeader);
			Helper.CreateJobCharges(invoice2.JobHeader);
			Factory.Save();

			invoice2.ET_OffBandProcessingStatusInfo.ValueChanged += (s, e) => { invoice2.AddRowError("Validation error!"); };
			AssertEquals("Default", StorageOffBandProcessingStatus.Codes.NIQ, invoice1.ET_OffBandProcessingStatus);
			AssertEquals("Default", StorageOffBandProcessingStatus.Codes.NIQ, invoice2.ET_OffBandProcessingStatus);
			Applicator.SelectedOffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			ApplyApplicator(new[] { invoice1, invoice2 }, @"INFO: I00000001 [HL I00000001] - Processing Status Was Successfully Changed.
WARNING: I00000002 [HL I00000002] - Error - Warehouse Periodic Invoice I00000002: Validation error!");

			AssertEquals("Should set value.", StorageOffBandProcessingStatus.Codes.QUE, invoice1.ET_OffBandProcessingStatus);
			AssertEquals("Should undo change.", StorageOffBandProcessingStatus.Codes.NIQ, invoice2.ET_OffBandProcessingStatus);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChangeOffBandProcessingStatusActionMethodApplicator(Factory);
		}

		protected WhsTestHelperFunctionsInvoice Helper => helper ?? (helper = new WhsTestHelperFunctionsInvoice(Factory));
		WhsTestHelperFunctionsInvoice helper;

		#endregion

		new ChangeOffBandProcessingStatusActionMethodApplicator Applicator => (ChangeOffBandProcessingStatusActionMethodApplicator)base.Applicator;
	}
}

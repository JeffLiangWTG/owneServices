using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class PGADataCorrectionSupporterExtensionTest : TestCaseWithFactory
	{
		class DummyPGATrackerSupporter : CusContainer, IPGADataChangeTrackerSupporter
		{
			public DummyPGATrackerSupporter(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{ }

			public bool IsUnCommittedRow => IsUnCommittedRow_Mock;
			public bool IsUnCommittedRow_Mock;
			public PGADataChangeTracker Tracker
			{
				get
				{
					AssertEquals(false, IsUnCommittedRow);
					return tracker;
				}
				set
				{
					tracker = value;
				}
			}
			PGADataChangeTracker tracker;
		}

		public void TestLoadAllPGARelatedDataIfNeeded()
		{
			var supporter = Factory.New<DummyPGATrackerSupporter>();
			supporter.Tracker = new PGADataChangeTracker(declaration);
			declaration.OnPGARelatedDataLoadProgress += (_, __) => AssertEquals(false, supporter.IsUnCommittedRow);
			CombineAssertions(() =>
			{
				supporter.IsUnCommittedRow_Mock = true;
				supporter.LoadAllPGARelatedDataIfNeeded();
				supporter.IsUnCommittedRow_Mock = false;
				supporter.LoadAllPGARelatedDataIfNeeded();
			});
		}

		public void TestLoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded()
		{
			var pgaList = new List<IPGADataCorrection>();
			var aphis = declaration.InvoiceLines[0].APHISHeaders.AddNew();
			pgaList.Add(aphis);
			aphis.US_TrackingStatus = PGATrackingStatusList.Codes.Added;

			var supporter = Factory.New<DummyPGATrackerSupporter>();
			supporter.Tracker = new PGADataChangeTracker(declaration);
			declaration.MarkPGAStatusToBeDeletedWarningChecker = (_) => false;
			CombineAssertions(() =>
			{
				supporter.IsUnCommittedRow_Mock = true;
				AssertEquals(OGAIndicatorList.Codes.Disclaimed, supporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, pgaList));
				supporter.IsUnCommittedRow_Mock = false;
				AssertEquals(OGAIndicatorList.Codes.Declared, supporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(OGAIndicatorList.Codes.Declared, OGAIndicatorList.Codes.Disclaimed, pgaList));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
		JobDeclaration declaration;
	}
}

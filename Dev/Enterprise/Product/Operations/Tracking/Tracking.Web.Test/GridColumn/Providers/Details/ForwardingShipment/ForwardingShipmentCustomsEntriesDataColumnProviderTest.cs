using Enterprise.Customs.US.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ForwardingShipmentCustomsEntriesDataColumnProvider))]
	sealed class ForwardingShipmentCustomsEntriesDataColumnProviderTest : GridColumnProviderTest
	{
		#region TestCases

		public override void TestColumnKeys()
		{
			base.TestColumnKeys();

			SetupNewProviderWithNoDeclaration();
			base.TestColumnKeys();

			SetupNewProviderWithNonAUDeclaration();
			base.TestColumnKeys();
		}

		public override void TestDefaultColumns()
		{
			base.TestDefaultColumns();

			SetupNewProviderWithNoDeclaration();
			base.TestDefaultColumns();

			SetupNewProviderWithNonAUDeclaration();
			base.TestDefaultColumns();
		}

		public override void TestRequiredColumns()
		{
			base.TestRequiredColumns();

			SetupNewProviderWithNoDeclaration();
			base.TestRequiredColumns();

			SetupNewProviderWithNonAUDeclaration();
			base.TestRequiredColumns();
		}

		public override void TestUniqueColumns()
		{
			base.TestUniqueColumns();

			SetupNewProviderWithNoDeclaration();
			base.TestUniqueColumns();

			SetupNewProviderWithNonAUDeclaration();
			base.TestUniqueColumns();
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Reference #", Customs.Business.CusEntryHeader.Schema.CH_BGMReference) { ColumnKey = WebTracker.Grids.CustomsEntriesData.ReferenceNumber });
			AddDefaultsColumn(new ZTextEditColumn("Entry #", Customs.Business.CusEntryHeader.Schema.EntryNumber) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryNumber });
			if (TestShipment != null && TestShipment.LastDeclaration != null && TestShipment.LastDeclaration is Customs.AU.Declaration.Business.JobDeclaration)
			{
				AddDefaultsColumn(new ZTextEditColumn("Message Status", Customs.AU.Declaration.Business.CusEntryHeader.Schema.CH_Status) { ColumnKey = WebTracker.Grids.CustomsEntriesData.MessageStatus });
				AddDefaultsColumn(new ZTextEditColumn("Entry advice", Customs.AU.Declaration.Business.CusEntryHeader.Schema.ImportEntryAdvice) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryAdvice });
			}
			else
			{
				AddDefaultsColumn(new ZTextEditColumn("Message Status", Customs.Business.CusEntryHeader.Schema.CH_Status) { ColumnKey = WebTracker.Grids.CustomsEntriesData.MessageStatus });
				AddDefaultsColumn(new ZTextEditColumn("Entry Advice", Customs.Business.CusEntryHeader.Schema.CH_EntryStatus) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryAdvice });
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestShipment = Factory.NewWithValidTestData<TrackingShipment>();
			Customs.AU.Declaration.Business.JobDeclaration testDeclaration = Factory.NewWithValidTestData<Customs.AU.Declaration.Business.JobDeclaration>();
			testDeclaration.JE_MessageType = Customs.Common.US.USJobMessageTypeList.Codes.Export;
			testDeclaration.JE_JS = TestShipment.PK;
			SetupNewProvider();
			AssertNotNull(TestProvider.Shipment);
			AssertNotNull(TestProvider.Shipment.LastDeclaration);
			Assert(TestProvider.Shipment.LastDeclaration is Customs.AU.Declaration.Business.JobDeclaration);
		}

		void SetupNewProviderWithNoDeclaration()
		{
			TestShipment = Factory.NewWithValidTestData<TrackingShipment>();
			SetupNewProvider();
			AssertNotNull(TestProvider.Shipment);
			AssertNull(TestProvider.Shipment.LastDeclaration);
		}

		void SetupNewProviderWithNonAUDeclaration()
		{
			TestShipment = Factory.NewWithValidTestData<TrackingShipment>();
			JobDeclaration testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = Customs.Common.US.USJobMessageTypeList.Codes.Export;
			testDeclaration.JE_JS = TestShipment.PK;
			SetupNewProvider();
			AssertNotNull(TestProvider.Shipment);
			AssertNotNull(TestProvider.Shipment.LastDeclaration);
			Assert(!(TestProvider.Shipment.LastDeclaration is Customs.AU.Declaration.Business.JobDeclaration));
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		new ForwardingShipmentCustomsEntriesDataColumnProvider TestProvider
		{
			get { return (ForwardingShipmentCustomsEntriesDataColumnProvider)base.TestProvider; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ForwardingShipmentCustomsEntriesDataColumnProvider(TestShipment);
		}

		TrackingShipment TestShipment
		{
			get { return testShipment; }
			set { testShipment = value; }
		}
		TrackingShipment testShipment;
	}
}

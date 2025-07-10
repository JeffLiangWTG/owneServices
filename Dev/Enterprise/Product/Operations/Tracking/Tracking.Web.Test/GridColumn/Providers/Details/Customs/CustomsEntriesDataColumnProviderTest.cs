using Enterprise.Customs.US.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CustomsEntriesDataColumnProvider))]
	sealed class CustomsEntriesDataColumnProviderTest : GridColumnProviderTest
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
			if (TestProvider.Declaration != null && TestProvider.Declaration.Declaration is Customs.AU.Declaration.Business.JobDeclaration)
			{
				AddDefaultsColumn(new ZTextEditColumn("Message Status", Customs.AU.Declaration.Business.CusEntryHeader.Schema.CH_Status) { ColumnKey = WebTracker.Grids.CustomsEntriesData.MessageStatus });
				AddDefaultsColumn(new ZTextEditColumn("Entry Advice", Customs.AU.Declaration.Business.CusEntryHeader.Schema.ImportEntryAdvice) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryAdvice });
			}
			else
			{
				AddDefaultsColumn(new ZTextEditColumn("Message Status", Customs.Business.CusEntryHeader.Schema.CH_Status) { ColumnKey = WebTracker.Grids.CustomsEntriesData.MessageStatus });
				AddDefaultsColumn(new ZTextEditColumn("Entry Advice", Customs.Business.CusEntryHeader.Schema.CH_EntryStatus) { ColumnKey = WebTracker.Grids.CustomsEntriesData.EntryAdvice });
			}
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDeclaration = new TrackingDeclaration(Factory.NewWithValidTestData<Customs.AU.Declaration.Business.JobDeclaration>());
			SetupNewProvider();
			AssertNotNull(TestProvider.Declaration);
			AssertNotNull(TestProvider.Declaration.Declaration);
			Assert(TestProvider.Declaration.Declaration is Customs.AU.Declaration.Business.JobDeclaration);
		}

		void SetupNewProviderWithNoDeclaration()
		{
			TestDeclaration = null;
			SetupNewProvider();
			AssertNull(TestProvider.Declaration);
		}

		void SetupNewProviderWithNonAUDeclaration()
		{
			TestDeclaration = new TrackingDeclaration(Factory.NewWithValidTestData<JobDeclaration>());
			SetupNewProvider();
			AssertNotNull(TestProvider.Declaration);
			AssertNotNull(TestProvider.Declaration.Declaration);
			Assert(!(TestProvider.Declaration.Declaration is Customs.AU.Declaration.Business.JobDeclaration));
		}

		new CustomsEntriesDataColumnProvider TestProvider
		{
			get { return (CustomsEntriesDataColumnProvider)base.TestProvider; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CustomsEntriesDataColumnProvider(TestDeclaration);
		}

		TrackingDeclaration TestDeclaration
		{
			get { return testDeclaration; }
			set { testDeclaration = value; }
		}
		TrackingDeclaration testDeclaration;
	}
}

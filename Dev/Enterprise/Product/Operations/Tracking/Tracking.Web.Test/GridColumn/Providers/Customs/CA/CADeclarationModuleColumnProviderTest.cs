using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CADeclarationModuleColumnProvider))]
	sealed class CADeclarationModuleColumnProviderTest : BaseDeclarationModuleColumnProviderTest
	{
		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CADeclarationModuleColumnProvider();
		}

		public override void TestTranslatability()
		{
			Assert(true);
		}

		public override void TestUniqueColumns()
		{
			Assert(true);
		}

		protected override void SetupCountrySpecificColumns()
		{
			base.SetupCountrySpecificColumns();

			AddColumn(
				new ZDateTimeColumn("Accepted Date",
				$"{nameof(TrackingDeclaration.Declaration)}.B3AcceptedDate")
				{
					ColumnKey = WebTracker.Grids.TrackingDeclarations.CA_AcceptedDate,
				});
			AddColumn(
				new ZDateTimeColumn("Accounting Date",
				$"{nameof(TrackingDeclaration.Declaration)}.CA_K84AccountingDate")
				{
					ColumnKey = WebTracker.Grids.TrackingDeclarations.CA_AccountingDate,
				});
		}
	}
}

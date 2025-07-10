using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(NZDeclarationModuleColumnProvider))]
	sealed class NZDeclarationModuleColumnProviderTest : BaseDeclarationModuleColumnProviderTest
	{
		protected override GridColumnProvider GetNewTestProvider()
		{
			return new NZDeclarationModuleColumnProvider();
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
				new ZTextEditColumn("TSW Status",
				$"{nameof(TrackingDeclaration.Declaration)}.{nameof(Integration.Customs.NZ.IJobDeclaration.JE_TSWCombinedStatusDesc)}")
				{
					ColumnKey = WebTracker.Grids.TrackingDeclarations.TSWStatus,
				});
		}
	}
}

using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class USContainerAdditionalNumbersPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestIAddColumnsToGridMember()
		{
			var container = Factory.New<ForwardingContainer>();
			using (var plugIn = new USContainerAdditionalNumbersPlugIn(container))
			{
				using (var grid = new ZGrid())
				{
					((IAddColumnsToGrid)plugIn).AddColumnsToContainerGrid(grid);
					AssertEquals("2 columns", 2, grid.ColumnStyles.Count);
					AssertEquals("1st should be ZTextBoxColumnStyleInfo", nameof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[0].GetType().Name);
					AssertEquals("2nd should be ZTextBoxColumnStyleInfo", nameof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[1].GetType().Name);
				}
			}
		}

		protected override ZPlugIn GetPlugInToTest() => new USContainerAdditionalNumbersPlugIn(container);

		ForwardingContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<ForwardingContainer>();
		}
	}
}

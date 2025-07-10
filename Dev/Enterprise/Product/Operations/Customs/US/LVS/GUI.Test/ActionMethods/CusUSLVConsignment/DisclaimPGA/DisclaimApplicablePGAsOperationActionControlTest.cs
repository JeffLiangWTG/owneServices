using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	public class DisclaimApplicablePGAsOperationActionControlTest : ZControlBaseTestCase<DisclaimApplicablePGAsUserControl>
	{
		public void TestGridColumns()
		{
			using (var form = new ZForm())
			{
				var control = new DisclaimApplicablePGAsUserControl();
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var grid = (ZGrid)control.Controls.Find("gridPGARequirements", true)[0];
				AssertNotNull(grid);

				var agencyColumn = (IZColumnStyleInfo)grid.ColumnStyles[0];
				AssertNotNull(agencyColumn);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), agencyColumn.GetType());
				AssertEquals("AgencyCodeWithDescription", ((ZTextBoxColumnStyleInfo)agencyColumn).ColumnName);

				var disclaimReasonColumn = (IZColumnStyleInfo)grid.ColumnStyles[1];
				AssertNotNull(disclaimReasonColumn);
				AssertEquals(typeof(ZDropEditColumnStyleInfo), disclaimReasonColumn.GetType());
				AssertEquals("DisclaimReason", ((ZDropEditColumnStyleInfo)disclaimReasonColumn).ColumnName);
			}
		}

		protected override DummyBusinessObject GetDummyForBinding()
		{
			return Factory.New<DummyDisclaimApplicablePGAsApplicator>();
		}

		protected override bool UsesControlDataBindings => false;

		protected override bool RequiresTypeDescriptor => false;

		class DummyDisclaimApplicablePGAsApplicator : DummyBusinessObject
		{
			public DummyDisclaimApplicablePGAsApplicator(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public CusUSLVItemPGADisclaimOptionCollection DisclaimOptions => new CusUSLVItemPGADisclaimOptionCollection(new BusinessObjectFactory());
		}
	}
}

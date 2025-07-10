using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	sealed class USAMSConsolManifestUserControlTest : TestCaseWithFactory
	{
		public void TestSelectAndShowBill()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			using (var form = new ZForm(header))
			{
				var userControl = new USAMSConsolManifestUserControl(header);
				form.Controls.Add(userControl);
				form.Show();
				Application.DoEvents();
				var billsUserControl = GetControl<USAMSConsolManifestUserControl, USAMSBillsUserControl>(userControl, "BillsDetailsUserControl");
				var grid = GetControl<USAMSBillsUserControl, ZGrid>(billsUserControl, "BillsGrid");
				AssertEquals("hidden to begin with", false, grid.Visible);
				userControl.SelectAndShowBill(bill1.PK);
				AssertEquals("should be shown now", true, grid.Visible);
				AssertEquals("should have selected bill1", bill1, grid.ListManager.GetCurrent());
				userControl.SelectAndShowBill(bill2.PK);
				AssertEquals("should have selected bill2", bill2, grid.ListManager.GetCurrent());
			}
		}

		T GetControl<P, T>(P parent, string name)
			where P : Control
			where T : Control
		{
			return (T)typeof(P).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parent);
		}
	}
}

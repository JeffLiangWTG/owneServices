using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class EquipmentDetailsUserControlTestCase : TestCaseWithFactory
	{
		public void TestEquimentGridColumns()
		{
			var trip = Factory.New<Trip>();
			using (var form = new ZForm(trip))
			{
				using (var control = new AllEquipmentUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var equipmentGrid = (ZGrid)control.Controls.Find("EquipmentGrid", true).FirstOrDefault();
					AssertNotNull("Equipment Grid", equipmentGrid);
					if (equipmentGrid != null)
					{
						AssertNotNull("Equipment Column", equipmentGrid.Columns["BJ_RQ_Equipment"]);
						AssertNotNull("Equipment Type Column", equipmentGrid.Columns["BJ_RC_RoadContainerType"]);
						AssertNotNull("Reg. Country Column", equipmentGrid.Columns["BJ_RN_NKRegistrationCountry"]);
						AssertNotNull("Reg. State Column", equipmentGrid.Columns["BJ_RW_NKRegistrationState"]);
					}
				}
			}
		}
	}
}

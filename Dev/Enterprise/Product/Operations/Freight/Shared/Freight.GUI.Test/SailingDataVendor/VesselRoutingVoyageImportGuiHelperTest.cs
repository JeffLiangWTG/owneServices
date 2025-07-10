using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.SailingDataVendor.GUI.Testing
{
	sealed class VesselRoutingVoyageImportGuiHelperTest : TestCaseWithFactory
	{
		public void TestQueryUser_ForBaseQueryUser()
		{
			QueryUserYesNoEventArgs queryUser = new QueryUserYesNoEventArgs("Message", false);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Helper.QueryUser(queryUser);
			AssertEquals("Should query the user for a yes/no response", true, queryUser.Response);
		}

		public void TestQueryUser_ForVesselSelectForm()
		{
			QueryUserSelectVesselFromLloydsNumber vesselSelect = new QueryUserSelectVesselFromLloydsNumber(Factory, "VesselName", "Lloyds");
			Helper.QueryUser(vesselSelect);
			AssertEquals("Should query the user to select a vessel, showing the form with the right business entity", vesselSelect, Helper.LastFormShown.LastDataSourceForTest);
		}

		#region Test Classes

		class TestVesselRoutingVoyageImportGuiHelper : VesselRoutingVoyageImportGuiHelper
		{
			public VesselSelectForm LastFormShown;

			protected override DialogResult FormShowDialogWithoutDispose(VesselSelectForm form)
			{
				LastFormShown = form;
				return DialogResult.OK;
			}
		}

		#endregion

		#region Implementation

		readonly TestVesselRoutingVoyageImportGuiHelper Helper = new TestVesselRoutingVoyageImportGuiHelper();

		#endregion
	}
}

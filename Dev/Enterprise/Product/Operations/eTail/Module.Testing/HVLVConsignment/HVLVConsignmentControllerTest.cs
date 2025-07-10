using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVConsignmentController))]
	class HVLVConsignmentControllerTest : ZControllerBasherTest
	{
		public void TestMakeUrlsOnlyOpenableForCurrentCompany()
		{
			AssertEquals(true, Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(HVLVConsignment), Controller.TypeOfTopLevelBusinessObject);
		}

		public void TestGetForm()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			using (var form = Controller.ShowFormForNewEntity(consignment))
			{
				AssertType<HVLVConsignmentForm>(form);
			}
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.HVLVConsignment, Controller.ModuleID);
		}

		public void TestSecurityCheckPoints()
		{
			CombineAssertions(() =>
			{
				AssertEquals("New:", Env.Security.HVLVBookingHeader, Controller.CheckPointForNewExposedForTest);
				AssertEquals("Delete:", Env.Security.HVLVBookingHeader, Controller.CheckPointForDeleteExposedForTest);
				AssertEquals("Edit:", Env.Security.HVLVBookingHeaderEdit, Controller.CheckPointForEditExposedForTest);
				AssertEquals("View:", Env.Security.HVLVBookingHeaderView, Controller.CheckPointForViewExposedForTest);
			});
		}

		public void TestFilterStrip()
		{
			IBusinessObjectCollection consignments = new ActiveBusinessObjectCollection<HVLVConsignment>(Factory);
			HVLVConsignmentFilterBusinessObject filterBO = new HVLVConsignmentFilterBusinessObject();
			using (ZForm form = new ZForm())
			{
				HVLVConsignmentFilterControl filterControl = new HVLVConsignmentFilterControl(consignments, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				filterControl.AddNewFilterStrip();
				AssertEquals(typeof(HVLVConsignmentModuleStrip), filterControl.LastFilterStripType);
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.HVLVConsignment;
	}
}

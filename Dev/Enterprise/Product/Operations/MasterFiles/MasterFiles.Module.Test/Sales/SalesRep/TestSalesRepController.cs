using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesRepController))]
	sealed class TestSalesRepController : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.SalesRep;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var salesRep = Factory.New<GlbStaff>();
			salesRep.GS_IsSalesRep = true;
			Factory.Save();
			return salesRep;
		}

		#region Test Show Forms

		public override void TestNewForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.SalesRep);
			using (var form = controller.ShowNewForm())
			{
				AssertType(typeof(GlbStaffForm), form);
				AssertEquals(ControllerIDs.GlbStaff, form.ControllerID);
			}
		}

		public override void TestViewForm()
		{
			AssertShowForm((controller, salesRep) => controller.ShowViewForm(salesRep));
		}

		public override void TestEditForm()
		{
			AssertShowForm((controller, salesRep) => controller.ShowEditForm(salesRep));
		}

		public override void TestDeleteForm()
		{
			AssertShowForm((controller, salesRep) => controller.ShowDeleteForm(salesRep));
		}

		void AssertShowForm(Func<ZController, BusinessObject, IZForm> showFormFunc)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.SalesRep);
			using (var form = showFormFunc(controller, GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertType(typeof(GlbStaffForm), form);
				AssertEquals(ControllerIDs.GlbStaff, form.ControllerID);
			}
		}

		#endregion

		public void TestWorkflowSecurityChecks()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.SalesRep);

			using (var form = (ZForm)controller.ShowNewForm())
			{
				var workflowTabPage = form.FindSingle<ZWorkflowTabPage>();
				AssertNoExceptionThrown(() => ((ZTabControl)workflowTabPage.Parent).SelectTab(workflowTabPage));
			}
		}
	}
}

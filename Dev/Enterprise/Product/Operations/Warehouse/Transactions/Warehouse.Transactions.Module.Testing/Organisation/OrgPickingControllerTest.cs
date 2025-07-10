using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrgPickingController))]
	class OrgPickingControllerTest : ZControllerBasherTest
	{
		#region Form

		public override void TestDeleteForm()
		{
			AssertExceptionThrown<NotSupportedException>(base.TestDeleteForm);
		}

		public override void TestEditForm()
		{
			AssertExceptionThrown<NotSupportedException>(base.TestEditForm);
		}

		public override void TestNewForm()
		{
			AssertExceptionThrown<NotSupportedException>(base.TestNewForm);
		}

		public override void TestViewForm()
		{
			AssertExceptionThrown<NotSupportedException>(base.TestViewForm);
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals("Module ID should be correct.", ModuleIDs.Organisation, new OrgPickingController().ModuleID);
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals("Type of Top Level BizO should be correct.", typeof(OrgHeader), new OrgPickingController().TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region TestPlugIn

		public void TestPlugIn()
		{
			using (var form = new ZForm())
			{
				form.PlugIns.Add(GetControllerID());
				var plugIn = form.PlugIns.GetPlugIn(GetControllerID());
				AssertEquals("PlugIn should be correct.", typeof(OrganisationFormWhsPickingPlugIn), plugIn.GetType());
			}
		}

		#endregion

		#region TestPluginTabPageCaption

		public void TestPluginTabPageCaption()
		{
			AssertEquals("PlugIn Caption should be correct.", "Picking", new OrgPickingController().PluginTabPageCaption.Caption);
		}

		#endregion

		#region TestSecurityCheckPoints

		public void TestSecurityCheckPoints()
		{
			AssertEquals("Security Check Point for Delete should be correct.", Env.Security.OrganisationDelete, new OrgPickingController().GetCheckPointForDelete(null));
			AssertEquals("Security Check Point for Edit should be correct.", Env.Security.OrganisationModify, new OrgPickingController().GetCheckPointForEdit(null));
			AssertEquals("Security Check Point for New should be correct.", Env.Security.OrganisationNew, new OrgPickingController().GetCheckPointForNew(null));
			AssertEquals("Security Check Point for View should be correct.", Env.Security.OrganisationView, new OrgPickingController().GetCheckPointForView(null));
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsConfigOrgPicking;
		}

		#endregion
	}
}

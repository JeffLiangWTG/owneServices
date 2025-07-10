using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CommissionAgreementController))]
	sealed class CommissionAgreementControllerTest : ZControllerBasherTest
	{
		#region Implementation

		protected override Type GetBusinessObjectType()
		{
			return typeof(OrgCommissionAgreement);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrgCommissionAgreement;
		}

		#endregion

		#region GetForm

		public void TestShowNewForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
			using (var form = controller.ShowNewForm())
			{
				AssertType(typeof(OpportunityForm), form);
				AssertEquals(ControllerIDs.Opportunity, form.ControllerID);
			}
		}

		public void TestShowViewForm()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
			using (var form = controller.ShowViewForm(agreement))
			{
				AssertType(typeof(OpportunityForm), form);
				AssertEquals(ControllerIDs.Opportunity, form.ControllerID);
			}
		}

		public void TestShowEditForm()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
			using (var form = controller.ShowEditForm(agreement))
			{
				AssertType(typeof(OpportunityForm), form);
				AssertEquals(ControllerIDs.Opportunity, form.ControllerID);
			}
		}

		public void TestShowDeleteForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
			AssertExceptionThrown(typeof(ControllerShowDeleteFormNotSupportedException), () =>
			{
				controller.ShowDeleteForm(null);
			});
		}

		public void TestGetForm()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
			using (var form = ((ZControllerInternals)controller).GetForm(agreement))
			{
				AssertType(typeof(OpportunityForm), form);
				AssertEquals(ControllerIDs.Opportunity, form.ControllerID);
			}
		}

		#endregion

		#region Security Checkpoints

		public void TestGetCheckPointForNew()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
			AssertEquals(Env.Security.None, controller.GetCheckPointForNew(null));
		}

		public void TestGetCheckPointForView()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
			var expectedCheckPointForView = ZControllerFactory.Create(ControllerIDs.Opportunity).GetCheckPointForView(null);
			AssertEquals("OrgCommissionAgreements are viewed on the Opportunity", expectedCheckPointForView, controller.GetCheckPointForView(null));
		}

		public void TestGetCheckPointForEdit()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
			var expectedCheckPointForEdit = ZControllerFactory.Create(ControllerIDs.Opportunity).GetCheckPointForEdit(null);
			AssertEquals("OrgCommissionAgreements are viewed on the Opportunity", expectedCheckPointForEdit, controller.GetCheckPointForEdit(null));
		}

		public void TestGetCheckPointForDelete()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
			AssertEquals(Env.Security.None, controller.GetCheckPointForDelete(null));
		}

		public void TestWorkflowSecurityChecks()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);

			using (var form = (ZForm)controller.ShowEditForm(agreement))
			{
				var workflowTabPage = form.FindSingle<ZWorkflowTabPage>();
				AssertNoExceptionThrown(() => ((ZTabControl)workflowTabPage.Parent).SelectTab(workflowTabPage));
			}
		}

		#endregion

		#region ICommissionAgreementController Members

		public void TestShowEditForm_RecipientRate()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var recipient = agreement.Recipients.AddNew();
			recipient.FillWithValidTestData();
			var recipientRate = recipient.Rates.AddNew();
			Factory.Save();

			var controller = (ICommissionAgreementController)ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement);
			using (var form = controller.ShowEditFormForRate(recipientRate))
			{
				AssertType(typeof(OpportunityForm), form);
			}
		}

		#endregion
	}
}

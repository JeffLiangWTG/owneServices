using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class CommissionAgreementController : OrgOpportunityController, ICommissionAgreementController
	{
		#region Standard Controller Overrides

		public override ControllerID ID
		{
			get { return ControllerIDs.OrgCommissionAgreement; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.OrgCommissionAgreement; }
		}

		#endregion

		#region GetForm

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var commissionAgreement = businessEntity as OrgCommissionAgreement;
			var opportunity = commissionAgreement != null ?
				commissionAgreement.Opportunity :
				businessEntity as OrgOpportunity;

			if (opportunity == null)
			{
				return null;
			}

			var form = (OpportunityForm)base.GetForm(opportunity);
			form.ControllerID = ControllerIDs.Opportunity;
			if (commissionAgreement != null)
			{
				EventHandler onFormShownHandler = null;
				onFormShownHandler = (sender, e) =>
				{
					form.NavigateToCommissionAgreement(commissionAgreement);
					form.Shown -= onFormShownHandler;
				};

				form.Shown += onFormShownHandler;
			}

			return form;
		}

		protected override void SetControllerID(IZForm form, ControllerID proposedControllerID)
		{
			// The form has its ControllerID set to Opportunity in GetForm. We don't want to reset it to OrgCommissionAgreement.
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowDeleteFormNotSupportedException("Does not support Delete functionality");
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return factory.Load<OrgCommissionAgreement>(sourceEntityPK);
		}

		#endregion

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OpportunityManagementView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OpportunityManagementEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region ICommissionAgreementController Members

		public IZForm ShowEditFormForRate(OrgCommissionAgreementRecipientRate agreementRecipientRate)
		{
			var agreement = agreementRecipientRate.CommissionAgreement;
			if (agreement == null)
			{
				return null;
			}

			var form = (OpportunityForm)ShowEditForm(agreement);
			if (form != null)
			{
				form.NavigateToCommissionAgreementRecipientRate(agreementRecipientRate);
			}

			return form;
		}

		#endregion
	}
}

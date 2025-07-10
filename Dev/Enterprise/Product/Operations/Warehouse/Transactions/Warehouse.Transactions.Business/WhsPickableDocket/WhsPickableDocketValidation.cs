using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsPickableDocketValidation : WhsDocketValidation
	{
		protected WhsPickableDocketValidation(WhsPickableDocket parent)
			: base(parent)
		{
		}

		protected new WhsPickableDocket Parent => (WhsPickableDocket)base.Parent;

		// persistent

		#region CheckWD_WP

		protected override void CheckWD_WP()
		{
			base.CheckWD_WP();

			var docketStatus = Parent.WD_DocketStatus;
			var finalisedDate = Parent.WD_FinalisedDate;
			if (Parent.WD_WP.IsEmpty)
			{
				if (docketStatus == DocketStatus.Codes.AttachedToPick
					|| docketStatus == DocketStatus.Codes.Picking
					|| finalisedDate.IsValid)
				{
					Parent.WD_WPInfo.AddError(Res.GetString("c59063cd-e0ac-4887-b423-ff17ac772d74", "Finalized Docket or Docket with status set to ATTACHED TO PICK or PICKING must be attached to any pick."));
				}
			}
			else if (docketStatus != DocketStatus.Codes.AttachedToPick
						&& docketStatus != DocketStatus.Codes.Picking
						&& !finalisedDate.IsValid)
			{
				Parent.WD_WPInfo.AddError(Res.GetString("020ada54-ccd4-422a-98f2-cba563a39e39", "Un-finalized Docket without status of ATTACHED TO PICK or PICKING must not be attached to any pick."));
			}
		}

		#endregion

		#region CheckWD_DocketStatus

		protected override void CheckWD_DocketStatus()
		{
			base.CheckWD_DocketStatus();

			var docketStatus = Parent.WD_DocketStatus;
			var finalisedDate = Parent.WD_FinalisedDate;
			if (docketStatus == DocketStatus.Codes.AttachedToPick
				|| docketStatus == DocketStatus.Codes.Picking
				|| finalisedDate.IsValid)
			{
				if (Parent.WD_WP.IsEmpty)
				{
					Parent.WD_DocketStatusInfo.AddError(Res.GetString("7177012C-A1D4-4D34-8784-B66D7AA6E075", "Un-finalized Docket not attached to any pick so the docket status cannot be set to ATTACHED TO PICK or PICKING."));
				}
			}
			else if (!Parent.WD_WP.IsEmpty)
			{
				Parent.WD_DocketStatusInfo.AddError(Res.GetString("29ABA665-5F84-4FA7-A840-D52DBB642036", "Un-finalized Docket has a pick so docket status must set to either ATTACHED TO PICK or PICKING."));
			}
		}

		#endregion

		#region WD_PickOption

		protected override void CheckWD_PickOption()
		{
			base.CheckWD_PickOption();

			ListValidation.ErrorIfInvalidCode(Parent.WD_PickOptionInfo);
			if (!Parent.WD_PickOptionInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.WD_PickOptionInfo);
			}
		}

		#endregion

		#region CheckWD_IsInwardsProcessingJob

		protected override void CheckWD_IsInwardsProcessingJob()
		{
			base.CheckWD_IsInwardsProcessingJob();
			WhsValidationHelper.CheckInwardProcessingJobOnlySetOnVirtualWarehouses(Parent, Parent.WD_IsInwardsProcessingJobInfo);
			CheckTransactionType(Parent.WD_IsInwardsProcessingJobInfo);
		}

		#endregion

		// calculated

		#region CheckConsigneeNameOrPK

		public void ValidateConsigneeNameOrPK()
		{
			ValidateConsigneeNameOrPKCore();
		}

		protected virtual void ValidateConsigneeNameOrPKCore()
		{
			ValidateCalculatedProperty(Parent.ConsigneeNameOrPKInfo);
		}

		protected virtual void CheckConsigneeNameOrPK()
		{
			JobDocAddress address = Parent.ConsigneeDocAddress;

			if (address.E2_AddressOverride)
			{
				address.Validation.ValidateE2_CompanyName();
				Parent.ConsigneeNameOrPKInfo.AddAllNotificationsFrom(address.E2_CompanyNameInfo);
			}
			else
			{
				address.Validation.ValidateOrganisationPK();
				Parent.ConsigneeNameOrPKInfo.AddAllNotificationsFrom(address.OrganisationPKInfo);
			}
		}

		#endregion

		#region CheckTransportCoNameOrPK

		public void ValidateTransportCoNameOrPK() // used by Grid Validation
		{
			ValidateTransportCoNameOrPKCore();
		}

		protected virtual void ValidateTransportCoNameOrPKCore()
		{
			ValidateCalculatedProperty(Parent.TransportCoNameOrPKInfo);
		}

		protected virtual void CheckTransportCoNameOrPK()
		{
			Parent.ValidateTransportCoNameOrPKInfo();
		}

		#endregion
	}
}

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentValidation : WhsDocketValidation
	{
		public WhsAdjustmentValidation(WhsAdjustment parent)
			: base(parent)
		{
		}

		#region CheckWD_DocketType

		protected override void CheckWD_DocketType()
		{
			base.CheckWD_DocketType();
			if (Parent.WD_DocketType != DocketType.Codes.Adjustment)
			{
				Parent.WD_DocketTypeInfo.AddError(Res.GetString("da81c315-67db-44a3-946b-c086b9fde5ad", "The Docket Type is not set to Adjustment."));
			}
		}

		#endregion

		#region CheckWD_DocketStatus

		protected override void CheckWD_DocketStatus()
		{
			base.CheckWD_DocketStatus();

			if ((Parent.WD_DocketStatus != DocketStatus.Codes.New) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Entered) &&
				(Parent.WD_DocketStatus != DocketStatus.Codes.Finalised))
			{
				Parent.WD_DocketStatusInfo.AddError(Res.GetString("5986bf27-9843-4586-8d40-d60c51a30540", "The Docket Status is invalid for this docket type."));
			}

			CheckUNDG();
		}

		#endregion

		#region CheckUNDG

		bool IsUNDGValidationRequired => Parent.IsFinalising;

		void CheckUNDG()
		{
			var docketStatusInfo = Parent.WD_DocketStatusInfo;
			if (!docketStatusInfo.HasErrors() && IsUNDGValidationRequired)
			{
				var adjustment = Adjustment;
				var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(adjustment.Factory, adjustment.Warehouse, adjustment.PK, adjustment.Lines.ToArray()).OverLimitMessage;
				if (!validationMessage.IsEmpty)
				{
					docketStatusInfo.AddError(BuildUNDGLimitExceededError(validationMessage));
				}
			}
		}

		#endregion

		#region CheckWD_OH_ClientCreditCheck

		protected override void CheckWD_OH_Client_CreditCheck()
		{
		}

		#endregion

		#region ValidateOwnershipAdjustedClientPK

		public void ValidateOwnershipAdjustedClientPK()
		{
			ValidateCalculatedProperty(Adjustment.OwnershipAdjustedClientPKInfo);
		}

		protected void CheckOwnershipAdjustedClientPK()
		{
			var adjustment = Adjustment;
			if (!adjustment.IsFinalisedOrCancelled && adjustment.IsNewOwnershipAdjustmentParent)
			{
				MandatoryValidation.CheckEntered(adjustment.OwnershipAdjustedClientPKInfo);
				TypeValidation.CheckValidGuid(adjustment.OwnershipAdjustedClientPKInfo);
				CheckOldAndNewClientAreDifferent(adjustment.Client, adjustment.OwnershipAdjustedClient, adjustment.OwnershipAdjustedClientPKInfo);
			}
		}

		void CheckOldAndNewClientAreDifferent(OrgHeader oldClient, OrgHeader newClient, ZPropertyInfo info)
		{
			if (!info.HasErrors() && oldClient != null && newClient != null && oldClient.PK == newClient.PK)
			{
				info.AddError(ResString.GetMultilingualString("4e85a285-215e-4b85-bb82-e99d3ef34dce", "Ownership Adjusted Client can't be same as Old Client."));
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOwnershipAdjustedClientPK();
		}

		#endregion

		#region Implementation

		protected override ZString TypeInMsg
		{
			get { return Res.GetString("eed6907c-d5af-4928-9153-7f9a4887bb9b", "Adjustment"); }
		}

		WhsAdjustment Adjustment
		{
			get { return (WhsAdjustment)Parent; }
		}

		#endregion
	}
}

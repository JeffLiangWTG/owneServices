using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsTransferValidation : WhsDocketValidation
	{
		public WhsTransferValidation(WhsTransfer parent)
			: base(parent)
		{
		}

		#region CheckWD_DocketType

		protected override void CheckWD_DocketType()
		{
			base.CheckWD_DocketType();
			if (Parent.WD_DocketType != CodeLists.DocketType.Codes.Transfer)
			{
				Parent.WD_DocketTypeInfo.AddError(Res.GetString("cfbf5dd6-7e71-47a1-91b9-ad6c36586f76", "The Docket Type is not set to Transfer."));
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
				Parent.WD_DocketStatusInfo.AddError(Res.GetString("71028744-8ec5-4bf8-97ea-e3909e2f52e0", "The Docket Status is invalid for this docket type."));
			}

			ValidateTransferUNDGs();
		}

		#endregion

		#region ValidateTransferUNDGs

		bool IsDocketUNDGValidationRequired(WhsTransfer transfer) =>
			WhsTransferValidationHelper.IsLineUNDGValidationRequired(transfer)
			&& transfer.IsFinalising;

		void ValidateTransferUNDGs()
		{
			var transfer = (WhsTransfer)Parent;
			var docketStatusInfo = transfer.WD_DocketStatusInfo;
			if (!docketStatusInfo.HasErrors() && IsDocketUNDGValidationRequired(transfer))
			{
				var validationMessage = WhsTransferValidationHelper.FindUNDGsOverLimit(transfer, transfer.Lines.ToArray<WhsTransferLine>());
				if (!validationMessage.IsEmpty)
				{
					transfer.WD_DocketStatusInfo.AddError(validationMessage);
				}
			}
		}

		#endregion

		#region CheckWD_OH_ClientCreditCheck

		protected override void CheckWD_OH_Client_CreditCheck()
		{
			// don't call base
		}

		#endregion

		#region CheckWD_WP_PickBeingReplenished

		protected override void CheckWD_WP_PickBeingReplenished()
		{
			base.CheckWD_WP_PickBeingReplenished();
			ListValidation.ErrorIfInvalidPK(Parent.WD_WP_PickBeingReplenishedInfo);
		}

		#endregion

		#region Implementation

		protected override bool ShouldCheckTransactionType => false;

		protected override ZString TypeInMsg => Res.GetString("ad3db029-e92a-4e87-a0fa-2f8d67e036da", "Transfer");

		#endregion
	}
}

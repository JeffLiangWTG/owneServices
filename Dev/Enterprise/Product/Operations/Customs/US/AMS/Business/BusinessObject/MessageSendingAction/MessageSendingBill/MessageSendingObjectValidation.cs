using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class MessageSendingObjectValidation : AutoMessageSendingObjectValidation
	{
		public MessageSendingObjectValidation(AutoMessageSendingObject parent)
			: base(parent) { }

		#region Implementation

		public new MessageSendingObject Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (MessageSendingObject)base.Parent; }
		}

		protected override void CheckMB_Send()
		{
			Parent.ClearRowNotifications();
			base.CheckMB_Send();

			if (!Parent.MoveDetail.HasBillOnFile
				&& Parent.ActionCode == ActionCode.AmendingAdd
				&& Parent.MB_BillActionCode == AMSBillSendingActionCodeList.Codes.AddBill)
			{
				Parent.MB_SendInfo.AddWarning(ValidationConstants.MessageSending.NotUseAmendmentManifestIsNotOnFile);
			}

			ValidateMB_BillActionCode();
			ValidateMB_AmendmentCode();
			ValidateMB_CustomsStatus();
			ValidateShouldSendManifestAmendmentMessage();
			ValidateMB_Date();
		}

		protected override void CheckMB_BillActionCode()
		{
			base.CheckMB_BillActionCode();
			if (Parent.MB_Send)
			{
				if (Parent.ActionCode == ActionCode.AmendingAdd &&
					Parent.IsBillAlreadyOnFile &&
					Parent.MB_BillActionCode == AMSBillSendingActionCodeList.Codes.AddBill)
				{
					Parent.MB_BillActionCodeInfo.AddMessageError(ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInstead);
				}
				else if (Parent.ActionCode == ActionCode.AmendingAdd &&
								Parent.MB_BillActionCode == AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity)
				{
					Parent.MB_BillActionCodeInfo.AddWarning(ValidationConstants.MessageSending.ReplaceMessageWillOnlyReplaceManifestQuantity);
				}
				else if (!Parent.MB_BillActionCodeInfo.ReadOnly)
				{
					if (ActionCodeTool.IsInBondArrivalExportationTOL(Parent.ActionCode))
					{
						MandatoryValidation.CheckEntered(Parent.MB_BillActionCodeInfo);
						ListValidation.ErrorIfInvalidCode(Parent.MB_BillActionCodeInfo);
					}
					else
					{
						ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.MB_BillActionCodeInfo);
					}
				}
			}
		}

		protected override void CheckMB_PortOfUnladingOverride()
		{
			base.CheckMB_PortOfUnladingOverride();
			ListValidation.WarnIfInvalidCode(Parent.MB_PortOfUnladingOverrideInfo);
		}

		protected override void CheckMB_CustomsStatus()
		{
			base.CheckMB_CustomsStatus();
			if (Parent.MB_Send && Parent.IsCreatingActionAndBillAlreadyOnFile)
			{
				Parent.MB_CustomsStatusInfo.AddMessageError(ValidationConstants.MessageSending.BillIsAlreadyOnFileUseUpdateInsteadForCreatingAction);
			}
		}

		protected override void CheckMB_AmendmentCode()
		{
			base.CheckMB_AmendmentCode();
			if (!Parent.MB_AmendmentCodeInfo.ReadOnly && Parent.MB_Send)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.MB_AmendmentCodeInfo);
			}
			ValidateMB_BillActionCode();
		}

		protected override void CheckMB_IssuerCode()
		{
			base.CheckMB_IssuerCode();

			if (Parent.MB_BillActionCode == InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByBillOfLading ||
				Parent.MB_BillActionCode == InBondAndVesselEventMessageCodeList.Codes.ExportInBondByBillOfLading ||
				Parent.MB_BillActionCode == InBondAndVesselEventMessageCodeList.Codes.CancelInBondArrivalByBillOfLading ||
				Parent.MB_BillActionCode == InBondAndVesselEventMessageCodeList.Codes.CancelInBondExportByBillOfLading ||
				Parent.MB_BillActionCode == InBondAndVesselEventMessageCodeList.Codes.CancelPermitsToTransferArrivalByBillOfLading)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.MB_IssuerCodeInfo);
			}
		}

		protected override void CheckMB_Date()
		{
			base.CheckMB_Date();
			if (Parent.ShouldValidateDate != null && Parent.ShouldValidateDate())
			{
				if (IsVesselAction && Parent.MB_Send)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.MB_DateInfo);

					if (Parent.MB_Date.IsInTheFutureDatePartOnly)
					{
						if (Parent.ActionCode == ActionCode.VesselDeparture)
						{
							Parent.MB_DateInfo.AddMessageError(ValidationConstants.MessageSending.ExportDateForVesselDeparture);
						}
						else
						{
							Parent.MB_DateInfo.AddMessageError(ValidationConstants.MessageSending.ArrivalDateForVesselArrivalOrChangeDateEvent);
						}
					}
				}
			}
		}

		bool IsVesselAction
		{
			get { return Parent.ActionCode == ActionCode.VesselArrival || Parent.ActionCode == ActionCode.VesselDeparture || Parent.ActionCode == ActionCode.ChangeEstDateOfArrival; }
		}

		void ValidateShouldSendManifestAmendmentMessage()
		{
			if (Parent.ActionCode == ActionCode.Creating && Parent.ShouldSendManifestAmendmentMessage)
			{
				Parent.AddRowMessageError(ValidationConstants.MessageSending.AmendmentMessageShouldBeFiled);
			}
		}

		#endregion
	}
}

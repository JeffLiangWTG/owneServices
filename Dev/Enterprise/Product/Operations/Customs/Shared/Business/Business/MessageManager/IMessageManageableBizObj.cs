namespace Enterprise.Customs.Business
{
	public interface IMessageManageableBizObj
	{
		IMessageManager GetMessageManagerForAmendmentDetection();

		/// <summary>
		/// Declaration attempts to re-merge before starting amendment detection
		/// </summary>
		ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue();

		bool IsInAStatusAmendmentSendable { get; }
	}

	public interface IBackDoorSavingSupportableBizObj : IMessageManageableBizObj
	{
		bool SupportBackDoorForSavingWhenAmendmentDetected { get; }

		AmendmentWithdrawalReason GetAmendmentWithdrawalReason();
	}

	public enum ContinueWithDetection { Yes, No }
}

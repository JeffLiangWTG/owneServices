using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public sealed class MessageManageableJobDeclaration : BaseJobDeclaration, IBackDoorSavingSupportableBizObj
	{
		public MessageManageableJobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			DPSPartiesForTesting = Array.Empty<ScreeningParty>();
		}

		protected override bool IsDPSFreightMovementRestrictedCore()
		{
			return DPSFreightMovementRestricted;
		}
		public bool DPSFreightMovementRestricted { get; set; }

		protected override ScreeningParty[] GetScreeningPartiesCore()
		{
			return DPSPartiesForTesting;
		}
		public ScreeningParty[] DPSPartiesForTesting { get; set; }

		public AmendmentWithdrawalReason GetAmendmentWithdrawalReason()
		{
			return AmendmentWithdrawalReasonExposed;
		}
		public AmendmentWithdrawalReason AmendmentWithdrawalReasonExposed = new AmendmentWithdrawalReason();

		public IMessageManager GetMessageManagerForAmendmentDetection()
		{
			return new TestHelperMultiMessageManager(this);
		}

		public ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue()
		{
			return ProcessBeforeDetectingAmendmentExposed;
		}
		public ContinueWithDetection ProcessBeforeDetectingAmendmentExposed = ContinueWithDetection.Yes;

		public bool SupportBackDoorForSavingWhenAmendmentDetected
		{
			get { return fSupportBackDoorForSavingWhenAmendmentDetected; }
			set { fSupportBackDoorForSavingWhenAmendmentDetected = value; }
		}
		bool fSupportBackDoorForSavingWhenAmendmentDetected;

		public bool IsInAStatusAmendmentSendable
		{
			get { return fIsInAStatusAmendmentSendable; }
			set { fIsInAStatusAmendmentSendable = value; }
		}
		bool fIsInAStatusAmendmentSendable = true;
	}
}

using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class MessageManageableBusinessObject : DummyEnterpriseBusinessObject, IBackDoorSavingSupportableBizObj
	{
		public MessageManageableBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public DummyDependentBusinessObjectCollection Dependents
		{
			get
			{
				if (fDependents == null)
				{
					fDependents = new DummyDependentBusinessObjectCollection(this, Factory);
					fDependents.Load();
					RegisterEditableChildObject(fDependents);
				}
				return fDependents;
			}
		}
		DummyDependentBusinessObjectCollection fDependents;

		#region IMessageManageableBizObj Members

		public ContinueWithDetection ProcessBeforeDetectingAmendmentExposed = ContinueWithDetection.Yes;
		public ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue()
		{
			return ProcessBeforeDetectingAmendmentExposed;
		}

		public bool IsInAStatusAmendmentSendable
		{
			get { return fIsInAStatusAmendmentSendable; }
			set { fIsInAStatusAmendmentSendable = value; }
		}
		bool fIsInAStatusAmendmentSendable = true;

		public bool SupportBackDoorForSavingWhenAmendmentDetected
		{
			get { return fSupportBackDoorForSavingWhenAmendmentDetected; }
			set { fSupportBackDoorForSavingWhenAmendmentDetected = value; }
		}
		bool fSupportBackDoorForSavingWhenAmendmentDetected;

		public DeferredAmendmentSavingOptions DeferredAmendmentSavingOptionsExposed = new DeferredAmendmentSavingOptions();
		public DeferredAmendmentSavingOptions GetDeferredAmendmentSavingOptions()
		{
			return DeferredAmendmentSavingOptionsExposed;
		}

		public AmendmentWithdrawalReason AmendmentWithdrawalReasonExposed = new AmendmentWithdrawalReason();
		public AmendmentWithdrawalReason GetAmendmentWithdrawalReason()
		{
			return AmendmentWithdrawalReasonExposed;
		}

		public delegate void ChangesAreSavedWithoutSendingEventHandler(RequiredMessagesInformation information);
		public ChangesAreSavedWithoutSendingEventHandler OnChangesSavedWithoutSending;

		public void ProcessWhenChangesAreSavedWithoutSending(RequiredMessagesInformation information)
		{
			if (OnChangesSavedWithoutSending != null)
			{
				OnChangesSavedWithoutSending(information);
			}
		}

		public IMessageManager GetMessageManagerForAmendmentDetection()
		{
			return new TestHelperMultiMessageManager(this);
		}

		public bool RunPreSaveValidationWhenSendingAMessage
		{
			get { return fRunPreSaveValidationWhenSendingAMessage; }
			set { fRunPreSaveValidationWhenSendingAMessage = value; }
		}
		bool fRunPreSaveValidationWhenSendingAMessage;

		public BusinessObject BizObjToRunValidationAgainstBeforeSending
		{
			get
			{
				if (fBizObjToRunValidationAgainstBeforeSending == null)
				{
					fBizObjToRunValidationAgainstBeforeSending = this;
				}
				return fBizObjToRunValidationAgainstBeforeSending;
			}
			set { fBizObjToRunValidationAgainstBeforeSending = value; }
		}
		BusinessObject fBizObjToRunValidationAgainstBeforeSending;

		#endregion
	}
}

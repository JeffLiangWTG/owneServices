
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ACEReconMessageSendingAction : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ACEReconMessageSendingAction(ReconDeclarationIReconciliation reconDeclaration, UpdateActionCode actionCode)
			: base(reconDeclaration.Factory)
		{
			this.ReconDeclaration = reconDeclaration;
			this.ReconWrappedJobDeclaration = reconDeclaration.ReconDeclaration.ReconWrappedJobDeclaration;
			this.ActionCode = actionCode;

			if (IsPaymentFinalizedRequired)
			{
				this.PaymentFinalized = SetPaymentFinalized();
				this.ClearHasChanges();
			}
		}
		internal readonly ReconDeclarationIReconciliation ReconDeclaration;
		internal readonly JobDeclaration ReconWrappedJobDeclaration;
		public readonly UpdateActionCode ActionCode;

		public bool IsPaid
		{
			get { return PaymentFinalized == YesNoDefaultList.Codes.Yes; }
		}

		public ZBool IsActionForReplaceOrAdd
		{
			get
			{
				return (ActionCode == UpdateActionCode.Replace || ActionCode == UpdateActionCode.Add);
			}
		}

		public ZBool IsPaymentFinalizedRequired
		{
			get
			{
				var hasBeenLodgedAtCustoms = ReconDeclaration.ReconDeclaration.HasBeenLodgedAtCustoms;
				return IsActionForReplaceOrAdd && hasBeenLodgedAtCustoms;
			}
		}

		ZString SetPaymentFinalized()
		{
			var paid = ReconDeclaration.US_Paid;

			if (paid != YesNoDefaultList.Codes.Yes)
			{
				if (ReconWrappedJobDeclaration.RelatedStatement != null && ReconWrappedJobDeclaration.RelatedStatement.IsFinal)
				{
					paid = YesNoDefaultList.Codes.Yes;
				}
				else if (ReconWrappedJobDeclaration.US_PaymentType != PaymentTypeList.Codes.IndividualBasis || ReconWrappedJobDeclaration.US_PaymentDueDate.IsInTheFutureDatePartOnly)
				{
					paid = YesNoDefaultList.Codes.No;
				}
			}
			return paid;
		}

		public ReconDeclarationLookups Lookups
		{
			get { return new ReconDeclarationLookups(ReconDeclaration.ReconDeclaration, ReconWrappedJobDeclaration); }
		}

		[List(nameof(Lookups) + "." + nameof(ReconDeclarationLookups.US_YesNoList))]
		[MaxLength(1)]
		public ZString PaymentFinalized
		{
			get { return paymentFinalized; }
			set
			{
				SetNonPersistentPropertyValue(PaymentFinalizedInfo, ref paymentFinalized, value);
				ValidatePaymentFinalized();
				ReGenerateMessageContents();
			}
		}
		ZString paymentFinalized;

		public ZPropertyInfo PaymentFinalizedInfo
		{
			get { return GetZPropertyInfo(nameof(PaymentFinalized)); }
		}

		public void ValidatePaymentFinalized()
		{
			if (!IsValidationSuspended)
			{
				PaymentFinalizedInfo.ClearAllNotifications();
				if (IsPaymentFinalizedRequired)
				{
					ListValidation.MessageErrorIfInvalidCode(PaymentFinalizedInfo, Lookups.US_YesNoList);
					if (PaymentFinalized.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(PaymentFinalizedInfo, "Suppress Payment Info");
					}

					if (PaymentFinalized == YesNoDefaultList.Codes.No && ReconWrappedJobDeclaration.US_PaymentDueDate.IsInThePastDatePartOnly)
					{
						PaymentFinalizedInfo.AddWarning(PaymentDueDateHasPassedButIsIndicatedAsNotPaid);
					}
				}
			}
		}
		internal const string PaymentDueDateHasPassedButIsIndicatedAsNotPaid = "Payment Due Date has passed, but this is indicated as not paid yet.";

		public ZBool CertificationSignature
		{
			get { return certificationSignature; }
			set
			{
				SetNonPersistentPropertyValue(CertificationSignatureInfo, ref certificationSignature, value);
				ValidateCertificationSignature();
				ReGenerateMessageContents();
			}
		}
		ZBool certificationSignature;

		public ZPropertyInfo CertificationSignatureInfo
		{
			get { return GetZPropertyInfo(propertyName: nameof(CertificationSignature)); }
		}

		public void ValidateCertificationSignature()
		{
			if (!IsValidationSuspended)
			{
				CertificationSignatureInfo.ClearAllNotifications();
				if (!CertificationSignature && ActionCode != UpdateActionCode.Delete)
				{
					CertificationSignatureInfo.AddMessageError(ShouldBeSigned);
				}
				ValidateAggregateRecon();
			}
		}
		internal const string ShouldBeSigned = "Should be signed";

		public void ValidateAggregateRecon()
		{
			if (!IsValidationSuspended)
			{
				if (ActionCode != UpdateActionCode.Delete && ReconDeclaration.AggregateReconciliationIndicator && !ReconDeclaration.IsNoChangeAggregate && !ReconDeclaration.EntryLineGroups.Any())
				{
					CertificationSignatureInfo.AddMessageError(AggregateReconWithNoChangedLines);
				}
			}
		}
		internal const string AggregateReconWithNoChangedLines = "This recon is marked as aggregate with changes, but there are no changed lines.";

		public ZString US_MessageContents
		{
			get { return GetSerialiseMessageContents(); }
		}

		ZString GetSerialiseMessageContents()
		{
			if (!messageContentsCached.HasValue)
			{
				messageContentsCached = new ACEReconciliationMessageBuilder(UpdateActionCodeConverter.ConvertToString(ActionCode), ReconDeclaration, CertificationSignature, IsPaid).GetSerialiseMessageContents();
			}

			return messageContentsCached.Value;
		}
		ZString? messageContentsCached;

		void ReGenerateMessageContents()
		{
			var oldMessageContents = ZString.Empty;
			if (messageContentsCached.HasValue)
			{
				oldMessageContents = messageContentsCached.Value;
				messageContentsCached = null;
			}
			US_MessageContentsInfo.RefreshBinding(oldMessageContents);
		}

		public ZPropertyInfo US_MessageContentsInfo
		{
			get { return GetZPropertyInfo(nameof(US_MessageContents)); }
		}

		public ReconMessageManager MessageManager
		{
			get { return messageManager ?? (messageManager = new ReconMessageManager(this)); }
		}
		ReconMessageManager messageManager;

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateCertificationSignature();
			base.RunPreSaveValidationCore();
		}

		#endregion
	}
}

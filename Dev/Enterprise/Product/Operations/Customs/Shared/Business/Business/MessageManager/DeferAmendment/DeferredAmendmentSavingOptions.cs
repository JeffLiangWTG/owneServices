using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class DeferredAmendmentSavingOptions : NonPersistentBusinessObject
		, IObsoleteValidation
		, IDeferredAmendmentSavingOptions
	{
		public DeferredAmendmentSavingOptions()
		{
		}

		#region Public Boolean

		public bool ShouldSaveWithoutSendingAmendment
		{
			get { return ShouldSaveWithoutSendingAmendmentCore; }
		}

		protected virtual bool ShouldSaveWithoutSendingAmendmentCore
		{
			get { return SaveWithEntryChanges || SaveWithoutEntryChanges; }
		}

		public bool ShouldSendMessages
		{
			get { return ShouldSendMessagesCore; }
		}

		protected virtual bool ShouldSendMessagesCore
		{
			get { return SendAmendment; }
		}

		#endregion

		#region Public Bindable Properties

		public ZBool SendAmendment
		{
			get { return fSendAmendment; }
			set
			{
				SetNonPersistentPropertyValue(SendAmendmentInfo, ref fSendAmendment, value);
				if (value)
				{
					SaveWithEntryChanges = false;
					SaveWithoutEntryChanges = false;
				}
			}
		}
		internal ZBool fSendAmendment;

		public ZPropertyInfo SendAmendmentInfo
		{
			get { return GetZPropertyInfo(nameof(SendAmendment)); }
		}

		[ReadOnlyMember(nameof(SignificantAmendmentsHaveBeenMade))]
		public ZBool SaveWithoutEntryChanges
		{
			get { return fSaveWithoutEntryChanges; }
			set
			{
				SetNonPersistentPropertyValue(SaveWithoutEntryChangesInfo, ref fSaveWithoutEntryChanges, value);
				if (value)
				{
					SendAmendment = false;
					SaveWithEntryChanges = false;
				}
			}
		}
		internal ZBool fSaveWithoutEntryChanges;

		public ZPropertyInfo SaveWithoutEntryChangesInfo
		{
			get { return GetZPropertyInfo(nameof(SaveWithoutEntryChanges)); }
		}

		public ZBool SaveWithEntryChanges
		{
			get { return fSaveWithEntryChanges; }
			set
			{
				SetNonPersistentPropertyValue(SaveWithEntryChangesInfo, ref fSaveWithEntryChanges, value);
				if (value)
				{
					SendAmendment = false;
					SaveWithoutEntryChanges = false;
				}
			}
		}
		internal ZBool fSaveWithEntryChanges;

		public ZPropertyInfo SaveWithEntryChangesInfo
		{
			get { return GetZPropertyInfo(nameof(SaveWithEntryChanges)); }
		}

		public ZBool IsCancelled
		{
			get { return fIsCancelled; }
			set
			{
				fIsCancelled = value;
				if (value)
				{
					SendAmendment = false;
					SaveWithoutEntryChanges = false;
					SaveWithEntryChanges = false;
					OnIsCancelled();
				}
			}
		}
		ZBool fIsCancelled;

		protected ZBool SignificantAmendments { get; set; }

		protected virtual void OnIsCancelled()
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SendAmendment = true;
			SignificantAmendments = false;
		}

		#endregion

		#region IDeferredAmendmentSavingOptions Members

		ZBool IDeferredAmendmentSavingOptions.ShouldSendMessages
		{
			get { return SendAmendment; }
		}

		ZBool IDeferredAmendmentSavingOptions.ShouldTakeReasonForSavingWithoutSendingSeparately
		{
			get { return SaveWithEntryChanges; }
		}

		ZBool IDeferredAmendmentSavingOptions.ShouldSaveWithoutSendingAmendment
		{
			get { return SaveWithEntryChanges || SaveWithoutEntryChanges; }
		}

		public ZBool SignificantAmendmentsHaveBeenMade// this must be public for ReadOnlyMember above
		{
			get { return SignificantAmendments; }
		}

#if DEBUG

		void IDeferredAmendmentSavingOptions.SetSaveWithoutEntryChangesValueForTestingTo(ZBool value)
		{
			SaveWithoutEntryChanges = value;
		}

		void IDeferredAmendmentSavingOptions.SetSaveWithEntryChangesValueForTestingTo(ZBool value)
		{
			SaveWithEntryChanges = value;
		}

		void IDeferredAmendmentSavingOptions.SetSendAmendmentValueForTestingTo(ZBool value)
		{
			SendAmendment = value;
		}

#endif

		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterData.Business
{
	public class PersonMergeParticipants : NonPersistentBusinessObject
	{
		public PersonMergeParticipants(PersonMergeBusinessObjectCollection retainedCollection, PersonMergeBusinessObjectCollection dissolvedCollection)
			: base()
		{
			RetainedCollection = retainedCollection;
			DissolvedCollection = dissolvedCollection;
		}

		ZString mergingWarningMessage = Res.GetString("PersonMergeSummaryForm|MergeWarningLabel", "This action is irreversible and cannot be undone.\r\nAre you sure you want to proceed?");
		public ZString MergingWarningMessage
		{
			get => mergingWarningMessage;
			set
			{
				SetNonPersistentPropertyValue(MergingWarningMessageInfo, ref mergingWarningMessage, value);
			}
		}

		public ZPropertyInfo MergingWarningMessageInfo => GetZPropertyInfo(nameof(MergingWarningMessage));

		public PersonMergeBusinessObjectCollection RetainedCollection { get; }
		public PersonMergeBusinessObjectCollection DissolvedCollection { get; }
	}
}

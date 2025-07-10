namespace Enterprise.MasterFiles.Business
{
	public class SuperActivityRelatedParentActivityPivotCollection : RelatedParentActivityPivotCollection
	{
		public SuperActivityRelatedParentActivityPivotCollection(ISuperRelatableActivity master)
			: base(master)
		{
		}

		new ISuperRelatableActivity Master
		{
			get { return (ISuperRelatableActivity)base.Master; }
		}

		public override IRelatableActivity GetActivityForAddingActivity(IRelatableActivity activityBeingAdded)
		{
			if (activityBeingAdded.ShouldIgnoreSuperAndSubActivityRelationships)
			{
				return base.GetActivityForAddingActivity(activityBeingAdded);
			}

			return Master.GetMatchingSubActivity(activityBeingAdded) ?? base.GetActivityForAddingActivity(activityBeingAdded);
		}
	}
}

namespace Enterprise.MasterFiles.Business
{
	public class SubActivityRelatedParentActivityPivotCollection : RelatedParentActivityPivotCollection
	{
		public SubActivityRelatedParentActivityPivotCollection(ISubRelatableActivity master)
			: base(master)
		{
		}

		new ISubRelatableActivity Master
		{
			get { return (ISubRelatableActivity)base.Master; }
		}

		public override IRelatableActivity GetActivityForAddingActivity(IRelatableActivity activityBeingAdded)
		{
			if (activityBeingAdded.ShouldIgnoreSuperAndSubActivityRelationships || activityBeingAdded.RelatedChildActivityPivotCollection.IsSuperAndSubActivityRelationshipsIgnored)
			{
				return base.GetActivityForAddingActivity(activityBeingAdded);
			}

			var superActivity = Master.SuperActivity;
			if (superActivity == null)
			{
				return base.GetActivityForAddingActivity(activityBeingAdded);
			}

			return superActivity.GetMatchingSubActivity(activityBeingAdded) ?? (IRelatableActivity)superActivity;
		}
	}
}

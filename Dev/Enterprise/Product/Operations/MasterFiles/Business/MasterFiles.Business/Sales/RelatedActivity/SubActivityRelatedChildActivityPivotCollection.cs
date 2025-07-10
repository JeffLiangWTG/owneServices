namespace Enterprise.MasterFiles.Business
{
	public class SubActivityRelatedChildActivityPivotCollection : RelatedChildActivityPivotCollection
	{
		public SubActivityRelatedChildActivityPivotCollection(ISubRelatableActivity master)
			: base(master)
		{
		}

		new ISubRelatableActivity Master
		{
			get { return (ISubRelatableActivity)base.Master; }
		}

		public override IRelatableActivity GetActivityForAddingActivity(IRelatableActivity activityBeingAdded)
		{
			if (activityBeingAdded.ShouldIgnoreSuperAndSubActivityRelationships || activityBeingAdded.RelatedParentActivityPivotCollection.IsSuperAndSubActivityRelationshipsIgnored)
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

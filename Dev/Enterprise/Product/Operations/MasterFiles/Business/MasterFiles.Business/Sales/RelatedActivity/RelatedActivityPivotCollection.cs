using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public abstract class RelatedActivityPivotCollection : ActiveBusinessObjectCollection<ViewRelatedActivityPivot>, IRelatedActivityPivotCollection
	{
		protected RelatedActivityPivotCollection(IRelatableActivity master, ZQuery filter)
			: base(master.Factory, filter)
		{
			this.Master = master;
		}

		protected readonly IRelatableActivity Master;

		public abstract IRelatedActivityPivot FindPivot(IRelatableActivity activity);
		public abstract IRelatedActivityPivot AddNewPivot(IRelatableActivity activity);
		public abstract IEnumerable<IRelatableActivity> Activities { get; }
		public abstract RelationValidationResult CheckIsValidActivity(IRelatableActivity activity, bool activityMustBeInDatabase);
		public abstract RelationUpdateResult AddActivity(IRelatableActivity parentActivity);
		public abstract RelationValidationResult CheckCanRemoveActivity(IRelatableActivity parentActivity);
		public abstract RelationUpdateResult RemoveActivity(IRelatableActivity parentActivity);
		public abstract IRelatableActivityPivotCollectionSnapshot TakeSnapshot();

		#region AllowsMultipleActivities

		public abstract bool AllowsMultipleActivities { get; }

		#endregion

		#region AddNew

		IRelatedActivityPivot IRelatedActivityPivotCollection.AddNew()
		{
			var newItem = base.AddNew();
			newItem.RAP_IsEditable = true;

			return newItem;
		}

		#endregion

		#region GetActivityForAddingActivity

		public virtual IRelatableActivity GetActivityForAddingActivity(IRelatableActivity activityBeingAdded)
		{
			return Master;
		}

		#endregion

		#region CheckValidActivityType

		public RelationValidationResult CheckValidActivityType(ZString activityType)
		{
			return CheckValidActivityTypeCore(activityType);
		}

		protected virtual RelationValidationResult CheckValidActivityTypeCore(ZString activityType)
		{
			return new RelationValidationResult(true);
		}

		#endregion

		#region Delete

		public void Delete(IRelatedActivityPivot pivot)
		{
			base.Delete((ViewRelatedActivityPivot)pivot);
		}

		#endregion

		#region RelinkRelatedSuperAndSubActivities

		public void RelinkRelatedSuperAndSubActivities(bool onlyIfHasChanges = true)
		{
			if (!onlyIfHasChanges || !Master.IsInDatabase || Master.ClientHasChanges || Master.ContactHasChanges)
			{
				var superAndSubActivityTablePrefixes = GetSuperAndSubActivityTablePrefixes();
				var relatedSuperAndSubActivities = LoadRelatedActivitiesWithTablePrefixes(superAndSubActivityTablePrefixes);
				RelinkRelatedSuperAndSubActivitiesCore(relatedSuperAndSubActivities);
			}
		}

		protected abstract IEnumerable<IRelatableActivity> LoadRelatedActivitiesWithTablePrefixes(IEnumerable<ZString> tablePrefixes);

		protected virtual IEnumerable<ZString> GetSuperAndSubActivityTablePrefixes()
		{
			return RelatedActivityLinkLookups.GetTablePrefixesForSuperAndSubActivities(Factory).SelectMany(x => new[] { x.Key, x.Value });
		}

		protected virtual void RelinkRelatedSuperAndSubActivitiesCore(IEnumerable<IRelatableActivity> activities)
		{
			if (!IsSuperAndSubActivityRelationshipsIgnored)
			{
				foreach (var activity in activities.ToArray())
				{
					if (!activity.ShouldIgnoreSuperAndSubActivityRelationships)
					{
						if (activity is ISuperRelatableActivity)
						{
							var matchingSubActivity = ((ISuperRelatableActivity)activity).GetMatchingSubActivity(Master);
							if (matchingSubActivity != null)
							{
								var snapShot = TakeSnapshot();
								if (!RemoveActivity(activity).Success || !AddActivity(matchingSubActivity).Success)
								{
									snapShot.Restore();
								}
							}
						}
						else if (activity is ISubRelatableActivity)
						{
							var superActivity = ((ISubRelatableActivity)activity).SuperActivity;
							if (superActivity != null)
							{
								if (!Master.ContactHasChanges)
								{
								}
								var matchingSubActivity = superActivity.GetMatchingSubActivity(Master);
								if (matchingSubActivity != null)
								{
									if (matchingSubActivity != activity)
									{
										var snapShot = TakeSnapshot();
										if (!RemoveActivity(activity).Success || !AddActivity(matchingSubActivity).Success)
										{
											snapShot.Restore();
										}
									}
								}
								else
								{
									var snapShot = TakeSnapshot();
									if (!RemoveActivity(activity).Success || !AddActivity(superActivity).Success)
									{
										snapShot.Restore();
									}
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator<IRelatedActivityPivot> IEnumerable<IRelatedActivityPivot>.GetEnumerator()
		{
			return base.GetEnumerator();
		}

		#endregion

		public IDisposable TemporarilyIgnoreSuperAndSubActivityRelationships()
		{
			isSuperAndSubActivityRelationshipsIgnored = true;
			return new SuperAndSubActivityRelationshipsIgnorer(this);
		}

		public bool IsSuperAndSubActivityRelationshipsIgnored
		{
			get { return isSuperAndSubActivityRelationshipsIgnored; }
		}
		bool isSuperAndSubActivityRelationshipsIgnored;

		class SuperAndSubActivityRelationshipsIgnorer : IDisposable
		{
			public SuperAndSubActivityRelationshipsIgnorer(RelatedActivityPivotCollection activityCollection)
			{
				this.activityCollection = activityCollection;
			}

			readonly RelatedActivityPivotCollection activityCollection;

			public void Dispose()
			{
				if (activityCollection.isSuperAndSubActivityRelationshipsIgnored)
				{
					activityCollection.isSuperAndSubActivityRelationshipsIgnored = false;
				}
			}
		}
	}
}

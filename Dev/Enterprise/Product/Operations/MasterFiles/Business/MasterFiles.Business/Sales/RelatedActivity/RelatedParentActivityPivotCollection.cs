using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RelatedParentActivityPivotCollection : RelatedActivityPivotCollection, IRelatedParentActivityPivotCollection
	{
		#region Constructor

		public RelatedParentActivityPivotCollection(IRelatableActivity master)
			: base(master, GetRelatedParentActivitiesQuery(master))
		{
		}

		static ZQuery GetRelatedParentActivitiesQuery(IRelatableActivity bizObj)
		{
			var result = new ZQuery();
			result.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityTableCode, bizObj.TablePrefix);
			result.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ChildActivityID, bizObj.PK);

			return result;
		}

		#endregion

		#region AllowsMultipleActivities

		public sealed override bool AllowsMultipleActivities
		{
			get { return ViewRelatedActivityPivot.IsMultipleParentsAllowed(Factory, Master.TablePrefix); }
		}

		#endregion

		#region AddNewPivot

		public override IRelatedActivityPivot FindPivot(IRelatableActivity activity)
		{
			IEnumerable<IRelatedActivityPivot> self = this;
			return self.FirstOrDefault(x => x.RAP_ParentActivityTableCode == activity.TablePrefix && x.RAP_ParentActivityID == activity.PK);
		}

		public override IRelatedActivityPivot AddNewPivot(IRelatableActivity activity)
		{
			var pivot = AddNew();

			using (pivot.SuspendSettingHasChanges())
			using (pivot.GetValidationSuspender())
			{
				pivot.RAP_IsEditable = true;
				pivot.ParentActivity = activity;
			}

			return pivot;
		}

		#endregion

		#region Activities

		public sealed override IEnumerable<IRelatableActivity> Activities
		{
			get
			{
				IEnumerable<ViewRelatedActivityPivot> self = this;
				return self.Select(x => x.ParentActivity).Where(activity => activity != null);
			}
		}

		#endregion

		#region CheckIsValidActivity

		public sealed override RelationValidationResult CheckIsValidActivity(IRelatableActivity parentActivity, bool parentActivityMustBeInDatabase)
		{
			var activityToAddParent = GetActivityForAddingActivity(parentActivity);
			if (parentActivity != null)
			{
				parentActivity = parentActivity.RelatedChildActivityPivotCollection.GetActivityForAddingActivity(activityToAddParent);
			}

			return CheckIsValidParentCore(activityToAddParent, parentActivity, parentActivityMustBeInDatabase);
		}

		protected virtual RelationValidationResult CheckIsValidParentCore(IRelatableActivity childActivityInLocalFactory, IRelatableActivity parentActivity, bool parentMustBeInDatabase)
		{
			if (parentActivity == null)
			{
				return new RelationValidationResult(true);
			}

			if (parentActivity.PK == childActivityInLocalFactory.PK)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("9b145365-557f-44de-ba24-cb227a84b80d", "Can not make {0} the parent of itself.", childActivityInLocalFactory.HumanReadableName));
			}

			if (parentMustBeInDatabase && !parentActivity.IsInDatabase)
			{
				return new RelationValidationResult(false, GetParentMustBeSavedMessage(parentActivity));
			}

			if (!childActivityInLocalFactory.RelatedParentActivityPivotCollection.AllowsMultipleActivities)
			{
				var existingParent = childActivityInLocalFactory.RelatedParentActivityPivotCollection.Activities.FirstOrDefault(activity => activity.PK != parentActivity.PK);
				if (existingParent != null)
				{
					return new RelationValidationResult(false, ResString.GetMultilingualString("982DB169-75FD-4F22-A6C1-28A83FD31578", "{0} is already the parent of {1}. {1} can only have one parent.", existingParent.HumanReadableName, childActivityInLocalFactory.HumanReadableName));
				}
			}

			var parentActivityType = parentActivity.ActivityType;
			var parentActivityTypeValidation = childActivityInLocalFactory.RelatedParentActivityPivotCollection.CheckValidActivityType(parentActivityType);
			if (!parentActivityTypeValidation.IsValid)
			{
				return parentActivityTypeValidation;
			}

			var relationshipSequence = HierarchicalSequenceBuilder.GetRelationshipSequence(x => x.RelatedChildActivityPivotCollection.Activities, childActivityInLocalFactory, parentActivity, Tuple.Create(parentActivity, childActivityInLocalFactory));
			if (relationshipSequence.Any())
			{
				return new RelationValidationResult(false,
					ResString.GetMultilingualString("2767F215-EE61-455C-B061-A4645F22659A", @"{0} is already a related descendant of {1}.
Making {0} the parent of {1} would create an illegal cycle.

Conflicting relationship sequence: {2}",
					parentActivity.HumanReadableName,
					childActivityInLocalFactory.HumanReadableName,
					string.Join(" > ", relationshipSequence.Select(activity => activity.HumanReadableName))));
			}

			return new RelationValidationResult(true);
		}

		public static MultilingualString GetParentMustBeSavedMessage(IRelatableActivity parentActivity)
		{
			return ResString.GetMultilingualString("55fbd9be-0d65-4c5b-9dfd-0a7500c604e0", "{0} must be saved before it can be a parent of another activity.", parentActivity.HumanReadableName);
		}

		#endregion

		#region AddActivity

		public sealed override RelationUpdateResult AddActivity(IRelatableActivity parentActivity)
		{
			return AddActivity(parentActivity, true);
		}

		public RelationUpdateResult AddActivity(IRelatableActivity parentActivity, bool shouldCheckValidity)
		{
			var activityToAddParent = GetActivityForAddingActivity(parentActivity);
			parentActivity = parentActivity.RelatedChildActivityPivotCollection.GetActivityForAddingActivity(activityToAddParent);

			return AddParentCore(activityToAddParent, parentActivity, shouldCheckValidity);
		}

		RelationUpdateResult AddParentCore(IRelatableActivity childActivity, IRelatableActivity parentActivity, bool shouldCheckValidity = true)
		{
			if (ViewRelatedActivityPivot.Load(Factory, parentActivity, childActivity) != null)
			{
				return new RelationUpdateResult(false, ResString.GetMultilingualString("637053cc-bb0f-4f18-bf1c-49e21310055b", "{0} is already a parent of {1}.", parentActivity.HumanReadableName, childActivity.HumanReadableName));
			}

			if (shouldCheckValidity)
			{
				var validationResult = childActivity.RelatedParentActivityPivotCollection.CheckIsValidActivity(parentActivity, true);
				if (!validationResult.IsValid)
				{
					return new RelationUpdateResult(false, validationResult.Reason);
				}

				var backwardsValidationResult = parentActivity.RelatedChildActivityPivotCollection.CheckIsValidActivity(childActivity, false);
				if (!backwardsValidationResult.IsValid)
				{
					return new RelationUpdateResult(false, backwardsValidationResult.Reason);
				}
			}

			childActivity.RelatedParentActivityPivotCollection.AddNewPivot(parentActivity);
			return new RelationUpdateResult(true);
		}

		#endregion

		#region CheckCanRemoveActivity

		public sealed override RelationValidationResult CheckCanRemoveActivity(IRelatableActivity parentActivity)
		{
			return CheckCanRemoveActivityCore(parentActivity);
		}

		protected virtual RelationValidationResult CheckCanRemoveActivityCore(IRelatableActivity parentActivity)
		{
			return new RelationValidationResult(true);
		}

		#endregion

		#region RemoveActivity

		public sealed override RelationUpdateResult RemoveActivity(IRelatableActivity parentActivity)
		{
			var pivot = FindPivot(parentActivity);
			if (pivot == null)
			{
				return new RelationUpdateResult(false, ResString.GetMultilingualString("d027bb96-b8a3-42b0-8411-cf1b01d5b02a", "{0} is not a parent of {1}.", parentActivity.HumanReadableName, Master.HumanReadableName));
			}

			var validationResult = CheckCanRemoveActivity(parentActivity);
			if (!validationResult.IsValid)
			{
				return new RelationUpdateResult(false, validationResult.Reason);
			}

			var backwardsValidationResult = parentActivity.RelatedChildActivityPivotCollection.CheckCanRemoveActivity(Master);
			if (!backwardsValidationResult.IsValid)
			{
				return new RelationUpdateResult(false, backwardsValidationResult.Reason);
			}

			Delete(pivot);
			return new RelationUpdateResult(true);
		}

		#endregion

		#region RelinkRelatedSuperAndSubActivities

		protected override IEnumerable<IRelatableActivity> LoadRelatedActivitiesWithTablePrefixes(IEnumerable<ZString> tablePrefixes)
		{
			return ViewRelatedActivityPivot.LoadPivotsWithChild(Factory, Master, tablePrefixes, !Master.IsInDatabase).Select(pivot => pivot.ParentActivity).Where(activity => activity != null);
		}

		#endregion

		#region Snapshot

		public override IRelatableActivityPivotCollectionSnapshot TakeSnapshot()
		{
			return new Snapshot(this);
		}

		public class Snapshot : IRelatableActivityPivotCollectionSnapshot
		{
			public Snapshot(RelatedParentActivityPivotCollection relatedActivityCollection)
			{
				collection = relatedActivityCollection;
				activitiesAtSnapshot = relatedActivityCollection.Activities.ToArray();
			}

			public void Restore()
			{
				var pivotsToRemove = new HashSet<ViewRelatedActivityPivot>(collection);
				var activitiesToAdd = new HashSet<IRelatableActivity>(activitiesAtSnapshot);

				foreach (var pivot in collection.ToArray())
				{
					var activity = pivot.ParentActivity;
					if (activitiesToAdd.Contains(activity))
					{
						activitiesToAdd.Remove(activity);
						pivotsToRemove.Remove(pivot);
					}
				}

				foreach (var pivot in pivotsToRemove)
				{
					pivot.Delete();
				}

				foreach (var activity in activitiesToAdd)
				{
					collection.AddNewPivot(activity);
				}
			}

			readonly RelatedParentActivityPivotCollection collection;
			readonly IRelatableActivity[] activitiesAtSnapshot;
		}

		#endregion

		#region HasParent

		public bool HasParent(IRelatableActivity parentActivity)
		{
			return ViewRelatedActivityPivot.Load(Factory, parentActivity, Master) != null;
		}

		#endregion

		#region Implementation

		protected override void SetDefaultsForNewElementCore(ViewRelatedActivityPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.ChildActivity = Master;
		}

		#endregion
	}
}

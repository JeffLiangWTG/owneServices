using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RelatedChildActivityPivotCollection : RelatedActivityPivotCollection, IRelatedChildActivityPivotCollection
	{
		#region Constructor

		public RelatedChildActivityPivotCollection(IRelatableActivity master)
			: base(master, GetRelatedChildActivitiesQuery(master))
		{
		}

		static ZQuery GetRelatedChildActivitiesQuery(IRelatableActivity bizObj)
		{
			var result = new ZQuery();
			result.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityTableCode, bizObj.TablePrefix);
			result.AddToFilter(ViewRelatedActivityPivotSchema.RAP_ParentActivityID, bizObj.PK);

			return result;
		}

		#endregion

		#region AllowsMultipleActivities

		public sealed override bool AllowsMultipleActivities
		{
			get { return true; }
		}

		#endregion

		#region Pivot

		public override IRelatedActivityPivot FindPivot(IRelatableActivity activity)
		{
			IEnumerable<IRelatedActivityPivot> self = this;
			return self.FirstOrDefault(x => x.RAP_ChildActivityTableCode == activity.TablePrefix && x.RAP_ChildActivityID == activity.PK);
		}

		public override IRelatedActivityPivot AddNewPivot(IRelatableActivity activity)
		{
			var pivot = AddNew();
			using (pivot.SuspendSettingHasChanges())
			using (pivot.GetValidationSuspender())
			{
				pivot.RAP_IsEditable = true;
				pivot.ChildActivity = activity;
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
				return self.Select(pivot => pivot.ChildActivity).Where(activity => activity != null);
			}
		}

		#endregion

		#region CheckValidChildActivityType

		protected sealed override RelationValidationResult CheckValidActivityTypeCore(ZString childActivityType)
		{
			var directionRules = OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.Value;
			if (DirectionRuleAffectedActivityTypes.Contains(Master.ActivityType) && DirectionRuleAffectedActivityTypes.Contains(childActivityType))
			{
				var currentSequences = GetSalesRelationSequences();
				var currentSuccedingNodes = currentSequences.SelectMany(sequence => directionRules.FindAllSucceedingNodes(sequence));
				var validChildActivityTypes = new HashSet<ZString>();
				foreach (var node in currentSuccedingNodes)
				{
					if (node.Type == SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity || node.Type == SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities)
					{
						return new RelationValidationResult(true);
					}
					validChildActivityTypes.Add(node.Type);
				}

				if (!validChildActivityTypes.Contains(childActivityType))
				{
					return new RelationValidationResult(false, ResString.GetMultilingualString("efd5babf-a835-4553-9308-98db6c6b1ce7", @"{0} cannot have children of activity type '{1}'.
This rule is defined in the registry item [{2}/{3}].

{4}",
							Master.HumanReadableName,
							RelatableActivityTypes.GetDescriptionFromCode(childActivityType),
							OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.Category,
							OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.Caption,
							GetAllowedTypesString(validChildActivityTypes)));
				}
			}

			return new RelationValidationResult(true);
		}

		List<ZString[]> GetSalesRelationSequences()
		{
			var resultSequences = new List<ZString[]>();
			AddSalesRelationSequences(Master, new Stack<ZString>(), resultSequences, new HashSet<ZGuid>());

			return resultSequences;
		}

		void AddSalesRelationSequences(IRelatableActivity from, Stack<ZString> suffixSequence, List<ZString[]> resultSequences, HashSet<ZGuid> previouslyTraversedActivityPks)
		{
			suffixSequence.Push(from.ActivityType);

			var directionRules = OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.Value;
			var parentActivities = from.RelatedParentActivityPivotCollection.Activities.Where(activity => DirectionRuleAffectedActivityTypes.Contains(activity.ActivityType));
			if (!parentActivities.Any()) // is root
			{
				resultSequences.Add(suffixSequence.ToArray());
			}
			else
			{
				foreach (var parentActivity in parentActivities)
				{
					if (!previouslyTraversedActivityPks.Contains(parentActivity.PK))
					{
						previouslyTraversedActivityPks.Add(parentActivity.PK);

						AddSalesRelationSequences(parentActivity, suffixSequence, resultSequences, previouslyTraversedActivityPks);
						suffixSequence.Pop();
					}
				}
			}
		}

		MultilingualString GetAllowedTypesString(HashSet<ZString> allowedTypes)
		{
			if (allowedTypes.Count > 0)
			{
				return ResString.GetMultilingualString("061e8929-5653-495c-a7c4-b74256cf1af1", @"Only the following types are allowed: {0}", string.Join(", ", allowedTypes.Select(type => RelatableActivityTypes.GetDescriptionFromCode(type))));
			}
			else
			{
				return ResString.GetMultilingualString("fd29aba7-9556-4c14-bbb6-852dec013690", "No additional children are allowed.");
			}
		}

		HashSet<string> DirectionRuleAffectedActivityTypes
		{
			get { return new HashSet<string>(SalesRelationDirectionRuleCollection.AffectedActivityTypes, StringComparer.OrdinalIgnoreCase); }
		}

		#endregion

		#region CheckIsValidActivity

		public sealed override RelationValidationResult CheckIsValidActivity(IRelatableActivity childActivity, bool childActivityMustBeInDatabase)
		{
			var activityToAddChild = GetActivityForAddingActivity(childActivity);
			childActivity = childActivity.RelatedParentActivityPivotCollection.GetActivityForAddingActivity(activityToAddChild);

			return CheckIsValidChildCore(activityToAddChild, childActivity, childActivityMustBeInDatabase);
		}

		protected virtual RelationValidationResult CheckIsValidChildCore(IRelatableActivity parentActivity, IRelatableActivity childActivity, bool childMustBeInDatabase)
		{
			if (childActivity == null)
			{
				throw new ArgumentNullException(nameof(childActivity));
			}

			if (childActivity.PK == parentActivity.PK)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("e66d4e83-067d-4e47-8741-021344aeebe2", "Can not make {0} a child of itself.", parentActivity.HumanReadableName));
			}

			if (childMustBeInDatabase && !childActivity.IsInDatabase)
			{
				return new RelationValidationResult(false, ResString.GetMultilingualString("8fb9870a-d23d-4133-9a2c-28fa5c8b2f1b", "{0} must be saved before it can be a child of another activity.", childActivity.HumanReadableName));
			}

			if (!childActivity.RelatedParentActivityPivotCollection.AllowsMultipleActivities)
			{
				var existingParent = childActivity.RelatedParentActivityPivotCollection.Activities.FirstOrDefault(activity => activity.PK != parentActivity.PK);
				if (existingParent != null)
				{
					return new RelationValidationResult(false, ResString.GetMultilingualString("982DB169-75FD-4F22-A6C1-28A83FD31578", "{0} is already the parent of {1}. {1} can only have one parent.", existingParent.HumanReadableName, childActivity.HumanReadableName));
				}
			}

			var childActivityType = childActivity.ActivityType;
			var childActivityTypeValidation = parentActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(childActivityType);
			if (!childActivityTypeValidation.IsValid)
			{
				return childActivityTypeValidation;
			}

			var relationshipSequence = HierarchicalSequenceBuilder.GetRelationshipSequence(x => x.RelatedParentActivityPivotCollection.Activities, parentActivity, childActivity, Tuple.Create(parentActivity, childActivity));
			if (relationshipSequence.Any())
			{
				return new RelationValidationResult(false,
					ResString.GetMultilingualString("44616A07-9BC6-4039-A257-353B7CAA7D79", @"{1} is already a related ancestor of {0}.
Making {1} the child of {0} would create an illegal cycle.

Conflicting relationship sequence: {2}",
					parentActivity.HumanReadableName,
					childActivity.HumanReadableName,
					string.Join(" > ", new Stack<IRelatableActivity>(relationshipSequence).Select(activity => activity.HumanReadableName))));
			}

			return new RelationValidationResult(true);
		}

		#endregion

		#region AddActivity

		public sealed override RelationUpdateResult AddActivity(IRelatableActivity childActivity)
		{
			var activityToAddChild = GetActivityForAddingActivity(childActivity);
			childActivity = childActivity.RelatedParentActivityPivotCollection.GetActivityForAddingActivity(activityToAddChild);

			return AddChildCore(activityToAddChild, childActivity);
		}

		RelationUpdateResult AddChildCore(IRelatableActivity parentActivity, IRelatableActivity childActivity)
		{
			if (parentActivity.RelatedChildActivityPivotCollection.Activities.Contains(childActivity))
			{
				return new RelationUpdateResult(false, ResString.GetMultilingualString("02279352-0b06-4b7d-8e87-2eb2ce651888", "{0} is already a child of {1}.", childActivity.HumanReadableName, parentActivity.HumanReadableName));
			}

			var validationResult = parentActivity.RelatedChildActivityPivotCollection.CheckIsValidActivity(childActivity, true);
			if (!validationResult.IsValid)
			{
				return new RelationUpdateResult(false, validationResult.Reason);
			}

			var backwardsValidationResult = childActivity.RelatedParentActivityPivotCollection.CheckIsValidActivity(parentActivity, false);
			if (!backwardsValidationResult.IsValid)
			{
				return new RelationUpdateResult(false, backwardsValidationResult.Reason);
			}

			var pivot = parentActivity.RelatedChildActivityPivotCollection.AddNew();
			pivot.ChildActivity = childActivity;
			return new RelationUpdateResult(true);
		}

		#endregion

		#region CheckCanRemoveActivity

		public sealed override RelationValidationResult CheckCanRemoveActivity(IRelatableActivity childActivity)
		{
			return CheckCanRemoveActivityCore(childActivity);
		}

		protected virtual RelationValidationResult CheckCanRemoveActivityCore(IRelatableActivity childActivity)
		{
			return new RelationValidationResult(true);
		}

		#endregion

		#region RemoveActivity

		public sealed override RelationUpdateResult RemoveActivity(IRelatableActivity childActivity)
		{
			var pivot = FindPivot(childActivity);
			if (pivot == null)
			{
				return new RelationUpdateResult(false, ResString.GetMultilingualString("1f76dcdb-4703-4e6c-80cc-4bd829128191", "{0} is not a child of {1}.", childActivity.HumanReadableName, Master.HumanReadableName));
			}

			var validationResult = CheckCanRemoveActivity(childActivity);
			if (!validationResult.IsValid)
			{
				return new RelationUpdateResult(false, validationResult.Reason);
			}

			var backwardsValidationResult = childActivity.RelatedParentActivityPivotCollection.CheckCanRemoveActivity(Master);
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
			return ViewRelatedActivityPivot.LoadPivotsWithParent(Factory, Master, tablePrefixes, !Master.IsInDatabase).Select(pivot => pivot.ChildActivity).Where(activity => activity != null);
		}

		#endregion

		#region Snapshot

		public override IRelatableActivityPivotCollectionSnapshot TakeSnapshot()
		{
			return new Snapshot(this);
		}

		public class Snapshot : IRelatableActivityPivotCollectionSnapshot
		{
			public Snapshot(RelatedChildActivityPivotCollection relatedActivityCollection)
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
					var activity = pivot.ChildActivity;
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

			readonly RelatedChildActivityPivotCollection collection;
			readonly IRelatableActivity[] activitiesAtSnapshot;
		}

		#endregion

		#region HasDescendant

		public bool HasDescendant(IRelatableActivity descendant)
		{
			return HasDescendant(Master, descendant);
		}

		static bool HasDescendant(IRelatableActivity activity, IRelatableActivity descendant)
		{
			foreach (var childActivity in activity.RelatedChildActivityPivotCollection.Activities)
			{
				if (childActivity.PK == descendant.PK)
				{
					return true;
				}
				else if (HasDescendant(childActivity, descendant))
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Collections

		public ICodeDescriptionPairList RelatableActivityTypes
		{
			get
			{
				if (relatableActivityTypes == null)
				{
					var result = new CodeDescriptionPairList();
					foreach (var typeCodeInfoPair in RelatedActivityLinkLookups.GetRelatableActivityTypeDefinitions(Factory))
					{
						result.AddPair(typeCodeInfoPair.Key, typeCodeInfoPair.Value.Description);
					}
					result.SortByDescription();

					relatableActivityTypes = result;
				}

				return relatableActivityTypes;
			}
		}
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		ICodeDescriptionPairList relatableActivityTypes;

		#endregion

		#region Implementation

		protected override void SetDefaultsForNewElementCore(ViewRelatedActivityPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.ParentActivity = Master;
		}

		#endregion
	}
}

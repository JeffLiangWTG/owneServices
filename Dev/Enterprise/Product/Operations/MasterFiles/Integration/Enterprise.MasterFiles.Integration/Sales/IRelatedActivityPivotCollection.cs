using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRelatedActivityPivotCollection : IEnumerable<IRelatedActivityPivot>
	{
		IEnumerable<IRelatableActivity> Activities { get; }
		bool AllowsMultipleActivities { get; }
		bool ReadOnly { get; }

		event EventHandler CountChanged;

		IRelatedActivityPivot FindPivot(IRelatableActivity activity);
		IRelatedActivityPivot AddNew();
		IRelatedActivityPivot AddNewPivot(IRelatableActivity activity);
		RelationUpdateResult AddActivity(IRelatableActivity activity);
		IRelatableActivity GetActivityForAddingActivity(IRelatableActivity activityBeingAdded);
		RelationValidationResult CheckIsValidActivity(IRelatableActivity activity, bool activityMustBeInDatabase);
		RelationValidationResult CheckValidActivityType(ZString activityType);

		void Delete(IRelatedActivityPivot pivot);
		void DeleteAll();
		RelationValidationResult CheckCanRemoveActivity(IRelatableActivity activity);
		RelationUpdateResult RemoveActivity(IRelatableActivity activity);

		void RelinkRelatedSuperAndSubActivities(bool onlyIfHasChanges = true);
		IRelatableActivityPivotCollectionSnapshot TakeSnapshot();

		IDisposable TemporarilyIgnoreSuperAndSubActivityRelationships();
		bool IsSuperAndSubActivityRelationshipsIgnored { get; }
	}

	public interface IRelatedChildActivityPivotCollection : IRelatedActivityPivotCollection
	{
		bool HasDescendant(IRelatableActivity descendant);
	}

	public interface IRelatedParentActivityPivotCollection : IRelatedActivityPivotCollection
	{
		bool HasParent(IRelatableActivity parentActivity);
	}

	public interface IRelatableActivityPivotCollectionSnapshot
	{
		void Restore();
	}
}

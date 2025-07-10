using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RelatedActivityLinkValidation : ZValidation
	{
		public RelatedActivityLinkValidation(RelatedActivityLink parent)
			: base(parent)
		{
			this.activityLink = parent;
		}

		readonly RelatedActivityLink activityLink;

		#region ToActivityTypeForBinding

		public void ValidateToActivityTypeForBinding()
		{
			((IValidationInternals)this).Validate(activityLink.ToActivityTypeForBindingInfo, CheckToActivityTypeForBinding);
		}

		protected void CheckToActivityTypeForBinding()
		{
			MandatoryValidation.CheckEntered(activityLink.ToActivityTypeForBindingInfo);

			if (activityLink.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(activityLink.ToActivityTypeForBindingInfo);
				var parentActivity = activityLink.Pivot.ParentActivity;
				var childActivity = activityLink.Pivot.ChildActivity;
				if (parentActivity != null && childActivity != null)
				{
					var typeValidationResult = parentActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(childActivity.ActivityType);
					if (!typeValidationResult.IsValid)
					{
						activityLink.ToActivityTypeForBindingInfo.AddError(typeValidationResult.Reason);
					}
				}
			}
		}

		#endregion

		#region ToActivityIDForBinding

		public void ValidateToActivityIDForBinding()
		{
			((IValidationInternals)this).Validate(activityLink.ToActivityIDForBindingInfo, CheckToActivityIDForBinding);
		}

		protected void CheckToActivityIDForBinding()
		{
			MandatoryValidation.CheckEntered(activityLink.ToActivityIDForBindingInfo);

			if (!activityLink.ToActivityTableCode.IsEmpty && activityLink.ToActivityID.IsValid)
			{
				if (activityLink.Factory.Load<ViewRelatedActivityPivot>(GetPivotsWithToActivityQuery(activityLink.ToActivityTableCode, activityLink.ToActivityID)).Length > 1)
				{
					activityLink.ToActivityIDForBindingInfo.AddError(GetDuplicateError());
				}

				if (activityLink.HasChanges)
				{
					var parentActivity = activityLink.Pivot.ParentActivity;
					var childActivity = activityLink.Pivot.ChildActivity;
					if (parentActivity != null && childActivity != null)
					{
						var childValidationResult = parentActivity.RelatedChildActivityPivotCollection.CheckIsValidActivity(childActivity, childActivity != activityLink.FromActivity);
						if (!childValidationResult.IsValid)
						{
							activityLink.ToActivityIDForBindingInfo.AddError(childValidationResult.Reason);
						}
						else
						{
							var parentValidationResult = childActivity.RelatedParentActivityPivotCollection.CheckIsValidActivity(parentActivity, parentActivity != activityLink.FromActivity);
							if (!childValidationResult.IsValid)
							{
								activityLink.ToActivityIDForBindingInfo.AddError(parentValidationResult.Reason);
							}
						}
					}
				}
			}
		}

		ZQuery GetPivotsWithToActivityQuery(string toActivityTableCode, ZGuid toActivityID)
		{
			var fromActivityTableCode = activityLink.FromActivityTableCode;
			var fromActivityID = activityLink.FromActivityID;

			var parentQuery = new ZQuery();
			parentQuery.AddToFilter(RelatedActivityPivotSchema.RAP_ParentActivityTableCode, fromActivityTableCode);
			parentQuery.AddToFilter(RelatedActivityPivotSchema.RAP_ParentActivityID, fromActivityID);
			parentQuery.AddToFilter(RelatedActivityPivotSchema.RAP_ChildActivityTableCode, toActivityTableCode);
			parentQuery.AddToFilter(RelatedActivityPivotSchema.RAP_ChildActivityID, toActivityID);

			var childQuery = new ZQuery();
			childQuery.AddToFilter(RelatedActivityPivotSchema.RAP_ChildActivityTableCode, fromActivityTableCode);
			childQuery.AddToFilter(RelatedActivityPivotSchema.RAP_ChildActivityID, fromActivityID);
			childQuery.AddToFilter(RelatedActivityPivotSchema.RAP_ParentActivityTableCode, toActivityTableCode);
			childQuery.AddToFilter(RelatedActivityPivotSchema.RAP_ParentActivityID, toActivityID);

			var query = new ZQuery(parentQuery, JoinCondition.Or, childQuery);
			query.FetchOnlyFromLocalCache = true;
			return query;
		}

		string GetDuplicateError()
		{
			return ResString.GetMultilingualString("c1373158-6324-4432-91ad-1b2f128b6b16", "Duplicate related activity already exists.");
		}

		#endregion

		#region Implementation

		public override Type AutoValidationType
		{
			get { return typeof(RelatedActivityLinkValidation); }
		}

		public override void ValidateAll()
		{
			ValidateToActivityTypeForBinding();
			ValidateToActivityIDForBinding();
		}

		#endregion
	}
}

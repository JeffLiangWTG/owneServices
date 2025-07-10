using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class RelatedActivityLink : NonPersistentBusinessObject
	{
		public static RelatedActivityLink Get(IRelatedActivityPivot relatedActivityPivot, IRelatableActivity fromActivity)
		{
			Argument.NotNull(relatedActivityPivot, "relatedActivityPivot");
			Argument.NotNull(fromActivity, "fromActivity");

			return new RelatedActivityLink(relatedActivityPivot, fromActivity);
		}

		RelatedActivityLink(IRelatedActivityPivot relatedActivityPivot, IRelatableActivity fromActivity)
			: base(relatedActivityPivot.Factory)
		{
			this.Pivot = relatedActivityPivot;
			this.FromActivity = fromActivity;
			this.IsFromActivityChildActivity =
				(FromActivity.TablePrefix == Pivot.RAP_ChildActivityTableCode) &&
				(FromActivity.PK == Pivot.RAP_ChildActivityID);

			this.Pivot.HasChangesChanged += Pivot_HasChangesChanged;
		}

		public readonly IRelatedActivityPivot Pivot;

		#region FromActivity

		public readonly IRelatableActivity FromActivity;
		readonly bool IsFromActivityChildActivity;

		#region FromActivityType

		public ZString FromActivityType
		{
			get
			{
				return FromActivity != null ? FromActivity.ActivityType : ZString.Empty;
			}
		}

		#endregion

		#region FromActivityID

		public ZGuid FromActivityID
		{
			get
			{
				return IsFromActivityChildActivity ? Pivot.RAP_ChildActivityID : Pivot.RAP_ParentActivityID;
			}
			set
			{
				if (IsFromActivityChildActivity)
				{
					Pivot.RAP_ChildActivityID = value;
				}
				else
				{
					Pivot.RAP_ParentActivityID = value;
				}
			}
		}

		#endregion

		#region FromActivityTableCode

		public ZString FromActivityTableCode
		{
			get
			{
				return IsFromActivityChildActivity ? Pivot.RAP_ChildActivityTableCode : Pivot.RAP_ParentActivityTableCode;
			}
			set
			{
				if (IsFromActivityChildActivity)
				{
					Pivot.RAP_ChildActivityTableCode = value;
				}
				else
				{
					Pivot.RAP_ParentActivityTableCode = value;
				}
			}
		}

		#endregion

		#region FromActivityCode

		public ZString FromActivityCode
		{
			get
			{
				return GetCodeFromActivityTypeAndId(FromActivityType, FromActivityID);
			}
		}

		#endregion

		#endregion

		#region ToActivity

		public IRelatableActivity ToActivity
		{
			get { return IsFromActivityChildActivity ? Pivot.ParentActivity : Pivot.ChildActivity; }
		}

		#region ToActivityTableCode

		public ZString ToActivityTableCode
		{
			get
			{
				return IsFromActivityChildActivity ? Pivot.RAP_ParentActivityTableCode : Pivot.RAP_ChildActivityTableCode;
			}
			set
			{
				if (IsFromActivityChildActivity)
				{
					Pivot.RAP_ParentActivityTableCode = value;
				}
				else
				{
					Pivot.RAP_ChildActivityTableCode = value;
				}
			}
		}

		#endregion

		#region ToActivityID

		public ZGuid ToActivityID
		{
			get
			{
				return IsFromActivityChildActivity ? Pivot.RAP_ParentActivityID : Pivot.RAP_ChildActivityID;
			}
			set
			{
				if (IsFromActivityChildActivity)
				{
					Pivot.RAP_ParentActivityID = value;
				}
				else
				{
					Pivot.RAP_ChildActivityID = value;
				}
			}
		}

		#endregion

		#region ToActivityType

		public ZString ToActivityType
		{
			get
			{
				var toActivity = ToActivity;
				return toActivity != null ? toActivity.ActivityType : ZString.Empty;
			}
		}

		#endregion

		#region ToActivityCode

		public ZString ToActivityCode
		{
			get
			{
				return GetCodeFromActivityTypeAndId(ToActivityType, ToActivityID);
			}
		}

		#endregion

		#endregion

		#region ToActivityForBinding

		public IRelatableActivity ToActivityForBinding
		{
			get
			{
				return GetSuperToActivity(ToActivity);
			}
		}

		/// <summary>
		/// Even though IRelatableActivity implements IAuditDetails, GUI can't bind to its properties via ToActivityForBinding
		/// </summary>
		public AuditDetailsWrapper ToActivityForBindingLocalAuditDetails
		{
			get { return auditDetails ?? (auditDetails = ToActivityForBinding != null ? new AuditDetailsWrapper(Factory, ToActivityForBinding) : null); }
		}
		AuditDetailsWrapper auditDetails;

		IRelatableActivity GetSuperToActivity(IRelatableActivity activity)
		{
			var activityAsSubActivity = activity as ISubRelatableActivity;
			if (activityAsSubActivity != null)
			{
				return activityAsSubActivity.SuperActivity;
			}

			return activity;
		}

		#region ToActivityIDForBinding

		[List("Lookups.ToActivityCollection")]
		public ZGuid ToActivityIDForBinding
		{
			get
			{
				var toActivityForBinding = ToActivityForBinding;
				return toActivityForBinding != null ? toActivityForBinding.PK : ZGuid.Empty;
			}
			set
			{
				ToActivityID = value;
				RecalculateToActivityTableCode();
				ToActivityIDForBindingInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateToActivityIDForBinding();
				}
			}
		}

		public ZPropertyInfo ToActivityIDForBindingInfo
		{
			get { return GetZPropertyInfo(nameof(ToActivityIDForBinding)); }
		}

		protected bool ToActivityIDForBinding_ReadOnly
		{
			get { return Pivot.IsInDatabaseIncludingChildren || string.IsNullOrEmpty(ToActivityTypeForBinding) || ToActivityTypeForBindingInfo.HasErrors(); }
		}

		#endregion

		#region ToActivityTypeForBinding

		[List("Lookups.RelatableActivityTypes")]
		[MaxLength(3)]
		public ZString ToActivityTypeForBinding
		{
			get
			{
				if (!toActivityTypeForBinding.HasValue)
				{
					var toActivityForBinding = ToActivityForBinding;
					toActivityTypeForBinding = toActivityForBinding != null ? toActivityForBinding.ActivityType : ZString.Empty;
				}
				return toActivityTypeForBinding.Value;
			}
			set
			{
				SetNonPersistentPropertyValue(ToActivityTypeForBindingInfo, ref toActivityTypeForBinding, value);
				RecalculateToActivityTableCode();

				if (!IsValidationSuspended)
				{
					Validation.ValidateToActivityTypeForBinding();
				}
			}
		}
		ZString? toActivityTypeForBinding;

		void RecalculateToActivityTableCode()
		{
			RelatableActivityTypeDefinition typeInfo;
			if (RelatableActivityTypeDefinitions.TryGetValue(ToActivityTypeForBinding, out typeInfo))
			{
				ToActivityTableCode = typeInfo.TablePrefix;
			}
		}

		public ZPropertyInfo ToActivityTypeForBindingInfo
		{
			get { return GetZPropertyInfo(nameof(ToActivityTypeForBinding), ResString.GetMultilingualString("86C3A29F-F3B7-464D-8920-BA3B238B2768", "Activity Type")); }
		}

		protected bool ToActivityTypeForBinding_ReadOnly
		{
			get { return Pivot.IsInDatabaseIncludingChildren; }
		}

		#endregion

		#region ToActivityForBindingControllerId

		public ControllerID ToActivityForBindingControllerId
		{
			get
			{
				RelatableActivityTypeDefinition typeInfo;
				if (RelatableActivityTypeDefinitions.TryGetValue(ToActivityTypeForBinding, out typeInfo))
				{
					return typeInfo.ControllerId;
				}

				return null;
			}
		}

		#endregion

		#endregion

		#region RelatableActivityTypeDefinitions

		IDictionary<string, RelatableActivityTypeDefinition> relatableActivityTypeDefinitions;
		internal IDictionary<string, RelatableActivityTypeDefinition> RelatableActivityTypeDefinitions
		{
			get
			{
				return relatableActivityTypeDefinitions ?? (relatableActivityTypeDefinitions = Lookups.RelatableActivityTypeDefinitions);
			}
		}

		#endregion

		#region Lookups

		public RelatedActivityLinkLookups Lookups
		{
			get { return lookups ?? (lookups = RelatedActivityLinkLookups.New(this)); }
		}
		RelatedActivityLinkLookups lookups;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public RelatedActivityLinkValidation Validation
		{
			get { return new RelatedActivityLinkValidation(this); }
		}

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get
			{
				var parentActivity = Pivot.ParentActivity;
				var childActivity = Pivot.ChildActivity;
				if (parentActivity != null && childActivity != null)
				{
					var childValidationResult = childActivity.RelatedParentActivityPivotCollection.CheckCanRemoveActivity(parentActivity);
					if (!childValidationResult.IsValid)
					{
						return false;
					}

					var parentValidationResult = parentActivity.RelatedChildActivityPivotCollection.CheckCanRemoveActivity(childActivity);
					if (!parentValidationResult.IsValid)
					{
						return false;
					}
				}

				return base.CanDelete;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var parentActivity = Pivot.ParentActivity;
				var childActivity = Pivot.ChildActivity;
				if (parentActivity != null && childActivity != null)
				{
					var childValidationResult = childActivity.RelatedParentActivityPivotCollection.CheckCanRemoveActivity(parentActivity);
					if (!childValidationResult.IsValid)
					{
						return childValidationResult.Reason;
					}

					var parentValidationResult = parentActivity.RelatedChildActivityPivotCollection.CheckCanRemoveActivity(childActivity);
					if (!parentValidationResult.IsValid)
					{
						return parentValidationResult.Reason;
					}
				}

				return base.ReasonForNotAbleToDelete;
			}
		}

		#endregion

		#region Implementation

		void Pivot_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			HasChanges = Pivot.HasChanges;
		}

		public override void Delete()
		{
			Pivot.Delete();
			base.Delete();
		}

		ZString GetCodeFromActivityTypeAndId(ZString activityType, ZGuid activityId)
		{
			RelatableActivityTypeDefinition typeInfo;
			if (RelatableActivityTypeDefinitions.TryGetValue(activityType, out typeInfo))
			{
				var controllerFactory = ObjectFactory.Get<IControllerFactory>();
				var controllerAndBiz = controllerFactory.GetCorrectControllerAndBusinessObject(typeInfo.ControllerId, activityId, false);

				var codeDescription = (ICodeDescription)controllerAndBiz.BusinessObject;
				return codeDescription.Code;
			}

			return ZString.Empty;
		}

		#endregion
	}
}

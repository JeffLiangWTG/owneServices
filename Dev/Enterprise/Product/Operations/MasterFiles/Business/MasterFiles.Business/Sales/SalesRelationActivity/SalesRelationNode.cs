using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class SalesRelationNode : ZNode<IRelatableActivity>
	{
		public SalesRelationNode(ISalesRelationModel relationModel, IRelatableActivity relatableActivity)
			: base((SalesRelationModel)relationModel, relatableActivity)
		{
		}

		public SalesRelationNode(SalesRelationModel relationModel, IRelatableActivity relatableActivity)
			: base(relationModel, relatableActivity)
		{
		}

		new SalesRelationModel TreeModel
		{
			get { return (SalesRelationModel)base.TreeModel; }
		}

		#region Properties

		public override IRelatableActivity BizObjForBinding
		{
			get
			{
				var bizObjAsSubRelatableActivity = BizObj as ISubRelatableActivity;
				if (bizObjAsSubRelatableActivity != null && ShouldAccessSuperActivity(bizObjAsSubRelatableActivity))
				{
					var superActivity = bizObjAsSubRelatableActivity.SuperActivity;
					if (superActivity != null)
					{
						return superActivity;
					}
				}

				return BizObj;
			}
		}

		bool ShouldAccessSuperActivity(ISubRelatableActivity subRelatableActivity)
		{
			return !(IsCampaignHeaderAccessDenied && subRelatableActivity is IGlbCompanyCampaignItem);
		}

		IDisposable ShouldAccessCampaignHeaderIfNeeded()
		{
			IsCampaignHeaderAccessDenied = false;
			return new DisposableAction(delegate
			{ IsCampaignHeaderAccessDenied = true; });
		}
		bool IsCampaignHeaderAccessDenied = true;

		public bool BizObjForBindingIsDeleted
		{
			get { return ((BusinessObject)BizObjForBinding).IsDeleted; }
		}

		public ZString ActivityType
		{
			get { return !BizObjForBindingIsDeleted ? BizObjForBinding.ActivityType : ZString.Empty; }
		}

		public ZString UniqueID
		{
			get
			{
				using (ShouldAccessCampaignHeaderIfNeeded())
				{
					return !BizObjForBindingIsDeleted ? CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)BizObjForBinding) : ZString.Empty;
				}
			}
		}

		public ZString Summary
		{
			get
			{
				using (ShouldAccessCampaignHeaderIfNeeded())
				{
					return !BizObjForBindingIsDeleted ? BizObjForBinding.Summary : ZString.Empty;
				}
			}
		}

		public DateTime SystemCreateTime
		{
			get { return !BizObjForBindingIsDeleted && BizObjForBinding.SystemCreateTimeUtc.IsValid ? BizObjForBinding.SystemCreateTimeUtc.ToLocalBranchTime().ToDateTime() : DateTime.MinValue; }
		}

		public DateTime SystemLastEditTime
		{
			get { return !BizObjForBindingIsDeleted && BizObjForBinding.SystemLastEditTimeUtc.IsValid ? BizObjForBinding.SystemLastEditTimeUtc.ToLocalBranchTime().ToDateTime() : DateTime.MinValue; }
		}

		public ZString ClientName
		{
			get { return !BizObjForBindingIsDeleted && BizObjForBinding.Client != null ? BizObjForBinding.Client.OH_FullName : ZString.Empty; }
		}

		public ZString ClientCode
		{
			get { return !BizObjForBindingIsDeleted && BizObjForBinding.Client != null ? BizObjForBinding.Client.OH_Code : ZString.Empty; }
		}

		public ZString ContactName
		{
			get { return !BizObjForBindingIsDeleted && BizObjForBinding.Contact != null ? BizObjForBinding.Contact.OC_ContactName : ZString.Empty; }
		}

		#endregion

		#region Parent

		protected override IRelatableActivity LoadParentBizObj()
		{
			EnsureListEventsAdded();

			IRelatableActivity result = null;
			if (BizObj.RelatedParentActivityPivotCollection.AllowsMultipleActivities && BizObj.RelatedParentActivityPivotCollection.Count() > 1 && (BizObj == TreeModel.Master || BizObj.RelatedChildActivityPivotCollection.HasDescendant(TreeModel.Master)))
			{
				// Activities that can have multiple parents should be the root of the sales relation model if they are the master or one of its ancestors
				// This is because we wouldn't know which parent to display in a tree!
				result = null;
			}
			else
			{
				var parentActivities = BizObj.RelatedParentActivityPivotCollection.Activities.Where(x => IncludedActivityTypes.Contains(x.ActivityType)).ToArray();
				if (parentActivities.Length <= 1)
				{
					result = parentActivities.FirstOrDefault();
				}
				else if (parentNode != null)
				{
					foreach (var parentActivity in parentActivities)
					{
						if (parentNode.BizObj.PK == parentActivity.PK)
						{
							result = parentActivity;
							break;
						}
					}
				}
			}

			if (result != null)
			{
				using (TreeModel.GetNodeRefreshSuspender())
				{
					// Do not add parent node if there is already a child node for same BizObj (to prevent infinite loop)
					if (FindDescendantNode(result) != null)
					{
						result = null;
					}
				}
			}

			return result;
		}

		protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(IRelatableActivity previousParent, IRelatableActivity newParent, bool checkValid)
		{
			RelationUpdateResult updateResult = new RelationUpdateResult(true);
			newParent = (checkValid && newParent != null) ? newParent.RelatedChildActivityPivotCollection.GetActivityForAddingActivity(BizObj) : newParent;

			if (previousParent != newParent)
			{
				var relationshipSnapshot = BizObj.RelatedParentActivityPivotCollection.TakeSnapshot();
				{
					if (updateResult.Success && previousParent != null)
					{
						if (checkValid)
						{
							updateResult = BizObj.RelatedParentActivityPivotCollection.RemoveActivity(previousParent);
						}
						else
						{
							var pivot = BizObj.RelatedParentActivityPivotCollection.FindPivot(previousParent);
							if (pivot != null)
							{
								pivot.Delete();
							}
							updateResult = new RelationUpdateResult(true);
						}
					}

					if (updateResult.Success && newParent != null)
					{
						if (checkValid)
						{
							updateResult = BizObj.RelatedParentActivityPivotCollection.AddActivity(newParent);
						}
						else
						{
							BizObj.RelatedParentActivityPivotCollection.AddNewPivot(newParent);
							updateResult = new RelationUpdateResult(true);
						}
					}

					if (!updateResult.Success)
					{
						relationshipSnapshot.Restore();
					}
				}

				return new ChangeParentOnBizObjResult(updateResult.Success, updateResult.Reason, updateResult.Success ? newParent : previousParent);
			}

			return new ChangeParentOnBizObjResult(true, newParent);
		}

		#endregion

		#region Children

		protected override IEnumerable<IRelatableActivity> LoadChildBizObjs()
		{
			EnsureListEventsAdded();
			return LoadChildBizObjs(BizObj);
		}

		protected IEnumerable<IRelatableActivity> LoadChildBizObjs(IRelatableActivity bizObj)
		{
			IEnumerable<IRelatableActivity> result;
			if (BizObj.RelatedParentActivityPivotCollection.AllowsMultipleActivities && BizObj != TreeModel.Master)
			{
				// The children of activities that can have multiple parents AND aren't an ancestor of the master should not be included in the sales relation model
				var master = TreeModel.Master;
				result = bizObj.RelatedChildActivityPivotCollection.Activities
					.Where(child => IncludedActivityTypes.Contains(child.ActivityType))
					.Where(child => child == master || child.RelatedChildActivityPivotCollection.HasDescendant(master));
			}
			else
			{
				result = bizObj.RelatedChildActivityPivotCollection.Activities
					.Where(child => IncludedActivityTypes.Contains(child.ActivityType));
			}

			using (TreeModel.GetNodeRefreshSuspender())
			{
				// Do not add child node if there is already a parent node for same BizObj (to prevent infinite loop)
				result = result.Where(x => FindAncestorNode(x) == null);
			}

			return result.Distinct();
		}

		#endregion

		#region Lookups

		HashSet<string> IncludedActivityTypes
		{
			get
			{
				if (includedActivityTypes == null)
				{
					includedActivityTypes = new HashSet<string>(SalesRelationTypeList.New().GetCodes(), StringComparer.OrdinalIgnoreCase);
				}

				return includedActivityTypes;
			}
		}
		HashSet<string> includedActivityTypes;

		#endregion

		#region ListChanged

		void EnsureListEventsAdded()
		{
			if (!listEventsAdded)
			{
				listEventsAdded = true;
				((IBindingList)BizObj.RelatedChildActivityPivotCollection).ListChanged += ChildPivotCollection_ListChanged;
				((IBindingList)BizObj.RelatedParentActivityPivotCollection).ListChanged += ParentPivotCollection_ListChanged;
			}
		}
		bool listEventsAdded;

		void ChildPivotCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			NotifyChildNodesChanged();
		}

		void ParentPivotCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			NotifyParentNodesChanged();
		}

		#endregion
	}
}

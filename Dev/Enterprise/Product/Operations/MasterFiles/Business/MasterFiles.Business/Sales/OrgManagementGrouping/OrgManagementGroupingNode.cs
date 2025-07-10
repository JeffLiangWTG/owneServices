using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class OrgManagementGroupingNode : ZNode<OrgHeader>
	{
		public OrgManagementGroupingNode(OrgManagementGroupingModel model, OrgHeader orgHeader)
			: base(model, orgHeader)
		{
		}

		#region Properties

		public ZString ClientCode
		{
			get { return BizObj.OH_Code; }
		}

		public ZString ClientName
		{
			get { return BizObj.OH_FullName; }
		}

		#endregion

		#region Parent

		protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(OrgHeader previousParent, OrgHeader newParent, bool checkValid)
		{
			RelationUpdateResult updateResult = new RelationUpdateResult(true);
			if (previousParent != newParent)
			{
				var relationSnapshot = BizObj.RelatedManagementParentRelations.TakeSnapshot();
				if (previousParent != null)
				{
					BizObj.RelatedManagementParentRelations.RemoveOrganisation(previousParent);
				}

				if (newParent != null)
				{
					if (checkValid)
					{
						updateResult = BizObj.RelatedManagementParentRelations.AddOrganisation(newParent);
					}
					else
					{
						BizObj.RelatedManagementParentRelations.AddOrganisationWithoutCheckingValid(newParent);
						updateResult = new RelationUpdateResult(true);
					}
				}

				if (!updateResult.Success)
				{
					relationSnapshot.Restore();
				}

				return new ChangeParentOnBizObjResult(updateResult.Success, updateResult.Reason, updateResult.Success ? newParent : previousParent);
			}

			return new ChangeParentOnBizObjResult(true, newParent);
		}

		protected override OrgHeader LoadParentBizObj()
		{
			EnsureListEventsAdded();

			var parentOrganisations = BizObj.RelatedManagementParentRelations.Organisations.ToArray();
			if (parentOrganisations.Length <= 1)
			{
				return parentOrganisations.FirstOrDefault();
			}

			if (parentNode != null)
			{
				foreach (var parentOrg in parentOrganisations)
				{
					if (parentNode.BizObj.PK == parentOrg.PK)
					{
						return parentOrg;
					}
				}
			}

			return null;
		}

		#endregion

		#region Children

		protected override IEnumerable<OrgHeader> LoadChildBizObjs()
		{
			EnsureListEventsAdded();

			return BizObj.RelatedManagementSubsidiaryRelations.Organisations.Distinct(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer);
		}

		#endregion

		#region ListChanged

		void EnsureListEventsAdded()
		{
			if (!listEventsAdded)
			{
				listEventsAdded = true;
				((IBindingList)BizObj.RelatedManagementSubsidiaryRelations).ListChanged += ChildPivotCollection_ListChanged;
				((IBindingList)BizObj.RelatedManagementParentRelations).ListChanged += ParentPivotCollection_ListChanged;
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

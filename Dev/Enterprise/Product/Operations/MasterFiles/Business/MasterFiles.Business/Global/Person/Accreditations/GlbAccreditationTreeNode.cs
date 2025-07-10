using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbAccreditationTreeNode : ZNode<GlbAccreditationTreeBizObjWrapperBase>
	{
		public GlbAccreditationTreeNode(ZTreeModel<GlbAccreditationTreeBizObjWrapperBase> treeModel, GlbAccreditationTreeBizObjWrapperBase bizObj)
			: base(treeModel, bizObj)
		{
		}

		protected override GlbAccreditationTreeBizObjWrapperBase LoadParentBizObj()
		{
			return null;
		}

		protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(
			GlbAccreditationTreeBizObjWrapperBase previousParent,
			GlbAccreditationTreeBizObjWrapperBase newParent, bool checkValid)
		{
			var groupWrapper = BizObjForBinding as GlbAccreditationGroupWrapper;

			if (groupWrapper == null)
			{
				return new ChangeParentOnBizObjResult(false, Res.GetString("284B2E5C-5E09-40A7-A306-C9485CF3A894", "Can only move a group or a skill"), previousParent);
			}

			return MoveGroup(previousParent, newParent, groupWrapper);
		}

		ChangeParentOnBizObjResult MoveGroup(
			GlbAccreditationTreeBizObjWrapperBase previousParent,
			GlbAccreditationTreeBizObjWrapperBase newParent,
			GlbAccreditationGroupWrapper groupWrapper)
		{
			var accreditation = (TreeModel as IGlbAccreditationTreeModel).Accreditation;
			var prevParentWrapper = previousParent as GlbAccreditationGroupWrapper;

			if (newParent == null)
			{
				groupWrapper.Group.HJG_ParentID = accreditation.PK;
				groupWrapper.Group.HJG_ParentTableCode = GlbAccreditationSchema.Constants.Prefix;
				TreeModel.RootNodes.Add(this);
				accreditation.Groups.Add(groupWrapper.Group);
				prevParentWrapper.Group.Groups.Remove(groupWrapper.Group);

				return new ChangeParentOnBizObjResult(true, newParent);
			}

			var newParentGroupWrapper = newParent as GlbAccreditationGroupWrapper;
			if (newParentGroupWrapper == null)
			{
				return new ChangeParentOnBizObjResult(false,
					Res.GetString("8E29E394-3367-424E-9B6D-585BD1F992D2", "Can only move to a group"), previousParent);
			}

			if (newParentGroupWrapper.Group.HJG_ParentTableCode == GlbAccreditationJobSkillGroupSchema.Constants.Prefix)
			{
				return new ChangeParentOnBizObjResult(false,
					Res.GetString("0F4380CD-0245-4350-BC1A-1E85B50B819E", "New parent already has another parent group. Only one level of sub-groups allowed."), previousParent);
			}

			if (groupWrapper.Group.Groups.Count > 0 || ChildNodes.Any(n => n.BizObjForBinding is GlbAccreditationGroupWrapper))
			{
				return new ChangeParentOnBizObjResult(false,
					Res.GetString("4298EF5D-0144-45E5-9B7A-1D8034DF4DAB", "You can only have one level of sub groups"),
					previousParent);
			}

			groupWrapper.Group.HJG_ParentID = newParentGroupWrapper.Group.PK;
			groupWrapper.Group.HJG_ParentTableCode = GlbAccreditationJobSkillGroupSchema.Constants.Prefix;
			newParentGroupWrapper.Group.Groups.Add(groupWrapper.Group);

			if (previousParent == null)
			{
				TreeModel.RootNodes.Remove(this);
				accreditation.Groups.Remove(groupWrapper.Group);
			}
			else
			{
				prevParentWrapper.Group.Groups.Remove(groupWrapper.Group);
			}

			return new ChangeParentOnBizObjResult(true, newParent);
		}

		protected override IEnumerable<GlbAccreditationTreeBizObjWrapperBase> LoadChildBizObjs()
		{
			return BizObjForBinding.Children?.Cast<GlbAccreditationTreeBizObjWrapperBase>() ?? Enumerable.Empty<GlbAccreditationTreeBizObjWrapperBase>();
		}

		public ZString Description => BizObjForBinding.Description;
		public ZString CommenceDate => BizObjForBinding.CommenceDateUtc;
		public ZString CompletionDate => BizObjForBinding.CompletionDateUtc;
		public ZString Score => BizObjForBinding.Score;
		public ZString Progress => BizObjForBinding.Progress;
		public ZString CompetencyRules => BizObjForBinding.CompetencyRules;
		public ZString ApplicantEmail => BizObjForBinding.ApplicantEmail;
		public ZString Comment => BizObjForBinding.Comment;
	}
}

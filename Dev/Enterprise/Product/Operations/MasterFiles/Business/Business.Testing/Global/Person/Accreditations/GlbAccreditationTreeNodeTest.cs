using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbAccreditationTreeNodeTest : TestCaseWithFactory
	{
		public void TestChangeParent()
		{
			var accreditation = Factory.New<IGlbAccreditation>();
			var group1 = Factory.New<IGlbAccreditationJobSkillGroup>();
			group1.HJG_ParentID = accreditation.PK;
			group1.HJG_ParentTableCode = "HAC";

			var group2 = Factory.New<IGlbAccreditationJobSkillGroup>();
			group2.HJG_ParentID = accreditation.PK;
			group2.HJG_ParentTableCode = "HAC";

			var group3 = Factory.New<IGlbAccreditationJobSkillGroup>();
			group3.HJG_ParentID = group1.PK;
			group3.HJG_ParentTableCode = "HJG";

			var group4 = Factory.New<IGlbAccreditationJobSkillGroup>();
			group4.HJG_ParentID = group2.PK;
			group4.HJG_ParentTableCode = "HJG";

			var group5 = Factory.New<IGlbAccreditationJobSkillGroup>();
			group5.HJG_ParentID = group1.PK;
			group5.HJG_ParentTableCode = "HJG";

			var person = Factory.New<GlbPerson>();

			var model = new GlbAccreditationTreeModel(accreditation);
			var wrapper1 = new GlbAccreditationGroupWrapperForTest(model, group1, person);
			var wrapper2 = new GlbAccreditationGroupWrapperForTest(model, group2, person);
			var wrapper3 = new GlbAccreditationGroupWrapperForTest(model, group3, person);
			var wrapper4 = new GlbAccreditationGroupWrapperForTest(model, group4, person);
			var wrapper5 = new GlbAccreditationGroupWrapperForTest(model, group5, person);

			var node1 = new GlbAccreditationTreeNodeForTest(model, wrapper1);
			var node2 = new GlbAccreditationTreeNodeForTest(model, wrapper2);
			var node3 = new GlbAccreditationTreeNodeForTest(model, wrapper3);
			var node4 = new GlbAccreditationTreeNodeForTest(model, wrapper4);
			var node5 = new GlbAccreditationTreeNodeForTest(model, wrapper5);

			AssertEquals(false, node1.ChangeParentOnBizObj_Exposed(null, node2.BizObjForBinding).Success);
			AssertEquals(true, node4.ChangeParentOnBizObj_Exposed(node2.BizObjForBinding, node1.BizObjForBinding).Success);
			AssertEquals(true, node5.ChangeParentOnBizObj_Exposed(node1.BizObjForBinding, null).Success);
			AssertEquals(true, node2.ChangeParentOnBizObj_Exposed(null, node5.BizObjForBinding).Success);
			AssertEquals(true, node3.ChangeParentOnBizObj_Exposed(node1.BizObjForBinding, node5.BizObjForBinding).Success);

			AssertEquals(accreditation.PK, group1.HJG_ParentID);
			AssertEquals("HAC", group1.HJG_ParentTableCode);

			AssertEquals(group5.PK, group2.HJG_ParentID);
			AssertEquals("HJG", group2.HJG_ParentTableCode);

			AssertEquals(group5.PK, group3.HJG_ParentID);
			AssertEquals("HJG", group3.HJG_ParentTableCode);

			AssertEquals(group1.PK, group4.HJG_ParentID);
			AssertEquals("HJG", group4.HJG_ParentTableCode);

			AssertEquals(accreditation.PK, group5.HJG_ParentID);
			AssertEquals("HAC", group5.HJG_ParentTableCode);
		}

		class GlbAccreditationTreeNodeForTest : GlbAccreditationTreeNode
		{
			public GlbAccreditationTreeNodeForTest(ZTreeModel<GlbAccreditationTreeBizObjWrapperBase> treeModel, GlbAccreditationTreeBizObjWrapperBase bizObj) : base(treeModel, bizObj)
			{
			}

			public ChangeParentOnBizObjResult ChangeParentOnBizObj_Exposed(
				GlbAccreditationTreeBizObjWrapperBase previousParent,
				GlbAccreditationTreeBizObjWrapperBase newParent)
			{
				return ChangeParentOnBizObj(previousParent, newParent, false);
			}
		}

		public void TestAccreditationNode()
		{
			var accreditation = Factory.New<IGlbAccreditation>();
			var group = Factory.New<IGlbAccreditationJobSkillGroup>();
			var person = Factory.New<GlbPerson>();

			var model = new GlbAccreditationTreeModel(accreditation);
			var wrapper = new GlbAccreditationGroupWrapperForTest(model, group, person);

			var node = new GlbAccreditationTreeNode(model, wrapper);

			AssertEquals("Description", node.Description);
			AssertEquals("CommenceDate", node.CommenceDate);
			AssertEquals("CompletionDate", node.CompletionDate);
			AssertEquals("Score", node.Score);
			AssertEquals("Progress", node.Progress);
			AssertEquals("CompetencyRules", node.CompetencyRules);
		}

		class GlbAccreditationGroupWrapperForTest : GlbAccreditationGroupWrapper
		{
			public GlbAccreditationGroupWrapperForTest(ZTreeModel<GlbAccreditationTreeBizObjWrapperBase> treeModel,
				IGlbAccreditationJobSkillGroup @group, GlbPerson person) : base(treeModel, @group, person)
			{
			}

			public override ZString Description => "Description";
			public override ZString CommenceDateUtc => "CommenceDate";
			public override ZString CompletionDateUtc => "CompletionDate";
			public override ZString Score => "Score";
			public override ZString Progress => "Progress";
			public override ZString CompetencyRules => "CompetencyRules";
		}
	}
}

using System.Linq;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgManagementGroupingNodeTest : ZNodeTestCase<OrgManagementGroupingNode, OrgHeader>
	{
		#region ParentNode

		public void TestLoadParentBizObj()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var subOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var subOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var subOrg1SubOrg = Factory.NewWithValidTestData<OrgHeader>();
			var subOrg2SubOrg = Factory.NewWithValidTestData<OrgHeader>();
			var subOrgWithTwoParents = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			OrgManagementRelatedPartyTestHelper.Create(Factory, org, subOrg1);
			OrgManagementRelatedPartyTestHelper.Create(Factory, org, subOrg2);
			OrgManagementRelatedPartyTestHelper.Create(Factory, subOrg1, subOrg1SubOrg);
			OrgManagementRelatedPartyTestHelper.Create(Factory, subOrg2, subOrg2SubOrg);
			OrgManagementRelatedPartyTestHelper.Create(Factory, subOrg1, subOrgWithTwoParents, GlbCompany.CurrentCompany);
			OrgManagementRelatedPartyTestHelper.Create(Factory, subOrg2, subOrgWithTwoParents, GlbCompany.CurrentCompany);

			AssertEquals(subOrg2, new OrgManagementGroupingNode(org.OrgManagementGroupingModel, subOrg2SubOrg).ParentNode.BizObj);
			AssertEquals(org, org.OrgManagementGroupingModel.FindNode(subOrg1).ParentNode.BizObj);
			AssertEquals(subOrg1, org.OrgManagementGroupingModel.FindNode(subOrg1SubOrg).ParentNode.BizObj);
			AssertEquals(null, subOrgWithTwoParents.OrgManagementGroupingModel.FindNode(subOrgWithTwoParents).ParentNode);
		}

		#endregion

		#region ChildNode

		public void TestChildNodes()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var subOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var subOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var subOrg1SubOrg = Factory.NewWithValidTestData<OrgHeader>();
			var subOrg2SubOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			OrgManagementRelatedPartyTestHelper.Create(Factory, org, subOrg1);
			OrgManagementRelatedPartyTestHelper.Create(Factory, org, subOrg2);
			OrgManagementRelatedPartyTestHelper.Create(Factory, subOrg1, subOrg1SubOrg);
			OrgManagementRelatedPartyTestHelper.Create(Factory, subOrg2, subOrg2SubOrg);

			var model = org.OrgManagementGroupingModel;

			AssertContainsExactElementsInAnyOrder(new OrgHeader[] { subOrg1, subOrg2 },
				new OrgManagementGroupingNode(model, org).ChildNodes.Select(node => node.BizObj));

			AssertContainsExactElementsInAnyOrder(new OrgHeader[] { subOrg1SubOrg },
				new OrgManagementGroupingNode(model, subOrg1).ChildNodes.Select(node => node.BizObj));

			AssertContainsExactElementsInAnyOrder(new OrgHeader[] { subOrg2SubOrg },
				new OrgManagementGroupingNode(model, subOrg2).ChildNodes.Select(node => node.BizObj));
		}

		public void TestChildNodes_OnlyIncludeCompanyOnceIfItExistsMultipleTimes()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var subOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			OrgManagementRelatedPartyTestHelper.Create(Factory, org, subOrg);
			OrgManagementRelatedPartyTestHelper.Create(Factory, org, subOrg, GlbCompany.CurrentCompany);

			var model = org.OrgManagementGroupingModel;
			AssertContainsExactElementsInAnyOrder(new OrgHeader[] { subOrg },
				new OrgManagementGroupingNode(model, org).ChildNodes.Select(node => node.BizObj));
		}

		#endregion

		#region Implementation

		protected override OrgHeader GetNewBizObjForTest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			return org;
		}

		protected override ZTreeModel<OrgHeader> GetNewTreeModelForTest()
		{
			return new OrgManagementGroupingModel(GetNewBizObjForTest());
		}

		protected override OrgHeader CreateChild(OrgHeader parent)
		{
			var child = Factory.NewWithValidTestData<OrgHeader>();
			OrgManagementRelatedPartyTestHelper.Create(Factory, parent, child);
			Factory.Save();
			return child;
		}

		protected override System.Collections.Generic.IEnumerable<OrgHeader> GetChildren(OrgHeader parent)
		{
			return parent.RelatedManagementSubsidiaryRelations.Organisations;
		}

		protected override OrgHeader CreateParent(OrgHeader child)
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			OrgManagementRelatedPartyTestHelper.Create(Factory, parent, child);
			Factory.Save();
			return parent;
		}

		protected override OrgHeader GetParent(OrgHeader child)
		{
			return child.RelatedManagementParentRelations.Organisations.FirstOrDefault();
		}

		#endregion
	}
}

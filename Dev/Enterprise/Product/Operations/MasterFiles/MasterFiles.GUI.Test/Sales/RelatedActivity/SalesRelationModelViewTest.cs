using System;
using System.Linq;
using Aga.Business.Tree;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SalesRelationModelView))]
	sealed class SalesRelationModelViewTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		#region ShowCommunication

		public void TestShowCommunication()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = header.PK;
			var firstCall = Factory.NewWithValidTestData<OrgSalesCall>();
			var secondCall = Factory.NewWithValidTestData<OrgSalesCall>();
			var secondCallOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			var model = new SalesRelationModel(inquiry);
			inquiry.RelatedChildActivityPivotCollection.AddActivity(firstCall);
			inquiry.RelatedChildActivityPivotCollection.AddActivity(secondCall);
			secondCall.RelatedChildActivityPivotCollection.AddActivity(secondCallOpportunity);
			var modelView = new SalesRelationModelView(model);

			Factory.Save();

			var inquiryNode = model.FindNode(inquiry);
			var firstCallNode = model.FindNode(firstCall);
			var secondCallNode = model.FindNode(secondCall);
			var secondCallOpportunityNode = model.FindNode(secondCallOpportunity);

			modelView.ShowCommunication = true;
			AssertArrayEqualsByElements("TreePath.Empty", new[] { inquiryNode }, modelView.GetChildren(TreePath.Empty).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("inquiryNode", new[] { firstCallNode, secondCallNode }, modelView.GetChildren(new TreePath(new[] { inquiryNode })).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("firstCallNode", Array.Empty<SalesRelationNode>(), modelView.GetChildren(new TreePath(new[] { inquiryNode, firstCallNode })).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("secondCallNode - Should not show communication children", Array.Empty<SalesRelationNode>(), modelView.GetChildren(new TreePath(new[] { inquiryNode, secondCallNode })).Cast<ZNode<IRelatableActivity>>().ToArray());

			modelView.ShowCommunication = false;
			AssertArrayEqualsByElements("TreePath.Empty", new[] { inquiryNode }, modelView.GetChildren(TreePath.Empty).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("inquiryNode", Array.Empty<SalesRelationNode>(), modelView.GetChildren(new TreePath(inquiryNode)).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("firstCallNode", Array.Empty<SalesRelationNode>(), modelView.GetChildren(new TreePath(new[] { inquiryNode, firstCallNode })).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("secondCallNode - Should not show communication children", Array.Empty<SalesRelationNode>(), modelView.GetChildren(new TreePath(new[] { inquiryNode, secondCallNode })).Cast<ZNode<IRelatableActivity>>().ToArray());
		}

		public void TestShowCommunication_AlwaysShowRoot()
		{
			var rootCall = Factory.NewWithValidTestData<OrgSalesCall>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var secondCall = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			rootCall.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(secondCall);
			Factory.Save();

			var model = new SalesRelationModel(inquiry);
			var rootCallNode = model.RootNode;
			var inquiryNode = rootCallNode.ChildNodes.ToArray()[0];
			var secondCallNode = inquiryNode.ChildNodes.ToArray()[0];
			var modelView = new SalesRelationModelView(model);

			modelView.ShowCommunication = true;
			AssertArrayEqualsByElements("TreePath.Empty", new[] { rootCallNode }, modelView.GetChildren(TreePath.Empty).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("rootCallNode", new[] { inquiryNode }, modelView.GetChildren(new TreePath(rootCallNode)).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("rootCallNode, inquiryNode", new[] { secondCallNode }, modelView.GetChildren(new TreePath(new[] { rootCallNode, inquiryNode })).Cast<ZNode<IRelatableActivity>>().ToArray());

			modelView.ShowCommunication = false;
			AssertArrayEqualsByElements("TreePath.Empty", new[] { rootCallNode }, modelView.GetChildren(TreePath.Empty).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("rootCallNode", new[] { inquiryNode }, modelView.GetChildren(new TreePath(rootCallNode)).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("rootCallNode, inquiryNode", Array.Empty<SalesRelationNode>(), modelView.GetChildren(new TreePath(new[] { rootCallNode, inquiryNode })).Cast<ZNode<IRelatableActivity>>().ToArray());
		}

		public void TestShowCommunication_AlwaysShowPathToMasterNode()
		{
			var rootCall = Factory.NewWithValidTestData<OrgSalesCall>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var secondCall = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			rootCall.RelatedChildActivityPivotCollection.AddActivity(inquiry);
			inquiry.RelatedChildActivityPivotCollection.AddActivity(opportunity);
			opportunity.RelatedChildActivityPivotCollection.AddActivity(secondCall);
			Factory.Save();

			var model = new SalesRelationModel(opportunity);
			var rootCallNode = model.RootNode;
			var inquiryNode = rootCallNode.ChildNodes.ToArray()[0];
			var opportunityNode = inquiryNode.ChildNodes.ToArray()[0];
			var secondCallNode = opportunityNode.ChildNodes.ToArray()[0];
			var modelView = new SalesRelationModelView(model);

			modelView.ShowCommunication = true;
			AssertArrayEqualsByElements("TreePath.Empty", new[] { rootCallNode }, modelView.GetChildren(TreePath.Empty).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("rootCallNode", new[] { inquiryNode }, modelView.GetChildren(new TreePath(rootCallNode)).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("inquiryNode", new[] { opportunityNode }, modelView.GetChildren(new TreePath(new[] { rootCallNode, inquiryNode })).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("opportunityNode", new[] { secondCallNode }, modelView.GetChildren(new TreePath(new[] { rootCallNode, inquiryNode, opportunityNode })).Cast<ZNode<IRelatableActivity>>().ToArray());

			modelView.ShowCommunication = false;
			AssertArrayEqualsByElements("TreePath.Empty", new[] { rootCallNode }, modelView.GetChildren(TreePath.Empty).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("rootCallNode", new[] { inquiryNode }, modelView.GetChildren(new TreePath(rootCallNode)).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("inquiryNode", new[] { opportunityNode }, modelView.GetChildren(new TreePath(new[] { rootCallNode, inquiryNode })).Cast<ZNode<IRelatableActivity>>().ToArray());
			AssertArrayEqualsByElements("opportunityNode", Array.Empty<SalesRelationNode>(), modelView.GetChildren(new TreePath(new[] { rootCallNode, inquiryNode, opportunityNode })).Cast<ZNode<IRelatableActivity>>().ToArray());
		}

		#endregion

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			return new SalesRelationModelView(opportunity.SalesRelationModel);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);
		}

		#endregion
	}
}

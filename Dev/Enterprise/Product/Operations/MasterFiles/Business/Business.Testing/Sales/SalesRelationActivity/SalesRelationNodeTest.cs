using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SalesRelationNodeTest : ZNodeTestCase<SalesRelationNode, IRelatableActivity>
	{
		#region Properties

		public void TestBizObjForBinding()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			var node = new SalesRelationNode(inquiry.SalesRelationModel, inquiry);
			AssertEquals(inquiry, node.BizObjForBinding);

			var superActivty = Factory.New<DummySuperRelatableActivity>();
			var subActivity = Factory.New<DummySubRelatableActivity>();
			subActivity.SuperActivity = superActivty;

			node = new SalesRelationNode(new SalesRelationModel(subActivity), subActivity);
			AssertEquals("Should return super activity as the bizobj for binding", superActivty, node.BizObjForBinding);

			subActivity.SuperActivity = null;
			AssertEquals("Should fallback to itself if subactivity does not have super activity", subActivity, node.BizObjForBinding);
		}

		public void TestBizObjForBinding_Campaign()
		{
			var campaign = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaign)));
			var item = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			item[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			item[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = ZGuid.NewZGuid();
			Factory.Save();

			var itemNode = GetRelationNode(item);
			AssertEquals("Should not return campaign header as the bizobj for binding", item, itemNode.BizObjForBinding);
		}

		public void TestActivityType()
		{
			var dummyRelatableActivity = Factory.New<DummyRelatableActivity>();
			dummyRelatableActivity.ActivityType = "XXX";

			var node = new SalesRelationNode(new SalesRelationModel(dummyRelatableActivity), dummyRelatableActivity);
			AssertEquals("XXX", node.ActivityType);

			dummyRelatableActivity.Delete();
			AssertEquals("", node.ActivityType);
		}

		public void TestActivityType_Campaign()
		{
			var campaign = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaign)));
			var item = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			item[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			item[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = ZGuid.NewZGuid();
			Factory.Save();

			var itemNode = GetRelationNode(item);
			AssertEquals(RelatableActivityTypeList.Codes.CampaignManagement, itemNode.ActivityType);
		}

		public void TestUniqueID()
		{
			var dummyRelatableActivity = Factory.New<DummyRelatableActivityWithCodePropertyAttribute>();
			dummyRelatableActivity.CodeProperty = "XXX";

			var node = new SalesRelationNode(new SalesRelationModel(dummyRelatableActivity), dummyRelatableActivity);
			AssertEquals("XXX", node.UniqueID);

			dummyRelatableActivity.Delete();
			AssertEquals("", node.UniqueID);
		}

		public void TestUniqueID_Campaign()
		{
			var campaign = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaign)));
			campaign[GlbCompanyCampaignSchema.G0_CampaignName] = "Campaign Header - Part of Summary";
			var item = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			item[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			item[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = ZGuid.NewZGuid();
			Factory.Save();

			var itemNode = GetRelationNode(item);
			AssertEquals("Campaign header's UniqueID", CodePropertyAttribute.CodeFromBusinessObject(campaign), itemNode.UniqueID);
		}

		public void TestSummary()
		{
			var dummyRelatableActivity = Factory.New<DummyRelatableActivity>();
			dummyRelatableActivity.Summary = "This is the summary";

			var node = new SalesRelationNode(new SalesRelationModel(dummyRelatableActivity), dummyRelatableActivity);
			AssertEquals("This is the summary", node.Summary);

			dummyRelatableActivity.Delete();
			AssertEquals("", node.Summary);
		}

		public void TestSummary_Campaign()
		{
			var campaign = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaign)));
			campaign[GlbCompanyCampaignSchema.G0_CampaignName] = "Campaign Header - Part of Summary";
			var item = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			item[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			item[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = ZGuid.NewZGuid();
			Factory.Save();

			var itemNode = GetRelationNode(item);
			AssertEquals("Campaign header's Summary", ((ISalesRelationActivity)campaign).Summary, itemNode.Summary);
		}

		public void TestSystemCreateTime()
		{
			var dummyRelatableActivity = Factory.New<DummyRelatableActivity>();
			dummyRelatableActivity.SystemCreateTimeUtc = new ZDateTime(2002, 2, 2);

			var node = new SalesRelationNode(new SalesRelationModel(dummyRelatableActivity), dummyRelatableActivity);
			AssertEquals(new ZDateTime(new DateTime(2002, 2, 2)), new ZDateTime(node.SystemCreateTime).ToUniversalBranchTime());

			dummyRelatableActivity.Delete();
			AssertEquals(DateTime.MinValue, node.SystemCreateTime);
		}

		[TestDate(2015, 4, 13)]
		public void TestSystemCreateTime_Campaign()
		{
			var campaign = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaign)));
			campaign[GlbCompanyCampaignSchema.G0_SystemCreateTimeUtc] = ZDateTime.UtcNow.AddDays(-1);
			campaign[GlbCompanyCampaignSchema.G0_SystemLastEditTimeUtc] = ZDateTime.UtcNow.AddDays(-1);
			var item = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			item[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			item[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = ZGuid.NewZGuid();
			item[GlbCompanyCampaignItemSchema.G8_SystemCreateTimeUtc] = ZDateTime.UtcNow;
			Factory.Save();

			var itemNode = GetRelationNode(item);
			AssertEquals(new ZDateTime(((ISalesRelationActivity)item).SystemCreateTimeUtc), new ZDateTime(itemNode.SystemCreateTime).ToUniversalBranchTime());
		}

		public void TestSystemLastEditTime()
		{
			var dummyRelatableActivity = Factory.New<DummyRelatableActivity>();
			dummyRelatableActivity.SystemLastEditTimeUtc = new ZDateTime(2002, 2, 2);

			var node = new SalesRelationNode(new SalesRelationModel(dummyRelatableActivity), dummyRelatableActivity);
			AssertEquals(new ZDateTime(new DateTime(2002, 2, 2)), new ZDateTime(node.SystemLastEditTime).ToUniversalBranchTime());

			dummyRelatableActivity.Delete();
			AssertEquals(DateTime.MinValue, node.SystemLastEditTime);
		}

		public void TestSystemLastEditTime_Campaign()
		{
			var campaign = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaign)));
			campaign[GlbCompanyCampaignSchema.G0_SystemCreateTimeUtc] = ZDateTime.UtcNow.AddDays(-1);
			campaign[GlbCompanyCampaignSchema.G0_SystemLastEditTimeUtc] = ZDateTime.UtcNow.AddDays(-1);
			var item = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			item[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			item[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = ZGuid.NewZGuid();
			item[GlbCompanyCampaignItemSchema.G8_SystemCreateTimeUtc] = ZDateTime.UtcNow;
			Factory.Save();

			var itemNode = GetRelationNode(item);
			AssertEquals(((ZDateTime)item[GlbCompanyCampaignItemSchema.G8_SystemLastEditTimeUtc]).ToLocalBranchTime(), itemNode.SystemLastEditTime);
		}

		public void TestClientName()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_FullName = "WiseTech Global";
			var dummyRelatableActivity = Factory.New<DummyRelatableActivity>();
			dummyRelatableActivity.Client = client;

			var node = new SalesRelationNode(new SalesRelationModel(dummyRelatableActivity), dummyRelatableActivity);
			AssertEquals("WiseTech Global", node.ClientName);

			dummyRelatableActivity.Delete();
			AssertEquals("", node.ClientName);
		}

		public void TestClientName_Campaign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Org Full Name";
			org.OH_Code = "OrgCode1";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jenny";
			Factory.Save();

			var campaign = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaign)));
			var item = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			item[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			item[GlbCompanyCampaignItemSchema.G8_RecipientID] = contact.PK;
			Factory.Save();

			var itemNode = GetRelationNode(item);
			AssertEquals("Org Full Name", itemNode.ClientName);
		}

		public void TestClientCode()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "WISTECH";
			var dummyRelatableActivity = Factory.New<DummyRelatableActivity>();
			dummyRelatableActivity.Client = client;

			var node = new SalesRelationNode(new SalesRelationModel(dummyRelatableActivity), dummyRelatableActivity);
			AssertEquals("WISTECH", node.ClientCode);

			dummyRelatableActivity.Delete();
			AssertEquals("", node.ClientCode);
		}

		public void TestClientCode_Campaign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Org Full Name";
			org.OH_Code = "OrgCode1";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jenny";
			Factory.Save();

			var campaign = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaign)));
			var item = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			item[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			item[GlbCompanyCampaignItemSchema.G8_RecipientID] = contact.PK;
			Factory.Save();

			var itemNode = GetRelationNode(item);
			AssertEquals("OrgCode1", itemNode.ClientCode);
		}

		public void TestContactName()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Andrew";
			var dummyRelatableActivity = Factory.New<DummyRelatableActivity>();
			dummyRelatableActivity.Contact = contact;

			var node = new SalesRelationNode(new SalesRelationModel(dummyRelatableActivity), dummyRelatableActivity);
			AssertEquals("Andrew", node.ContactName);

			dummyRelatableActivity.Delete();
			AssertEquals("", node.ContactName);
		}

		public void TestContactName_Campaign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Org Full Name";
			org.OH_Code = "OrgCode1";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jenny";

			Factory.Save();

			var campaign = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaign)));
			var item = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			item[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			item[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			item[GlbCompanyCampaignItemSchema.G8_RecipientID] = contact.PK;
			Factory.Save();

			var itemNode = GetRelationNode(item);
			AssertEquals("Jenny", itemNode.ContactName);
		}

		SalesRelationNode GetRelationNode(BusinessObject bizo)
		{
			var relationItem = bizo as ISalesRelationActivity;
			var model = relationItem.SalesRelationModel as SalesRelationModel;
			return model.FindNode(relationItem) as SalesRelationNode;
		}

		#endregion

		#region ParentNode

		public void TestSetParentNode_WithRemoveParentValidationError()
		{
			#region Test Data

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.LinkedInquiry = inquiry;
			Factory.Save();

			var model = inquiry.SalesRelationModel;
			var inquiryNode = model.FindNode(inquiry);
			var communicationNode = model.FindNode(communication);
			AssertEquals("Precondition", inquiryNode, communicationNode.ParentNode);

			#endregion

			bool raisedChangeNodeParentFailedEvent = false;
			model.ChangeNodeParentFailed += (sender, e) =>
			{
				raisedChangeNodeParentFailedEvent = true;
			};
			communicationNode.ParentNode = null;

			AssertEquals("Should have raised ChangeNodeParentFailed event", true, raisedChangeNodeParentFailedEvent);
			AssertEquals("Parent node should not change", inquiryNode, communicationNode.ParentNode);
			AssertContainsExactElementsInAnyOrder("Parents collection should not change", BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[] { inquiry },
				communication.RelatedParentActivityPivotCollection.Activities.Cast<BusinessObject>());
		}

		public void TestSetParentNode_WithAddParentValidationError()
		{
			#region Test Data

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(opportunity1);
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(opportunity2);

			var model = inquiry.SalesRelationModel;
			var inquiryNode = model.FindNode(inquiry);
			var opportunity1Node = model.FindNode(opportunity1);
			var opportunity2Node = model.FindNode(opportunity2);
			AssertEquals("Precondition", inquiryNode, opportunity1Node.ParentNode);

			var disallowAllDirectionRules = new SalesRelationDirectionRuleCollection();
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, disallowAllDirectionRules);
			AssertEquals("Precondition", false, opportunity1.RelatedParentActivityPivotCollection.CheckIsValidActivity(opportunity2, true).IsValid);

			#endregion

			bool raisedChangeNodeParentFailedEvent = false;
			model.ChangeNodeParentFailed += (sender, e) =>
			{
				raisedChangeNodeParentFailedEvent = true;
			};
			opportunity1Node.ParentNode = opportunity2Node;

			AssertEquals("Should have raised ChangeNodeParentFailed event", true, raisedChangeNodeParentFailedEvent);
			AssertEquals("Parent node should not change", inquiryNode, opportunity1Node.ParentNode);
			AssertContainsExactElementsInAnyOrder("Parents collection should not change", BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[] { inquiry },
				opportunity1.RelatedParentActivityPivotCollection.Activities.Cast<BusinessObject>());
		}

		public void TestLoadParentBizObj_ShouldBeNullIfBizObjAllowsMultipleParentsAndIsAnAncestorOfTheMaster()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var communication1 = Factory.NewWithValidTestData<OrgSalesCall>();
			var inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication2 = Factory.NewWithValidTestData<OrgSalesCall>();
			var inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			AssertEquals("Precondition", true, communication1.RelatedParentActivityPivotCollection.AllowsMultipleActivities);
			AssertEquals("Precondition", true, communication2.RelatedParentActivityPivotCollection.AllowsMultipleActivities);

			opportunity1.RelatedChildActivityPivotCollection.AddNewPivot(communication1);
			opportunity2.RelatedChildActivityPivotCollection.AddNewPivot(communication1);
			communication1.RelatedChildActivityPivotCollection.AddNewPivot(inquiry1);
			inquiry1.RelatedChildActivityPivotCollection.AddNewPivot(communication2);
			communication2.RelatedChildActivityPivotCollection.AddNewPivot(inquiry2);

			AssertEquals(communication2, new SalesRelationNode(inquiry1.SalesRelationModel, inquiry2).ParentNode.BizObj);
			AssertEquals(inquiry1, inquiry1.SalesRelationModel.FindNode(communication2).ParentNode.BizObj);
			AssertEquals(communication1, inquiry1.SalesRelationModel.FindNode(inquiry1).ParentNode.BizObj);
			AssertNull("Should be null as communication1 allows multiple parents and is an ancestor of the master node", inquiry1.SalesRelationModel.FindNode(communication1).ParentNode);
		}

		public void TestLoadParentObj_AllowMultipleParentsButOnlyOneExists()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var communication1 = Factory.NewWithValidTestData<OrgSalesCall>();
			var communication2 = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			AssertEquals("Precondition", true, communication1.RelatedParentActivityPivotCollection.AllowsMultipleActivities);
			AssertEquals("Precondition", true, communication2.RelatedParentActivityPivotCollection.AllowsMultipleActivities);

			opportunity1.RelatedChildActivityPivotCollection.AddNewPivot(communication1);
			opportunity2.RelatedChildActivityPivotCollection.AddNewPivot(communication2);
			opportunity3.RelatedChildActivityPivotCollection.AddNewPivot(communication2);

			AssertEquals(opportunity1, new SalesRelationNode(communication1.SalesRelationModel, communication1).ParentNode.BizObj);
			AssertNull(new SalesRelationNode(communication2.SalesRelationModel, communication2).ParentNode);
		}

		public void TestLoadParentBizObj_OnlyIncludesSalesRelationTypesAndCommunications()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var dummy = Factory.NewWithValidTestData<DummyRelatableActivity>();

			opportunity1.RelatedParentActivityPivotCollection.AddNewPivot(inquiry);
			opportunity2.RelatedParentActivityPivotCollection.AddNewPivot(communication);
			opportunity3.RelatedParentActivityPivotCollection.AddNewPivot(dummy);

			AssertEquals("Should have loaded inquiry as parent", inquiry.PK, opportunity1.SalesRelationModel.RootNode.BizObj.PK);
			AssertEquals("Should have loaded communication as parent", communication.PK, opportunity2.SalesRelationModel.RootNode.BizObj.PK);
			AssertEquals("Should not have loaded dummyRelatableActivity as parent", opportunity3.PK, opportunity3.SalesRelationModel.RootNode.BizObj.PK);
		}

		#endregion

		#region ChildNodes

		public void TestChildNodes_ShouldNotIncludeCommunicationChildrenThatAreNotAnAncestorOfTheMaster()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiry_communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var inquiry_communication_opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry_communication_opportunity1_opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry_communication_opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry_inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			AssertEquals("Precondition", true, inquiry_communication.RelatedParentActivityPivotCollection.AllowsMultipleActivities);

			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(inquiry_communication);
			inquiry_communication.RelatedChildActivityPivotCollection.AddNewPivot(inquiry_communication_opportunity1);
			inquiry_communication_opportunity1.RelatedChildActivityPivotCollection.AddNewPivot(inquiry_communication_opportunity1_opportunity);
			inquiry_communication.RelatedChildActivityPivotCollection.AddNewPivot(inquiry_communication_opportunity2);
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(inquiry_inquiry);

			var model = inquiry_communication_opportunity1_opportunity.SalesRelationModel;

			AssertContainsExactElementsInAnyOrder(new IRelatableActivity[] { inquiry_communication, inquiry_inquiry }, new SalesRelationNode(model, inquiry).ChildNodes.Select(node => node.BizObj));

			AssertContainsExactElementsInAnyOrder("Should not include inquiry_communication_opportunity2 because it is a communication child and not an ancestor of the master",
				new IRelatableActivity[] { inquiry_communication_opportunity1 }, new SalesRelationNode(model, inquiry_communication).ChildNodes.Select(node => node.BizObj));

			AssertContainsExactElementsInAnyOrder(new IRelatableActivity[] { inquiry_communication_opportunity1_opportunity }, new SalesRelationNode(model, inquiry_communication_opportunity1).ChildNodes.Select(node => node.BizObj));
		}

		public void TestChildNodes_OnlyIncludesSalesRelationTypesAndCommunications()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var dummy = Factory.NewWithValidTestData<DummyRelatableActivity>();

			opportunity.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			opportunity.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			opportunity.RelatedChildActivityPivotCollection.AddNewPivot(dummy);

			AssertContainsExactElementsInAnyOrder("Should not have included dummyRelatableActivity as child",
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[]
				{
					inquiry,
					communication
				},
				opportunity.SalesRelationModel.MasterNode.ChildNodes.Select(node => (BusinessObject)node.BizObj));
		}

		public void TestChildNodes_DoNotIncludeChildIfItProducesACycle()
		{
			var opportunityA = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunityB = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunityC = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			opportunityA.RelatedParentActivityPivotCollection.AddNewPivot(opportunityB);
			opportunityB.RelatedParentActivityPivotCollection.AddNewPivot(opportunityC);
			opportunityC.RelatedParentActivityPivotCollection.AddNewPivot(opportunityA);

			opportunityA.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);

			AssertEquals(opportunityC.PK, opportunityA.SalesRelationModel.RootNode.BizObj.PK);
			var opportunityANode = opportunityA.SalesRelationModel.MasterNode;
			AssertContainsExactElementsInAnyOrder("Should not include opportunityC as a child node of opportunityA",
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[] { inquiry },
				opportunityANode.ChildNodes.Select(node => node.BizObj).Cast<BusinessObject>());

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			opportunityA.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			AssertContainsExactElementsInAnyOrder("Should not include opportunityC as a child node of opportunityA",
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[] { inquiry, communication },
				opportunityANode.ChildNodes.Select(node => node.BizObj).Cast<BusinessObject>());
		}

		public void TestChildNodes_DoNotIncludeDuplicateChild()
		{
			var opportunityA = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunityB = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunityC = Factory.NewWithValidTestData<OrgOpportunity>();

			opportunityA.RelatedChildActivityPivotCollection.AddNewPivot(opportunityB);
			opportunityA.RelatedChildActivityPivotCollection.AddNewPivot(opportunityC);
			opportunityA.RelatedChildActivityPivotCollection.AddNewPivot(opportunityB);
			opportunityB.RelatedChildActivityPivotCollection.AddNewPivot(opportunityC);

			var opportunityANode = opportunityA.SalesRelationModel.MasterNode;
			AssertContainsExactElementsInAnyOrder("Should not include opportunityB and opportunityC as the child nodes of opportunityA",
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[] { opportunityB, opportunityC },
				opportunityANode.ChildNodes.Select(node => node.BizObj).Cast<BusinessObject>());
		}

		public void TestChildNodes_ConcurrencyError()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var opportunity1A = factory1.NewWithValidTestData<OrgOpportunity>();
			var opportunity1B = factory1.NewWithValidTestData<OrgOpportunity>();
			var opportunity1C = factory1.NewWithValidTestData<OrgOpportunity>();
			opportunity1A.RelatedChildActivityPivotCollection.AddNewPivot(opportunity1B);
			opportunity1A.RelatedChildActivityPivotCollection.AddNewPivot(opportunity1C);
			factory1.Save();
			{
				var opportunity2A = factory2.Load<OrgOpportunity>(opportunity1A.PK);
				var opportunity2B = factory2.Load<OrgOpportunity>(opportunity1B.PK);
				var opportunity2C = factory2.Load<OrgOpportunity>(opportunity1C.PK);
				opportunity1A.RelatedChildActivityPivotCollection.RemoveActivity(opportunity1C);
				opportunity1B.RelatedChildActivityPivotCollection.AddNewPivot(opportunity1C);
				opportunity2A.RelatedChildActivityPivotCollection.RemoveActivity(opportunity2C);
				opportunity2B.RelatedChildActivityPivotCollection.AddNewPivot(opportunity2C);
				factory1.Save();
				AssertExceptionThrown<ZSaveConcurrencyException>(factory2.Save);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);

			base.SetUp();
		}

		protected override IRelatableActivity GetNewBizObjForTest()
		{
			var bizObj = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			return bizObj;
		}

		protected override ZTreeModel<IRelatableActivity> GetNewTreeModelForTest()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			return new SalesRelationModel(opportunity);
		}

		protected override IRelatableActivity CreateChild(IRelatableActivity parent)
		{
			var child = (IRelatableActivity)Factory.NewWithValidTestData<OrgOpportunity>();
			child.RelatedParentActivityPivotCollection.AddActivity(parent);
			Factory.Save();

			return child;
		}

		protected override IEnumerable<IRelatableActivity> GetChildren(IRelatableActivity parent)
		{
			return parent.RelatedChildActivityPivotCollection.Activities;
		}

		protected override IRelatableActivity CreateParent(IRelatableActivity child)
		{
			var parent = (IRelatableActivity)Factory.NewWithValidTestData<OrgOpportunity>();
			parent.RelatedChildActivityPivotCollection.AddActivity(child);
			Factory.Save();

			return parent;
		}

		protected override IRelatableActivity GetParent(IRelatableActivity child)
		{
			return child.RelatedParentActivityPivotCollection.Activities.FirstOrDefault();
		}

		#endregion
	}
}

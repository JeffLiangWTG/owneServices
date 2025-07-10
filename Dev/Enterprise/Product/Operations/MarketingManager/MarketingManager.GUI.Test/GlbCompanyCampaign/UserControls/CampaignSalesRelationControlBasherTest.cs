using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MarketingManager.ServiceTask;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignSalesRelationControlFormForTest))]
	sealed class CampaignSalesRelationControlBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			return new CampaignSalesRelationControlFormForTest(campaign);
		}

		public void TestNewOpportunityCreatedManually_DRMMasterCampaign()
		{
			AssertNewOpportunityCreatedManuallyMasterCampaign(CampaignTypeList.Codes.DripMarketing);
		}

		public void TestNewOpportunityCreatedManually_INSMasterCampaign()
		{
			AssertNewOpportunityCreatedManuallyMasterCampaign(CampaignTypeList.Codes.InsideSales);
		}

		public void AssertNewOpportunityCreatedManuallyMasterCampaign(string masterCampaignType)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = masterCampaignType;
			masterCampaign.G0_CampaignName = "Master Campaign";
			Factory.Save();

			using (var form = new CampaignSalesRelationControlFormForTest(masterCampaign))
			{
				form.Show();
				Application.DoEvents();

				var campaignSalesRelationControl = form.CampaignSalesRelationControl;
				var opportunityButton = campaignSalesRelationControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().First(item => item.Text == "Opportunity");
				opportunityButton.PerformClick();

				campaignSalesRelationControl.NewOpportunity.P8_OH = org.PK;
				campaignSalesRelationControl.InvokeNewFormSavedForTest();
				AssertEquals(masterCampaign.PK, campaignSalesRelationControl.NewOpportunity.P8_G0);
			}
		}

		public void TestNewOpportunityCreatedManually_BroadcastTouchCampaign()
		{
			AssertNewOpportunityCreatedManuallyTouchCampaign(CampaignTypeList.Codes.DripMarketing, CampaignTypeList.Codes.Broadcast);
		}

		public void TestNewOpportunityCreatedManually_SurveyTouchCampaign()
		{
			AssertNewOpportunityCreatedManuallyTouchCampaign(CampaignTypeList.Codes.DripMarketing, CampaignTypeList.Codes.Survey);
		}

		public void TestNewOpportunityCreatedManually_VotingTouchCampaign()
		{
			AssertNewOpportunityCreatedManuallyTouchCampaign(CampaignTypeList.Codes.DripMarketing, CampaignTypeList.Codes.Voting);
		}

		public void TestNewOpportunityCreatedManually_PreApproachEmailTouchCampaign()
		{
			AssertNewOpportunityCreatedManuallyTouchCampaign(CampaignTypeList.Codes.InsideSales, InsideSalesTouchTypeList.Codes.PreApproachEmail);
		}

		public void TestNewOpportunityCreatedManually_OpportunityCreationTouchCampaign()
		{
			AssertNewOpportunityCreatedManuallyTouchCampaign(CampaignTypeList.Codes.InsideSales, InsideSalesTouchTypeList.Codes.OpportunityCreation);
		}

		public void AssertNewOpportunityCreatedManuallyTouchCampaign(string masterCampaignType, string touchType)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = masterCampaignType;
			masterCampaign.G0_CampaignName = "Master Campaign";

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_CampaignName = "Test Campaign";
			touch.G0_BroadcastVoteSurveyExam = touchType;
			Factory.Save();

			using (var form = new CampaignSalesRelationControlFormForTest(touch))
			{
				form.Show();
				Application.DoEvents();

				var campaignSalesRelationControl = form.CampaignSalesRelationControl;
				var opportunityButton = campaignSalesRelationControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().First(item => item.Text == "Opportunity");
				opportunityButton.PerformClick();

				campaignSalesRelationControl.NewOpportunity.P8_OH = org.PK;
				campaignSalesRelationControl.InvokeNewFormSavedForTest();
				AssertEquals(touch.PK, campaignSalesRelationControl.NewOpportunity.P8_G0);
			}
		}

		public void TestDetachTouchCampaignIsNotEditable()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			masterCampaign.G0_CampaignName = "Master Campaign";

			var touch = masterCampaign.AllTouches.AddNew();
			touch.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch.G0_CampaignName = "Touch Campaign";
			Factory.Save();

			using (var form = new CampaignSalesRelationControlFormForTest(touch))
			{
				form.Show();
				Application.DoEvents();

				Env.Security.SalesRelationsDelete.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.CampaignSalesRelationControl.Tree.AllNodes.First(n => ((SalesRelationNode)n.Tag).BizObj is GlbCompanyCampaign campaign && campaign.IsTouchCampaign).IsSelected = true;
				form.CampaignSalesRelationControl.DetachToolStripButton.PerformClick();

				AssertMultilineASCIIEquals(@"Unable to detach the following Relatable Activities:
• Touch Campaign", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDetachOpportunityCreatedViaServiceTaskFromTouchCampaign()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;
			masterCampaign.G0_CampaignName = "Master Campaign";

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_CampaignName = "Test Campaign";
			touch.HtmlDocumentBlob = ZBlob.FromAscii("Test Html");
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_G0_Master = masterCampaign.PK;
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			masterCampaign.AllTouches.Add(touch);

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = Business.TrackingStatusCodes.Codes.OPQ;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			var serviceTask = new CampaignItemScheduleProcessorServiceTask { ServiceLogger = new LoggerForTest() };
			serviceTask.RunTask();

			using (var form = new CampaignSalesRelationControlFormForTest(touch))
			{
				form.Show();
				Application.DoEvents();

				Env.Security.SalesRelationsDelete.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				AssertEquals(1, form.CampaignSalesRelationControl.Tree.AllNodes.Count(n => ((SalesRelationNode)n.Tag).BizObj is OrgOpportunity));

				form.CampaignSalesRelationControl.Tree.AllNodes.First(n => ((SalesRelationNode)n.Tag).BizObj is OrgOpportunity).IsSelected = true;
				form.CampaignSalesRelationControl.DetachToolStripButton.PerformClick();

				AssertEquals(0, form.CampaignSalesRelationControl.Tree.AllNodes.Count(n => ((SalesRelationNode)n.Tag).BizObj is OrgOpportunity));
			}
		}

		public void TestDragNDropTouchCampaignIsNotEditable()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			masterCampaign.G0_CampaignName = "Master Campaign";

			var touch = masterCampaign.AllTouches.AddNew();
			touch.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch.G0_CampaignName = "Test with Touch campaign - Touch One";

			var touch2 = masterCampaign.AllTouches.AddNew();
			touch2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch2.G0_CampaignName = "Test with Touch campaign - Touch Two";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "AA@gmail.com";
			contact.OC_ContactName = "AA";

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			opportunity.P8_G0 = touch.PK;
			opportunity.P8_GC = touch.G0_GC;
			opportunity.P8_OpportunityDescription = "Opportunity One";
			opportunity.RelatedParentActivityPivotCollection.AddNewPivot(touch);

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity2.P8_OH = org.PK;
			opportunity2.P8_G0 = touch.PK;
			opportunity2.P8_GC = touch.G0_GC;
			opportunity2.P8_OpportunityDescription = "Opportunity Two";
			opportunity2.RelatedParentActivityPivotCollection.AddNewPivot(touch);
			Factory.Save();

			using (var tree = new SalesRelationTreeForTest())
			{
				var masterCampaignNode = tree.GetRelationNode(masterCampaign);
				var touchCampaignNode = tree.GetRelationNode(touch);
				var touchCampaignNode2 = tree.GetRelationNode(touch2);
				var opportunityNode = tree.GetRelationNode(opportunity);
				var opportunityNode2 = tree.GetRelationNode(opportunity2);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				AssertEquals("Precondition: Parent of touch campaign node 2 should be the master campaign", masterCampaignNode.Summary, touchCampaignNode2.ParentNode.BizObj.Summary);
				tree.OnNodeDragDrop_Inside_Exposed(new List<ZNode<IRelatableActivity>> { touchCampaignNode2 }, masterCampaignNode);
				AssertMultilineASCIIEquals("Error message should be displayed",
@"Unable to move the following Relatable Activities:
• Test with Touch campaign - Touch Two", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not move touch campaign node 2", masterCampaignNode.Summary, touchCampaignNode2.ParentNode.BizObj.Summary);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				AssertEquals("Precondition: Parent of the opportunity node should be the touch campaign node", touchCampaignNode.Summary, opportunityNode.ParentNode.BizObj.Summary);
				tree.OnNodeDragDrop_Inside_Exposed(new List<ZNode<IRelatableActivity>> { opportunityNode }, masterCampaignNode);
				AssertEquals("Error message should not be displayed", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should move the opportunity node", masterCampaignNode.Summary, opportunityNode.ParentNode.BizObj.Summary);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				AssertEquals("Precondition: Parent of the touch campaign node should be the master campaign", masterCampaignNode.Summary, touchCampaignNode.ParentNode.BizObj.Summary);
				AssertEquals("Precondition: Parent of touch campaign node 2 should be the master campaign", masterCampaignNode.Summary, touchCampaignNode2.ParentNode.BizObj.Summary);
				tree.OnNodeDragDrop_Inside_Exposed(new List<ZNode<IRelatableActivity>> { touchCampaignNode, touchCampaignNode2 }, masterCampaignNode);
				AssertMultilineASCIIEquals("Error message should be displayed",
@"Unable to move the following Relatable Activities:
• Test with Touch campaign - Touch One
• Test with Touch campaign - Touch Two", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not change parent for the touch campaign", masterCampaignNode.Summary, touchCampaignNode.ParentNode.BizObj.Summary);
				AssertEquals("Should not change parent for touch campaign 2", masterCampaignNode.Summary, touchCampaignNode2.ParentNode.BizObj.Summary);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				AssertEquals("Precondition: Parent of opportunity node 2 should be the touch campaign node", touchCampaignNode.Summary, opportunityNode2.ParentNode.BizObj.Summary);
				tree.OnNodeDragDrop_Inside_Exposed(new List<ZNode<IRelatableActivity>> { opportunityNode2 }, touchCampaignNode2);
				AssertEquals("Error message should not be displayed", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should set drop node as parent node", touchCampaignNode2.Summary, opportunityNode2.ParentNode.BizObj.Summary);

				AssertEquals("Precondition: Parent of the touch campaign node should be the master campaign", masterCampaignNode.Summary, touchCampaignNode.ParentNode.BizObj.Summary);
				tree.OnNodeDragDrop_Before_Exposed(touchCampaignNode, masterCampaignNode);
				AssertMultilineASCIIEquals("Error message should be displayed",
@"Unable to move the following Relatable Activities:
• Test with Touch campaign - Touch One", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not change parent for the touch campaign", masterCampaignNode.Summary, touchCampaignNode.ParentNode.BizObj.Summary);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				AssertEquals("Precondition: Parent of master campaign should not be set", null, masterCampaignNode.ParentNode);
				tree.OnNodeDragDrop_Before_Exposed(opportunityNode, masterCampaignNode);
				AssertEquals("Error message should not be displayed", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should set parent of master campaign to opportunity", opportunityNode.Summary, masterCampaignNode.ParentNode.BizObj.Summary);
			}
		}

		#region Classes

		public class CampaignSalesRelationControlForTest : CampaignSalesRelationControl
		{
			internal new ZToolStripDropDownButton NewToolStripDropDownButton => base.NewToolStripDropDownButton;

			internal new void InvokeNewFormSavedForTest() => base.InvokeNewFormSavedForTest();

			internal OrgOpportunity NewOpportunity => (OrgOpportunity)NewFormEntityBusinessEntity;

			public void OnDragDrop_Exposed(DragEventArgs e)
			{
				base.OnDragDrop(e);
			}
		}

		public class CampaignSalesRelationControlFormForTest : ZForm
		{
			public CampaignSalesRelationControlFormForTest(GlbCompanyCampaign campaign)
				: base(campaign)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1200, 768);

				Controls.Add(CampaignSalesRelationControl);
				BindingSource.SetBindingMember(CampaignSalesRelationControl, "SalesRelationModel");
				CaptionRenderingEnabled = true;
			}

			internal CampaignSalesRelationControlForTest CampaignSalesRelationControl = new CampaignSalesRelationControlForTest();
		}

		public class SalesRelationTreeForTest : SalesRelationTree
		{
			public void OnNodeDragDrop_Before_Exposed<T>(ZNode<T> node, ZNode<T> dropNode) where T : class, IBusiness
			{
				base.OnNodeDragDrop_Before(node, dropNode);
			}

			public void OnNodeDragDrop_Inside_Exposed<T>(List<ZNode<T>> nodes, ZNode<T> dropNode) where T : class, IBusiness
			{
				base.OnNodeDragDrop_Inside(nodes, dropNode);
			}

			public SalesRelationNode GetRelationNode(IBusiness bizo)
			{
				var relationItem = bizo as ISalesRelationActivity;
				var model = relationItem.SalesRelationModel as SalesRelationModel;
				return model.FindNode(relationItem) as SalesRelationNode;
			}
		}
		#endregion

		#endregion
	}
}

using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Aga.Controls.Tree;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SalesRelationControlTest : TestCaseWithFactory
	{
		#region New Activity

		public void TestSetupNewActivityButton_AnyDirectionRules()
		{
			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);

			var opportunity = Factory.New<OrgOpportunity>();
			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				AssertArrayEqualsByElements(
					new[] { "Inquiry", "Campaign", "Opportunity", "Quotation", "One Off Quote", "Project" },
					form.SalesRelationControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		public void TestSetupNewActivityButton_OnlyAddForTypesThatMeetDirectionRules()
		{
			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(RelatableActivityTypeList.Codes.OpportunityManager, RelatableActivityTypeList.Codes.OpportunityManager);
			directionRules.AddNewRule(RelatableActivityTypeList.Codes.OpportunityManager, RelatableActivityTypeList.Codes.Quotations);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);

			var opportunity = Factory.New<OrgOpportunity>();
			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				AssertArrayEqualsByElements(
					new[] { "Opportunity", "Quotation" },
					form.SalesRelationControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		[RequiresSTA]
		public void TestSetupNewActivityButton_NoButtonsWhenDoesNotMeetAnyDirectionRules()
		{
			var directionRules = new SalesRelationDirectionRuleCollection();
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);

			var opportunity = Factory.New<OrgOpportunity>();
			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				AssertArrayEqualsByElements(
					Array.Empty<string>(),
					form.SalesRelationControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		[RequiresSTA]
		public void TestSetupNewActivityButton_QuoteSubButtons()
		{
			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);

			BusinessObject quote = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			using (var form = new SalesRelationControlFormForTest(((ISalesRelationActivity)quote).SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				var quoteButton = form.SalesRelationControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().First(item => item.Text == "Quotation") as ToolStripMenuItem;
				AssertNotNull(quoteButton);
				AssertArrayEqualsByElements(
					new[] { "New For Same Client", "Amendment For Same Client", "Copy For Same Client" },
					quoteButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}

			var opportunity = Factory.New<OrgOpportunity>();
			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				var quoteButton = form.SalesRelationControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().First(item => item.Text == "Quotation") as ToolStripMenuItem;
				AssertNotNull(quoteButton);
				AssertArrayEqualsByElements(
					Array.Empty<string>(),
					quoteButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		public void TestShowEditForm()
		{
			var rootInquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var childInquiry = Factory.NewWithValidTestData<SalesEnquiry>() as ISalesRelationActivity;
			var childCampaignItem = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IGlbCompanyCampaignItem)));
			childCampaignItem[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			childCampaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = ZGuid.NewZGuid();
			Factory.Save();
			rootInquiry.RelatedChildActivityPivotCollection.AddActivity(opportunity);
			opportunity.RelatedChildActivityPivotCollection.AddActivity(childInquiry);
			childInquiry.RelatedChildActivityPivotCollection.AddNewPivot(childCampaignItem as ISalesRelationActivity); // to prevent adding the header (i.e. campaign) as related activity
			Factory.Save();

			using (var form = new SalesRelationControlFormForTest(rootInquiry.SalesRelationModel))
			{
				form.Show();

				form.SalesRelationControl.ShowEditForm_Exposed(rootInquiry);
				AssertNull("root inquiry (i.e. master node) should not bring up edit form", ZFormModaliser.LastFormShownForTest);

				form.SalesRelationControl.ShowEditForm_Exposed(opportunity);
				var actualForm = (ZForm)form.SalesRelationControl.LastController_ForTestOnly.LastShownForm;
				AssertType(typeof(OpportunityForm), actualForm);
				actualForm.Close();

				form.SalesRelationControl.ShowEditForm_Exposed(childInquiry);
				actualForm = (ZForm)form.SalesRelationControl.LastController_ForTestOnly.LastShownForm;
				AssertType(typeof(SalesEnquiryForm), actualForm);
				actualForm.Close();

				form.SalesRelationControl.ShowEditForm_Exposed(childCampaignItem);
				actualForm = (ZForm)form.SalesRelationControl.LastController_ForTestOnly.LastShownForm;
				AssertEquals("GlbCompanyCampaignForm", actualForm.GetType().Name);
				var mainTabControl = actualForm.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				AssertNotNull("MainTabControl", mainTabControl);
				AssertEquals("TrackingTabPage", mainTabControl.SelectedTab.Name);
				actualForm.Close();
			}
		}

		public void TestNewActivityButton()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			Factory.Save();
			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				Env.Security.SalesRelationsNew.IsAllowed = true;
				form.SalesRelationControl.NewToolStripDropDownButton.DropDownItems[0].PerformClick();
				AssertType(typeof(SalesEnquiryForm), ZFormModaliser.LastFormShownForTest);
				var formBizObj = ((SalesEnquiryForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
				AssertType(typeof(SalesEnquiry), formBizObj);
				AssertEquals("Should have imported org from parent opportunity", org.PK, ((SalesEnquiry)formBizObj).O1_OH_ConvertedToQualifiedLead);

				ZFormModaliser.LastFormShownForTest = null;

				Env.Security.SalesRelationsNew.IsAllowed = false;
				form.SalesRelationControl.NewToolStripDropDownButton.DropDownItems[0].PerformClick();
				AssertType(null, ZFormModaliser.LastFormShownForTest);
				AssertEquals(Env.Security.SalesRelationsNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.SalesRelationsNew.IsAllowed = true;
				Env.Security.InquiryManagerNew.IsAllowed = false;
				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
					{
						form.SalesRelationControl.NewToolStripDropDownButton.DropDownItems[0].PerformClick();
						AssertEquals(Env.Security.InquiryManagerNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					});
				AssertType(null, ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestNewQuoteButtonForOpportunity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			Factory.Save();

			var generateQuoteController = new MockGenerateQuoteForSalesValueAssociatedEntityControllerForTest();
			using (ObjectFactory.Substitute<IGenerateQuoteForSalesValueAssociatedEntityController>(generateQuoteController))
			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				var quoteButton = form.SalesRelationControl.NewToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().First(item => item.Text == "Quotation") as ToolStripMenuItem;
				AssertNotNull(quoteButton);

				quoteButton.PerformClick();

				AssertEquals(form, generateQuoteController.ParentModalForm);
				AssertEquals(opportunity, generateQuoteController.EntityCalled);
			}
		}

		#endregion

		#region Attach

		[RequiresSTA]
		public void TestSetupAttachActivityButton_AnyDirectionRules()
		{
			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);

			var opportunity = Factory.New<OrgOpportunity>();
			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				AssertArrayEqualsByElements(
					new[] { "Inquiry", "Campaign", "Opportunity", "Quotation", "One Off Quote", "Project" },
					form.SalesRelationControl.AttachToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		public void TestSetupAttachActivityButton_OnlyAddForTypesThatMeetDirectionRules()
		{
			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(RelatableActivityTypeList.Codes.OpportunityManager, RelatableActivityTypeList.Codes.OpportunityManager);
			directionRules.AddNewRule(RelatableActivityTypeList.Codes.OpportunityManager, RelatableActivityTypeList.Codes.Quotations);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);

			var opportunity = Factory.New<OrgOpportunity>();
			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				AssertArrayEqualsByElements(
					new[] { "Opportunity", "Quotation" },
					form.SalesRelationControl.AttachToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		[RequiresSTA]
		public void TestSetupAttachActivityButton_NoButtonsWhenDoesNotMeetAnyDirectionRules()
		{
			var directionRules = new SalesRelationDirectionRuleCollection();
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);

			var opportunity = Factory.New<OrgOpportunity>();
			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				AssertArrayEqualsByElements(
					Array.Empty<string>(),
					form.SalesRelationControl.AttachToolStripDropDownButton.DropDownItems.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		[RequiresSTA]
		public void TestAttachActivityButton()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				Env.Security.SalesRelationsNew.IsAllowed = true;
				form.SalesRelationControl.AttachToolStripDropDownButton.DropDownItems[0].PerformClick();
				AssertType(typeof(EmbeddedModulePopup), ZFormModaliser.LastFormShownDialogForTest);
				using (var modulePopup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(SalesRelationControl.AddRelatedActivityModuleDecisionProvider), modulePopup.EmbeddedModulePopupOKButtonStrategy);
				}

				ZFormModaliser.LastFormShownDialogForTest = null;
				Env.Security.SalesRelationsNew.IsAllowed = false;
				form.SalesRelationControl.AttachToolStripDropDownButton.DropDownItems[0].PerformClick();
				AssertType(null, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(Env.Security.SalesRelationsNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Detach

		public void TestDetachActivityButton()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunityForAttachDetachTest>();
			var childInquiry = Factory.NewWithValidTestData<SalesEnquiry>() as ISalesRelationActivity;
			Factory.Save();
			opportunity.RelatedChildActivityPivotCollection.AddActivity(childInquiry);
			Factory.Save();

			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();

				Env.Security.SalesRelationsDelete.IsAllowed = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.SalesRelationControl.Tree.AllNodes.First(n => ((SalesRelationNode)n.Tag).BizObj is OrgOpportunityForAttachDetachTest).IsSelected = true;
				form.SalesRelationControl.DetachToolStripButton.PerformClick();

				AssertEquals("Child Detach not called on Opportunity", 0, opportunity.NumImportChildInfoOnDetachCalled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.SalesRelationControl.Tree.AllNodes.First(n => ((SalesRelationNode)n.Tag).BizObj is SalesEnquiry).IsSelected = true;
				form.SalesRelationControl.DetachToolStripButton.PerformClick();

				AssertEquals("Child Detach called on Opportunity", 1, opportunity.NumImportChildInfoOnDetachCalled);
			}
		}

		public void TestDetachActivityButton_DisplayMessageIfNotEditable()
		{
			var masterCampaign = Factory.New<IGlbCompanyCampaign>();
			masterCampaign.G0_CampaignName = "Test Master Campaign";
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touchCampaign1 = Factory.New<IGlbCompanyCampaign>();
			touchCampaign1.G0_CampaignName = "Test Touch Campaign 1";
			touchCampaign1.G0_G0_Master = masterCampaign.PK;

			var touchCampaign2 = Factory.New<IGlbCompanyCampaign>();
			touchCampaign2.G0_CampaignName = "Test Touch Campaign 2";
			touchCampaign2.G0_G0_Master = masterCampaign.PK;

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			(masterCampaign as ISalesRelationActivity).RelatedChildActivityPivotCollection.AddNewPivot(opportunity);

			Factory.Save();

			using (var form = new SalesRelationControlFormForTest(new SalesRelationModel(masterCampaign as ISalesRelationActivity)))
			{
				form.Show();
				Application.DoEvents();
				Env.Security.SalesRelationsDelete.IsAllowed = true;
				var nodes = form.SalesRelationControl.Tree.AllNodes;
				var detachButton = form.SalesRelationControl.DetachToolStripButton;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				detachButton.PerformClick();

				AssertEquals("Only info message should be displayed when no items selected", 2, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert("Empty message", UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
				AssertEquals("Please select an item in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should display info message when no items selected", UnitTestUserNotification.Instance.LastMessage.WasInformation);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == touchCampaign1.PK).IsSelected = true;
				detachButton.PerformClick();

				AssertEquals("Only info message should be displayed when detaching touch campaign", 2, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert("Empty message", UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
				AssertMultilineASCIIEquals(@"Unable to detach the following Relatable Activities:
• Test Touch Campaign 1", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Detaching touch campaign should display info message", UnitTestUserNotification.Instance.LastMessage.WasInformation);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == touchCampaign1.PK).IsSelected = false;
				nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == masterCampaign.PK).IsSelected = true;
				detachButton.PerformClick();

				AssertEquals("Only info message should be displayed when detaching root node", 2, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert("Empty message", UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
				AssertMultilineASCIIEquals(@"Unable to detach the following Relatable Activities:
• Test Master Campaign", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Detaching root node should display info message", UnitTestUserNotification.Instance.LastMessage.WasInformation);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == touchCampaign1.PK).IsSelected = true;
				nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == touchCampaign2.PK).IsSelected = true;
				detachButton.PerformClick();

				AssertEquals("Only info message should be displayed when detaching invalid nodes", 2, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert("Empty message", UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
				AssertMultilineASCIIEquals(@"Unable to detach the following Relatable Activities:
• Test Master Campaign
• Test Touch Campaign 1
• Test Touch Campaign 2", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Detaching invalid nodes should display info message", UnitTestUserNotification.Instance.LastMessage.WasInformation);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == opportunity.PK).IsSelected = true;
				detachButton.PerformClick();

				AssertEquals("Only prompt should be displayed when detaching at least one valid node and clicking Cancel", 2, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert("Empty message", UnitTestUserNotification.Instance.PreviousMessages[1].WasNone);
				AssertEquals("Detach the selected Relatable Activities?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Prompt to detach valid nodes should be Question", UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				detachButton.PerformClick();

				AssertEquals("Prompt and info message should be displayed when attempting to detach valid and invalid nodes", 3, UnitTestUserNotification.Instance.PreviousMessages.Length);
				Assert("Empty message", UnitTestUserNotification.Instance.PreviousMessages[2].WasNone);
				AssertEquals("Detach the selected Relatable Activities?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				Assert("Prompt to detach valid nodes should be Question", UnitTestUserNotification.Instance.PreviousMessages[1].WasQuestion);

				AssertMultilineASCIIEquals(@"Unable to detach the following Relatable Activities:
• Test Master Campaign
• Test Touch Campaign 1
• Test Touch Campaign 2", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Detaching invalid nodes should display info message", UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		#endregion

		#region DragDrop

		public void TestDragDropInside()
		{
			var opp1 = Factory.NewWithValidTestData<OrgOpportunityForAttachDetachTest>();
			var opp2 = Factory.NewWithValidTestData<OrgOpportunityForAttachDetachTest>();
			var opp3 = Factory.NewWithValidTestData<OrgOpportunityForAttachDetachTest>();
			Factory.Save();

			var opp1Node = opp1.SalesRelationModel.MasterNode;
			opp1Node.AddNewChild(opp2);
			opp1Node.ChildNodes.First().AddNewChild(opp3);

			using (var form = new SalesRelationControlFormForTest(opp1.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();
				var salesRelationTree = (SalesRelationTree)form.SalesRelationControl.Tree;
				var onDragDrop = typeof(SalesRelationTree).GetMethod("OnDragDrop", BindingFlags.Instance | BindingFlags.NonPublic);

				AssertEquals("Precondition: Should not have attached opportunity 3 to opportunity 1 yet", 0, opp1.NumImportChildInfoOnAttachCalled);
				AssertEquals("Precondition: Should not have detached opportunity 3 from opportunity 2 yet", 0, opp2.NumImportChildInfoOnDetachCalled);

				var nodes = salesRelationTree.AllNodes;
				var dropPosition = new DropPosition() { Position = NodePosition.Inside };
				dropPosition.Node = nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == opp1.PK);
				salesRelationTree.DropPosition = dropPosition;

				var dataObj = new DataObject();
				dataObj.SetData(new TreeNodeAdv[] { nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == opp3.PK) });
				var dragEvent = new DragEventArgs(dataObj, 0, 1, 1, DragDropEffects.Move, DragDropEffects.Move);
				onDragDrop.Invoke(salesRelationTree, new object[] { dragEvent });

				AssertEquals("Should have attached opportunity 3 to opportunity 1", 1, opp1.NumImportChildInfoOnAttachCalled);
				AssertEquals("Should have detached opportunity 3 from opportunity 2", 1, opp2.NumImportChildInfoOnDetachCalled);

				AssertEquals("Opportunity 2 attach should not be called", 0, opp2.NumImportChildInfoOnAttachCalled);
				AssertEquals("Opportunity 3 attach should not be called", 0, opp3.NumImportChildInfoOnAttachCalled);
				AssertEquals("Opportunity 1 detach should not be called", 0, opp1.NumImportChildInfoOnDetachCalled);
				AssertEquals("Opportunity 3 detach should not be called", 0, opp3.NumImportChildInfoOnDetachCalled);
			}
		}

		[RequiresSTA]
		public void TestDragDropInside_SwapWithParent()
		{
			var opp1 = Factory.NewWithValidTestData<OrgOpportunityForAttachDetachTest>();
			var opp2 = Factory.NewWithValidTestData<OrgOpportunityForAttachDetachTest>();
			var opp3 = Factory.NewWithValidTestData<OrgOpportunityForAttachDetachTest>();
			Factory.Save();

			var opp1Node = opp1.SalesRelationModel.MasterNode;
			opp1Node.AddNewChild(opp2);
			opp1Node.ChildNodes.First().AddNewChild(opp3);

			using (var form = new SalesRelationControlFormForTest(opp1.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();
				var salesRelationTree = (SalesRelationTree)form.SalesRelationControl.Tree;
				var onDragDrop = typeof(SalesRelationTree).GetMethod("OnDragDrop", BindingFlags.Instance | BindingFlags.NonPublic);

				AssertEquals("Precondition: Should not have attached opportunity 3 to opportunity 1 yet", 0, opp1.NumImportChildInfoOnAttachCalled);
				AssertEquals("Precondition: Should not have attached opportunity 2 to opportunity 3 yet", 0, opp3.NumImportChildInfoOnAttachCalled);
				AssertEquals("Precondition: Should not have detached opportunity 3 from opportunity 2 yet", 0, opp2.NumImportChildInfoOnDetachCalled);
				AssertEquals("Precondition: Should not have detached opportunity 2 from opportunity 1 yet", 0, opp1.NumImportChildInfoOnDetachCalled);

				var nodes = salesRelationTree.AllNodes;
				var dropPosition = new DropPosition() { Position = NodePosition.Inside };
				dropPosition.Node = nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == opp2.PK);
				salesRelationTree.DropPosition = dropPosition;

				var dataObj = new DataObject();
				dataObj.SetData(new TreeNodeAdv[] { nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == opp3.PK) });
				var dragEvent = new DragEventArgs(dataObj, 0, 1, 1, DragDropEffects.Move, DragDropEffects.Move);
				onDragDrop.Invoke(salesRelationTree, new object[] { dragEvent });

				AssertEquals("Should have attached opportunity 3 to opportunity 1", 1, opp1.NumImportChildInfoOnAttachCalled);
				AssertEquals("Should have attached opportunity 2 to opportunity 3", 1, opp3.NumImportChildInfoOnAttachCalled);
				AssertEquals("Should have detached opportunity 3 from opportunity 2", 1, opp2.NumImportChildInfoOnDetachCalled);
				AssertEquals("Should have detached opportunity 2 from opportunity 1", 1, opp1.NumImportChildInfoOnDetachCalled);

				AssertEquals("Opportunity 2 attach should not be called", 0, opp2.NumImportChildInfoOnAttachCalled);
				AssertEquals("Opportunity 3 detach should not be called", 0, opp3.NumImportChildInfoOnDetachCalled);
			}
		}

		[RequiresSTA]
		public void TestDragDropBefore()
		{
			var opp1 = Factory.NewWithValidTestData<OrgOpportunityForAttachDetachTest>();
			var opp2 = Factory.NewWithValidTestData<OrgOpportunityForAttachDetachTest>();
			var opp3 = Factory.NewWithValidTestData<OrgOpportunityForAttachDetachTest>();
			Factory.Save();

			var opp1Node = opp1.SalesRelationModel.MasterNode;
			opp1Node.AddNewChild(opp2);
			opp1Node.ChildNodes.First().AddNewChild(opp3);

			using (var form = new SalesRelationControlFormForTest(opp1.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();
				var salesRelationTree = (SalesRelationTree)form.SalesRelationControl.Tree;
				var onDragDrop = typeof(SalesRelationTree).GetMethod("OnDragDrop", BindingFlags.Instance | BindingFlags.NonPublic);

				AssertEquals("Precondition: Should not have attached opportunity 1 to opportunity 3 yet", 0, opp3.NumImportChildInfoOnAttachCalled);
				AssertEquals("Precondition: Should not have detached opportunity 3 from opportunity 2 yet", 0, opp2.NumImportChildInfoOnDetachCalled);

				var nodes = salesRelationTree.AllNodes;
				var dropPosition = new DropPosition() { Position = NodePosition.Before };
				dropPosition.Node = nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == opp1.PK);
				salesRelationTree.DropPosition = dropPosition;

				var dataObj = new DataObject();
				dataObj.SetData(new TreeNodeAdv[] { nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == opp3.PK) });
				var dragEvent = new DragEventArgs(dataObj, 0, 1, 1, DragDropEffects.Move, DragDropEffects.Move);
				onDragDrop.Invoke(salesRelationTree, new object[] { dragEvent });

				AssertEquals("Should have attached opportunity 1 to opportunity 3", 1, opp3.NumImportChildInfoOnAttachCalled);
				AssertEquals("Should have detached opportunity 3 from opportunity 2", 1, opp2.NumImportChildInfoOnDetachCalled);

				AssertEquals("Opportunity 1 attach should not be called", 0, opp1.NumImportChildInfoOnAttachCalled);
				AssertEquals("Opportunity 2 attach should not be called", 0, opp2.NumImportChildInfoOnAttachCalled);
				AssertEquals("Opportunity 1 detach should not be called", 0, opp1.NumImportChildInfoOnDetachCalled);
				AssertEquals("Opportunity 3 detach should not be called", 0, opp3.NumImportChildInfoOnDetachCalled);
			}
		}

		public void TestDragDrop_NoErrorAndRevertNodesIfUnsuccessful()
		{
			var parentOpp = Factory.NewWithValidTestData<OrgOpportunity>();
			var childOpp = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			parentOpp.RelatedChildActivityPivotCollection.AddNewPivot(childOpp);
			parentOpp.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			Factory.Save();

			using (var form = new SalesRelationControlFormForTest(parentOpp.SalesRelationModel))
			{
				form.Show();
				Application.DoEvents();
				var salesRelationTree = (SalesRelationTree)form.SalesRelationControl.Tree;
				var onDragDrop = typeof(SalesRelationTree).GetMethod("OnDragDrop", BindingFlags.Instance | BindingFlags.NonPublic);

				var nodes = salesRelationTree.AllNodes;
				var parentOppNode = nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == parentOpp.PK);
				var childOppNode = nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == childOpp.PK);
				var inquiryNode = nodes.First(n => ((SalesRelationNode)n.Tag).BizObj.PK == inquiry.PK);
				AssertEquals("Precondition: Parent and child opportunity relation should be set", parentOppNode, childOppNode.Parent);
				AssertEquals("Precondition: Inquiry should be child of parent opportunity", parentOppNode, inquiryNode.Parent);

				salesRelationTree.DropPosition = new DropPosition() { Position = NodePosition.Inside, Node = parentOppNode };
				var dataObj = new DataObject();
				dataObj.SetData(new TreeNodeAdv[] { inquiryNode });
				var dragEvent = new DragEventArgs(dataObj, 0, 1, 1, DragDropEffects.Move, DragDropEffects.Move);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertNoExceptionThrown("Infinite loop should not occur when reverting nodes", () => onDragDrop.Invoke(salesRelationTree, new object[] { dragEvent }));
				AssertEquals("Parent and child opportunity relation should still be set as the drag operation was unsuccessful", parentOppNode, childOppNode.Parent);
				AssertEquals("Inquiry should still be child of parent opportunity as the drag operation was unsuccessful", parentOppNode, inquiryNode.Parent);

				salesRelationTree.DropPosition = new DropPosition() { Position = NodePosition.Before, Node = parentOppNode };

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertNoExceptionThrown("Infinite loop should not occur when reverting nodes", () => onDragDrop.Invoke(salesRelationTree, new object[] { dragEvent }));
				AssertEquals("Parent and child opportunity relation should still be set as the drag operation was unsuccessful", parentOppNode, childOppNode.Parent);
				AssertEquals("Inquiry should still be child of parent opportunity as the drag operation was unsuccessful", parentOppNode, inquiryNode.Parent);
			}
		}

		#endregion

		#region Show/Hide Commumication

		public void TestRemembersShowCommunicationChecked()
		{
			var opportunity = Factory.New<OrgOpportunity>();

			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				AssertEquals("Should be unchecked by default", false, form.SalesRelationControl.ShowCommunicationsCheckBox.Checked);

				form.SalesRelationControl.ShowCommunicationsCheckBox.Checked = true;
			}

			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();
				AssertEquals("Should remember from previous form", true, form.SalesRelationControl.ShowCommunicationsCheckBox.Checked);
			}

			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.Show();

				form.SalesRelationControl.ShowCommunicationsCheckBox.Checked = true;

				var opportunity2 = Factory.New<OrgOpportunity>();
				form.SalesRelationControl.SetDataBinding(opportunity2.SalesRelationModel, "");

				AssertEquals("Should remember from previous binding", true, form.SalesRelationControl.ShowCommunicationsCheckBox.Checked);
			}
		}

		#endregion

		#region ReadOnly

		public void TestSetButtonsToReadOnlyIfDisplayModeReadOnly()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();

			using (var form = new SalesRelationControlFormForTest(opportunity.SalesRelationModel))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				AssertEquals(false, form.SalesRelationControl.AttachToolStripDropDownButton.Enabled);
				AssertEquals(false, form.SalesRelationControl.NewToolStripDropDownButton.Enabled);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);
		}

		public class SalesRelationControlFormForTest : ZForm
		{
			public SalesRelationControlFormForTest(ISalesRelationModel model)
				: base(model)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				SalesRelationControl.Dock = DockStyle.Fill;
				Controls.Add(SalesRelationControl);
				BindingSource.SetBindingMember(SalesRelationControl, ".");
				CaptionRenderingEnabled = true;
			}

			public readonly SalesRelationControlForTest SalesRelationControl = new SalesRelationControlForTest();
		}

		public class SalesRelationControlForTest : SalesRelationControl
		{
			public SalesRelationControlForTest()
				: base()
			{
			}

			public void ShowEditForm_Exposed(IBusiness bizObjToEdit)
			{
				base.ShowEditForm(bizObjToEdit);
			}
		}

		class OrgOpportunityForAttachDetachTest : OrgOpportunity
		{
			public OrgOpportunityForAttachDetachTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public int NumImportChildInfoOnAttachCalled => numImportChildInfoOnAttachCalled;

			public int NumImportChildInfoOnDetachCalled => numImportChildInfoOnDetachCalled;

			protected override void ImportChildInfoOnAttach(IRelatableActivity childActivity)
			{
				numImportChildInfoOnAttachCalled++;
			}

			protected override void ImportChildInfoOnDetach(IRelatableActivity childActivity)
			{
				numImportChildInfoOnDetachCalled++;
			}

			int numImportChildInfoOnAttachCalled;
			int numImportChildInfoOnDetachCalled;
		}

		#endregion
	}
}

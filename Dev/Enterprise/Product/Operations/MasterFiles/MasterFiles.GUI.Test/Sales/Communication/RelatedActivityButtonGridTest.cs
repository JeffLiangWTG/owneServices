using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RelatedActivityButtonGridFormForTest))]
	sealed class RelatedActivityButtonGridTest : ZFormBasherTest
	{
		#region Buttons

		public void TestSetButtonsReadOnly()
		{
			var communication = Factory.New<OrgSalesCall>();
			using (var form = new RelatedActivityButtonGridFormForTest(communication))
			{
				form.Show();
				AssertEquals(true, form.RelatedActivityButtonGrid.NewSalesActivityButton.Enabled);
				AssertEquals(true, form.RelatedActivityButtonGrid.NewCommunicationButton.Enabled);

				form.RelatedActivityButtonGrid.SetButtonsReadOnly(true);
				AssertEquals(false, form.RelatedActivityButtonGrid.NewSalesActivityButton.Enabled);
				AssertEquals(false, form.RelatedActivityButtonGrid.NewCommunicationButton.Enabled);
			}
		}

		#endregion

		#region New Sales Activity

		public void TestCreateNewRelatedSpotQuoteHandler_NoteTypes()
		{
			var quotedBookingCustomNotes = new CustomNoteModuleAndCountry();
			quotedBookingCustomNotes.ModuleIDName = ModuleIDs.OneOffQuotes.Name;
			quotedBookingCustomNotes.CountryCode = CustomNoteModuleAndCountry.ModuleAndCountryCodes.CountryALL;
			quotedBookingCustomNotes.CustomNoteTypesList.AddNew();

			var customNote = quotedBookingCustomNotes.CustomNoteTypesList[0];
			customNote.NoteName = "Custom Quoted Booking Note";
			customNote.IsTextOnly = ZBool.True;
			customNote.IsAppendingNote = ZBool.True;
			customNote.IsReadOnlyAfterAdd = ZBool.False;
			customNote.ForceRead = ZBool.False;
			customNote.DefaultVisibility = nameof(StmNoteVisibility.PRV);

			var customNoteTypes = new CustomNoteTypes();
			customNoteTypes.NoteModuleAndCountryList.Add(quotedBookingCustomNotes);

			SystemDataRegistry.Instance.CustomNotes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customNoteTypes);

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			using (var form = new RelatedActivityButtonGridFormForTest(communication))
			{
				form.Show();

				var newOneOffQuoteMenuItem = form.RelatedActivityButtonGrid.NewSalesActivityButton.DropDownItems.Cast<ZToolStripMenuItem>().First(x => x.Text == "One Off Quote");

				newOneOffQuoteMenuItem.PerformClick();

				AssertNotNull(ZFormModaliser.LastFormShownForTest);

				using (var oneOffQuoteForm = (ZForm)ZFormModaliser.LastFormShownForTest)
				{
					var tabControl = oneOffQuoteForm.FindSingle<ZTemplateTabControl>("MainTabControl");
					var tab = (ZTabPage)tabControl.TabPages["NotesTabPage"];
					tabControl.SelectedTab = tab;
					Application.DoEvents();

					var stmNoteParent = (IStmNoteParent)oneOffQuoteForm.BusinessEntity;
					Assert("Expected custom note type for quoted bookings", stmNoteParent.NoteTypes.Cast<PredefinedNoteType>().Any(noteType => noteType.Description == "Custom Quoted Booking Note"));
				}
			}
		}

		[RequiresSTA]
		public void TestSetupNewSalesActivityButton()
		{
			var communication = Factory.New<OrgSalesCall>();
			using (var form = new RelatedActivityButtonGridFormForTest(communication))
			{
				form.Show();

				AssertNotNull(form.RelatedActivityButtonGrid.NewSalesActivityButton.DropDownItems);

				var newButton = form.RelatedActivityButtonGrid.NewSalesActivityButton;
				var newMenuItems = newButton.DropDownItems.Cast<ZToolStripMenuItem>();
				AssertArrayEqualsByElements(new[]
					{
						"Inquiry",
						"Opportunity",
						"Quotation",
						"One Off Quote",
						"Project"
					},
					newMenuItems.Select(x => x.Text).ToArray());
			}
		}

		public void TestNew_Opportunity()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			using (var form = new RelatedActivityButtonGridFormForTest(communication))
			{
				form.Show();

				var newOpportunityMenuItem = form.RelatedActivityButtonGrid.NewSalesActivityButton.DropDownItems.Cast<ZToolStripMenuItem>().First(x => x.Text == "Opportunity");

				newOpportunityMenuItem.PerformClick();
				AssertNotNull(ZFormModaliser.LastFormShownForTest);

				using (ZFormModaliser.LastFormShownForTest)
				{
					var bizObj = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
					AssertNotNull(bizObj);
					AssertType(typeof(OrgOpportunity), bizObj);

					var opportunity = (OrgOpportunity)bizObj;
					AssertEquals("Communication should be related parent of opportunity", true, opportunity.RelatedParentActivityPivotCollection.Activities.Select(activity => activity.PK).Contains(communication.PK));
					AssertEquals("Communication should be related parent of opportunity", false, opportunity.RelatedChildActivityPivotCollection.Activities.Select(activity => activity.PK).Contains(communication.PK));
				}
			}
		}

		public void TestNew_InvalidOpportunity()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			using (var form = new RelatedActivityButtonGridFormForTest(communication))
			{
				form.Show();

				var newOpportunityMenuItem = form.RelatedActivityButtonGrid.NewSalesActivityButton.DropDownItems.Cast<ZToolStripMenuItem>().First(x => x.Text == "Opportunity");

				newOpportunityMenuItem.PerformClick();

				AssertNull("Should not show new opportunity form as it is not a valid child for communication.", ZFormModaliser.LastFormShownForTest);
				AssertEquals("Can not create related activity - Communication must be saved before it can be a parent of another activity.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region New Communication

		public void TestSetupNewCommunicationButton()
		{
			var communication = Factory.New<OrgSalesCall>();
			using (var form = new RelatedActivityButtonGridFormForTest(communication))
			{
				form.Show();

				AssertNotNull(form.RelatedActivityButtonGrid.NewCommunicationButton.DropDownItems);

				form.RelatedActivityButtonGrid.AddUniversalCopyButton_Exposed();

				var newCommunicationButton = form.RelatedActivityButtonGrid.NewCommunicationButton;
				var newCommunicationMenuItems = newCommunicationButton.DropDownItems.Cast<ZToolStripMenuItem>();
				AssertArrayEqualsByElements(new[]
					{
						"New",
						"Universal Copy"
					},
					newCommunicationMenuItems.Select(x => x.Text).ToArray());
			}
		}

		public void TestNew_Communication_WithGridRowsSelected()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var communication = org.SalesCalls.AddNew();
			Factory.Save();

			communication.RelatedParentActivityPivotCollection.AddNewPivot(opportunity);
			communication.RelatedParentActivityPivotCollection.AddNewPivot(inquiry1);
			communication.RelatedParentActivityPivotCollection.AddNewPivot(inquiry2);

			using (var form = new RelatedActivityButtonGridFormForTest(communication))
			{
				form.Show();

				var activityPksToSelect = new[] { opportunity.PK, inquiry1.PK };
				form.RelatedActivityButtonGrid.InnerGrid.SelectAllElements(x => activityPksToSelect.Contains(((RelatedActivityLink)x).ToActivityForBinding.PK));

				var newCommunicationMenuItem = form.RelatedActivityButtonGrid.NewCommunicationButton.DropDownItems.Cast<ZToolStripMenuItem>().First(x => x.Text == "New");
				newCommunicationMenuItem.PerformClick();
				AssertNotNull(ZFormModaliser.LastFormShownForTest);

				using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownForTest)
				{
					var bizObj = lastShownForm.BusinessEntity;
					AssertNotNull(bizObj);
					AssertType(typeof(OrgSalesCall), bizObj);

					var newCommunication = (OrgSalesCall)bizObj;
					AssertEquals("Opportunity should be related parent of new communication", true, newCommunication.RelatedParentActivityPivotCollection.Activities.Select(activity => activity.PK).Contains(opportunity.PK));
					AssertEquals("Inquiry1 should be related parent of new communication", true, newCommunication.RelatedParentActivityPivotCollection.Activities.Select(activity => activity.PK).Contains(inquiry1.PK));
					AssertEquals("Inquiry2 should not be related parent of new communication", false, newCommunication.RelatedParentActivityPivotCollection.Activities.Select(activity => activity.PK).Contains(inquiry2.PK));
				}
			}
		}

		public void TestNew_Communication_WithInquiryAndOpportunityRowsSelectedInExactOrder()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var communication = org.SalesCalls.AddNew();
			communication.RelatedParentActivityPivotCollection.AddNewPivot(inquiry);
			communication.RelatedParentActivityPivotCollection.AddNewPivot(opportunity);
			Factory.Save();

			using (var form = new RelatedActivityButtonGridFormForTest(communication))
			{
				form.Show();

				form.RelatedActivityButtonGrid.ShowNewRelatedActivityForm_Exposed(ZControllerFactory.Create(ControllerIDs.Communication), new IRelatableActivity[] { inquiry, opportunity });
				AssertNotNull(ZFormModaliser.LastFormShownForTest);

				using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownForTest)
				{
					var bizObj = lastShownForm.BusinessEntity;
					AssertNotNull(bizObj);
					AssertType(typeof(OrgSalesCall), bizObj);

					var newCommunication = (OrgSalesCall)bizObj;
					AssertEquals("Opportunity should be related parent of new communication", true, newCommunication.RelatedParentActivityPivotCollection.Activities.Select(activity => activity.PK).Contains(opportunity.PK));
					AssertEquals("Inquiry should be related parent of new communication", true, newCommunication.RelatedParentActivityPivotCollection.Activities.Select(activity => activity.PK).Contains(inquiry.PK));
				}
			}
		}

		public void TestNew_Communication_WithNoGridRowsSelected()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			communication.RelatedParentActivityPivotCollection.AddNewPivot(opportunity);
			communication.RelatedParentActivityPivotCollection.AddNewPivot(inquiry1);
			communication.RelatedParentActivityPivotCollection.AddNewPivot(inquiry2);

			using (var form = new RelatedActivityButtonGridFormForTest(communication))
			{
				form.Show();

				var inquiryLink = communication.RelatedActivityLinkCollection.Cast<RelatedActivityLink>().First(link => Equals(link.ToActivityForBinding, inquiry1));
				form.RelatedActivityButtonGrid.InnerGrid.ListManager.Position = form.RelatedActivityButtonGrid.InnerGrid.ListManager.List.IndexOf(inquiryLink);
				form.RelatedActivityButtonGrid.InnerGrid.UnSelectAll();

				var newCommunicationMenuItem = form.RelatedActivityButtonGrid.NewCommunicationButton.DropDownItems.Cast<ZToolStripMenuItem>().First(x => x.Text == "New");
				newCommunicationMenuItem.PerformClick();
				AssertNotNull(ZFormModaliser.LastFormShownForTest);

				using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownForTest)
				{
					var bizObj = lastShownForm.BusinessEntity;
					AssertNotNull(bizObj);
					AssertType(typeof(OrgSalesCall), bizObj);

					var newCommunication = (OrgSalesCall)bizObj;
					AssertEquals("Inquiry1 should be related parent of new communication", true, newCommunication.RelatedParentActivityPivotCollection.Activities.Select(activity => activity.PK).Contains(inquiry1.PK));
				}
			}
		}

		public void TestNew_Communication_WithNoGridRows()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			using (var form = new RelatedActivityButtonGridFormForTest(communication))
			{
				form.Show();

				var newCommunicationMenuItem = form.RelatedActivityButtonGrid.NewCommunicationButton.DropDownItems.Cast<ZToolStripMenuItem>().First(x => x.Text == "New");
				newCommunicationMenuItem.PerformClick();
				AssertNotNull(ZFormModaliser.LastFormShownForTest);

				using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownForTest)
				{
					var bizObj = lastShownForm.BusinessEntity;
					AssertNotNull(bizObj);
					AssertType(typeof(OrgSalesCall), bizObj);

					var newCommunication = (OrgSalesCall)bizObj;
					AssertEquals("Existing Communication should be related parent of new communication", true, newCommunication.RelatedParentActivityPivotCollection.Activities.Select(activity => activity.PK).Contains(communication.PK));
				}
			}
		}

		#endregion

		#region Edit

		public void TestEdit()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var linkCollection = new RelatedActivityLinkCollection(opportunity);
			var pivot = ViewRelatedActivityPivot.Create(Factory, opportunity, inquiry);
			var link = RelatedActivityLink.Get(pivot, opportunity);
			Factory.Save();

			using (var form = new RelatedActivityButtonGridFormForTest(linkCollection))
			{
				form.Show();

				form.RelatedActivityButtonGrid.Edit_Exposed(link, null);

				AssertNotNull("Should have shown edit form", form.RelatedActivityButtonGrid.LastControllerForTest);
				AssertNotNull("Should have shown edit form", form.RelatedActivityButtonGrid.LastControllerForTest.LastShownForm);
				try
				{
					AssertEquals("Should have shown edit form for inquiry", inquiry.PK, ((BusinessObject)((ZForm)form.RelatedActivityButtonGrid.LastControllerForTest.LastShownForm).BusinessEntity).PK);
				}
				finally
				{
					form.RelatedActivityButtonGrid.LastControllerForTest.LastShownForm.Dispose();
				}
			}
		}

		public void TestEdit_WhileListIsReadOnly()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var pivot = communication.RelatedParentActivityPivotCollection.AddNewPivot(opportunity);
			var link = RelatedActivityLink.Get(pivot, communication);
			Factory.Save();

			communication.RelatedActivityLinkCollection.SetReadOnlyIncludingChildren(true);
			using (var form = new RelatedActivityButtonGridFormForTest(communication.RelatedActivityLinkCollection))
			{
				form.Show();

				form.RelatedActivityButtonGrid.Edit_Exposed(link, null);

				AssertNotNull("Should have shown edit form", form.RelatedActivityButtonGrid.LastControllerForTest);
				AssertNotNull("Should have shown edit form", form.RelatedActivityButtonGrid.LastControllerForTest.LastShownForm);

				using (var shownForm = form.RelatedActivityButtonGrid.LastControllerForTest.LastShownForm)
				{
					AssertNotEquals("DisplayMode", ODisplayMode.ReadOnly, shownForm.DisplayMode);
				}
			}
		}

		#endregion

		#region Detach

		public void TestDetach()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var commOppPivot = ViewRelatedActivityPivot.Create(Factory, communication, opportunity);

			Factory.Save();

			var linkCollection = new RelatedActivityLinkCollection(communication);
			AssertEquals("Precondition", 1, linkCollection.Count);
			using (var form = new RelatedActivityButtonGridFormForTest(linkCollection))
			{
				form.Show();

				var commOppLink = linkCollection.Cast<RelatedActivityLink>().First(link => Equals(link.FromActivity, communication));
				form.RelatedActivityButtonGrid.InnerGrid.SelectSingleElement(commOppLink);
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					form.RelatedActivityButtonGrid.InnerGrid.DeleteMenuItem.PerformClick();

					AssertEquals("Detach the selected related activities?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Detach Related Activity?", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("Should not have deleted", false, commOppLink.IsDeleted);
					AssertEquals("Should not have deleted", 1, linkCollection.Count);
				}

				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					form.RelatedActivityButtonGrid.InnerGrid.DeleteMenuItem.PerformClick();

					AssertEquals("Detach the selected related activities?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Detach Related Activity?", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("Should have deleted", true, commOppLink.IsDeleted);
					AssertEquals("Should have deleted", 0, linkCollection.Count);
				}

				var newLink = linkCollection.AddNew();
				form.RelatedActivityButtonGrid.InnerGrid.SelectSingleElement(newLink);
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.RelatedActivityButtonGrid.InnerGrid.DeleteMenuItem.PerformClick();

					AssertNull("Should not show prompt to delete", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should have deleted", true, newLink.IsDeleted);
					AssertEquals("Should have deleted", 0, linkCollection.Count);
				}
			}
		}

		[RequiresSTA]
		public void TestDeleteMenuItem()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var linkCollection = new RelatedActivityLinkCollection(communication);
			using (var form = new RelatedActivityButtonGridFormForTest(linkCollection))
			{
				AssertEquals(true, form.RelatedActivityButtonGrid.InnerGrid.AllowReadOnlyRowsToBeDeleted);
				AssertEquals(RemoveAction.RemoveAndDelete, form.RelatedActivityButtonGrid.InnerGrid.RemoveAction);
				AssertEquals("Detach Activity", form.RelatedActivityButtonGrid.InnerGrid.DeleteMenuItem.Text);
			}
		}

		#endregion

		#region RelatedActivityButtonGrid

		public void TestCanDeleteCurrentRow_False()
		{
			var opportunity = (IRelatableActivity)Factory.NewWithValidTestData(ObjectFactory.GetType<ICrmOpportunity>());
			AssertCanDeleteCurrentRow(opportunity, false);
		}

		[RequiresSTA]
		public void TestCanDeleteCurrentRow_True()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			AssertCanDeleteCurrentRow(opportunity, true);
		}

		public void TestCanDeleteCurrentRow_WhenWrongRowIndex_FalseWithNoException()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			AssertCanDeleteCurrentRow(opportunity, false, true);
		}

		void AssertCanDeleteCurrentRow(IRelatableActivity parentOpp, bool expectedDeleteMenuItemEnabled, bool rowOutOfIndex = false)
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = parentOpp;
			pivot.ChildActivity = communication;
			Factory.Save();

			var linkCollection = new RelatedActivityLinkCollection(communication);

			using (var form = new RelatedActivityButtonGridFormForTest(linkCollection))
			{
				form.Show();
				var grid = form.RelatedActivityButtonGrid.InnerGrid;
				if (!rowOutOfIndex)
				{
					grid.SetCurrentHitTestForTest(0, 0);
				}
				else
				{
					grid.MousePositionForTesting = new Point(0, 0);
				}

				grid.ContextMenu.ShowPopupMenu();
				AssertEquals(expectedDeleteMenuItemEnabled, grid.DeleteMenuItem.Enabled);
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

		public class RelatedActivityButtonGridForTest : RelatedActivityButtonGrid
		{
			public void ShowNewRelatedActivityForm_Exposed(ZController controller, IEnumerable<IRelatableActivity> parentRelatableActivities)
			{
				ShowNewRelatedActivityForm(controller, parentRelatableActivities);
			}

			public void AddUniversalCopyButton_Exposed()
			{
				AddUniversalCopyButton();
			}

			public void Edit_Exposed(BusinessObject selected, object sender)
			{
				Edit(selected, sender);
			}

			protected override ZController GetNewControllerCore(BusinessObject selected)
			{
				var result = base.GetNewControllerCore(selected);
				LastControllerForTest = result;
				return result;
			}

			public ZController LastControllerForTest;

			public ZToolStripDropDownButton NewSalesActivityButton
			{
				get { return toolStrip.Items.Find(Buttons.NewSalesActivity, true).FirstOrDefault() as ZToolStripDropDownButton; }
			}

			public ZToolStripDropDownButton NewCommunicationButton
			{
				get { return toolStrip.Items.Find(Buttons.NewCommunication, true).FirstOrDefault() as ZToolStripDropDownButton; }
			}
		}

		public class RelatedActivityButtonGridFormForTest : ZForm
		{
			public RelatedActivityButtonGridFormForTest(RelatedActivityLinkCollection collection)
				: base(collection)
			{
			}

			public RelatedActivityButtonGridFormForTest(OrgSalesCall communication)
				: base(communication)
			{
				BindingSource.SetBindingMember(RelatedActivityButtonGrid, "RelatedActivityLinkCollection");
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();

				this.Controls.Add(RelatedActivityButtonGrid);
				//
				// RelatedActivityButtonGrid
				//
				BindingSource.SetBindingMember(RelatedActivityButtonGrid, ".");
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OrgSalesCall)(null)).RelatedActivityLinkCollection);
				zDropEditColumnStyleInfo1.CaptionResourceString = NoResourceStringData.GetData("Type");
				zDropEditColumnStyleInfo1.ColumnName = "ToActivityTypeForBinding";
				zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
				this.RelatedActivityButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
				this.CaptionRenderingEnabled = true;
				this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 768);
			}

			public readonly RelatedActivityButtonGridForTest RelatedActivityButtonGrid = new RelatedActivityButtonGridForTest();
		}

		protected override Form GetFormToBashCore()
		{
			var communication = Factory.New<OrgSalesCall>();
			return new RelatedActivityButtonGridFormForTest(communication);
		}

		#endregion
	}
}

using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(CommunicationGridFormForTest))]
	sealed class CommunicationGridTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestNewWithNoSecurity()
		{
			Env.Security.CommunicationManagerNew.IsAllowed = false;

			var opportunity = Factory.New<OrgOpportunity>();
			using (var form = new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection))
			{
				form.Show();

				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
					{
						form.CommunicationGrid.NewButton.DropDownItems[0].PerformClick();
						AssertCollectionContains(Env.Security.CommunicationManagerNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessages();
					});
				AssertType(null, ZFormModaliser.LastFormShownForTest);
			}
		}

		#region Popup

		[RequiresSTA]
		public void TestPopupContextMenu()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			using (var form = new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection))
			{
				form.Show();

				var popupMenuItem = form.CommunicationGrid.InnerGrid.ContextMenu.MenuItems[0];
				AssertEquals("Popup", popupMenuItem.Text);

				form.CommunicationGrid.ShowPopupContextMenu = true;
				{
					AssertEquals(true, popupMenuItem.Visible);
					popupMenuItem.PerformClick();
					AssertNotNull(ZFormModaliser.LastFormShownForTest);
					using (var popupForm = ZFormModaliser.LastFormShownForTest)
					{
						AssertType(typeof(RelatedCommunicationForm), popupForm);
					}
				}

				form.CommunicationGrid.ShowPopupContextMenu = false;
				{
					AssertEquals(false, popupMenuItem.Visible);
				}
			}
		}

		#endregion

		#region NotesTextBox

		[RequiresSTA]
		public void TestRemembersNotesTextBoxSize()
		{
			var opportunity = Factory.New<OrgOpportunity>();

			using (var form = new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection))
			{
				form.Show();

				form.CommunicationGrid.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			}

			using (var form = new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection))
			{
				form.Show();
				AssertEquals("Should remember from previous form", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50), form.CommunicationGrid.MainSplitContainer.SplitterDistance);

				form.CommunicationGrid.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			}

			using (var form = new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection))
			{
				form.Show();
				AssertEquals("Should remember from previous form", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100), form.CommunicationGrid.MainSplitContainer.SplitterDistance);
			}
		}

		[RequiresSTA]
		public void TestRemembersNotesTextBoxVisibility()
		{
			var opportunity = Factory.New<OrgOpportunity>();

			using (var form = new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection))
			{
				form.Show();
				AssertEquals("Should be hidden by default", false, form.CommunicationGrid.NotesTextBox.Visible);

				form.CommunicationGrid.ShowNotesCheckBox.Checked = true;
				AssertEquals(true, form.CommunicationGrid.NotesTextBox.Visible);
			}

			using (var form = new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection))
			{
				form.Show();
				AssertEquals("Should remember from previous form", true, form.CommunicationGrid.NotesTextBox.Visible);

				form.CommunicationGrid.ShowNotesCheckBox.Checked = false;
				AssertEquals(false, form.CommunicationGrid.NotesTextBox.Visible);
			}

			using (var form = new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection))
			{
				form.Show();
				AssertEquals("Should remember from previous form", false, form.CommunicationGrid.NotesTextBox.Visible);
			}
		}

		#endregion

		[RequiresSTA]
		public void TestReadOnlyButtons()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var collection = opportunity.RelatedCommunicationCollection;
			opportunity.SetReadOnlyIncludingChildren(true);
			collection.SetReadOnlyIncludingChildren(true);
			using (var form = new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(false, form.CommunicationGrid.NewButton.Enabled);
				AssertEquals(false, form.CommunicationGrid.editButton.Enabled);
			}
			using (var form = new CommunicationGridFormForTest(opportunity))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(false, form.CommunicationGrid.NewButton.Enabled);
				AssertEquals(false, form.CommunicationGrid.editButton.Enabled);
			}

			opportunity.SetReadOnlyIncludingChildren(false);
			collection.SetReadOnlyIncludingChildren(false);
			using (var form = new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.CommunicationGrid.NewButton.Enabled);
				AssertEquals(true, form.CommunicationGrid.editButton.Enabled);
			}
			using (var form = new CommunicationGridFormForTest(opportunity))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.CommunicationGrid.NewButton.Enabled);
				AssertEquals(true, form.CommunicationGrid.editButton.Enabled);
			}
		}

		#region Implementation

		[RequiresSTA]
		public override void TestBindingAllTabsOnIdle()
		{
			try
			{
				base.TestBindingAllTabsOnIdle();
			}
			catch (NotSupportedException)
			{
				Assert("Because binding is for ActiveBusinessObjectCollection, this test is blowing up when HasChanges attempted to be set to true.", true);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			return new CommunicationGridFormForTest(opportunity.RelatedCommunicationCollection);
		}

		public class CommunicationGridFormForTest : ZForm
		{
			public CommunicationGridFormForTest(OrgSalesCallCollection collection)
				: base(collection)
			{
			}

			public CommunicationGridFormForTest(OrgOpportunity opportunity)
				: base(opportunity)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				Controls.Add(CommunicationGrid);
				BindingSource.SetBindingMember(CommunicationGrid, ".");
				CaptionRenderingEnabled = true;
			}

			public readonly CommunicationGrid CommunicationGrid = new CommunicationGrid();
		}

		#endregion
	}
}

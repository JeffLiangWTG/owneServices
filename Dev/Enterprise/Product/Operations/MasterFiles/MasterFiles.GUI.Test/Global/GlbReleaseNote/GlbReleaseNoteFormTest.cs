using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Global.Testing
{
	[TestedType(typeof(DummyGlbReleaseNoteForm))]
	sealed class GlbReleaseNoteFormTest : GlbReleaseNoteTestCase
	{
		public void TestViewViaButton()
		{
			Registry.Business.WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			PrepareTestData();

			using (DummyGlbReleaseNoteForm form = GetFormToBash())
			{
				form.Show();

				AssertNull("Precondition: There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Precondition: No note should be shown.", form.DestinationURL);

				form.ReleaseNotesGrid.Select(0);
				form.ReleaseNotesGrid.Select(1);

				form.ViewButton.PerformClick();
				AssertEquals("An error message should be shown.", "Please select only one update note to view at a time.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("No note should be shown.", form.DestinationURL);

				form.ReleaseNotesGrid.UnSelect(0);
				form.ReleaseNotesGrid.ListManager.Position = 1;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ViewButton.PerformClick();
				AssertNull("There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Note2 should be shown.", Note2.GetDownloadURL(), form.DestinationURL);

				form.DestinationURL = null;
				Manager.ReleaseNotes.DeleteAll();
				form.ViewButton.PerformClick();
				AssertEquals("An error message should be shown.", "Please select an update note to view.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("No note should be shown.", form.DestinationURL);
			}
		}

		public void TestViewViaDoubleClick()
		{
			Registry.Business.WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			PrepareTestData();

			using (DummyGlbReleaseNoteForm form = GetFormToBash())
			{
				form.Show();

				AssertNull("Precondition: No note should be shown.", form.DestinationURL);

				form.DummyHasRowAtMouseCursorPosition = false;
				MethodInfo methodInfo = typeof(ZGrid).GetMethod("OnDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
				methodInfo.Invoke(form.ReleaseNotesGrid, new object[] { EventArgs.Empty });
				AssertNull("No note should be shown.", form.DestinationURL);

				form.ReleaseNotesGrid.Select(1);
				form.DummyHasRowAtMouseCursorPosition = true;
				methodInfo.Invoke(form.ReleaseNotesGrid, new object[] { EventArgs.Empty });
				AssertEquals("Note2 should be shown.", Note2.GetDownloadURL(), form.DestinationURL);
			}
		}

		[ExpectNoExceptions]
		public void TestDoubleClickIntoMilk()
		{
			PrepareTestData();

			using (DummyGlbReleaseNoteForm form = GetFormToBash())
			{
				form.Show();

				form.ReleaseNotesGrid.Select(1);
				form.DummyHasRowAtMouseCursorPosition = null;
				MethodInfo methodInfo = typeof(ZGrid).GetMethod("OnDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
				Cursor.Position = new Point(form.ReleaseNotesGrid.Left + 4, form.ReleaseNotesGrid.Bottom - 4);
				methodInfo.Invoke(form.ReleaseNotesGrid, new object[] { EventArgs.Empty });
				AssertNull("No note should be shown.", form.DestinationURL);
			}
		}

		public void TestOpenNoteInWebBrowser_IsWiseTechGlobalItemViaTrustedMessaging()
		{
			PrepareTestData();
			WebUrlLauncher.ClearLastUrlLaunched();

			try
			{
				using (DummyGlbReleaseNoteForm form = GetFormToBash())
				{
					var note1 = Factory.New<GlbReleaseNoteForTest>();
					WebDataRegistry.Instance.EnableTrustedMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					note1.GF_Section = "C1U";
					AssertEquals(true, note1.IsWiseTechGlobalItemViaTrustedMessaging);
					form.Show();

					var checker = new SystemUserAccountCollectionTermCheckerForTest();
					using (ObjectFactory.Substitute<ISystemUserAccountCollectionTermChecker>(checker))
					{
						AssertEquals(false, checker.TermAcknowledged);
						form.OpenNoteInWebBrowser_Exposed(note1);
						AssertEquals(string.Empty, WebUrlLauncher.LastUrlLaunched);

						checker.TermAcknowledged = true;
						AssertEquals(true, checker.TermAcknowledged);
						form.OpenNoteInWebBrowser_Exposed(note1);
						AssertEquals("http://www.cw1.com/123.pdf", WebUrlLauncher.LastUrlLaunched);
					}
				}
			}
			finally
			{
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		class SystemUserAccountCollectionTermCheckerForTest : ISystemUserAccountCollectionTermChecker
		{
			public bool TermAcknowledged { get; set; }
			public Task<bool> CheckTermAcknowledged() => Task.FromResult(TermAcknowledged);
		}

		class GlbReleaseNoteForTest : GlbReleaseNote
		{
			public GlbReleaseNoteForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override ZString GetDownloadURL() => "http://www.cw1.com/123.pdf";
		}

		#region Implementation

		new DummyGlbReleaseNoteForm GetFormToBash()
		{
			return (DummyGlbReleaseNoteForm)base.GetFormToBash();
		}

		protected override Form GetFormToBashCore()
		{
			return new DummyGlbReleaseNoteForm(Manager);
		}

		#region class DummyGlbReleaseNoteForm

		class DummyGlbReleaseNoteForm : GlbReleaseNoteForm
		{
			public DummyGlbReleaseNoteForm(GlbReleaseNoteManager businessEntity) : base(businessEntity)
			{
				InitializeComponent();
			}

			new void InitializeComponent()
			{
				ZTextBoxColumnStyleInfo columnStyleInfo = new ZTextBoxColumnStyleInfo();
				columnStyleInfo.Caption = "Link";
				columnStyleInfo.ColumnName = "GF_URL";
				columnStyleInfo.Width = 380;
				TypeDescriptor.AddAttributes(columnStyleInfo, new SuppressFormsLocalizedTestAttribute());
				ReleaseNotesGrid.ColumnStyles.Add(columnStyleInfo);

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.ColumnName = "GF_ReleaseNoteDate";
				zTextBoxColumnStyleInfo2.IsVisible = true;
				ReleaseNotesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			}

			protected override void OpenNoteInWebBrowser(GlbReleaseNote note)
			{
				this.DestinationURL = note.GetDownloadURL();
			}

			public void OpenNoteInWebBrowser_Exposed(GlbReleaseNote note)
			{
				base.OpenNoteInWebBrowser(note);
			}

			protected override bool HasRowAtMouseCursorPosition
			{
				get { return DummyHasRowAtMouseCursorPosition ?? base.HasRowAtMouseCursorPosition; }
			}

			public bool? DummyHasRowAtMouseCursorPosition = false;
			public string DestinationURL;
		}

		#endregion

		#endregion
	}
}

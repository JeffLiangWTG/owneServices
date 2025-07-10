using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.GUI.ReflectiveFieldMap;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.Environment;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class ContentDesignerControlTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestCopyMacro()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Simulation Campaign";

			using (var form = GetFormForTest(campaign))
			{
				SafeClipboard.Clear();

				var memberDescription = new MemberDescriptionForTest(null, "The Campaign Name", MemberDescription.MacroTagTypes.Email);
				var node = new MapTreeNode(new MapTreeUserControl.MapTreeNotNode(form.ContentControl.MapTreeUserControl, null, memberDescription));

				form.ContentControl.CopyMacroButton_Click();
				AssertNull("Do not copy when selected node is null", SafeClipboard.GetData(DataFormats.UnicodeText));

				form.ContentControl.MapTreeUserControl.mapFilterTreeView.DisplayTree.Nodes.Add(node);
				form.ContentControl.MapTreeUserControl.SelectedTreeNode = node;
				form.ContentControl.CopyMacroButton_Click();

				AssertEquals("Macro in clipboard", "(*CampaignName*)", SafeClipboard.GetData(DataFormats.UnicodeText));
			}
		}

		public void TestAddMacro()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Simulation Campaign";

			using (var form = GetFormForTest(campaign))
			{
				form.Show();

				form.ContentControl.EmailContentTextBoxExposed.Text = "<body><p>The big fat cat sat on the green mat</body>";

				form.ContentControl.AddMacroButton_Click();
				AssertEquals("Macro doesn't work when selected node is null", "<body><p>The big fat cat sat on the green mat</body>", form.ContentControl.EmailContentTextBoxExposed.Text);

				var memberDescription = new MemberDescriptionForTest(null, "The Campaign Name", MemberDescription.MacroTagTypes.Email);
				var node = new MapTreeNode(new MapTreeUserControl.MapTreeNotNode(form.ContentControl.MapTreeUserControl, null, memberDescription));
				form.ContentControl.MapTreeUserControl.mapFilterTreeView.DisplayTree.Nodes.Add(node);
				form.ContentControl.MapTreeUserControl.SelectedTreeNode = node;

				//                                                      01234567890
				form.ContentControl.EmailContentTextBoxExposed.SelectionStart = 9;
				form.ContentControl.EmailContentTextBoxExposed.SelectionLength = 0;

				form.ContentControl.AddMacroButton_Click();
				var expected = @"<body><p>(*CampaignName*)The big fat cat sat on the green mat</body>";

				AssertEquals("Macro Added", expected, form.ContentControl.EmailContentTextBoxExposed.Text);
			}
		}

		public void TestBrowseButtonClick()
		{
			ZFormModaliser.LastCommonDialogShownDialogForTest = null;
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				AssertNull("Dialog should not have been shown", ZFormModaliser.LastCommonDialogShownDialogForTest);

				form.ContentControl.BrowseButton_Click();

				CommonDialog dialog = ZFormModaliser.LastCommonDialogShownDialogForTest;
				AssertEquals("Dialog Type", typeof(OpenFileDialog), dialog.GetType());
				AssertEquals("Default Extension", "*.htm|*.html", ((OpenFileDialog)dialog).DefaultExt);
				AssertEquals("Filter", "HTML files (*.htm, *.html)|*.htm;*.html|All files (*.*)|*.*", ((OpenFileDialog)dialog).Filter);
				ZFormModaliser.LastCommonDialogShownDialogForTest = null;
			}
		}

		public void TestBrowseButton_ClickWithNameAttributesInFile()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			string fileName = Env.TempPath + "test.html";
			string sExpected = string.Empty;
			try
			{
				using (StreamWriter writer = new StreamWriter(fileName))
				{
					writer.WriteLine("<html><head></head><body><p><a href=\"http://www.test.gov\" tid=\"Tester\">Tester</a></p></body></html>");
				}

				ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
				using (var form = GetFormForTest(campaign))
				{
					form.Show();
					form.ContentControl.BrowseButton_Click();
					form.ContentControl.HTMLFileDialog_FileOk();
					sExpected =
@"<html><head></head><body><p><a href=""http://www.test.gov"" tid=""Tester"">Tester</a></p>
</body></html>";
					AssertEqualsIgnoreLineBreaks("TemplateHtmlText should contain LinkTracking attribute only \r\n" + campaign.TemplateEditor.TemplateHtmlText.ToString(), sExpected, campaign.TemplateEditor.TemplateHtmlText.ToString());
				}

				using (StreamWriter writer = new StreamWriter(fileName))
				{
					writer.WriteLine("<html><head></head><body><p><a href=\"http://www.test.gov\">Tester</a></p></body></html>");
				}

				ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				campaign = Factory.New<GlbCompanyCampaign>();
				using (var form = GetFormForTest(campaign))
				{
					form.Show();
					form.ContentControl.BrowseButton_Click();
					form.ContentControl.HTMLFileDialog_FileOk();
					sExpected =
@"<html><head></head><body><p><a href=""http://www.test.gov"">Tester</a></p>
</body></html>";
					AssertEqualsIgnoreLineBreaks("TemplateHtmlText should not contain LinkTracking macro \r\n" + campaign.TemplateEditor.TemplateHtmlText.ToString(), sExpected, campaign.TemplateEditor.TemplateHtmlText.ToString());
				}
			}
			finally
			{
				File.Delete(fileName);
				ZFormModaliser.LastCommonDialogShownDialogForTest = null;
			}
		}

		public void TestHTMLFileDialog_FileOk()
		{
			string fileName = Env.TempPath + "test.html";
			string wrongFileName = Env.TempPath + "test.wrong";

			try
			{
				using (StreamWriter writer = new StreamWriter(fileName))
				{
					writer.WriteLine("-------------------");
				}

				ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var campaign = Factory.New<GlbCompanyCampaign>();
				using (var form = GetFormForTest(campaign))
				{
					form.Show();
					Assert("Document blob should be empty", campaign.TemplateEditor.TemplateBlob.IsEmpty);
					form.ContentControl.BrowseButton_Click();
					form.ContentControl.HTMLFileDialog_FileOk();
					Assert("Document blob should NOT be empty", !campaign.TemplateEditor.TemplateBlob.IsEmpty);
				}

				using (StreamWriter writer = new StreamWriter(wrongFileName))
				{
					writer.WriteLine("-------------------");
				}

				ZFormModaliser.FileNameToSelectInShowCommonDialog = wrongFileName;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				campaign = Factory.New<GlbCompanyCampaign>();
				campaign.HtmlDocumentBlob = ZBlob.Empty;
				using (var form = GetFormForTest(campaign))
				{
					form.Show();
					Assert("Document blob should be empty", campaign.TemplateEditor.TemplateBlob.IsEmpty);
					form.ContentControl.BrowseButton_Click();
					form.ContentControl.HTMLFileDialog_FileOk();
					Assert("Document blob should still be empty", campaign.TemplateEditor.TemplateBlob.IsEmpty);
					Assert("Error should have been shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Error message should have been shown", "The selected file has an invalid file extension of type .wrong"
						+ ". Please choose another file with either a .htm or .html extension", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				File.Delete(fileName);
				File.Delete(wrongFileName);
			}
		}

#if !WINZOR
		public void TestHTMLFileDialog_FileOk_Remote()
		{
			using (var file = TempFile.NewWithExtension("html"))
			{
				File.WriteAllLines(file.Filename, new[] { "-------------------" });
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var campaign = Factory.New<GlbCompanyCampaign>();
				using (var form = GetFormForTest(campaign))
				{
					form.Show();

					form.ContentControl.SetIsRemote(true);
					form.ContentControl.SetMappedPathFound(true);
					form.ContentControl.BrowseButton_Click();
					form.ContentControl.HTMLFileDialog_FileOk();

					Application.DoEvents();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				campaign = Factory.New<GlbCompanyCampaign>();
				using (var form = GetFormForTest(campaign))
				{
					form.Show();

					form.ContentControl.SetIsRemote(true);
					form.ContentControl.SetMappedPathFound(false);
					form.ContentControl.BrowseButton_Click();
					form.ContentControl.HTMLFileDialog_FileOk();

					Application.DoEvents();
					Assert("Error should have been shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Error message should have been shown", "The drive of the selected file is not mapped for Remote Desktop. This is required to use the Upload functionality.",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}
#endif

		public void TestHTMLFileDialog_FileOk_FromWordNotFiltered()
		{
			using (var file = TempFile.NewWithExtension("html"))
			{
				File.WriteAllLines(file.Filename, new[] { @"<html><head><meta name=Generator content=""Microsoft Word 15""></head><body></body></html>" });
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var campaign = Factory.New<GlbCompanyCampaign>();
				using (var form = GetFormForTest(campaign))
				{
					form.Show();

					form.ContentControl.BrowseButton_Click();
					form.ContentControl.HTMLFileDialog_FileOk();

					Application.DoEvents();

					Assert("Error should have been shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Error message should have been shown", @"When saving a Word document for HTML please save it as ""Web Page, Filtered"" rather than ""Web Page"".",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHTMLFileDialog_FileOk_ValidImageUrl()
		{
			using (var file = TempFile.NewWithExtension("html"))
			{
				File.WriteAllLines(file.Filename, new[] { @"<html><head><meta charset=""utf-8""></head><body><img src=""http://somewhere/test.jpg""></body></html>" });
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var campaign = Factory.New<GlbCompanyCampaign>();
				using (var form = GetFormForTest(campaign))
				{
					form.Show();

					form.ContentControl.BrowseButton_Click();
					form.ContentControl.HTMLFileDialog_FileOk();

					Application.DoEvents();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}

			using (var file = TempFile.NewWithExtension("html"))
			{
				File.WriteAllLines(file.Filename, new[] { @"<html><head><meta charset=""utf-8""></head><body><img src=""http://somewhere/test.exe""></body></html>" });
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var campaign = Factory.New<GlbCompanyCampaign>();
				using (var form = GetFormForTest(campaign))
				{
					form.Show();

					form.ContentControl.BrowseButton_Click();
					form.ContentControl.HTMLFileDialog_FileOk();

					Application.DoEvents();

					Assert("Warning should have been shown", UnitTestUserNotification.Instance.LastMessage.WasWarning);
					AssertEquals("Warning message should have been shown", @"Only HTTP and HTTPS URLs with the following image extensions are supported within the image source attribute:
JPG, JPEG, GIF, PNG, BMP
The content you have uploaded contains file paths or unsupported extensions in the image source attribute, and such sources have been removed.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[DeveloperOnlyTest]
		public void TestCopyToClipboardButtonClick()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("(*LinkTracking(a,b)*)");
			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				form.ContentControl.CopyToClipboard_Click();
				AssertEndsWith("LinkTracking removed", "b", SafeClipboard.GetText());
			}
		}

		public void TestSaveButtonClick()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "First Ever";
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("<html><body bgcolor='Lime'>Hello</body></html");
			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				form.ContentControl.SaveButton_Click();

				CommonDialog dialog = ZFormModaliser.LastCommonDialogShownDialogForTest;
				AssertEquals("Dialog Type", typeof(SaveFileDialog), dialog.GetType());
				AssertEquals("Filter", "HTML Files (*.html)|*.html|SHTML Files (*.shtml)|*.shtml|HTM Files (*.htm)|*.htm|Text Files (*.txt)|*.txt", ((SaveFileDialog)dialog).Filter);
				AssertEquals("Initial FileName", "___First Ever", campaign.CampaignID);
				ZFormModaliser.LastCommonDialogShownDialogForTest = null;
			}
		}

		public void TestClearDocumentButtonClick()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				Assert("Document blob should NOT be empty", !campaign.TemplateEditor.TemplateBlob.IsEmpty);
				form.ContentControl.ClearButton_Click();
				Assert("Document blob should be empty", campaign.TemplateEditor.TemplateBlob.IsEmpty);
			}
		}

		public void TestEditorTrackedLinkHasErrors()
		{
			GlbCompanyCampaign campaign = new GlbCompanyCampaignTestHelper(Factory).GetCampaignWithoutErrors();
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("Again and Again");
			Factory.Save();
			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				campaign.TemplateEditor.TemplateHtmlText = @"<html><body><a href=""http://www.wisegrid.com"">WiseGrid</a><a href=""http://www.wisegrid.com"">WiseGrid</a>";
				campaign.TemplateEditor.TrackedLinks[0].HasTrackingMacro = true;
				campaign.TemplateEditor.TrackedLinks[0].GCL_Context = "WiseGrid";
				campaign.TemplateEditor.TrackedLinks[1].HasTrackingMacro = true;
				campaign.TemplateEditor.TrackedLinks[1].GCL_Context = "WiseGrid (2)";
				form.FireSaveButton();
				Application.DoEvents();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				campaign.TemplateEditor.TrackedLinks[1].GCL_Context = "WiseGrid";
				form.FireSaveButton();
				Application.DoEvents();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		const string BodyTemplate = @"<a href=""http://www.wisegrid.com""{0}>WiseGrid</a><a href=""http://www.wisegrid1.com""{1}>WiseGrid1</a>";
		const string DocumentBlob = @"<html><body><a href=""http://www.wisegrid.com"">WiseGrid</a><a href=""http://www.wisegrid1.com"">WiseGrid1</a></body></html>";

		public void TestTextBoxHtmlUpdatedWhenTrackedLinkChanges()
		{
			var campaign = new GlbCompanyCampaignTestHelper(Factory).GetCampaignWithoutErrors();
			var bodyInitial = string.Format(BodyTemplate, "", "");

			campaign.HtmlDocumentBlob = ZBlob.FromAscii(DocumentBlob);
			Factory.Save();

			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				AssertMultilineASCIIEquals("Html content", bodyInitial, form.ContentControl.SelectBodyHtml());
				AssertEquals("Tracking count", 2, campaign.TemplateEditor.TrackedLinks.Count);

				var link1 = campaign.TemplateEditor.TrackedLinks[0];
				link1.HasTrackingMacro = true;
				AssertMultilineASCIIEquals("Html content", string.Format(BodyTemplate, $" tid=\"{link1.GCL_Context}\"", ""), form.ContentControl.SelectBodyHtml());

				link1.HasTrackingMacro = false;
				AssertMultilineASCIIEquals("Html content", bodyInitial, form.ContentControl.SelectBodyHtml());

				var link2 = campaign.TemplateEditor.TrackedLinks[1];
				link2.HasTrackingMacro = true;
				AssertMultilineASCIIEquals("Html content", string.Format(BodyTemplate, "", $" tid=\"{link2.GCL_Context}\""), form.ContentControl.SelectBodyHtml());

				link2.HasTrackingMacro = false;
				AssertMultilineASCIIEquals("Html content", bodyInitial, form.ContentControl.SelectBodyHtml());

				link1.HasTrackingMacro = true;
				link2.HasTrackingMacro = true;
				AssertMultilineASCIIEquals("Html content", string.Format(BodyTemplate, $" tid=\"{link1.GCL_Context}\"", $" tid=\"{link2.GCL_Context}\""), form.ContentControl.SelectBodyHtml());

				link1.HasTrackingMacro = false;
				link2.HasTrackingMacro = false;
				AssertMultilineASCIIEquals("Html content", bodyInitial, form.ContentControl.SelectBodyHtml());
			}
		}

		public void TestTrackAllLinksButton_Click()
		{
			var campaign = new GlbCompanyCampaignTestHelper(Factory).GetCampaignWithoutErrors();
			var bodyInitial = string.Format(BodyTemplate, "", "");

			campaign.HtmlDocumentBlob = ZBlob.FromAscii(DocumentBlob);
			Factory.Save();

			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				AssertMultilineASCIIEquals("Html content", bodyInitial, form.ContentControl.SelectBodyHtml());
				AssertEquals("Tracking count", 2, campaign.TemplateEditor.TrackedLinks.Count);

				var link1 = campaign.TemplateEditor.TrackedLinks[0];
				var link2 = campaign.TemplateEditor.TrackedLinks[1];
				link1.HasTrackingMacro = false;
				link2.HasTrackingMacro = false;
				AssertMultilineASCIIEquals("Html content", bodyInitial, form.ContentControl.SelectBodyHtml());

				form.ContentControl.TrackAllLinksButton_Click();
				AssertEquals(true, link1.HasTrackingMacro);
				AssertEquals(true, link2.HasTrackingMacro);
				AssertMultilineASCIIEquals("Html content", string.Format(BodyTemplate, $" tid=\"{link1.GCL_Context}\"", $" tid=\"{link2.GCL_Context}\""), form.ContentControl.SelectBodyHtml());
			}
		}

		public void TestParseEmailWithMalformedContent()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Testing Campaign Email Parsing";
			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				campaign.TemplateEditor.EnableMacroDataPreview = true;
				form.ContentControl.EmailContentTextBoxExposed.Text = @"<body>(*CampaignName*</body>";
				campaign.TemplateEditor.TemplateHtmlText = form.ContentControl.EmailContentTextBoxExposed.Text;
				var contentHTML = form.ContentControl.GetPreviewContentText_Exposed();
				AssertEquals("The attached document is malformed, a start tag (* should always be followed by an end tag *)", contentHTML);
			}
		}

		public void TestEnableMacroPreview()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Macro Testing Campaign";
			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				form.ContentControl.EmailContentTextBoxExposed.Text = @"<body>(*CampaignName*)</body>";
				campaign.TemplateEditor.TemplateHtmlText = form.ContentControl.EmailContentTextBoxExposed.Text;
				AssertEquals("Pre-condition:", campaign.TemplateEditor.TemplateHtmlText, @"<body>(*CampaignName*)</body>");

				campaign.TemplateEditor.EnableMacroDataPreview = true;
				var contentHTML = form.ContentControl.GetPreviewContentText_Exposed();
				AssertEquals("The html editor in preview mode should contains Macro Testing Campaign.", contentHTML, "<body>Macro Testing Campaign</body>");
				campaign.TemplateEditor.EnableMacroDataPreview = false;
				contentHTML = form.ContentControl.GetPreviewContentText_Exposed();
				AssertEquals("The html editor in preview mode should contains the orginal macro (*CampaignName*).", contentHTML, "<body>(*CampaignName*)</body>");
			}
		}

		public void TestMapTreeUserControl_NodeSelected()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "BBCE";

			var contact = org.Contacts.AddNew();

			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Simulation Campaign";

			var docCompanyCampaignType = ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocCompanyCampaignItem>();
			var infos = docCompanyCampaignType.GetProperties();

			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				var memberDescription = new MemberDescriptionForTest(null, "The Campaign Name", MemberDescription.MacroTagTypes.Email);
				var node = new MapTreeNode(new MapTreeUserControl.MapTreeNotNode(form.ContentControl.MapTreeUserControl, null, memberDescription));
				var property = infos.FirstOrDefault(s => s.Name == memberDescription.GetFullPath());
				form.ContentControl.MapTreeUserControl.mapFilterTreeView.DisplayTree.Nodes.Add(node);
				form.ContentControl.MapTreeUserControlExposed_NodeSelected(node);
				AssertEquals("CampaignName\r\n\r\n(*CampaignName*)\r\n\r\nThe Campaign Name", form.ContentControl.InfoTextBoxExposed.Text);

				campaign.TemplateEditor.SimulationContactPK = contact.PK;
				form.ContentControl.MapTreeUserControlExposed_NodeSelected(node);
				AssertEquals("CampaignName\r\n\r\nPreview:\r\nSimulation Campaign\r\n\r\nThe Campaign Name", form.ContentControl.InfoTextBoxExposed.Text);
			}
		}

		public void TestMapTreeUserControl_NodeDoubleClicked()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "BBCE";

			var contact = org.Contacts.AddNew();

			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Simulation Campaign";

			var docCompanyCampaignType = ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocCompanyCampaignItem>();
			var infos = docCompanyCampaignType.GetProperties();

			using (var form = GetFormForTest(campaign))
			{
				form.Show();

				form.ContentControl.EmailContentTextBoxExposed.Text = "<html><body></body></html>";

				const string bodyTag = "<body>";
				int bodyPos = form.ContentControl.EmailContentTextBoxExposed.Text.IndexOf(bodyTag);
				int bodyTagLen = bodyTag.Length;
				form.ContentControl.EmailContentTextBoxExposed.SelectionStart = bodyPos + bodyTagLen;
				form.ContentControl.EmailContentTextBoxExposed.SelectionLength = 0;

				var memberDescription = new MemberDescriptionForTest(null, "The Campaign Name", MemberDescription.MacroTagTypes.Email);
				var node = new MapTreeNode(new MapTreeUserControl.MapTreeNotNode(form.ContentControl.MapTreeUserControl, null, memberDescription));
				var property = infos.FirstOrDefault(s => s.Name == memberDescription.GetFullPath());
				form.ContentControl.MapTreeUserControl.mapFilterTreeView.DisplayTree.Nodes.Add(node);
				form.ContentControl.MapTreeUserControlExposed_NodeDoubleClicked(node);

				AssertEndsWith("Preview Text should not show", ">(*CampaignName*)</body></html>", form.ContentControl.EmailContentTextBoxExposed.Text);
				campaign.TemplateEditor.TemplateHtmlText = form.ContentControl.EmailContentTextBoxExposed.Text;

				campaign.TemplateEditor.SimulationContactPK = contact.PK;
				form.ContentControl.MapTreeUserControlExposed_NodeDoubleClicked(node);

				AssertEndsWith("Preview Text should not show", ">(*CampaignName*)(*CampaignName*)</body></html>", form.ContentControl.EmailContentTextBoxExposed.Text);
			}
		}

		public void TestSimulationContactWithTrackedLink()
		{
			const string testerHref = @"<a href='http://www.test.gov' target='_blank' tid='Tester'>Tester</a>";
			const string macroContactName = @"<p>(*ContactName*)</p>";
			const string simulationContactName = @"<p>Contact AU</p>";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "VVW";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact AU";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Simulation Contact Campaign";

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			Factory.Save();

			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				form.ContentControl.EmailContentTextBoxExposed.Text = @"<html><head></head><body><p><a href='http://www.test.gov' target='_blank' tid='Tester'>Tester</a></p><p>(*ContactName*)</p></body></html>";
				form.ContentControl.EmailContentTextBoxExposed.DataBindings["Text"].WriteValue();

				AssertContains(testerHref, form.ContentControl.SelectBodyHtml());
				AssertContains(macroContactName, form.ContentControl.SelectBodyHtml());

				campaign.TemplateEditor.SimulationContactPK = contact.PK;
				campaign.TemplateEditor.EnableMacroDataPreview = true;
				string simulationHtmlText = form.ContentControl.GetPreviewContentText_Exposed();

				AssertContains(testerHref, simulationHtmlText);
				AssertContains(simulationContactName, simulationHtmlText);
			}
		}

		public void TestHighlight_WhenContentInHTML()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test campaign";
			campaign.HtmlDocumentBlob = ZBlob.FromAscii(@"<html><body bgcolor='Lime'>Hello There <a href='http://www.test.gov'>Tester</a>Hello There Again<a href='http://www.test2.gov'>Tester2</a>Hello Again <a href='http://www.test3.gov'>Tester2</a></body></html");

			var expectedTextBoxHighlight1 = @"http://www.test2.gov";
			var expectedTextBoxHighlight2 = @"http://www.test3.gov";

			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				Application.DoEvents();

				form.ContentControl.linkGrid.CurrentRowIndex = 1;

				AssertEquals("Highlight One Text Box", expectedTextBoxHighlight1, form.ContentControl.EmailContentTextBoxExposed.SelectedText);

				form.ContentControl.linkGrid.CurrentRowIndex = 2;

				AssertEquals("Highlight Two Text Box", expectedTextBoxHighlight2, form.ContentControl.EmailContentTextBoxExposed.SelectedText);
			}
		}

		class MemberDescriptionForTest : MemberDescription
		{
			public MemberDescriptionForTest(MemberDescription parentMemberDescription, string helpText, MacroTagTypes macroTagType)
				: base(parentMemberDescription, helpText, macroTagType)
			{
			}

			public override bool CanHaveChildMembers()
			{
				return false;
			}

			public override (Type ChildType, Type PossibleCollectionType) GetChildTypes()
			{
				return default;
			}

			public override string GetFullPath()
			{
				return "CampaignName";
			}

			public override string GetFormattedTextLabel()
			{
				return "CampaignName";
			}
		}

		public void TestMacroImagePlaceHolderImagesAreDeletedOnExit()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			string path;
			using (var editor = GetFormForTest(campaign))
			{
				editor.Show();

				path = editor.ContentControl.HtmlEditorTempFileDirectoryForTest.DirectoryName;
				File.WriteAllText(path + @"\aaaa.png", "aaa");
				AssertNotNullOrEmpty(path);
				AssertEquals(true, Directory.Exists(path));
			}

			AssertEquals(false, Directory.Exists(path));
		}

		public void TestInitMacroImageUploadMenu()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();

			Factory.Save();

			using (var editor = GetFormForTest(campaign))
			{
				editor.Show();

				var menuEventArgs = new MacroImageMenuGroupInitEventArgs();
				var menus = new MacroImageMenuGroup("");
				menuEventArgs.Menus = menus;
				editor.ContentControl.HtmlEditorControl_HtmlEditorGuiEvent_Exposed(null, menuEventArgs);

				var parser = new CampaignDocumentParser(Factory);

				AssertIMacroImageMenu(menus, parser, campaignItem);
			}
		}

		void AssertIMacroImageMenu(IMacroImageMenu menu, CampaignDocumentParser parser, GlbCompanyCampaignItem campaignItem)
		{
			if (menu.MacroImageMenuType == typeof(MacroImageMenuGroup))
			{
				var menuGroup = menu as MacroImageMenuGroup;

				foreach (var subMenu in menuGroup.Menus)
				{
					AssertIMacroImageMenu(subMenu, parser, campaignItem);
				}
			}

			if (menu.MacroImageMenuType == typeof(MacroImageMenu))
			{
				var menuItem = menu as MacroImageMenu;
				Assert(!string.IsNullOrWhiteSpace(menuItem.ImageText));
				Assert(!string.IsNullOrWhiteSpace(menuItem.Macro));
				Assert(!string.IsNullOrWhiteSpace(menuItem.MenuText));
				AssertNotNull(menuItem.PlaceholderImage);

				var outputText = parser.Parse(CampaignDocumentParser.ParseType.PlainText, campaignItem, menuItem.Macro);
				AssertEquals("", outputText);
			}
		}

		public void TestHandelUploadLocalImageEvent()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			using (var editor = GetFormForTest(campaign))
			{
				editor.Show();
				var args = new UploadLocalImageEventArgs();
				editor.ContentControl.HtmlEditorControl_HtmlEditorGuiEvent_Exposed(null, args);
				using (args.Dialog)
				{
					AssertNotNull(args.Dialog as ZOpenFileDialog);
					args.Dialog.FileName = @"C:\1.png";
					AssertEquals(@"C:\1.png", args.Dialog.FileName);
				}
			}
		}

		public void TestFindPhraseInHTMLContent()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Testing Campaign Email Find";
			Factory.Save();

			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				form.ContentControl.EmailContentTextBoxExposed.Text = @"<body><p>The big fat cat sat on the green mat</body>";
				//                                                      0123456789012345678901234567890123456789012345678901
				//                                                                1         2         3         4         5
				form.ContentControl.SearchTextBoxExposed.Text = "AT";
				form.ContentControl.SetMatchCaseCheckBox(true);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.ContentControl.FindNextButton_Click();

				AssertEquals("Reached the end of the content, would you like to search from the start?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Nothing found", 0, form.ContentControl.EmailContentTextBoxExposed.SelectionLength);

				form.ContentControl.SetMatchCaseCheckBox(false);

				form.ContentControl.FindNextButton_Click();

				AssertEquals("first at found", 2, form.ContentControl.EmailContentTextBoxExposed.SelectionLength);
				AssertEquals("first at position", 18, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);

				form.ContentControl.FindNextButton_Click();

				AssertEquals("second at found", 2, form.ContentControl.EmailContentTextBoxExposed.SelectionLength);
				AssertEquals("second at position", 22, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);

				form.ContentControl.FindNextButton_Click();

				AssertEquals("third at found", 2, form.ContentControl.EmailContentTextBoxExposed.SelectionLength);
				AssertEquals("third at position", 26, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);

				form.ContentControl.FindNextButton_Click();

				AssertEquals("fourth at found", 2, form.ContentControl.EmailContentTextBoxExposed.SelectionLength);
				AssertEquals("fourth at position", 43, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.ContentControl.FindNextButton_Click();

				AssertEquals("Reached the end of the content, would you like to search from the start?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Selection Start still at same position", 43, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);

				form.ContentControl.FindPreviousButton_Click();

				AssertEquals("first previous at found", 2, form.ContentControl.EmailContentTextBoxExposed.SelectionLength);
				AssertEquals("first previous at position", 26, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);

				form.ContentControl.FindPreviousButton_Click();

				AssertEquals("second previous at found", 2, form.ContentControl.EmailContentTextBoxExposed.SelectionLength);
				AssertEquals("second previous at position", 22, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);

				form.ContentControl.FindPreviousButton_Click();

				AssertEquals("third previous at found", 2, form.ContentControl.EmailContentTextBoxExposed.SelectionLength);
				AssertEquals("third previous at position", 18, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.ContentControl.FindPreviousButton_Click();

				AssertEquals("Reached the start of the content, would you like to search from the end?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Selection Start still at same position", 18, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ContentControl.FindPreviousButton_Click();

				AssertEquals("Reached the start of the content, would you like to search from the end?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("from end previous at found", 2, form.ContentControl.EmailContentTextBoxExposed.SelectionLength);
				AssertEquals("from end at position", 43, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ContentControl.FindNextButton_Click();

				AssertEquals("Reached the end of the content, would you like to search from the start?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("from start next at found", 2, form.ContentControl.EmailContentTextBoxExposed.SelectionLength);
				AssertEquals("from start at position", 18, form.ContentControl.EmailContentTextBoxExposed.SelectionStart);
			}
		}

		public void TestEmailContentWordWrapEnabled()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();

			using (var form = GetFormForTest(campaign))
			{
				AssertEquals(false, form.ContentControl.EmailContentTextBoxExposed.WordWrap);
			}
		}

		public void TestEmailContentHorizontalScrollBarEnabled()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();

			using (var form = GetFormForTest(campaign))
			{
				AssertEquals(ScrollBars.Both, form.ContentControl.EmailContentTextBoxExposed.ScrollBars);
			}
		}

		public void TestPreviewContentLink()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			string htmlFilePath = string.Empty;
			const string htmlText = "<html><body>Hello World</body></html>";

			try
			{
				using (var form = GetFormForTest(campaign))
				{
					form.Show();
					form.ContentControl.EmailContentTextBoxExposed.Text = htmlText;
					form.ContentControl.PreviewContentLink_Click();
					htmlFilePath = form.ContentControl.HtmlFilePath;
				}

				AssertNotEquals("Filename set", string.Empty, htmlFilePath);

				var content = File.ReadAllText(htmlFilePath);
				AssertEquals("File contents", htmlText, content);
			}
			finally
			{
				if (!string.IsNullOrEmpty(htmlFilePath) && File.Exists(htmlFilePath))
				{
					File.Delete(htmlFilePath);
				}
			}
		}

		public void TestInsertImageClick()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();

			using (var form = GetFormForTest(campaign))
			{
				form.Show();
				form.ContentControl.EmailContentTextBoxExposed.Text = "<html><body></body></html>";
				//                                                     123456789012
				form.ContentControl.EmailContentTextBoxExposed.SelectionStart = 12;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is ImageDialog imageDialog)
					{
						imageDialog.ImageURL = "https://www.nine.com/a.jpg";
					}
				});
				form.ContentControl.InsertImageButton_Click();

				AssertEquals("With image inserted", @"<html><body><img src=""https://www.nine.com/a.jpg"" style=""border-color:White;border-style:None;"" /></body></html>", form.ContentControl.EmailContentTextBoxExposed.Text);
			}
		}

		#region Implementation

		public class DummyCampaignForm : ZChildForm
		{
			public DummyCampaignForm(GlbCompanyCampaign campaign)
				: base(campaign)
			{
				this.CaptionRenderingEnabled = true;
			}

			public ContentDesignerControlForTest ContentControl;

			protected override void InitializeComponent()
			{
				ContentControl = new ContentDesignerControlForTest();

				ContentControl.SuspendLayout();
				Controls.Add(ContentControl);

				ContentControl.AllowDrop = true;
				BindingSource.SetBindingMember(ContentControl, ".");
				ContentControl.Dock = System.Windows.Forms.DockStyle.Fill;
				ContentControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				ContentControl.Name = "ContentControl";
				ContentControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1346, 653);
				ContentControl.TabIndex = 0;
				ContentControl.ResumeLayout(true);
				ContentControl.PerformLayout();
			}
		}

		DummyCampaignForm GetFormForTest(GlbCompanyCampaign campaign)
		{
			return new DummyCampaignForm(campaign);
		}

		public class ContentDesignerControlForTest : ContentDesignerControl
		{
			public ContentDesignerControlForTest() : base() { }

			public TempDirectory HtmlEditorTempFileDirectoryForTest => HtmlEditorTempFileDirectory;

			bool isCargoWiseRemote;
			string htmlFilePath = string.Empty;

			protected override bool IsCargoWiseRemote => isCargoWiseRemote;

#if !WINZOR
			bool mappedPathFound;

			protected override IMappedClientPath GetMappedClientPath()
			{
				return new TestMappedClientPath() { FindMappedPath = mappedPathFound };
			}
#endif

			protected override void OpenHTMLFile(string filePath)
			{
				htmlFilePath = filePath;
			}

			public void SetIsRemote(bool value)
			{
				isCargoWiseRemote = value;
			}

			public void SetMappedPathFound(bool value)
			{
#if !WINZOR
				mappedPathFound = value;
#endif
			}

			public string SelectBodyHtml()
			{
				string fullHtml = EmailContentTextBoxExposed.Text;
				return Regex.Match(fullHtml, @"<body[^>]*>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
			}

			public MapTreeUserControl MapTreeUserControl
			{
				get { return mapTreeUserControl; }
			}

			public string HtmlFilePath
			{
				get { return htmlFilePath; }
			}

			public void CopyMacroButton_Click()
			{
				base.CopyMacroButton_Click(null, EventArgs.Empty);
			}

			public void AddMacroButton_Click()
			{
				base.AddMacroButton_Click(null, EventArgs.Empty);
			}

			public void InsertImageButton_Click()
			{
				base.InsertImageButton_Click(null, EventArgs.Empty);
			}

			public void BrowseButton_Click()
			{
				base.UploadButton_Click(null, EventArgs.Empty);
			}

			public void HTMLFileDialog_FileOk()
			{
				base.HTMLFileDialog_FileOk(null, new CancelEventArgs(false));
			}

			public void ClearButton_Click()
			{
				base.ClearButton_Click(null, EventArgs.Empty);
			}

			public void SaveButton_Click()
			{
				base.SaveButton_Click(null, EventArgs.Empty);
			}

			public void CopyToClipboard_Click()
			{
				base.CopyToClipboard_Click(null, EventArgs.Empty);
			}

			public void TrackAllLinksButton_Click()
			{
				base.TrackAllLinksButton_Click(null, EventArgs.Empty);
			}

			public void FindNextButton_Click()
			{
				FindNextButton_Click(null, EventArgs.Empty);
			}

			public void FindPreviousButton_Click()
			{
				FindPreviousButton_Click(null, EventArgs.Empty);
			}

			public void PreviewContentLink_Click()
			{
				PreviewContentLink_Click(null, new LinkLabelLinkClickedEventArgs(new LinkLabel.Link()));
			}

			public ZTextBox SearchTextBoxExposed
			{
				get { return base.searchTextBox; }
			}

			public void SetMatchCaseCheckBox(bool value)
			{
				base.matchCaseCheckbox.Checked = value;
			}

			public ImageDialog ImageDialog_Exposed
			{
				get
				{
					return ImageDialog;
				}
			}

			public ZTextBox EmailContentTextBoxExposed
			{
				get { return base.emailContentTextBox; }
			}

			public CargoWise.Windows.UI.KTextBox InfoTextBoxExposed
			{
				get { return base.infoTextBox; }
			}

			public void MapTreeUserControlExposed_NodeSelected(MapTreeNode nodeSelected)
			{
				base.MapTreeUserControl_NodeSelected(nodeSelected);
			}

			public void MapTreeUserControlExposed_NodeDoubleClicked(MapTreeNode nodeSelected)
			{
				base.MapTreeUserControl_NodeDoubleClicked(nodeSelected);
			}

			public string GetPreviewContentText_Exposed()
			{
				return base.GetPreviewContentText();
			}

			public void HtmlEditorControl_HtmlEditorGuiEvent_Exposed(object sender, EventArgs e)
			{
				base.HtmlEditorControl_HtmlEditorGuiEvent(sender, e);
			}
		}

		#endregion
	}
}

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.GUI.ReflectiveFieldMap;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class ContentDesignerControl : ZUserControl
	{
		public ContentDesignerControl()
		{
			InitializeComponent();
		}

		public new GlbCompanyCampaign CurrentDataItem => (GlbCompanyCampaign)base.CurrentDataItem;

		protected CampaignEmailTemplateEditor TemplateEditor => CurrentDataItem?.TemplateEditor;

		protected virtual bool IsCargoWiseRemote => ZOpenFileDialog.IsRemote;

		bool isLoaded;

#if !WINZOR
		protected virtual IMappedClientPath GetMappedClientPath()
		{
			return new MappedClientPath();
		}
#endif

		protected ImageDialog ImageDialog;

		static string InvalidImageSourceWarning => Res.GetString("baccadb6-8830-427c-88ff-6236a85282b1", @"Only HTTP and HTTPS URLs with the following image extensions are supported within the image source attribute:
{0}
The content you have uploaded contains file paths or unsupported extensions in the image source attribute, and such sources have been removed.", string.Join(", ", CampaignEmailTemplateEditor.SupportedExtensions));

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			AddContactGuidFindBox();

			if (DesignModeFinder.IsDesigning)
			{
				return;
			}

			emailContentTextBox.MaxLength = int.MaxValue;
			emailContentTextBox.Select(0, 0);

			TemplateEditor.SetTemplateBlob += SetTemplateBlob;
			TemplateEditor.RefreshHostHandler += TemplateEditor_RefreshHostHandler;
			TemplateEditor.PreLinkValueChanged += TemplateEditor_PreLinkValueChanged;
			TemplateEditor.SimulationContactPKInfo.ValueChanged += SimulationContactPKInfo_ValueChanged;

			if (string.IsNullOrEmpty(TemplateEditor.TemplateHtmlText))
			{
				TemplateEditor.ClearTemplateHtml();
			}
			else
			{
				_ = RemoveEmbeddedImage(TemplateEditor.TemplateHtmlText, true);
			}

			mapTreeUserControl.SetReflectors(new DocDataProviderReflector[]
			{
				new DocDataProviderReflector(ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocCompanyCampaignItem>(), MemberDescription.MacroTagTypes.Email),
				new DocDataProviderReflector(typeof(GlbCompanyCampaignItem), MemberDescription.MacroTagTypes.Email)
			});
			mapTreeUserControl.NodeDoubleClicked += MapTreeUserControl_NodeDoubleClicked;
			mapTreeUserControl.NodeSelected += MapTreeUserControl_NodeSelected;
			mapTreeUserControl.ExpandTreeViewNode(0);

			linkGrid.ListManager.CurrentChanged += LinkGrid_CurrentCellChanged;

			splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
			originalWidth = Width;
			splitWidth = splitContainer1.SplitterDistance;
			splitContainer3.Panel2.Resize += Panel2_Resize;
			ContactGuidFindBox.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);

			this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;

			isLoaded = true;
		}

		void AddContactGuidFindBox()
		{
			ContactGuidFindBox = new ZGuidFindBox();
			ContactGuidFindBox.SuspendLayout();
			AddPanel2Control(ContactGuidFindBox);

			// 
			// ContactGuidFindBox
			// 
			ContactGuidFindBox.AllowDrop = true;
			ContactGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			ContactGuidFindBox.AutoSize = true;
			SetContactBindingMember();
			ContactGuidFindBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("390ad27b-1705-41ae-9ceb-cb31eaf34a01", "Simulation Contact");
			ContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 1, true);
			ContactGuidFindBox.ModuleID = GetModuleID();
			ContactGuidFindBox.Name = "ContactGuidFindBox";
			ContactGuidFindBox.ShowDescriptionBox = false;
			ContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			ContactGuidFindBox.TabIndex = 3;

			ContactGuidFindBox.ResumeLayout(true);
			ContactGuidFindBox.PerformLayout();
		}

		protected ZGuidFindBox ContactGuidFindBox;

		protected SplitterPanel GetPanel2()
		{
			return splitContainer3.Panel2;
		}

		protected void AddPanel2Control(Control control)
		{
			splitContainer3.Panel2.Controls.Add(control);
		}

		protected void InfoTextBox_ResizeAndRelocate()
		{
			var height = infoTextBox.Height - CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20);
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetHeight(infoTextBox, height, false);
			infoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 43);
		}

		protected virtual void SetContactBindingMember()
		{
			BindingSource.SetBindingMember(ContactGuidFindBox, "TemplateEditor.SimulationContactPK");
		}

		protected virtual ZArchitecture.Modules.ModuleIdentifier GetModuleID()
		{
			return Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
		}

		protected virtual void Panel2_Resize(object sender, EventArgs e)
		{
			var width = splitContainer3.Panel2.Width - CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(136);
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(ContactGuidFindBox.CodeBox, width, false);
		}

		void SimulationContactPKInfo_ValueChanged(object sender, EventArgs e)
		{
			TemplateEditor.EnableMacroDataPreview = true;
			if (mapTreeUserControl.SelectedTreeNode is MapTreeNode selectedNode)
			{
				MapTreeUserControl_NodeSelected(selectedNode);
			}
		}

		void TemplateEditor_RefreshHostHandler()
		{
			ClearFormAcceptButtonFocus();

			if (linkGrid.List.IsSorted)
			{
				TemplateEditor.TrackedLinks.Sort(TemplateEditor.TrackedLinks.SortInformation);
			}
		}

		void TemplateEditor_PreLinkValueChanged()
		{
			emailContentTextBox.DataBindings["Text"].WriteValue();
		}

		#region Calculate Control Positions

		int originalWidth;
		int splitWidth;
		bool startMoving;

		void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (startMoving)
			{
				splitWidth = splitContainer1.SplitterDistance;
			}
		}

		protected override void OnResize(EventArgs e)
		{
			startMoving = false;

			base.OnResize(e);

			if (originalWidth > 0 && (Width - originalWidth + splitWidth) > 0)
			{
				splitContainer1.SplitterDistance = Width - originalWidth + splitWidth;
			}

			startMoving = true;
		}

		#endregion

		void DocumentParseExceptionMessage(DocumentEngineException ex)
		{
			var pattern = @"\<(.*?)\>";
			string reportString = string.Empty;
			var matches = Regex.Matches(ex.Message, pattern);
			foreach (Match match in matches)
			{
				reportString += "(*" + match.Groups[1] + "*)\r\n";
			}

			var message = Res.GetString("5ff50c57-5066-4c9a-af0b-a13d01a49c49",
@"There was a problem trying to evaluate the macro template.
Macros cannot be simulated until the following macro(s) are resolved/removed:

{0}", (!string.IsNullOrEmpty(reportString) ? reportString : ex.Message));

			Globals.Message.ShowError(message);
		}

		string UriFormatExceptionMessage(string message)
		{
			return Res.GetString("1f7251a4-0ba2-4bfe-92a4-2e3b63f0eddb",
@"There was a problem trying to evaluate the macro template.

{0}", message);
		}

		protected string GetSimulationHtml()
		{
			return TemplateEditor.EmailParser.Parse(CampaignDocumentParser.ParseType.HtmlPreview, CreateMacroSimulationCampaignItem(), TemplateEditor.TemplateHtmlText).ToString();
		}

		protected void SetTemplateBlob(object sender, CampaignEmailTemplateEditor.HtmlTextEventArgs e)
		{
			e.ConvertedHtml = FilterMsWordHtmlHelper.GetFilteredOrRawHtmlContentFromUpload(e.Html);
		}

		protected void MapTreeUserControl_NodeSelected(MapTreeNode nodeSelected)
		{
			if (nodeSelected != null)
			{
				if (!TemplateEditor.SimulationContactPK.IsEmpty)
				{
					bool isParsed = false;
					nodeSelected.MemberDescription.UsePreviewText = true;
					string macroText = "";
					try
					{
						try
						{
							macroText = TemplateEditor.EmailParser.Parse(CampaignDocumentParser.ParseType.PlainText, CreateMacroSimulationCampaignItem(), nodeSelected.MemberDescription.GetMemberInformation());
							isParsed = true;
						}
						catch (DocumentEngineException ex)
						{
							DocumentParseExceptionMessage(ex);
						}
						catch (UriFormatException ex)
						{
							Globals.Message.ShowError(UriFormatExceptionMessage(ex.Message));
						}
					}
					finally
					{
						if (!isParsed)
						{
							macroText = Res.GetString("979aa198-62fb-4855-b2e0-cc4d8a7e7290", "{0}\r\n\r\nMacro could not be evaluated.", nodeSelected.MemberDescription.GetMemberInformation());
						}
					}

					infoTextBox.Text = macroText;
				}
				else
				{
					nodeSelected.MemberDescription.UsePreviewText = false;
					infoTextBox.Text = nodeSelected.MemberDescription.GetMemberInformation();
				}
			}
		}

		GlbCompanyCampaignItem CreateMacroSimulationCampaignItem()
		{
			using (TemplateEditor.SuspendSettingHasChanges())
			{
				var newFactory = new BusinessObjectFactory();
				var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(TemplateEditor.Campaign);
				var campaignItem = campaignInNewFactory.CampaignsItemsSent.AddNew();
				campaignItem.G8_G0 = TemplateEditor.Campaign.PK;
				if (!TemplateEditor.SimulationContactPK.IsEmpty)
				{
					return SetupCampaignItem(campaignItem);
				}
				return campaignItem;
			}
		}

		protected virtual GlbCompanyCampaignItem SetupCampaignItem(GlbCompanyCampaignItem campaignItem)
		{
			campaignItem.G8_RecipientID = TemplateEditor.SimulationContactPK;
			return campaignItem;
		}

		protected void MapTreeUserControl_NodeDoubleClicked(MapTreeNode nodeSelected)
		{
			if (nodeSelected != null)
			{
				emailContentTextBox.SelectedText = RemovePreviewKeywordFromMacro(nodeSelected.MemberDescription.GetMacro());
			}
		}

		void EmailContentTextBox_TextChanged(object sender, EventArgs e)
		{
			if (isLoaded)
			{
				TemplateEditor.Campaign.HasChanges = true;
			}
		}

		protected void CopyMacroButton_Click(object sender, EventArgs e)
		{
			if (mapTreeUserControl?.SelectedTreeNode is MapTreeNode mapTreeNode)
			{
				string macro = mapTreeNode.MemberDescription.GetMacro();
				SafeClipboard.Clear();

				if (!string.IsNullOrEmpty(macro))
				{
					SafeClipboard.SetText(macro);
				}
			}
		}

		protected void AddMacroButton_Click(object sender, EventArgs e)
		{
			if (mapTreeUserControl?.SelectedTreeNode is MapTreeNode mapTreeNode)
			{
				emailContentTextBox.SelectedText = RemovePreviewKeywordFromMacro(mapTreeNode.MemberDescription.GetMacro());

				ClearFormAcceptButtonFocus();
			}
		}

		protected void TrackAllLinksButton_Click(object sender, EventArgs e)
		{
			var linkArray = TemplateEditor.TrackedLinks.ToArray();
			foreach (GlbCompanyCampaignLink link in linkArray)
			{
				link.HasTrackingMacro = true;
			}
		}

		string RemovePreviewKeywordFromMacro(string macroText)
		{
			return macroText.Replace("Preview:\r\n", "");
		}

		internal ContinueWithSave RecordChangesIfAny()
		{
			if (isLoaded && !TemplateEditor.TrackedLinks.HasErrors())
			{
				if (TemplateEditor.SyncEmbeddedImages())
				{
					Globals.Message.ShowWarning(InvalidImageSourceWarning, Res.GetString("3d82dd70-6598-40bc-9650-bec74eb2331e", "Invalid image source in image source attribute"));
				}
				TemplateEditor.SetCampaignEmailTemplate();
				TemplateEditor.Campaign.StatModel.ReloadLinksAndClicks();
			}
			return ContinueWithSave.Yes;
		}

		protected void UploadButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowCommonDialogWithoutDispose(htmlFileDialog);
			ClearFormAcceptButtonFocus();
		}

		protected void InsertImageButton_Click(object sender, EventArgs e)
		{
			using (ImageDialog = new ImageDialog())
			{
				ImageDialog.HtmlEditorGuiEvent += HtmlEditorControl_HtmlEditorGuiEvent;
				ImageDialog.HtmlEditorTempFilePath = HtmlEditorTempFileDirectory.DirectoryName;

				ImageDialog.Element = new ImageElementWithMacro();
				if (ZFormModaliser.ShowDialogWithoutDispose(ImageDialog) == DialogResult.OK)
				{
					emailContentTextBox.SelectedText = ImageDialog.Element.ToHtmlStringWithMacro();
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File filter")]
		protected void SaveButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZSaveFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.Filter = "HTML Files (*.html)|*.html|SHTML Files (*.shtml)|*.shtml|HTM Files (*.htm)|*.htm|Text Files (*.txt)|*.txt";
				dialog.DefaultExt = "htm";
				dialog.AddExtension = true;
				dialog.FileName = TemplateEditor.Campaign.CampaignID;

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == System.Windows.Forms.DialogResult.OK)
				{
					Stream toFile = dialog.OpenFile();
					try
					{
						emailContentTextBox.DataBindings["Text"].WriteValue();

						using (StreamWriter writer = new StreamWriter(toFile, Encoding.UTF8))
						{
							writer.Write(TemplateEditor.HtmlTextWithoutMacros);
						}
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						Globals.Message.ShowError(ResString.GetMultilingualString("585d54a1-b3af-494c-88e4-0680a65f0d34", "Saving error message: {0}", exception.Message));
					}
				}
			}

			ClearFormAcceptButtonFocus();
		}

		protected void CopyToClipboard_Click(object sender, EventArgs e)
		{
			emailContentTextBox.DataBindings["Text"].WriteValue();
			try
			{
				SafeClipboard.SetData(DataFormats.Text, TemplateEditor.HtmlTextWithoutMacros);
			}
			catch (DocumentParsingFailedException ex)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("E6D23AC6-2CA5-4253-930F-55D3ABA9D633", "Error copying to clipboard massage: {0}", ex.Message));
			}
			ClearFormAcceptButtonFocus();
		}

		protected void FindNextButton_Click(object sender, EventArgs e)
		{
			var startPosition = emailContentTextBox.SelectionStart < 0 ? 0 : emailContentTextBox.SelectionStart + emailContentTextBox.SelectionLength;
			var edgeMessage = ResString.GetMultilingualString("D2FB5B1F-D3ED-49F0-9102-AB8A204E3B8F", "Reached the end of the content, would you like to search from the start?");
			DoSearch(startPosition, true, edgeMessage);
		}

		protected void FindPreviousButton_Click(object sender, EventArgs e)
		{
			var edgeMessage = ResString.GetMultilingualString("D995B176-98FB-4777-AC95-DE62389AB089", "Reached the start of the content, would you like to search from the end?");
			var startPosition = emailContentTextBox.SelectionStart < 0 ? 0 : emailContentTextBox.SelectionStart;

			DoSearch(startPosition, false, edgeMessage);
		}

		protected void DoSearch(int startPosition, bool searchForward, string edgeMessage)
		{
			var searchPhrase = searchTextBox.Text;
			var matchCase = matchCaseCheckbox.Checked;

			var newPosition = GetNewPosition(searchPhrase, startPosition, searchForward, matchCase);

			if (newPosition < 0)
			{
				var caption = Res.GetString("F8F0F1DF-BAFD-4BF1-A54F-9155C23C099D", "Find Search Phrase");

				if (DialogResult.Yes == Globals.Message.Show(edgeMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes))
				{
					newPosition = GetNewPosition(searchPhrase, searchForward ? 0 : emailContentTextBox.Text.Length - 1, searchForward, matchCase);
					if (newPosition < 0)
					{
						emailContentTextBox.Focus();
						emailContentTextBox.SelectionLength = 0;
						emailContentTextBox.SelectionStart = emailContentTextBox.Text.Length - 1;
						emailContentTextBox.ScrollToCaret();
					}
				}
			}

			if (newPosition >= 0)
			{
				emailContentTextBox.Focus();
				emailContentTextBox.SelectionLength = searchPhrase.Length;
				emailContentTextBox.SelectionStart = newPosition;
				emailContentTextBox.ScrollToCaret();
			}
		}

		protected int GetNewPosition(string searchPhrase, int fromPosition, bool searchForward, bool matchCase)
		{
			if (searchForward)
			{
				return emailContentTextBox.Text.IndexOf(searchPhrase, fromPosition, matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
			}
			else
			{
				return emailContentTextBox.Text.LastIndexOf(searchPhrase, fromPosition, matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File extension")]
		protected void PreviewContentLink_Click(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var tempFullFileName = Temp.GetTempFileNameWithExtension("html");

			emailContentTextBox.DataBindings["Text"].WriteValue();

			File.WriteAllText(tempFullFileName, GetPreviewContentText(), Encoding.UTF8);

			OpenHTMLFile(tempFullFileName);
		}

		protected string GetPreviewContentText()
		{
			string contentHtml = string.Empty;
			string errorMessage = null;
			try
			{
				contentHtml = TemplateEditor.EnableMacroDataPreview ? GetSimulationHtml() : TemplateEditor.TemplateHtmlText.ToString();
			}
			catch (DocumentEngineException ex)
			{
				DocumentParseExceptionMessage(ex);
				contentHtml = TemplateEditor.TemplateHtmlText.ToString();
			}
			catch (UriFormatException ex)
			{
				Globals.Message.ShowError(UriFormatExceptionMessage(ex.Message));
				contentHtml = TemplateEditor.TemplateHtmlText.ToString();
			}
			catch (DocumentParsingFailedException ex)
			{
				errorMessage = ex.Message;
			}
			return errorMessage ?? contentHtml;
		}

		protected virtual void OpenHTMLFile(string filePath)
		{
			FileOpener.Open(filePath);
		}

		protected void HTMLFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string fileExtension = Path.GetExtension(htmlFileDialog.UnmappedFileName);
			if (TemplateEditor.ValidFileExtensions.Contains(fileExtension.ToUpperInvariant()))
			{
				try
				{
					TemplateEditor.TemplateFileName = htmlFileDialog.UnmappedFileName;

					IMappedClientPath mappedClientPath = null;
					if (IsCargoWiseRemote)
					{
#if !WINZOR
						mappedClientPath = GetMappedClientPath();
						if (string.IsNullOrEmpty(mappedClientPath.GetMappedPath(htmlFileDialog.UnmappedFileName)))
						{
							string message = Res.GetString("2541ee23-05a1-45da-94fb-37904c616287", "The drive of the selected file is not mapped for Remote Desktop. This is required to use the Upload functionality.");
							Globals.Message.ShowError(message, Res.GetString("cd05abc3-9ffa-4b46-8b1d-f2e27274fbbc", "Upload File"));
							return;
						}
#endif
					}

					using (var fileStream = htmlFileDialog.OpenFile())
					{
						var bytes = fileStream.ToByteArray();
						if (CampaignEmailTemplateEditor.IsGeneratedFromWordButNotFiltered(bytes))
						{
							string message = Res.GetString("856395c2-db90-44ec-afb3-b7124bf2b26a", @"When saving a Word document for HTML please save it as ""Web Page, Filtered"" rather than ""Web Page"".");
							Globals.Message.ShowError(message, Res.GetString("cd05abc3-9ffa-4b46-8b1d-f2e27274fbbc", "Upload File"));
							return;
						}

						TemplateEditor.TemplateBlob = bytes;
					}

					var htmlText = TemplateEditor.TemplateHtmlText;

					var filePath = Path.GetDirectoryName(htmlFileDialog.UnmappedFileName);

					htmlText = CampaignEmailTemplateEditor.EmbedImagesInHtmlFromLocalDirectory(filePath, htmlText, mappedClientPath);
					htmlText = CampaignEmailTemplateEditor.RemoveContentEditableAttributes(htmlText);
					htmlText = RemoveEmbeddedImage(htmlText);

					TemplateEditor.SetTemplateHtmlText(htmlText, CampaignEmailTemplateEditor.HtmlTextSource.TextEditor);

					TemplateEditor.SyncImageTrackIDAttributeAndTrackingMacro();
					TemplateEditor.Campaign.HasChanges = true;
				}
				catch (IOException ex)
				{
					TemplateEditor.TemplateBlob = ZBlob.Empty;
					Globals.Message.ShowError(ex.Message, Res.GetString("0c9fb097-b057-4e9c-8a7c-12f36ffd06da", "Could not attach file"));
				}
			}
			else
			{
				string invalidExtensionMessage = Res.GetString("781bef09-c206-4571-94d9-ae74b7100226", "The selected file has an invalid file extension of type {0}. Please choose another file with either a .htm or .html extension", fileExtension);
				Globals.Message.ShowError(invalidExtensionMessage, Res.GetString("0c9fb097-b057-4e9c-8a7c-12f36ffd06da", "Could not attach file"));
			}
		}

		protected void ClearButton_Click(object sender, EventArgs e)
		{
			TemplateEditor.TemplateBlob = ZBlob.Empty;
			TemplateEditor.ClearTemplateHtml();
			TemplateEditor.TemplateFileName = ZString.Empty;

			ClearFormAcceptButtonFocus();
		}

		void linkGrid_DoubleClick(object sender, EventArgs e)
		{
			if (linkGrid.ListManager.List.Cast<GlbCompanyCampaignLink>().Any())
			{
				GlbCompanyCampaignLink selectedItem = (GlbCompanyCampaignLink)linkGrid.ListManager.GetCurrent();
				if (selectedItem != null)
				{
					WebUrlLauncher.Launch(selectedItem.GCL_URL);
				}
			}
		}

		#region Highlight

		void LinkGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (linkGrid.CurrentRowIndex != lastLinkGridIndex)
			{
				HighlightCurrentLink();
				lastLinkGridIndex = linkGrid.CurrentRowIndex;
			}
		}
		int lastLinkGridIndex = -1;

		void HighlightCurrentLink()
		{
			var link = linkGrid.ListManager.GetCurrent() as GlbCompanyCampaignLink;

			if (link != null)
			{
				int start;
				int length;
				var urlCount = TemplateEditor.HighlightLink(link, out start, out length);
				emailContentTextBox.HideSelection = false;
				emailContentTextBox.Select(start, 0);
				emailContentTextBox.ScrollToCaret();
				emailContentTextBox.Select(start, length);
			}
		}

		#endregion Highlight

		void ClearFormAcceptButtonFocus()
		{
			infoTextBox.Focus();
			infoTextBox.Select(0, 0);
		}

		internal string RemoveEmbeddedImage(string html, bool saveTemplate = false)
		{
			var hasEmbeddedImage = false;
			var htmlText = CampaignEmailTemplateEditor.RemoveEmbeddedImage(html, ref hasEmbeddedImage);
			if (hasEmbeddedImage)
			{
				Globals.Message.ShowWarning(InvalidImageSourceWarning, Res.GetString("3d82dd70-6598-40bc-9650-bec74eb2331e", "Invalid image source in image source attribute"));
				if (saveTemplate)
				{
					TemplateEditor.SetTemplateHtmlText(htmlText, CampaignEmailTemplateEditor.HtmlTextSource.TextEditor);
				}
			}
			return htmlText;
		}

		#region Macro Image

		protected void HtmlEditorControl_HtmlEditorGuiEvent(object sender, EventArgs e)
		{
			var menuEventArgs = e as MacroImageMenuGroupInitEventArgs;
			if (menuEventArgs != null)
			{
				InitMacroImageUploadMenu(menuEventArgs.Menus);
				return;
			}

			var uploadLocalImageEventArgs = e as UploadLocalImageEventArgs;
			if (uploadLocalImageEventArgs != null)
			{
				HandelUploadLocalImageEvent(uploadLocalImageEventArgs);
				return;
			}
		}

		void InitMacroImageUploadMenu(MacroImageMenuGroup menus)
		{
			var profileImageMenuText = ResString.GetMultilingualString("8C4CC67D-2545-4BF8-80AE-8CBF75CCC801", "Staff Profile Photo");
			var signatureImageMenuText = ResString.GetMultilingualString("A11CF552-B77D-437B-9794-E4FDF180A2E7", "Staff Profile Signature");

			var emailSenderMenuText = ResString.GetMultilingualString("016B70AF-0D34-4F4A-A79E-6E35690A6024", "Email Sender");
			var staffAssignedMenuText = ResString.GetMultilingualString("87B1876A-966D-4D5D-97F1-3E82A9AF3473", "Staff Assignment");

			var profileImage = Properties.Resources.ProfileImage;
			var signatureImage = Properties.Resources.SignatureImage;

			//the hardcoded macro is guarded by unit test
			var senderProfileMacro = "(*EmailSenderStaff.ProfilePhotoEncoded*)";
			var senderSignatureMacro = "(*EmailSenderStaff.SignatureEncoded*)";
			var staffAssignedProfileMacro = "(*GetStaffAssignment({0}).ProfilePhotoEncoded*)";
			var staffAssignedSignatureMacro = "(*GetStaffAssignment({0}).SignatureEncoded*)";

			var profileMenu = menus.AddMenuGroup(profileImageMenuText);
			profileMenu.AddMenu(emailSenderMenuText, profileImage, senderProfileMacro, profileImageMenuText);
			var profileStaffs = profileMenu.AddMenuGroup(staffAssignedMenuText);

			var signatureMenu = menus.AddMenuGroup(signatureImageMenuText);
			signatureMenu.AddMenu(emailSenderMenuText, signatureImage, senderSignatureMacro, signatureImageMenuText);
			var signatureStaffs = signatureMenu.AddMenuGroup(staffAssignedMenuText);

			foreach (ICodeDescription role in (new BusinessObjectFactory()).New<OrgStaffAssignments>().Lookups.StaffRoles)
			{
				var menuText = ZString.Format("{0} - {1}", role.Code, role.Description);

				profileStaffs.AddMenu(menuText, profileImage, ZString.Format(staffAssignedProfileMacro, role), profileImageMenuText);
				signatureStaffs.AddMenu(menuText, signatureImage, ZString.Format(staffAssignedSignatureMacro, role), signatureImageMenuText);
			}
		}

		void HandelUploadLocalImageEvent(UploadLocalImageEventArgs uploadLocalImageEventArgs)
		{
			uploadLocalImageEventArgs.Dialog = new WrappedZOpenFileDialog();
		}

		protected TempDirectory HtmlEditorTempFileDirectory = new TempDirectory();

		#endregion
	}

	class ProgressFormDirectUpdateLabel : ProgressForm
	{
		public void SetProgressLabel(ZString text)
		{
			Status = text;
			this.ProgressLabel.Text = Status;
		}
	}
}

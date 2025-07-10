using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class ImageDialog : ZChildForm, IImageDialog
	{
		float? widthToHeightAspectRatio;
		bool toolTipShown;

		public ImageDialog()
			: base()
		{
			InitializeComponent();
			chkBorderColor.AllowOverlap(chkBorderStyle);
		}

		public event EventHandler HtmlEditorGuiEvent;

		internal void OnHtmlEditorGuiEvent(object sender, EventArgs e)
		{
			if (HtmlEditorGuiEvent != null)
			{
				HtmlEditorGuiEvent(sender, e);
			}
		}

		public ImageElementWithMacro Element
		{
			get { return ReadUi(); }
			set
			{
				UpdateUi(value);
			}
		}

		public string HtmlEditorTempFilePath { get; set; }

		public string ImageMacro { get; set; }
		public string ImageLinkForMacro { get; set; }

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string ImageURL { get; set; }

		protected override bool ShowStatusBar => false;

		protected override void UpdateStatusBar(string notification, INotificationType notificationType)
		{
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void ImageInsertDialog_Load(object sender, EventArgs e)
		{
			InitUploadImageMenuStrip();
		}

		void InitUploadImageMenuStrip()
		{
			UploadImageMenuStrip.Items.Clear();
			var menus = new MacroImageMenuGroup("");
			OnHtmlEditorGuiEvent(this, new MacroImageMenuGroupInitEventArgs() { Menus = menus });
			ParseMacroImageMenu(UploadImageMenuStrip.Items, menus);
		}

		[SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "3rd party controls")]
		void ParseMacroImageMenu(ToolStripItemCollection toolStripItems, MacroImageMenuGroup menuGroup)
		{
			foreach (var item in menuGroup.Menus)
			{
				if (item.MacroImageMenuType == typeof(MacroImageMenuGroup))
				{
					var macroImageMenuGroup = item as MacroImageMenuGroup;
					var subToolStripMenu = new ToolStripMenuItem(macroImageMenuGroup.MenuText);
					toolStripItems.Add(subToolStripMenu);
					ParseMacroImageMenu(subToolStripMenu.DropDownItems, macroImageMenuGroup);
				}

				if (item.MacroImageMenuType == typeof(MacroImageMenu))
				{
					var menu = item as MacroImageMenu;
					toolStripItems.Add(menu.MenuText, null,
						(sender, e) => UploadMacroImage(menu));
				}
			}
		}

		void btnBrowseFile_Click(object sender, EventArgs e)
		{
			Uri.TryCreate(this.txtURL.Text, UriKind.Absolute, out var fileUri);
			var uriIsFile = fileUri?.IsFile ?? false;

			var eventArgs = new UploadLocalImageEventArgs();
			OnHtmlEditorGuiEvent(this, eventArgs);

			using (var dialog = eventArgs.Dialog)
			{
				dialog.Filter = MakeSupportedExtensionsFilterString();
				dialog.FilterIndex = 0;
				dialog.RestoreDirectory = true;
				dialog.FileName = uriIsFile ? fileUri.AbsolutePath : "";
				dialog.InitialDirectory = uriIsFile ? Path.GetDirectoryName(fileUri.AbsolutePath) : System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyComputer);

				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				var fileExtension = Path.GetExtension(dialog.UnmappedFileName);
				if (!CampaignEmailTemplateEditor.IsSupportedExtension(fileExtension))
				{
					ShowInvalidImageExtensionError();
					return;
				}

				txtURL.Text = dialog.FileName;
				ImageURL = dialog.FileName;
				ImageMacro = string.Empty;
				ImageLinkForMacro = string.Empty;

				setImageDimensionaAndAspectRatio(dialog.FileName);
			}
		}

		void ShowInvalidImageExtensionError()
		{
			var errorMessageText = Res.GetString("a69690ee-2186-47e0-9afc-ce5e47d522fc", "Only image extensions {0}, {1}, {2}, {3} or {4} are supported", "jpg", "jpeg", "gif", "png", "bmp");
			var errorMessageCaption = Res.GetString("46367479-323e-45be-8205-b3dec2ead363", "Local File Invalid");
			Globals.Message.ShowError(errorMessageText, errorMessageCaption);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Third party source code string")]
		string MakeSupportedExtensionsFilterString()
		{
			var joinedExtensions = string.Join(";", CampaignEmailTemplateEditor.SupportedExtensions.Select(s => string.Format(CultureInfo.InvariantCulture, "*.{0}", s.ToLowerInvariant())));
			return string.Format(CultureInfo.InvariantCulture, "Supported images ({0})|{0}", joinedExtensions);
		}

		void UploadMacroImage(MacroImageMenu sender)
		{
			if (string.IsNullOrWhiteSpace(HtmlEditorTempFilePath))
			{
				throw new ArgumentException("HtmlEditorTempFilePath should not be empty.");
			}
			else
			{
				var newFileName = Path.Combine(HtmlEditorTempFilePath,
					Guid.NewGuid().ToString() + sender.PlaceholderImage.RawFormat.GetFormatExtension());

				sender.PlaceholderImage.Save(newFileName);
				ImageLinkForMacro = newFileName;
			}

			ImageMacro = sender.Macro;
			txtAlt.Text = sender.ImageText;
			txtURL.Text = sender.Macro;

			setImageDimensionaAndAspectRatio(ImageLinkForMacro);
		}

		void rdoLocalFile_CheckedChanged(object sender, EventArgs e)
		{
			txtURL.Text = string.Empty;
			btnBrowseFile.Enabled = rdoLocalFile.Checked;
		}

		void rdMacro_CheckedChanged(object sender, EventArgs e)
		{
			txtURL.Text = string.Empty;
			buttonUploadImage.Enabled = rdMacro.Checked;
		}

		void rdInternetURL_CheckedChanged(object sender, EventArgs e)
		{
			txtURL.Text = string.Empty;
			txtURL.Enabled = rdInternetURL.Checked;
		}

		void chkAlignment_CheckedChanged(object sender, EventArgs e)
		{
			cmbAlign.Enabled = chkAlignment.Checked;
		}

		void chkBorderThickness_CheckedChanged(object sender, EventArgs e)
		{
			txtBorder.Enabled = chkBorderThickness.Checked;
		}

		void chkHeight_CheckedChanged(object sender, EventArgs e)
		{
			txtHeight.Enabled = chkHeight.Checked;
		}

		void chkWidth_CheckedChanged(object sender, EventArgs e)
		{
			txtWidth.Enabled = chkWidth.Checked;
		}

		void chkBorderColor_CheckedChanged(object sender, EventArgs e)
		{
			lnkBgColor.Enabled = chkBorderColor.Checked;
			if (!chkBorderColor.Checked)
			{
				txtBgColor.BackColor = default(Color);
			}
		}

		void lnkBgColor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (ColorDialog myDialog = new ColorDialog { AllowFullOpen = true, AnyColor = true })
			{
				if (myDialog.ShowDialog() == DialogResult.OK)
				{
					txtBgColor.BackColor = myDialog.Color;
				}
			}
		}

		void chkBorderStyle_CheckedChanged(object sender, EventArgs e)
		{
			cmbBorderStyle.Enabled = chkBorderStyle.Checked;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is handled locally.")]
		void txtHeight_TextChanged(object sender, EventArgs e)
		{
			if (txtHeight.Focused && chkLockAspectRatio.Checked && this.widthToHeightAspectRatio.HasValue && this.widthToHeightAspectRatio.Value > 0)
			{
				try
				{
					string value = txtHeight.Text;
					string digitPart;
					string unitPart;
					getValueAndUnit(value, out digitPart, out unitPart);

					if (digitPart.Length > 0)
					{
						float height = float.Parse(digitPart, CultureInfo.CurrentCulture);
						if (height > 0)
						{
							decimal width = Convert.ToDecimal(this.widthToHeightAspectRatio.Value * height);
							txtWidth.Text = ((int)Utilities.Round(width, 0)).ToString(CultureInfo.InvariantCulture) + unitPart;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// ignored
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is handled locally.")]
		void txtWidth_TextChanged(object sender, EventArgs e)
		{
			if (txtWidth.Focused && chkLockAspectRatio.Checked && this.widthToHeightAspectRatio.HasValue && this.widthToHeightAspectRatio.Value > 0)
			{
				try
				{
					string value = txtWidth.Text;
					string digitPart;
					string unitPart;
					getValueAndUnit(value, out digitPart, out unitPart);

					if (digitPart.Length > 0)
					{
						float width = float.Parse(digitPart, CultureInfo.CurrentCulture);
						if (width > 0)
						{
							decimal height = Convert.ToDecimal(width / this.widthToHeightAspectRatio.Value);
							txtHeight.Text = ((int)Utilities.Round(height, 0)).ToString(CultureInfo.InvariantCulture) + unitPart;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// ignored
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "HTML values")]
		void txtBorder_Validating(object sender, CancelEventArgs e)
		{
			if (string.IsNullOrEmpty(txtBorder.Text))
			{
				e.Cancel = false;
				return;
			}

			var validValues = new string[] { "medium", "thin", "thick", "initial", "inherit" };
			foreach (var validValue in validValues)
			{
				if (txtBorder.Text.Equals(validValue, StringComparison.InvariantCultureIgnoreCase))
				{
					txtBorder.Text = validValue;
					e.Cancel = false;
					return;
				}
			}

			var errorMessageText = Res.GetString("1bc58b3a-e7e5-4681-92da-2db6e73e172a", "Border width must be either medium, thin, thick, initial, inherit or a numeric length followed by a unit of either {0}, {1}, {2}, {3}, {4} or {5}.", "cm", "mm", "in", "px", "pt", "pc");
			var errorMessageCaption = Res.GetString("d9ed6e71-f0dd-4e05-9a40-1a88d1c19b72", "Border Width Invalid");

			string value = txtBorder.Text;
			string digitPart;
			string unitPart;
			getValueAndUnit(value, out digitPart, out unitPart);

			int digit;
			if (string.IsNullOrEmpty(digitPart) || !int.TryParse(digitPart, out digit))
			{
				Globals.Message.ShowError(errorMessageText, errorMessageCaption);
				e.Cancel = true;
				return;
			}

			if (string.IsNullOrEmpty(unitPart))
			{
				unitPart = "px";
			}

			var validUnits = new string[] { "cm", "mm", "in", "px", "pt", "pc" };
			foreach (var validUnit in validUnits)
			{
				if (unitPart.Equals(validUnit, StringComparison.InvariantCultureIgnoreCase))
				{
					txtBorder.Text = $"{digit}{validUnit}";
					e.Cancel = false;
					return;
				}
			}

			Globals.Message.ShowError(errorMessageText, errorMessageCaption);
			e.Cancel = true;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is handled locally.")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "image file extension, embedded image string, Not a code smell")]
		ImageElementWithMacro ReadUi()
		{
			string src = null;

			if (rdoLocalFile.Checked && !string.IsNullOrEmpty(ImageURL))
			{
				try
				{
					if (File.Exists(ImageURL))
					{
						src = HtmlEditorUtils.GetBase64DataUrlForLocalImage(ImageURL);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// ignored
				}
			}

			if (!string.IsNullOrEmpty(ImageLinkForMacro))
			{
				try
				{
					if (File.Exists(ImageLinkForMacro))
					{
						src = HtmlEditorUtils.GetBase64DataUrlForLocalImage(ImageLinkForMacro);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// ignored
				}
			}

			ImageElementWithMacro theElement = new ImageElementWithMacro
			{
				SrcUrl = src ?? (string.IsNullOrEmpty(ImageMacro) ? ImageURL : ImageLinkForMacro),
				Macro = ImageMacro
			};

			if (chkWidth.Checked)
			{
				theElement.Width = txtWidth.Text.Trim();
			}
			if (chkHeight.Checked)
			{
				theElement.Height = txtHeight.Text.Trim();
			}

			theElement.BorderColor = txtBgColor.BackColor;

			if (chkBorderStyle.Checked)
			{
				theElement.BorderStyle = cmbBorderStyle.Text;
			}
			else
			{
				theElement.BorderStyle = "None";
			}

			if (chkBorderThickness.Checked && !string.IsNullOrEmpty(txtBorder.Text))
			{
				theElement.BorderWidth = txtBorder.Text;
			}
			else
			{
				theElement.BorderWidth = string.Empty;
			}
			theElement.Title = txtToolTip.Text.Trim();
			theElement.AlternativeText = txtAlt.Text.Trim();
			if (chkAlignment.Checked && !string.IsNullOrEmpty(cmbAlign.Text))
			{
				theElement.Align = cmbAlign.Text;
			}
			return theElement;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is handled locally.")]
		void UpdateUi(ImageElementWithMacro element)
		{
			if (this.IsLocalResourceSelectionDisabled)
			{
				rdoLocalFile.Enabled = false;
				btnBrowseFile.Enabled = false;
			}

			if (!string.IsNullOrEmpty(element.Macro))
			{
				txtURL.Text = element.Macro;
				ImageURL = string.Empty;
				ImageMacro = element.Macro;
				ImageLinkForMacro = element.SrcUrl;
			}
			else
			{
				txtURL.Text = element.SrcUrl;
				ImageURL = element.SrcUrl;
				ImageMacro = string.Empty;
				ImageLinkForMacro = string.Empty;
			}

			rdMacro.Checked = false;
			rdoLocalFile.Checked = false;
			rdInternetURL.Checked = false;

			if (!string.IsNullOrEmpty(element.Macro))
			{
				rdMacro.Checked = true;
			}
			else if (element?.SrcUrl?.StartsWith("data:image", StringComparison.OrdinalIgnoreCase) ?? false)
			{
				rdoLocalFile.Checked = true;
			}
			else
			{
				rdInternetURL.Checked = true;
			}

			txtToolTip.Text = element.Title;
			txtAlt.Text = element.AlternativeText;
			cmbAlign.Text = element.Align;
			chkAlignment.Checked = !string.IsNullOrEmpty(element.Align);
			txtBorder.Text = element.BorderWidth ?? string.Empty;
			chkBorderThickness.Checked = !string.IsNullOrEmpty(txtBorder.Text);
			txtWidth.Text = element.Width;
			chkWidth.Checked = !string.IsNullOrEmpty(element.Width);
			txtHeight.Text = element.Height;
			chkHeight.Checked = !string.IsNullOrEmpty(element.Height);
			chkBorderColor.Checked = element.BorderColor.HasValue;
			txtBgColor.BackColor = element.BorderColor ?? Color.White;
			chkBorderStyle.Checked = !string.IsNullOrEmpty(element.BorderStyle);
			cmbBorderStyle.Text = element.BorderStyle;

			if (chkHeight.Checked && chkWidth.Checked)
			{
				try
				{
					string widthDigitPart;
					string unit;
					getValueAndUnit(txtWidth.Text, out widthDigitPart, out unit);

					string heightDigitPart;
					getValueAndUnit(txtHeight.Text, out heightDigitPart, out unit);
					if (widthDigitPart.Length > 0 && heightDigitPart.Length > 0)
					{
						float width = float.Parse(widthDigitPart, CultureInfo.CurrentCulture);
						float height = float.Parse(heightDigitPart, CultureInfo.CurrentCulture);
						if (width > 0 && height > 0)
						{
							this.widthToHeightAspectRatio = width / height;
						}
						else
						{
							this.widthToHeightAspectRatio = null;
						}
					}
				}
				catch
				{
					this.widthToHeightAspectRatio = null;
				}
			}
			else
			{
				this.widthToHeightAspectRatio = null;
			}
		}

		void setImageDimensionaAndAspectRatio(string imageFileName)
		{
			Size? theImageDimension = HtmlEditorUtils.GetImageDimension(imageFileName);
			if (theImageDimension.HasValue)
			{
				chkHeight.Checked = true;
				txtHeight.Text = $"{theImageDimension.Value.Height}px";
				chkWidth.Checked = true;
				txtWidth.Text = $"{theImageDimension.Value.Width}px";
				if (theImageDimension.Value.Width > 0 && theImageDimension.Value.Height > 0)
				{
					this.widthToHeightAspectRatio = theImageDimension.Value.Width / (float)theImageDimension.Value.Height;
				}
				else
				{
					this.widthToHeightAspectRatio = null;
				}
			}
			else
			{
				this.widthToHeightAspectRatio = null;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "RegEx expression")]
		static void getValueAndUnit(string value, out string digitPart, out string unitPart)
		{
			const string digitRegEx = @"\d+";
			digitPart = Regex.Match(value, digitRegEx, RegexOptions.IgnoreCase).Groups[0].Value;
			unitPart = string.IsNullOrEmpty(digitPart) ? string.Empty : value.Replace(digitPart, "").Trim();
		}

		public bool IsLocalResourceSelectionDisabled { get; set; }

		void pnlUrl_MouseMove(object sender, MouseEventArgs e)
		{
			Control parent = sender as Control;
			if (parent == null)
			{
				return;
			}

			Control ctrl = parent.GetChildAtPoint(e.Location);
			if (ctrl != null)
			{
				if (ctrl.Visible && toolTip1.Tag == null && !toolTipShown)
				{
					string tipstring = toolTip1.GetToolTip(ctrl);
					toolTip1.Show(tipstring.Trim(), ctrl, ctrl.Width / 2, ctrl.Height / 2);
					toolTip1.Tag = ctrl;
					toolTipShown = true;
				}
			}
			else
			{
				ctrl = toolTip1.Tag as Control;
				if (ctrl != null)
				{
					toolTip1.Hide(ctrl);
					toolTip1.Tag = null;
					toolTipShown = false;
				}
			}
		}

		void btnOK_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(txtURL.Text))
			{
				this.DialogResult = DialogResult.None;
				Globals.Message.Show(Res.GetString("eaaf6555-378a-4770-8f6d-fea207263ff2", "Please provide Image URL"));
				txtURL.Focus();
			}
			if (rdInternetURL.Checked && !UrlValidation.IsValidAbsoluteHttpOrHttpsUrl(txtURL.Text))
			{
				this.DialogResult = DialogResult.None;
				ShowInvalidInternetURLError();
			}
		}

		void ShowInvalidInternetURLError()
		{
			var errorMessageText = Res.GetString("0273ebea-87a1-4e24-be24-7740ec1948e9", "Only accept HTTP(S) image for internet URL");
			var errorMessageCaption = Res.GetString("8e7b8d8a-0d53-4ae4-8242-013d68bc384b", "Internet URL Invalid");

			Globals.Message.ShowError(errorMessageText, errorMessageCaption);
			txtURL.Focus();
		}

		void ButtonUploadImage_Click(object sender, EventArgs e)
		{
			UploadImageMenuStrip.Show(buttonUploadImage, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, buttonUploadImage.Height, true));
		}

		void txtURL_Validating(object sender, CancelEventArgs e)
		{
			if (string.IsNullOrEmpty(txtURL.Text) || !rdInternetURL.Checked)
			{
				e.Cancel = false;
				return;
			}

			if (!UrlValidation.IsValidAbsoluteHttpOrHttpsUrl(txtURL.Text))
			{
				ShowInvalidInternetURLError();
				e.Cancel = false;
				return;
			}

			if (!CampaignEmailTemplateEditor.IsSupportedExtension(CampaignEmailTemplateEditor.GetExtension(txtURL.Text)))
			{
				var errorMessageText = Res.GetString("a69690ee-2186-47e0-9afc-ce5e47d522fc", "Only image extensions {0}, {1}, {2}, {3} or {4} are supported", "jpg", "jpeg", "gif", "png", "bmp");
				var errorMessageCaption = Res.GetString("8e7b8d8a-0d53-4ae4-8242-013d68bc384b", "Internet URL Invalid");

				Globals.Message.ShowError(errorMessageText, errorMessageCaption);
				e.Cancel = true;
				return;
			}

			e.Cancel = false;
		}

		void TxtURL_Validated(object sender, EventArgs e)
		{
			if (txtURL.Text.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
			{
				string[] parts = txtURL.Text.Split(new string[] { "base64," }, StringSplitOptions.None);
				if (parts.Length == 2 && !string.IsNullOrEmpty(parts[1]))
				{
					ImageURL = txtURL.Text;
					rdoLocalFile.Checked = true;
					rdInternetURL.Checked = false;
				}
			}
			if (rdInternetURL.Checked)
			{
				ImageURL = txtURL.Text;
			}
		}
	}
}

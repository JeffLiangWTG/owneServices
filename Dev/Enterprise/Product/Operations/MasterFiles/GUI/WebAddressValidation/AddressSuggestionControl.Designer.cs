using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	partial class AddressSuggestionControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AddressesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ManualVerifyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddressesListView = new ListView();
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AddressesPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// AddressesPanel
			// 
			this.AddressesPanel.AutoScroll = true;
			this.AddressesPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AddressesPanel.BackColor = System.Drawing.Color.Transparent;
			this.AddressesPanel.Controls.Add(this.InfoLabel);
			this.AddressesPanel.Controls.Add(this.ManualVerifyButton);
			this.AddressesPanel.Controls.Add(this.ValidateAddressButton);
			this.AddressesPanel.Controls.Add(this.AddressesListView);
			this.AddressesPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AddressesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.AddressesPanel.Name = "AddressesPanel";
			this.AddressesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 294, true);
			this.AddressesPanel.TabIndex = 4;
			// 
			// InfoLabel
			// 
			this.InfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.InfoLabel.BackColor = System.Drawing.Color.White;
			this.InfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.InfoLabel.Name = "InfoLabel";
			this.InfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 263, true);
			this.InfoLabel.TabIndex = 15;
			this.InfoLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.InfoLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			// 
			// ManualVerifyButton
			// 
			this.ManualVerifyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ManualVerifyButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressSuggestionControl|8e5dbd09-9c28-4816-90a3-92cbfd9b6d33", "Accept as Entered");
			this.ManualVerifyButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.Warning1616;
			this.ManualVerifyButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ManualVerifyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 295, true);
			this.ManualVerifyButton.Name = "ManualVerifyButton";
			this.ManualVerifyButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ManualVerifyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 24, true);
			this.ManualVerifyButton.TabIndex = 13;
			this.ManualVerifyButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.ManualVerifyButton.ToolTipCaption = null;
			this.ManualVerifyButton.UseVisualStyleBackColor = true;
			this.ManualVerifyButton.Click += new System.EventHandler(this.ManualVerifyButton_Click);
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ValidateAddressButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressSuggestionControl|B231A35F-7E08-4DC3-A450-A56061CFD4B5", "Use Selected");
			this.ValidateAddressButton.Enabled = false;
			this.ValidateAddressButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.TopPick1616;
			this.ValidateAddressButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 295, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 24, true);
			this.ValidateAddressButton.TabIndex = 14;
			this.ValidateAddressButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.ValidateAddressButton.ToolTipCaption = null;
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);
			// 
			// AddressesListView
			// 
			this.AddressesListView.View = View.Details;
			this.AddressesListView.HideSelection = false;
			this.AddressesListView.Columns.Add("", this.AddressesListView.ClientSize.Width);
			this.AddressesListView.HeaderStyle = ColumnHeaderStyle.None;
			this.AddressesListView.Dock = System.Windows.Forms.DockStyle.Top;
			this.AddressesListView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.AddressesListView.Name = "AddressesListView";
			this.AddressesListView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 265);
			this.AddressesListView.TabIndex = 0;
			ImageList addressImageList = new ImageList();
			addressImageList.ImageSize = new Size(30, 30);
			addressImageList.Images.Add("TopPick3232", Properties.Resources.TopPick3232);
			addressImageList.Images.Add("UnmatchedApartment3232", Properties.Resources.UnmatchedApartment3232);
			this.AddressesListView.SmallImageList = addressImageList;
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressSuggestionControl|42380D8A-C4E8-4DFE-887F-9FABA7C625C2", "Suggested Addresses");
			this.HeaderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.HeaderLabel.IsFontBold = true;
			this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 22, true);
			this.HeaderLabel.TabIndex = 5;
			// 
			// AddressSuggestionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.HeaderLabel);
			this.Controls.Add(this.AddressesPanel);
			this.Name = "AddressSuggestionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 320, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AddressesPanel.ResumeLayout(false);
			this.AddressesPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public ZArchitecture.GUI.ZPanel AddressesPanel;
		public ZArchitecture.ZLabel HeaderLabel;
		protected ZArchitecture.GUI.ZButton ValidateAddressButton;
		private ListView AddressesListView;
		protected ZArchitecture.GUI.ZButton ManualVerifyButton;
		protected ZArchitecture.ZLabel InfoLabel;
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class TariffFindBoxTreeViewForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.TariffTreeView = new Enterprise.Customs.Universal.GUI.TariffTreeView(FilterBusinessObject);
			this.contextMenuStrip = new CargoWise.Windows.UI.KContextMenuStrip(this.components);
			this.ExpandToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.CollapseToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.CollapseAllToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ViewToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.FindTextToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.FullDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.oKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ViewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ToolBarPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ToolbarRightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Toolstrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.StripControl = new Enterprise.Customs.Universal.GUI.TariffTreeViewFilterStripControl(FilterBusinessObject);
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FilterStripGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.contextMenuStrip.SuspendLayout();
			this.ToolBarPanel.SuspendLayout();
			this.ToolbarRightPanel.SuspendLayout();
			this.LanguageDropEdit.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 499, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 30, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(340);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(341);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.TariffSearchHelper);
			// 
			// ToolBarPanel
			// 
			this.ToolBarPanel.BackColor = System.Drawing.Color.Transparent;
			this.ToolBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToolBarPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ToolBarPanel.Controls.Add(this.ToolbarRightPanel);
			this.ToolBarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolBarPanel.Name = "ToolBarPanel";
			this.ToolBarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 28, true);
			this.ToolBarPanel.TabIndex = 1;
			// 
			// ToolbarRightPanel
			// 
			this.ToolbarRightPanel.Controls.Add(this.Toolstrip);
			this.ToolbarRightPanel.Controls.Add(this.LanguageDropEdit);
			this.ToolbarRightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ToolbarRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolbarRightPanel.Name = "ToolbarRightPanel";
			this.ToolbarRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 0, true);
			this.ToolbarRightPanel.TabIndex = 0;
			// 
			// LanguageDropEdit
			// 
			this.LanguageDropEdit.AllowDrop = true;
			this.LanguageDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LanguageDropEdit, "Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.TariffSearchHelper)(null)).Language)));
			this.LanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(992, 5, true);
			this.LanguageDropEdit.Name = "LanguageDropEdit";
			this.LanguageDropEdit.PreBoundMaxLength = 7;
			this.LanguageDropEdit.ShouldResizeByMaxLength = true;
			this.LanguageDropEdit.ShowDescriptionBox = false;
			this.LanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 28, true);
			this.LanguageDropEdit.TabIndex = 1;
			// 
			// Toolstrip
			// 
			this.Toolstrip.BackColor = System.Drawing.Color.Transparent;
			this.Toolstrip.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.Toolstrip.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Toolstrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.Toolstrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Toolstrip.Name = "Toolstrip";
			this.Toolstrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 28, true);
			this.Toolstrip.TabIndex = 0;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.FilterStripGroupBox);
			this.MainPanel.Controls.Add(this.TariffTreeView);
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 500, true);
			this.MainPanel.TabIndex = 0;
			// 
			// FilterStripGroupBox
			// 
			this.FilterStripGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FilterStripGroupBox.Controls.Add(this.StripControl);
			this.FilterStripGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterStripGroupBox.Name = "FilterStripGroupBox";
			this.FilterStripGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 500, true);
			this.FilterStripGroupBox.TabIndex = 3;
			this.FilterStripGroupBox.RefreshCaptionLabel();
			// 
			// StripControl
			//
			this.StripControl.CaptionRenderingEnabled = true;
			this.StripControl.AllowDrop = true;
			this.StripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.StripControl.Name = "StripControl";
			this.StripControl.AutoSize = true;
			this.StripControl.TabIndex = 1;
			this.StripControl.PerformSearch += new System.EventHandler<PerformSearchEventArgs>(this.StripControl_PerformSearch);
			this.StripControl.Layout += new System.Windows.Forms.LayoutEventHandler(this.StripControl_Layout);
			this.StripControl.BackColor = SystemDataRegistry.Instance.ColorTheme.FormBackgroundColor;
			// 
			// TariffTreeView
			// 
			this.TariffTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.TariffTreeView.BackColor = System.Drawing.SystemColors.Control;
			this.TariffTreeView.ContextMenuStrip = this.contextMenuStrip;
			this.TariffTreeView.HideSelection = false;
			this.TariffTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 220, true);
			this.TariffTreeView.Name = "TariffTreeView";
			this.TariffTreeView.ReadOnly = true;
			this.TariffTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 191, true);
			this.TariffTreeView.TabIndex = 2;
			this.TariffTreeView.DoubleClick += new System.EventHandler(this.TariffTreeView_DoubleClick);
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.ExpandToolStripMenuItem,
			this.CollapseToolStripMenuItem,
			this.CollapseAllToolStripMenuItem,
			this.FindTextToolStripMenuItem,
			this.ViewToolStripMenuItem });
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 92, true);
			// 
			// ExpandToolStripMenuItem
			// 
			this.ExpandToolStripMenuItem.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("88cc3476-9399-4bbd-b23c-c168fe59d9c6", "&Expand");
			this.ExpandToolStripMenuItem.Name = "ExpandToolStripMenuItem";
			this.ExpandToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 22, true);
			this.ExpandToolStripMenuItem.Click += new System.EventHandler(this.ExpandToolStripMenuItem_Click);
			// 
			// CollapseToolStripMenuItem
			// 
			this.CollapseToolStripMenuItem.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("5f55aa6e-3afc-4b8a-a284-1cf3cdbce616", "&Collapse");
			this.CollapseToolStripMenuItem.Name = "CollapseToolStripMenuItem";
			this.CollapseToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 22, true);
			this.CollapseToolStripMenuItem.Click += new System.EventHandler(this.CollapseToolStripMenuItem_Click);
			// 
			// CollapseAllToolStripMenuItem
			// 
			this.CollapseAllToolStripMenuItem.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("99bf9286-43a7-4486-872e-7ad902bc3e4b", "C&ollapse All");
			this.CollapseAllToolStripMenuItem.Name = "CollapseAllToolStripMenuItem";
			this.CollapseAllToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 22, true);
			this.CollapseAllToolStripMenuItem.Click += new System.EventHandler(this.CollapseAllToolStripMenuItem_Click);
			// 
			// ViewToolStripMenuItem
			// 
			this.ViewToolStripMenuItem.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("82be63b0-b342-49f8-aa4b-e89d7c407480", "&View");
			this.ViewToolStripMenuItem.Name = "ViewToolStripMenuItem";
			this.ViewToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 22, true);
			this.ViewToolStripMenuItem.Click += new System.EventHandler(this.ViewToolStripMenuItem_Click);
			// 
			// FindTextToolStripMenuItem
			// 
			this.FindTextToolStripMenuItem.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("5200c074-6723-4d0f-80e8-74834d7aaf88", "Show &Find Form");
			this.FindTextToolStripMenuItem.Name = "FindTextToolStripMenuItem";
			this.FindTextToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
			this.FindTextToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 22, true);
			this.FindTextToolStripMenuItem.Click += new System.EventHandler(this.FindTextToolStripMenuItem_Click);
			// 
			// FullDescriptionTextBox
			// 
			this.FullDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FullDescriptionTextBox, "TariffDataObjects.FullDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.TariffDataObject)(((System.Collections.IList)(((Enterprise.Customs.Universal.TariffSearchHelper)(null)).TariffDataObjects)).SyncRoot)).FullDescription)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FullDescriptionTextBox, false);
			this.FullDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 417, true);
			this.FullDescriptionTextBox.Multiline = true;
			this.FullDescriptionTextBox.Name = "FullDescriptionTextBox";
			this.FullDescriptionTextBox.ReadOnly = true;
			this.FullDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 71, true);
			this.FullDescriptionTextBox.TabIndex = 3;
			// 
			// oKButton
			// 
			this.oKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oKButton.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("7ce7e21d-945d-426f-826c-1656de95f340", "&OK");
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 468, true);
			this.oKButton.Name = "oKButton";
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.oKButton.TabIndex = 5;
			this.oKButton.Click += new System.EventHandler(this.oKButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("ac94c04b-e7d6-4167-b2d8-0c7b09295b5e", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(606, 468, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// ViewButton
			// 
			this.ViewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewButton.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("565f887f-e8b7-4903-9d10-27dc162af67a", "&View");
			this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 467, true);
			this.ViewButton.Name = "ViewButton";
			this.ViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.ViewButton.TabIndex = 4;
			this.ViewButton.Click += new System.EventHandler(this.ViewButton_Click);
			// 
			// TariffFindBoxTreeViewForm
			// 
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("8b8be32f-c0de-4bb0-a199-eed89f2ef097", "Select a Tariff Code");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 529, true);
			this.Controls.Add(this.ViewButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.oKButton);
			this.Controls.Add(this.FullDescriptionTextBox);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.ToolBarPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.Universal";
			this.DataSourceType = typeof(Enterprise.Customs.Universal.TariffSearchHelper);
			this.DataSourceTypeName = "Enterprise.Customs.Universal.TariffSearchHelper";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 568, true);
			this.Name = "TariffFindBoxTreeViewForm";
			this.Text = "Select a Tariff Code";
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.ToolBarPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FullDescriptionTextBox, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.ViewButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.contextMenuStrip.ResumeLayout(false);
			this.contextMenuStrip.PerformLayout();
			this.ToolBarPanel.ResumeLayout(false);
			this.ToolBarPanel.PerformLayout();
			this.ToolbarRightPanel.ResumeLayout(false);
			this.ToolbarRightPanel.PerformLayout();
			this.LanguageDropEdit.ResumeLayout(true);
			this.LanguageDropEdit.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ToolStripItem HideShowToolStripItem;
		ZPanel MainPanel;
		ZGroupBox FilterStripGroupBox;
		ZPanel ToolBarPanel;
		ZPanel ToolbarRightPanel;
		ZToolStrip Toolstrip;
		TariffTreeView TariffTreeView;
		ZTextBox FullDescriptionTextBox;
		ZButton oKButton;
		ZButton ViewButton;
		TariffTreeViewFilterStripControl StripControl;
		KContextMenuStrip contextMenuStrip;
		System.ComponentModel.IContainer components;
		ZToolStripMenuItem ExpandToolStripMenuItem;
		ZToolStripMenuItem CollapseAllToolStripMenuItem;
		ZToolStripMenuItem CollapseToolStripMenuItem;
		ZToolStripMenuItem ViewToolStripMenuItem;
		ZToolStripMenuItem FindTextToolStripMenuItem;
		ZButton cancelButton;
		ZDropEdit LanguageDropEdit;
		RefCusTariffFilterStripBusinessObject FilterBusinessObject;

		#endregion
	}
}

namespace Enterprise.Packing.GUI
{
	public partial class PackOrUnpackItemsDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1014:EmbeddedIconRule", Justification = "Embedding a pack icon, not product a icon")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackOrUnpackItemsDialog));
			this.CommonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CommonCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CommonOkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PackageQtyTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Grid = new Enterprise.Packing.GUI.ItemsGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommonPanel.SuspendLayout();
			this.GridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 237, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Packing.Business.PackItemsBusinessObject);
			// 
			// CommonPanel
			// 
			this.CommonPanel.Controls.Add(this.CommonCancelButton);
			this.CommonPanel.Controls.Add(this.CommonOkButton);
			this.CommonPanel.Controls.Add(this.PackageQtyTextBox);
			this.CommonPanel.Controls.Add(this.PackageDropEdit);
			this.CommonPanel.Controls.Add(this.GridPanel);
			this.CommonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommonPanel.Name = "CommonPanel";
			this.CommonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 237, true);
			this.CommonPanel.TabIndex = 0;
			// 
			// CommonCancelButton
			// 
			this.CommonCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CommonCancelButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("CommonDialog|e862c01e-14cb-4793-ab81-3797cac1e554", "Cancel");
			this.CommonCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CommonCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(597, 201, true);
			this.CommonCancelButton.Name = "CommonCancelButton";
			this.CommonCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CommonCancelButton.TabIndex = 3;
			this.CommonCancelButton.UseVisualStyleBackColor = true;
			// 
			// CommonOkButton
			// 
			this.CommonOkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CommonOkButton.CaptionResourceString = null;
			this.CommonOkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 202, true);
			this.CommonOkButton.Name = "CommonOkButton";
			this.CommonOkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CommonOkButton.TabIndex = 2;
			this.CommonOkButton.UseVisualStyleBackColor = true;
			this.CommonOkButton.Click += new System.EventHandler(this.CommonOkButton_Click);
			// 
			// PackageQtyTextBox
			// 
			this.BindingSource.SetBindingMember(this.PackageQtyTextBox, "PackageQtyToCreate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Packing.Business.PackItemsBusinessObject)(null)).PackageQtyToCreate)));
			this.PackageQtyTextBox.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("ItemsDialog|b7d43fed-54e0-4cbb-af6e-9047d99fee3f", "Pkg.", "Package", "");
			this.PackageQtyTextBox.DecimalPlaces = 0;
			this.PackageQtyTextBox.Decimals = 0;
			this.PackageQtyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 12, true);
			this.PackageQtyTextBox.Name = "PackageQtyTextBox";
			this.PackageQtyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 20, true);
			this.PackageQtyTextBox.TabIndex = 0;
			this.PackageQtyTextBox.Text = "1";
			this.PackageQtyTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PackageDropEdit
			// 
			this.PackageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageDropEdit, "PackageTypeToCreate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Packing.Business.PackItemsBusinessObject)(null)).PackageTypeToCreate)));
			this.PackageDropEdit.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("ItemsDialog|92e639a3-f919-4aa2-9929-5c08d30107ca", "Package");
			this.PackageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 12, true);
			this.PackageDropEdit.MaxItemsToShowInDropDown = 20;
			this.PackageDropEdit.Name = "PackageDropEdit";
			this.PackageDropEdit.PreBoundMaxLength = 3;
			this.PackageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 20, true);
			this.PackageDropEdit.TabIndex = 1;
			// 
			// GridPanel
			// 
			this.GridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.GridPanel.Controls.Add(this.Grid);
			this.GridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 44, true);
			this.GridPanel.Name = "GridPanel";
			this.GridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 151, true);
			this.GridPanel.TabIndex = 102;
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, "PackableItemParentsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Packing.Business.PackItemsBusinessObject)(null)).PackableItemParentsForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PackableItemParentWrapper)(((System.Collections.IList)(((Enterprise.Packing.Business.PackItemsBusinessObject)(null)).PackableItemParentsForBinding)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PackableItemParentWrapper)(((System.Collections.IList)(((Enterprise.Packing.Business.PackItemsBusinessObject)(null)).PackableItemParentsForBinding)).SyncRoot)).PackableItemParent.TotalQtyUQ)));
			this.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("ItemsDialog|98beb6e0-9c39-4c32-aaec-086e9f19563f", "Desc.", "Description", "");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.GroupName = null;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("ItemsDialog|4683bb11-0ca0-4de9-b1c1-1c087f659f99", "UQ");
			zTextBoxColumnStyleInfo2.ColumnName = "PackableItemParent+TotalQtyUQ";
			zTextBoxColumnStyleInfo2.GroupName = null;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.CopySelectedRowsAllowed = true;
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.GridId = "394189cb-28d9-4337-8ff5-d41d5b6cf448";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 151, true);
			this.Grid.TabIndex = 0;
			// 
			// ItemsDialog
			// 
			this.AcceptButton = this.CommonOkButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CommonCancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("f24aa8df-32f2-4827-b51d-86ab22fa501b", "Items...");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 237, true);
			this.Controls.Add(this.CommonPanel);
			this.DataSourceType = typeof(Enterprise.Packing.Business.PackItemsBusinessObject);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 200, true);
			this.Name = "ItemsDialog";
			this.ShouldSerializeTabPageMethods = false;
			this.ShowInTaskbar = false;
			this.Text = "PackItemsForm";
			this.Controls.SetChildIndex(this.CommonPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommonPanel.ResumeLayout(false);
			this.CommonPanel.PerformLayout();
			this.GridPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		ZArchitecture.GUI.ZDropEdit PackageDropEdit;
		ZArchitecture.ZCalcEdit PackageQtyTextBox;
		ZArchitecture.GUI.ZPanel GridPanel;
		ItemsGrid Grid;
		protected ZArchitecture.GUI.ZPanel CommonPanel;
		protected ZArchitecture.GUI.ZButton CommonOkButton;
		protected ZArchitecture.GUI.ZButton CommonCancelButton;
	}
}

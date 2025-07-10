namespace Enterprise.Customs.US.InBond.GUI
{
	partial class MovementHeaderResetToOriginalForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EntriesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 165, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(743, 137, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Text = "&OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(823, 137, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.Text = "&Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// EntriesGrid
			// 
			this.EntriesGrid.AllowCopyToNewRowMenuItem = false;
			this.EntriesGrid.AllowDragDropWithChanges = false;
			this.EntriesGrid.AllowNavigation = false;
			this.EntriesGrid.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.EntriesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.EntriesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "InBond Number";
			zTextBoxColumnStyleInfo1.ColumnName = "RO_MovementUniqueCode";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "Bill Number";
			zTextBoxColumnStyleInfo5.ColumnName = "RO_BillNumber";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.Caption = "Container Number";
			zTextBoxColumnStyleInfo6.ColumnName = "RO_ContainerNumber";
			zTextBoxColumnStyleInfo6.IsMandatory = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.Caption = "Level";
			zTextBoxColumnStyleInfo7.ColumnName = "RO_Level";
			zTextBoxColumnStyleInfo7.IsMandatory = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.Caption = "Reset?";
			zCheckBoxColumnStyleInfo1.ColumnName = "RO_ResetToOriginal";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.Caption = "Description";
			zTextBoxColumnStyleInfo2.ColumnName = "RO_CustomsStatusDescription";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.Caption = "Status";
			zTextBoxColumnStyleInfo3.ColumnName = "RO_CustomsStatus";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.Caption = "Reason";
			zTextBoxColumnStyleInfo4.ColumnName = "RO_ResetReason";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntriesGrid.CopySelectedRowsAllowed = false;
			this.EntriesGrid.GridId = "0D6D1408-E683-414C-B11F-B21D4338A139";
			this.EntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntriesGrid.LayoutKey = "EntriesGrid";
			this.EntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntriesGrid.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 131, true);
			this.EntriesGrid.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 131, true);
			this.EntriesGrid.Name = "EntriesGrid";
			this.EntriesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.EntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 131, true);
			this.EntriesGrid.TabIndex = 0;
			// 
			// MovementHeaderResetToOriginalForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 189, true);
			this.Controls.Add(this.EntriesGrid);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 228, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 228, true);
			this.Name = "MovementHeaderResetToOriginalForm";
			this.Text = "Reset Movement Headers";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.EntriesGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private ZArchitecture.GUI.ZButton OKButton;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.ZGrid EntriesGrid;
	}
}

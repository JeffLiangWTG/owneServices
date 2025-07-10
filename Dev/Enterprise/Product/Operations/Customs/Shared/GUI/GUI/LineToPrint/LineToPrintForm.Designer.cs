namespace Enterprise.Customs.GUI
{
	partial class LineToPrintForm
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
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.LineToPrintGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LineToPrintPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LineToPrintCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LineToPrintGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 165, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// LineToPrintGrid
			// 
			this.LineToPrintGrid.AllowBeginDrag = false;
			this.LineToPrintGrid.AllowCopyToNewRowMenuItem = false;
			this.LineToPrintGrid.AllowDragDropWithChanges = false;
			this.LineToPrintGrid.AllowNavigation = false;
			this.LineToPrintGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LineToPrintGrid, "LinesToPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).LinesToPrint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.AutoLineToPrint)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).LinesToPrint)).SyncRoot)).Organisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.AutoLineToPrint)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).LinesToPrint)).SyncRoot)).Identifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.AutoLineToPrint)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).LinesToPrint)).SyncRoot)).LastPrintDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.AutoLineToPrint)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).LinesToPrint)).SyncRoot)).ShouldBePrinted)));
			this.LineToPrintGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("85aeeab2-8c27-4d02-9549-09d423e30ec4", "Organization");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "Organisation";
			zTextBoxColumnStyleInfo1.GroupName = null;
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ab962905-6957-4147-a14e-b39f9365caa7", "Identifier");
			zTextBoxColumnStyleInfo2.ColumnName = "Identifier";
			zTextBoxColumnStyleInfo2.GroupName = null;
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("739c16a0-cfac-4004-a281-8731383978f1", "Last Print Date");
			zDateEditColumnStyleInfo1.ColumnName = "LastPrintDate";
			zDateEditColumnStyleInfo1.GroupName = null;
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("880f5564-38b1-4ec6-bc89-8098ed5a7402", "Print");
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldBePrinted";
			zCheckBoxColumnStyleInfo1.GroupName = null;
			this.LineToPrintGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LineToPrintGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LineToPrintGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LineToPrintGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LineToPrintGrid.CopySelectedRowsAllowed = true;
			this.LineToPrintGrid.GridId = "95db1b63-a5a0-4dff-8490-c34a9388b6b2";
			this.LineToPrintGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LineToPrintGrid.LayoutKey = "LineToPrintGrid";
			this.LineToPrintGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.LineToPrintGrid.Name = "LineToPrintGrid";
			this.LineToPrintGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.LineToPrintGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 126, true);
			this.LineToPrintGrid.TabIndex = 0;
			// 
			// LineToPrintPrintButton
			// 
			this.LineToPrintPrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.LineToPrintPrintButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6579aac0-a8d3-4957-83f9-836aec7ed485", "Print");
			this.LineToPrintPrintButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.LineToPrintPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 134, true);
			this.LineToPrintPrintButton.Name = "LineToPrintPrintButton";
			this.LineToPrintPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.LineToPrintPrintButton.TabIndex = 1;
			this.LineToPrintPrintButton.UseVisualStyleBackColor = true;
			// 
			// LineToPrintCancelButton
			// 
			this.LineToPrintCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.LineToPrintCancelButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("32e27426-b6b5-486e-9ca0-82e481ce5489", "Cancel");
			this.LineToPrintCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.LineToPrintCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 134, true);
			this.LineToPrintCancelButton.Name = "LineToPrintCancelButton";
			this.LineToPrintCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.LineToPrintCancelButton.TabIndex = 2;
			this.LineToPrintCancelButton.UseVisualStyleBackColor = true;
			// 
			// LineToPrintForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 189, true);
			this.Controls.Add(this.LineToPrintGrid);
			this.Controls.Add(this.LineToPrintCancelButton);
			this.Controls.Add(this.LineToPrintPrintButton);
			this.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			this.Name = "LineToPrintForm";
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("55E93CF8-2B8C-4EFC-A8E3-9A0C3339775F", "Line To Print Form");
			this.Controls.SetChildIndex(this.LineToPrintPrintButton, 0);
			this.Controls.SetChildIndex(this.LineToPrintCancelButton, 0);
			this.Controls.SetChildIndex(this.LineToPrintGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LineToPrintGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid LineToPrintGrid;
		private ZArchitecture.GUI.ZButton LineToPrintPrintButton;
		private ZArchitecture.GUI.ZButton LineToPrintCancelButton;
	}
}

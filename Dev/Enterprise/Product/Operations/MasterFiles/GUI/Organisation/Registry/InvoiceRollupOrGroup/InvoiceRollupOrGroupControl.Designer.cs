namespace Enterprise.MasterFiles.GUI
{
	partial class InvoiceRollupOrGroupControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.GroupChargesBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PostingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DisplayDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JobTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GroupChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupChargesBox.SuspendLayout();
			this.PostingDropEdit.SuspendLayout();
			this.InvoiceDropEdit.SuspendLayout();
			this.StyleDropEdit.SuspendLayout();
			this.DisplayDropEdit.SuspendLayout();
			this.ModeDropEdit.SuspendLayout();
			this.DirectionDropEdit.SuspendLayout();
			this.JobTypeDropEdit.SuspendLayout();
			this.InvoiceCurrencyCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GroupChargesGrid)).BeginInit();
			this.GroupChargesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.InvoiceRollupOrGroupCollection);
			// 
			// GroupChargesBox
			// 
			this.GroupChargesBox.AutoSize = true;
			this.GroupChargesBox.CaptionResourceString = null;
			this.GroupChargesBox.Controls.Add(this.PostingDropEdit);
			this.GroupChargesBox.Controls.Add(this.InvoiceDropEdit);
			this.GroupChargesBox.Controls.Add(this.StyleDropEdit);
			this.GroupChargesBox.Controls.Add(this.DisplayDropEdit);
			this.GroupChargesBox.Controls.Add(this.ModeDropEdit);
			this.GroupChargesBox.Controls.Add(this.DirectionDropEdit);
			this.GroupChargesBox.Controls.Add(this.JobTypeDropEdit);
			this.GroupChargesBox.Controls.Add(this.InvoiceCurrencyCodeFindBox);
			this.GroupChargesBox.Controls.Add(this.GroupChargesGrid);
			this.GroupChargesBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GroupChargesBox, false);
			this.GroupChargesBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupChargesBox.Name = "GroupChargesBox";
			this.GroupChargesBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 274, true);
			this.GroupChargesBox.TabIndex = 5;
			this.GroupChargesBox.TabStop = false;
			// 
			// PostingDropEdit
			// 
			this.PostingDropEdit.AllowDrop = true;
			this.PostingDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PostingDropEdit, "InvoicePostingStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).InvoicePostingStyle)));
			this.PostingDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|fe53257d-951f-4272-bcf8-561162ed909b", "Posting");
			this.PostingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 217, true);
			this.PostingDropEdit.Name = "PostingDropEdit";
			this.PostingDropEdit.PreBoundMaxLength = 3;
			this.PostingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 17, true);
			this.PostingDropEdit.TabIndex = 11;
			// 
			// InvoiceDropEdit
			// 
			this.InvoiceDropEdit.AllowDrop = true;
			this.InvoiceDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.InvoiceDropEdit, "InvoiceLineDisplayOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).InvoiceLineDisplayOption)));
			this.InvoiceDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|dfb361de-64d8-4f5f-9a65-faecd5427ddb", "Invoice");
			this.InvoiceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 191, true);
			this.InvoiceDropEdit.Name = "InvoiceDropEdit";
			this.InvoiceDropEdit.PreBoundMaxLength = 3;
			this.InvoiceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 17, true);
			this.InvoiceDropEdit.TabIndex = 9;
			// 
			// StyleDropEdit
			// 
			this.StyleDropEdit.AllowDrop = true;
			this.StyleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.StyleDropEdit, "GroupOrSubtotalStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).GroupOrSubtotalStyle)));
			this.StyleDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|a2b40f51-2bad-424b-bf74-6c01c4355cf5", "Style", "Roll Up or Sub Total Style", "");
			this.StyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 167, true);
			this.StyleDropEdit.Name = "StyleDropEdit";
			this.StyleDropEdit.PreBoundMaxLength = 3;
			this.StyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 17, true);
			this.StyleDropEdit.TabIndex = 5;
			// 
			// DisplayDropEdit
			// 
			this.DisplayDropEdit.AllowDrop = true;
			this.DisplayDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DisplayDropEdit, "GroupOrSubTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).GroupOrSubTotal)));
			this.DisplayDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|9fdf54ec-ec33-4f42-b2fe-933bfec69a23", "Display", "Roll Up or Sub Total Display", "");
			this.DisplayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 141, true);
			this.DisplayDropEdit.Name = "DisplayDropEdit";
			this.DisplayDropEdit.PreBoundMaxLength = 3;
			this.DisplayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.DisplayDropEdit.TabIndex = 4;
			// 
			// ModeDropEdit
			// 
			this.ModeDropEdit.AllowDrop = true;
			this.ModeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ModeDropEdit, "TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).TransportMode)));
			this.ModeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|c81dfadc-1032-4e78-9798-f0e503c33aa7", "Mode", "Transport Mode", "");
			this.ModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 115, true);
			this.ModeDropEdit.Name = "ModeDropEdit";
			this.ModeDropEdit.PreBoundMaxLength = 3;
			this.ModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.ModeDropEdit.TabIndex = 3;
			// 
			// DirectionDropEdit
			// 
			this.DirectionDropEdit.AllowDrop = true;
			this.DirectionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "ServiceDirection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).ServiceDirection)));
			this.DirectionDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|19bcd96e-aca6-46f8-81cc-712b89c4df88", "Direction");
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 141, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.PreBoundMaxLength = 3;
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.DirectionDropEdit.TabIndex = 2;
			// 
			// JobTypeDropEdit
			// 
			this.JobTypeDropEdit.AllowDrop = true;
			this.JobTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JobTypeDropEdit, "JobType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).JobType)));
			this.JobTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|40c546a9-5497-4107-bdd4-1b2f73521809", "Job", "Job Invoice Type.");
			this.JobTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 115, true);
			this.JobTypeDropEdit.Name = "JobTypeDropEdit";
			this.JobTypeDropEdit.PreBoundMaxLength = 3;
			this.JobTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.JobTypeDropEdit.TabIndex = 1;
			// 
			// InvoiceCurrencyCodeFindBox
			// 
			this.InvoiceCurrencyCodeFindBox.AllowDrop = true;
			this.InvoiceCurrencyCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.InvoiceCurrencyCodeFindBox, "InvoicePostingCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).InvoicePostingCurrency)));
			this.InvoiceCurrencyCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e49dff44-7410-4971-a8c5-300b26b44d03", "Currency");
			this.InvoiceCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 244, true);
			this.InvoiceCurrencyCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.InvoiceCurrencyCodeFindBox.Name = "InvoiceCurrencyCodeFindBox";
			this.InvoiceCurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.InvoiceCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.InvoiceCurrencyCodeFindBox.TabIndex = 12;
			// 
			// GroupChargesGrid
			// 
			this.GroupChargesGrid.AllowNavigation = false;
			this.GroupChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GroupChargesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).ServiceDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).GroupOrSubTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).GroupOrSubtotalStyle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).InvoiceLineDisplayOption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).InvoicePostingStyle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.InvoiceRollupOrGroup)(null)).InvoicePostingCurrency)));
			this.GroupChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|418f3e8c-4323-44c2-91ce-4cd1b868ed3e", "Job", "Job Invoice Type.");
			zDropEditColumnStyleInfo1.ColumnName = "JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|4443db49-761a-4dab-9f02-b869e39c7ecf", "Direction");
			zDropEditColumnStyleInfo2.ColumnName = "ServiceDirection";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|bc63c6f1-b484-474d-a71a-acadf07d88c5", "Mode", "Transport Mode", "");
			zDropEditColumnStyleInfo3.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|359c2e27-d836-44bd-b5cf-63f40caff949", "Display", "Roll Up or Sub Total Display", "");
			zDropEditColumnStyleInfo4.ColumnName = "GroupOrSubTotal";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|6558d66a-7fbc-4d48-a98b-520659b0fc09", "Style", "Roll Up or Sub Total Style", "");
			zDropEditColumnStyleInfo5.ColumnName = "GroupOrSubtotalStyle";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|076a7a1c-8771-48ee-9ba6-4cbfd27cf304", "Invoice");
			zDropEditColumnStyleInfo6.ColumnName = "InvoiceLineDisplayOption";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("InvoiceRollupOrGroupControl|bcb48c1e-5bf6-423f-8b36-78c4512a4029", "Posting");
			zDropEditColumnStyleInfo7.ColumnName = "InvoicePostingStyle";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dc378bd1-911b-436e-ae20-cfee1b4a29ff", "Currency", "Invoice Currency", "");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "InvoicePostingCurrency";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.GroupChargesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.GroupChargesGrid.GridId = "4106810c-f79e-4914-b45c-f5d39b260ea6";
			this.GroupChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GroupChargesGrid.LayoutKey = "GroupChargesGrid";
			this.GroupChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.GroupChargesGrid.Name = "GroupChargesGrid";
			this.GroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 91, true);
			this.GroupChargesGrid.TabIndex = 0;
			// 
			// InvoiceRollupOrGroupControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupChargesBox);
			this.Name = "InvoiceRollupOrGroupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupChargesBox.ResumeLayout(false);
			this.GroupChargesBox.PerformLayout();
			this.PostingDropEdit.ResumeLayout(true);
			this.PostingDropEdit.PerformLayout();
			this.InvoiceDropEdit.ResumeLayout(true);
			this.InvoiceDropEdit.PerformLayout();
			this.StyleDropEdit.ResumeLayout(true);
			this.StyleDropEdit.PerformLayout();
			this.DisplayDropEdit.ResumeLayout(true);
			this.DisplayDropEdit.PerformLayout();
			this.ModeDropEdit.ResumeLayout(true);
			this.ModeDropEdit.PerformLayout();
			this.DirectionDropEdit.ResumeLayout(true);
			this.DirectionDropEdit.PerformLayout();
			this.JobTypeDropEdit.ResumeLayout(true);
			this.JobTypeDropEdit.PerformLayout();
			this.InvoiceCurrencyCodeFindBox.ResumeLayout(true);
			this.InvoiceCurrencyCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GroupChargesGrid)).EndInit();
			this.GroupChargesGrid.ResumeLayout(false);
			this.GroupChargesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZGroupBox GroupChargesBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit PostingDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit InvoiceDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit StyleDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit DisplayDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ModeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit DirectionDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit JobTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox InvoiceCurrencyCodeFindBox;
		internal Enterprise.ZArchitecture.ZGrid GroupChargesGrid;

	}
}

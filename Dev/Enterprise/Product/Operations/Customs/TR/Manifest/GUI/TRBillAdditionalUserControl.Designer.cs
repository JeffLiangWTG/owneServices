namespace Enterprise.Customs.TR.Manifest.GUI
{
	partial class TRBillAdditionalUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.groupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeclarationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationGrid)).BeginInit();
			this.DeclarationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Manifest.Business.AsycudaBill);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.DeclarationGrid);
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 373, true);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.CaptionResourceString = Enterprise.Customs.TR.Manifest.GUI.Res.GetData("ee2f1997-14ed-4782-bd12-5ae5f9b17904", "Related Declaration");
			// 
			// DeclarationGrid
			// 
			this.DeclarationGrid.AllowNavigation = false;
			this.DeclarationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DeclarationGrid, "RelatedDeclarationForExports");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).RelatedDeclarationForExports)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.RelatedDeclarationForExport)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).RelatedDeclarationForExports)).SyncRoot)).CSI_Procedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.RelatedDeclarationForExport)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).RelatedDeclarationForExports)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Manifest.Business.RelatedDeclarationForExport)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).RelatedDeclarationForExports)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.RelatedDeclarationForExport)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).RelatedDeclarationForExports)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.RelatedDeclarationForExport)(((System.Collections.IList)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).RelatedDeclarationForExports)).SyncRoot)).CSI_Quantity2)));
			this.DeclarationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Procedure";
			zDropEditColumnStyleInfo1.MaxDropDownItems = 12;
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo2.MaxDropDownItems = 3;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity2";
			zCalcEditColumnStyleInfo2.Decimals = 3;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DeclarationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DeclarationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DeclarationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DeclarationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DeclarationGrid.GridId = "dc8cdc3e-5c49-41e0-bd14-be354f34f741";
			this.DeclarationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeclarationGrid.LayoutKey = "DeclarationGrid";
			this.DeclarationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DeclarationGrid.Name = "DeclarationGrid";
			this.DeclarationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(937, 354, true);
			this.DeclarationGrid.TabIndex = 0;
			// 
			// TRBillAdditionalUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.groupBox1);
			this.Name = "TRBillAdditionalUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationGrid)).EndInit();
			this.DeclarationGrid.ResumeLayout(false);
			this.DeclarationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox groupBox1;
		private ZArchitecture.ZGrid DeclarationGrid;
	}
}

namespace Enterprise.Customs.NL.NCTS.GUI
{
	partial class UnloadingDetailsUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UnloadingRemarksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UnloadingRemarksFreeTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UnloadingRemarksGrid)).BeginInit();
			this.UnloadingRemarksGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.NCTS.Business.NctsHeader);
			// 
			// UnloadingRemarksGrid
			// 
			this.UnloadingRemarksGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UnloadingRemarksGrid, "ArrivalMovementHeader.UnloadingRemarkCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.UnloadingRemarkCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NL.NCTS.Business.NonPersistentNctsUnloadingRemark)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.UnloadingRemarkCollection)).SyncRoot)).ItemNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.NCTS.Business.NonPersistentNctsUnloadingRemark)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.UnloadingRemarkCollection)).SyncRoot)).EoriNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.NCTS.Business.NonPersistentNctsUnloadingRemark)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.UnloadingRemarkCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.NCTS.Business.NonPersistentNctsUnloadingRemark)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.UnloadingRemarkCollection)).SyncRoot)).Number)));
			this.UnloadingRemarksGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ItemNumber";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "EoriNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Number";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.UnloadingRemarksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.UnloadingRemarksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UnloadingRemarksGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.UnloadingRemarksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.UnloadingRemarksGrid.GridId = "8405922b-9a12-415e-bdf5-54d6e3769afd";
			this.UnloadingRemarksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnloadingRemarksGrid.LayoutKey = "UnloadingRemarksGrid";
			this.UnloadingRemarksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 136, true);
			this.UnloadingRemarksGrid.Name = "UnloadingRemarksGrid";
			this.UnloadingRemarksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 79, true);
			this.UnloadingRemarksGrid.TabIndex = 6;
			// 
			// UnloadingRemarksFreeTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.UnloadingRemarksFreeTextTextBox, "ArrivalMovementHeader.UnloadingRemarksFreeText");
			this.UnloadingRemarksFreeTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.UnloadingRemarksFreeText)));
			this.UnloadingRemarksFreeTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 20, true);
			this.UnloadingRemarksFreeTextTextBox.Multiline = true;
			this.UnloadingRemarksFreeTextTextBox.Name = "UnloadingRemarksFreeTextTextBox";
			this.UnloadingRemarksFreeTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 100, true);
			this.UnloadingRemarksFreeTextTextBox.TabIndex = 7;
			// 
			// UnloadingDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.UnloadingRemarksFreeTextTextBox);
			this.Controls.Add(this.UnloadingRemarksGrid);
			this.Name = "UnloadingDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 235, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UnloadingRemarksGrid)).EndInit();
			this.UnloadingRemarksGrid.ResumeLayout(false);
			this.UnloadingRemarksGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid UnloadingRemarksGrid;
		internal ZArchitecture.ZTextBox UnloadingRemarksFreeTextTextBox;
	}
}

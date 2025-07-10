namespace Enterprise.TransportConsignment.GUI
{
	partial class DtbConsignmentRunSheetInstructionsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.InstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InstructionsGrid)).BeginInit();
            this.InstructionsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet);
            // 
            // InstructionsGrid
            // 
            this.InstructionsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.InstructionsGrid, "RunSheetInstructions");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).K1_Sequence)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).Address.E2_CompanyName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).AddressAsSingleLine)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).TotalPickupWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheetInstruction)(((System.Collections.IList)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RunSheetInstructions)).SyncRoot)).K1_ReceivedBy)));
            this.InstructionsGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "K1_Sequence";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            zTextBoxColumnStyleInfo1.ColumnName = "Address+E2_CompanyName";
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zTextBoxColumnStyleInfo2.ColumnName = "AddressAsSingleLine";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
            this.InstructionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.InstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.InstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.InstructionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InstructionsGrid.GridId = "2d22df03-00d2-413f-aea7-c3fa76e853e9";
            this.InstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.InstructionsGrid.IsWholeRowSelectedOnClick = true;
            this.InstructionsGrid.LayoutKey = "ConsignmentsGrid";
            this.InstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.InstructionsGrid.Name = "InstructionsGrid";
            this.InstructionsGrid.ReadOnly = true;
            this.InstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 150, true);
            this.InstructionsGrid.TabIndex = 2;
            // 
            // DtbConsignmentRunSheetInstructionsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.InstructionsGrid);
            this.Name = "DtbConsignmentRunSheetInstructionsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 150, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InstructionsGrid)).EndInit();
            this.InstructionsGrid.ResumeLayout(false);
            this.InstructionsGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid InstructionsGrid;
	}
}

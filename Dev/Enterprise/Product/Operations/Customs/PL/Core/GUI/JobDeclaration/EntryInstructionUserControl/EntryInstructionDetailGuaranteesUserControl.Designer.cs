
namespace Enterprise.Customs.PL.GUI
{
	partial class EntryInstructionDetailGuaranteesUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();

			this.GuaranteesGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GuaranteesGrid)).BeginInit();
            this.GuaranteesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction);
            // 
            // GuaranteesGrid
            // 
            this.GuaranteesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.GuaranteesGrid, "Guarantees");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).Guarantees)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.GuaranteeBondDetail)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).Guarantees)).SyncRoot)).PW_BondType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.PL.Business.Declaration.GuaranteeBondDetail)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).Guarantees)).SyncRoot)).PW_CPH_Guarantee)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.GuaranteeBondDetail)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).Guarantees)).SyncRoot)).PW_BondNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.GuaranteeBondDetail)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).Guarantees)).SyncRoot)).PW_HolderIdentification)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.GuaranteeBondDetail)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).Guarantees)).SyncRoot)).PW_Password)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.PL.Business.Declaration.GuaranteeBondDetail)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).Guarantees)).SyncRoot)).PW_BondAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.GuaranteeBondDetail)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(null)).Guarantees)).SyncRoot)).PW_RX_NKCurrency)));
			this.GuaranteesGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.ColumnName = "PW_BondType";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PW_CPH_Guarantee";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Guarantees;
			zTextBoxColumnStyleInfo1.ColumnName = "PW_BondNumber";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
            zTextBoxColumnStyleInfo2.ColumnName = "PW_HolderIdentification";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo3.ColumnName = "PW_Password";
            zTextBoxColumnStyleInfo3.PasswordChar = '*';
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "GuaranteeBondAmountDecimalPlaces";
            zCalcEditColumnStyleInfo1.ColumnName = "PW_BondAmount";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.Decimals = 2;
			zTextBoxColumnStyleInfo4.ColumnName = "PW_RX_NKCurrency";
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            this.GuaranteesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GuaranteesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.GuaranteesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.GuaranteesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GuaranteesGrid.GridId = "5fffd7c8-f74d-4ae6-8c65-b3fa077a5cb4";
            this.GuaranteesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.GuaranteesGrid.LayoutKey = "GuaranteesGrid";
            this.GuaranteesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.GuaranteesGrid.Name = "GuaranteesGrid";
            this.GuaranteesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 207, true);
            this.GuaranteesGrid.TabIndex = 0;
            // 
            // EntryInstructionDetailGuaranteesUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.GuaranteesGrid);
            this.Name = "EntryInstructionDetailGuaranteesUserControl";
			this.CaptionRenderingEnabled = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 207, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GuaranteesGrid)).EndInit();
            this.GuaranteesGrid.ResumeLayout(false);
            this.GuaranteesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid GuaranteesGrid;
	}
}

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module
{
	public partial class CusStatementFilterControl
	{


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.grid.SuspendLayout();
            this.AddStripButton.SuspendLayout();
            this.RecentItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.BindingSource.SetBindingMember(this.grid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusStatementLineCharge)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusStatementLineCharge)(null)).AccountNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ZA.Business.CusStatementLineCharge)(null)).ProcessDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusStatementLineCharge)(null)).EntryNum)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusStatementLineCharge)(null)).ChargeTypeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.ZA.Business.CusStatementLineCharge)(null)).B4_ChargeAmount)));
            zTextBoxColumnStyleInfo1.Caption = "";
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("eaee5bd5-dc27-4f43-b766-c380521c46be", "FAN (Financial Account Number)");
            zTextBoxColumnStyleInfo1.ColumnName = "AccountNo";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
            zDateEditColumnStyleInfo1.Caption = "";
            zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("d90e926e-5eae-4817-b97f-7c206054856c", "Statement Date");
            zDateEditColumnStyleInfo1.ColumnName = "ProcessDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo2.Caption = "";
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("938fee33-e40d-4c98-9a16-2818ffe57361", "LRN / PRN Number");
            zTextBoxColumnStyleInfo2.ColumnName = "EntryNum";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            zTextBoxColumnStyleInfo3.Caption = "";
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("7513b195-c452-4ba3-b506-77af36ee4d18", "Charge Type");
            zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            zTextBoxColumnStyleInfo3.ColumnName = "ChargeTypeDescription";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo4.Caption = "";
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("1c895d61-6ab2-4624-b2d0-55e2b750c629", "Amount");
            zTextBoxColumnStyleInfo4.ColumnName = "B4_ChargeAmount";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 141, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.CusStatementLineCharge);
            // 
            // CusStatementFilterControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Name = "CusStatementFilterControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 293, true);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.grid.ResumeLayout(false);
            this.grid.PerformLayout();
            this.AddStripButton.ResumeLayout(true);
            this.AddStripButton.PerformLayout();
            this.RecentItemsPanel.ResumeLayout(false);
            this.RecentItemsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

	}
}
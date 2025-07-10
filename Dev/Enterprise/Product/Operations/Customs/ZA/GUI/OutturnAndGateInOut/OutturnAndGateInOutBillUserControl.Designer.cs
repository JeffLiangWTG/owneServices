using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OutturnAndGateInOutBillUserControl
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
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.zGroupBoxBill = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zTextBoxMRN = new Enterprise.ZArchitecture.ZTextBox();
            this.zDropEditCustomsStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.zTextBoxCustomsCPC = new Enterprise.ZArchitecture.ZTextBox();
            this.zTextBoxLRN = new Enterprise.ZArchitecture.ZTextBox();
            this.zCodeFindBoxBillIssuer = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zTextBoxBillNumber = new Enterprise.ZArchitecture.ZTextBox();
            this.zGridBills = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zGroupBoxBill.SuspendLayout();
            this.zDropEditCustomsStatus.SuspendLayout();
            this.zCodeFindBoxBillIssuer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zGridBills)).BeginInit();
            this.zGridBills.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.AsycudaManifestHeader);
            // 
            // zGroupBoxBill
            // 
            this.zGroupBoxBill.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("c9da775d-48dd-42b2-b9ad-e886e77b9919", "Bill Details");
            this.zGroupBoxBill.Controls.Add(this.zTextBoxMRN);
            this.zGroupBoxBill.Controls.Add(this.zDropEditCustomsStatus);
            this.zGroupBoxBill.Controls.Add(this.zTextBoxCustomsCPC);
            this.zGroupBoxBill.Controls.Add(this.zTextBoxLRN);
            this.zGroupBoxBill.Controls.Add(this.zCodeFindBoxBillIssuer);
            this.zGroupBoxBill.Controls.Add(this.zTextBoxBillNumber);
            this.zGroupBoxBill.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.zGroupBoxBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 389, true);
            this.zGroupBoxBill.Name = "zGroupBoxBill";
            this.zGroupBoxBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 88, true);
            this.zGroupBoxBill.TabIndex = 1;
            this.zGroupBoxBill.TabStop = false;
            // 
            // zTextBoxMRN
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxMRN, "Bills.MRN");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).MRN)));
            this.zTextBoxMRN.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(582, 57, true);
            this.zTextBoxMRN.Name = "zTextBoxMRN";
            this.zTextBoxMRN.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
            this.zTextBoxMRN.TabIndex = 5;
            // 
            // zDropEditCustomsStatus
            // 
            this.zDropEditCustomsStatus.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zDropEditCustomsStatus, "Bills.ABL_BillStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).ABL_BillStatus)));
            this.zDropEditCustomsStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(582, 31, true);
            this.zDropEditCustomsStatus.Name = "zDropEditCustomsStatus";
            this.zDropEditCustomsStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
            this.zDropEditCustomsStatus.TabIndex = 4;
            // 
            // zTextBoxCustomsCPC
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxCustomsCPC, "Bills.CustomsCPC");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).CustomsCPC)));
            this.zTextBoxCustomsCPC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 57, true);
            this.zTextBoxCustomsCPC.Name = "zTextBoxCustomsCPC";
            this.zTextBoxCustomsCPC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
            this.zTextBoxCustomsCPC.TabIndex = 3;
            // 
            // zTextBoxLRN
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxLRN, "Bills.LRN");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).LRN)));
            this.zTextBoxLRN.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 31, true);
            this.zTextBoxLRN.Name = "zTextBoxLRN";
            this.zTextBoxLRN.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
            this.zTextBoxLRN.TabIndex = 2;
            // 
            // zCodeFindBoxBillIssuer
            // 
            this.zCodeFindBoxBillIssuer.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.zCodeFindBoxBillIssuer, "Bills.ABL_BillIssuer");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).ABL_BillIssuer)));
            this.zCodeFindBoxBillIssuer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 57, true);
            this.zCodeFindBoxBillIssuer.Name = "zCodeFindBoxBillIssuer";
            this.zCodeFindBoxBillIssuer.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.zCodeFindBoxBillIssuer.ParentType = null;
            this.zCodeFindBoxBillIssuer.ShowDescriptionBox = false;
            this.zCodeFindBoxBillIssuer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.zCodeFindBoxBillIssuer.TabIndex = 1;
            // 
            // zTextBoxBillNumber
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxBillNumber, "Bills.ABL_BillNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).ABL_BillNumber)));
            this.zTextBoxBillNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 31, true);
            this.zTextBoxBillNumber.Name = "zTextBoxBillNumber";
            this.zTextBoxBillNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
            this.zTextBoxBillNumber.TabIndex = 0;
            // 
            // zGridBills
            // 
            this.zGridBills.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.zGridBills, "Bills");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).ABL_BillNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).ABL_BillIssuer)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).LRN)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).CustomsCPC)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.AsycudaBill)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Bills)).SyncRoot)).MRN)));
            this.zGridBills.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "ABL_BillNumber";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo1.ColumnName = "ABL_BillIssuer";
            zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.ColumnName = "LRN";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo3.ColumnName = "CustomsCPC";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo4.ColumnName = "MRN";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            this.zGridBills.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.zGridBills.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.zGridBills.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.zGridBills.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.zGridBills.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.zGridBills.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zGridBills.GridId = "14f1b367-4fd1-4a26-ba20-636b676ac94f";
            this.zGridBills.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.zGridBills.LayoutKey = "zGridBills";
            this.zGridBills.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zGridBills.Name = "zGridBills";
            this.zGridBills.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 389, true);
            this.zGridBills.TabIndex = 0;
            // 
            // OutturnAndGateInOutBillUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.zGridBills);
            this.Controls.Add(this.zGroupBoxBill);
            this.Name = "OutturnAndGateInOutBillUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 477, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zGroupBoxBill.ResumeLayout(false);
            this.zGroupBoxBill.PerformLayout();
            this.zDropEditCustomsStatus.ResumeLayout(true);
            this.zDropEditCustomsStatus.PerformLayout();
            this.zCodeFindBoxBillIssuer.ResumeLayout(true);
            this.zCodeFindBoxBillIssuer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zGridBills)).EndInit();
            this.zGridBills.ResumeLayout(false);
            this.zGridBills.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

#endregion

		ZGroupBox zGroupBoxBill;
		internal ZArchitecture.ZTextBox zTextBoxCustomsCPC;
		internal ZArchitecture.ZTextBox zTextBoxLRN;
		ZCodeFindBox zCodeFindBoxBillIssuer;
		ZArchitecture.ZTextBox zTextBoxBillNumber;
		internal ZArchitecture.ZGrid zGridBills;
		ZDropEdit zDropEditCustomsStatus;
		ZArchitecture.ZTextBox zTextBoxMRN;
	}
}

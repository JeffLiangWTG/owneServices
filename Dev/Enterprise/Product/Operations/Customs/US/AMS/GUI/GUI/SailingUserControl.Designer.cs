namespace Enterprise.Customs.US.AMS.GUI
{
	partial class SailingUserControl
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
			this.SailingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SailingStatisticsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AMSBillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SailingContainersCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SailingBillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ContainersOnHeaderCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EditSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClearSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SailingETADateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SailingDischargeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SailingETDDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SailingPortOfLadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RefreshStatisticsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SailingATDDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SailingGroupBox.SuspendLayout();
			this.SailingStatisticsPanel.SuspendLayout();
			this.SailingETADateEdit.SuspendLayout();
			this.SailingETDDateEdit.SuspendLayout();
			this.SailingATDDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.CusInBondHeader);
			// 
			// SailingGroupBox
			// 
			this.SailingGroupBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("f4f377a1-a018-48b0-940b-c82dd78a95ef", "Sailing");
			this.SailingGroupBox.Controls.Add(this.RefreshStatisticsButton);
			this.SailingGroupBox.Controls.Add(this.SailingStatisticsPanel);
			this.SailingGroupBox.Controls.Add(this.EditSailingButton);
			this.SailingGroupBox.Controls.Add(this.ClearSailingButton);
			this.SailingGroupBox.Controls.Add(this.SelectSailingButton);
			this.SailingGroupBox.Controls.Add(this.SailingETADateEdit);
			this.SailingGroupBox.Controls.Add(this.SailingDischargeTextBox);
			this.SailingGroupBox.Controls.Add(this.SailingETDDateEdit);
			this.SailingGroupBox.Controls.Add(this.SailingPortOfLadingTextBox);
			this.SailingGroupBox.Controls.Add(this.SailingATDDateEdit);
			this.SailingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SailingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SailingGroupBox.Name = "SailingGroupBox";
			this.SailingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1070, 76, true);
			this.SailingGroupBox.TabIndex = 0;
			this.SailingGroupBox.TabStop = false;
			// 
			// SailingStatisticsPanel
			// 
			this.SailingStatisticsPanel.Controls.Add(this.AMSBillsCalcEdit);
			this.SailingStatisticsPanel.Controls.Add(this.SailingContainersCalcEdit);
			this.SailingStatisticsPanel.Controls.Add(this.SailingBillsCalcEdit);
			this.SailingStatisticsPanel.Controls.Add(this.ContainersOnHeaderCalcEdit);
			this.SailingStatisticsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(710, 14, true);
			this.SailingStatisticsPanel.Name = "SailingStatisticsPanel";
			this.SailingStatisticsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 56, true);
			this.SailingStatisticsPanel.TabIndex = 23;
			// 
			// AMSBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AMSBillsCalcEdit, "BH_NoOfAMSBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_NoOfAMSBills)));
			this.AMSBillsCalcEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("69249363-b446-4f65-abdc-0ed36a0efb6f", "Bills on this AMS");
			this.AMSBillsCalcEdit.DecimalPlaces = 2;
			this.AMSBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 5, true);
			this.AMSBillsCalcEdit.Name = "AMSBillsCalcEdit";
			this.AMSBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.AMSBillsCalcEdit.TabIndex = 18;
			this.AMSBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SailingContainersCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SailingContainersCalcEdit, "BH_NoOfSailingContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_NoOfSailingContainers)));
			this.SailingContainersCalcEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("902b2a5a-49b1-42bc-9c3d-9e9650b2b406", "Containers related to Sailing");
			this.SailingContainersCalcEdit.DecimalPlaces = 2;
			this.SailingContainersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 31, true);
			this.SailingContainersCalcEdit.Name = "SailingContainersCalcEdit";
			this.SailingContainersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.SailingContainersCalcEdit.TabIndex = 21;
			this.SailingContainersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SailingBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SailingBillsCalcEdit, "BH_NoOfSailingBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_NoOfSailingBills)));
			this.SailingBillsCalcEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("7fdd5956-3376-49f5-b231-21ddbb23d673", "Bills related to Sailing");
			this.SailingBillsCalcEdit.DecimalPlaces = 2;
			this.SailingBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 5, true);
			this.SailingBillsCalcEdit.Name = "SailingBillsCalcEdit";
			this.SailingBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.SailingBillsCalcEdit.TabIndex = 19;
			this.SailingBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ContainersOnHeaderCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ContainersOnHeaderCalcEdit, "BH_NoOfAMSContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_NoOfAMSContainers)));
			this.ContainersOnHeaderCalcEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("96d16ad9-03fc-4663-90a6-d099a0c6d123", "Containers on this AMS");
			this.ContainersOnHeaderCalcEdit.DecimalPlaces = 2;
			this.ContainersOnHeaderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 31, true);
			this.ContainersOnHeaderCalcEdit.Name = "ContainersOnHeaderCalcEdit";
			this.ContainersOnHeaderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.ContainersOnHeaderCalcEdit.TabIndex = 20;
			this.ContainersOnHeaderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EditSailingButton
			// 
			this.EditSailingButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("6c856a10-92cc-409e-84b6-dfa012c80795", "Edit Sailing");
			this.EditSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 43, true);
			this.EditSailingButton.Name = "EditSailingButton";
			this.EditSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.EditSailingButton.TabIndex = 7;
			this.EditSailingButton.UseVisualStyleBackColor = true;
			this.EditSailingButton.Click += new System.EventHandler(this.EditSailingButton_Click);
			// 
			// ClearSailingButton
			// 
			this.ClearSailingButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("a14cb68e-8ae7-44ce-8ee5-b3c4b8f8f0b5", "Clear Sailing");
			this.ClearSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 17, true);
			this.ClearSailingButton.Name = "ClearSailingButton";
			this.ClearSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.ClearSailingButton.TabIndex = 6;
			this.ClearSailingButton.UseVisualStyleBackColor = true;
			this.ClearSailingButton.Click += new System.EventHandler(this.ClearSailingButton_Click);
			// 
			// SelectSailingButton
			// 
			this.SelectSailingButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("de9f8732-880a-4064-93d3-ac16b415dbf4", "Select Sailing");
			this.SelectSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 17, true);
			this.SelectSailingButton.Name = "SelectSailingButton";
			this.SelectSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.SelectSailingButton.TabIndex = 5;
			this.SelectSailingButton.UseVisualStyleBackColor = true;
			this.SelectSailingButton.Click += new System.EventHandler(this.SelectSailingButton_Click);
			// 
			// SailingETADateEdit
			// 
			this.SailingETADateEdit.AllowDrop = true;
			this.SailingETADateEdit.AutoCompleteMonthThreshold = 1;
			this.SailingETADateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SailingETADateEdit, "Sailings.JX_JB_E_ARV");
			this.SailingETADateEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("67cd8fd7-65a5-4379-9e4f-0932415a9521", "ETA");
			this.SailingETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 45, true);
			this.SailingETADateEdit.Name = "SailingETADateEdit";
			this.SailingETADateEdit.TabIndex = 4;
			// 
			// SailingDischargeTextBox
			// 
			this.BindingSource.SetBindingMember(this.SailingDischargeTextBox, "Sailings.JX_JB_RL_NKPortOfDischarge");
			this.SailingDischargeTextBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("0af6668d-03a6-46c1-974c-28c107bd8a42", "Discharge");
			this.SailingDischargeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 19, true);
			this.SailingDischargeTextBox.Name = "SailingDischargeTextBox";
			this.SailingDischargeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SailingDischargeTextBox.TabIndex = 2;
			// 
			// SailingATDDateEdit
			// 
			this.SailingATDDateEdit.AllowDrop = true;
			this.SailingATDDateEdit.AutoCompleteMonthThreshold = 1;
			this.SailingATDDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SailingATDDateEdit, "BH_SailingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).BH_SailingDate)));
			this.SailingATDDateEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("6a33ce03-252b-4b35-9343-553acc24bd86", "ATD");
			this.SailingATDDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SailingATDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 45, true);
			this.SailingATDDateEdit.Name = "SailingATDDateEdit";
			this.SailingATDDateEdit.TabIndex = 3;
			// 
			// SailingETDDateEdit
			// 
			this.SailingETDDateEdit.AllowDrop = true;
			this.SailingETDDateEdit.AutoCompleteMonthThreshold = 1;
			this.SailingETDDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SailingETDDateEdit, "Sailings.JX_JA_E_DEP");
			this.SailingETDDateEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("b7079496-4a5d-4691-b8ed-ff688adff416", "ETD");
			this.SailingETDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 45, true);
			this.SailingETDDateEdit.Name = "SailingETDDateEdit";
			this.SailingETDDateEdit.TabIndex = 1;
			// 
			// SailingPortOfLadingTextBox
			// 
			this.BindingSource.SetBindingMember(this.SailingPortOfLadingTextBox, "Sailings.JX_JA_RL_NKPortOfLoading");
			this.SailingPortOfLadingTextBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("0115c8ed-1dc2-406a-a85c-5bd560daad78", "Loading");
			this.SailingPortOfLadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 19, true);
			this.SailingPortOfLadingTextBox.Name = "SailingPortOfLadingTextBox";
			this.SailingPortOfLadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SailingPortOfLadingTextBox.TabIndex = 0;
			// 
			// RefreshStatisticsButton
			// 
			this.RefreshStatisticsButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("92157783-5f92-4cf7-b091-24cbc0758ad7", "Refresh Statistics");
			this.RefreshStatisticsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 43, true);
			this.RefreshStatisticsButton.Name = "RefreshStatisticsButton";
			this.RefreshStatisticsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
			this.RefreshStatisticsButton.TabIndex = 24;
			this.RefreshStatisticsButton.UseVisualStyleBackColor = true;
			this.RefreshStatisticsButton.Click += new System.EventHandler(this.RefreshStatisticsButton_Click);
			// 
			// SailingUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SailingGroupBox);
			this.Name = "SailingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 76, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SailingGroupBox.ResumeLayout(false);
			this.SailingGroupBox.PerformLayout();
			this.SailingStatisticsPanel.ResumeLayout(false);
			this.SailingStatisticsPanel.PerformLayout();
			this.SailingETADateEdit.ResumeLayout(true);
			this.SailingETADateEdit.PerformLayout();
			this.SailingETDDateEdit.ResumeLayout(true);
			this.SailingETDDateEdit.PerformLayout();
			this.SailingATDDateEdit.ResumeLayout(true);
			this.SailingATDDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SailingGroupBox;
		private ZArchitecture.ZTextBox SailingPortOfLadingTextBox;
		private ZArchitecture.GUI.ZDateEdit SailingETDDateEdit;
		private ZArchitecture.ZTextBox SailingDischargeTextBox;
		private ZArchitecture.GUI.ZDateEdit SailingETADateEdit;
		internal ZArchitecture.GUI.ZButton EditSailingButton;
		private ZArchitecture.GUI.ZButton ClearSailingButton;
		private ZArchitecture.GUI.ZButton SelectSailingButton;
		private ZArchitecture.ZCalcEdit AMSBillsCalcEdit;
		private ZArchitecture.ZCalcEdit SailingContainersCalcEdit;
		private ZArchitecture.ZCalcEdit SailingBillsCalcEdit;
		private ZArchitecture.ZCalcEdit ContainersOnHeaderCalcEdit;
		internal ZArchitecture.GUI.ZPanel SailingStatisticsPanel;
		internal ZArchitecture.GUI.ZButton RefreshStatisticsButton;
		private ZArchitecture.GUI.ZDateEdit SailingATDDateEdit;
	}
}

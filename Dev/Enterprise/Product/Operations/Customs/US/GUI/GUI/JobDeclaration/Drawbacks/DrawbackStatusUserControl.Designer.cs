namespace Enterprise.Customs.US.GUI
{
	partial class DrawbackStatusUserControl
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
			this.components = new System.ComponentModel.Container();
			this.StatusTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.StatusTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StatementPaymentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.liquidatedDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.liquidationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StatusSummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LiquidationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.liquidationDetailsUserControl = new Enterprise.Customs.US.GUI.LiquidationDetailsUserControl();
			this.liquidationsUserControl = new Enterprise.Customs.US.GUI.LiquidationsUserControl();
			this.StatusNotificationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.entrySummaryStatusNotificationsUserControl = new Enterprise.Customs.US.GUI.EntrySummaryStatusNotificationsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusTabControl.SuspendLayout();
			this.StatusTabPage.SuspendLayout();
			this.StatementPaymentGroupBox.SuspendLayout();
			this.liquidationDateEdit.SuspendLayout();
			this.StatusSummaryGroupBox.SuspendLayout();
			this.LiquidationTabPage.SuspendLayout();
			this.liquidationDetailsUserControl.SuspendLayout();
			this.liquidationsUserControl.SuspendLayout();
			this.StatusNotificationsTabPage.SuspendLayout();
			this.entrySummaryStatusNotificationsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// StatusTabControl
			// 
			this.StatusTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.StatusTabControl.Controls.Add(this.StatusTabPage);
			this.StatusTabControl.Controls.Add(this.LiquidationTabPage);
			this.StatusTabControl.Controls.Add(this.StatusNotificationsTabPage);
			this.StatusTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusTabControl.Name = "StatusTabControl";
			this.StatusTabControl.SelectedIndex = 0;
			this.StatusTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 605, true);
			this.StatusTabControl.TabIndex = 0;
			// 
			// StatusTabPage
			// 
			this.StatusTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.StatusTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("03c58a55-fa39-4923-a8a3-91333cfd29a9", "Status");
			this.StatusTabPage.Controls.Add(this.StatementPaymentGroupBox);
			this.StatusTabPage.Controls.Add(this.StatusSummaryGroupBox);
			this.StatusTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatusTabPage.Name = "StatusTabPage";
			this.StatusTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.StatusTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 578, true);
			this.StatusTabPage.TabIndex = 0;
			this.StatusTabPage.Text = "Status";
			// 
			// StatementPaymentGroupBox
			// 
			this.StatementPaymentGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c62bffc1-a69d-4b9e-816f-27432102df9e", "Anticipated Liquidation");
			this.StatementPaymentGroupBox.Controls.Add(this.liquidatedDutyCalcEdit);
			this.StatementPaymentGroupBox.Controls.Add(this.liquidationDateEdit);
			this.StatementPaymentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 64, true);
			this.StatementPaymentGroupBox.Name = "StatementPaymentGroupBox";
			this.StatementPaymentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 56, true);
			this.StatementPaymentGroupBox.TabIndex = 1;
			this.StatementPaymentGroupBox.TabStop = false;
			this.StatementPaymentGroupBox.Text = "Anticipated Liquidation";
			// 
			// liquidatedDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.liquidatedDutyCalcEdit, "US_AnticipatedLiquidatedDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_AnticipatedLiquidatedDuty)));
			this.liquidatedDutyCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f24ba4dd-f26a-4613-93b9-96bbbd9fde1a", "Liquidated Duty");
			this.liquidatedDutyCalcEdit.DecimalPlaces = 2;
			this.liquidatedDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 23, true);
			this.liquidatedDutyCalcEdit.Name = "liquidatedDutyCalcEdit";
			this.liquidatedDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.liquidatedDutyCalcEdit.TabIndex = 1;
			this.liquidatedDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// liquidationDateEdit
			// 
			this.liquidationDateEdit.AllowDrop = true;
			this.liquidationDateEdit.AutoCompleteMonthThreshold = 1;
			this.liquidationDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.liquidationDateEdit, "US_AnticipatedLiquidationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_AnticipatedLiquidationDate)));
			this.liquidationDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e7fff252-4585-437b-80f0-cf82f13d8159", "Liquidation Date");
			this.liquidationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 23, true);
			this.liquidationDateEdit.Name = "liquidationDateEdit";
			this.liquidationDateEdit.TabIndex = 0;
			// 
			// StatusSummaryGroupBox
			// 
			this.StatusSummaryGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fc57c504-2174-496f-a21d-ba62a413a01a", "Summary");
			this.StatusSummaryGroupBox.Controls.Add(this.MessageStatusTextBox);
			this.StatusSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.StatusSummaryGroupBox.Name = "StatusSummaryGroupBox";
			this.StatusSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 56, true);
			this.StatusSummaryGroupBox.TabIndex = 0;
			this.StatusSummaryGroupBox.TabStop = false;
			this.StatusSummaryGroupBox.Text = "Summary";
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "JE_MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).JE_MessageStatusDescription)));
			this.MessageStatusTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c41843ba-1cac-44ae-b9f0-3f2dd9e0889b", "Message Status");
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 25, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.MessageStatusTextBox.TabIndex = 0;
			// 
			// LiquidationTabPage
			// 
			this.LiquidationTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.LiquidationTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("89fce64f-79df-4402-936e-73dc54681517", "Liquidation");
			this.LiquidationTabPage.Controls.Add(this.liquidationDetailsUserControl);
			this.LiquidationTabPage.Controls.Add(this.liquidationsUserControl);
			this.LiquidationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LiquidationTabPage.Name = "LiquidationTabPage";
			this.LiquidationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.LiquidationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 578, true);
			this.LiquidationTabPage.TabIndex = 1;
			this.LiquidationTabPage.Text = "Liquidation";
			// 
			// liquidationDetailsUserControl
			// 
			this.liquidationDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.liquidationDetailsUserControl, "Liquidations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.CusLiquidation)(((Enterprise.Customs.US.Business.CusLiquidation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Liquidations)).SyncRoot)))));
			this.liquidationDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.liquidationDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 171, true);
			this.liquidationDetailsUserControl.Name = "liquidationDetailsUserControl";
			this.liquidationDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(947, 405, true);
			this.liquidationDetailsUserControl.TabIndex = 1;
			// 
			// liquidationsUserControl
			// 
			this.liquidationsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.liquidationsUserControl, "Liquidations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.CusLiquidationCollection)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).Liquidations)));
			this.liquidationsUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.liquidationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.liquidationsUserControl.Name = "liquidationsUserControl";
			this.liquidationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(947, 169, true);
			this.liquidationsUserControl.TabIndex = 0;
			// 
			// StatusNotificationsTabPage
			// 
			this.StatusNotificationsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.StatusNotificationsTabPage.Controls.Add(this.entrySummaryStatusNotificationsUserControl);
			this.StatusNotificationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.StatusNotificationsTabPage.Name = "StatusNotificationsTabPage";
			this.StatusNotificationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StatusNotificationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 578, true);
			this.StatusNotificationsTabPage.TabIndex = 2;
			this.StatusNotificationsTabPage.Text = "Entry Summary Status Notifications";
			// 
			// entrySummaryStatusNotificationsUserControl
			// 
			this.entrySummaryStatusNotificationsUserControl.AllowDrop = true;
			this.entrySummaryStatusNotificationsUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.entrySummaryStatusNotificationsUserControl, "ENSStatusNotifications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.ErrorsRecordCollection)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).ENSStatusNotifications)));
			this.entrySummaryStatusNotificationsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.entrySummaryStatusNotificationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.entrySummaryStatusNotificationsUserControl.Name = "entrySummaryStatusNotificationsUserControl";
			this.entrySummaryStatusNotificationsUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.entrySummaryStatusNotificationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(949, 169, true);
			this.entrySummaryStatusNotificationsUserControl.TabIndex = 0;
			// 
			// DrawbackStatusUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StatusTabControl);
			this.Name = "DrawbackStatusUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 605, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusTabControl.ResumeLayout(false);
			this.StatusTabControl.PerformLayout();
			this.StatusTabPage.ResumeLayout(false);
			this.StatusTabPage.PerformLayout();
			this.StatementPaymentGroupBox.ResumeLayout(false);
			this.StatementPaymentGroupBox.PerformLayout();
			this.liquidationDateEdit.ResumeLayout(true);
			this.liquidationDateEdit.PerformLayout();
			this.StatusSummaryGroupBox.ResumeLayout(false);
			this.StatusSummaryGroupBox.PerformLayout();
			this.LiquidationTabPage.ResumeLayout(false);
			this.LiquidationTabPage.PerformLayout();
			this.liquidationDetailsUserControl.ResumeLayout(true);
			this.liquidationDetailsUserControl.PerformLayout();
			this.liquidationsUserControl.ResumeLayout(true);
			this.liquidationsUserControl.PerformLayout();
			this.StatusNotificationsTabPage.ResumeLayout(false);
			this.StatusNotificationsTabPage.PerformLayout();
			this.entrySummaryStatusNotificationsUserControl.ResumeLayout(true);
			this.entrySummaryStatusNotificationsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabControl StatusTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage StatusTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage LiquidationTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage StatusNotificationsTabPage;
		private EntrySummaryStatusNotificationsUserControl entrySummaryStatusNotificationsUserControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox StatusSummaryGroupBox;
		private LiquidationDetailsUserControl liquidationDetailsUserControl;

		private LiquidationsUserControl liquidationsUserControl;
		private Enterprise.ZArchitecture.ZTextBox MessageStatusTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox StatementPaymentGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit liquidationDateEdit;
		private Enterprise.ZArchitecture.ZCalcEdit liquidatedDutyCalcEdit;
	}
}

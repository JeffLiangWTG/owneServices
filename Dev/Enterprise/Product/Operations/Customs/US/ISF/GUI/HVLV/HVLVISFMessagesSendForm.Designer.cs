namespace Enterprise.Customs.US.ISF.GUI
{
	partial class HVLVISFMessagesSendForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.isfHeadersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonSend = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonSelectAll = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.isfHeadersGrid)).BeginInit();
			this.isfHeadersGrid.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 233, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ISF.Business.HVLVISFMessagesSendWrapper);
			// 
			// isfHeadersGrid
			// 
			this.isfHeadersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.isfHeadersGrid, "MessagesSendWrapperCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.isfHeadersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo1.CaptionResourceString = GUI.Res.GetData("372a6a59-dc7a-45d3-a319-7de826d1edf4", "Job No.");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "JobStatus";
			zTextBoxColumnStyleInfo2.CaptionResourceString = GUI.Res.GetData("64a88424-3105-4da0-ba81-ed3b0074f62d", "Status");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSend";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = GUI.Res.GetData("11068f26-332c-441b-b473-1a33d8f7411a", "Send ?");
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.isfHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.isfHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.isfHeadersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.isfHeadersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.isfHeadersGrid.GridId = "39ebe769-2aa1-414e-b7d2-33bc032fa704";
			this.isfHeadersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.isfHeadersGrid.LayoutKey = "isfHeadersGrid";
			this.isfHeadersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.isfHeadersGrid.Name = "isfHeadersGrid";
			this.isfHeadersGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.isfHeadersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 214, true);
			this.isfHeadersGrid.TabIndex = 1;
			// 
			// panelMatchTopTitle
			// 
			this.zPanel1.Controls.Add(this.ButtonCancel);
			this.zPanel1.Controls.Add(this.ButtonSend);
			this.zPanel1.Controls.Add(this.ButtonSelectAll);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 214, true);
			this.zPanel1.Name = "panelMatchTopTitle";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 50, true);
			this.zPanel1.TabIndex = 2;
			// 
			// ButtonCancel
			//
			this.ButtonCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.ButtonCancel.IsCaptionOverridden = true;
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 25, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 23, true);
			this.ButtonCancel.TabIndex = 3;
			this.ButtonCancel.Text = Res.GetString("0433fb75-2116-4be7-8b3e-a3d1152b9d07", "Cancel");
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// ButtonSend
			//
			this.ButtonSend.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.ButtonSend.IsCaptionOverridden = true;
			this.ButtonSend.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 25, true);
			this.ButtonSend.Name = "ButtonSend";
			this.ButtonSend.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 23, true);
			this.ButtonSend.TabIndex = 2;
			this.ButtonSend.Text = Res.GetString("a3a5a366-6792-42ed-a483-0550decdc667", "Send");
			this.ButtonSend.ToolTipCaption = null;
			this.ButtonSend.UseVisualStyleBackColor = true;
			this.ButtonSend.Click += new System.EventHandler(this.ButtonSend_Click);
			// 
			// ButtonSelectAll
			//
			this.ButtonSelectAll.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.ButtonSelectAll.IsCaptionOverridden = true;
			this.ButtonSelectAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 25, true);
			this.ButtonSelectAll.Name = "ButtonSelectAll";
			this.ButtonSelectAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 23, true);
			this.ButtonSelectAll.TabIndex = 0;
			this.ButtonSelectAll.Text = Res.GetString("5703daa1-2dee-4bc8-9b8c-1d030ab037c3", "Select/Deselect All");
			this.ButtonSelectAll.ToolTipCaption = null;
			this.ButtonSelectAll.UseVisualStyleBackColor = true;
			this.ButtonSelectAll.Click += new System.EventHandler(this.ButtonSelectAll_Click);
			// 
			// HVLVISFMessagesSendForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 288, true);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.isfHeadersGrid);
			this.DataSourceType = typeof(Enterprise.Customs.US.ISF.Business.HVLVISFMetaHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 288, true);
			this.Name = "HVLVISFMessagesSendForm";
			this.Text = Res.GetString("23581b5f-3765-4ac8-9878-3862fb730e5f", "ISF Messages");
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.isfHeadersGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.isfHeadersGrid)).EndInit();
			this.isfHeadersGrid.ResumeLayout(false);
			this.isfHeadersGrid.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid isfHeadersGrid;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZButton ButtonCancel;
		private ZArchitecture.GUI.ZButton ButtonSend;
		private ZArchitecture.GUI.ZButton ButtonSelectAll;
	}
}

namespace Enterprise.MasterFiles.GUI
{
	partial class ActiveUserForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.postingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.textBoxInitials = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxFullName = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxHomeBranch = new Enterprise.ZArchitecture.ZTextBox();
			this.dateEditUTCLoginTime = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.dateEditLocalLoginTime = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.dateEditUserLoginTime = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.textBoxComputerName = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxProcessId = new Enterprise.ZArchitecture.ZCalcEdit();
			this.groupBoxSemaphores = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.gridActiveSemaphores = new Enterprise.ZArchitecture.ZGrid();
			this.buttonRefresh = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBoxSemaphores.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridActiveSemaphores)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 436, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ActiveUser);
			// 
			// postingButtons
			// 
			this.postingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 405, true);
			this.postingButtons.Name = "postingButtons";
			this.postingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.postingButtons.TabIndex = 7;
			// 
			// textBoxInitials
			// 
			this.BindingSource.SetBindingMember(this.textBoxInitials, "AU_Initials");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).AU_Initials)));
			this.textBoxInitials.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|2cc993b8-a554-4def-b5db-8b48cd29dc6e", "Initials", "User Initials", "");
			this.textBoxInitials.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 12, true);
			this.textBoxInitials.Name = "textBoxInitials";
			this.textBoxInitials.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.textBoxInitials.TabIndex = 8;
			// 
			// textBoxFullName
			// 
			this.BindingSource.SetBindingMember(this.textBoxFullName, "AU_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).AU_FullName)));
			this.textBoxFullName.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|2ec01ac9-fa01-456f-8ecf-72fd30b1de3d", "Name", "Full Name", "User Full Name", "");
			this.textBoxFullName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 38, true);
			this.textBoxFullName.Name = "textBoxFullName";
			this.textBoxFullName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.textBoxFullName.TabIndex = 9;
			// 
			// textBoxHomeBranch
			// 
			this.BindingSource.SetBindingMember(this.textBoxHomeBranch, "Staff+HomeBranch+HomePort+RL_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).Staff.HomeBranch.HomePort.RL_Code)));
			this.textBoxHomeBranch.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|e3390e56-76ac-482b-b28c-b88d070fc25b", "Home Branch");
			this.textBoxHomeBranch.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 64, true);
			this.textBoxHomeBranch.Name = "textBoxHomeBranch";
			this.textBoxHomeBranch.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.textBoxHomeBranch.TabIndex = 10;
			// 
			// dateEditUTCLoginTime
			// 
			this.dateEditUTCLoginTime.AutoCompleteMonthThreshold = 1;
			this.dateEditUTCLoginTime.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditUTCLoginTime, "AU_UTCLoginTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).AU_UTCLoginTime)));
			this.dateEditUTCLoginTime.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|18c9235c-049a-4d84-a532-29347188c0ba", "UTC Login Time");
			this.dateEditUTCLoginTime.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.dateEditUTCLoginTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 12, true);
			this.dateEditUTCLoginTime.Name = "dateEditUTCLoginTime";
			this.dateEditUTCLoginTime.TabIndex = 11;
			// 
			// dateEditLocalLoginTime
			// 
			this.dateEditLocalLoginTime.AutoCompleteMonthThreshold = 1;
			this.dateEditLocalLoginTime.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditLocalLoginTime, "AU_YourLocalLoginTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).AU_YourLocalLoginTime)));
			this.dateEditLocalLoginTime.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|2c73cc26-d6d3-4f80-86b8-6970ffb52d7e", "Local Login Time");
			this.dateEditLocalLoginTime.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.dateEditLocalLoginTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 38, true);
			this.dateEditLocalLoginTime.Name = "dateEditLocalLoginTime";
			this.dateEditLocalLoginTime.TabIndex = 12;
			// 
			// dateEditUserLoginTime
			// 
			this.dateEditUserLoginTime.AutoCompleteMonthThreshold = 1;
			this.dateEditUserLoginTime.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditUserLoginTime, "AU_UserLocalLoginTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).AU_UserLocalLoginTime)));
			this.dateEditUserLoginTime.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|c85bfd82-76a0-4221-9329-952d18cf8368", "Home Branch Login Time");
			this.dateEditUserLoginTime.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.dateEditUserLoginTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 64, true);
			this.dateEditUserLoginTime.Name = "dateEditUserLoginTime";
			this.dateEditUserLoginTime.TabIndex = 13;
			// 
			// textBoxComputerName
			// 
			this.BindingSource.SetBindingMember(this.textBoxComputerName, "AU_ComputerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).AU_ComputerName)));
			this.textBoxComputerName.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|c301b1c1-f2d7-40b0-9053-e2e4ea647fe1", "Computer Name");
			this.textBoxComputerName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 90, true);
			this.textBoxComputerName.Name = "textBoxComputerName";
			this.textBoxComputerName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.textBoxComputerName.TabIndex = 14;
			// 
			// textBoxProcessId
			// 
			this.BindingSource.SetBindingMember(this.textBoxProcessId, "AU_ProcessID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).AU_ProcessID)));
			this.textBoxProcessId.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|99eb01bb-d614-46d5-be47-13dbc6b29799", "Process ID");
			this.textBoxProcessId.DecimalPlaces = 0;
			this.textBoxProcessId.Decimals = 0;
			this.textBoxProcessId.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 90, true);
			this.textBoxProcessId.Name = "textBoxProcessId";
			this.textBoxProcessId.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.textBoxProcessId.TabIndex = 15;
			this.textBoxProcessId.Text = "0";
			this.textBoxProcessId.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// groupBoxSemaphores
			// 
			this.groupBoxSemaphores.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxSemaphores.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|9b424a42-1817-45da-85a8-1b3b484cff36", "Active Semaphores");
			this.groupBoxSemaphores.Controls.Add(this.gridActiveSemaphores);
			this.groupBoxSemaphores.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 152, true);
			this.groupBoxSemaphores.Name = "groupBoxSemaphores";
			this.groupBoxSemaphores.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 237, true);
			this.groupBoxSemaphores.TabIndex = 16;
			this.groupBoxSemaphores.TabStop = false;
			// 
			// gridActiveSemaphores
			// 
			this.gridActiveSemaphores.AllowNavigation = false;
			this.gridActiveSemaphores.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.gridActiveSemaphores, "ActiveSemaphores");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).ActiveSemaphores)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ActiveSemaphoreHandle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).ActiveSemaphores)).SyncRoot)).AS_ServiceClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ActiveSemaphoreHandle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).ActiveSemaphores)).SyncRoot)).AS_LockInfo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ActiveSemaphoreHandle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).ActiveSemaphores)).SyncRoot)).AS_UseCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ActiveSemaphoreHandle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).ActiveSemaphores)).SyncRoot)).AS_AcquiredTimeUTC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ActiveSemaphoreHandle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).ActiveSemaphores)).SyncRoot)).AcquiredTimeYourLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ActiveSemaphoreHandle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUser)(null)).ActiveSemaphores)).SyncRoot)).AcquiredTimeUserLocal)));
			this.gridActiveSemaphores.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|d1b57761-7a1f-4a65-8f8a-d46a89e467d5", "Service Class");
			zTextBoxColumnStyleInfo1.ColumnName = "AS_ServiceClass";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|39122950-c355-4ee0-ac69-c09cb7e18706", "Lock Info");
			zTextBoxColumnStyleInfo2.ColumnName = "AS_LockInfo";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|878b9917-6ee1-4c16-86d5-98730564ca82", "Use Count");
			zCalcEditColumnStyleInfo1.ColumnName = "AS_UseCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|8e073e39-31d9-4a70-a117-a49e16c795c9", "UTC Time", "UTC Creation Time", "");
			zDateEditColumnStyleInfo1.ColumnName = "AS_AcquiredTimeUTC";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|ea060818-29b3-4e32-a7b4-f8f4e7598926", "Local Time", "Local Creation Time", "");
			zDateEditColumnStyleInfo2.ColumnName = "AcquiredTimeYourLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|a4163403-eede-49fb-a7bb-22fd94c43904", "Branch Time", "Home Branch Time", "Home Branch Creation Time", "");
			zDateEditColumnStyleInfo3.ColumnName = "AcquiredTimeUserLocal";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			this.gridActiveSemaphores.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridActiveSemaphores.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.gridActiveSemaphores.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.gridActiveSemaphores.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.gridActiveSemaphores.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.gridActiveSemaphores.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.gridActiveSemaphores.GridId = "1f7e6763-a66f-4baf-9c95-9bce2309c749";
			this.gridActiveSemaphores.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridActiveSemaphores.IsWholeRowSelectedOnClick = true;
			this.gridActiveSemaphores.LayoutKey = "gridActiveSemaphores";
			this.gridActiveSemaphores.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.gridActiveSemaphores.Name = "gridActiveSemaphores";
			this.gridActiveSemaphores.ReadOnly = true;
			this.gridActiveSemaphores.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 212, true);
			this.gridActiveSemaphores.TabIndex = 0;
			// 
			// buttonRefresh
			// 
			this.buttonRefresh.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|8868fc31-2600-4ce1-8245-5f8c0d35db52", "Refresh");
			this.buttonRefresh.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 123, true);
			this.buttonRefresh.Name = "buttonRefresh";
			this.buttonRefresh.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.buttonRefresh.TabIndex = 17;
			this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
			// 
			// ActiveUserForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 460, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserForm|2d535ce1-a8ac-4b2c-9a56-8d872f632789", "Active User");
			this.Controls.Add(this.buttonRefresh);
			this.Controls.Add(this.groupBoxSemaphores);
			this.Controls.Add(this.textBoxProcessId);
			this.Controls.Add(this.textBoxComputerName);
			this.Controls.Add(this.dateEditUserLoginTime);
			this.Controls.Add(this.dateEditLocalLoginTime);
			this.Controls.Add(this.dateEditUTCLoginTime);
			this.Controls.Add(this.textBoxHomeBranch);
			this.Controls.Add(this.textBoxFullName);
			this.Controls.Add(this.textBoxInitials);
			this.Controls.Add(this.postingButtons);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.ActiveUser);
			this.Name = "ActiveUserForm";
			this.Text = "Active User";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.postingButtons, 0);
			this.Controls.SetChildIndex(this.textBoxInitials, 0);
			this.Controls.SetChildIndex(this.textBoxFullName, 0);
			this.Controls.SetChildIndex(this.textBoxHomeBranch, 0);
			this.Controls.SetChildIndex(this.dateEditUTCLoginTime, 0);
			this.Controls.SetChildIndex(this.dateEditLocalLoginTime, 0);
			this.Controls.SetChildIndex(this.dateEditUserLoginTime, 0);
			this.Controls.SetChildIndex(this.textBoxComputerName, 0);
			this.Controls.SetChildIndex(this.textBoxProcessId, 0);
			this.Controls.SetChildIndex(this.groupBoxSemaphores, 0);
			this.Controls.SetChildIndex(this.buttonRefresh, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBoxSemaphores.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridActiveSemaphores)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtons;
		private Enterprise.ZArchitecture.ZTextBox textBoxInitials;
		private Enterprise.ZArchitecture.ZTextBox textBoxFullName;
		private Enterprise.ZArchitecture.ZTextBox textBoxHomeBranch;
		private Enterprise.ZArchitecture.GUI.ZDateEdit dateEditUTCLoginTime;
		private Enterprise.ZArchitecture.GUI.ZDateEdit dateEditLocalLoginTime;
		private Enterprise.ZArchitecture.GUI.ZDateEdit dateEditUserLoginTime;
		private Enterprise.ZArchitecture.ZTextBox textBoxComputerName;
		private Enterprise.ZArchitecture.ZCalcEdit textBoxProcessId;
		private Enterprise.ZArchitecture.GUI.ZGroupBox groupBoxSemaphores;
		private Enterprise.ZArchitecture.ZGrid gridActiveSemaphores;
		private Enterprise.ZArchitecture.GUI.ZButton buttonRefresh;
	}
}

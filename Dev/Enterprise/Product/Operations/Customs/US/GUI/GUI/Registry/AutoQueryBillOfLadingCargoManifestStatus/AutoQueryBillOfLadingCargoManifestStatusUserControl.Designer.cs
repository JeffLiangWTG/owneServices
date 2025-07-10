namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class AutoQueryBillOfLadingCargoManifestStatusUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SendBasedOnETACheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SendOnETAGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendOnFirstSaveGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SendOnFirstSaveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UpdateEntryWithResultsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SendOnETAGroupBox.SuspendLayout();
			this.SendOnFirstSaveGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.AutoQueryBillOfLadingCargoManifestStatus);
			// 
			// SendBasedOnETACheckBox
			// 
			this.BindingSource.SetBindingMember(this.SendBasedOnETACheckBox, "SendBasedOnETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.DataRegistry.Business.AutoQueryBillOfLadingCargoManifestStatus)(null)).SendBasedOnETA)));
			this.SendBasedOnETACheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SendBasedOnETACheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.SendBasedOnETACheckBox.Name = "SendBasedOnETACheckBox";
			this.SendBasedOnETACheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.SendBasedOnETACheckBox.TabIndex = 0;
			this.SendBasedOnETACheckBox.Text = "Send based on ETA";
			this.SendBasedOnETACheckBox.UseVisualStyleBackColor = true;
			// 
			// SendOnETAGroupBox
			// 
			this.SendOnETAGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c91cd3c0-1e6b-40f5-a640-6b1dca01bce8", "Send on ETA");
			this.SendOnETAGroupBox.Controls.Add(this.SendBasedOnETACheckBox);
			this.SendOnETAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.SendOnETAGroupBox.Name = "SendOnETAGroupBox";
			this.SendOnETAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 46, true);
			this.SendOnETAGroupBox.TabIndex = 0;
			this.SendOnETAGroupBox.TabStop = false;
			// 
			// SendOnFirstSaveGroupBox
			// 
			this.SendOnFirstSaveGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1d2d5eea-c743-4d32-9e69-335455568d3d", "Send on First Save");
			this.SendOnFirstSaveGroupBox.Controls.Add(this.SendOnFirstSaveCheckBox);
			this.SendOnFirstSaveGroupBox.Controls.Add(this.UpdateEntryWithResultsCheckBox);
			this.SendOnFirstSaveGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 57, true);
			this.SendOnFirstSaveGroupBox.Name = "SendOnFirstSaveGroupBox";
			this.SendOnFirstSaveGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 74, true);
			this.SendOnFirstSaveGroupBox.TabIndex = 0;
			this.SendOnFirstSaveGroupBox.TabStop = false;
			// 
			// SendOnFirstSaveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SendOnFirstSaveCheckBox, "SendOnFirstSave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.DataRegistry.Business.AutoQueryBillOfLadingCargoManifestStatus)(null)).SendOnFirstSave)));
			this.SendOnFirstSaveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SendOnFirstSaveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.SendOnFirstSaveCheckBox.Name = "SendOnFirstSaveCheckBox";
			this.SendOnFirstSaveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.SendOnFirstSaveCheckBox.TabIndex = 0;
			this.SendOnFirstSaveCheckBox.Text = "Send on first save of declaration";
			this.SendOnFirstSaveCheckBox.UseVisualStyleBackColor = true;
			// 
			// UpdateEntryWithResultsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UpdateEntryWithResultsCheckBox, "UpdateEntryWithResults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.DataRegistry.Business.AutoQueryBillOfLadingCargoManifestStatus)(null)).UpdateEntryWithResults)));
			this.UpdateEntryWithResultsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.UpdateEntryWithResultsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 47, true);
			this.UpdateEntryWithResultsCheckBox.Name = "UpdateEntryWithResultsCheckBox";
			this.UpdateEntryWithResultsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.UpdateEntryWithResultsCheckBox.TabIndex = 0;
			this.UpdateEntryWithResultsCheckBox.Text = "Update Entry with results";
			this.UpdateEntryWithResultsCheckBox.UseVisualStyleBackColor = true;
			// 
			// AutoQueryBillOfLadingCargoManifestStatusUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SendOnETAGroupBox);
			this.Controls.Add(this.SendOnFirstSaveGroupBox);
			this.Name = "AutoQueryBillOfLadingCargoManifestStatusUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 145, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SendOnETAGroupBox.ResumeLayout(false);
			this.SendOnETAGroupBox.PerformLayout();
			this.SendOnFirstSaveGroupBox.ResumeLayout(false);
			this.SendOnFirstSaveGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}


		internal Enterprise.ZArchitecture.GUI.ZGroupBox SendOnETAGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox SendOnFirstSaveGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox SendBasedOnETACheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox SendOnFirstSaveCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox UpdateEntryWithResultsCheckBox;

		#endregion
	}
}

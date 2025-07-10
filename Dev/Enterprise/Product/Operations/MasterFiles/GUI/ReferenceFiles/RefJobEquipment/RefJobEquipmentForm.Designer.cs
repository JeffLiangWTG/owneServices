using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.GUI
{
	partial class RefJobEquipmentForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.JEQ_IsActiveCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.JEQ_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JEQ_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 178, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.JEQ_IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.JEQ_DescriptionTextBox);
			this.MainTabPage.Controls.Add(this.JEQ_CodeTextBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 151, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 151, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 178, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.JobEquipment);
			// 
			// JEQ_IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.JEQ_IsActiveCheckBox, "JEQ_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.JobEquipment)(null)).JEQ_IsActive)));
			this.JEQ_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 61, true);
			this.JEQ_IsActiveCheckBox.Name = "JEQ_IsActiveCheckBox";
			this.JEQ_IsActiveCheckBox.CaptionResourceString = Res.GetData("9594a4ba-af32-4645-abe4-81973dd95d0f", "Active", "Is Active", "Identifies whether this equipment combination is active.");
			this.JEQ_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 20, true);
			this.JEQ_IsActiveCheckBox.UseVisualStyleBackColor = true;
			this.JEQ_IsActiveCheckBox.TabIndex = 3;
			// 
			// JEQ_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.JEQ_DescriptionTextBox, "JEQ_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobEquipment)(null)).JEQ_Description)));
			this.JEQ_DescriptionTextBox.CaptionResourceString = Res.GetData("6738f31b-7639-4dea-a064-eecc83900138", "Desc.", "Description", "Description of the equipment combination.");
			this.JEQ_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 35, true);
			this.JEQ_DescriptionTextBox.Name = "JEQ_DescriptionTextBox";
			this.JEQ_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.JEQ_DescriptionTextBox.TabIndex = 2;
			// 
			// JEQ_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.JEQ_CodeTextBox, "JEQ_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobEquipment)(null)).JEQ_Code)));
			this.JEQ_CodeTextBox.CaptionResourceString = Res.GetData("2238174b-e33e-473a-8888-6c0a1db19e54", "Code" , "Code", "Equipment combination code");
			this.JEQ_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 9, true);
			this.JEQ_CodeTextBox.Name = "JEQ_CodeTextBox";
			this.JEQ_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.JEQ_CodeTextBox.TabIndex = 1;
			// 
			// EquipmentCombinationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Res.GetData("5f17293b-d68b-4c5e-96e6-496e3b1649f1", "Equipment combination");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 234, true);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.JobEquipment);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "EquipmentCombinationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "EquipmentCombinationForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox JEQ_IsActiveCheckBox;
		private ZArchitecture.ZTextBox JEQ_DescriptionTextBox;
		private ZArchitecture.ZTextBox JEQ_CodeTextBox;
	}
}

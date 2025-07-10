namespace Enterprise.Freight.Forwarding.GUI
{
	partial class TemperatureControlBlock
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
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.TemperatureControlBlockCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TemperatureControlMinTempDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TemperatureControlMaxTempDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TemperatureControlMinButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TemperatureControlMaxButton = new Enterprise.ZArchitecture.GUI.ZButton();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();

			//
			// Binding Source
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.IRequiredTemperature);
			//// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.IRequiredTemperature)(null)).RequiredTemperatureMinimum));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.IRequiredTemperature)(null)).RequiredTemperatureMaximum));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)(((Enterprise.Freight.Business.IRequiredTemperature)(null)).RequiredTemperatureUnit));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.IRequiredTemperature)(null)).RequiresTemperatureControl));

			this.BindingSource.SetBindingMember(this.TemperatureControlBlockCheckBox, "RequiresTemperatureControl");
			this.TemperatureControlBlockCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemperatureControlBlockCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("TemperatureControlBlock|99990d2e-ef30-80ba-4f96-abcbf06a20f7", "Is Temperature Controlled");
			this.TemperatureControlBlockCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TemperatureControlBlockCheckBox.Name = "TemperatureControlBlockCheckBox";
			this.TemperatureControlBlockCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.TemperatureControlBlockCheckBox.TabIndex = 3;
			//
			// TemperatureControlMinTempDropEdit
			//
			this.BindingSource.SetBindingMember(this.TemperatureControlMinTempDropEdit, ".");
			this.TemperatureControlMinTempDropEdit.BindToAmount = "RequiredTemperatureMinimum";
			this.TemperatureControlMinTempDropEdit.BindToUnit = "RequiredTemperatureUnit";
			this.TemperatureControlMinTempDropEdit.Decimals = 1;
			this.TemperatureControlMinTempDropEdit.Name = "temperatureControlMinTempDropEdit";
			this.TemperatureControlMinTempDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.TemperatureControlMinTempDropEdit.TabIndex = 3;
			this.TemperatureControlMinTempDropEdit.UnitPreBoundMaxLength = 1;
			this.TemperatureControlMinTempDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("TemperatureControlBlock|fc08c081-b5e8-7693-4014-774e7d923636", "Min Temp.");
			//
			// TemperatureControlMaxTempDropEdit
			//
			this.BindingSource.SetBindingMember(this.TemperatureControlMaxTempDropEdit, ".");
			this.TemperatureControlMaxTempDropEdit.BindToAmount = "RequiredTemperatureMaximum";
			this.TemperatureControlMaxTempDropEdit.BindToUnit = "RequiredTemperatureUnit";
			this.TemperatureControlMaxTempDropEdit.Decimals = 1;
			this.TemperatureControlMaxTempDropEdit.Name = "temperatureControlMaxTempDropEdit";
			this.TemperatureControlMaxTempDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.TemperatureControlMaxTempDropEdit.TabIndex = 3;
			this.TemperatureControlMaxTempDropEdit.UnitPreBoundMaxLength = 1;
			this.TemperatureControlMaxTempDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("TemperatureControlBlock|7ed4fbee-8743-e1a9-409d-964c12defb74", "Max Temp.");

			// 
			// TemperatureControlMinButton
			// 
			this.TemperatureControlMinButton.Name = "TemperatureControlMinButton";
			this.TemperatureControlMinButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.TemperatureControlMinButton.Enabled = false;
			this.TemperatureControlMinButton.TabIndex = 30;
			this.TemperatureControlMinButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("TemperatureControlBlock|cb762390-7730-b0bd-4844-ab805343a584", "Set To Min", "Set To Minimum");
			this.TemperatureControlMinButton.Click += new System.EventHandler(this.TemperatureControlMinButton_Click);
			// 
			// TemperatureControlMaxButton
			// 
			this.TemperatureControlMaxButton.Name = "TemperatureControlMaxButton";
			this.TemperatureControlMaxButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.TemperatureControlMaxButton.Enabled = false;
			this.TemperatureControlMaxButton.TabIndex = 30;
			this.TemperatureControlMaxButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("TemperatureControlBlock|49205f4c-8e40-7290-4a4f-c6cef41ed388", "Set To Max", "Set To Maximum");
			this.TemperatureControlMaxButton.Click += new System.EventHandler(this.TemperatureControlMaxButton_Click);

			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TemperatureControlBlockCheckBox);
			this.Controls.Add(this.TemperatureControlMinTempDropEdit);
			this.Controls.Add(this.TemperatureControlMaxTempDropEdit);
			this.Controls.Add(this.TemperatureControlMinButton);
			this.Controls.Add(this.TemperatureControlMaxButton);
			this.Name = "TemperatureControlBlock";
			this.CaptionRenderingEnabled = true;

			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 70, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCheckBox TemperatureControlBlockCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit TemperatureControlMinTempDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit TemperatureControlMaxTempDropEdit;
		private Enterprise.ZArchitecture.GUI.ZButton TemperatureControlMinButton;
		private Enterprise.ZArchitecture.GUI.ZButton TemperatureControlMaxButton;
	}
}

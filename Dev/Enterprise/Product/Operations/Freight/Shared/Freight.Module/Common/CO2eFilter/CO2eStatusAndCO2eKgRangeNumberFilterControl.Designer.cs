namespace Enterprise.Freight.Module
{
	partial class CO2eStatusAndCO2eKgRangeNumberFilterControl
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
			this.CO2eStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CO2eStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CO2eStatusAndCO2eKgRangeNumberFilter);
			// 
			// CO2eStatusDropEdit
			// 
			this.CO2eStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CO2eStatusDropEdit, "CO2eStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Module.CO2eStatusAndCO2eKgRangeNumberFilter)(null)).CO2eStatus)));
			this.CO2eStatusDropEdit.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("ff65c5e7-f5ea-49b3-bb53-1f83ac48a9e0", "CO2e Status");
			this.CO2eStatusDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CO2eStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 24, true);
			this.CO2eStatusDropEdit.Name = "CO2eStatusDropEdit";
			this.CO2eStatusDropEdit.PreBoundMaxLength = 3;
			this.CO2eStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 17, true);
			this.CO2eStatusDropEdit.TabIndex = 4;
			// 
			// CO2eStatusAndCO2eKgRangeNumberFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CO2eStatusDropEdit);
			this.Name = "CO2eStatusAndCO2eKgRangeNumberFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 44, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CO2eStatusDropEdit.ResumeLayout(true);
			this.CO2eStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZDropEdit CO2eStatusDropEdit;
		#endregion
	}
}

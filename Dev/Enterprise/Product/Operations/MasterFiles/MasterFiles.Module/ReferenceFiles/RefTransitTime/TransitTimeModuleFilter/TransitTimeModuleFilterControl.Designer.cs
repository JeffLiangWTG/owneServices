namespace Enterprise.MasterFiles.Module
{
	partial class TransitTimeModuleFilterControl
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
			this.comparisonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.daysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.hoursCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.comparisonDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.TransitTimeModuleFilter);
			// 
			// comparisonDropEdit
			// 
			this.comparisonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.comparisonDropEdit, "ComparisonOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Module.TransitTimeModuleFilter)(null)).ComparisonOperator)));
			this.comparisonDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.comparisonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 1, true);
			this.comparisonDropEdit.Name = "comparisonDropEdit";
			this.comparisonDropEdit.ShowDescriptionBox = false;
			this.comparisonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.comparisonDropEdit.TabIndex = 0;
			// 
			// daysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.daysCalcEdit, "TransitDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Module.TransitTimeModuleFilter)(null)).TransitDays)));
			this.daysCalcEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("606eec85-f715-4304-ab8a-8945327b473b", "Days");
			this.daysCalcEdit.DecimalPlaces = 0;
			this.daysCalcEdit.Decimals = 0;
			this.daysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 1, true);
			this.daysCalcEdit.Name = "daysCalcEdit";
			this.daysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 18, true);
			this.daysCalcEdit.TabIndex = 1;
			this.daysCalcEdit.Text = "0";
			this.daysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// hoursCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.hoursCalcEdit, "TransitHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Module.TransitTimeModuleFilter)(null)).TransitHours)));
			this.hoursCalcEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("abe3f114-d2dd-42a9-8773-08f8b7eafa90", "Hours");
			this.hoursCalcEdit.DecimalPlaces = 0;
			this.hoursCalcEdit.Decimals = 0;
			this.hoursCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 0, true);
			this.hoursCalcEdit.Name = "hoursCalcEdit";
			this.hoursCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 18, true);
			this.hoursCalcEdit.TabIndex = 2;
			this.hoursCalcEdit.Text = "0";
			this.hoursCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TransitTimeModuleFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.hoursCalcEdit);
			this.Controls.Add(this.daysCalcEdit);
			this.Controls.Add(this.comparisonDropEdit);
			this.Name = "TransitTimeModuleFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.comparisonDropEdit.ResumeLayout(true);
			this.comparisonDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZDropEdit comparisonDropEdit;
		private ZArchitecture.ZCalcEdit daysCalcEdit;
		private ZArchitecture.ZCalcEdit hoursCalcEdit;
	}
}

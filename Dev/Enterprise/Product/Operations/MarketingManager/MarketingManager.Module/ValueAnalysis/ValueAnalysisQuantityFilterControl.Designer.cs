using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Module
{
	partial class ValueAnalysisQuantityFilterControl
	{
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.Period = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Period.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Module.ValueAnalysisQuantityFilter);
			// 
			// Period
			// 
			this.Period.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Period, "PeriodDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Module.ValueAnalysisQuantityFilter)(null)).PeriodDescription)));
			this.Period.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Period.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 27, true);
			this.Period.Name = "Period";
			this.Period.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 17, true);
			this.Period.TabIndex = 4;
			// 
			// ValueAnalysisQuantityFilterControl
			// 
			this.Controls.Add(this.Period);
			this.Name = "ValueAnalysisQuantityFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 100, true);
			this.Controls.SetChildIndex(this.Period, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Period.ResumeLayout(true);
			this.Period.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit Period;
	}
}

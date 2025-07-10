namespace Enterprise.Customs.Module
{
	partial class ZPeriodUserControl
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
			this.OperatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PeriodYearEdit = new Enterprise.ZArchitecture.GUI.ZYearEdit();
			this.PeriodLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PeriodMonthEdit = new Enterprise.Customs.Module.MonthEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OperatorDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Module.PeriodFilter);
			// 
			// OperatorDropEdit
			// 
			this.OperatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OperatorDropEdit, "ComparisonOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Module.PeriodFilter)(null)).ComparisonOperator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Module.PeriodFilter)(null)).ComparisonOperator_List)));
			this.OperatorDropEdit.BindToList = "ComparisonOperator_List";
			this.OperatorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.OperatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OperatorDropEdit.Name = "OperatorDropEdit";
			this.OperatorDropEdit.PreBoundMaxLength = 7;
			this.OperatorDropEdit.ShouldResizeByMaxLength = true;
			this.OperatorDropEdit.ShowDescriptionBox = false;
			this.OperatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.OperatorDropEdit.TabIndex = 0;
			// 
			// PeriodYearEdit
			// 
			this.PeriodYearEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodYearEdit, "PeriodYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Module.PeriodFilter)(null)).PeriodYear)));
			this.PeriodYearEdit.CaptionResourceString = null;
			this.PeriodYearEdit.DecimalPlaces = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PeriodYearEdit, false);
			this.PeriodYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 0, true);
			this.PeriodYearEdit.Name = "PeriodYearEdit";
			this.PeriodYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.PeriodYearEdit.TabIndex = 4;
			this.PeriodYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PeriodLabel
			// 
			this.PeriodLabel.AutoSize = true;
			this.PeriodLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PeriodLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 3, true);
			this.PeriodLabel.Name = "PeriodLabel";
			this.PeriodLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.PeriodLabel.TabIndex = 3;
			this.PeriodLabel.Text = "/";
			// 
			// PeriodMonthEdit
			// 
			this.PeriodMonthEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodMonthEdit, "PeriodMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Module.PeriodFilter)(null)).PeriodMonth)));
			this.PeriodMonthEdit.CaptionResourceString = null;
			this.PeriodMonthEdit.DecimalPlaces = 0;
			this.PeriodMonthEdit.Decimals = 0;
			this.PeriodMonthEdit.IsCalculatorEnabled = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PeriodMonthEdit, false);
			this.PeriodMonthEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 0, true);
			this.PeriodMonthEdit.Name = "PeriodMonthEdit";
			this.PeriodMonthEdit.ShowGroupSeparators = false;
			this.PeriodMonthEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.PeriodMonthEdit.TabIndex = 2;
			this.PeriodMonthEdit.Text = "00";
			this.PeriodMonthEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZPeriodUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OperatorDropEdit);
			this.Controls.Add(this.PeriodMonthEdit);
			this.Controls.Add(this.PeriodYearEdit);
			this.Controls.Add(this.PeriodLabel);
			this.Name = "ZPeriodUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OperatorDropEdit.ResumeLayout(true);
			this.OperatorDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZDropEdit OperatorDropEdit;
		private MonthEdit PeriodMonthEdit;
		private ZArchitecture.ZLabel PeriodLabel;
		private ZArchitecture.GUI.ZYearEdit PeriodYearEdit;
	}
}

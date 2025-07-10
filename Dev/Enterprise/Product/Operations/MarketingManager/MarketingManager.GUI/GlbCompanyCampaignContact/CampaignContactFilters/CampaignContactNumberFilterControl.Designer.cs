namespace Enterprise.MarketingManager.GUI
{
	partial class CampaignContactNumberFilterControl
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
			this.ComparisonOperatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NumberEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.GUI.CampaignContactNumberFilter);
			// 
			// ComparisonOperatorDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ComparisonOperatorDropEdit, "ComparisonOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.GUI.CampaignContactNumberFilter)(null)).ComparisonOperator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.GUI.CampaignContactNumberFilter)(null)).ComparisonOperator_List)));
			this.ComparisonOperatorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ComparisonOperatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 1, true);
			this.ComparisonOperatorDropEdit.Name = "ComparisonOperatorDropEdit";
			this.ComparisonOperatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.ComparisonOperatorDropEdit.TabIndex = 0;
			// 
			// AmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberEdit, "Property");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.GUI.CampaignContactNumberFilter)(null)).Property)));
			this.NumberEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 1, true);
			this.NumberEdit.Name = "NumberEdit";
			this.NumberEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.NumberEdit.DecimalPlaces = 0;
			this.NumberEdit.TabIndex = 1;
			this.NumberEdit.Text = "0";
			this.NumberEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CampaignContactNumberFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ComparisonOperatorDropEdit);
			this.Controls.Add(this.NumberEdit);
			this.Name = "CampaignContactNumberFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit ComparisonOperatorDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit NumberEdit;
	}
}

namespace Enterprise.Customs.NL.NCTS.GUI
{
	partial class DeclarationDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FallbackProcedureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FallbackUserControl = new DeclarationDetailsFallbackUserControl();
			this.FallbackUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// FallbackProcedureCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FallbackProcedureCheckBox, "IsFallbackProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NL.NCTS.Business.NctsDepartureMovementHeader)(null)).IsFallbackProcedure)));
			this.FallbackProcedureCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.FallbackProcedureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.FallbackProcedureCheckBox.Name = "FallbackProcedureCheckBox";
			this.FallbackProcedureCheckBox.TabIndex = 6;
			this.FallbackProcedureCheckBox.UseVisualStyleBackColor = true;
			// 
			// FallbackUserControl
			// 
			this.FallbackUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FallbackUserControl, ".");
			this.FallbackUserControl.Name = "FallbackUserControl";
			this.FallbackUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.FallbackUserControl.TabIndex = 1;
			// 
			// DeclarationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FallbackProcedureCheckBox);
			this.Controls.Add(this.FallbackUserControl);
			this.FallbackUserControl.ResumeLayout(true);
			this.FallbackUserControl.PerformLayout();
			this.Name = "DeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 186, true);
			this.Tag = "";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox FallbackProcedureCheckBox;
		internal DeclarationDetailsFallbackUserControl FallbackUserControl;
	}
}

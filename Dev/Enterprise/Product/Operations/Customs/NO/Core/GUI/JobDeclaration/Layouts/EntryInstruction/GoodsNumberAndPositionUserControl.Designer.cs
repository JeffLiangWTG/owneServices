namespace Enterprise.Customs.NO.GUI
{
	sealed partial class GoodsNumberAndPositionUserControl
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
			this.CEI_GoodsNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CEI_PositionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CEI_SubPositionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.CusEntryInstruction);
			// 
			// CEI_GoodsNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CEI_GoodsNumberTextBox, "CEI_GoodsNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryInstruction)(null)).CEI_GoodsNumber)));
			this.CEI_GoodsNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CEI_GoodsNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CEI_GoodsNumberTextBox.Name = "CEI_GoodsNumberTextBox";
			this.CEI_GoodsNumberTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.CEI_GoodsNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.CEI_GoodsNumberTextBox.TabIndex = 1;
			// 
			// CEI_PositionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CEI_PositionNumberTextBox, "CEI_Position");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryInstruction)(null)).CEI_Position)));
			this.CEI_PositionNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CEI_PositionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 0, true);
			this.CEI_PositionNumberTextBox.Name = "CEI_PositionNumberTextBox";
			this.CEI_PositionNumberTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.CEI_PositionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.CEI_PositionNumberTextBox.TabIndex = 2;
			// 
			// CEI_SubPositionNumberTextBox|
			// 
			this.BindingSource.SetBindingMember(this.CEI_SubPositionNumberTextBox, "CEI_SubPosition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryInstruction)(null)).CEI_SubPosition)));
			this.CEI_SubPositionNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CEI_SubPositionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 0, true);
			this.CEI_SubPositionNumberTextBox.Name = "CEI_SubPositionNumberTextBox";
			this.CEI_SubPositionNumberTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.CEI_SubPositionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CEI_SubPositionNumberTextBox.TabIndex = 3;
			// 
			// GoodsNumberAndPositionUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CEI_GoodsNumberTextBox);
			this.Controls.Add(this.CEI_PositionNumberTextBox);
			this.Controls.Add(this.CEI_SubPositionNumberTextBox);
			this.Name = "GoodsNumberAndPositionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.ZTextBox CEI_GoodsNumberTextBox;
		ZArchitecture.ZTextBox CEI_PositionNumberTextBox;
		ZArchitecture.ZTextBox CEI_SubPositionNumberTextBox;
	}
}

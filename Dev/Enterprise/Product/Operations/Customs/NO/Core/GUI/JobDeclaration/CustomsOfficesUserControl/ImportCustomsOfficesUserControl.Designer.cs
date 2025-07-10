namespace Enterprise.Customs.NO.GUI
{
	partial class ImportCustomsOfficesUserControl
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
			this.CustomsDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GoodsNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PositionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
			// 
			// CustomsDetailsGroupBox
			// 
			this.CustomsDetailsGroupBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("ffe0721e-fef9-41c0-a20c-de5beee17601", "Customs Details");
			this.CustomsDetailsGroupBox.Controls.Add(this.GoodsNumberTextBox);
			this.CustomsDetailsGroupBox.Controls.Add(this.PositionNumberTextBox);
			this.CustomsDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsDetailsGroupBox.Name = "CustomsDetailsGroupBox";
			this.CustomsDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 46, true);
			this.CustomsDetailsGroupBox.TabIndex = 0;
			this.CustomsDetailsGroupBox.TabStop = false;
			// 
			// GoodsNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsNumberTextBox, "JE_GoodsNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_GoodsNumber)));
			this.GoodsNumberTextBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("7cee5cd6-e153-438d-8c32-4a77344b8258", "Goods Number");
			this.GoodsNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GoodsNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 17, true);
			this.GoodsNumberTextBox.Name = "GoodsNumberTextBox";
			this.GoodsNumberTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.GoodsNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 38, true);
			this.GoodsNumberTextBox.TabIndex = 1;
			// 
			// PositionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PositionNumberTextBox, "JE_Position");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_Position)));
			this.PositionNumberTextBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("a699873f-dd05-4db7-b0dd-20f38e5702f7", "Position");
			this.PositionNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PositionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 17, true);
			this.PositionNumberTextBox.Name = "PositionNumberTextBox";
			this.PositionNumberTextBox.ShouldEscapeAllSpecialCharacters = true;
			this.PositionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 38, true);
			this.PositionNumberTextBox.TabIndex = 2;
			// 
			// ImportCustomsOfficesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsDetailsGroupBox);
			this.Name = "ImportCustomsOfficesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 46, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsDetailsGroupBox.ResumeLayout(false);
			this.CustomsDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CustomsDetailsGroupBox;
		private ZArchitecture.ZTextBox PositionNumberTextBox;
		private ZArchitecture.ZTextBox GoodsNumberTextBox;
	}
}

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CFXUserControl
	{
		private ZGroupBox CFXGroupBox;
		private ZArchitecture.ZCalcEdit CFXSeaImportCalcEdit1;
		private ZArchitecture.ZLabel CFXAirImportPercentLabel;
		private ZArchitecture.ZLabel CFXSeaImportPercentLabel;
		private ZArchitecture.ZCalcEdit CFXAirImportCalcEdit;
		private ZArchitecture.ZLabel CFXAirExportPercentLabel;
		private ZArchitecture.ZLabel CFXSeaExportPercentLabel;
		private ZArchitecture.ZCalcEdit CFXSeaExportCalcEdit;
		private ZArchitecture.ZCalcEdit CFXAirExportCalcEdit;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.CFXGroupBox = new ZGroupBox();
			this.CFXSeaImportCalcEdit1 = new ZArchitecture.ZCalcEdit();
			this.CFXAirImportPercentLabel = new ZArchitecture.ZLabel();
			this.CFXSeaImportPercentLabel = new ZArchitecture.ZLabel();
			this.CFXAirImportCalcEdit = new ZArchitecture.ZCalcEdit();
			this.CFXAirExportPercentLabel = new ZArchitecture.ZLabel();
			this.CFXSeaExportPercentLabel = new ZArchitecture.ZLabel();
			this.CFXSeaExportCalcEdit = new ZArchitecture.ZCalcEdit();
			this.CFXAirExportCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CFXGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.RatingHeader);
			// 
			// CFXGroupBox
			// 
			this.CFXGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CFXUserControl|ede9d25b-5f56-4a2f-8fde-f5ebe8c796ae", "CFX", "Currency Uplift Information for this quotation.");
			this.CFXGroupBox.Controls.Add(this.CFXSeaImportCalcEdit1);
			this.CFXGroupBox.Controls.Add(this.CFXAirImportPercentLabel);
			this.CFXGroupBox.Controls.Add(this.CFXSeaImportPercentLabel);
			this.CFXGroupBox.Controls.Add(this.CFXAirImportCalcEdit);
			this.CFXGroupBox.Controls.Add(this.CFXAirExportPercentLabel);
			this.CFXGroupBox.Controls.Add(this.CFXSeaExportPercentLabel);
			this.CFXGroupBox.Controls.Add(this.CFXSeaExportCalcEdit);
			this.CFXGroupBox.Controls.Add(this.CFXAirExportCalcEdit);
			this.CFXGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CFXGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CFXGroupBox.Name = "CFXGroupBox";
			this.CFXGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 58, true);
			this.CFXGroupBox.TabIndex = 12;
			this.CFXGroupBox.TabStop = false;
			// 
			// CFXSeaImportCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.CFXSeaImportCalcEdit1, "TH_SeaCFX");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RatingHeader)(null)).TH_SeaCFX);
			this.CFXSeaImportCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 14, true);
			this.CFXSeaImportCalcEdit1.DecimalPlaces = 2;
			this.CFXSeaImportCalcEdit1.Name = "CFXSeaImportCalcEdit1";
			this.CFXSeaImportCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CFXSeaImportCalcEdit1.TabIndex = 19;
			this.CFXSeaImportCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CFXAirImportPercentLabel
			// 
			this.CFXAirImportPercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 14, true);
			this.CFXAirImportPercentLabel.Name = "CFXAirImportPercentLabel";
			this.CFXAirImportPercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 20, true);
			this.CFXAirImportPercentLabel.TabIndex = 14;
			this.CFXAirImportPercentLabel.Text = "%";
			// 
			// CFXSeaImportPercentLabel
			// 
			this.CFXSeaImportPercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 14, true);
			this.CFXSeaImportPercentLabel.Name = "CFXSeaImportPercentLabel";
			this.CFXSeaImportPercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 20, true);
			this.CFXSeaImportPercentLabel.TabIndex = 20;
			this.CFXSeaImportPercentLabel.Text = "%";
			// 
			// CFXAirImportCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CFXAirImportCalcEdit, "TH_AirCFX");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RatingHeader)(null)).TH_AirCFX);
			this.CFXAirImportCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 14, true);
			this.CFXAirImportCalcEdit.DecimalPlaces = 2;
			this.CFXAirImportCalcEdit.Name = "CFXAirImportCalcEdit";
			this.CFXAirImportCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CFXAirImportCalcEdit.TabIndex = 13;
			this.CFXAirImportCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CFXAirExportPercentLabel
			// 
			this.CFXAirExportPercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 34, true);
			this.CFXAirExportPercentLabel.Name = "CFXAirExportPercentLabel";
			this.CFXAirExportPercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 20, true);
			this.CFXAirExportPercentLabel.TabIndex = 17;
			this.CFXAirExportPercentLabel.Text = "%";
			// 
			// CFXSeaExportPercentLabel
			// 
			this.CFXSeaExportPercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 34, true);
			this.CFXSeaExportPercentLabel.Name = "CFXSeaExportPercentLabel";
			this.CFXSeaExportPercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 20, true);
			this.CFXSeaExportPercentLabel.TabIndex = 23;
			this.CFXSeaExportPercentLabel.Text = "%";
			// 
			// CFXSeaExportCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CFXSeaExportCalcEdit, "TH_ExportSeaCFX");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RatingHeader)(null)).TH_ExportSeaCFX);
			this.CFXSeaExportCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 34, true);
			this.CFXSeaExportCalcEdit.DecimalPlaces = 2;
			this.CFXSeaExportCalcEdit.Name = "CFXSeaExportCalcEdit";
			this.CFXSeaExportCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CFXSeaExportCalcEdit.TabIndex = 22;
			this.CFXSeaExportCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CFXAirExportCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CFXAirExportCalcEdit, "TH_ExportAirCFX");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RatingHeader)(null)).TH_ExportAirCFX);
			this.CFXAirExportCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 34, true);
			this.CFXAirExportCalcEdit.DecimalPlaces = 2;
			this.CFXAirExportCalcEdit.Name = "CFXAirExportCalcEdit";
			this.CFXAirExportCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CFXAirExportCalcEdit.TabIndex = 16;
			this.CFXAirExportCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CFXUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CFXGroupBox);
			this.Name = "CFXUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 58, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CFXGroupBox.ResumeLayout(false);
			this.CFXGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}

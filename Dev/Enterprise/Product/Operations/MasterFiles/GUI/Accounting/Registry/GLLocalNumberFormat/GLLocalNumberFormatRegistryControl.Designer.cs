namespace Enterprise.MasterFiles.GUI
{
	public partial class GLLocalNumberFormatRegistryControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.GLLocalNumberFormatGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GLLocalNumberFormatGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GLLocalNumberFormatCollection);
			// 
			// GLLocalNumberFormatGrid
			// 
			this.GLLocalNumberFormatGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GLLocalNumberFormatGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GLLocalNumberFormat)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GLLocalNumberFormat)(null)).Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GLLocalNumberFormat)(null)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GLLocalNumberFormat)(null)).NumberFormat)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GLLocalNumberFormat)(null)).IsFixedLength)));
			this.GLLocalNumberFormatGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("75302e8a-ca00-42ea-ac5f-9f2906cd14f2", "Language");
			zDropEditColumnStyleInfo1.ColumnName = "Language";
			zCodeFindBoxColumnStyleInfo1.Caption = "";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2dfa6518-4708-4936-a19d-5c92f0a98846", "Country/Region");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CountryCode";
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("36e58fb8-2b02-427e-94f9-2703dc8d284d", "Local GL Account Format Rule");
			zTextBoxColumnStyleInfo1.ColumnName = "NumberFormat";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
			zCheckBoxColumnStyleInfo1.Caption = "";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9d499a00-38b1-4fe5-8f0f-83389729b5f9", "Fixed Length");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsFixedLength";
			this.GLLocalNumberFormatGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GLLocalNumberFormatGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.GLLocalNumberFormatGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GLLocalNumberFormatGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.GLLocalNumberFormatGrid.CopySelectedRowsAllowed = true;
			this.GLLocalNumberFormatGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GLLocalNumberFormatGrid.GridId = "d3365d9e-b23c-4c73-9f5a-098198f272ea";
			this.GLLocalNumberFormatGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GLLocalNumberFormatGrid.LayoutKey = "GLLocalNumberFormatGrid";
			this.GLLocalNumberFormatGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GLLocalNumberFormatGrid.Name = "GLLocalNumberFormatGrid";
			this.GLLocalNumberFormatGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 291, true);
			this.GLLocalNumberFormatGrid.TabIndex = 0;
			// 
			// GLLocalNumberFormatRegistryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GLLocalNumberFormatGrid);
			this.Name = "GLLocalNumberFormatRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 291, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GLLocalNumberFormatGrid)).EndInit();
			this.ResumeLayout(false);
		}

		internal Enterprise.ZArchitecture.ZGrid GLLocalNumberFormatGrid;
	}
}

namespace Enterprise.MasterFiles.GUI
{
	public partial class RevenueRecognitionControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.RevenueRecognitionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RevenueRecognitionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RevenueRecognitionCollection);
			// 
			// RevenueRecognitionGrid
			// 
			this.RevenueRecognitionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RevenueRecognitionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RevenueRecognition)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RevenueRecognition)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RevenueRecognition)(null)).DirectionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RevenueRecognition)(null)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RevenueRecognition)(null)).BrokerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RevenueRecognition)(null)).RecognitionDateOptionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RevenueRecognition)(null)).RecognitionDateOptionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RevenueRecognition)(null)).Offset)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RevenueRecognition)(null)).OffsetType)));
			this.RevenueRecognitionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RevenueRecognitionControl|febe2667-d543-4b0f-9e56-6fa9ff3cbf38", "Job Type");
			zDropEditColumnStyleInfo1.ColumnName = "JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RevenueRecognitionControl|61a2c64e-4181-40b8-b400-695a35f9d62e", "Direction");
			zDropEditColumnStyleInfo2.ColumnName = "DirectionCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RevenueRecognitionControl|bdfcedf7-41d5-4bbe-a736-068a63d093f7", "Mode");
			zDropEditColumnStyleInfo3.ColumnName = "Mode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RevenueRecognitionControl|fee2e6ff-1bea-492b-8bd9-a0a56fff2d9a", "Broker");
			zDropEditColumnStyleInfo4.ColumnName = "BrokerCode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RevenueRecognitionControl|2cdcc617-4972-46ec-bbb7-ab9dfc258ead", "Recognition Date Option");
			zDropEditColumnStyleInfo5.ColumnName = "RecognitionDateOptionCode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RevenueRecognitionControl|f17c099d-f112-4cba-8a67-5959892d2226", "Rec Description", "Rec Date Description", "Recognition Date Description", "");
			zTextBoxColumnStyleInfo1.ColumnName = "RecognitionDateOptionDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RevenueRecognitionControl|5da9bed4-a463-4836-bb91-f5380a941b56", "Offset");
			zCalcEditColumnStyleInfo1.ColumnName = "Offset";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RevenueRecognitionControl|13077cb6-5bdf-4028-924a-486f76ab06b5", "Offset Type");
			zDropEditColumnStyleInfo6.ColumnName = "OffsetType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.RevenueRecognitionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RevenueRecognitionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RevenueRecognitionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RevenueRecognitionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.RevenueRecognitionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.RevenueRecognitionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RevenueRecognitionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RevenueRecognitionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.RevenueRecognitionGrid.GridId = "A9035049-0829-49EF-B6F3-FE788551B93F";
			this.RevenueRecognitionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RevenueRecognitionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RevenueRecognitionGrid.LayoutKey = "RevenueRecognitionGrid";
			this.RevenueRecognitionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RevenueRecognitionGrid.Name = "RevenueRecognitionGrid";
			this.RevenueRecognitionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			this.RevenueRecognitionGrid.TabIndex = 0;
			// 
			// RevenueRecognitionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RevenueRecognitionGrid);
			this.Name = "RevenueRecognitionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RevenueRecognitionGrid)).EndInit();
			this.ResumeLayout(false);
		}

		internal Enterprise.ZArchitecture.ZGrid RevenueRecognitionGrid;
	}
}

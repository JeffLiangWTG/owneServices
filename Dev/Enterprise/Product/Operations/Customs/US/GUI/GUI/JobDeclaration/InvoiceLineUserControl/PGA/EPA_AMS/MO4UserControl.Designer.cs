namespace Enterprise.Customs.US.GUI
{
	partial class MO4UserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.LinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MO4Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MO4Grid)).BeginInit();
			this.MO4Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AMSLineCollection);
			// 
			// LinesGroupBox
			// 
			this.LinesGroupBox.Controls.Add(this.MO4Grid);
			this.LinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesGroupBox.Name = "LinesGroupBox";
			this.LinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 286, true);
			this.LinesGroupBox.TabIndex = 0;
			this.LinesGroupBox.TabStop = false;
			// 
			// MO4Grid
			// 
			this.MO4Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MO4Grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_ProductNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)).AddInfoLookups.ProductNumberCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_NetWeightUQ)));
			this.MO4Grid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "AddInfoLookups.ProductNumberCodes";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9958f171-0ae2-4e25-8390-283ce8663c14", "Product Number");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "US_ProductNumber";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("da7a216f-13f6-4f4c-9974-6182fda9f05d", "Net Weight");
			zCalcEditColumnStyleInfo1.ColumnName = "US_NetWeight";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("ba812d34-692a-4a60-bbef-7509054524c3", "Net Weight");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("694fb0bd-e065-424a-af31-44c4a4392ed8", "UQ");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_NetWeightUQ";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("ba812d34-692a-4a60-bbef-7509054524c3", "Net Weight");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.MO4Grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MO4Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MO4Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MO4Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MO4Grid.GridId = "10a111d0-25fc-4f36-a510-1794d1c9cbfc";
			this.MO4Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MO4Grid.LayoutKey = "MO4Grid";
			this.MO4Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MO4Grid.Name = "MO4Grid";
			this.MO4Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 267, true);
			this.MO4Grid.TabIndex = 0;
			// 
			// MO4UserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LinesGroupBox);
			this.Name = "MO4UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LinesGroupBox.ResumeLayout(false);
			this.LinesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MO4Grid)).EndInit();
			this.MO4Grid.ResumeLayout(false);
			this.MO4Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGroupBox LinesGroupBox;
		public ZArchitecture.ZGrid MO4Grid;
	}
}

namespace Enterprise.Customs.DataRegistry.GUI
{
	partial class ASNRefreshOptionsControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ASNRefreshOptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ASNRefreshOptionsGrid)).BeginInit();
			this.ASNRefreshOptionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DataRegistry.Business.ASNRefreshOptionsConfig);
			// 
			// ASNRefreshOptionsGrid
			// 
			this.ASNRefreshOptionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ASNRefreshOptionsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.ASNRefreshOptionsConfig)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.ASNRefreshOptionsConfig)(null)).FieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.ASNRefreshOptionsConfig)(null)).FieldTypeDesc)));
			this.ASNRefreshOptionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("74ff8e62-eba3-4891-8038-33d22e10a8b5", "Field Type");
			zDropEditColumnStyleInfo1.ColumnName = "FieldType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3493e4f2-c064-42ad-86a1-b9596a0c5a01", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "FieldTypeDesc";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ASNRefreshOptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ASNRefreshOptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ASNRefreshOptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ASNRefreshOptionsGrid.GridId = "ea1fd62b-c68f-4183-a926-6dd099d8c952";
			this.ASNRefreshOptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ASNRefreshOptionsGrid.LayoutKey = "ASNRefreshOptionsGrid";
			this.ASNRefreshOptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ASNRefreshOptionsGrid.Name = "ASNRefreshOptionsGrid";
			this.ASNRefreshOptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 150, true);
			this.ASNRefreshOptionsGrid.TabIndex = 1;
			// 
			// ASNRefreshOptionsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ASNRefreshOptionsGrid);
			this.Name = "ASNRefreshOptionsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ASNRefreshOptionsGrid)).EndInit();
			this.ASNRefreshOptionsGrid.ResumeLayout(false);
			this.ASNRefreshOptionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ASNRefreshOptionsGrid;
	}
}

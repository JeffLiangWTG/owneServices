namespace Enterprise.MasterFiles.GUI
{
	public partial class TaxDateDefaultingOptionControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.TaxDateDefaultingOptionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxDateDefaultingOptionGrid)).BeginInit();
			this.TaxDateDefaultingOptionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TaxDateDefaultingOptionCollection);
			// 
			// TaxDateDefaultingOptionGrid
			// 
			this.TaxDateDefaultingOptionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxDateDefaultingOptionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxDateDefaultingOption)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxDateDefaultingOption)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxDateDefaultingOption)(null)).DirectionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxDateDefaultingOption)(null)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxDateDefaultingOption)(null)).TaxDateOption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxDateDefaultingOption)(null)).Ledger)));
			this.TaxDateDefaultingOptionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaxDateDefaultingOption|4D93E313-FC9D-4734-8DA3-B5580A2DE2F6", "Job Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaxDateDefaultingOption|10DBA0FB-87A7-4512-85EB-FB66AED39088", "Direction");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "DirectionCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaxDateDefaultingOption|87C30FE3-41A0-457C-A734-71FC5CF0AFB6", "Mode");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "Mode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaxDateDefaultingOption|92608004-E618-4455-A0D4-AED716FB5F0A", "Tax Date Option");
			zDropEditColumnStyleInfo4.ColumnName = "TaxDateOption";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaxDateDefaultingOption|D65D8AB7-122F-44FE-B997-A096B09FBBAE", "Ledger");
			zDropEditColumnStyleInfo5.ColumnName = "Ledger";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TaxDateDefaultingOptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TaxDateDefaultingOptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TaxDateDefaultingOptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.TaxDateDefaultingOptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TaxDateDefaultingOptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.TaxDateDefaultingOptionGrid.CopySelectedRowsAllowed = true;
			this.TaxDateDefaultingOptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxDateDefaultingOptionGrid.GridId = "3C570121-F368-457C-B66C-DC36BA4B1699";
			this.TaxDateDefaultingOptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxDateDefaultingOptionGrid.LayoutKey = "TaxDateDefaultingOptionGrid";
			this.TaxDateDefaultingOptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxDateDefaultingOptionGrid.Name = "TaxDateDefaultingOptionGrid";
			this.TaxDateDefaultingOptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			this.TaxDateDefaultingOptionGrid.TabIndex = 0;
			// 
			// TaxDateDefaultingOptionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TaxDateDefaultingOptionGrid);
			this.Name = "TaxDateDefaultingOptionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxDateDefaultingOptionGrid)).EndInit();
			this.TaxDateDefaultingOptionGrid.ResumeLayout(false);
			this.TaxDateDefaultingOptionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal Enterprise.ZArchitecture.ZGrid TaxDateDefaultingOptionGrid;
	}
}

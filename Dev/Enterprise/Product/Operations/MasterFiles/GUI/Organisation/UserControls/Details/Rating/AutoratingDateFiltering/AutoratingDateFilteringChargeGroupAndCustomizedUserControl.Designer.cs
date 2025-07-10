namespace Enterprise.MasterFiles.GUI
{
	public partial class AutoratingDateFilteringChargeGroupAndCustomizedUserControl
	{
		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ChargeGroupGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ChargeGroupSetupGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupSetupGrid)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ChargeGroupGrid
			// 
			this.ChargeGroupGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeGroupGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.Rating.RatingDateConfig)(null)))));
			this.ChargeGroupGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AutoratingDateFilteringChargeGroupAndCustomizedUserControl|1355BBC5-FF48-4C15-BB9C-556239FDA91D", "Charge Group");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeGroup";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AutoratingDateFilteringChargeGroupAndCustomizedUserControl|28AE84DD-85E2-427D-998A-0F8979134F36", "Charge Group Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeGroupDescription";
			this.ChargeGroupGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargeGroupGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargeGroupGrid.GridId = "fa37b0a7-da43-468c-bc30-7b0c0e32a42f";
			this.ChargeGroupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeGroupGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeGroupGrid.LayoutKey = "ChargeGroupGrid";
			this.ChargeGroupGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeGroupGrid.Name = "ChargeGroupGrid";
			this.ChargeGroupGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 161, true);
			this.ChargeGroupGrid.TabIndex = 1;
			this.ChargeGroupGrid.ReadOnly = true;
			// 
			// ChargeGroupSetupGrid
			// 
			this.ChargeGroupSetupGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeGroupSetupGrid, "ChargeGroupSettings");
			this.ChargeGroupSetupGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AutoratingDateFilteringChargeGroupAndCustomizedUserControl|5D5C24B1-C1EC-439E-85DC-CBC0993BDCD9", "Job Type");
			zDropEditColumnStyleInfo1.ColumnName = "JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AutoratingDateFilteringChargeGroupAndCustomizedUserControl|D345DA1B-A6EF-48BB-95EA-685F48E0BA56", "Direction");
			zDropEditColumnStyleInfo2.ColumnName = "DirectionCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AutoratingDateFilteringChargeGroupAndCustomizedUserControl|D359547D-D526-4B8A-9CBF-D3937928E216", "Transport Mode");
			zDropEditColumnStyleInfo3.ColumnName = "Mode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ChargeGroupSetupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChargeGroupSetupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ChargeGroupSetupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ChargeGroupSetupGrid.GridId = "796FC81F-3932-47DE-9491-CE6C17C0490F";
			this.ChargeGroupSetupGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeGroupSetupGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeGroupSetupGrid.LayoutKey = "ChargeGroupSetupGrid";
			this.ChargeGroupSetupGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeGroupSetupGrid.Name = "ChargeGroupSetupGrid";
			this.ChargeGroupSetupGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 157, true);
			this.ChargeGroupSetupGrid.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ChargeGroupGrid);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ChargeGroupSetupGrid);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 322, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(161);
			this.splitContainer1.TabIndex = 2;
			// 
			// ????
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ChargeGroupSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 322, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupSetupGrid)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

		protected Enterprise.ZArchitecture.ZGrid ChargeGroupSetupGrid;
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		Enterprise.ZArchitecture.ZGrid ChargeGroupGrid;
	}
}

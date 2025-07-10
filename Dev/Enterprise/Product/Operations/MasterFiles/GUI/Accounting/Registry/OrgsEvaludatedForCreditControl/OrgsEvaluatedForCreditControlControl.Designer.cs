namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgsEvaluatedForCreditControlControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.OrgsEvaluatedForCreditControlGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgsEvaluatedForCreditControlGrid)).BeginInit();
			this.OrgsEvaluatedForCreditControlGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgsEvaluatedForCreditControlCollection);
			// 
			// OrgsEvaluatedForCreditControlGrid
			// 
			this.OrgsEvaluatedForCreditControlGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrgsEvaluatedForCreditControlGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgsEvaluatedForCreditControl)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgsEvaluatedForCreditControl)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgsEvaluatedForCreditControl)(null)).DirectionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgsEvaluatedForCreditControl)(null)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgsEvaluatedForCreditControl)(null)).INCOTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgsEvaluatedForCreditControl)(null)).FreightPaymentTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgsEvaluatedForCreditControl)(null)).OrganizationType)));
			this.OrgsEvaluatedForCreditControlGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgsEvaluatedForCreditControl|febe2667-d543-4b0f-9e56-6fa9ff3cbf38", "Job Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgsEvaluatedForCreditControl|61a2c64e-4181-40b8-b400-695a35f9d62e", "Direction");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "DirectionCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgsEvaluatedForCreditControl|bdfcedf7-41d5-4bbe-a736-068a63d093f7", "Mode");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "Mode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9c1aeeac-321f-00ad-4105-1183ce420d59", "Incoterm");
			zDropEditColumnStyleInfo4.ColumnName = "INCOTerm";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("eac9d94e-32e9-444c-8d46-b8c0b18c5466", "Freight Payment Term");
			zDropEditColumnStyleInfo5.ColumnName = "FreightPaymentTerm";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgsEvaluatedForCreditControl|674b8b89-602e-44e8-93fe-1fa131b44426", "Organization Type");
			zDropEditColumnStyleInfo6.ColumnName = "OrganizationType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrgsEvaluatedForCreditControlGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrgsEvaluatedForCreditControlGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OrgsEvaluatedForCreditControlGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OrgsEvaluatedForCreditControlGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OrgsEvaluatedForCreditControlGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.OrgsEvaluatedForCreditControlGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.OrgsEvaluatedForCreditControlGrid.CopySelectedRowsAllowed = true;
			this.OrgsEvaluatedForCreditControlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgsEvaluatedForCreditControlGrid.GridId = "e558b226-e145-4ffa-b49e-3799b11f8b40";
			this.OrgsEvaluatedForCreditControlGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgsEvaluatedForCreditControlGrid.LayoutKey = "OrgsEvaluatedForCreditControlGrid";
			this.OrgsEvaluatedForCreditControlGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgsEvaluatedForCreditControlGrid.Name = "OrgsEvaluatedForCreditControlGrid";
			this.OrgsEvaluatedForCreditControlGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			this.OrgsEvaluatedForCreditControlGrid.TabIndex = 0;
			// 
			// OrgsEvaluatedForCreditControlControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrgsEvaluatedForCreditControlGrid);
			this.Name = "OrgsEvaluatedForCreditControlControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgsEvaluatedForCreditControlGrid)).EndInit();
			this.OrgsEvaluatedForCreditControlGrid.ResumeLayout(false);
			this.OrgsEvaluatedForCreditControlGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal Enterprise.ZArchitecture.ZGrid OrgsEvaluatedForCreditControlGrid;
	}
}

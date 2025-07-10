namespace Enterprise.MasterFiles.GUI
{
	public partial class ProfitShareAgreementControl
	{

		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditJobTypes = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditGatewayAgentTypes = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditGatewayProfitApportionmentMethods = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZChargeCodesFindBoxColumnStyleInfo zGuidMultiFindColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZChargeCodesFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.ProfitShareGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PartyDetailsControl = new ProfitSharePartyDetailsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProfitShareGrid)).BeginInit();
			this.ProfitShareGrid.SuspendLayout();
			this.PartyDetailsControl.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgProfitShareDetails);
			//
			// ProfitShareGrid
			//
			this.ProfitShareGrid.AllowNavigation = false;
			this.ProfitShareGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProfitShareGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_SendingPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_ReceivingPortOrCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_GatewayAgentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_GatewayProfitApportionmentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_FreightMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_AgreementType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_ShareLosses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).AgreementTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_OH_ControllingAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_OrgOverrideType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgProfitShareDetails)(null)).O4_OH_OrgOverride)));
			this.ProfitShareGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "O4_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.ColumnName = "O4_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "O4_SendingPortOrCountry";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "O4_ReceivingPortOrCountry";
			zDropEditJobTypes.ColumnName = "O4_JobType";
			zDropEditGatewayAgentTypes.ColumnName = "O4_GatewayAgentType";
			zDropEditGatewayProfitApportionmentMethods.ColumnName = "O4_GatewayProfitApportionmentMethod";
			zDropEditColumnStyleInfo1.ColumnName = "O4_FreightMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo2.ColumnName = "O4_AgreementType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCheckBoxColumnStyleInfo1.ColumnName = "O4_ShareLosses";
			zGuidMultiFindColumnStyleInfo1.ColumnName = "AgreementTypeDescription";
			zGuidMultiFindColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "O4_OH_ControllingAgent";
			zDropEditColumnStyleInfo3.ColumnName = "O4_OrgOverrideType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "O4_OH_OrgOverride";
			this.ProfitShareGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ProfitShareGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ProfitShareGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ProfitShareGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ProfitShareGrid.ColumnStyles.Add(zDropEditJobTypes);
			this.ProfitShareGrid.ColumnStyles.Add(zDropEditGatewayAgentTypes);
			this.ProfitShareGrid.ColumnStyles.Add(zDropEditGatewayProfitApportionmentMethods);
			this.ProfitShareGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ProfitShareGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ProfitShareGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ProfitShareGrid.ColumnStyles.Add(zGuidMultiFindColumnStyleInfo1);
			this.ProfitShareGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ProfitShareGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ProfitShareGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ProfitShareGrid.CopySelectedRowsAllowed = true;
			this.ProfitShareGrid.GridId = "f85fbb04-1ecf-4441-9408-17294f583334";
			this.ProfitShareGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProfitShareGrid.LayoutKey = "ProfitShareSetupGrid";
			this.ProfitShareGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ProfitShareGrid.Name = "ProfitShareGrid";
			this.ProfitShareGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 207, true);
			this.ProfitShareGrid.TabIndex = 31;
			//
			// PartyDetailsControl
			//
			this.PartyDetailsControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PartyDetailsControl, ".");
			this.PartyDetailsControl.CaptionRenderingEnabled = true;
			this.PartyDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 221, true);
			this.PartyDetailsControl.Name = "PartyDetailsControl";
			this.PartyDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 123, true);
			this.PartyDetailsControl.TabIndex = 33;
			//
			// ProfitShareAgreementControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PartyDetailsControl);
			this.Controls.Add(this.ProfitShareGrid);
			this.Name = "ProfitShareAgreementControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 347, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProfitShareGrid)).EndInit();
			this.ProfitShareGrid.ResumeLayout(false);
			this.ProfitShareGrid.PerformLayout();
			this.PartyDetailsControl.ResumeLayout(false);
			this.PartyDetailsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion


	}
}

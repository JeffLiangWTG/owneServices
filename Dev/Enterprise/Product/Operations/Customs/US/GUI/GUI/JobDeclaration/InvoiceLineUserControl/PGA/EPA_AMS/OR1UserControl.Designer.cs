namespace Enterprise.Customs.US.GUI
{
	partial class OR1UserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			this.OR1GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OR1Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OR1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OR1Grid)).BeginInit();
			this.OR1Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AMSLineCollection);
			// 
			// OR1GroupBox
			// 
			this.OR1GroupBox.Controls.Add(this.OR1Grid);
			this.OR1GroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OR1GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OR1GroupBox.Name = "OR1GroupBox";
			this.OR1GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 327, true);
			this.OR1GroupBox.TabIndex = 0;
			this.OR1GroupBox.TabStop = false;
			this.OR1GroupBox.Text = "Certificate Details";
			// 
			// OR1Grid
			// 
			this.OR1Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OR1Grid, ".");

			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_ProductLabel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMSLine)(null)).AddInfoLookups.ProductNumberCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_LotNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_LotEntity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).FinalHandlerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_OA_FinalHandler)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).CerFinalHandlerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.AMSLine)(null)).US_OA_CerFinalHandler)));
			this.OR1Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_ProductLabel";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_LotNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_LotEntity";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "FinalHandlerOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zAddressDropEditColumnStyleInfo1.ColumnName = "US_OA_FinalHandler";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CerFinalHandlerOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			zAddressDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zAddressDropEditColumnStyleInfo2.ColumnName = "US_OA_CerFinalHandler";
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OR1Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OR1Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OR1Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OR1Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.OR1Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.OR1Grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.OR1Grid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.OR1Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OR1Grid.GridId = "ee4496a2-0476-4d40-aaf7-7d92f21aa829";
			this.OR1Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OR1Grid.LayoutKey = "OR1Grid";
			this.OR1Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.OR1Grid.Name = "OR1Grid";
			this.OR1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 312, true);
			this.OR1Grid.TabIndex = 0;
			// 
			// OR1UserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OR1GroupBox);
			this.Name = "OR1UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 327, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OR1GroupBox.ResumeLayout(false);
			this.OR1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OR1Grid)).EndInit();
			this.OR1Grid.ResumeLayout(false);
			this.OR1Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox OR1GroupBox;
		public ZArchitecture.ZGrid OR1Grid;
	}
}

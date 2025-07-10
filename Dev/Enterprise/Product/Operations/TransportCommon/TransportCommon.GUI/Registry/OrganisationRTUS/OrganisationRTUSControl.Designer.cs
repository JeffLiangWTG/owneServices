using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class OrganisationRTUSControl
	{
		ZGrid OrganisationRTUSGrid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			this.OrganisationRTUSGrid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationRTUSGrid)).BeginInit();
			this.OrganisationRTUSGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(TransportCommon.Registry.OrganisationRTUSCollection);
			//
			// OrganisationRTUSGrid
			//
			this.OrganisationRTUSGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrganisationRTUSGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.OrganisationRTUSOption)(null)).OrganisationPK);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.OrganisationRTUSOption)(null)).OrganisationName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.OrganisationRTUSOption)(null)).CBACode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.OrganisationRTUSOption)(null)).CBADescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.OrganisationRTUSOption)(null)).Url);
			this.OrganisationRTUSGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("OrganisationRTUSControl|570A9134-20B8-41AC-8E05-63FFCD9C9B08", "Organization");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo1.GroupName = Enterprise.TransportCommon.GUI.Res.GetData("OrganisationRTUSControl|335AAA20-16B3-4DEA-BED9-9AD76690D32C", "Organization");
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("OrganisationRTUSControl|BCC4FDB1-47BB-4BD5-8337-05987FF44350", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "OrganisationName";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.TransportCommon.GUI.Res.GetData("OrganisationRTUSControl|335AAA20-16B3-4DEA-BED9-9AD76690D32C", "Organization");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("OrganisationRTUSControl|0C28B747-04FD-4A6C-8919-49BC1146A8BC", "CBA");
			zDropEditColumnStyleInfo1.ColumnName = "CBACode";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.TransportCommon.GUI.Res.GetData("OrganisationRTUSControl|0C28B747-04FD-4A6C-8919-49BC1146A8BC", "CBA");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("OrganisationRTUSControl|826C0845-CA65-462E-BB60-1D887A06B14D", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "CBADescription";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.TransportCommon.GUI.Res.GetData("OrganisationRTUSControl|0C28B747-04FD-4A6C-8919-49BC1146A8BC", "CBA");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("d6727a4c-66fa-40ac-b802-b7a6d74c36d9", "URL");
			zTextBoxColumnStyleInfo3.ColumnName = "Url";
			zTextBoxColumnStyleInfo3.MaxLengthOverride = 200;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			this.OrganisationRTUSGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OrganisationRTUSGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrganisationRTUSGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrganisationRTUSGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrganisationRTUSGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrganisationRTUSGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganisationRTUSGrid.GridId = "0C28B747-04FD-4A6C-8919-49BC1146A8BC";
			this.OrganisationRTUSGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrganisationRTUSGrid.LayoutKey = "OrganisationRTUSGrid";
			this.OrganisationRTUSGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationRTUSGrid.Name = "OrganisationRTUSGrid";
			this.OrganisationRTUSGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 240, true);
			this.OrganisationRTUSGrid.TabIndex = 0;
			//
			// OrganisationRTUSControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrganisationRTUSGrid);
			this.Name = "OrganisationRTUSControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationRTUSGrid)).EndInit();
			this.OrganisationRTUSGrid.ResumeLayout(false);
			this.OrganisationRTUSGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}

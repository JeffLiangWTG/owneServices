
namespace Enterprise.Recruiter.GUI
{
	public partial class ReferringPartyConfigurationControl
	{
		private Enterprise.ZArchitecture.ZGrid MainGrid;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.MainGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainGrid)).BeginInit();
			this.MainGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.ReferringPartyConfigurationCollection);
			// 
			// MainGrid
			// 
			this.MainGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MainGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.ReferringPartyConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.ReferringPartyConfiguration)(null)).Domain)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.ReferringPartyConfiguration)(null)).ReferringPartyDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.ReferringPartyConfiguration)(null)).OrganizationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.ReferringPartyConfiguration)(null)).DefaultReferringSource)));
			this.MainGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("b9696d22-16f1-4a5b-a2c2-ed1270ccd72a", "Email / Domain");
			zTextBoxColumnStyleInfo1.ColumnName = "Domain";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("684a298a-60f8-4de9-938a-dc6517805dfa", "Referring Party");
			zDropEditColumnStyleInfo1.ColumnName = "ReferringPartyDescription";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ShowInDropDown = ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("d9207196-20ef-4bb0-be31-b68f6f7d658c", "Organization");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrganizationPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("4d27c947-c3ff-430f-b423-a927b53b4e78", "Default Referring Source");
			zDropEditColumnStyleInfo2.ColumnName = "DefaultReferringSource";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MainGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MainGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.MainGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MainGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGrid.GridId = "ef531a7f-020a-4a87-9693-3dd956215f64";
			this.MainGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MainGrid.LayoutKey = "MainGrid";
			this.MainGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGrid.Name = "MainGrid";
			this.MainGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 514, true);
			this.MainGrid.TabIndex = 0;
			// 
			// ReferringPartyConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGrid);
			this.Name = "ReferringPartyConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 514, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MainGrid)).EndInit();
			this.MainGrid.ResumeLayout(false);
			this.MainGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}

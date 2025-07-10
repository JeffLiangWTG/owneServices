
namespace Enterprise.Recruiter.GUI
{
	public partial class CertificationCodeMappingControl
	{
		internal Enterprise.ZArchitecture.ZGrid CertMappingsGrid;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CertMappingsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CertMappingsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.RecruiterTestTypeCollection);
			// 
			// RecruiterTestTypeGrid
			// 
			this.CertMappingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CertMappingsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.CertificationCodeMapping)(null)).MainCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.CertificationCodeMapping)(null)).SpecialisationCode)));
			this.CertMappingsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "MainCodeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("RecruiterTestTypeControl|9F5495D5-69C3-4218-AFAC-B27412DBB54C", "Main Code");
			zDropEditColumnStyleInfo1.ColumnName = "MainCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "SpecialisationCodeList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("RecruiterTestTypeControl|DB3AD5DB-C95C-4F11-99BA-DA3B6268BBF8", "Specialization Code");
			zDropEditColumnStyleInfo2.ColumnName = "SpecialisationCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.CertMappingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CertMappingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CertMappingsGrid.GridId = "B5134E2F-9C08-421A-BC3A-B1CA784F1D8F";
			this.CertMappingsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertMappingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CertMappingsGrid.LayoutKey = "CertificationCodeMappingGrid";
			this.CertMappingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CertMappingsGrid.Name = "CertificationCodeMappingGrid";
			this.CertMappingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 385, true);
			this.CertMappingsGrid.TabIndex = 0;
			// 
			// RecruiterTestTypeControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CertMappingsGrid);
			this.Name = "CertificationCodeMappingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 514, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CertMappingsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}

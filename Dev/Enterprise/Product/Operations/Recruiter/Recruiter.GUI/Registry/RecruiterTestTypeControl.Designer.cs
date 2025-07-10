
namespace Enterprise.Recruiter.GUI
{
	public partial class RecruiterTestTypeControl
	{
		internal Enterprise.ZArchitecture.ZGrid RecruiterTestTypeGrid;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.RecruiterTestTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RecruiterTestTypeGrid)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.RecruiterTestTypeCollection);
			// 
			// RecruiterTestTypeGrid
			// 
			this.RecruiterTestTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RecruiterTestTypeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)).Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)).CategoryList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)).CategoryTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)).NotificationTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)).NotificationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)).DocumentNames)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)).DocumentName)));
			this.RecruiterTestTypeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("RecruiterTestTypeControl|aa0d9dba-dd1a-4618-b2d9-975c1aae1d6e", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("RecruiterTestTypeControl|4946451a-66ba-405c-ae1f-81ea3e70cefa", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("RecruiterTestTypeControl|B905F9A9-F4B9-4E2B-8DAC-0F516D7FD8B0", "Category Title");
			zTextBoxColumnStyleInfo3.ColumnName = "CategoryTitle";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.BindToList = "CategoryList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("RecruiterTestTypeControl|f39ccb6b-b07a-4722-923f-85c4b8cc1baf", "Category");
			zDropEditColumnStyleInfo1.ColumnName = "Category";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("RecruiterTestTypeControl|809e7415-8e92-465a-a679-0837291540ed", "Visible On Web");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsVisibleOnWeb";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "NotificationTypeList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("RecruiterTestTypeControl|59b80b01-a9a0-4139-8551-032f4c7a9e94", "Notification Type");
			zDropEditColumnStyleInfo2.ColumnName = "NotificationType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo3.BindToList = "DocumentNames";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("RecruiterTestTypeControl|6b37fbaa-4611-4b14-b70d-a8f22b59048d", "Document Name");
			zDropEditColumnStyleInfo3.ColumnName = "DocumentName";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.RecruiterTestTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RecruiterTestTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RecruiterTestTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RecruiterTestTypeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RecruiterTestTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RecruiterTestTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RecruiterTestTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RecruiterTestTypeGrid.GridId = "500ab525-71cb-421c-afa1-fb0c6785b3f3";
			this.RecruiterTestTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecruiterTestTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecruiterTestTypeGrid.LayoutKey = "RecruiterTestTypeGrid";
			this.RecruiterTestTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RecruiterTestTypeGrid.Name = "RecruiterTestTypeGrid";
			this.RecruiterTestTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 385, true);
			this.RecruiterTestTypeGrid.TabIndex = 0;
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
			this.splitContainer1.Panel1.Controls.Add(this.RecruiterTestTypeGrid);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.zGroupBox1);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 514, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(385);
			this.splitContainer1.TabIndex = 1;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("RecruiterTestTypeControl|5d2725d7-23d3-49e3-bf66-b610a44411e0", "Test Note");
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 125, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "TestNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.RecruiterTestType)(null)).TestNote)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 106, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// RecruiterTestTypeControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "RecruiterTestTypeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 514, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RecruiterTestTypeGrid)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}

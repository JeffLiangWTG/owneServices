namespace Enterprise.Customs.GUI.Guarantees
{
	partial class GuaranteeAccessLineControl
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
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MainAccessCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainAccessPersonNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainAccessCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionnalCodesListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalAccessCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.MainAccessCodeGroupBox.SuspendLayout();
			this.AdditionnalCodesListGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalAccessCodesGrid)).BeginInit();
			this.AdditionalAccessCodesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseCusGuaranteeHeader);
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.IsSplitterFixed = true;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.MainAccessCodeGroupBox);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.AutoScroll = true;
			this.splitContainer.Panel2.Controls.Add(this.AdditionnalCodesListGroupBox);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 255, true);
			this.splitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(40);
			this.splitContainer.SplitterWidth = 16;
			this.splitContainer.TabIndex = 1;
			// 
			// MainAccessCodeGroupBox
			// 
			this.MainAccessCodeGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("863CC49E-6874-4C97-B503-FC4468E9E24E", "Main Access Code");
			this.MainAccessCodeGroupBox.Controls.Add(this.MainAccessPersonNameTextBox);
			this.MainAccessCodeGroupBox.Controls.Add(this.MainAccessCodeTextBox);
			this.MainAccessCodeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainAccessCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainAccessCodeGroupBox.Name = "MainAccessCodeGroupBox";
			this.MainAccessCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 40, true);
			this.MainAccessCodeGroupBox.TabIndex = 1;
			this.MainAccessCodeGroupBox.TabStop = false;
			// 
			// MainAccessPersonNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.MainAccessPersonNameTextBox, "MainAccessPersonName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).MainAccessPersonName)));
			this.MainAccessPersonNameTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("EFD30561-E49B-4D45-8FED-A898D9947C4C", "For Person (Name)");
			this.MainAccessPersonNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MainAccessPersonNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(353, 18, true);
			this.MainAccessPersonNameTextBox.Name = "MainAccessPersonNameTextBox";
			this.MainAccessPersonNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 20, true);
			this.MainAccessPersonNameTextBox.TabIndex = 1;
			// 
			// MainAccessCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MainAccessCodeTextBox, "MainAccessCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).MainAccessCode)));
			this.MainAccessCodeTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("863CC49E-6874-4C97-B503-FC4468E9E24E", "Main Access Code");
			this.MainAccessCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainAccessCodeTextBox.Name = "MainAccessCodeTextBox";
			this.MainAccessCodeTextBox.PasswordChar = '*';
			this.MainAccessCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.MainAccessCodeTextBox.TabIndex = 1;
			// 
			// AdditionnalCodesListGroupBox
			// 
			this.AdditionnalCodesListGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("GuaranteeAccessLineControl|1D55E372-311B-445A-9F69-DFCC150C05CD", "Additional Access Codes");
			this.AdditionnalCodesListGroupBox.Controls.Add(this.AdditionalAccessCodesGrid);
			this.AdditionnalCodesListGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionnalCodesListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionnalCodesListGroupBox.Name = "AdditionnalCodesListGroupBox";
			this.AdditionnalCodesListGroupBox.Size = this.splitContainer.Panel2.Size;
			this.AdditionnalCodesListGroupBox.TabIndex = 1;
			this.AdditionnalCodesListGroupBox.TabStop = false;
			// 
			// AdditionalAccessCodesGrid
			// 
			this.AdditionalAccessCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalAccessCodesGrid, "AdditionalAccessCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).AdditionalAccessCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusGuaranteeRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).AdditionalAccessCodes)).SyncRoot)).CPR_ValueFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusGuaranteeRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).AdditionalAccessCodes)).SyncRoot)).CPR_Description)));
			this.AdditionalAccessCodesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("GuaranteeAccessLineControl|B12CE184-C036-4F9E-B68A-3332551DBEDB", "Access Code");
			zTextBoxColumnStyleInfo1.ColumnName = "CPR_ValueFrom";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.PasswordChar = '*';
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("GuaranteeAccessLineControl|7A530412-4598-44A8-9316-DE5BB36E4A65", "For Person (Name)");
			zTextBoxColumnStyleInfo2.ColumnName = "CPR_Description";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(430);
			this.AdditionalAccessCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalAccessCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdditionalAccessCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalAccessCodesGrid.GridId = "ca1c0d58-312c-4a20-ac52-1f64d310bb66";
			this.AdditionalAccessCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalAccessCodesGrid.LayoutKey = "AdditionalAccessCodesGrid";
			this.AdditionalAccessCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AdditionalAccessCodesGrid.Name = "AdditionalAccessCodesGrid";
			this.AdditionalAccessCodesGrid.Size = this.AdditionnalCodesListGroupBox.Size;
			this.AdditionalAccessCodesGrid.TabIndex = 1;
			// 
			// GuaranteeAccessLineControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer);
			this.Name = "GuaranteeAccessLineControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 255, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			this.MainAccessCodeGroupBox.ResumeLayout(false);
			this.MainAccessCodeGroupBox.PerformLayout();
			this.AdditionnalCodesListGroupBox.ResumeLayout(false);
			this.AdditionnalCodesListGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalAccessCodesGrid)).EndInit();
			this.AdditionalAccessCodesGrid.ResumeLayout(false);
			this.AdditionalAccessCodesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private CargoWise.Windows.UI.KSplitContainer splitContainer;
		internal Enterprise.ZArchitecture.ZGrid AdditionalAccessCodesGrid;
		private Enterprise.ZArchitecture.ZTextBox MainAccessCodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MainAccessCodeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AdditionnalCodesListGroupBox;
		internal ZArchitecture.ZTextBox MainAccessPersonNameTextBox;
		#endregion
	}
}

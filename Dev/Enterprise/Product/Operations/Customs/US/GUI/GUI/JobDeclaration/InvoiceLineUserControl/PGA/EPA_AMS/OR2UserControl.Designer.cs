namespace Enterprise.Customs.US.GUI
{
	partial class OR2UserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.LotNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LotNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.CertificatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificatesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LotNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LotNumbersGrid)).BeginInit();
			this.LotNumbersGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.CertificatesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).BeginInit();
			this.CertificatesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AMS);
			// 
			// LotNumbersGroupBox
			// 
			this.LotNumbersGroupBox.Controls.Add(this.LotNumbersGrid);
			this.LotNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LotNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LotNumbersGroupBox.Name = "LotNumbersGroupBox";
			this.LotNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 234, true);
			this.LotNumbersGroupBox.TabIndex = 0;
			this.LotNumbersGroupBox.TabStop = false;
			this.LotNumbersGroupBox.Text = "Lot Numbers";
			// 
			// LotNumbersGrid
			// 
			this.LotNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LotNumbersGrid, "LotCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).LotCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLotCode)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).LotCodes)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLotCode)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).LotCodes)).SyncRoot)).CY_Code)));
			this.LotNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3fd25ec1-15a0-40ad-8fa4-35a35f27b49d", "Lot Number");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(176);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9dd2d2e9-e33b-49d7-b089-72e2ce142e18", "Lot Entity");
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.LotNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LotNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LotNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LotNumbersGrid.GridId = "dbeea3ff-ba4b-4746-935f-d8d8238227b1";
			this.LotNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LotNumbersGrid.LayoutKey = "LotNumbersGrid";
			this.LotNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.LotNumbersGrid.Name = "LotNumbersGrid";
			this.LotNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 217, true);
			this.LotNumbersGrid.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.CertificatesGroupBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.LotNumbersGroupBox);
			this.splitContainer1.Size = new System.Drawing.Size(1016, 292);
			this.splitContainer1.SplitterDistance = 550;
			this.splitContainer1.TabIndex = 1;
			// 
			// CertificatesGroupBox
			// 
			this.CertificatesGroupBox.Controls.Add(this.CertificatesGrid);
			this.CertificatesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CertificatesGroupBox.Name = "CertificatesGroupBox";
			this.CertificatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 234, true);
			this.CertificatesGroupBox.TabIndex = 1;
			this.CertificatesGroupBox.TabStop = false;
			this.CertificatesGroupBox.Text = "Certificates";
			// 
			// CertificatesGrid
			// 
			this.CertificatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CertificatesGrid, "AMSLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)).SyncRoot)).US_CertNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.AMSLine)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AMS)(null)).AMSLines)).SyncRoot)).US_CertType)));
			this.CertificatesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("25e062e7-eee3-425c-b862-c7a91c83718f", "Certificate No.");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_CertNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(256);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("852c0695-4689-4c50-bd94-273eb76a490c", "LPCO Type");
			zDropEditColumnStyleInfo2.ColumnName = "US_CertType";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CertificatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CertificatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificatesGrid.GridId = "f1dbd8c2-5eca-4ff8-a278-97d5fb5f1ee4";
			this.CertificatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CertificatesGrid.LayoutKey = "CertificatesGrid";
			this.CertificatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.CertificatesGrid.Name = "CertificatesGrid";
			this.CertificatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 217, true);
			this.CertificatesGrid.TabIndex = 0;
			// 
			// OR2UserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.splitContainer1);
			this.Name = "OR2UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 234, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LotNumbersGroupBox.ResumeLayout(false);
			this.LotNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LotNumbersGrid)).EndInit();
			this.LotNumbersGrid.ResumeLayout(false);
			this.LotNumbersGrid.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.CertificatesGroupBox.ResumeLayout(false);
			this.CertificatesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).EndInit();
			this.CertificatesGrid.ResumeLayout(false);
			this.CertificatesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox LotNumbersGroupBox;
		public ZArchitecture.ZGrid LotNumbersGrid;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private ZArchitecture.GUI.ZGroupBox CertificatesGroupBox;
		private ZArchitecture.ZGrid CertificatesGrid;
	}
}

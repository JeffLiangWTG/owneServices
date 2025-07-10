namespace Enterprise.Rating.GUI
{
	partial class GRIUpdateControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		void InitializeComponent()
		{
			CargoWise.Windows.UI.KPanel bottomPanel;
			Enterprise.ZArchitecture.GUI.ZGuidDropEdit printerDropEdit;
			Enterprise.ZArchitecture.GUI.ZCheckBox includeCoverPageCheckBox;
			Enterprise.ZArchitecture.GUI.ZDateEdit lastRunDateEdit;
			Enterprise.ZArchitecture.ZLabel instructionsLabel;
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			bottomPanel = new CargoWise.Windows.UI.KPanel();
			printerDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			includeCoverPageCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			lastRunDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			instructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			clientGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanel.SuspendLayout();
			printerDropEdit.SuspendLayout();
			lastRunDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(clientGrid)).BeginInit();
			clientGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.RateUpdater);
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(printerDropEdit);
			bottomPanel.Controls.Add(includeCoverPageCheckBox);
			bottomPanel.Controls.Add(lastRunDateEdit);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 304, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 52, true);
			bottomPanel.TabIndex = 1;
			// 
			// printerDropEdit
			// 
			printerDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(printerDropEdit, "PrinterPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.RateUpdater)(null)).PrinterPK)));
			printerDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 8, true);
			printerDropEdit.Name = "printerDropEdit";
			printerDropEdit.PreBoundMaxLength = 30;
			printerDropEdit.ShowDescriptionBox = false;
			printerDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 17, true);
			printerDropEdit.TabIndex = 1;
			// 
			// includeCoverPageCheckBox
			// 
			includeCoverPageCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(includeCoverPageCheckBox, "IncludeCoverPage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.RateUpdater)(null)).IncludeCoverPage)));
			includeCoverPageCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			includeCoverPageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			includeCoverPageCheckBox.Name = "includeCoverPageCheckBox";
			includeCoverPageCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 16, true);
			includeCoverPageCheckBox.TabIndex = 2;
			// 
			// lastRunDateEdit
			// 
			lastRunDateEdit.AllowDrop = true;
			lastRunDateEdit.AutoCompleteMonthThreshold = 1;
			lastRunDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(lastRunDateEdit, "LastRunDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.RateUpdater)(null)).LastRunDate)));
			lastRunDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			lastRunDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			lastRunDateEdit.Name = "lastRunDateEdit";
			lastRunDateEdit.TabIndex = 0;
			// 
			// instructionsLabel
			// 
			instructionsLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("GRIUpdateControl|bac4e2a5-cecd-4cba-86e5-3fb92126af4f", "", "The following clients require an updated pricing page due to changes in your Company Tariffs and Costings, or changes in their Client rates.");
			instructionsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			instructionsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			instructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			instructionsLabel.Name = "instructionsLabel";
			instructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 22, true);
			instructionsLabel.TabIndex = 2;
			// 
			// clientGrid
			// 
			clientGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(clientGrid, "Rates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.RateUpdater)(null)).Rates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.UpdateRate)(((System.Collections.IList)(((Enterprise.Rating.Business.RateUpdater)(null)).Rates)).SyncRoot)).IncludeInUpdate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Rating.Business.UpdateRate)(((System.Collections.IList)(((Enterprise.Rating.Business.RateUpdater)(null)).Rates)).SyncRoot)).ClientPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.UpdateRate)(((System.Collections.IList)(((Enterprise.Rating.Business.RateUpdater)(null)).Rates)).SyncRoot)).FullName)));
			clientGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInUpdate";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ClientPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "FullName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			clientGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			clientGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			clientGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			clientGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			clientGrid.GridId = "19350e12-06fd-4939-b614-0f6f4a902837";
			clientGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			clientGrid.LayoutKey = "RatesGrid";
			clientGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true);
			clientGrid.Name = "clientGrid";
			clientGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 282, true);
			clientGrid.TabIndex = 3;
			// 
			// GRIUpdateControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(clientGrid);
			this.Controls.Add(instructionsLabel);
			this.Controls.Add(bottomPanel);
			this.Name = "GRIUpdateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 356, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanel.ResumeLayout(false);
			bottomPanel.PerformLayout();
			printerDropEdit.ResumeLayout(true);
			printerDropEdit.PerformLayout();
			lastRunDateEdit.ResumeLayout(true);
			lastRunDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(clientGrid)).EndInit();
			clientGrid.ResumeLayout(false);
			clientGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public Enterprise.ZArchitecture.ZGrid clientGrid;
	}
}

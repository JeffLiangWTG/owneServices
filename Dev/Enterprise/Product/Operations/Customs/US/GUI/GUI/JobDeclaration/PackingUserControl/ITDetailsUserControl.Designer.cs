namespace Enterprise.Customs.US.GUI
{
	partial class ITDetailsUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.ITNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ITNumbersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ITAndSplitDetailsCollection);
			// 
			// ITNumbersGrid
			// 
			this.ITNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ITNumbersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ITAndSplitDetails)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ITAndSplitDetails)(null)).US_ITNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ITAndSplitDetails)(null)).US_NoOfPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ITAndSplitDetails)(null)).US_CarrierCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ITAndSplitDetails)(null)).US_FlightNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ITAndSplitDetails)(null)).US_ArrivalDate)));
			this.ITNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a5a4da20-0971-4ce4-b519-1c10edf17929", "IT Number");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_ITNumber";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ecf3f9ad-d348-4064-a5f1-30edbe6a6222", "Manifest Qty");
			zCalcEditColumnStyleInfo1.ColumnName = "US_NoOfPacks";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("482f39cc-e455-402a-b6a0-614fec14d5d2", "Carrier");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "US_CarrierCode";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("75d6db6a-f497-44ac-a190-53f62f8d9d53", "Flight/Trip No");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "US_FlightNumber";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("373b7132-0f1c-414b-8fa3-0ff9f50666a8", "Arrival Date");
			zDateEditColumnStyleInfo1.ColumnName = "US_ArrivalDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.ITNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ITNumbersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ITNumbersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ITNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ITNumbersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ITNumbersGrid.CopySelectedRowsAllowed = true;
			this.ITNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ITNumbersGrid.GridId = "cac52037-f8e0-4866-8178-fc824843e20f";
			this.ITNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ITNumbersGrid.LayoutKey = "ITNumbersGrid";
			this.ITNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ITNumbersGrid.Name = "ITNumbersGrid";
			this.ITNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			this.ITNumbersGrid.TabIndex = 2;
			// 
			// ITDetailsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ITNumbersGrid);
			this.Name = "ITDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ITNumbersGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid ITNumbersGrid;

	}
}

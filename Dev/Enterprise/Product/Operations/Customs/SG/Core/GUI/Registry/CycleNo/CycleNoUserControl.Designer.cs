namespace Enterprise.Customs.SG.Registry.GUI
{
	partial class CycleNoUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.CycleNoGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CycleNoGrid)).BeginInit();
			this.CycleNoGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.Registry.CycleNoCollection);
			// 
			// CycleNoGrid
			// 
			this.CycleNoGrid.AllowNavigation = false;
			this.CycleNoGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CycleNoGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.Registry.CycleNo)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.Registry.CycleNo)(null)).CycleNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.SG.Registry.CycleNo)(null)).SubmissionTime)));
			this.CycleNoGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("0068e400-364d-42ba-9faa-8ca0a8bb0cf2", "Cycle Number");
			zCalcEditColumnStyleInfo1.ColumnName = "CycleNum";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("f76652e4-66d2-4a02-a818-57476597a46b", "AECs Submission Time");
			zDateEditColumnStyleInfo1.ColumnName = "SubmissionTime";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			this.CycleNoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CycleNoGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CycleNoGrid.GridId = "a0ebfd6b-df1f-458f-ab72-33bdaabd7a81";
			this.CycleNoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CycleNoGrid.LayoutKey = "CycleNoGrid";
			this.CycleNoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
			this.CycleNoGrid.Name = "CycleNoGrid";
			this.CycleNoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 421, true);
			this.CycleNoGrid.TabIndex = 0;
			// 
			// CycleNoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CycleNoGrid);
			this.Name = "CycleNoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 427, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CycleNoGrid)).EndInit();
			this.CycleNoGrid.ResumeLayout(false);
			this.CycleNoGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid CycleNoGrid;
	}
}

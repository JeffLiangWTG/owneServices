namespace Enterprise.Customs.DataRegistry.GUI
{
	partial class UCMEDIMessageTestTypeUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ApplicationCodesToTestGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationCodesToTestGrid)).BeginInit();
			this.ApplicationCodesToTestGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DataRegistry.Business.UCMEDIMessageTestTypeCollection);
			// 
			// ApplicationCodesToTestGrid
			// 
			this.ApplicationCodesToTestGrid.AllowNavigation = false;
			this.ApplicationCodesToTestGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ApplicationCodesToTestGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DataRegistry.Business.UCMEDIMessageTestType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.UCMEDIMessageTestType)(null)).ApplicationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DataRegistry.Business.UCMEDIMessageTestType)(null)).UCKDelayTimeInMilliseconds)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DataRegistry.Business.UCMEDIMessageTestType)(null)).UCQDelayTimeInMilliseconds)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DataRegistry.Business.UCMEDIMessageTestType)(null)).UCUDelayTimeInMilliseconds)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.UCMEDIMessageTestType)(null)).ShouldMessageBeProcessedInASeparateFactory)));
			this.ApplicationCodesToTestGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("e7367fbb-9766-4e42-9726-bb5dd6108496", "Application Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ApplicationCode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("c837be42-f6bc-4425-be12-08c1749db5a7", "UCK", "UCK Delay Time In Milliseconds", "");
			zCalcEditColumnStyleInfo1.ColumnName = "UCKDelayTimeInMilliseconds";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("c5c8051d-016e-4a2f-9fb8-ab56a1f08599", "UCQ", "UCQ Delay Time In Milliseconds", "");
			zCalcEditColumnStyleInfo2.ColumnName = "UCQDelayTimeInMilliseconds";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("85c068a4-7b00-423c-835e-e348ff615b2c", "UCU", "UCU Delay Time In Milliseconds", "");
			zCalcEditColumnStyleInfo3.ColumnName = "UCUDelayTimeInMilliseconds";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("bfd193c6-fe6c-4017-aeb2-377f96dd4129", "Process In Separate Factory");
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldMessageBeProcessedInASeparateFactory";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155);
			this.ApplicationCodesToTestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ApplicationCodesToTestGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ApplicationCodesToTestGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ApplicationCodesToTestGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ApplicationCodesToTestGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ApplicationCodesToTestGrid.GridId = "e314bef2-048b-4bd0-8301-e41bf9740163";
			this.ApplicationCodesToTestGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApplicationCodesToTestGrid.LayoutKey = "ApplicationCodesToTestGrid";
			this.ApplicationCodesToTestGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApplicationCodesToTestGrid.Name = "ApplicationCodesToTestGrid";
			this.ApplicationCodesToTestGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 271, true);
			this.ApplicationCodesToTestGrid.TabIndex = 0;
			// 
			// UCMEDIMessageTestTypeUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ApplicationCodesToTestGrid);
			this.Name = "UCMEDIMessageTestTypeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 271, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationCodesToTestGrid)).EndInit();
			this.ApplicationCodesToTestGrid.ResumeLayout(false);
			this.ApplicationCodesToTestGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid ApplicationCodesToTestGrid;
	}
}

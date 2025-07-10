namespace Enterprise.MasterFiles.GUI
{
	partial class DefaultEPaymentReasonControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DefaultReasonGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultReasonGrid)).BeginInit();
			this.DefaultReasonGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.DefaultEPaymentReasonCollection);
			// 
			// DefaultReasonGrid
			// 
			this.DefaultReasonGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DefaultReasonGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DefaultEPaymentReason)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DefaultEPaymentReason)(null)).ProviderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DefaultEPaymentReason)(null)).ReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DefaultEPaymentReason)(null)).ReasonDescription)));
			this.DefaultReasonGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1917781d-667f-402a-9ce7-18fda620e375", "Provider");
			zDropEditColumnStyleInfo1.ColumnName = "ProviderCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6bce21c2-594c-44a7-9159-cee2d7aeed5f", "Code");
			zDropEditColumnStyleInfo2.ColumnName = "ReasonCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cf32ced2-340f-45b1-9b67-ee25633a85ed", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ReasonDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.DefaultReasonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DefaultReasonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DefaultReasonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DefaultReasonGrid.GridId = "6056499f-7c89-4a4b-957a-6f9c79035003";
			this.DefaultReasonGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultReasonGrid.LayoutKey = "zGrid1";
			this.DefaultReasonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DefaultReasonGrid.Name = "DefaultReasonGrid";
			this.DefaultReasonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 232, true);
			this.DefaultReasonGrid.TabIndex = 0;
			// 
			// DefaultEPaymentReasonControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultReasonGrid);
			this.Name = "DefaultEPaymentReasonControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 272, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultReasonGrid)).EndInit();
			this.DefaultReasonGrid.ResumeLayout(false);
			this.DefaultReasonGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid DefaultReasonGrid;
	}
}

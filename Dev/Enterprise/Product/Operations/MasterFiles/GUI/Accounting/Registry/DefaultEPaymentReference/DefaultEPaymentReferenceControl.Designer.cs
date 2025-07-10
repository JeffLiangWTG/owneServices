namespace Enterprise.MasterFiles.GUI
{
	partial class DefaultEPaymentReferenceControl
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
			this.PaymentReferenceGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PaymentReferenceGrid)).BeginInit();
			this.PaymentReferenceGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.DefaultEPaymentReferenceCollection);
			// 
			// DefaultReasonGrid
			// 
			this.PaymentReferenceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PaymentReferenceGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DefaultEPaymentReference)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DefaultEPaymentReference)(null)).ProviderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DefaultEPaymentReference)(null)).ReferenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DefaultEPaymentReference)(null)).Reference)));
			this.PaymentReferenceGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1917781d-667f-402a-9ce7-18fda620e375", "Provider");
			zDropEditColumnStyleInfo1.ColumnName = "ProviderCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("66AC72EB-6339-46F6-B45C-A1B6C4F74A15", "Reference Type");
			zDropEditColumnStyleInfo2.ColumnName = "ReferenceType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("96E76F0C-ED12-4D66-A41C-19AD8288E765", "Reference");
			zTextBoxColumnStyleInfo1.ColumnName = "Reference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.PaymentReferenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PaymentReferenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PaymentReferenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PaymentReferenceGrid.GridId = "EB576F36-0210-495F-9051-2F690B4AD19E";
			this.PaymentReferenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PaymentReferenceGrid.LayoutKey = "zGrid1";
			this.PaymentReferenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PaymentReferenceGrid.Name = "PaymentReferenceGrid";
			this.PaymentReferenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 232, true);
			this.PaymentReferenceGrid.TabIndex = 0;
			// 
			// DefaultEPaymentReferenceControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PaymentReferenceGrid);
			this.Name = "DefaultEPaymentReferenceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 272, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PaymentReferenceGrid)).EndInit();
			this.PaymentReferenceGrid.ResumeLayout(false);
			this.PaymentReferenceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid PaymentReferenceGrid;
	}
}

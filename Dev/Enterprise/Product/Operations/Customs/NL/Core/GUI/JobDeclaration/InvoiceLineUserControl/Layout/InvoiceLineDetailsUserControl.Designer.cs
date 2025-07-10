using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	public partial class InvoiceLineDetailsUserControl
	{
		void InitializeComponent()
		{
			this.ECCNCodesUserControl = new Enterprise.Customs.NL.GUI.ECCNCodesUserControl();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ECCNCodesUserControl.SuspendLayout();
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobComInvoiceLine);

			this.ECCNCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ECCNCodesUserControl, ".");
			this.ECCNCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 152, true);
			this.ECCNCodesUserControl.Name = "ECCNCodesUserControl";
			this.ECCNCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 22, true);
			this.ECCNCodesUserControl.TabIndex = 4;
			
			this.Controls.Add(this.ECCNCodesUserControl);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 525, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ECCNCodesUserControl.ResumeLayout(true);
			this.ECCNCodesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ECCNCodesUserControl ECCNCodesUserControl;		
	}
}

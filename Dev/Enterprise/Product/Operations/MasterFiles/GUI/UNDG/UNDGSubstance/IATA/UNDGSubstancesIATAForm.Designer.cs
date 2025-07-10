namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceIATAForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			this.SuspendLayout();

			//
			// UNDGSubstanceADNForm
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstance);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 674);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 674);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstance);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.UNDGSubstance";
			this.Name = "UNDGSubstanceIATAForm";

			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}

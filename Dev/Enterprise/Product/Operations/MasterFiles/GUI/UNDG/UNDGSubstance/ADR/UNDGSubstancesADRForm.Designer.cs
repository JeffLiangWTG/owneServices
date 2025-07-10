namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceADRForm
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
			// UNDGSubstanceADRForm
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstanceADR);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 725);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 725, true);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstanceADR);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.UNDGSubstanceADR";
			this.Name = "UNDGSubstanceADRForm";

			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}

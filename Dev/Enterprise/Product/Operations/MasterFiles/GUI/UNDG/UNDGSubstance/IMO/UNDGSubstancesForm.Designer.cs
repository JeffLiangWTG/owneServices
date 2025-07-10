namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceForm
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
			// UNDGSubstanceForm
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstance);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 767, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 767, true);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstance);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.UNDGSubstance";
			this.Name = "UNDGSubstanceForm";

			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}

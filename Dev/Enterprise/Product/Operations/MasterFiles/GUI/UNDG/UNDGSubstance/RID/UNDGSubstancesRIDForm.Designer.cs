namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceRIDForm
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
			// UNDGSubstanceRIDForm
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstanceRID);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 725);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 725);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstanceRID);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.UNDGSubstanceRID";
			this.Name = "UNDGSubstanceRIDForm";

			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}

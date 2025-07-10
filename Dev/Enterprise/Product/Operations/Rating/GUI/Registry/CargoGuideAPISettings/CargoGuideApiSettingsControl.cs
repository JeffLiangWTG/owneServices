using Enterprise.Registry.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CargoGuideApiSettingsControl : RegistryBusinessObjectTemplateZUserControl
	{
		public CargoGuideApiSettingsControl()
		{
			InitializeComponent();
		}

		readonly System.ComponentModel.IContainer components;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}

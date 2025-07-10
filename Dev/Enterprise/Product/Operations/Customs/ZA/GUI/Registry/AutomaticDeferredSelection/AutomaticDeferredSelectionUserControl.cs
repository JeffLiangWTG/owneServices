using Enterprise.Registry.GUI;

namespace Enterprise.Customs.ZA.DataRegistry.GUI
{
	public partial class AutomaticDeferredSelectionUserControl : RegistryZUserControl
	{
		public AutomaticDeferredSelectionUserControl() : base()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			AllowAutomaticDeferredSelectionCheckBox.ReadOnly = readOnly;
			DaysBeforeETAIntEdit.ReadOnly = readOnly;
		}
	}
}

using System;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CarrierConfigurationUserControl : OrganisationContainerControl
	{
		public CarrierConfigurationUserControl()
		{
			InitializeComponent();

#if DEBUG

			RefShippingLineTabPage.RunWhenBindingOrFirstShown((object sender, EventArgs e) => { MissingResourceStringChecker.ExcludeFromTest(IntegrationsLabel); });

#endif
		}
	}
}

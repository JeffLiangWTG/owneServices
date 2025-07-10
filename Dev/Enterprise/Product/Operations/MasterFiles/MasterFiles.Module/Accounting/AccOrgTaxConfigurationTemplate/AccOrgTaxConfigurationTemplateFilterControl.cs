using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccOrgTaxConfigurationTemplateFilterControl : ZFilterStripControl
	{
		public AccOrgTaxConfigurationTemplateFilterControl()
		{
			InitializeComponent();
		}

		public AccOrgTaxConfigurationTemplateFilterControl(IBusinessObjectCollection gridCollection, AccOrgTaxConfigurationTemplateFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

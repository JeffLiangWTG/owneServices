using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgContactsFilterControl : ZFilterStripControl
	{
		public OrgContactsFilterControl(IBusinessObjectCollection gridCollection, OrgContactsFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

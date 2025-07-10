using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccTaxOverrideGroupFilterControl : ZFilterStripControl
	{
		public AccTaxOverrideGroupFilterControl(IBusinessObjectCollection gridCollection, AccTaxOverrideGroupFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

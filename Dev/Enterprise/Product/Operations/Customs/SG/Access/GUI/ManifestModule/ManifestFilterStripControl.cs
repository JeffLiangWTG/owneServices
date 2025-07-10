using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Module;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.SG.Access.GUI
{
	public partial class ManifestFilterStripControl : AsycudaFilterStripControl
	{
		public ManifestFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			RemoveGridColumns();
		}

		protected void RemoveGridColumns()
		{
			grid.SetAvailability(false, AsycudaManifestHeader.Schema.AMA_RN_NKCountry);
		}
	}
}

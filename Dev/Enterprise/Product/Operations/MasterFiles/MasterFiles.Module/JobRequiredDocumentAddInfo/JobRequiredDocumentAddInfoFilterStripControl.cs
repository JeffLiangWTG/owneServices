using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class JobRequiredDocumentAddInfoFilterStripControl : ZFilterStripControl
	{
		public JobRequiredDocumentAddInfoFilterStripControl()
		{
			InitializeComponent();
		}

		public JobRequiredDocumentAddInfoFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}

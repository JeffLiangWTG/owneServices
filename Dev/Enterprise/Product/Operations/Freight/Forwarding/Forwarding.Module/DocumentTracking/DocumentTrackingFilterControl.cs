using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Module
{
	public partial class DocumentTrackingFilterControl : ZFilterStripControl
	{
		public DocumentTrackingFilterControl(JobRequiredDocumentCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new DocumentTrackingFilterStrip();
		}
	}
}

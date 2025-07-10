using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class ProcessTaskTemplateFilterControl : ZFilterStripControl
	{
		public ProcessTaskTemplateFilterControl()
		{
			InitializeComponent();
		}

		public ProcessTaskTemplateFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

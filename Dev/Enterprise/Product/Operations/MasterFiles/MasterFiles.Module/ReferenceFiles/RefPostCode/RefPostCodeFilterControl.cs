using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefPostCodeFilterControl : ZFilterStripControl
	{
		public RefPostCodeFilterControl()
		{
			InitializeComponent();
		}

		public RefPostCodeFilterControl(IBusinessObjectCollection gridCollection, RefPostCodeFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

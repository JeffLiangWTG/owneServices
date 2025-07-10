using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccGLAccountDescriptorFilterControl : ZFilterStripControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZFilterStrip();
		}

		public AccGLAccountDescriptorFilterControl(IBusinessObjectCollection gridCollection, AccGLAccountDescriptorFilterBusinessObject filterBusinessObj)
			: base(gridCollection, filterBusinessObj)
		{
			InitializeComponent();
		}
	}
}

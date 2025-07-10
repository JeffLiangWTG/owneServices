using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class EDICodeMappingUserControl : ZFilterStripControl
	{
		public EDICodeMappingUserControl(IBusinessObjectCollection gridCollection, EDICodeMappingFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip() => new EDICodeMappingFilterStrip();
	}
}

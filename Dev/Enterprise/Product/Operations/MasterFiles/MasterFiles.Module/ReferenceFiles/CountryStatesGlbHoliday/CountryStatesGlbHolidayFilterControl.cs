using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class CountryStatesGlbHolidayFilterControl : ZFilterStripControl
	{
		public CountryStatesGlbHolidayFilterControl(IBusinessObjectCollection gridCollection, CountryStatesGlbHolidayFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new CountryStatesGlbHolidayFilterStrip();
		}
	}
}

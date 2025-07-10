using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CountryStatesGlbHolidayForm
	{
		public CountryStatesGlbHolidayForm(CountryStatesGlbHolidayBizo glbHolidayBizo) : base(glbHolidayBizo)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}
	}
}

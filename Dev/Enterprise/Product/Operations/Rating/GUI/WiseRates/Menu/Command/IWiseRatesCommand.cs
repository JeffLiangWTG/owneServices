using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public interface IWiseRatesCommand
	{
		bool Assign(WiseEntryView wiseEntryView, object sender);
		bool IsEnabled(WiseEntryView wiseEntryView);
	}
}

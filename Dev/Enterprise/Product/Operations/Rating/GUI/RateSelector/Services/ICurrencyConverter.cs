using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.GUI.RateSelector.Services
{
	public interface ICurrencyConverter
	{
		Money Convert(Money amount, RefCurrency targetCurrency);
	}
}

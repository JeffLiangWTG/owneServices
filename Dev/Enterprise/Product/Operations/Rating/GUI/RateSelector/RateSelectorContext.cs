using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Services;
using WiseRates.Api.Model;

namespace Enterprise.Rating.GUI.RateSelector
{
	public class RateSelectorContext
	{
		public BusinessObjectFactory Factory { get; set; }
		public RatesSearchResponse RatesServiceResponse { get; set; }
		public string RatesServiceTraceID { get; set; }
		public RateSelectorFilterStripBusinessObject Filters { get; set; }
		public ICurrencyConverter CurrencyConverter { get; set; }
		public IDialogService DialogService { get; set; }
		public MemoryLogger Logger { get; set; }
	}
}

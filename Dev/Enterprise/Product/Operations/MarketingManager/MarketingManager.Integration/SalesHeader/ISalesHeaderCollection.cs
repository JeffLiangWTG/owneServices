using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Integration
{
	public interface ISalesHeaderCollection : IBusinessObjectCollection
	{
		new ISalesHeader this[int i] { get; }

		ZDecimal TotalValue { get; }
		ZDecimal CommittedValue { get; }
		ZDecimal PipelineValue { get; }
		ZDecimal UnsuccessfulValue { get; }

		void Refresh(bool fromListChangedEvent = false);
		void ValidateAllCurrencies();
	}
}

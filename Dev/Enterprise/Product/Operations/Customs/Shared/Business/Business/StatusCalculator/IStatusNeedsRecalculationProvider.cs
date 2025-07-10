using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business
{
	public interface IStatusNeedsRecalculationProvider : IFactoryProvider
	{
		bool StatusNeedsRecalculation { get; }
		EDIMessageCollection Messages { get; }
	}
}

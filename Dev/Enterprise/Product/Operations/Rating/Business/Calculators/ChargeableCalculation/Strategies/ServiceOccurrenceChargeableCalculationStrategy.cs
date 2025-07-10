using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	public class ServiceOccurrenceChargeableCalculationStrategy : ServiceChargeableCalculationStrategy
	{
		public ServiceOccurrenceChargeableCalculationStrategy(Calculator calculator)
			: base(calculator)
		{
		}

		protected override Quantity GetChargeableAmountCore(JobServiceInfo service, ZString unit)
		{
			return new Quantity(service.ServiceCount, unit, reference: service.ServiceReference);
		}
	}
}


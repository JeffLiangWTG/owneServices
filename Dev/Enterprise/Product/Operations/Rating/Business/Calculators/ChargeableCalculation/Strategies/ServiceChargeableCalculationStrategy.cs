using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public abstract class ServiceChargeableCalculationStrategy : ChargeableCalculationStrategyBase
	{
		protected ServiceChargeableCalculationStrategy(Calculator calculator)
			: base(calculator)
		{
		}

		protected override Quantity GetChargeableAmountCore(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			if (parameters.ServiceRater.JobServices.Count == 0)
			{
				return Quantity.Empty((NoResString)"The job has no services."); // Autorating log
			}

			var services = GetServices(parameters, RateLine);
			return !services.Any()
				? Quantity.Empty((NoResString)"The job has no services matching rate line") // Autorating log
				: GetChargeableAmountCore(services, unit);
		}

		Quantity GetChargeableAmountCore(IEnumerable<JobServiceInfo> services, ZString unit)
		{
			ZDecimal chargeableAmountSum = 0m;
			var serviceReferences = new List<string>(0);
			foreach (var serviceInfo in services)
			{
				var amount = GetChargeableAmountCore(serviceInfo, unit).Amount;
				if (amount != 0)
				{
					chargeableAmountSum += amount;
					serviceReferences.Add(serviceInfo.ServiceReference.IsEmpty ? serviceInfo.ServiceId : serviceInfo.ServiceReference);
				}
			}

			if (chargeableAmountSum != 0m)
			{
				return new Quantity(chargeableAmountSum, unit, reference: string.Join(", ", serviceReferences));
			}

			return new Quantity(0, unit);
		}

		protected abstract Quantity GetChargeableAmountCore(JobServiceInfo service, ZString unit);

		internal static IEnumerable<JobServiceInfo> GetServices(AutoRatingCalculatorParameters parameters, IRateLine line)
		{
			return parameters.GetServicesBeingCalculated(line);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	public class TimeChargeableCalculationStrategy : ChargeableCalculationStrategyBase
	{
		public TimeChargeableCalculationStrategy(Calculator calculator)
			: base(calculator)
		{
		}

		protected override Quantity GetChargeableAmountCore(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			if (!QuantityUnit.IsTime(unit))
			{
				return default(Quantity);
			}

			var quantities = GetMultipleChargeableAmounts(parameters, unit);
			return RepeatedQuantity.Sum(quantities, unit);
		}

		/// <summary>
		/// Return a list of time quantities found in the parameters, along with a repeat count and container count if it's per-container.
		/// If the quantity is not per-container, it is a job level time quantity.
		/// If the quantity is from a service, the repeat count is the number of occurrences of the service.
		/// 
		/// Will return a list of service times if:
		///  - the line charge code is a service charge
		///  - there are service with times.
		/// 
		/// If the charge code is not for a service, or there are no services with a time, will
		/// return the single time measure in the criteria, if any.
		/// </summary>
		public IList<RepeatedQuantity> GetMultipleChargeableAmounts(AutoRatingCalculatorParameters parameters, ZString timeUnit)
		{
			var quantities = new List<RepeatedQuantity>();
			var line = RateLine;
			var chargeCode = line.ChargeCode;
			var isJobServiceChargeCode = !chargeCode.AC_ChargeSubGroup.IsEmpty;
			if (isJobServiceChargeCode)
			{
				var services = ServiceChargeableCalculationStrategy.GetServices(parameters, RateLine);
				foreach (var info in services)
				{
					AddService(timeUnit, quantities, info);
				}
			}

			// If there were no services with a time, look for a time measure.
			if (!quantities.Any())
			{
				var timeInfo = parameters.Criteria.JobMeasures?.GetTime();
				if (timeInfo != null)
				{
					var excludedHolidays = timeUnit != QuantityUnit.WK
						? Calculator.ExcludedHolidays
						: 0;
					var duration = timeInfo.SpanExcluding(excludedHolidays);
					var unitCount = ConvertToQuantityUnits(duration, timeUnit);
					var quantity = new Quantity(unitCount, timeUnit);
					quantities.Add(new RepeatedQuantity(quantity, repeatCount: 1, containerCount: 0));
				}
			}

			return quantities;
		}

		static void AddService(ZString unit, List<RepeatedQuantity> quantities, JobServiceInfo info)
		{
			if (info != null && info.IsEnabled && info.ServiceDuration != TimeSpan.Zero)
			{
				var unitCount = ConvertToQuantityUnits(info.ServiceDuration, unit);
				var reference = info?.ContainerNumber ?? string.Empty;
				if (!info.ServiceReference.IsEmpty)
				{
					reference += (reference.Length != 0 ? " " : string.Empty) + info.ServiceReference;
				}

				quantities.Add(new RepeatedQuantity(new Quantity(unitCount, unit, reference: reference), info.ServiceCount, info.ContainerCount));
			}
		}

		static decimal ConvertToQuantityUnits(TimeSpan duration, string unit)
		{
			switch (unit)
			{
				case QuantityUnit.HR:
					return (decimal)duration.TotalHours;
				case QuantityUnit.DY:
					return Math.Ceiling((decimal)duration.TotalDays);
				case QuantityUnit.WK:
					return Math.Ceiling((decimal)duration.TotalDays / 7);
				default:
					return 0;
			}
		}
	}
}


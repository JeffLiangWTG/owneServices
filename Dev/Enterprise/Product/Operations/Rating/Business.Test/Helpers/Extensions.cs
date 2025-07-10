using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Testing
{
	public static class Extensions
	{
		#region RateEntry

		public static RateLine AddPerUnitCharge(this RateEntry entry, string chargeCode, decimal rate, string unit = "KG", decimal minimum = 0, string unitFactor = null)
		{
			var line = entry.AddRateLine(chargeCode, MinimumOrPerUnitCalculator.Code, unit);
			line.TL_UnitFactor = unitFactor;
			var calc = line.GetCalculator<MinimumOrPerUnitCalculator>();
			calc.Minimum = minimum;
			calc.PerUnit = rate;

			return line;
		}

		/// <summary>
		/// Add unit calculator charge.
		/// Differs from the above AddPerUnitCharge method which adds MinimumOrPerUnitCalculator.
		/// </summary>
		public static RateLine AddUnitCharge(this RateEntry entry, string chargeCode, decimal rate, string unit = "KG")
		{
			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, unit);
			var calc = line.GetCalculator<UnitCalculator>();
			calc.PerUnit = rate;

			return line;
		}

		public static RateLine AddFlatCharge(this RateEntry entry, string chargeCode, decimal rate)
		{
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = rate;

			return line;
		}

		public static RateLine AddPercentageCharge(this RateEntry entry, string chargeCode, AccChargeCode chargeCodeAppliesTo, decimal percentage)
		{
			var line = entry.AddRateLine(chargeCode, PercentageCalculator.Code);
			var calc = line.GetCalculator<PercentageCalculator>();
			calc.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = chargeCodeAppliesTo.PK;
			calc.Percent = percentage;

			return line;
		}

		public static RateLine AddPercentageCharge(this RateEntry entry, string chargeCode, string appliesTo, decimal percentage)
		{
			var line = entry.AddRateLine(chargeCode, PercentageCalculator.Code);
			var calc = line.GetCalculator<PercentageCalculator>();
			calc.AddApplyToItem(appliesTo);
			calc.Percent = percentage;

			return line;
		}

		public static RateLine AddCMBCalculatorAccumulated(this RateEntry entry, string chargeCode)
		{
			var line = entry.AddRateLine(chargeCode, CombinedCalculator.Code, Weight.Kilograms);
			line.Calculator.IsAccumulated = true;

			return line;
		}

		public static RateLine AddHighestRateCharge(this RateEntry rateEntry, string chargeCode, decimal weightRate, string weightUnit, decimal volumeRate, string volumeUnit)
		{
			var hrcLine = rateEntry.AddRateLine(chargeCode, HighestRateCalculator.Code);
			var hrcCalc = hrcLine.GetCalculator<HighestRateCalculator>();
			var hrcCalcVolumeLineItem = hrcCalc.AddRateLineItem(Calculator.Items.Operator.UNT, 0m, volumeRate, 0m);
			hrcCalcVolumeLineItem.TM_BreakWeightVolume = volumeUnit;

			var hrcCalcWeightLineItem = hrcCalc.AddRateLineItem(Calculator.Items.Operator.UNT, 0m, weightRate, 0m);
			hrcCalcWeightLineItem.TM_BreakWeightVolume = weightUnit;

			return hrcLine;
		}

		#endregion

		#region RateLine

		public static RateLine AddPerUnitCharge(this RateLine line, string chargeCode, decimal rate, string unit = "KG", decimal minimum = 0, string unitFactor = null)
		{
			return AddPerUnitCharge((RateEntry)line.ParentRateEntry, chargeCode, rate, unit, minimum, unitFactor);
		}

		public static RateLine AddUnitCharge(this RateLine line, string chargeCode, decimal rate, string unit = "KG")
		{
			return AddUnitCharge(((RateEntry)line.ParentRateEntry), chargeCode, rate, unit);
		}

		public static RateLine AddFlatCharge(this RateLine line, string chargeCode, decimal rate)
		{
			return AddFlatCharge((RateEntry)line.ParentRateEntry, chargeCode, rate);
		}

		public static RateLine AddPercentageCharge(this RateLine line, string chargeCode, AccChargeCode chargeCodeAppliesTo, decimal percentage)
		{
			return AddPercentageCharge((RateEntry)line.ParentRateEntry, chargeCode, chargeCodeAppliesTo, percentage);
		}

		public static RateLine AddSlidingPlusRateLine(this RateLine line, ZDecimal breakAmount, ZDecimal relevantValue, ZDecimal flatAmount)
		{
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, breakAmount, relevantValue, flatAmount);
			return line;
		}

		public static RateLine AddSlidingMinusRateLine(this RateLine line, ZDecimal breakAmount, ZDecimal relevantValue, ZDecimal flatAmount)
		{
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, breakAmount, relevantValue, flatAmount);
			return line;
		}

		public static RateLine SetBreaksPerAsContainerTypeOrClass(this RateLine line)
		{
			line.Calculator.BreaksPer = Calculator.Items.BreaksPerContainerTypeOrClass;
			return line;
		}

		public static RateLine SetBreaksPerAsContainer(this RateLine line)
		{
			line.Calculator.BreaksPer = Calculator.Items.BreaksPerContainer;
			return line;
		}

		public static RateEntry AddRateEntry(this RateLine line,
			ZString category,
			string mode = "",
			string origin = "",
			string destination = "",
			string serviceLevel = "",
			string container = "",
			string commodity = "",
			bool removeLines = false,
			ZDate startDate = default,
			ZDate endDate = default)
		{
			return line.Parent.Parent.AddRateEntry(category, mode, origin, destination, serviceLevel, container, commodity, removeLines, startDate, endDate);
		}

		#endregion

		#region BusinessObjectFactory

		public static AccChargeCode GetChargeCode(this BusinessObjectFactory factory, string chargeCode)
		{
			var filter = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var code = factory.LoadTop1<AccChargeCode>(filter);
			return code;
		}

		#endregion

		#region Task

		/// <summary>
		/// Waits until the given task has completed execution by
		/// polling on the main thread instead of blocking it completely.
		///
		/// Only to be used in tests. Not for production code.
		/// </summary>
		/// <param name="doEvents">
		/// Please pass in Application.DoEvents here. It needs to be this way because otherwise DAT
		/// thinks this class is in production code and complains about calling
		/// Appilcation.DoEvents.
		/// </param>
		public static void SpinWait(this Task task, Action doEvents)
		{
			using (var semaphore = new ManualResetEvent(false))
			{
				var waitTask = task.ContinueWith((prevTask) => semaphore.Set());

				while (!semaphore.WaitOne(TimeSpan.FromMilliseconds(100)))
				{
					doEvents();
				}
			}
		}

		#endregion
	}
}

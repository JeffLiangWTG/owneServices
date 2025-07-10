using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	//Recon Declaration
	public interface IReconInterestDataProviderHost : IReconInterestDataProvider
	{
		ZBool IsAggregate { get; }
		IEnumerable<IReconInterestDataProvider> ReconInterestData { get; }
	}

	//Recon Original Entry
	public interface IReconInterestDataProvider
	{
		ZDate OriginalPaymentDate { get; }
		ZDate ReconPaymentDate { get; }
		ZDecimal OriginalPayable { get; }
		ZDecimal ReconPayable { get; }
		ZBool HasBeenUnderPaid { get; }

		void UpdateOrAddInterestCharge(ZDecimal amount);
	}

	/// <summary>
	/// This should run after new duty & fee are calculated
	/// </summary>
	class ReconInterestCalculator
	{
		public ReconInterestCalculator(IReconInterestDataProviderHost dataHost)
		{
			this.dataHost = dataHost;
		}

		readonly IReconInterestDataProviderHost dataHost;

		public void Execute()
		{
			Execute(dataHost.ReconInterestData);
		}

		public void Execute(IEnumerable<IReconInterestDataProvider> dataProviders)
		{
			dataHost.UpdateOrAddInterestCharge(0m);

			foreach (IReconInterestDataProvider dataProvider in dataProviders)
			{
				dataProvider.UpdateOrAddInterestCharge(0m);
			}

			if (dataHost.IsAggregate)
			{
				CalculateForDataProvider(dataHost);
			}
			else
			{
				foreach (IReconInterestDataProvider dataProvider in dataProviders)
				{
					CalculateForDataProvider(dataProvider);
				}
			}
		}

		internal ZDecimal CalculateForDataProvider(IReconInterestDataProvider dataProvider)
		{
			//Interest should be calculated and sent in messages only on underpayment. Interest on refund is calculated by Customs

			ZDecimal result = 0m;
			if (dataProvider.HasBeenUnderPaid)
			{
				result = CalculateInterest(dataProvider);

				dataProvider.UpdateOrAddInterestCharge(result);
			}

			return result;
		}

		ZDecimal CalculateInterest(IReconInterestDataProvider dataProvider)
		{
			ZDecimal result = 0m;

			ZDate startDate = dataProvider.OriginalPaymentDate;
			ZDate reconPayDate = dataProvider.ReconPaymentDate;

			ZDecimal principal = dataProvider.ReconPayable - dataProvider.OriginalPayable;

			if (principal > 0)
			{
				while (startDate <= reconPayDate)
				{
					ZDate endOfQuarter = GetEndDateOfAQuater(startDate);
					ZDate endDate = endOfQuarter < reconPayDate ? endOfQuarter : reconPayDate;

					ZDecimal interestCalculated = CalculateInterestForOneQuater(principal, startDate, endDate);
					interestCalculated = interestCalculated.Round(2);

					principal += interestCalculated;
					result += interestCalculated;

					startDate = endDate.AddDays(1);
				}
			}

			return result;
		}

		const int numberOfDigitsToUse = 8;

		decimal CalculateInterestForOneQuater(ZDecimal principal, ZDate startDate, ZDate endDate)
		{
			ZDecimal interestRate = GetInterestRate(startDate);

			int numberOfDays = (endDate - startDate).Days + 1;
			int numberOfDaysInYear = DateTime.IsLeapYear(startDate.Year) ? 366 : 365;

			ZDecimal dailyInterestRate = interestRate / numberOfDaysInYear;

			double compoundedRateForDays = Math.Pow(decimal.ToDouble(1 + dailyInterestRate.Round(numberOfDigitsToUse)), numberOfDays);

			ZDecimal finalInterestRate = compoundedRateForDays - 1;

			return (decimal)principal * finalInterestRate.Round(numberOfDigitsToUse);
		}

		protected virtual ZDecimal GetInterestRate(ZDate startDate)
		{
			ZDecimal result = ReconInterestRateRetriever.GetRate(startDate);

			return result / 100;
		}

		/// <summary>
		/// Get the nearest future among '31 Mar' or '30 Jun' or '30 Sep' or '31 Dec' of the year
		/// </summary>
		public static ZDate GetEndDateOfAQuater(ZDate startDate)
		{
			QuarterMonth month = 0;

			if (startDate.Month <= (int)QuarterMonth.March)
			{
				month = QuarterMonth.March;
			}
			else if (startDate.Month <= (int)QuarterMonth.June)
			{
				month = QuarterMonth.June;
			}
			else if (startDate.Month <= (int)QuarterMonth.September)
			{
				month = QuarterMonth.September;
			}
			else
			{
				month = QuarterMonth.December;
			}

			int day = month == QuarterMonth.June || month == QuarterMonth.September ? 30 : 31;

			return new ZDate(startDate.Year, (int)month, day);
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive("Instantiated through reflection")]
		enum QuarterMonth { March = 3, June = 6, September = 9, December = 12 }
	}
}

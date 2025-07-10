using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using SortDirection = System.ComponentModel.ListSortDirection;

namespace Enterprise.Customs.US.Business
{
	public class OGAInvValueConverter
	{
		/// <summary>
		/// Convert from OGA value in Inv Curr to an FOB value in local curr 
		/// </summary>
		/// <param name="declaration"></param>
		public void Convert(JobDeclaration declaration)
		{
			var toCurrency = JobDeclaration.GetLocalCurrency();

			var fdaArgs = GetOGAArgs(invoiceLine => invoiceLine.FDAs,
				fda => ((FDA)fda).US_InvCurrFDAValueInfo,
				fda => ((FDA)fda).US_FDAValueInfo, 0,
				invoiceLine => invoiceLine.JI_LinePrice,
				invoiceLine => invoiceLine.GetEnteredValueForOGA());

			var pgaArgs = GetOGAArgs(invoiceLine => invoiceLine.LaceyActLines,
				pga => ((PGA)pga).US_InvCurrPGAValueInfo,
				pga => ((PGA)pga).US_PGALineValueInfo, 0,
				invoiceLine => invoiceLine.JI_LinePrice,
				invoiceLine => invoiceLine.GetEnteredValueForOGA());

			var fwsArgs = GetOGAArgs(invoiceLine => invoiceLine.FWSHeaders,
				pga => ((FWSHeader)pga).US_InvCurrPGAValueInfo,
				pga => ((FWSHeader)pga).US_ValueInfo, 0,
				invoiceLine => invoiceLine.JI_LinePrice,
				invoiceLine => invoiceLine.GetEnteredValueForOGA());

			var aceFDAArgs = GetOGAArgs(invoiceLine => invoiceLine.ACE_FDALines,
				fda => ((ACEFDA)fda).US_InvCurrValueInfo,
				fda => ((ACEFDA)fda).US_TotalValueInfo, 0,
				invoiceLine => invoiceLine.JI_LinePrice,
				invoiceLine => invoiceLine.GetEnteredValueForOGA());

			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				if (invoiceLine.InvoiceHeader != null)
				{
					var invoiceCurr = invoiceLine.InvoiceHeader.Invoice_Currency;

					if (invoiceCurr != null)
					{
						var customsValue = invoiceLine.GetEnteredValueForOGA();

						var linePriceInUSD = invoiceLine.JI_LinePriceInLocalCurrency;
						var customsFactor = linePriceInUSD > 0m ? customsValue / linePriceInUSD : 0m;

						CalculateCore(invoiceLine, fdaArgs, invoiceCurr, toCurrency, customsFactor);
						invoiceLine.FDAValueUSDRunningTotalStringInfo.RefreshBinding();
						CalculateCore(invoiceLine, pgaArgs, invoiceCurr, toCurrency, customsFactor);
						CalculateCore(invoiceLine, aceFDAArgs, invoiceCurr, toCurrency, customsFactor);
						CalculateCore(invoiceLine, fwsArgs, invoiceCurr, toCurrency, customsFactor);
					}
				}
			}
		}

		void CalculateCore(JobComInvoiceLine invoiceLine, OGAInvValueConverterArgs args, RefCurrency fromCurr, RefCurrency toCurr, decimal factor)
		{
			var totalOfRoundedToValue = ZDecimal.Zero;
			var totalRoundedOfSumOfUnroundedToValue = ZDecimal.Zero;
			var totalFromValue = ZDecimal.Zero;
			var calculatedValues = new Dictionary<ZGuid, ZDecimal>();

			foreach (var oga in args.GetOGAs(invoiceLine))
			{
				var fromValue = (ZDecimal)args.GetOGAFromValueProperty(oga).Value;
				totalFromValue += fromValue;

				var fromValueConvertedMoney = new Money(fromValue * (factor > 0m ? factor : 1m), fromCurr);

				var toValueInToCurr = invoiceLine.CurrencyConverter.ConvertExact(fromValueConvertedMoney, toCurr).Amount;
				calculatedValues.Add(oga.PK, toValueInToCurr);

				totalOfRoundedToValue += toValueInToCurr.Round(args.NumberOfDecimalsToRound);
				totalRoundedOfSumOfUnroundedToValue += toValueInToCurr;
			}

			totalRoundedOfSumOfUnroundedToValue = totalRoundedOfSumOfUnroundedToValue.Round(args.NumberOfDecimalsToRound);

			if (totalFromValue == args.GetTotalToCompare(invoiceLine))
			{
				totalRoundedOfSumOfUnroundedToValue = Math.Min(totalRoundedOfSumOfUnroundedToValue, args.GetTotalToBalanceAgainst(invoiceLine));
			}

			var linesToFix = new List<BusinessObject>(args.GetOGAs(invoiceLine).Cast<BusinessObject>());

			if (totalOfRoundedToValue != totalRoundedOfSumOfUnroundedToValue)
			{
				var fixMethod = totalOfRoundedToValue > totalRoundedOfSumOfUnroundedToValue ? USCustomsValueRoundingTool.RoundingIssueFixMethod.Truncate : USCustomsValueRoundingTool.RoundingIssueFixMethod.RoundUp;
				var sortDirection = totalOfRoundedToValue > totalRoundedOfSumOfUnroundedToValue ? SortDirection.Ascending : SortDirection.Descending;

				linesToFix.Sort(new ComparerForRounding(sortDirection, calculatedValues));

				while (totalOfRoundedToValue != totalRoundedOfSumOfUnroundedToValue && linesToFix.Count > 0)
				{
					var oga = linesToFix[0];

					linesToFix.Remove(oga);
					ApplyRounding(args, oga, fixMethod, calculatedValues);

					totalOfRoundedToValue = calculatedValues.Sum(x => x.Value.Round(args.NumberOfDecimalsToRound));
				}
			}

			linesToFix.ForEach(oga => ApplyRounding(args, oga, USCustomsValueRoundingTool.RoundingIssueFixMethod.None, calculatedValues));
		}

		void ApplyRounding(OGAInvValueConverterArgs args, BusinessObject oga, USCustomsValueRoundingTool.RoundingIssueFixMethod fixMethod, Dictionary<ZGuid, ZDecimal> calculatedValues)
		{
			var enteredValue = calculatedValues[oga.PK];

			if (fixMethod == USCustomsValueRoundingTool.RoundingIssueFixMethod.Truncate)
			{
				enteredValue = args.NumberOfDecimalsToRound == 0 ? enteredValue.Truncate() : (ZDecimal)(enteredValue - 0.01m);
			}
			else if (fixMethod == USCustomsValueRoundingTool.RoundingIssueFixMethod.RoundUp)
			{
				enteredValue = args.NumberOfDecimalsToRound == 0 ? enteredValue.Truncate() + 1m : enteredValue + 0.01m;
			}
			else
			{
				enteredValue = enteredValue.Round(args.NumberOfDecimalsToRound);
			}

			calculatedValues[oga.PK] = enteredValue;
			args.GetOGAToValueProperty(oga).Value = enteredValue;
		}

		class ComparerForRounding : IComparer<BusinessObject>
		{
			public ComparerForRounding(SortDirection sortDirection, Dictionary<ZGuid, ZDecimal> calculatedValues)
			{
				this.sortDirection = sortDirection;
				this.calculatedValues = calculatedValues;
			}

			readonly SortDirection sortDirection;
			readonly Dictionary<ZGuid, ZDecimal> calculatedValues;

			#region IComparer<FDA> Members

			int IComparer<BusinessObject>.Compare(BusinessObject x, BusinessObject y)
			{
				var xEnteredValue = calculatedValues.GetValueSafe(x?.PK ?? ZGuid.Empty);
				var yEnteredValue = calculatedValues.GetValueSafe(y?.PK ?? ZGuid.Empty);

				ZDecimal xDecimals = xEnteredValue - xEnteredValue.Truncate();
				ZDecimal yDecimals = yEnteredValue - yEnteredValue.Truncate();

				int result;

				if (sortDirection == SortDirection.Ascending)
				{
					result = xDecimals.CompareTo(yDecimals);
				}
				else
				{
					result = yDecimals.CompareTo(xDecimals);
				}

				return result;
			}

			#endregion
		}

		/// <summary>
		/// This is triggered by an online upgrade
		/// This converts from an entered value in USD to a value in Inv. Curr in its INCOTERM
		/// </summary>
		/// <param name="declaration"></param>
		public void ConvertFromEnteredValueToInvValue(JobDeclaration declaration)
		{
			var fdaArgs = GetOGAArgs(invoiceLine => invoiceLine.FDAs,
				fda => ((FDA)fda).US_FDAValueInfo,
				fda => ((FDA)fda).US_InvCurrFDAValueInfo, 2,
				invoiceLine => invoiceLine.GetEnteredValueForOGA(),
				invoiceLine => invoiceLine.JI_LinePrice);

			var pgaArgs = GetOGAArgs(invoiceLine => invoiceLine.LaceyActLines,
				pga => ((PGA)pga).US_PGALineValueInfo,
				pga => ((PGA)pga).US_InvCurrPGAValueInfo, 0,
				invoiceLine => invoiceLine.GetEnteredValueForOGA(),
				invoiceLine => invoiceLine.JI_LinePrice);

			var fromCurr = JobDeclaration.GetLocalCurrency();
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				var toCurr = invoiceLine.InvoiceHeader.Invoice_Currency;

				if (toCurr != null)
				{
					var customsValue = invoiceLine.GetEnteredValueForOGA();

					var linePriceInUSD = invoiceLine.JI_LinePriceInLocalCurrency;
					var factor = customsValue > 0m ? linePriceInUSD / customsValue : 0m;

					CalculateCore(invoiceLine, fdaArgs, fromCurr, toCurr, factor);
					CalculateCore(invoiceLine, pgaArgs, fromCurr, toCurr, factor);
				}
			}
		}

		#region Args

		class OGAInvValueConverterArgs
		{
			public Func<JobComInvoiceLine, IEnumerable<BusinessObject>> GetOGAs;
			public Func<BusinessObject, ZPropertyInfo> GetOGAFromValueProperty;
			public Func<BusinessObject, ZPropertyInfo> GetOGAToValueProperty;
			public int NumberOfDecimalsToRound;
			public Func<JobComInvoiceLine, ZDecimal> GetTotalToCompare;
			public Func<JobComInvoiceLine, ZDecimal> GetTotalToBalanceAgainst;
		}

		OGAInvValueConverterArgs GetOGAArgs(Func<JobComInvoiceLine, IEnumerable<BusinessObject>> getOGAs, Func<BusinessObject, ZPropertyInfo> getOGAFromValueProperty, Func<BusinessObject, ZPropertyInfo> getOGAToValueProperty, int numberOfDecimalsToRound, Func<JobComInvoiceLine, ZDecimal> getTotalToCompare, Func<JobComInvoiceLine, ZDecimal> getTotalToBalanceAgainst)
		{
			var result = new OGAInvValueConverterArgs();

			result.GetOGAs = getOGAs;
			result.GetOGAFromValueProperty = getOGAFromValueProperty;
			result.GetOGAToValueProperty = getOGAToValueProperty;
			result.NumberOfDecimalsToRound = numberOfDecimalsToRound;
			result.GetTotalToCompare = getTotalToCompare;
			result.GetTotalToBalanceAgainst = getTotalToBalanceAgainst;

			return result;
		}

		#endregion
	}
}

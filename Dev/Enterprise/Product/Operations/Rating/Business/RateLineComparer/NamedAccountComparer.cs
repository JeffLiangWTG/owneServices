using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class NamedAccountComparer : BaseRateLineComparer
	{
		public NamedAccountComparer(string jobNamedAccount)
		{
			JobNamedAccount = jobNamedAccount.Trim().ToUpper(CultureInfo.InvariantCulture);
		}

		readonly string JobNamedAccount;

		public override int Compare(FastLine line1, FastLine line2)
		{
			var result = 0;
			var line1NamedAccounts = GetNamedAccountsFromLineAndAddBlankIfListIsEmpty(line1);
			var line2NamedAccounts = GetNamedAccountsFromLineAndAddBlankIfListIsEmpty(line2);

			var line1Matched = line1NamedAccounts.Contains(JobNamedAccount);
			var line2Matched = line2NamedAccounts.Contains(JobNamedAccount);

			if (line1Matched || line2Matched)
			{
				result += line1Matched ? 1 : 0;
				result -= line2Matched ? 1 : 0;
				return result;
			}

			result += line1NamedAccounts.Contains(string.Empty) ? 1 : 0;
			result -= line2NamedAccounts.Contains(string.Empty) ? 1 : 0;

			return result;
		}

		protected override string GetName()
		{
			return (NoResString)"Named Account"; // log message
		}

		protected override string GetReasonCore(FastLine overriddenLine, FastLine overriddenBy)
		{
			var overriddenLineNACs = string.Join(", ", GetNamedAccountsFromLineAndAddBlankIfListIsEmpty(overriddenLine));
			var overriddenByNACs = string.Join(", ", GetNamedAccountsFromLineAndAddBlankIfListIsEmpty(overriddenBy));

			return ZString.Format((NoResString)"Job NAC '{0}'. '{1}' has higher priority than '{2}'.", EmptyOrString(JobNamedAccount), EmptyOrString(overriddenByNACs), EmptyOrString(overriddenLineNACs)); // log message
		}

		static List<string> GetNamedAccountsFromLineAndAddBlankIfListIsEmpty(FastLine rateLine)
		{
			var namedAccounts = GetNamedAccountsFromLine(rateLine);
			if (!namedAccounts.Any())
			{
				namedAccounts.Add(string.Empty);
			}

			return namedAccounts;
		}

		public static List<string> GetNamedAccountsFromLine(FastLine rateLine)
		{
			var namedAccounts = rateLine.ParentRateEntry.NamedAccounts?.ToList() ?? new List<string>();
			namedAccounts.ForEach(x => x = x.Trim().ToUpper(CultureInfo.InvariantCulture));

			return namedAccounts;
		}

		static Func<string, string> EmptyOrString => (s => string.IsNullOrEmpty(s) ? (NoResString)"empty" : s); // part of formatted log message
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[Flags]
	public enum AccExRateConfigurationLevelEnum
	{
		None = 0,
		System = 1,
		Company = 2,
		DebtorGroup = 4,
		CreditorGroup = 8,
		Debtor = 16,
		Creditor = 32
	}

	static class AccExchangeRateConfigurationExtensions
	{
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		internal static string ToTablePrefix(this AccExRateConfigurationLevelEnum level)
		{
			switch (level)
			{
				case AccExRateConfigurationLevelEnum.Debtor:
				case AccExRateConfigurationLevelEnum.Creditor:
					return OrgHeaderSchema.Constants.Prefix;

				case AccExRateConfigurationLevelEnum.DebtorGroup:
					return OrgDebtorGroupSchema.Constants.Prefix;

				case AccExRateConfigurationLevelEnum.CreditorGroup:
					return OrgCreditorGroupSchema.Constants.Prefix;

				case AccExRateConfigurationLevelEnum.Company:
				case AccExRateConfigurationLevelEnum.System:
				case AccExRateConfigurationLevelEnum.None:
					return string.Empty;

				default:
					throw new InvalidOperationException($"Unknown level value {level}");
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		internal static string ToLedgerType(this AccExRateConfigurationLevelEnum level)
		{
			switch (level)
			{
				case AccExRateConfigurationLevelEnum.Debtor:
				case AccExRateConfigurationLevelEnum.DebtorGroup:
					return LedgerTypes.AccountsReceivable;

				case AccExRateConfigurationLevelEnum.Creditor:
				case AccExRateConfigurationLevelEnum.CreditorGroup:
					return LedgerTypes.AccountsPayable;

				case AccExRateConfigurationLevelEnum.Company:
				case AccExRateConfigurationLevelEnum.System:
				case AccExRateConfigurationLevelEnum.None:
					return string.Empty;

				default:
					throw new InvalidOperationException($"Unknown level value {level}");
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		internal static string GetLevelName(this AccExRateConfigurationLevelEnum level)
		{
			switch (level)
			{
				case AccExRateConfigurationLevelEnum.None:
					return Res.GetString("9d800791-e5aa-4ad8-8c08-5785c9bdd2a7", "None");
				case AccExRateConfigurationLevelEnum.System:
					return Res.GetString("2ae145f2-239f-42ea-b6c4-d42664709c27", "System");
				case AccExRateConfigurationLevelEnum.Company:
					return Res.GetString("165b8071-a22b-47b3-85f1-322cdf87ef20", "Company");
				case AccExRateConfigurationLevelEnum.DebtorGroup:
					return Res.GetString("0b41088a-b190-4330-99e4-d2073874dbf4", "Debtor Group");
				case AccExRateConfigurationLevelEnum.CreditorGroup:
					return Res.GetString("0558d778-03f4-4542-ad2f-3186104deaa4", "Creditor Group");
				case AccExRateConfigurationLevelEnum.Debtor:
					return Res.GetString("dc001787-5dd2-4e3e-ab42-0ee4f9838fb4", "Debtor");
				case AccExRateConfigurationLevelEnum.Creditor:
					return Res.GetString("4f7f5ed4-80b1-4110-898d-0c03cf61e405", "Creditor");
				default:
					throw new InvalidOperationException($"Unknown level value {level}");
			}
		}

		internal static IEnumerable<AccExRateConfigurationLevelEnum> GetThisAndUpperLevels(this AccExRateConfigurationLevelEnum level)
		{
			yield return level;
			switch (level)
			{
				case AccExRateConfigurationLevelEnum.CreditorGroup:
				case AccExRateConfigurationLevelEnum.DebtorGroup:
					{
						yield return AccExRateConfigurationLevelEnum.Company;
						yield return AccExRateConfigurationLevelEnum.System;
						break;
					}
				case AccExRateConfigurationLevelEnum.Debtor:
					{
						yield return AccExRateConfigurationLevelEnum.DebtorGroup;
						yield return AccExRateConfigurationLevelEnum.Company;
						yield return AccExRateConfigurationLevelEnum.System;
						break;
					}
				case AccExRateConfigurationLevelEnum.Creditor:
					{
						yield return AccExRateConfigurationLevelEnum.CreditorGroup;
						yield return AccExRateConfigurationLevelEnum.Company;
						yield return AccExRateConfigurationLevelEnum.System;
						break;
					}
				case AccExRateConfigurationLevelEnum.Company:
					{
						yield return AccExRateConfigurationLevelEnum.System;
						break;
					}
			}
		}
	}
}

using System.Collections.Generic;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public interface IACCase
	{
		ZString CaseNumber { get; }
		ZString CountryCode { get; }
		bool IsEffective { get; }
		bool IsReportable(ZDateTime effetiveDate);
		bool IsCashRequired(ZDate effetiveDate);
		ZString ManufacturerIDCode { get; }

		ZString CaseStatus { get; }
		CodeDescriptionPairList CaseStatusList { get; }
		IEnumerable<ZString> RelatedTariffs { get; }
	}

	static class IACCaseExtensionMethods
	{
		public static bool MatchesTariff(this IACCase caseRecord, ZString tariff)
		{
			bool result = false;

			if (caseRecord != null)
			{
				foreach (ZString dumpingTariff in caseRecord.RelatedTariffs)
				{
					ZString truncatedTariff = tariff.SubstringSafe(0, dumpingTariff.Length);
					result = dumpingTariff == truncatedTariff;
					if (result)
					{
						break;
					}
				}
			}

			return result;
		}
	}
}

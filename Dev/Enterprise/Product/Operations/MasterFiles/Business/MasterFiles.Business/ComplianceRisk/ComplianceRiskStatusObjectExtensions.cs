using System;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class ComplianceRiskStatusObjectExtensions
	{
		public static ZString GetOverallRiskDescription(this ComplianceRiskStatusObject risk) => GetRiskDescription(risk.JobRisk);

		public static ZString GetPartyRiskDescription(this ComplianceRiskStatusObject risk) => GetRiskDescription(risk.PartyRisk);

		public static ZString GetLocationRiskDescription(this ComplianceRiskStatusObject risk) => GetRiskDescription(risk.LocationRisk);

		public static ZString GetCommodityRiskDescription(this ComplianceRiskStatusObject risk) => GetRiskDescription(risk.CommodityRisk);

		[ThreadStatic]
		static ComplianceRiskStatusCodeList codeList;
		static ComplianceRiskStatusCodeList CodeList => codeList ??= new ComplianceRiskStatusCodeList();

		static ZString GetRiskDescription(ZString code)
		{
			return CodeList.GetDescriptionFromCode(code) ?? code;
		}
	}
}

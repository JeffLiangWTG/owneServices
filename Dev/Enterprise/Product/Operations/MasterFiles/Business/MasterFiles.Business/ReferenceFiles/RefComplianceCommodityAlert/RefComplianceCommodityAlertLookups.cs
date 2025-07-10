using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefComplianceCommodityAlertLookups : AutoRefComplianceCommodityAlertLookups
	{
		public RefComplianceCommodityAlertLookups(AutoRefComplianceCommodityAlert parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TradeDirections => Parent.Factory.GetCachedValue<CodeDescriptionPairList>("RefComplianceCommodityAlertLookups|TradeDirections", () => new RefComplianceCommodityAlertDirectionList());

		public CodeDescriptionPairList AlertTypes => Parent.Factory.GetCachedValue<CodeDescriptionPairList>("RefComplianceCommodityAlertLookups|AlertTypes", () => new RefComplianceCommodityAlertTypeList());

		public CodeDescriptionPairList CommodityRiskStatus => Parent.Factory.GetCachedValue<CodeDescriptionPairList>("RefComplianceCommodityAlertLookups|CommodityRiskStatus", () => GetCommodityRiskStatus());

		public static CodeDescriptionPairList GetCommodityRiskStatus() =>
			new CodeDescriptionPairList
			{
				new CodeDescriptionPair(ComplianceRiskStatusCodeList.Codes.HighRisk, ComplianceRiskStatusCodeList.Descriptions.HighRisk),
				new CodeDescriptionPair(ComplianceRiskStatusCodeList.Codes.PossibleRisk, ComplianceRiskStatusCodeList.Descriptions.PossibleRisk),
			};
	}
}

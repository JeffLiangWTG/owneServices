using System;

namespace CargoWise.Billing.Collectors.MasterData;

public class CmdOrgHeaderUsageCollector : CmdCollector
{
	protected override string QueryScript => @"
				org_header AS
				(
					SELECT
						o.OH_PK,
						CONVERT(VARBINARY(MAX), [OH_FullName]) CompanyName,
						OH_Category OrgCategory,
						NULLIF(OH_Language, 'EN') [Language],
						OH_RL_NKClosestPort PortCode,
						NULLIF(OH_IsActive, 1) IsActive,
						NULLIF(OH_IsGlobalAccount, 0) IsGlobalAccount,
						NULLIF(OH_IsNationalAccount, 0) IsNationalAccount,
						NULLIF(OH_IsForwarder, 0) IsForwarder,
						NULLIF(OH_IsShippingProvider, 0) IsShippingProvider,
						NULLIF(OH_IsAirWholesaler, 0) IsAirWholesaler,
						NULLIF(OH_IsSeaWholesaler, 0) IsSeaWholesaler,
						NULLIF(OH_IsRailProvider, 0) IsRailProvider,
						NULLIF(OH_IsLineHaulProvider, 0) IsLineHaulProvider,
						NULLIF(OH_IsMiscFreightServices, 0) IsMiscFreightServices,
						NULLIF(OH_IsAirCTO, 0) IsAirCTO,
						NULLIF(OH_IsAirLine, 0) IsAirLine,
						NULLIF(OH_IsBroker, 0) IsBroker,
						NULLIF(OH_IsContainerYard, 0) IsContainerYard,
						NULLIF(OH_IsLocalTransport, 0) IsLocalTransport,
						NULLIF(OH_IsPackDepot, 0) IsPackDepot,
						NULLIF(OH_IsSeaCTO, 0) IsSeaCTO,
						NULLIF(OH_IsShippingLine, 0) IsShippingLine,
						NULLIF(OH_IsUnpackDepot, 0) IsUnpackDepot,
						NULLIF(OH_IsRailHead, 0) IsRailHead,
						NULLIF(OH_IsRoadFreightDepot, 0) IsRoadFreightDepot,
						NULLIF(OH_IsShippingConsortium, 0) IsShippingConsortium,
						NULLIF(OH_IsFumigationContractor, 0) IsFumigationContractor,
						NULLIF(OH_IsSalesLead, 0) IsSalesLead,
						NULLIF(OH_IsCompetitor, 0) IsCompetitor,
						NULLIF(OH_IsTempAccount, 0) IsTempAccount,
						NULLIF(OH_IsPersonalEffectsAccount, 0) IsPersonalEffectsAccount,
						NULLIF(OH_IsConsignee, 0) IsConsignee,
						NULLIF(OH_IsConsignor, 0) IsConsignor,
						NULLIF(OH_IsTransportClient, 0) IsTransportClient,
						NULLIF(OH_IsWarehouseClient, 0) IsWarehouseClient,
						NULLIF(OH_IsDistributionCentre, 0) IsDistributionCentre,
						NULLIF(OH_IsControllingCustomer, 0) IsControllingCustomer,
						NULLIF(OH_IsControllingAgent, 0) IsControllingAgent,
						NULLIF(OH_IsFerryWaterTerminal, 0) IsFerryWaterTerminal,
						NULLIF(OH_IsContainerLeasingCompany, 0) IsContainerLeasingCompany,
						NULLIF(OH_IsInlandWaterwayProvider, 0) IsInlandWaterwayProvider,
						NULLIF(OH_IsVGMContractor, 0) IsVGMContractor,
						[OH_SystemLastEditTimeUtc] SystemLastEditTimeUtc
					FROM dbo.OrgHeader o
					JOIN gb_org_header h ON o.OH_PK = h.OH_PK
				)
";
	public override string FeatureCode => "CDO";
	public override string FeatureName => "Org Header Records";
	public override string GuidReference => "OH_PK";
	public override string FromClause => "org_header";
	public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(
		SELECT
			CompanyName, OrgCategory, Language, PortCode, IsActive, IsGlobalAccount, IsNationalAccount, IsForwarder,
			IsShippingProvider, IsAirWholesaler, IsSeaWholesaler, IsRailProvider, IsLineHaulProvider, IsMiscFreightServices,
			IsAirCTO, IsAirLine, IsBroker, IsContainerYard, IsLocalTransport, IsPackDepot, IsSeaCTO, IsShippingLine, IsUnpackDepot,
			IsRailHead, IsRoadFreightDepot, IsShippingConsortium, IsFumigationContractor, IsSalesLead, IsCompetitor, IsTempAccount,
			IsPersonalEffectsAccount, IsConsignee, IsConsignor, IsTransportClient, IsWarehouseClient, IsDistributionCentre,
			IsControllingCustomer, IsControllingAgent, IsFerryWaterTerminal, IsContainerLeasingCompany, IsInlandWaterwayProvider,
			IsVGMContractor, SystemLastEditTimeUtc
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)))";
}

using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrganisationFilterHelper
	{
		public OrganisationFilterHelper()
		{
		}

		#region SecondaryOrgTypeFilter

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZQuery GetSecondaryOrgTypeFilter(ZString value)
		{
			var secondaryOrgType = value;
			var query = new ZQuery();

			if (secondaryOrgType == OrganisationSecondaryTypes.ARQualityAssured)
			{
				query = GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsDebtor, OrgCompanyDataSchema.OB_ARQualityAssured);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.APQualityAssured)
			{
				query = GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsCreditor, OrgCompanyDataSchema.OB_APQualityAssured);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.CreditOnHold)
			{
				query = GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsDebtor, OrgCompanyDataSchema.OB_AROnCreditHold);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.IncludedInAutoRateUpdate)
			{
				query = GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsDebtor, OrgCompanyDataSchema.OB_ARAutoUpdateRates);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.NotIncludedInAutoRateUpdate)
			{
				query = GetCompanyDataQueryWith2Fields(OrgCompanyDataSchema.OB_IsDebtor, OrgCompanyDataSchema.OB_ARAutoUpdateRates, ZBool.False);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.LocalTransport)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsLocalTransport, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.ShippingLine)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsShippingLine, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.Airline)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsAirLine, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.Rail)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsRailProvider, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.InlandWaterway)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsInlandWaterwayProvider, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.AirWholesaler)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsAirWholesaler, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.SeaWholesaler)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsSeaWholesaler, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.LineHaul)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsLineHaulProvider, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.VesselConsortium)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsShippingConsortium, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.Principal)
			{
				query = GetForeignTableQueryForCompanyData(OrgCompanyDataSchema.OB_CRIsShipsAgencyPrincipal, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.HandlesAirFreight)
			{
				query = GetAgentPortsQuery(OrgAppointedAgentPortsSchema.O5_AirAgentStatus);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.HandlesSeaFreight)
			{
				query = GetAgentPortsQuery(OrgAppointedAgentPortsSchema.O5_SeaAgentStatus);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.HandlesRoadFreight)
			{
				query = GetAgentPortsQuery(OrgAppointedAgentPortsSchema.O5_RoadAgentStatus);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.HandlesRailFreight)
			{
				query = GetAgentPortsQuery(OrgAppointedAgentPortsSchema.O5_RailAgentStatus);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.Depot)
			{
				query = PackDepotQuery;
				query.AddToFilter(UnpackDepotQuery, JoinCondition.Or);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.PackingDepot)
			{
				query = PackDepotQuery;
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.UnpackingDepot)
			{
				query = UnpackDepotQuery;
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.CTO)
			{
				query = CTOQuery;
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.AirCTO)
			{
				query = AirCTOQuery;
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.DistributionCentre)
			{
				query = DistributionCentreQuery;
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.SeaCTO)
			{
				query = SeaCTOQuery;
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.RoadDepotTransitShed)
			{
				query = RoadDepotTransitShedQuery;
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.RailHeadDepot)
			{
				query = RailHeadDepotQuery;
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.FerryWaterTerminal)
			{
				query = FerryWaterTerminalQuery;
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.ContainerYard)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsContainerYard, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.FumigationContractor)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsFumigationContractor, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.ContainerLeasingCompany)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsContainerLeasingCompany, ZBool.True);
			}
			else if (secondaryOrgType == OrganisationSecondaryTypes.VGMContractor)
			{
				query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsVGMContractor, ZBool.True);
			}
			else
			{
				foreach (var userFlagType in OrgUserFlagType.All)
				{
					if (secondaryOrgType == GetSecondaryOrgTypeSalesLabel(userFlagType.Label))
					{
						query = new ZQuery(OrgHeaderSchema.OH_IsSalesLead, ZBool.True);
						query.AddToFilter(userFlagType.OrgHeaderColumn, ZBool.True);
						break;
					}
				}
			}

			return query;
		}

		protected ZQuery PackDepotQuery
		{
			get
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsPackDepot, ZBool.True);
				return query;
			}
		}

		protected ZQuery UnpackDepotQuery
		{
			get
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsUnpackDepot, ZBool.True);
				return query;
			}
		}

		protected ZQuery CTOQuery
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);

				var isCTOQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				isCTOQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsAirCTO, ZBool.True);
				isCTOQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsSeaCTO, ZBool.True);
				isCTOQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsRoadFreightDepot, ZBool.True);
				isCTOQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsRailHead, ZBool.True);
				isCTOQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsFerryWaterTerminal, ZBool.True);

				query.AddToFilter(isCTOQuery, JoinCondition.And);
				return query;
			}
		}

		protected ZQuery AirCTOQuery
		{
			get
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsAirCTO, ZBool.True);
				return query;
			}
		}

		ZQuery DistributionCentreQuery
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsMiscFreightServices, true);
				query.AddToFilter(OrgHeaderSchema.OH_IsDistributionCentre, true);
				return query;
			}
		}

		protected ZQuery SeaCTOQuery
		{
			get
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsSeaCTO, ZBool.True);
				return query;
			}
		}

		protected ZQuery RoadDepotTransitShedQuery
		{
			get
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsRoadFreightDepot, ZBool.True);
				return query;
			}
		}

		protected ZQuery RailHeadDepotQuery
		{
			get
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsRailHead, ZBool.True);
				return query;
			}
		}

		protected ZQuery FerryWaterTerminalQuery
		{
			get
			{
				ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, ZBool.True);
				query.AddToFilter(OrgHeaderSchema.OH_IsFerryWaterTerminal, ZBool.True);
				return query;
			}
		}

		protected ZDBOnlyQuery GetForeignTableQueryForCompanyData(SchemaColumn orgCompanyDataField, object value)
		{
			return GetForeignTableQueryForCompanyData(orgCompanyDataField, value, SQLComparisonOperator.Equal);
		}

		protected ZDBOnlyQuery GetForeignTableQueryForCompanyData(SchemaColumn orgCompanyDataField, object value, SQLComparisonOperator @operator)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(JoinCondition.And, orgCompanyDataField, @operator, value);
			subQuery.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected ZDBOnlyQuery GetCompanyDataQueryWith2Fields(SchemaColumn orgCompanyDataField, SchemaColumn companyDataField)
		{
			return GetForeignTableQueryForCompanyDataWith2Fields(orgCompanyDataField, companyDataField, ZBool.True);
		}

		protected ZDBOnlyQuery GetCompanyDataQueryWith2Fields(SchemaColumn orgCompanyDataField, SchemaColumn companyDataField, object companyDataValue)
		{
			return GetForeignTableQueryForCompanyDataWith2Fields(orgCompanyDataField, companyDataField, companyDataValue);
		}

		protected ZDBOnlyQuery GetForeignTableQueryForCompanyDataWith2Fields(SchemaColumn orgCompanyDataField, SchemaColumn foreignField, object foreignValue)
		{
			return GetForeignTableQueryForCompanyDataWith2Fields(orgCompanyDataField, foreignField, foreignValue, SQLComparisonOperator.Equal);
		}

		protected ZDBOnlyQuery GetForeignTableQueryForCompanyDataWith2Fields(SchemaColumn orgCompanyDataField, SchemaColumn foreignField, object foreignValue, SQLComparisonOperator @operator)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(orgCompanyDataField, ZBool.True);
			query.AddSubQuery(subQuery, JoinCondition.And);

			subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(JoinCondition.And, foreignField, @operator, foreignValue);
			subQuery.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		ZDBOnlyQuery GetAgentPortsQuery(SchemaStringColumn agentPortsField)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsForwarder, ZBool.True);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgAppointedAgentPorts), OrgAppointedAgentPortsSchema.O5_OH);

			subQuery.AddToFilter(agentPortsField, AgentStatusList.Codes.Handles);
			subQuery.AddToFilter(JoinCondition.Or, agentPortsField, AgentStatusList.Codes.Appointed);
			subQuery.AddToFilter(JoinCondition.Or, agentPortsField, AgentStatusList.Codes.Published);

			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		string GetSecondaryOrgTypeSalesLabel(string label)
		{
			return Res.GetString("8D2CEFAA-FB58-42D1-B23C-214D0DC08D5F", "Sales") + " - " + label;
		}

		public CodeDescriptionPairList GetSecondaryOrgTypes(OrgModuleType orgModuleType)
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			if (orgModuleType == OrgModuleType.CompanyCampaignContact)
			{
				list.AddPair(OrganisationSecondaryTypes.None, "");
			}

			if (orgModuleType == OrgModuleType.Standard || orgModuleType == OrgModuleType.CompanyCampaignContact)
			{
				list.AddPair(OrganisationSecondaryTypes.ARQualityAssured, "");
				list.AddPair(OrganisationSecondaryTypes.APQualityAssured, "");
				list.AddPair(OrganisationSecondaryTypes.IncludedInAutoRateUpdate, "");
				list.AddPair(OrganisationSecondaryTypes.NotIncludedInAutoRateUpdate, "");
				list.AddPair(OrganisationSecondaryTypes.CreditOnHold, "");
				list.AddPair(OrganisationSecondaryTypes.HandlesAirFreight, "");
				list.AddPair(OrganisationSecondaryTypes.HandlesSeaFreight, "");
				list.AddPair(OrganisationSecondaryTypes.HandlesRoadFreight, "");
				list.AddPair(OrganisationSecondaryTypes.HandlesRailFreight, "");
				list.AddPair(OrganisationSecondaryTypes.Depot, "");
				list.AddPair(OrganisationSecondaryTypes.DistributionCentre, "");
				list.AddPair(OrganisationSecondaryTypes.PackingDepot, "");
				list.AddPair(OrganisationSecondaryTypes.UnpackingDepot, "");
				list.AddPair(OrganisationSecondaryTypes.CTO, "");
				list.AddPair(OrganisationSecondaryTypes.AirCTO, "");
				list.AddPair(OrganisationSecondaryTypes.SeaCTO, "");
				list.AddPair(OrganisationSecondaryTypes.RoadDepotTransitShed, "");
				list.AddPair(OrganisationSecondaryTypes.RailHeadDepot, "");
				list.AddPair(OrganisationSecondaryTypes.FerryWaterTerminal, "");
				list.AddPair(OrganisationSecondaryTypes.ContainerYard, "");
				list.AddPair(OrganisationSecondaryTypes.FumigationContractor, "");
				list.AddPair(OrganisationSecondaryTypes.ContainerLeasingCompany, "");
				list.AddPair(OrganisationSecondaryTypes.VGMContractor, "");
				list.AddPair(OrganisationSecondaryTypes.LocalTransport, "");
				list.AddPair(OrganisationSecondaryTypes.ShippingLine, "");
				list.AddPair(OrganisationSecondaryTypes.Airline, "");
				list.AddPair(OrganisationSecondaryTypes.Rail, "");
				list.AddPair(OrganisationSecondaryTypes.InlandWaterway, "");
				list.AddPair(OrganisationSecondaryTypes.AirWholesaler, "");
				list.AddPair(OrganisationSecondaryTypes.SeaWholesaler, "");
				list.AddPair(OrganisationSecondaryTypes.LineHaul, "");
				list.AddPair(OrganisationSecondaryTypes.Principal, "");
				list.AddPair(OrganisationSecondaryTypes.VesselConsortium, "");
			}

			if (orgModuleType == OrgModuleType.ClientIntelligence && Env.Registry.OrgShowARTab)
			{
				list.AddPair(OrganisationSecondaryTypes.ARQualityAssured, "");
				list.AddPair(OrganisationSecondaryTypes.IncludedInAutoRateUpdate, "");
				list.AddPair(OrganisationSecondaryTypes.NotIncludedInAutoRateUpdate, "");
			}

			if (orgModuleType == OrgModuleType.ClientIntelligence || orgModuleType == OrgModuleType.Standard)
			{
				foreach (var orgUserFlagType in OrgUserFlagType.All)
				{
					list.AddPair(GetSecondaryOrgTypeSalesLabel(orgUserFlagType.Label));
				}
			}

			return list;
		}

		#endregion SecondaryOrgTypeFilter

		#region MarketingOptionsFilter

		public ZQuery GetMarketingOptionsFilter(ZString value)
		{
			foreach (var userFlagType in OrgUserFlagType.All)
			{
				if (value == userFlagType.Label)
				{
					var query = new ZQuery(userFlagType.OrgHeaderColumn, ZBool.True);
					return query;
				}
			}

			return new ZQuery();
		}

		public CodeDescriptionPairList GetMarketingOptions()
		{
			var list = new CodeDescriptionPairList();
			foreach (var orgUserFlagType in OrgUserFlagType.All)
			{
				list.AddPair(orgUserFlagType.Label, "");
			}

			return list;
		}

		#endregion

	}
}

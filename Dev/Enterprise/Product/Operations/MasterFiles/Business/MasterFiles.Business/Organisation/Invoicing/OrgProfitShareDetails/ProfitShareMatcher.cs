using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProfitShareMatcher
	{
		public ProfitShareMatcher(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));

			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public ColumnValueRanker GetRanker(ZString freightMode, ZString containerMode,
			ZString sendingLocationCode, ZString receivingLocationCode, OrganisationsWithTypes orgOverrides,
			OrgHeader controllingParty, string jobType = null, string gatewayAgentType = null,
			OrgProfitShareDetails[] profitSharesWithSkippedClientOverrideRanking = null,
			bool freightModeGroupagePriority = false)
		{
			var ranker = new ColumnValueRanker();

			// Only certain container modes are allowed for given freight modes. However this
			// restriction is not made explicit in this ProfitShareMatcher. Instead, it relies on
			// the job to ensure only valid combinations are present.
			if (freightModeGroupagePriority)
			{
				ranker.Add(OrgProfitShareDetailsSchema.O4_FreightMode, (ZString)Core.Constants.ContainerModes.Groupage, containerMode, freightMode, (ZString)OrgProfitShareDetailsLookups.FreightModesList.ALL.Code);
			}
			else
			{
				ranker.Add(OrgProfitShareDetailsSchema.O4_FreightMode, containerMode, freightMode, (ZString)OrgProfitShareDetailsLookups.FreightModesList.ALL.Code);
			}

				var sendingLocationCodeList = GetApplicableLocationCodeList(sendingLocationCode);
			ranker.AddEnumerable(OrgProfitShareDetailsSchema.O4_SendingPortOrCountry, sendingLocationCodeList);

			var receivingLocationCodeList = GetApplicableLocationCodeList(receivingLocationCode);
			ranker.AddEnumerable(OrgProfitShareDetailsSchema.O4_ReceivingPortOrCountry, receivingLocationCodeList);

			if (jobType == JobTypesList.Codes.GCN)
			{
				if (string.IsNullOrEmpty(gatewayAgentType))
				{
					throw new DeveloperNotificationException("Gateway consol should have at least one gateway agent.");
				}

				ranker.Add(OrgProfitShareDetailsSchema.O4_JobType, (ZString)jobType);

				switch (gatewayAgentType)
				{
					case GatewayAgentTypesList.Codes.BGW:
						ranker.Add(OrgProfitShareDetailsSchema.O4_GatewayAgentType, (ZString)GatewayAgentTypesList.Codes.BGW,
							(ZString)GatewayAgentTypesList.Codes.SGW, (ZString)GatewayAgentTypesList.Codes.RGW, ZString.Empty);
						break;
					case GatewayAgentTypesList.Codes.SGW:
						ranker.Add(OrgProfitShareDetailsSchema.O4_GatewayAgentType, (ZString)GatewayAgentTypesList.Codes.SGW, ZString.Empty);
						break;
					case GatewayAgentTypesList.Codes.RGW:
						ranker.Add(OrgProfitShareDetailsSchema.O4_GatewayAgentType, (ZString)GatewayAgentTypesList.Codes.RGW, ZString.Empty);
						break;
				}
			}
			else
			{
				ranker.Add(OrgProfitShareDetailsSchema.O4_JobType, (ZString)JobTypesList.Codes.SHP, ZString.Empty);
			}

			if (profitSharesWithSkippedClientOverrideRanking != null)
			{
				var isOrgOverrideAdded = false;
				if (orgOverrides != null)
				{
					var matchedOrgTypePairs = GetOrgTypePKPairsOnlyForOrgsMatchedByTypeInProfitShares(orgOverrides, profitSharesWithSkippedClientOverrideRanking);
					if (matchedOrgTypePairs.Any())
					{
						var orgPKsInFallbackOrder = matchedOrgTypePairs.Select(x => (IZType)x.Item2).ToList();
						orgPKsInFallbackOrder.Add(null);
						ranker.AddEnumerable(OrgProfitShareDetailsSchema.O4_OH_OrgOverride, orgPKsInFallbackOrder);

						var orgTypesInFallbackOrder = matchedOrgTypePairs.Select(x => (IZType)x.Item1).ToList();
						orgTypesInFallbackOrder.Add((ZString)OrgProfitShareDetailsLookups.OrgOverrideTypesList.ALL.Code);
						ranker.AddEnumerable(OrgProfitShareDetailsSchema.O4_OrgOverrideType, orgTypesInFallbackOrder);

						isOrgOverrideAdded = true;
					}
				}

				if (!isOrgOverrideAdded)
				{
					ranker.Add(OrgProfitShareDetailsSchema.O4_OH_OrgOverride, null, ZGuid.Empty);
				}
			}

			if (controllingParty != null)
			{
				ranker.Add(OrgProfitShareDetailsSchema.O4_OH_ControllingAgent, controllingParty.PK, null, ZGuid.Empty);
			}
			else
			{
				ranker.Add(OrgProfitShareDetailsSchema.O4_OH_ControllingAgent, null, ZGuid.Empty);
			}

			return ranker;
		}

		Tuple<ZString, ZGuid>[] GetOrgTypePKPairsOnlyForOrgsMatchedByTypeInProfitShares(OrganisationsWithTypes orgOverrides, IEnumerable<OrgProfitShareDetails> profitShares)
		{
			var requiredOrgTypePKPairs = orgOverrides.GetOrgTypePKPairs();
			var existingOrgTypePKPairs = profitShares.Select(x => new { x.O4_OrgOverrideType, x.O4_OH_OrgOverride });
			var matchedOrgTypePairs = from requiredPair in requiredOrgTypePKPairs
									  from existingPair in existingOrgTypePKPairs
									  where requiredPair.Item2 == existingPair.O4_OH_OrgOverride && (existingPair.O4_OrgOverrideType == requiredPair.Item1 || existingPair.O4_OrgOverrideType == OrgProfitShareDetailsLookups.OrgOverrideTypesList.ALL.Code)
									  select requiredPair;

			return matchedOrgTypePairs.ToArray();
		}

		// <summary>
		/// Location codes are giving priorities from high to low by their lengths:
		/// - An UNLOCO has length of 5
		/// - A city code has length of 3
		/// - A country code has length of 2
		/// - A zone (region) has length of 4
		/// - An empty string
		/// </summary>
		static IImmutableDictionary<int, int> LocationPrioritiesByCodeLength =>
			new Dictionary<int, int> { { 5, 5 }, { 3, 4 }, { 2, 3 }, { 4, 1 }, { 0, 0 } }
				.ToImmutableDictionary();

		static int CompareLocationCodesByLengths(ZString code1, ZString code2) =>
			LocationPrioritiesByCodeLength[code2.Length] - LocationPrioritiesByCodeLength[code1.Length];

		IEnumerable<IZType> GetApplicableLocationCodeList(ZString locationCode)
		{
			var location = LocationHelper.GetLocationFromString(locationCode, factory);
			var locationCodeSet = RatingZoneRetriever.GetApplicableLocationCodes(location, Array.Empty<OrgHeader>());
			locationCodeSet.Add(ZString.Empty);
			var locationCodeList = locationCodeSet.ToList();
			locationCodeList.Sort(CompareLocationCodesByLengths);

			return locationCodeList.Cast<IZType>();
		}

		public void ApplyProfitShareDetailsFilter(ZQuery query, ZDateTime jobDate, OrganisationsWithTypes orgOverrides)
		{
			query.AddToFilter(OrgProfitShareDetailsSchema.O4_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, jobDate);
			query.AddToFilter(OrgProfitShareDetailsSchema.O4_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, jobDate);

			if (orgOverrides != null)
			{
				var orgOverrideFilter = new ZQuery();
				orgOverrideFilter.DefaultJoinCondition = JoinCondition.Or;

				var orgTypePKPairs = orgOverrides.GetOrgTypePKPairs();
				foreach (var pair in orgTypePKPairs)
				{
					var typeOrgPairQuery = new ZQuery(OrgProfitShareDetailsSchema.O4_OrgOverrideType, pair.Item1);
					typeOrgPairQuery.AddToFilter(OrgProfitShareDetailsSchema.O4_OH_OrgOverride, pair.Item2);
					orgOverrideFilter.AddToFilter(typeOrgPairQuery);
				}

				var anyTypeOrgQuery = new ZQuery(OrgProfitShareDetailsSchema.O4_OrgOverrideType, OrgProfitShareDetailsLookups.OrgOverrideTypesList.ALL.Code);
				anyTypeOrgQuery.AddToFilter(OrgProfitShareDetailsSchema.O4_OH_OrgOverride, orgTypePKPairs.Select(x => x.Item2).ToArray());
				orgOverrideFilter.AddToFilter(anyTypeOrgQuery);

				orgOverrideFilter.AddToFilter(OrgProfitShareDetailsSchema.O4_OH_OrgOverride, null);

				query.AddToFilter(orgOverrideFilter);
			}
		}

		public IEnumerable<OrgProfitShareDetails> ApplyProfitShareDetailsFilter(IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList, ZDateTime jobDate, OrganisationsWithTypes orgOverrides)
		{
			var result = orgProfitShareDetailsList.Where(x => x.O4_StartDate.Date <= jobDate.Date && x.O4_EndDate.Date >= jobDate.Date);

			if (orgOverrides != null)
			{
				var orgTypePKPairs = orgOverrides.GetOrgTypePKPairs().ToDictionary(x => x.Item1, x => x.Item2);
				return result.Where(x
					=> (x.O4_OH_OrgOverride.IsEmpty
					|| x.O4_OrgOverrideType == OrgProfitShareDetailsLookups.OrgOverrideTypesList.ALL.Code && orgTypePKPairs.ContainsValue(x.O4_OH_OrgOverride))
					|| (orgTypePKPairs.TryGetValue(x.O4_OrgOverrideType, out ZGuid value) && value == x.O4_OH_OrgOverride));
			}

			return result;
		}
	}

	public class OrganisationsWithTypes
	{
		public OrganisationsWithTypes(OrgHeader localClient, IJobInvoicingSupporter invoicingSupporter = null)
		{
			LocalClient = localClient;
			if (invoicingSupporter != null)
			{
				Consignee = invoicingSupporter.Consignee;
				Consignor = invoicingSupporter.Consignor;
				PickUpAgent = invoicingSupporter.PickUpAgent;
				ImportBroker = invoicingSupporter.ImportBroker;
				ExportBroker = invoicingSupporter.ExportBroker;
			}
		}

		public OrgHeader LocalClient { get; set; }
		public OrgHeader Consignee { get; set; }
		public OrgHeader Consignor { get; set; }
		public OrgHeader PickUpAgent { get; set; }
		public OrgHeader ImportBroker { get; set; }
		public OrgHeader ExportBroker { get; set; }

		public Tuple<ZString, ZGuid>[] GetOrgTypePKPairs()
		{
			var pairs = new List<Tuple<ZString, ZGuid>>();
			if (LocalClient != null)
			{
				pairs.Add(new Tuple<ZString, ZGuid>(OrgProfitShareDetailsLookups.OrgOverrideTypesList.LOC.Code, LocalClient.PK));
			}
			if (Consignee != null)
			{
				pairs.Add(new Tuple<ZString, ZGuid>(OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNE.Code, Consignee.PK));
			}
			if (Consignor != null)
			{
				pairs.Add(new Tuple<ZString, ZGuid>(OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNR.Code, Consignor.PK));
			}
			if (PickUpAgent != null)
			{
				pairs.Add(new Tuple<ZString, ZGuid>(OrgProfitShareDetailsLookups.OrgOverrideTypesList.PUA.Code, PickUpAgent.PK));
			}
			if (ImportBroker != null)
			{
				pairs.Add(new Tuple<ZString, ZGuid>(OrgProfitShareDetailsLookups.OrgOverrideTypesList.IBR.Code, ImportBroker.PK));
			}
			if (ExportBroker != null)
			{
				pairs.Add(new Tuple<ZString, ZGuid>(OrgProfitShareDetailsLookups.OrgOverrideTypesList.EBR.Code, ExportBroker.PK));
			}

			return pairs.ToArray();
		}
	}
}

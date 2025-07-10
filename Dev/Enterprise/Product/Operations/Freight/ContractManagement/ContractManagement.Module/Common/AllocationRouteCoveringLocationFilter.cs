using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Module
{
	public class AllocationRouteCoveringLocationFilter : ModuleLocationFilter
	{
		protected AllocationRouteCoveringLocationFilter(
			ZString description,
			IBusinessObjectCollection locationList,
			SchemaStringColumn originLocation,
			SchemaStringColumn deliveryLocation
		)
			: base(description,
				originLocation,
				locationList,
				deliveryLocation,
				locationList,
				false)
		{
			SetDefaultFields();
		}

		public AllocationRouteCoveringLocationFilter(
			ZString description,
			IBusinessObjectCollection locationList
		)
			: base(description,
				  RatingContractAllocationLineSchema.RCA_LoadLocation,
				  locationList,
				  RatingContractAllocationLineSchema.RCA_DischargeLocation,
				  locationList,
				  false)
		{
			SetDefaultFields();
		}

		public bool ShowRelatedUNLOCOs
		{
			get => showRelatedUNLOCOs;
			set
			{
				if (showRelatedUNLOCOs != value)
				{
					showRelatedUNLOCOs = value;
					InvalidateCachedQuery();
				}
			}
		}

		void SetDefaultFields()
		{
			showRelatedUNLOCOs = true;
		}

		bool showRelatedUNLOCOs = true;
		protected virtual bool IncludeLinkedSchedules => true;

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var parentQuery = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));

			// Property1 = Load Value
			// Property2 = Discharge Value
			if (Property1.Length > 0)
			{
				AddLoadLocationFilters(parentQuery);
			}

			if (Property2.Length > 0)
			{
				AddDischargeLocationFilters(parentQuery);
			}

			return parentQuery;
		}

		void AddLoadLocationFilters(ZDBOnlyQuery parentQuery)
		{
			var loadLocationQuery = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));
			var portAndCountryQuery = GetExactMatchingQuery(FilterColumn1, Property1);
			loadLocationQuery.AddToFilter(portAndCountryQuery, JoinCondition.Or);

			switch (Property1.Length)
			{
				case 2:
					AddInternationalZoneCoveringQuery(loadLocationQuery, FilterColumn1, Property1);
					break;

				case 5:
					AddInternationalZoneCoveringQuery(loadLocationQuery, FilterColumn1, Property1);
					if (IncludeLinkedSchedules)
					{
						AddLinkedScheduleOriginQuery(loadLocationQuery);
					}
					
					if (ShowRelatedUNLOCOs)
					{
						AddRelatedPortsQueryForAllocationRoute(loadLocationQuery, FilterColumn1, Property1);
						if (IncludeLinkedSchedules)
						{
							AddRelatedPortsQueryForOriginLinkedSchedule(loadLocationQuery);
						}
					}
					break;

				default:
					break;
			}

			parentQuery.AddToFilter(loadLocationQuery, JoinCondition.And);
		}

		void AddDischargeLocationFilters(ZDBOnlyQuery parentQuery)
		{
			var dischargeLocationQuery = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));
			var portAndCountryQuery = GetExactMatchingQuery(FilterColumn2, Property2);
			dischargeLocationQuery.AddToFilter(portAndCountryQuery, JoinCondition.Or);

			switch (Property2.Length)
			{
				case 2:
					AddInternationalZoneCoveringQuery(dischargeLocationQuery, FilterColumn2, Property2);
					break;

				case 5:
					AddInternationalZoneCoveringQuery(dischargeLocationQuery, FilterColumn2, Property2);
					if (IncludeLinkedSchedules)
					{
						AddLinkedScheduleDestinationQuery(dischargeLocationQuery);
					}

					if (ShowRelatedUNLOCOs)
					{
						AddRelatedPortsQueryForAllocationRoute(dischargeLocationQuery, FilterColumn2, Property2);
						if (IncludeLinkedSchedules)
						{
							AddRelatedPortsQueryForDestinationLinkedSchedule(dischargeLocationQuery);
						}
					}
					break;

				default:
					break;
			}

			parentQuery.AddToFilter(dischargeLocationQuery, JoinCondition.And);
		}

		void AddLinkedScheduleOriginQuery(ZDBOnlyQuery parentQuery)
		{
			var voyageOriginSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			voyageOriginSubQuery.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, Property1);

			var sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), RatingContractAllocationLineSchema.RCA_JX_SailingSchedule);
			sailingSubQuery.AddSubQuery(voyageOriginSubQuery, JoinCondition.And);
			parentQuery.AddSubQuery(sailingSubQuery, JoinCondition.Or);
		}

		void AddLinkedScheduleDestinationQuery(ZDBOnlyQuery parentQuery)
		{
			var voyageDestinationSubQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
			voyageDestinationSubQuery.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, Property2);

			var sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), RatingContractAllocationLineSchema.RCA_JX_SailingSchedule);
			sailingSubQuery.AddSubQuery(voyageDestinationSubQuery, JoinCondition.And);
			parentQuery.AddSubQuery(sailingSubQuery, JoinCondition.Or);
		}

		void AddRelatedPortsQueryForAllocationRoute(ZDBOnlyQuery parentQuery, SchemaColumn allocationRouteColumn, ZString portCode)
		{
			var query = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));

			var relatedPortsQuery = GetRelatedPortsQuery(portCode);
			query.AddSubQuery(allocationRouteColumn, relatedPortsQuery, JoinCondition.And);

			parentQuery.AddToFilter(query, JoinCondition.Or);
		}

		void AddRelatedPortsQueryForOriginLinkedSchedule(ZDBOnlyQuery parentQuery)
		{
			var query = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));

			var relatedPortsQuery = GetRelatedPortsQuery(Property1);

			var jobQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.PK);
			jobQuery.AddSubQuery(JobVoyOriginSchema.JA_RL_NKPortOfLoading, relatedPortsQuery, JoinCondition.And);

			var jobSailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			jobSailingQuery.AddSubQuery(JobSailingSchema.JX_JA, jobQuery, JoinCondition.And);

			query.AddSubQuery(RatingContractAllocationLineSchema.RCA_JX_SailingSchedule, jobSailingQuery, JoinCondition.And);

			parentQuery.AddToFilter(query, JoinCondition.Or);
		}

		void AddRelatedPortsQueryForDestinationLinkedSchedule(ZDBOnlyQuery parentQuery)
		{
			var query = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));

			var relatedPortsQuery = GetRelatedPortsQuery(Property2);

			var jobQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.PK);
			jobQuery.AddSubQuery(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, relatedPortsQuery, JoinCondition.And);

			var jobSailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			jobSailingQuery.AddSubQuery(JobSailingSchema.JX_JB, jobQuery, JoinCondition.And);

			query.AddSubQuery(RatingContractAllocationLineSchema.RCA_JX_SailingSchedule, jobSailingQuery, JoinCondition.And);

			parentQuery.AddToFilter(query, JoinCondition.Or);
		}

		static ZDBOnlySubQuery GetRelatedPortsQuery(ZString portCode)
		{
			var relatedPortGroupQuery = new ZDBOnlySubQuery(typeof(RefUNLOCORelatedPort), RefUNLOCORelatedPortSchema.RLR_GroupNumber);
			relatedPortGroupQuery.AddToFilter(RefUNLOCORelatedPortSchema.RLR_RL_NKRelatedPort, portCode);

			var relatedPortsQuery = new ZDBOnlySubQuery(typeof(RefUNLOCORelatedPort), RefUNLOCORelatedPortSchema.RLR_RL_NKRelatedPort);
			relatedPortsQuery.AddSubQuery(RefUNLOCORelatedPortSchema.RLR_GroupNumber, relatedPortGroupQuery, JoinCondition.And);

			return relatedPortsQuery;
		}

		ZQuery GetExactMatchingQuery(SchemaColumn allocationRouteColumn, ZString location)
		{
			var query = new ZQuery();

			if (location.Length == 0)
			{
				return query;
			}

			query.AddToFilter(allocationRouteColumn, SQLComparisonOperator.Equal, location);

			var isPortCode = location.Length == 5;
			if (isPortCode)
			{
				var countryCode = location.SubstringSafe(0, 2);
				query.AddToFilter(JoinCondition.Or, allocationRouteColumn, SQLComparisonOperator.Equal, countryCode);
			}

			return query;
		}

		void AddInternationalZoneCoveringQuery(ZDBOnlyQuery parentQuery, SchemaColumn column, ZString portOrCountry)
		{
			var query = new ZDBOnlyQuery(typeof(RatingContractAllocationLine));
			var internationalZoneSubQuery = new ZDBOnlySubQuery(typeof(IRefZoneHeader), RefZoneHeaderSchema.FZ_Code);

			var isPortCode = portOrCountry.Length == 5;
			if (isPortCode)
			{
				var unlocoZonesSubQuery = GetUnlocoZonesSubQuery(portOrCountry);
				internationalZoneSubQuery.AddSubQuery(RefZoneHeaderSchema.PK, unlocoZonesSubQuery, JoinCondition.And);

				if (ShowRelatedUNLOCOs)
				{
					var unlocoZonesOfRelatedPortsSubQuery = GetUnlocoZonesOfRelatedPortsSubQuery(portOrCountry);
					internationalZoneSubQuery.AddSubQuery(RefZoneHeaderSchema.PK, unlocoZonesOfRelatedPortsSubQuery, JoinCondition.Or);
				}
			}

			var countryCode = portOrCountry.SubstringSafe(0, 2);
			var countryZonesSubQuery = GetCountryZonesSubQuery(countryCode);
			var joinCondition = isPortCode ? JoinCondition.Or : JoinCondition.And;

			internationalZoneSubQuery.AddSubQuery(RefZoneHeaderSchema.PK, countryZonesSubQuery, joinCondition);
			var zoneTypes = new string[]
			{
				RefZoneHeaderLookups.ZoneTypeCodes.Contract, RefZoneHeaderLookups.ZoneTypeCodes.All
			};
			internationalZoneSubQuery.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, zoneTypes);

			query.AddSubQuery(column, internationalZoneSubQuery, JoinCondition.And);
			parentQuery.AddToFilter(query, JoinCondition.Or);
		}

		static ZDBOnlySubQuery GetUnlocoZonesOfRelatedPortsSubQuery(ZString port)
		{
			var refUnlocoQuery = GetRelatedPortsQuery(port);

			var unlocoSubQuery = new ZDBOnlySubQuery(typeof(IRefUNLOCO), RefUNLOCOSchema.PK);
			unlocoSubQuery.AddSubQuery(RefUNLOCOSchema.RL_Code, refUnlocoQuery, JoinCondition.And);

			var zonePivotSubQuery = new ZDBOnlySubQuery(typeof(IRefZonePivot), RefZonePivotSchema.F2_FZ);
			zonePivotSubQuery.AddToFilter(RefZonePivotSchema.F2_ParentTableCode, RefUNLOCOSchema.Constants.Prefix);
			zonePivotSubQuery.AddSubQuery(RefZonePivotSchema.F2_ParentID, unlocoSubQuery, JoinCondition.And);

			return zonePivotSubQuery;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			SetDefaultFields();
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(nameof(ShowRelatedUNLOCOs), ShowRelatedUNLOCOs.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			if (reader.Name == nameof(ShowRelatedUNLOCOs))
			{
				ShowRelatedUNLOCOs = reader.ReadElementString(nameof(ShowRelatedUNLOCOs)) == true.ToString();
			}
		}
	}
}

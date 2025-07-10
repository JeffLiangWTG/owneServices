using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public sealed class SailingFilterBuilder
	{
		#region Enums

		public enum Dates
		{
			None = 0,
			ETD = 1,
			ETA = 2,
			ATD = 3,
			ATA = 4,
			LCLAvailable = 5,
			LCLCutOff = 6,
			LCLReceivalCommences = 7,
			LCLStorage = 8,
			FCLAvailable = 9,
			FCLCutOff = 10,
			FCLReceivalCommences = 11,
			FCLStorage = 12,
			LoadETA = 13,
			LoadATA = 14,
			CTOCutOff = 15,
			CFSCutOff = 16,
			DocsDue = 17,
			VGMCutOff = 18,
			CTOReceival = 19,
			CFSReceival = 20,
			CTOAvailable = 21,
			CFSAvailable = 22,
			CTOStorage = 23,
			CFSStorage = 24
		}

		[Flags]
		public enum RelationshipFlags
		{
			None = 0,
			Direct = 1,
			ViaConsol = 2,
			ViaDirectTransports = 4,
			ViaDocsAndCartage = 8,
			AllSchedules = Direct | ViaConsol | ViaDirectTransports,
		}

		#endregion

		public SailingFilterBuilder(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			this.Factory = factory;
			Reset();
		}

		public ZString TransportMode { get; set; }
		public ZString Vessel { get; set; }
		public ZString VoyageFlight { get; set; }
		public SQLComparisonOperator VoyageFlightComparisonOperator { get; set; }
		public ZBool IncludeArchived { get; set; }
		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }

		public ZDateTime ETAFrom { get; set; }
		public ZDateTime ETATo { get; set; }
		public DateComparisonOperator ETAComparisonOperator { get; set; }
		public bool ShouldSearchForUnlinkedOrNoTransports { get; set; }
		ZBool FilterHasNonProxiedField { get; set; }
		ZBool ShouldFetchNonlinkedTransportForNonProxiedFields { get; set; }

		[ThreadStatic]
		static ReadOnlyCollection<SchemaDateTimeColumn> dateFieldsNotProxied;
		static ReadOnlyCollection<SchemaDateTimeColumn> DateFieldsNotProxied => dateFieldsNotProxied ?? (dateFieldsNotProxied = new ReadOnlyCollection<SchemaDateTimeColumn>(new[]
					{
						JobVoyOriginSchema.JA_E_ARV, JobVoyOriginSchema.JA_A_ARV, JobVoyOriginSchema.JA_CutOff, JobVoyOriginSchema.JA_ReceivalCommences,
						JobVoyDestinationSchema.JB_AvailabilityDate, JobVoyDestinationSchema.JB_StorageDate,
						JobSailingSchema.JX_DepotAvailabilityDate, JobSailingSchema.JX_DepotCutOff, JobSailingSchema.JX_DepotReceivalCommences, JobSailingSchema.JX_DepotStorageDate
					}));

		public void SetDateRange(Dates dateType, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			DateRange dateRange = GetDateRange(dateType);
			dateRange.FromDate = fromDate;
			dateRange.ToDate = toDate;
			dateRange.ComparisonOperator = comparisonOperator;
		}

		public void Reset()
		{
			Vessel = "";
			VoyageFlight = "";
			VoyageFlightComparisonOperator = SQLComparisonOperator.Equal;
			IncludeArchived = true;
			LoadPort = "";
			DischargePort = "";
			DatesToFilter.Clear();
			FilterHasNonProxiedField = false;
			ShouldFetchNonlinkedTransportForNonProxiedFields = false;
		}

		public ZDBOnlyQuery ToSailingFilter()
		{
			ZQuery unmanagedTransportFilter = new ZQuery();
			ZQuery voyageFilter = new ZQuery();
			ZQuery originFilter = new ZQuery();
			ZQuery destinationFilter = new ZQuery();
			ZQuery sailingFilter = new ZQuery();

			AddVoyageFilters(voyageFilter, unmanagedTransportFilter);
			AddOriginFilters(originFilter, unmanagedTransportFilter);
			AddDestinationFilters(destinationFilter, unmanagedTransportFilter);
			AddSailingFilters(sailingFilter);

			return MergeSailingFilters(voyageFilter, originFilter, destinationFilter, sailingFilter);
		}

		public ZDBOnlyQuery ToTransportFilter(bool needUnion = false, bool needShipmentSailingUnion = false)
		{
			var transportFilter = new ZQuery();
			var voyageFilter = new ZQuery();
			var originFilter = new ZQuery();
			var destinationFilter = new ZQuery();
			var sailingFilter = new ZQuery();
			var transportQuery = new ZDBOnlyQuery(typeof(Transport));
			AddVoyageFilters(voyageFilter, transportFilter);
			AddOriginFilters(originFilter, transportFilter);
			AddDestinationFilters(destinationFilter, transportFilter);
			AddSailingFilters(sailingFilter);
			AddTransportDatatimeFilters(transportFilter);

			if (VoyageFlightComparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				transportFilter.AddToFilter(JobConsolTransportSchema.JW_VoyageFlight, SQLComparisonOperator.Equal, ZString.Empty);
				transportQuery.AddToFilter(transportFilter, JoinCondition.And);
			}
			else
			{
				var sailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
				sailingQuery.AddToFilter(MergeSailingFilters(voyageFilter, originFilter, destinationFilter, sailingFilter));

				var shouldFilterByNotArchived = !IncludeArchived && (!VoyageFlight.IsEmpty || !Vessel.IsEmpty || VoyageFlightComparisonOperator == SpecialComparisonOperator.IsNotBlank);

				if (!transportFilter.IsEmpty && !FilterHasNonProxiedField)
				{
					if (shouldFilterByNotArchived)
					{
						transportFilter.AddToFilter(JobConsolTransportSchema.JW_IsLinked, false);
					}
					transportQuery.AddToFilter(transportFilter, JoinCondition.And);
				}
				else if (FilterHasNonProxiedField)
				{
					transportQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingQuery, JoinCondition.And);

					if (ShouldFetchNonlinkedTransportForNonProxiedFields)
					{
						var nonlinkedTransportFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
						nonlinkedTransportFilter.AddToFilter(JobConsolTransportSchema.JW_JX, null);
						nonlinkedTransportFilter.AddToFilter(transportFilter, JoinCondition.And);
						transportQuery.AddAsUnionQuery(nonlinkedTransportFilter, true);
					}
				}

				if (shouldFilterByNotArchived)
				{
					var voyageQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyageSchema.PK);
					var destinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.PK);

					voyageQuery.AddToFilter(voyageFilter);
					destinationQuery.AddToFilter(destinationFilter);

					destinationQuery.AddSubQuery(JobVoyDestinationSchema.JB_JV, voyageQuery, JoinCondition.And);
					if (sailingQuery.IsEmpty)
					{
						sailingQuery.AddSubQuery(JobSailingSchema.JX_JB, destinationQuery, JoinCondition.And);
					}

					var transportSubQuery = needUnion
											? new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID)
											: new ZDBOnlyQuery(typeof(Transport));
					transportSubQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingQuery, JoinCondition.And);

					if (needUnion)
					{
						transportQuery.AddAsUnionQuery((ZDBOnlySubQuery)transportSubQuery, true);
					}
					else
					{
						transportQuery.AddToFilter(transportSubQuery, JoinCondition.Or);
					}
				}

				if (!sailingQuery.IsEmpty && needShipmentSailingUnion)
				{
					var shipmentSailingQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
					shipmentSailingQuery.AddSubQuery(JobShipmentSchema.JS_JX, sailingQuery, JoinCondition.And);
					transportQuery.AddAsUnionQuery(shipmentSailingQuery, true);
				}
			}
			return transportQuery;
		}

		public ZDBOnlyQuery ToConsolFilter()
		{
			ZDBOnlySubQuery transportFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportFilter.AddToFilter(ToTransportFilter(true));

			ZDBOnlyQuery consolFilter = new ZDBOnlyQuery(typeof(CommonConsol));

			if (!transportFilter.IsEmpty)
			{
				consolFilter.AddSubQuery(JobConsolSchema.PK, transportFilter, JoinCondition.And);
			}

			return consolFilter;
		}

		public ZDBOnlyQuery ToShipmentFilter(RelationshipFlags relationship)
		{
			ZDBOnlyQuery shipmentFilter = new ZDBOnlyQuery(typeof(CommonShipment));
			ZDBOnlySubQuery transportFilter = null;
			ZDBOnlySubQuery routingFilter = null;
			var hasNoRoutingFilter = new ZDBOnlyQuery(typeof(CommonShipment));

			var filterPartsJoinCondition = VoyageFlightComparisonOperator == SpecialComparisonOperator.IsBlank
					? JoinCondition.And
					: JoinCondition.Or;

			if ((relationship & RelationshipFlags.ViaDocsAndCartage) > 0)
			{
				var docsAndCartageFilter = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				docsAndCartageFilter.AddToFilter(ToDocsAndCartageFilter());
				if (!docsAndCartageFilter.IsEmpty)
				{
					shipmentFilter.AddSubQuery(docsAndCartageFilter, filterPartsJoinCondition);
				}
			}

			if ((relationship & RelationshipFlags.ViaConsol) > 0)
			{
				transportFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
				transportFilter.AddToFilter(ToTransportFilter(true));

				if (!transportFilter.IsEmpty)
				{
					ZDBOnlySubQuery shipLinkFilter = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
					shipLinkFilter.AddSubQuery(JobConShipLinkSchema.JN_JK, transportFilter, JoinCondition.And);

					routingFilter = shipLinkFilter;
				}

				if (VoyageFlightComparisonOperator == SpecialComparisonOperator.IsBlank && Vessel.IsEmpty)
				{
					ZDBOnlySubQuery shipmentWithoutConsols = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS, true);
					hasNoRoutingFilter.AddSubQuery(shipmentWithoutConsols, JoinCondition.Or);
				}

				if (ShouldSearchForUnlinkedOrNoTransports)
				{
					var unlinkedTransportsQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
					unlinkedTransportsQuery.AddToFilter(JobConsolTransportSchema.JW_IsLinked, false);

					var shipmentsFromConsolsWithUnlinkedTransports = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
					shipmentsFromConsolsWithUnlinkedTransports.AddSubQuery(JobConShipLinkSchema.JN_JK, unlinkedTransportsQuery, JoinCondition.Or);

					shipmentFilter.AddSubQuery(unlinkedTransportsQuery, JoinCondition.Or);
					shipmentFilter.AddSubQuery(shipmentsFromConsolsWithUnlinkedTransports, JoinCondition.Or);
					shipmentFilter.AddSubQuery(GetShipmentWithoutTransportsSubQuery(), JoinCondition.Or);
				}
			}

			if ((relationship & RelationshipFlags.ViaDirectTransports) > 0)
			{
				transportFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
				transportFilter.AddToFilter(ToTransportFilter(true, true));

				if (!transportFilter.IsEmpty)
				{
					if (routingFilter == null)
					{
						routingFilter = transportFilter;
					}
					else
					{
						routingFilter.AddAsUnionQuery(transportFilter, true);
					}
				}

				if (VoyageFlightComparisonOperator == SpecialComparisonOperator.IsBlank && Vessel.IsEmpty)
				{
					var shipmentWithoutTransports = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID, true);
					hasNoRoutingFilter.AddSubQuery(shipmentWithoutTransports, JoinCondition.And);
				}
			}

			if (routingFilter != null)
			{
				var combinedRoutingFilter = new ZDBOnlyQuery(typeof(CommonShipment));
				combinedRoutingFilter.AddSubQuery(JobShipmentSchema.PK, routingFilter, filterPartsJoinCondition);

				if (!hasNoRoutingFilter.IsEmpty)
				{
					combinedRoutingFilter.AddToFilter(hasNoRoutingFilter, JoinCondition.Or);
				}

				shipmentFilter.AddToFilter(combinedRoutingFilter, filterPartsJoinCondition);
			}

			if ((relationship & RelationshipFlags.Direct) > 0)
			{
				if (VoyageFlightComparisonOperator == SpecialComparisonOperator.IsBlank)
				{
					if (!Vessel.IsEmpty && shipmentFilter.IsEmpty)
					{
						var noResultQuery = new ZQuery();
						noResultQuery.IsNoResultQuery = true;
						shipmentFilter.AddToFilter(noResultQuery);
					}
					else
					{
						shipmentFilter.AddToFilter(filterPartsJoinCondition, JobShipmentSchema.JS_JX, SQLComparisonOperator.Equal, null);
					}
				}
				else
				{
					if (transportFilter == null)
					{
						ZDBOnlySubQuery sailingFilter = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
						sailingFilter.AddToFilter(ToSailingFilter());

						if (!sailingFilter.IsEmpty)
						{
							shipmentFilter.AddSubQuery(JobShipmentSchema.JS_JX, sailingFilter, filterPartsJoinCondition);
						}
					}
				}
			}

			return shipmentFilter;
		}

		ZDBOnlySubQuery GetConsolsWithTransportsSubQuery()
		{
			var consolWithoutTransports = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			consolWithoutTransports.AddToFilter(JoinCondition.And, JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.Consol);

			return consolWithoutTransports;
		}

		ZDBOnlySubQuery GetShipmentWithoutTransportsSubQuery()
		{
			var filter = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK, true);

			var consolWithoutTransports = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			consolWithoutTransports.AddSubQuery(JobConShipLinkSchema.JN_JK, GetConsolsWithTransportsSubQuery(), JoinCondition.And);
			filter.AddSubQuery(consolWithoutTransports, JoinCondition.And);

			var shipmentWithoutTransports = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			shipmentWithoutTransports.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.Shipment);
			filter.AddSubQuery(shipmentWithoutTransports, JoinCondition.Or);

			return filter;
		}

		public ZDBOnlyQuery ToContainerFilter()
		{
			ZDBOnlySubQuery consolFilter = new ZDBOnlySubQuery(typeof(CommonConsol), JobConsolSchema.PK);
			consolFilter.AddToFilter(ToConsolFilter());

			ZDBOnlyQuery containerFilter = new ZDBOnlyQuery(typeof(CommonContainer));
			if (!consolFilter.IsEmpty)
			{
				containerFilter.AddSubQuery(JobContainerSchema.JC_JK, consolFilter, JoinCondition.And);
			}

			return containerFilter;
		}

		public ZDBOnlyQuery ToDeclarationContainerFilter()
		{
			var cusContainerType = ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.Shared.IBaseCusContainer));
			if (cusContainerType == null)
			{
				return null;
			}

			var result = new ZDBOnlyQuery(typeof(CommonContainer));

			var destinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.PK);
			AddDateRangeFilter(destinationQuery, JobVoyDestinationSchema.JB_AvailabilityDate, Dates.FCLAvailable);

			var sailingQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			sailingQuery.AddSubQuery(JobSailingSchema.JX_JB, destinationQuery, JoinCondition.And);

			var transportQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportQuery.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.Declaration);
			transportQuery.AddToFilter(JobConsolTransportSchema.JW_IsLinked, true);
			transportQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingQuery, JoinCondition.And);

			var cusContainerQuery = new ZDBOnlySubQuery(cusContainerType, CusContainerSchema.CO_JC);
			cusContainerQuery.AddSubQuery(CusContainerSchema.CO_JE, transportQuery, JoinCondition.And);

			result.AddSubQuery(cusContainerQuery, JoinCondition.And);

			return result;
		}

		public ZDBOnlySubQuery ToCommercialInvoiceSubQuery()
		{
			ZDBOnlySubQuery transportFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportFilter.AddToFilter(JobConsolTransportSchema.JW_ParentType, Core.Constants.TransportParentTypes.CommercialInvoice);
			transportFilter.AddToFilter(ToTransportFilter());
			return transportFilter;
		}

		public ZDBOnlyQuery ToDocsAndCartageFilter()
		{
			ZDBOnlyQuery docsAndCartageFilter = new ZDBOnlyQuery(typeof(JobDocsAndCartage));

			AddDateRangeFilter(docsAndCartageFilter, JobDocsAndCartageSchema.JP_LCLAvailable, Dates.LCLAvailable);
			AddDateRangeFilter(docsAndCartageFilter, JobDocsAndCartageSchema.JP_LCLStorageCommences, Dates.LCLStorage);

			return docsAndCartageFilter;
		}

		public ZDBOnlyQuery ToDeclarationFilter(RelationshipFlags relationship = RelationshipFlags.None)
		{
			var declarationFilter = new ZDBOnlyQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration));

			if ((relationship & RelationshipFlags.ViaDocsAndCartage) > 0)
			{
				var docsAndCartageFilter = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				docsAndCartageFilter.AddToFilter(ToDocsAndCartageFilter());
				if (!docsAndCartageFilter.IsEmpty)
				{
					declarationFilter.AddSubQuery(docsAndCartageFilter, JoinCondition.Or);
				}
			}

			//ViaShipment
			var shipmentFilter = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
			shipmentFilter.AddToFilter(ToShipmentFilter(RelationshipFlags.ViaConsol));
			if (!shipmentFilter.IsEmpty)
			{
				declarationFilter.AddSubQuery(JobDeclarationSchema.JE_JS, shipmentFilter, JoinCondition.Or);
			}

			//Standalone
			var standaloneFilter = new ZDBOnlyQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration));

			if (!TransportMode.IsEmpty)
			{
				standaloneFilter.AddToFilter(JobDeclarationSchema.JE_TransportMode, TransportMode);
			}

			if (!LoadPort.IsEmpty)
			{
				standaloneFilter.AddToFilter(LocationHelper.GetLocationFilter(Factory, LoadPort, JobDeclarationSchema.JE_RL_NKPortOfLoading, ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>()));
			}

			if (!DischargePort.IsEmpty)
			{
				standaloneFilter.AddToFilter(LocationHelper.GetLocationFilter(Factory, DischargePort, JobDeclarationSchema.JE_RL_NKPortOfArrival, ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>()));
			}

			if (!Vessel.IsEmpty || VoyageFlightComparisonOperator == SpecialComparisonOperator.IsNotBlank || VoyageFlightComparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				standaloneFilter.AddToFilter_PossiblyCommaSeparated(JobDeclarationSchema.JE_VesselName, VoyageFlightComparisonOperator, Vessel);
			}

			if (!VoyageFlight.IsEmpty || VoyageFlightComparisonOperator == SpecialComparisonOperator.IsNotBlank || VoyageFlightComparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				standaloneFilter.AddToFilter_PossiblyCommaSeparated(JobDeclarationSchema.JE_VoyageFlightNo, VoyageFlightComparisonOperator, VoyageFlight);
			}

			if ((relationship & RelationshipFlags.ViaDirectTransports) > 0)
			{
				var transportFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
				transportFilter.AddToFilter(ToTransportFilter());
				if (!transportFilter.IsEmpty)
				{
					standaloneFilter.AddSubQuery(transportFilter, JoinCondition.Or);
				}
			}

			var dateQuery = new ZQuery();
			AddDateRangeFilter(dateQuery, JobDeclarationSchema.JE_ExportDate, Dates.ATD);
			AddDateRangeFilter(dateQuery, JobDeclarationSchema.JE_ExportDate, Dates.ETD);
			AddDateRangeFilter(dateQuery, JobDeclarationSchema.JE_DateOfArrival, Dates.ATA);
			AddDateRangeFilter(dateQuery, JobDeclarationSchema.JE_DateOfArrival, Dates.ETA);
			standaloneFilter.AddToFilter(dateQuery);

			declarationFilter.AddToFilter(standaloneFilter, JoinCondition.Or);
			return declarationFilter;
		}

		#region Implementation

		class DateRange
		{
			public ZDateTime FromDate { get; set; }
			public ZDateTime ToDate { get; set; }
			public DateComparisonOperator ComparisonOperator { get; set; }
		}

		void AddTransportDatatimeFilters(ZQuery transportFilter)
		{
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_TerminalReceivalCommences, Dates.CTOReceival);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_TerminalCutOff, Dates.CTOCutOff);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_DocumentaryCutOff, Dates.DocsDue);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_DepotReceivalCommences, Dates.CFSReceival);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_DepotCutOff, Dates.CFSCutOff);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_VGMCutOff, Dates.VGMCutOff);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_TerminalAvailabilityDate, Dates.CTOAvailable);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_TerminalStorageDate, Dates.CTOStorage);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_DepotAvailabilityDate, Dates.CFSAvailable);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_DepotStorageDate, Dates.CFSStorage);
		}

		void AddVoyageFilters(ZQuery voyageFilter, ZQuery transportFilter)
		{
			var joinOperator = VoyageFlightComparisonOperator == SpecialComparisonOperator.IsNotBlank ? JoinCondition.Or : JoinCondition.And;

			if (!TransportMode.IsEmpty)
			{
				voyageFilter.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, TransportMode);
				transportFilter.AddToFilter(JobConsolTransportSchema.JW_TransportMode, TransportMode);
			}

			if (!Vessel.IsEmpty || VoyageFlightComparisonOperator == SpecialComparisonOperator.IsNotBlank || VoyageFlightComparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				voyageFilter.AddToFilter_PossiblyCommaSeparated(joinOperator, JobVoyageSchema.JV_RV_NKVessel, VoyageFlightComparisonOperator, Vessel);
				transportFilter.AddToFilter_PossiblyCommaSeparated(joinOperator, JobConsolTransportSchema.JW_Vessel, VoyageFlightComparisonOperator, Vessel);
			}

			if (!VoyageFlight.IsEmpty || VoyageFlightComparisonOperator == SpecialComparisonOperator.IsNotBlank || VoyageFlightComparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				voyageFilter.AddToFilter_PossiblyCommaSeparated(joinOperator, JobVoyageSchema.JV_VoyageFlight, VoyageFlightComparisonOperator, VoyageFlight);
				transportFilter.AddToFilter_PossiblyCommaSeparated(joinOperator, JobConsolTransportSchema.JW_VoyageFlight, VoyageFlightComparisonOperator, VoyageFlight);
			}

			voyageFilter.IgnoreActiveFilter = IncludeArchived;
		}

		void AddOriginFilters(ZQuery originFilter, ZQuery transportFilter)
		{
			AddDateRangeFilter(originFilter, JobVoyOriginSchema.JA_E_ARV, Dates.LoadETA);
			AddDateRangeFilter(originFilter, JobVoyOriginSchema.JA_A_ARV, Dates.LoadATA);

			AddDateRangeFilter(originFilter, JobVoyOriginSchema.JA_E_DEP, Dates.ETD);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_ETD, Dates.ETD);

			AddDateRangeFilter(originFilter, JobVoyOriginSchema.JA_A_DEP, Dates.ATD);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_ATD, Dates.ATD);

			AddDateRangeFilter(originFilter, JobVoyOriginSchema.JA_CutOff, Dates.FCLCutOff);
			AddDateRangeFilter(originFilter, JobVoyOriginSchema.JA_ReceivalCommences, Dates.FCLReceivalCommences);

			if (!LoadPort.IsEmpty)
			{
				originFilter.AddToFilter(LocationHelper.GetLocationFilter(Factory, LoadPort, JobVoyOriginSchema.JA_RL_NKPortOfLoading, typeof(VoyageOrigin)));
				transportFilter.AddToFilter(LocationHelper.GetLocationFilter(Factory, LoadPort, JobConsolTransportSchema.JW_RL_NKLoadPort, typeof(Transport)));
			}
		}

		void AddDestinationFilters(ZQuery destinationFilter, ZQuery transportFilter)
		{
			if (!ETAFrom.IsEmpty || !ETATo.IsEmpty)
			{
				AddDateRangeFilter(destinationFilter, JobVoyDestinationSchema.JB_E_ARV, ETAComparisonOperator, ETAFrom, ETATo);
				AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_ETA, ETAComparisonOperator, ETAFrom, ETATo);
			}
			else
			{
				AddDateRangeFilter(destinationFilter, JobVoyDestinationSchema.JB_E_ARV, Dates.ETA);
				AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_ETA, Dates.ETA);
			}

			AddDateRangeFilter(destinationFilter, JobVoyDestinationSchema.JB_A_ARV, Dates.ATA);
			AddDateRangeFilter(transportFilter, JobConsolTransportSchema.JW_ATA, Dates.ATA);

			AddDateRangeFilter(destinationFilter, JobVoyDestinationSchema.JB_AvailabilityDate, Dates.FCLAvailable);
			AddDateRangeFilter(destinationFilter, JobVoyDestinationSchema.JB_StorageDate, Dates.FCLStorage);

			if (!DischargePort.IsEmpty)
			{
				destinationFilter.AddToFilter(LocationHelper.GetLocationFilter(Factory, DischargePort, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, typeof(VoyageDestination)));
				transportFilter.AddToFilter(LocationHelper.GetLocationFilter(Factory, DischargePort, JobConsolTransportSchema.JW_RL_NKDiscPort, typeof(Transport)));
			}
		}

		void AddSailingFilters(ZQuery sailingFilter)
		{
			AddDateRangeFilter(sailingFilter, JobSailingSchema.JX_DepotAvailabilityDate, Dates.LCLAvailable);
			AddDateRangeFilter(sailingFilter, JobSailingSchema.JX_DepotCutOff, Dates.LCLCutOff);
			AddDateRangeFilter(sailingFilter, JobSailingSchema.JX_DepotReceivalCommences, Dates.LCLReceivalCommences);
			AddDateRangeFilter(sailingFilter, JobSailingSchema.JX_DepotStorageDate, Dates.LCLStorage);
		}

		void AddDateRangeFilter(ZQuery filter, SchemaDateTimeColumn column, Dates dateType)
		{
			DateRange dateRange = GetDateRange(dateType);
			AddDateRangeFilter(filter, column, dateRange.ComparisonOperator, dateRange.FromDate, dateRange.ToDate);
		}

		void AddDateRangeFilter(ZQuery filter, SchemaDateTimeColumn column, DateComparisonOperator dateComparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery subFilter = new ZQuery();

			if (dateComparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				if (column.IsNullable)
				{
					subFilter.AddToFilter(column, SQLComparisonOperator.NotEqual, null);
				}
			}
			else if (dateComparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				if (column.IsNullable)
				{
					subFilter.AddToFilter(column, SQLComparisonOperator.Equal, null);
				}
				else
				{
					subFilter.IsNoResultQuery = true;
				}
			}
			else
			{
				if (fromDate.IsValid)
				{
					subFilter.AddToFilter(column, SQLComparisonOperator.GreaterThanOrEqualTo, fromDate);
				}

				if (toDate.IsValid)
				{
					subFilter.AddToFilter(column, SQLComparisonOperator.LessThanOrEqualTo, toDate);
				}
			}

			if (!FilterHasNonProxiedField)
			{
				FilterHasNonProxiedField = !subFilter.IsEmpty && DateFieldsNotProxied.Contains(column);
				ShouldFetchNonlinkedTransportForNonProxiedFields = FilterHasNonProxiedField && dateComparisonOperator == DateComparisonOperator.HasNoDateEntered;
			}

			filter.AddToFilter(subFilter);
		}

		ZDBOnlyQuery MergeSailingFilters(ZQuery voyageFilter, ZQuery originFilter, ZQuery destinationFilter, ZQuery sailingFilter)
		{
			ZDBOnlySubQuery voyageQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyageSchema.PK);
			ZDBOnlySubQuery originQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.PK);
			ZDBOnlySubQuery destinationQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.PK);
			ZDBOnlyQuery sailingQuery = new ZDBOnlyQuery(typeof(JobSailing));

			voyageQuery.AddToFilter(voyageFilter);
			originQuery.AddToFilter(originFilter);
			destinationQuery.AddToFilter(destinationFilter);
			sailingQuery.AddToFilter(sailingFilter);

			if (!voyageQuery.IsEmpty)
			{
				if (!originQuery.IsEmpty)
				{
					originQuery.AddSubQuery(JobVoyOriginSchema.JA_JV, voyageQuery, JoinCondition.And);
				}
				else
				{
					destinationQuery.AddSubQuery(JobVoyDestinationSchema.JB_JV, voyageQuery, JoinCondition.And);
				}
			}

			if (!originQuery.IsEmpty)
			{
				sailingQuery.AddSubQuery(JobSailingSchema.JX_JA, originQuery, JoinCondition.And);
			}

			if (!destinationQuery.IsEmpty)
			{
				sailingQuery.AddSubQuery(JobSailingSchema.JX_JB, destinationQuery, JoinCondition.And);
			}

			return sailingQuery;
		}

		DateRange GetDateRange(Dates dateType)
		{
			DateRange dateRange;
			if (!DatesToFilter.TryGetValue(dateType, out dateRange))
			{
				dateRange = new DateRange();
				dateRange.ComparisonOperator = DateComparisonOperator.HasDateInRange;
				DatesToFilter.Add(dateType, dateRange);
			}
			return dateRange;
		}

		Dictionary<Dates, DateRange> DatesToFilter
		{
			get { return datesToFilter ?? (datesToFilter = new Dictionary<Dates, DateRange>()); }
		}
		Dictionary<Dates, DateRange> datesToFilter;

		readonly BusinessObjectFactory Factory;

		#endregion
	}
}

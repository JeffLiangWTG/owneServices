using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	// Tested in WhsTransitConsignmentDataObjectReaderTest
	public abstract class WhsTransitConsignmentMatchingHelper<T>
		where T : BusinessObject
	{
		protected WhsTransitConsignmentMatchingHelper(UniversalObjectFactory factory, IOrgHeader bookingParty, IColumnIndexer warehouse, IXmlImportLogger logger)
		{
			Factory = Argument.NotNull(factory, "factory");
			Warehouse = Argument.NotNull(warehouse, "warehouse");
			Logger = Argument.NotNull(logger, "logger");
			ExternalBookingParty = bookingParty;

			if (Logger.IsInternalImport())
			{
				ExternalBookingParty = null;
			}
		}

		readonly UniversalObjectFactory Factory;
		readonly IColumnIndexer Warehouse;
		readonly IXmlImportLogger Logger;
		readonly IOrgHeader ExternalBookingParty;

		#region MatchConsignment

		IDataSourceDataObject dataSource;
		bool hasLandTransportConsignmentSource => dataSource?.Type == (ZString?)nameof(DataContextType.LandTransportConsignment);
		bool hasForwardingShipmentSource => dataSource?.Type == (ZString?)nameof(DataContextType.ForwardingShipment);
		bool hasAirManifestLineSource => dataSource?.Type == (ZString?)nameof(DataContextType.AirManifestLine);

		public T MatchConsignment(ZString houseBill, ZString consignmentID, UniversalShipment sourceDO, bool excludeClosedConsignments = true)
		{
			if (sourceDO?.DataContext?.DataSourceCollection?.Count() > 1)
			{
				Logger.Log(LogType.Warning, ResString.GetMultilingualString("fb6dd4e7-aa76-4d90-8550-631b46090d53", "Consignment matching may not function correctly as multiple Data Sources were provided."));
			}

			dataSource = sourceDO?.FirstDataSource();
			T matchedConsignment = null;

			if (HasSufficientDataForJobLinkMatching)
			{
				matchedConsignment = GetMatchingConsignmentFromJobLinks(sourceDO);
			}

			matchedConsignment = matchedConsignment ?? GetMatchingConsignment(HouseBillNumberColumn, houseBill, excludeClosedConsignments);
			matchedConsignment = matchedConsignment ?? GetMatchingConsignment(ConsignmentIDColumn, houseBill, excludeClosedConsignments);
			matchedConsignment = matchedConsignment ?? GetMatchingConsignment(ConsignmentIDColumn, consignmentID, excludeClosedConsignments);

			return matchedConsignment;
		}

		#region Job Link Matching

		bool HasSufficientDataForJobLinkMatching
		{
			get
			{
				if (dataSource == null || string.IsNullOrEmpty(dataSource.Key))
				{
					return false;
				}

				if (!hasLandTransportConsignmentSource && !hasForwardingShipmentSource && !hasAirManifestLineSource)
				{
					return false;
				}

				if (IsExternalImportWithNoBookingParty)
				{
					return false;
				}

				return true;
			}
		}

		bool IsExternalImportWithNoBookingParty => ExternalBookingParty == null && !Logger.IsInternalImport();

		T GetMatchingConsignmentFromJobLinks(UniversalShipment sourceDO)
		{
			var jobLinks = GetMatchingJobLinks(sourceDO);
			var consignmentRows = GetConsignmentRowsFromJobLinks(jobLinks);
			return GetLatestConsignment(consignmentRows);
		}

		IEnumerable<IColumnIndexer> GetMatchingJobLinks(UniversalShipment sourceDO)
		{
			if (Enum.TryParse(dataSource.Type, out DataContextType context))
			{
				var source = sourceDO.GetMatchingDataSource(context);
				return UniversalJobLinkHelper.GetMatchingJobLinks(Factory, source?.Key, context, ExternalBookingParty, PKSchemaColumn.ColumnPrefix);
			}

			return Array.Empty<StmUniversalJobLink>();
		}

		IColumnIndexer[] GetConsignmentRowsFromJobLinks(IEnumerable<IColumnIndexer> jobLinks)
		{
			var query = new ZQuery(PKSchemaColumn, jobLinks.Select(link => link.GetValue(StmUniversalJobLinkSchema.UCL_ParentID)));
			query.AddToFilter(WarehouseColumn, Warehouse.GetValue(WhsWarehouseSchema.PK));

			return Array.ConvertAll(Factory.RowFactory.Load(PKSchemaColumn.TableName, query), DataObjectReader.GetColumnIndexerFromRow);
		}

		T GetMatchingConsignment(SchemaStringColumn columnToMatch, string columnValToMatch, bool excludeClosedConsignments)
		{
			T matchedConsignment = null;
			if (!string.IsNullOrEmpty(columnValToMatch))
			{
				matchedConsignment = GetLatestMatchingConsignment(columnToMatch, columnValToMatch, excludeClosedConsignments);

				if (matchedConsignment == null)
				{
					matchedConsignment = GetLatestMatchingConsignment(columnToMatch, columnValToMatch, excludeClosedConsignments: false);
				}
			}

			return matchedConsignment;
		}

		T GetLatestMatchingConsignment(SchemaStringColumn columnToMatch, string columnValToMatch, bool excludeClosedConsignments)
		{
			T matchedConsignment = null;
			var consignmentQuery = new ZQuery(columnToMatch, columnValToMatch);
			consignmentQuery.AddToFilter(WarehouseColumn, Warehouse.GetValue(WhsWarehouseSchema.PK)); // any additional Filtering should be considered straight away

			if (excludeClosedConsignments)
			{
				consignmentQuery.AddToFilter(CompleteTimeColumn, ZDateTimeOffset.Empty);
			}

			var matchedConsignments = Array.ConvertAll(Factory.RowFactory.Load(columnToMatch.TableName, consignmentQuery), DataObjectReader.GetColumnIndexerFromRow);
			if (matchedConsignments.Any())
			{
				matchedConsignment = GetLatestConsignment(matchedConsignments);
			}

			return matchedConsignment;
		}

		#endregion

		T GetLatestConsignment(IColumnIndexer[] matchingConsignments)
		{
			if (matchingConsignments.Length >= 1)
			{
				matchingConsignments =
				(
					from c in matchingConsignments
					orderby c.GetValue(CreatedTimeColumn) descending
					select c
				).ToArray();
				var consignment = matchingConsignments[0]; // grab first element as they are ordered by latest Create Time
				var pkColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(consignment.TableName);
				return Factory.Load<T>(consignment.GetValue(pkColumn)); // need to return a BizO to the architecture
			}

			return null;
		}

		protected abstract SchemaStringColumn JobIDColumn { get; }
		protected abstract SchemaStringColumn ConsignmentIDColumn { get; }
		protected abstract SchemaStringColumn HouseBillNumberColumn { get; }
		protected abstract SchemaGuidColumn WarehouseColumn { get; }
		protected abstract SchemaGuidColumn PKSchemaColumn { get; }
		protected abstract SchemaDateTimeOffsetColumn CompleteTimeColumn { get; }
		protected abstract ZString JobDescription { get; }

		#region GetConsignmentsMatchingAddresses

		protected abstract SchemaDateTimeColumn CreatedTimeColumn { get; }

		#endregion

		#endregion
	}
}

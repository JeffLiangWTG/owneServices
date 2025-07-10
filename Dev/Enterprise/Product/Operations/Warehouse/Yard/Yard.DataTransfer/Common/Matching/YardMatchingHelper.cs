using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public static class YardMatchingHelper
	{
		#region GetYard

		public static IColumnIndexer GetYard(Shipment sourceDO, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			IColumnIndexer yard = null;

			if (sourceDO.OrganizationAddressCollection != null && sourceDO.OrganizationAddressCollection.Any(address => address.AddressType.GetValueOrDefault() == nameof(DocAddressType.LocalCartageYard)))
			{
				yard = GetYardFromOrgAddress(sourceDO, factory, logger);
			}

			if (yard == null)
			{
				logger.Log(LogType.Warning, Res.GetString("95297812-4b25-43fd-b38b-f214ba7c2648", "Could not find Yard. No organization address or premise information is found in UXML."));
			}

			return yard;
		}

		static IColumnIndexer GetYardFromOrgAddress(Shipment sourceDO, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var yardAddressDataObject = sourceDO.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.LocalCartageYard));
			return GetYardFromYardAddress(yardAddressDataObject, factory, logger);
		}

		static IColumnIndexer GetYardFromYardAddress(OrganizationAddress yardAddressDataObject, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var yard = GetYard(factory, yardAddressDataObject, logger);

			if (yard != null)
			{
				logger.Log(LogType.Information, Res.GetString("870e392f-0c43-4e12-aad7-d76c470748c0", "Found Yard {0} from Local Cartage Yard Address.", yard[WhsWarehouseSchema.Constants.WW_WarehouseCode]));
			}
			else
			{
				logger.Log(LogType.Warning, Res.GetString("cf4e6b71-77ae-46e1-9fed-ce89824ff047", "Could not find Yard from Local Cartage Yard Address."));
			}

			return yard;
		}

		static IColumnIndexer GetYard(UniversalObjectFactory factory, OrganizationAddress yardAddressDataObject, IXmlImportLogger logger)
		{
			IColumnIndexer yard = null;
			var gateAddressMatcher = ObjectFactory.New<IGateManagementOrganisationDataObjectReader>(yardAddressDataObject, logger, factory);
			var matchedAddress = gateAddressMatcher.GetMatched();
			if (matchedAddress is OrgAddress cfsOrgAddress)
			{
				yard = GetYard(factory, cfsOrgAddress);
			}

			return yard;
		}

		static IColumnIndexer GetYard(UniversalObjectFactory factory, OrgAddress cfsOrgAddress)
		{
			var yardQuery = GetYardQuery(cfsOrgAddress);

			var activeYards = factory.RowFactory.Load(WhsWarehouseSchema.Constants.TableName, yardQuery);
			if (activeYards.Length > 1)
			{
				var yardsWithSameOrgAddress = string.Join(", ", activeYards.Select(w => w[WhsWarehouseSchema.Constants.WW_WarehouseCode]));
				var multipleYardsErrorMessage =
					Res.GetString("50cf6579-5b24-46e2-ba57-71be69cd0942",
@"Multiple Active Container Yards {0} matching the Address '{1} - {2}' were found.
Ensure only one Active Container Yards exists matching this Address.", yardsWithSameOrgAddress, cfsOrgAddress.Header.OH_Code, cfsOrgAddress.Address1);
				throw new DataObjectReadFailureException(multipleYardsErrorMessage);
			}

			return DataObjectReader.GetColumnIndexerFromRow(activeYards.SingleOrDefault());
		}

		static ZQuery GetYardQuery(OrgAddress cfsOrgAddress)
		{
			var yardQuery = new ZQuery();
			yardQuery.AddToFilter(WhsWarehouseSchema.WW_OA_WarehouseAddress, cfsOrgAddress.PK);
			yardQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.ContainerYard);
			yardQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
			yardQuery.MaximumRows = 2;
			return yardQuery;
		}

		#endregion

		#region YardUnit

		public static CYDYardUnitState GetYardUnitForDropOff(BusinessObjectFactory factory, string unitNumber, WhsWarehouse yard)
		{
			var today = yard.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow).Date;

			var query = new ZDBOnlyQuery(typeof(CYDYardUnitState));
			_ = query.AddToFilter(CYDYardUnitStateSchema.YUS_UnitID, unitNumber);

			var receiveAdviceLineSubQuery = new ZDBOnlySubQuery(typeof(CYDReceiveAdviceLine), CYDYardUnitStateSchema.YUS_YRL_ReceiveLine);

			var receiveAdviceSubQuery = new ZDBOnlySubQuery(typeof(CYDReceiveAdvice), CYDReceiveAdviceLineSchema.YRL_YRA_ReceiveAdvice);
			receiveAdviceSubQuery.AddToFilter(CYDReceiveAdviceSchema.YRA_WW_Yard, yard.PK);
			receiveAdviceSubQuery.AddToFilter(CYDReceiveAdviceSchema.YRA_FromDate, SQLComparisonOperator.LessThanOrEqualTo, today);
			receiveAdviceSubQuery.AddToFilter(CYDReceiveAdviceSchema.YRA_ToDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);
			receiveAdviceLineSubQuery.AddSubQuery(receiveAdviceSubQuery, JoinCondition.And);

			query.AddSubQuery(receiveAdviceLineSubQuery, JoinCondition.And);

			const string deliverySubQuery = $"""
{CYDYardUnitStateSchema.Constants.YUS_YDL_Delivery} IN
(
	SELECT
		{CYDDeliverySchema.Constants.PK}
	FROM
		{CYDDeliverySchema.Constants.TableName}
	WHERE
		{CYDDeliverySchema.Constants.YDL_IsReject} = 0
)
OR
{CYDYardUnitStateSchema.Constants.YUS_YDL_Delivery} IS NULL
""";
			query.AddFilterAndZSQLParameterCollection(deliverySubQuery, new ZSqlParameterCollection());

			var matchedYardUnits = factory
				.Load<CYDYardUnitState>(query)
				.Where(unit => unit.ReceiveAdvice.Client.Address is not null && !unit.HasBeenGatedOut)
				.ToArray();

			var result = matchedYardUnits
				.OrderBy(unit => unit.ReceiveAdvice.YRA_FromDate)
				.ThenBy(unit => unit.ReceiveAdviceLine.YRL_OffHireDate)
				.ThenBy(unit => unit.ReceiveAdvice.YRA_ToDate)
				.ThenBy(unit => unit.YUS_SystemCreateTimeUtc)
				.FirstOrDefault();

			return result;
		}

		#endregion
	}
}

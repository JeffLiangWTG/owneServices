using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsDataObjectReader<T> : ShipmentDataObjectReader<T>
		where T : BusinessObject
	{
		protected WhsDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		#region ClientAddress

		protected IColumnIndexer ClientAddress
		{
			get
			{
				if (clientAddress == null)
				{
					var clientAddressDO = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(OrganisationTypes.WarehouseClient));
					var addressBO = clientAddressDO != null ? new OrganisationDataObjectReader(clientAddressDO, logger, factory).GetMatched() : null;
					clientAddress = GetColumnIndexerFromRow(addressBO);

					if (clientAddress == null)
					{
						var errorMessage = Res.GetString("9e187a05-ae60-4893-b5f3-0ef1db246ab1", "Unable to match Client.");
						throw new DataObjectReadFailureException(errorMessage);
					}
				}

				return clientAddress;
			}
		}

		IColumnIndexer clientAddress;

		#endregion

		#region WarehousePK

		protected ZGuid WarehousePK
		{
			get
			{
				if (!warehousePK.HasValue)
				{
					var warehouseDO = dataObject.Order?.Warehouse;

					if (warehouseDO != null)
					{
						var query = new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, warehouseDO.Code);
						query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, new[] { WarehouseTypes.Codes.Product, WarehouseTypes.Codes.FreeTradeZone });
						query.MaximumRows = 1;

						var warehouse = factory.RowFactory.Load(WhsWarehouseSchema.Constants.TableName, query).SingleOrDefault();
						if (warehouse == null)
						{
							var errorMessage = Res.GetString("81ec20e6-2ceb-4c5d-8a53-608aaee960cb", "Unable to match Warehouse: {0}", warehouseDO.ToStringContents());
							throw new DataObjectReadFailureException(errorMessage);
						}

						warehousePK = GetColumnIndexerFromRow(warehouse).GetValue(WhsWarehouseSchema.PK);
					}
					else
					{
						throw new DataObjectReadFailureException(Res.GetString("a8ae0179-5928-48f9-8c08-93711e711b7c", "Could not import due to missing Warehouse Information."));
					}
				}

				return warehousePK.GetValueOrDefault();
			}
		}

		ZGuid? warehousePK;

		#endregion
	}
}

using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using WarehouseDO = Enterprise.UniversalDataBuss.DataObjects.Universal.Warehouse;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsVASOrderDataObjectWriter : TopLevelDataObjectWriter<WhsVASOrder, Shipment>
	{
		internal WhsVASOrderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		#region PopulateDataObject

		protected override void PopulateDataObject(WhsVASOrder vasOrderBO, Shipment vasOrderDO)
		{
			PopulateData(vasOrderBO, vasOrderDO);
			PopulateRelatedEntities(vasOrderBO, vasOrderDO);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		void PopulateData(WhsVASOrder vasOrderBO, Shipment vasOrderDO)
		{
			var orderDataObject = new Order(writeManager.WriterStrategy);
			orderDataObject.OrderNumber = vasOrderBO.WVO_CustomerReferenceNo;

			var serviceArea = vasOrderBO.ServiceArea;
			var warehouse = vasOrderBO.Warehouse;

			if (warehouse != null)
			{
				orderDataObject.StagingArea = serviceArea.WA_Name;
				orderDataObject.Warehouse = new WarehouseDO { Code = warehouse.WW_WarehouseCode, Name = warehouse.WW_WarehouseName };
			}

			vasOrderDO.Order = orderDataObject;
		}

		void PopulateRelatedEntities(WhsVASOrder vasOrderBO, Shipment vasOrderDO)
		{
			vasOrderDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { new OrganizationDataObjectWriter(writeManager, nameof(OrganisationTypes.WarehouseClient)).GetDataObject(vasOrderBO.Client?.MainAddress) });

			vasOrderDO.Order.SetOrderLineCollection(() => ProcessCollection(vasOrderBO.Lines, new WhsVASOrderLineDataObjectWriter(writeManager), CollectionContent.Complete));
		}

		#endregion

		#region Context

		#region GetEDIMessageSubType

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		#endregion

		#region GetTopLevelDataContextType

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.WarehouseVASOrder;
		}

		#endregion

		#endregion
	}
}

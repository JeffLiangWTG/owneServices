using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WarehouseVASOrderDataContextManager : ShipmentDataContextManager<WhsVASOrder>
	{
		#region Context

		public override ZString DataContextKey
		{
			get { return ParentBO.WVO_JobID; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseVASOrder; }
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		#endregion

		#region Events

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderNumber, ParentBO.WVO_CustomerReferenceNo);
			}

			return contextValues;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new WarehouseVASOrderEventParentFinder(factory, this, logger);
		}

		#endregion

		#region Matching

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(WhsVASOrderSchema.WVO_JobID, matchingValues.Key);
		}

		#endregion

		#region Shipments

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new WhsVASOrderDataObjectReader(shipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WhsVASOrderDataObjectWriter(writeManager);
		}

		#endregion

		#region Recipient Roles

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		#endregion
	}
}
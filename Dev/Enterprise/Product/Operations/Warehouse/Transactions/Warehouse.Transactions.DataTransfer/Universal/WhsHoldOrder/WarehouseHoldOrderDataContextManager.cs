using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WarehouseHoldOrderDataContextManager : ShipmentDataContextManager<WhsHoldOrder>
	{
		#region Context

		public override ZString DataContextKey
		{
			get { return ((IJobNumber)ParentBO).JobNumber; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseHoldOrder; }
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		#endregion

		#region Events

		public override bool ManagesEvents
		{
			get { return false; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		#endregion

		#region Matching

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return ZQuery.NoResultQuery;
		}

		#endregion

		#region Shipments

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new WhsHoldOrderDataObjectReader(shipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
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

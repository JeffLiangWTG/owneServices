using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteVehicleMovementDataContextManager : ShipmentDataContextManager<GteVehicleMovement>
	{
		public override bool ManagesShipments => true;

		public override DataContextType DataContextType => DataContextType.GateVehicleMovement;

		public override ZString DataContextKey => ParentBO.VehicleEntries.FirstOrDefault(x => x.GVE_IsIncoming)?.GVE_GateActionNumber ?? "";

		public override string DefaultOutputDirectory => SystemDataRegistry.Instance.GateVehicleMovementExportDirectory.Value;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZDBOnlyQuery(typeof(GteVehicleMovement));
			query.AddToFilter(GteVehicleMovementSchema.GVM_CancelledReason, SQLComparisonOperator.Equal, ZString.Empty);

			var subQuery = new ZDBOnlySubQuery(typeof(GteVehicleEntry), GteVehicleEntrySchema.GVE_GVM_VehicleMovement);
			subQuery.AddToFilter(GteVehicleEntrySchema.GVE_GateActionNumber, matchingValues.Key);
			subQuery.AddToFilter(GteVehicleEntrySchema.GVE_CancelledReason, SQLComparisonOperator.Equal, ZString.Empty);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return new List<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new GteVehicleMovementEventParentFinder(this, factory, logger);

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalDataBuss.DataObjects.Universal.Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new GteVehicleMovementDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new GteVehicleMovementDataObjectWriter(writeManager);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;
	}
}

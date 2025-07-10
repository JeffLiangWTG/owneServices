using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteBookingDataContextManager : ShipmentDataContextManager<GteBooking>
	{
		#region DataContext

		public override DataContextType DataContextType => DataContextType.GateBooking;

		public override ZString DataContextKey => ParentBO.GBK_ReferenceNumber;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) =>
			new ZQuery(GteBookingSchema.GBK_ReferenceNumber, matchingValues.Key);

		#endregion DataContext

		public override string DefaultOutputDirectory => SystemDataRegistry.Instance.GateBookingExportDirectory.Value;

		public override bool ManagesShipments => true;

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;

		#region Get Reader/Writer

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new GteBookingDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new GteBookingDataObjectWriter(writeManager);
		}

		#endregion Get Reader/Writer
	}
}

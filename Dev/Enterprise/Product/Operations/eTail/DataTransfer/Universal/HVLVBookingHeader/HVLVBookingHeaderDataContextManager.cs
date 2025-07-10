using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVBookingHeaderDataContextManager : ShipmentDataContextManager<HVLVBookingHeader>
	{
		public override DataContextType DataContextType => DataContextType.HVLVBookingHeader;

		public override ZString DataContextKey => ParentBO.HVH_BookingReference;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) =>
			new ZQuery(HVLVBookingHeaderSchema.HVH_BookingReference, matchingValues.Key);

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) =>
			new HVLVBookingHeaderEventParentFinder(this, factory, logger);

		public override string DefaultOutputDirectory => SystemDataRegistry.Instance.HVLVBookingHeaderExportDirectory.Value;

		public override bool ManagesShipments => true;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory) =>
			new HVLVBookingHeaderDataObjectReader(universalShipment, logger, factory);

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) =>
			new HVLVBookingHeaderDataObjectWriter(writeManager);
	}
}

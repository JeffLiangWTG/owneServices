using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class TripDataContextManager : ShipmentDataContextManager<Trip>, IShipmentDataContextManagerInternal
	{
		public override bool ManagesShipments => false;

		public override DataContextType DataContextType => DataContextType.USeManifestTrip;

		public override ZString DataContextKey => ParentBO.BH_JobReference;

		public override string DefaultOutputDirectory => "";

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) => new ZQuery(CusInBondHeaderSchema.BH_JobReference, matchingValues.Key);

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new TripEventParentFinder(this, factory, logger);

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalXml.Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (IsFromHVLVShipment(dataObject, out var hvlvShipmentDataObject))
			{
				return new HVLVTripDataObjectReader(dataObject, hvlvShipmentDataObject, logger, factory);
			}

			return null;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) => null;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;

		bool IsFromHVLVShipment(UniversalXml.Shipment universalShipment, out UniversalXml.Shipment hvlvShipmentDataObject)
		{
			var result = false;
			hvlvShipmentDataObject = universalShipment.SubShipmentCollection?.FirstOrDefault();

			if (hvlvShipmentDataObject != null)
			{
				result = hvlvShipmentDataObject.SubShipmentCollection?.Any(subShipment => subShipment.GetMatchingDataSource(DataContextType.HVLVConsignment) != null) ?? false;
			}

			return result;
		}
	}
}

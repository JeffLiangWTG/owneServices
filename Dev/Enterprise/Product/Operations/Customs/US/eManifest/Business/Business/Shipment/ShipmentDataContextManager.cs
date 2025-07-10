using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class ShipmentDataContextManager : ShipmentDataContextManager<Shipment>
	{
		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new ShipmentDataObjectWriter(writeManager);
		}

		public override DataContextType DataContextType => DataContextType.USeManifestShipment;

		public override ZString DataContextKey => "";

		public override string DefaultOutputDirectory => "";

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources,
			IXmlSessionTracker importSessionLogger) => false;

		public override bool ManagesShipments => false;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalXml.Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory) => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory,
			IXmlImportLogger logger) => null;
	}
}

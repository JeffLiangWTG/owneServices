using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVOriginLoadListDataContextManager : ShipmentDataContextManager<HVLVOriginLoadList>
	{
		public override DataContextType DataContextType => DataContextType.HVLVOriginLoadList;

		public override ZString DataContextKey => ParentBO.HVL_UniqueReference;

		public override string DefaultOutputDirectory => null;

		public override bool ManagesShipments => true;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) => new ZQuery(HVLVOriginLoadListSchema.HVL_UniqueReference, matchingValues.Key);

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory) => new HVLVOriginLoadListDataObjectReader(universalShipment, logger, factory);

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) => new HVLVOriginLoadListDataObjectWriter(writeManager);
	}
}

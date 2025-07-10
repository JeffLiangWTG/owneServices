using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVOuterPackageDataContextManager : ShipmentDataContextManager<HVLVOuterPackage>
	{
		public override bool ManagesShipments => true;

		public override DataContextType DataContextType => DataContextType.HVLVOuterPackage;

		public override ZString DataContextKey => ParentBO.HVO_PackageBarcode;

		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZQuery(HVLVOuterPackageSchema.HVO_PackageBarcode, matchingValues.Key);
			query.OrderBy = HVLVOuterPackageSchema.Constants.HVO_SystemCreateTimeUtc + " DESC";
			query.MaximumRows = 1;
			return query;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory) => new HVLVOuterPackageTopLevelDataObjectReader(universalShipment, logger, factory);

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) => new HVLVOuterPackageTopLevelDataObjectWriter(writeManager);

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;
	}
}

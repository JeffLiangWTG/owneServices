using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class USLVConsignmentDataContextManager : ShipmentDataContextManager<CusUSLVConsignment>
	{
		public override DataContextType DataContextType => DataContextType.USCustomsLVConsignment;

		public override ZString DataContextKey => "";

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new USLVConsignmentDataReader(universalShipment, logger, factory, null, null);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new USLVConsignmentDataWriter(writeManager);
		}

		public override bool ManagesShipments => true;

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		public override string DefaultOutputDirectory => null;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;
	}
}


using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class ZAAsycudaManifestHeaderDataContextManager : ShipmentDataContextManager<AsycudaManifestHeader>
	{
		public override bool ManagesShipments => true;

		public override bool ManagesEvents => true;

		public override DataContextType DataContextType => DataContextType.ZAOutTurn;

		public override ZString DataContextKey => ParentBO.AMA_JobReference;

		public override string DefaultOutputDirectory => string.Empty;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZQuery result = null;
			if (!matchingValues.Key.IsEmpty)
			{
				result = new ZQuery(AsycudaManifestHeaderSchema.AMA_JobReference, matchingValues.Key);
				result.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, AsycudaManifestHeader.ApplicationCode_Out);
			}
			return result;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new AsycudaManifestHeaderDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new AsycudaManifestHeaderDataObjectWriter(writeManager);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;
	}
}

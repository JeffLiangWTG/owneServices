using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.DataTransfer.Universal.Universal.PackageState.Writer;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitPackageStateDataContextManager : ShipmentDataContextManager<WhsItemPackageState>
	{
		#region Context

		public override ZString DataContextKey => ParentBO.Package.KP_PackageID;

		public override DataContextType DataContextType => DataContextType.TransitPackage;

		public override string DefaultOutputDirectory => null;

		#endregion

		#region Recipient Roles

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		#endregion

		#region Shipments

		public override bool ManagesShipments => true;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WhsTransitPackageStateDataObjectWriter(writeManager);
		}

		#endregion

		#region Events

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return ZQuery.NoResultQuery;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		#endregion
	}
}

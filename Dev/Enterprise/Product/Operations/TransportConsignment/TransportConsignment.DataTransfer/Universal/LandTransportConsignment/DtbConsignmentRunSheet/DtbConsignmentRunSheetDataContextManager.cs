using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class DtbConsignmentRunSheetDataContextManager : ShipmentDataContextManager<DtbConsignmentRunSheet>
	{
		#region DataContextKey

		public override ZString DataContextKey
		{
			get { return ParentBO.KG_RunSheetNumber; }
		}

		#endregion

		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportConsignmentRunSheet; }
		}

		#endregion

		#region DefaultOutputDirectory

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		#endregion

		#region ManagesShipments

		public override bool ManagesShipments
		{
			get { return true; }
		}

		#endregion

		#region ShipmentDataObjectWriter

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new DtbConsignmentRunSheetDataObjectWriter(writeManager);
		}

		#endregion

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return ZQuery.NoResultQuery;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}
	}
}



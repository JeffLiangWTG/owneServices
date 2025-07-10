using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class CFSReceiveDataContextManager : ShipmentDataContextManager<CFSReceive>
	{
		public override bool ManagesShipments => true;

		public override DataContextType DataContextType => DataContextType.CFSReceive;

		public override ZString DataContextKey => ParentBO.JSB_BookingId;

		public override string DefaultOutputDirectory => ZString.Empty;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZDBOnlyQuery query = null;

			var dataContextKey = matchingValues.Key;
			if (!dataContextKey.IsEmpty)
			{
				query = new ZDBOnlyQuery(typeof(JobSupplierBooking));
				query.AddToFilter(JobSupplierBookingSchema.JSB_BookingId, dataContextKey);
			}

			return query;
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
			return new CFSReceiveDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new CFSReceiveDataObjectWriter(writeManager);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class DtbCarrierBookingConsignmentDataContextManager : ShipmentDataContextManager<DtbCarrierBookingConsignment>
	{
		#region Implementation

		public override ZString DataContextKey
		{
			get { return ParentBO.LTC_JobID; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.CarrierBookingConsignment; }
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZDBOnlyQuery(typeof(DtbConsignment));
			query.AddToFilter(DtbConsignmentSchema.LTC_JobID, matchingValues.Key);

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

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var consignmentDataObject = SchemaVersionManager.Current == UniversalXmlSchema.Version_2012_11_DO_NOT_USE
				? universalShipment
				: UniversalShipment.GetSourceDataObject(universalShipment);

			return new DtbCarrierBookingConsignmentDataObjectReader(consignmentDataObject, logger, factory, ParentBO, universalShipment);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new DtbCarrierBookingConsignmentDataObjectWriter(writeManager);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		#endregion
	}
}



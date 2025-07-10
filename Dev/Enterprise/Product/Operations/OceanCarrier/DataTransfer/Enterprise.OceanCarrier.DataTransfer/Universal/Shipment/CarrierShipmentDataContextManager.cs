using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.OceanCarrier.Business;
using Enterprise.OceanCarrier.DataTransfer.Universal.Shipment;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.OceanCarrier.DataTransfer.Universal
{
	public sealed class CarrierShipmentDataContextManager : ShipmentDataContextManager<CarrierShipmentHeader>, IShipmentDataContextManager
	{
		public override bool ManagesShipments
		{
			get { return true; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.CarrierShipment; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.CSH_CarrierShipmentReference; }
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(CarrierShipmentHeaderSchema.CSH_CarrierShipmentReference, matchingValues.Key);
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>(2);
			result.AddIfNotEmpty(UniversalEvent.ContextTypes.CarrierShipmentReference, ParentBO.CSH_CarrierShipmentReference);
			return result;
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new CarrierShipmentEventParentFinder(factory, this, logger);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new CarrierShipmentDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new CarrierShipmentDataObjectWriter(writeManager);
		}
	}
}

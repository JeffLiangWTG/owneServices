using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveConsignmentDataContextManager : WhsTransitConsignmentDataContextManager<WhsItemReceiveConsignment>
	{
		#region Context

		public override ZString DataContextKey
		{
			get { return ParentBO.WRC_JobID; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransitReceive; }
		}

		#endregion

		#region Shipments

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (shipment.IsTransitWarehouseCombined())
			{
				WhsTransitLogHelper.LogStartOfReceiveInstruction(logger);
			}
			return new WhsTransitReceiveConsignmentDataObjectReader(shipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WhsTransitReceiveConsignmentDataObjectWriter(writeManager);
		}

		#endregion

		#region SupportedServiceType

		protected override IEnumerable<ServiceCodeType> SupportedServiceTypes => new[] { ServiceCodeType.TWR, ServiceCodeType.TWX };

		#endregion

		#region Event

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(WhsItemReceiveConsignmentSchema.WRC_JobID, matchingValues.Key);
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new WhsTransitReceiveConsignmentEventParentFinder(factory, this, logger);
		}

		#endregion
	}
}

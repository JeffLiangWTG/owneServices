using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.DataTransfer.Universal.Universal;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitDispatchConsignmentDataContextManager : WhsTransitConsignmentDataContextManager<WhsItemDispatchConsignment>
	{
		#region Context

		public override ZString DataContextKey
		{
			get { return ParentBO.WDC_JobID; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransitDispatch; }
		}

		#endregion

		#region Shipments

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			//var specificDCNAndIDTargetted = shipment.DataContext?.DataTargetCollection?.
			//		Any(t => t.Type.HasValue && t.Type.Value == DataContextType.TransitDispatch.ToString()
			//				&& t.Key.HasValue && !string.IsNullOrEmpty(t.Key.Value)) ?? false;
			//if (specificDCNAndIDTargetted)
			//{
			//	return new WhsTransitDispatchConsignmentDataObjectReader(shipment, logger, factory);
			//}
			return new WhsTransitDispatchConsignmentDataObjectReader(shipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WhsTransitDispatchConsignmentDataObjectWriter(writeManager);
		}

		#endregion

		#region SupportedServiceType

		protected override IEnumerable<ServiceCodeType> SupportedServiceTypes => new[] { ServiceCodeType.TWD, ServiceCodeType.TWP };

		#endregion

		#region Events

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(WhsItemDispatchConsignmentSchema.WDC_JobID, matchingValues.Key);
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new WhsTransitDispatchConsignmentEventParentFinder(factory, this, logger);
		}

		#endregion
	}
}

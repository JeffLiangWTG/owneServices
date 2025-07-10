using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Integration.Interfaces;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitDispatchConsolDataContextManager : WhsTransitConsignmentConsolDataContextManager<WhsTransitDispatchConsol>, IGateManagementFacilityDataContextManager
	{
		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransitDispatchConsol; }
		}

		#endregion

		#region SupportedServiceType

		protected override IEnumerable<ServiceCodeType> SupportedServiceTypes => new[] { ServiceCodeType.TWD, ServiceCodeType.TWP };

		#endregion

		#region GetShipmentDataObjectReader

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new WhsTransitDispatchConsolDataObjectReader(universalShipment, logger, factory);
		}

		#endregion

		public BusinessObject GetLinkedEntity(UniversalObjectFactory factory, IGteGateMovementBooking gateMovementBooking)
		{
			var linkQuery = new ZQuery(StmUniversalJobLinkSchema.UCL_SourceKey, gateMovementBooking.GBM_SourceReferenceNumber);
			var universalLink = factory.LoadTop1<StmUniversalJobLink>(linkQuery);
			if (universalLink == null)
			{
				return null;
			}

			if (gateMovementBooking.GBM_IsPickup)
			{
				var pickupQuery = new ZQuery(WhsItemDispatchTransportationUnitSchema.PK, universalLink.UCL_ParentID);
				return factory.LoadTop1<WhsItemDispatchTransportationUnit>(pickupQuery);
			}
			else
			{
				var deliveryQuery = new ZQuery(WhsItemReceiveTransportationUnitSchema.PK, universalLink.UCL_ParentID);
				return factory.LoadTop1<WhsItemReceiveTransportationUnit>(deliveryQuery);
			}
		}
	}
}

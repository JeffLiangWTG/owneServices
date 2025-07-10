using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Integration.Interfaces;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveConsolDataContextManager : WhsTransitConsignmentConsolDataContextManager<WhsTransitReceiveConsol>, IGateManagementFacilityDataContextManager
	{
		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransitReceiveConsol; }
		}

		#endregion

		#region SupportedRoleType

		protected override IEnumerable<ServiceCodeType> SupportedServiceTypes => new[] { ServiceCodeType.TWR, ServiceCodeType.TWX };

		#endregion

		#region GetShipmentDataObjectReader

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new WhsTransitReceiveConsolDataObjectReader(universalShipment, logger, factory);
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

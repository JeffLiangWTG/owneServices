using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public abstract class WhsTransitHeaderDataContextManager<T> : ShipmentDataContextManager<T>
		where T : BusinessObject, IItemHeader
	{
		#region Context

		public override string DefaultOutputDirectory => null;

		#endregion

		#region Events

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportReference, ParentBO.TransportReference);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLNumber, ParentBO.MasterBillNumber);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.ContainerNumber, ParentBO.ContainerNumber);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.DepotCode, ParentBO.Warehouse?.WarehouseAddress?.OA_Code ?? ZString.Empty);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.TimeOfArrival, ParentBO.GateInTime);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.ContainerISOCode, ParentBO.ContainerISOType);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.CarriersBookingReference, ParentBO.CarrierBookingReference);
			}

			return contextValues;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		protected override IEnumerable<IUniversalJobLink> GetEventDataTargetCore(RecipientRoleType recipientRole, IOrgHeader recipientOrganisation)
		{
			if (recipientRole == RecipientRoleType.GDM)
			{
				var links = UniversalJobLinkHelper.GetMatchingJobLinks(ParentBO, DataContextType.GateMovementBooking, null)?.ToList() ?? [];
				links.AddRange(UniversalJobLinkHelper.GetMatchingJobLinks(ParentBO, DataContextType.GateBooking, null)?.ToList() ?? []);
				return links.Count > 0 ? links : base.GetEventDataTargetCore(recipientRole, recipientOrganisation);
			}
			return base.GetEventDataTargetCore(recipientRole, recipientOrganisation);
		}

		#endregion

		#region Shipments

		public override bool ManagesShipments => true;

		#endregion

		#region Recipient Roles

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		#endregion
	}
}

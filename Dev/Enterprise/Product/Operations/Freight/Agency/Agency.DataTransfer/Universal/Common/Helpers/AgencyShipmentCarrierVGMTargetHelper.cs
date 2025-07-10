using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	static class AgencyShipmentCarrierVGMTargetHelper
	{
		public static ZBool IsProcessingAsAgencyBooking(IEnumerable<IRecipientRoleDataObject> recipientRoles, IXmlSessionTracker importSessionLogger)
		{
			return IsValidCarrierVGM(recipientRoles) && importSessionLogger != null ? !ContainsValidBillOfLading(importSessionLogger) : ZBool.True;
		}

		public static ZBool IsProcessingAsBillOfLading(IEnumerable<IRecipientRoleDataObject> recipientRoles, IXmlSessionTracker importSessionLogger)
		{
			return IsValidCarrierVGM(recipientRoles) && importSessionLogger != null ? ContainsValidBillOfLading(importSessionLogger) : ZBool.True;
		}

		static ZBool IsValidCarrierVGM(IEnumerable<IRecipientRoleDataObject> recipientRoles)
		{
			return AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && recipientRoles.Any(o => o.Code == RecipientRoleType.CAR && o.ServiceCode == ServiceCodeType.VGM);
		}

		static ZBool ContainsValidBillOfLading(IXmlSessionTracker importSessionLogger)
		{
			var dataObject = importSessionLogger.TopLevelDataObject as UniversalShipment;

			var oceanBillNumber = dataObject?.WayBillNumber.GetValueOrDefault() ?? ZString.Empty;
			var carriersBookingReference = dataObject?.BookingConfirmationReference.GetValueOrDefault() ?? ZString.Empty;
			var containerNumber = dataObject?.ContainerCollection?.FirstOrDefault()?.ContainerNumber ?? ZString.Empty;

			if (containerNumber.IsEmpty || (oceanBillNumber.IsEmpty && carriersBookingReference.IsEmpty))
			{
				return false;
			}

			var factory = new BusinessObjectFactory();
			var query = new ZQuery();

			var subShipmentQuery = new ZDBOnlyQuery(typeof(AgencyShipment));
			subShipmentQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
			subShipmentQuery.AddToFilter(JobShipmentSchema.JS_IsCancelled, false);
			subShipmentQuery.AddToFilter(JobShipmentSchema.JS_ShipmentStatus, ShipmentStatusHelperMethods.GetBillOfLadingStageStatus());

			var subContainerQuery = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			subContainerQuery.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNumber);
			subContainerQuery.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);
			subShipmentQuery.AddSubQuery(JobShipmentSchema.PK, subContainerQuery, JoinCondition.And);

			query.AddToFilter(subShipmentQuery);

			var subShipmentReferenceQuery = new ZDBOnlyQuery(typeof(AgencyShipment));
			if (!oceanBillNumber.IsEmpty)
			{
				subShipmentReferenceQuery.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_HouseBill, oceanBillNumber);
			}
			if (!carriersBookingReference.IsEmpty)
			{
				subShipmentReferenceQuery.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_CFSReference, carriersBookingReference);
			}

			query.AddToFilter(subShipmentReferenceQuery);

			return factory.LoadTop1<AgencyShipment>(query) != null;
		}
	}
}

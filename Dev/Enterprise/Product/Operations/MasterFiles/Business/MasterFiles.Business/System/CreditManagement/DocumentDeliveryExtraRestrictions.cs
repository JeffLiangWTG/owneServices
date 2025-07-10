using System;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.TransportBooking;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business
{
	class DocumentDeliveryExtraRestrictions
	{
		internal DocumentDeliveryExtraRestrictions(
					bool isDPSFreightMovementRestricted,
					bool isAviationSecurityFreightMovementRestricted,
					Type bizOType)
		{
			IsDPSFreightMovementRestricted = isDPSFreightMovementRestricted;
			IsAviationSecurityFreightMovementRestricted = isAviationSecurityFreightMovementRestricted;

			IsRestrictionForConsol = typeof(IForwardingConsol).IsAssignableFrom(bizOType);
			IsRestrictionForShipment = typeof(IForwardingShipment).IsAssignableFrom(bizOType);
			IsRestrictionForQuotedBooking = typeof(IQuotedBooking).IsAssignableFrom(bizOType);
			IsRestrictionForDtbBooking = typeof(IDtbBooking).IsAssignableFrom(bizOType);
			IsRestrictedForDeclaration = typeof(Enterprise.Integration.Customs.IBaseJobDeclaration).IsAssignableFrom(bizOType);
		}

		internal bool IsDPSFreightMovementRestricted { get; }
		internal bool IsAviationSecurityFreightMovementRestricted { get; }
		internal bool IsRestrictionForConsol { get; private set; }
		internal bool IsRestrictionForShipment { get; private set; }
		internal bool IsRestrictionForQuotedBooking { get; private set; }
		internal bool IsRestrictionForDtbBooking { get; private set; }
		internal bool IsRestrictedForDeclaration { get; private set; }

		internal bool IsRestricted =>
			IsDPSFreightMovementRestricted ||
			IsAviationSecurityFreightMovementRestricted;
	}
}

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IComplianceRiskStatusSupporter
	{
		ComplianceRiskStatusObject GetStatus(IComplianceItemRiskStatusProvider provider);

		/// <summary>
		/// Job direction can be NULL for non-international job types, e.g. warehouse
		/// </summary>
		bool IsDPSFreightMovementRestricted(ZString screeningStatus, IComplianceJobDirectionProvider jobDirection, BusinessObject parent);

		IEnumerable<ComplianceCommodity> FetchCommodityDetailsInDB(IComplianceItemRiskStatusProvider provider);

		void PreFetchHintsCommodityDetails(IComplianceItemRiskStatusProvider[] providers, BusinessObjectFactory factory);

		void CopyAssessmentDetailsFromBookingToShipment(ZGuid bookingPK, ZGuid shipmentPK, BusinessObjectFactory factory);

		void DeleteNotSavedAssessmentDetailsCopiedFromBooking(ZGuid shipmentPK, BusinessObjectFactory factory);

		void AddComplianceDocumentHoldStatusEventLog(IComplianceItemRiskStatusProvider provider, ZString eventUser, ZString documentName);
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
namespace Enterprise.Warehouse.Integration
{
	public interface IWhsDocket : IBusiness
	{
		ZGuid PK { get; }
		object this[string propertyName] { get; set; }

		ZDateTimeOffset WD_ArrivalDate { get; set; }
		ZDateTimeOffset WD_RequiredDate { get; set; }
		ZString WD_CustomerReference { get; set; }
		ZString WD_DocketID { get; set; }
		ZString WD_DocketSubType { get; set; }
		ZString WD_ReceiveCategory { get; set; }
		ZString WD_DocketType { get; set; }
		ZString WD_DocketStatus { get; set; }
		ZString WD_ExternalReference { get; set; }
		ZDateTimeOffset WD_FinalisedDate { get; set; }
		ZDecimal WD_TotalUnits { get; set; }
		ZGuid WD_OH_Client { get; set; }
		ZGuid WD_WW_Whs { get; set; }
		ZGuid SupplierPK { get; }
		ZGuid WD_WP { get; set; }
		ZString WD_RS_NKServiceLevel { get; set; }
		ZString WD_PL_NKCarrierServiceLevel { get; }
		ZGuid WD_WP_ParentPickForTransfer { get; set; }
		ZString WD_TransportReference { get; }
		ZString WD_CustomsParentReference { get; set; }
		ZByte WD_ExternalReferenceSplit { get; set; }
		bool IsCustomsTransaction { get; }
		ZBool WD_IsInwardsProcessingJob { get; set; }
		ZString WD_TaskPlanningStatus { get; set; }
		void FinaliseDocketWithoutUserConfirmation();
	}
}

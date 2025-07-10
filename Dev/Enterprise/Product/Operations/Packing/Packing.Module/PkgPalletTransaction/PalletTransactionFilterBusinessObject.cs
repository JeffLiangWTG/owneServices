using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.Packing.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Module
{
	public class PalletTransactionFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Docket ID", PkgPalletTransactionSchema.KTR_PaperDocketID).MultilingualDescription = ResString.GetMultilingualString("Packing|PalletFilter|DocketID", "Docket ID");
			filters.AddTextFilter("Transaction Type", PkgPalletTransactionSchema.KTR_TransactionType, () => new PalletTransactionTypeList()).MultilingualDescription = ResString.GetMultilingualString("Packing|PalletFilter|TransactionType", "Transaction Type");
			filters.AddTextFilter("Pallet Type", PkgPalletTransactionSchema.KTR_PalletType, () => PackingRegistry.Instance.PalletTypes.Value.Types.GetCodeDescriptionPairList()).MultilingualDescription = ResString.GetMultilingualString("Packing|PalletFilter|PalletType", "Pallet Type");
			filters.AddTextFilter("Equipment Code", PkgPalletTransactionSchema.KTR_EquipmentCode).MultilingualDescription = ResString.GetMultilingualString("Packing|PalletFilter|EquipmentCode", "Equipment Code");
			filters.AddTextFilter("Status", PkgPalletTransactionSchema.KTR_Status, () => new PalletTransactionStatusList()).MultilingualDescription = ResString.GetMultilingualString("Packing|PalletFilter|Status", "Status");
			filters.AddNumberRangeFilter("Quantity", PkgPalletTransactionSchema.KTR_Quantity).MultilingualDescription = ResString.GetMultilingualString("Packing|PalletFilter|Quantity", "Quantity");
			filters.AddDateFilter("Transaction Date/Time", PkgPalletTransactionSchema.KTR_SystemCreateTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("Packing|PalletFilter|Date", "Transaction Date/Time");

			var jobReferenceNumberFilter = filters.AddTextFilter("Job Reference Number", GetJobReferenceQuery);
			jobReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Packing|PalletFilter|JobRef", "Job Reference Number");
			jobReferenceNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;

			AddNumbersAndReferencesFilters(filters);

			return filters;
		}

		#region AddNumbersAndReferencesFilters

		void AddNumbersAndReferencesFilters(ModuleFilterCollection moduleFilters)
		{
			var consignmentIDFilter = moduleFilters.AddTextFilter("ConsignmentID", GetConsignmentIDQuery);
			consignmentIDFilter.MaxLength = DtbBookingSchema.KM_JobID.MaxLength;
			consignmentIDFilter.MultilingualDescription = ResString.GetMultilingualString("Packing|PalletFilter|ConsignmentID", "Consignment ID");
			consignmentIDFilter.Category = FilterCategories.NumbersAndReferences;
			consignmentIDFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			consignmentIDFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var conNoteNoFilter = moduleFilters.AddTextFilter("ConnoteNo", GetConnnoteNoQuery);
			conNoteNoFilter.MaxLength = DtbBookingSchema.KM_TransportReference.MaxLength;
			conNoteNoFilter.MultilingualDescription = ResString.GetMultilingualString("Packing|PalletFilter|ConnoteNo", "Connote No.");
			conNoteNoFilter.Category = FilterCategories.NumbersAndReferences;
			conNoteNoFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			conNoteNoFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
		}

		#endregion

		#region Number and Reference Filters

		ZQuery GetConsignmentIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetConsignmentDetailsQuery(DtbBookingSchema.KM_JobID, comparisonOperator, value);
		}

		ZQuery GetConnnoteNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetConsignmentDetailsQuery(DtbBookingSchema.KM_TransportReference, comparisonOperator, value);
		}

		static ZQuery GetConsignmentDetailsQuery(SchemaStringColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var bookingSubQuery = new ZDBOnlySubQuery(typeof(IDtbBooking), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			bookingSubQuery.AddToFilter(column, comparisonOperator, value);

			var instructionSubQuery = new ZDBOnlySubQuery(typeof(IDtbBookingInstruction), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
			instructionSubQuery.AddSubQuery(bookingSubQuery, JoinCondition.And);

			var confirmationSubQuery = new ZDBOnlySubQuery(typeof(IDtbConsignmentConfirmation), DtbBookingConfirmationSchema.PK);
			confirmationSubQuery.AddSubQuery(instructionSubQuery, JoinCondition.And);

			var palletTransactionQuery = new ZDBOnlyQuery(typeof(PkgPalletTransaction));
			palletTransactionQuery.AddSubQuery(PkgPalletTransactionSchema.KTR_ParentID, confirmationSubQuery, JoinCondition.And);
			return palletTransactionQuery;
		}

		#endregion

		ZQuery GetJobReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(PkgPalletTransaction));
			var subQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}
	}
}

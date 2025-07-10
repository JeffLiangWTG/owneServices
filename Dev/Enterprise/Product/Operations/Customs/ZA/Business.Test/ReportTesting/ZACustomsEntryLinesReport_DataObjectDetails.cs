using System;
using System.Collections.Generic;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class ZACustomsEntryLinesReport_DataObjectDetails : ReportDataObjectDetails
	{
		public ZACustomsEntryLinesReport_DataObjectDetails()
		{
			ObjectName = "Report_ZACustomsEntryLines";
			SqlObjectType = Enterprise.ReportTesting.SqlObjectType.FunctionTable;
			ExpectedColumnsInOrder = PopulateExpectedColumnsInOrder();
			PopulateParameterDetails();
		}

		List<ReportSchemaColumn> PopulateExpectedColumnsInOrder()
		{
			var columns = new List<ReportSchemaColumn>()
			{ new ReportSchemaColumn(typeof(string), "JobNumber"), new ReportSchemaColumn(typeof(string), "EntryNumber"), new ReportSchemaColumn(typeof(short), "EntryLineNo"), new ReportSchemaColumn(typeof(DateTime), "EntryReleaseDate"), new ReportSchemaColumn(typeof(string), "ShipmentType"), new ReportSchemaColumn(typeof(string), "MessageStatus"), new ReportSchemaColumn(typeof(string), "EntryStatus"), new ReportSchemaColumn(typeof(DateTime), "EntrySubmittedDate"), new ReportSchemaColumn(typeof(decimal), "CustomsValue"), new ReportSchemaColumn(typeof(string), "TariffCode"), new ReportSchemaColumn(typeof(string), "CPC"), new ReportSchemaColumn(typeof(DateTime), "AssessmentDate"), new ReportSchemaColumn(typeof(string), "VesselName"), new ReportSchemaColumn(typeof(string), "VoyFlight"), new ReportSchemaColumn(typeof(DateTime), "ETA"), new ReportSchemaColumn(typeof(string), "TransportMode"), new ReportSchemaColumn(typeof(string), "ClientRef"), new ReportSchemaColumn(typeof(string), "TransportDocNo"), new ReportSchemaColumn(typeof(string), "HouseBill"), new ReportSchemaColumn(typeof(string), "CustomsOffice"), new ReportSchemaColumn(typeof(DateTime), "JobRegisteredDate"), new ReportSchemaColumn(typeof(decimal), "StatsQty"), new ReportSchemaColumn(typeof(decimal), "AdditionalQty1"), new ReportSchemaColumn(typeof(decimal), "AdditionalQty2"), new ReportSchemaColumn(typeof(string), "StatsQtyUnit"), new ReportSchemaColumn(typeof(string), "OrderNumber"), new ReportSchemaColumn(typeof(string), "ProductCode"), new ReportSchemaColumn(typeof(string), "GoodsOrigin"), new ReportSchemaColumn(typeof(string), "AdditionalQty1Unit"), new ReportSchemaColumn(typeof(string), "AdditionalQty2Unit"), new ReportSchemaColumn(typeof(string), "FirstContainerNo"), new ReportSchemaColumn(typeof(int), "NoOfContainers"), new ReportSchemaColumn(typeof(string), "ImporterCode"), new ReportSchemaColumn(typeof(Guid), "ImporterPK"), new ReportSchemaColumn(typeof(string), "SupplierCode"), new ReportSchemaColumn(typeof(Guid), "SupplierPK"), new ReportSchemaColumn(typeof(Guid), "BranchPk"), new ReportSchemaColumn(typeof(Guid), "EntryHeaderPk"), new ReportSchemaColumn(typeof(Guid), "EntryLinePk") };
			return columns;
		}

		void PopulateParameterDetails()
		{
			AddParameter("@CurrentCompany", typeof(Guid));
			AddParameter("@ShipmentType", typeof(string));
			AddParameter("@TransportMode", typeof(string));
			AddParameter("@CustomsOffice", typeof(string));
			AddParameter("@JobRegistedOnFrom", typeof(DateTime));
			AddParameter("@JobRegistedOnTo", typeof(DateTime));
			AddParameter("@EntrySubmittedDateFrom", typeof(DateTime));
			AddParameter("@EntrySubmittedDateTo", typeof(DateTime));
			AddParameter("@AssessmentDateFrom", typeof(DateTime));
			AddParameter("@AssessmentDateTo", typeof(DateTime));
			AddParameter("@ProcedureCode", typeof(string));
			AddParameter("@EntryStatus", typeof(string));
			AddParameter("@MessageStatus", typeof(string));
			AddParameter("@EntryReleaseDateFrom", typeof(DateTime));
			AddParameter("@EntryReleaseDateTo", typeof(DateTime));
		}
	}
}

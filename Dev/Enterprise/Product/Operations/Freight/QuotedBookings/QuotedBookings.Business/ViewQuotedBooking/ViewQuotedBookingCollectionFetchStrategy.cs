using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.ZQueryHelpers;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EUExitControl;

namespace Enterprise.Freight.QuotedBookings.Business
{
	sealed class ViewQuotedBookingCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public ViewQuotedBookingCollectionFetchStrategy(IBusinessObjectCollection collection)
			: base(collection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			if (columns.Any(col => col.ColumnName.StartsWith("WorkflowItems+MilestonesIncludingRelated", StringComparison.OrdinalIgnoreCase)))
			{
				foreach (ViewQuotedBooking viewQuotedBooking in businessObjects)
				{
					viewQuotedBooking.Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(AsycudaBillSchema.ABL_JS_Shipment, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(CusInBondHeaderSchema.BH_ParentID, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(CusSCAHouseSchema.CA_JS, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(DtbBookingConsolidationSchema.KB_ParentID, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(JobCartageSchema.JJ_ParentID, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(WhsDocketJobPivotSchema.WV_ParentId, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(JobPickupDeliveryConfirmSchema.EU_JS, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(JobDeclarationSchema.JE_JS, viewQuotedBooking.VB_JS);
					viewQuotedBooking.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, viewQuotedBooking.VB_TH);
					viewQuotedBooking.Factory.AddFetchHint(DtbBookingConsolidationSchema.KB_ParentID, viewQuotedBooking.VB_TH);
					viewQuotedBooking.Factory.AddFetchHint(JobDocumentDataSchema.Instance, JobDocumentDataZQueryFilters.GetIndexedQuery(viewQuotedBooking.VB_JS, JobShipmentSchema.Constants.Prefix));

					var shipment = viewQuotedBooking.QuotedBooking?.Booking;
					if (shipment != null)
					{
						var invoiceLoader = new InvoiceLoader(viewQuotedBooking.Factory);
						viewQuotedBooking.Factory.AddFetchHint(typeof(AccTransactionHeader), invoiceLoader.GetInvoicesForUniqueRefQuery(null, shipment.JS_UniqueConsignRef));
						if (shipment.IsExport())
						{
							viewQuotedBooking.Factory.AddFetchHint(CusExitHeaderSchema.CXH_ParentID, shipment.PK);
							viewQuotedBooking.Factory.AddFetchHint(CusExitHeaderSchema.CXH_ParentTableCode, (ZString)JobShipmentSchema.Constants.Prefix);

							var exitHeaderLoader = ObjectFactory.Get<ICusExitHeaderLoader>("EUExitControl.ICusExitHeaderLoader", viewQuotedBooking.Factory);
							var exitHeader = exitHeaderLoader.Load(!shipment.IsInDatabase, shipment.PK, JobShipmentSchema.Constants.Prefix).SingleOrDefault();
							if (exitHeader != null)
							{
								viewQuotedBooking.Factory.AddFetchHint(CusExitReportSchema.CER_ClusterKey, exitHeader.CXH_ClusterKey);
							}
						}
					}
				}
			}

			base.FetchForViewCore(businessObjects, columns);
		}
	}
}

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateWorkOrdersActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public GenerateWorkOrdersActionMethodApplicator()
			: base(Res.GetString("66D7EFC8-4230-43B0-A71C-676ABBB17F76", "Generate Work Orders")) // text used for logging
		{
		}

		#region Generate Work Orders

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] dockets)
		{
			log.SetSectionProgressMax(dockets.Length);

			AddFetchHintsIfRequired(dockets);

			foreach (WhsOrder order in dockets)
			{
				GenerateWorkOrders(order, log);
				log.BumpSectionProgress();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		void AddFetchHintsIfRequired(BusinessObject[] dockets)
		{
			var orders = dockets.Cast<WhsOrder>().ToArray();
			if (orders.Length > 0)
			{
				var factory = orders[0].Factory;
				foreach (var order in orders)
				{
					factory.AddFetchHint(WhsDocketJobPivotSchema.WV_WD_Docket, order.PK);

					var bookingConsolQuery = new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, order.PK);
					bookingConsolQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, SQLComparisonOperator.NotEqual, TransportConsolidationJobTypes.Codes.Consignment);
					factory.AddFetchHint(DtbBookingConsolidationSchema.Instance, bookingConsolQuery);

					var jobCartageQuery = new ZQuery(JobCartageSchema.JJ_ParentID, order.PK);
					jobCartageQuery.AddToFilter(JobCartageSchema.JJ_IsCancelled, SQLComparisonOperator.Equal, 0);
					factory.AddFetchHint(JobCartageSchema.Instance, jobCartageQuery);

					var processTasksQuery = new ZQuery();
					processTasksQuery.AddToFilter(ProcessTasksSchema.P9_ParentID, order.PK);
					factory.AddFetchHint(ProcessTasksSchema.Instance, processTasksQuery);

					var workOrderQuery = new ZQuery();
					workOrderQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.WorkOrder);
					workOrderQuery.AddToFilter(WhsDocketSchema.WD_WD_ParentDocket, order.PK);
					workOrderQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);
					workOrderQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Cancelled);
					factory.AddFetchHint(WhsDocketSchema.Instance, workOrderQuery);

					var warehouseQuery = new ZQuery();
					warehouseQuery.AddToFilter(WhsWarehouseSchema.PK, order.WD_WW_Whs);
					factory.AddFetchHint(WhsWarehouseSchema.Instance, warehouseQuery);

					factory.AddFetchHint(WhsDocketLineSchema.WE_WD, order.PK);
					factory.AddFetchHint(OrgHeaderSchema.PK, order.WD_OH_Client);
					factory.AddFetchHint(OrgMiscServSchema.OM_OH, order.WD_OH_Client);

					var orgCompanyDataquery = new ZQuery(OrgCompanyDataSchema.OB_OH, order.WD_OH_Client);
					orgCompanyDataquery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					factory.AddFetchHint(OrgCompanyDataSchema.Instance, orgCompanyDataquery);
				}

				foreach (var warehouse in orders.Select(d => d.Warehouse))
				{
					factory.AddFetchHint(GlbBranchSchema.PK, warehouse.WW_GB_RelatedCompanyBranch);
					factory.AddFetchHint(OrgAddressSchema.PK, warehouse.WW_OA_WarehouseAddress);
					factory.AddFetchHint(WhsAreaSchema.WA_WW_Whs, warehouse.PK);
				}

				foreach (var productPK in orders.SelectMany(o => o.Lines).Select(l => l.WE_OP).Distinct())
				{
					factory.AddFetchHint(OrgSupplierPartSchema.PK, productPK);
					factory.AddFetchHint(OrgPartBOMSchema.OE_OP_MainProduct, productPK);
					factory.AddFetchHint(WhsProductParamsByWhsAndClientSchema.W3_OP, productPK);
				}

				foreach (var bom in orders.SelectMany(o => o.Lines).SelectMany(l => l.SupplierPart.BillOfMaterials))
				{
					factory.AddFetchHint(OrgSupplierPartSchema.PK, bom.OE_OP_Component);
					factory.AddFetchHint(WhsProductParamsByWhsAndClientSchema.W3_OP, bom.OE_OP_Component);
					factory.AddFetchHint(OrgPartRelationSchema.OU_OP, bom.OE_OP_Component);

					var bomCompQuery = new ZQuery();
					bomCompQuery.AddToFilter(OrgPartBOMSchema.OE_OP_MainProduct, bom.OE_OP_Component);
					bomCompQuery.AddToFilter(JoinCondition.Or, OrgPartBOMSchema.OE_OP_Component, bom.OE_OP_Component);
					factory.AddFetchHint(OrgPartBOMSchema.Instance, bomCompQuery);
				}
			}
		}

		void GenerateWorkOrders(WhsOrder order, IOperationalActionSectionLog log)
		{
			var notify = new NotificationsLogger(GetDocketIdLink(order), log);
			order.BOM.AutoCreateWorkOrders(notify);
		}

		#endregion

		#region NotificationsLogger

		class NotificationsLogger : NotificationBufferWithDefaultResponse
		{
			public NotificationsLogger(LogControllerLink orderLink, IOperationalActionSectionLog log)
			{
				OrderLink = orderLink;
				Log = log;
			}

			public override void Notify(INotification notification)
			{
				var msg = notification.Message.Replace("\r\n", " ");

				var message = Res.GetString("CC550127-23E1-4DB7-B3BF-62F842610456", "Order {0} - {1}", "{0}", msg);

				Log.NotifyFormat(GetErrorLevel(notification.Type), message, OrderLink);
			}

			OperationalActionLogErrorLevel GetErrorLevel(INotificationType notificationType)
			{
				OperationalActionLogErrorLevel result;

				if (notificationType.EnumValueName == nameof(NotificationTypes.Error) || notificationType.EnumValueName == nameof(NotificationTypes.Warning))
				{
					result = OperationalActionLogErrorLevel.Warning; // if we set an error, the entire op action will be say "Aborted".
				}
				else
				{
					result = OperationalActionLogErrorLevel.Informational;
				}

				return result;
			}

			readonly LogControllerLink OrderLink;
			readonly IOperationalActionSectionLog Log;
		}

		#endregion
	}
}

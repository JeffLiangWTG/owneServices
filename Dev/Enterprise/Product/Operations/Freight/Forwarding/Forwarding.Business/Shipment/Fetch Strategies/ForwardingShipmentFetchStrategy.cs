using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	internal sealed class ForwardingShipmentFetchStrategy : ShipmentFetchStrategy
	{
		public ForwardingShipmentFetchStrategy(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override void FetchForValidateCore()
		{
			ForwardingShipment shipment = BusinessObject as ForwardingShipment;

			base.FetchForValidateCore();
			Factory.AddFetchHint(JobOrderHeaderSchema.JD_JS, shipment.PK);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			var shipment = (ForwardingShipment)BusinessObject;

			base.FetchForViewCore(columns);

			var requiresConsol = false;
			var requiresJobMawbs = false;
			var requiresOrders = false;
			var requiresShipmentEntryNumbers = false;
			var requiresShipmentProcessTasks = false;
			var requiresWarehouseOrders = false;
			var requiresJobOrderItem = false;
			var requiresUNDGDataItem = false;

			foreach (TableColumn column in columns)
			{
				switch (column.ColumnName)
				{
					case ForwardingModuleShipment.Schema.JS_JK_ConsolID:
					case ForwardingModuleShipment.Schema.JS_JK_MasterBillNum:
					case ForwardingModuleShipment.Schema.JS_JK_ReceivingAgent:
					case ForwardingModuleShipment.Schema.JS_JK_SendingAgent:
					case ForwardingModuleShipment.Schema.JS_JK_Vessel:
					case ForwardingModuleShipment.Schema.JS_JK_VoyageFlight:
					case ForwardingModuleShipment.Schema.JS_Calc_PossibleOversize:
						requiresConsol = true;
						break;

					case ForwardingModuleShipment.Schema.JS_Calc_ImportManifestStatus:
					case ForwardingModuleShipment.Schema.EntryNumberStatus:
						requiresShipmentEntryNumbers = true;
						break;

					case ForwardingModuleShipment.Schema.JS_GenericOrderNumbers:
						requiresOrders = true;
						requiresWarehouseOrders = true;
						break;

					case nameof(ForwardingShipment.JS_OrderReferences):
						requiresJobOrderItem = true;
						break;

					case nameof(ForwardingShipment.IsHazardous):
					case nameof(ForwardingShipment.JS_Calc_DGClass):
					case nameof(ForwardingShipment.JS_Calc_DGSubstance):
					case nameof(ForwardingShipment.JS_Calc_DGPSA):
					case nameof(ForwardingShipment.JS_Calc_DIHazardousWasteCode):
					case nameof(ForwardingShipment.JS_Calc_DISpecialPermitNumber):
					case nameof(ForwardingShipment.JS_Calc_DISpecialPermitIssueDate):
					case nameof(ForwardingShipment.JS_Calc_DIIsSalvagePackaging):
					case nameof(ForwardingShipment.JS_Calc_DIIsResidueLastContained):
						requiresUNDGDataItem = true;
						break;

					case $"{nameof(ForwardingShipment.Job)}+{nameof(ForwardingShipment.Job.JH_TotalProfitRevenueMargin)}":
						requiresWarehouseOrders = true;
						break;

					default:
						if (column.ColumnName.StartsWith("WorkflowItems+", StringComparison.Ordinal))
						{
							requiresShipmentProcessTasks = true;
						}
						break;
				}
			}

			if (requiresConsol)
			{
				AddConsolFetchHint();
			}

			if (requiresJobMawbs)
			{
				Factory.AddFetchHint(JobMawbSchema.JM_ParentID, BusinessObject.PK);
			}

			if (requiresShipmentEntryNumbers)
			{
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			}

			if (requiresShipmentProcessTasks)
			{
				Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, BusinessObject.PK);
			}

			if (requiresOrders)
			{
				Factory.AddFetchHint(JobOrderHeaderSchema.JD_JS, BusinessObject.PK);
			}

			if (requiresWarehouseOrders)
			{
				Factory.AddFetchHint(WhsDocketJobPivotSchema.WV_ParentId, BusinessObject.PK);
			}

			if (requiresJobOrderItem && shipment.DocsAndCartage != null)
			{
				Factory.AddFetchHint(JobOrderItemSchema.JT_JP, shipment.DocsAndCartage.PK);
			}

			if (requiresUNDGDataItem)
			{
				foreach (var packline in shipment.OuterPackLines)
				{
					Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, packline.PK);
				}
			}
		}

		protected override void AddCusHawbFetchHint()
		{
			Factory.AddFetchHint(CusHAWBSchema.CS_JS, BusinessObject.PK);
		}

		protected override void AddSCAHouseFetchHint()
		{
			Factory.AddFetchHint(CusSCAHouseSchema.CA_JS, BusinessObject.PK);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();

			var shipment = BusinessObject as ForwardingShipment;
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, shipment.PK);
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, shipment.PK);
		}
	}
}

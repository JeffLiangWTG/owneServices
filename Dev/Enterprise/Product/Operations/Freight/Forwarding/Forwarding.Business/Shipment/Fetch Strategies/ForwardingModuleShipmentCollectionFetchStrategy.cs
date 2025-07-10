using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.ZQueryHelpers;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Forwarding.Business
{
	class ForwardingModuleShipmentCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public ForwardingModuleShipmentCollectionFetchStrategy(ForwardingModuleShipmentCollection collection)
			: base(collection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			if (columns.Any(col => col.ColumnName.StartsWith("WorkflowItems+MilestonesIncludingRelated", StringComparison.OrdinalIgnoreCase)))
			{
				foreach (ForwardingModuleShipment shipment in businessObjects)
				{
					var jobQuery = new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK);
					jobQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
					shipment.Factory.AddFetchHint(typeof(JobHeader), jobQuery);

					shipment.Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, shipment.PK);
					shipment.Factory.AddFetchHint(AsycudaBillSchema.ABL_JS_Shipment, shipment.PK);
					shipment.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, shipment.PK);
					shipment.Factory.AddFetchHint(CusHAWBSchema.CS_JS, shipment.PK);
					shipment.Factory.AddFetchHint(CusInBondHeaderSchema.BH_ParentID, shipment.PK);
					shipment.Factory.AddFetchHint(CusCAeMHHouseSchema.BW_ParentID, shipment.PK);
					shipment.Factory.AddFetchHint(CusSCAHouseSchema.CA_JS, shipment.PK);
					shipment.Factory.AddFetchHint(DtbBookingConsolidationSchema.KB_ParentID, shipment.PK);
					shipment.Factory.AddFetchHint(JobCartageSchema.JJ_ParentID, shipment.PK);
					shipment.Factory.AddFetchHint(WhsDocketJobPivotSchema.WV_ParentId, shipment.PK);
					shipment.Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, shipment.PK);
					shipment.Factory.AddFetchHint(JobPickupDeliveryConfirmSchema.EU_JS, shipment.PK);
					shipment.Factory.AddFetchHint(JobDocumentDataSchema.Instance, JobDocumentDataZQueryFilters.GetIndexedQuery(shipment.PK, shipment.TablePrefix));

					var invoiceLoader = new InvoiceLoader(shipment.Factory);
					shipment.Factory.AddFetchHint(typeof(AccTransactionHeader), invoiceLoader.GetInvoicesForUniqueRefQuery(null, shipment.JS_UniqueConsignRef));

					if (shipment.IsExport())
					{
						var exitHeaderLoader = ObjectFactory.Get<EUExitControl.ICusExitHeaderLoader>("EUExitControl.ICusExitHeaderLoader", shipment.Factory);
						(var mainQuery, var secondaryQuery) = exitHeaderLoader.GetLoadQuery(shipment.PK, JobShipmentSchema.Constants.Prefix);
						shipment.Factory.AddFetchHint(CusExitHeaderSchema.Instance, mainQuery, secondaryQuery);

						var exitHeader = exitHeaderLoader.Load(!shipment.IsInDatabase, shipment.PK, JobShipmentSchema.Constants.Prefix).SingleOrDefault();
						if (exitHeader != null)
						{
							shipment.Factory.AddFetchHint(CusExitReportSchema.CER_ClusterKey, exitHeader.CXH_ClusterKey);
						}
					}

					AddConsolFetchHint(shipment);

					foreach (var declaration in shipment.Declarations)
					{
						shipment.Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, declaration.PK);
						shipment.Factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
					}
				}

				foreach (ForwardingModuleShipment shipment in businessObjects)
				{
					var inBondHeader = (ZArchitecture.EnterpriseBusinessObject)shipment.InBondHeader;
					if (inBondHeader != null)
					{
						shipment.Factory.AddFetchHint(CusInBondMoveHeaderSchema.BM_BH, inBondHeader.PK);
					}
				}

				foreach (ForwardingModuleShipment shipment in businessObjects)
				{
					AddGlobalManifestsFetchHint(shipment);
				}

				foreach (ForwardingModuleShipment shipment in businessObjects)
				{
					AddProcessTasksFetchHint(shipment);
				}
			}
			else if (columns.Any(col => col.ColumnName == ForwardingShipment.Schema.DeliveryGoodsSignedForBy || col.ColumnName == ForwardingShipment.Schema.PickupGoodsSignedForBy))
			{
				foreach (ForwardingModuleShipment shipment in businessObjects)
				{
					AddConsolFetchHint(shipment);
					shipment.Factory.AddFetchHint(JobPackLinesSchema.JL_JS, shipment.PK);
				}

				foreach (ForwardingModuleShipment shipment in businessObjects)
				{
					AddPickupDeliveryFetchHint(shipment);
				}
			}
			else if (columns.Any(col => col.ColumnName == nameof(ForwardingModuleShipment.IsHazardous)))
			{
				foreach (ForwardingModuleShipment shipment in businessObjects)
				{
					AddCommodityAttributeFetchHint(shipment);
				}
			}
			else if (columns.Any(col => col.ColumnName == nameof(ForwardingModuleShipment.CommodityCodes)))
			{
				foreach (ForwardingModuleShipment shipment in businessObjects)
				{
					AddCommodityCodeFetchHint(shipment);
				}
			}
			else if (AreColumnsShowingPresenceOfNotes(columns))
			{
				AddNotesFetchHints(businessObjects);
			}

			base.FetchForViewCore(businessObjects, columns);
		}

		bool AreColumnsShowingPresenceOfNotes(TableColumn[] columns)
		{
			return columns.Any(col => col.ColumnName.StartsWith(nameof(ForwardingShipment.NotesChecker), StringComparison.OrdinalIgnoreCase));
		}

		#region Fetch Hints Implementation

		void AddConsolFetchHint(ForwardingModuleShipment shipment)
		{
			var consolShipmentPivotQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
			consolShipmentPivotQuery.AddToFilter(JobConShipLinkSchema.JN_JS, shipment.PK);

			var consolsQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
			consolsQuery.AddSubQuery(consolShipmentPivotQuery, JoinCondition.And);

			shipment.Factory.AddFetchHint(typeof(ForwardingConsol), consolsQuery);
		}

		void AddProcessTasksFetchHint(ForwardingShipment shipment)
		{
			foreach (var workflowProvider in shipment.BusinessObjectsWithRelatedEvents.Where(bo => bo is IWorkflowProvider))
			{
				shipment.Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, workflowProvider.PK);
			}
		}

		void AddGlobalManifestsFetchHint(ForwardingShipment shipment)
		{
			foreach (ForwardingConsol consol in shipment.Consols)
			{
				var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, consol.PK);
				query.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, Enterprise.Integration.Customs.ASYCUDA.ApplicationCodeTypes.Consolidator);
				shipment.Factory.AddFetchHint(AsycudaManifestHeaderSchema.Instance, query);
			}
		}

		void AddPickupDeliveryFetchHint(ForwardingModuleShipment shipment)
		{
			shipment.Factory.AddFetchHint(JobPickupDeliveryConfirmSchema.EU_JS, shipment.PK);

			foreach (CommonContainer container in shipment.Containers)
			{
				shipment.Factory.AddFetchHint(typeof(CommonPickupDeliveryConfirm), JobPickupDeliveryConfirmSchema.EU_JC, container.PK);
			}
		}

		void AddNotesFetchHints(BusinessObject[] businessObjects)
		{
			foreach (ForwardingModuleShipment shipment in businessObjects)
			{
				AddConsolFetchHint(shipment);
				shipment.NotesChecker.AddBusinessObjectsWithRelatedNotesFetchHints();
			}

			foreach (ForwardingModuleShipment shipment in businessObjects)
			{
				shipment.NotesChecker.AddNotesIncludingRelatedFetchHints();
			}
		}

		void AddCommodityAttributeFetchHint(ForwardingModuleShipment shipment)
		{
			foreach (PackLine packLine in shipment.OuterPackLines)
			{
				shipment.Factory.AddFetchHint(typeof(RefCommodityCode), RefCommodityCodeSchema.RH_Code, packLine.JL_RH_NKCommodityCode);
				shipment.Factory.AddFetchHint(typeof(UNDGDataItem), UNDGDataItemSchema.DI_ParentID, packLine.PK);
			}
		}

		void AddCommodityCodeFetchHint(ForwardingModuleShipment shipment)
		{
			foreach (PackLine packline in shipment.OuterPackLines)
			{
				shipment.Factory.AddFetchHint(typeof(RefCommodityCode), RefCommodityCodeSchema.RH_Code, packline.JL_RH_NKCommodityCode);
			}
		}

		#endregion
	}
}

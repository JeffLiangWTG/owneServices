using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class CancelDocketsActionMethodApplicator<T> : WhsOperationalActionMethodApplicator
		where T : WhsDocket
	{
		protected CancelDocketsActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{ }

		const string OutputTextFormat = "{0} {1} - {2}";

		protected void CancelDockets(IOperationalActionSectionLog log, IEnumerable<T> targets)
		{
			log.SetSectionProgressMax(targets.Count());

			AddFetchHintsIfRequired(targets);

			foreach (T target in targets)
			{
				var docket = (WhsDocket)target;
				var link = GetDocketIdLink(docket);

				if (!docket.IsCancelled)
				{
					var result = docket.CanCancel();
					if (result != ZString.Empty)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, docket.HumanReadableName, link, result);
					}
					else
					{
						docket.CancelReactivateDocket();
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, OutputTextFormat, docket.HumanReadableName, link, Res.GetString("641011CB-3A6E-4529-9543-8E76883BB48C", "is canceled successfully."));
					}
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, docket.HumanReadableName, link, WhsDocket.CantCancelReasonMsg);
				}

				log.BumpSectionProgress();
			}
		}

		void AddFetchHintsIfRequired(IEnumerable<WhsDocket> dockets)
		{
			if (dockets?.Any() ?? false)
			{
				var factory = dockets.First().Factory;

				foreach (var docket in dockets)
				{
					AddDocketFetchHint(docket, factory);
				}

				foreach (var docketLine in dockets.Where(d => d is WhsReceive).SelectMany(o => o.Lines))
				{
					AddReceiveLineFetchHint(docketLine, factory);
				}

				foreach (var docketLine in dockets.Where(d => d is WhsOrder).SelectMany(o => o.Lines))
				{
					AddOrderLineFetchHint(docketLine, factory);
				}

				var warehouses = factory.Load<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.PK, dockets.Select(d => d.WD_WW_Whs)));
				foreach (var warehouse in warehouses)
				{
					AddWarehouseFetchHint(warehouse, factory);
				}
			}
		}

		void AddDocketFetchHint(WhsDocket docket, BusinessObjectFactory factory)
		{
			factory.AddFetchHint(WhsDocketLineSchema.WE_WD, docket.PK);

			var jobHeaderQuery = new ZQuery(JobHeaderSchema.JH_ParentID, docket.PK);
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			factory.AddFetchHint(JobHeaderSchema.Instance, jobHeaderQuery);

			if (docket is WhsOrder order)
			{
				AddOrderFetchHint(order, factory);
			}
		}

		static void AddOrderFetchHint(WhsOrder order, BusinessObjectFactory factory)
		{
			factory.AddFetchHint(WhsLoadOrderSchema.WOV_WD_Docket, order.PK);
		}

		void AddReceiveLineFetchHint(WhsDocketLine receiveLine, BusinessObjectFactory factory)
		{
			factory.AddFetchHint(WhsInventoryViewSchema.WI_WE_InDocketLine, receiveLine.PK);

			var query = new ZQuery();
			query.AddToFilter(WhsPickLineSchema.WZ_OriginalReservedQty, SQLComparisonOperator.GreaterThan, 0m);
			query.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);
			var pickLineQuery = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine.PK);
			pickLineQuery.AddToFilter(query);

			factory.AddFetchHint(WhsPickLineSchema.Instance, pickLineQuery);
			factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, receiveLine.PK);
		}

		void AddOrderLineFetchHint(WhsDocketLine orderLine, BusinessObjectFactory factory)
		{
			factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, orderLine.PK);
		}

		void AddWarehouseFetchHint(WhsWarehouse warehouse, BusinessObjectFactory factory)
		{
			factory.AddFetchHint(GlbBranchSchema.PK, warehouse.WW_GB_RelatedCompanyBranch);
			factory.AddFetchHint(OrgAddressSchema.PK, warehouse.WW_OA_WarehouseAddress);
		}
	}
}

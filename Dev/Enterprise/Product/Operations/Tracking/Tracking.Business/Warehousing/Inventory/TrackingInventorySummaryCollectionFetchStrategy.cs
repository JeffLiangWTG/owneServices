using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	class TrackingInventorySummaryCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public TrackingInventorySummaryCollectionFetchStrategy(TrackingInventorySummaryCollection collection)
			: base(collection) { }

		public void AddFetchHints(IEnumerable<TrackingInventorySummary> summaries)
		{
			if (summaries.Any())
			{
				var factory = summaries.First().Factory;
				var supplierPartPKs = summaries.Select(s => s.WI_OP);
				var warehousePKs = summaries.Select(w => w.WI_WW_Whs);

				factory.AddFetchHint(OrgSupplierPartSchema.Instance, new ZQuery(OrgSupplierPartSchema.PK, supplierPartPKs));
				factory.AddFetchHint(WhsWarehouseSchema.Instance, new ZQuery(WhsWarehouseSchema.PK, warehousePKs));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			var summaries = businessObjects.OfType<TrackingInventorySummary>();

			if (summaries.Any())
			{
				var isFetchForPageView = columns.Length > 1;
				if (isFetchForPageView)
				{
					summaries.LoadInventories();
				}

				foreach (var column in columns)
				{
					string columnName = column.ColumnName;

					if (columnName == TrackingInventorySummary.Schema.WI_ClientQuantity)
					{
						var factory = summaries.First().Factory;
						var supplierPartPKs = summaries.Select(s => s.WI_OP).Distinct();
						factory.AddFetchHint(OrgPartRelationSchema.Instance, new ZQuery(OrgPartRelationSchema.OU_OP, supplierPartPKs));

						var clientPKs = summaries.Select(s => s.WI_OH_Client).Distinct();
						var relatedOrganisationPKs = summaries
							.SelectMany(s => s.SupplierPart?.RelatedOrganisations)
							.OfType<OrgPartRelation>()
							.Select(r => r.OU_OH)
							.Distinct();
						clientPKs = clientPKs.Union(relatedOrganisationPKs);

						var refpacksQuery = new ZQuery(RefPacksSchema.RP_OH_Supplier, null);
						refpacksQuery.AddToFilter(new ZQuery(RefPacksSchema.RP_OH_Supplier, clientPKs), JoinCondition.Or);
						factory.AddFetchHint(RefPacksSchema.Instance, refpacksQuery);
					}
					else if (columnName == TrackingInventorySummary.Schema.HasProductImage)
					{
						var supplierParts = summaries.Select(s => s.SupplierPart);
						var docFactory = supplierParts.FirstOrDefault()?.DocManagerInfo()?.MasterFactory as BusinessObjectFactory;

						if (docFactory != null)
						{
							foreach (var supplierPart in supplierParts)
							{
								supplierPart.DocManagerInfo().MasterFactory = (IDocumentFactory)docFactory;
								docFactory.AddFetchHint(StorageMainSchema.SM_ParentFK, supplierPart.PK);
							}
						}
					}
				}

				if (isFetchForPageView)
				{
					var inventories = summaries
						.SelectMany(s => s.Inventories)
						.OfType<TrackingWhsInventory>();
					var docketLines = inventories.Select(i => i.InDocketLine).Distinct();
					var docketDocFactory = docketLines.FirstOrDefault()?.DocFactory;

					if (docketDocFactory != null)
					{
						foreach (var docketLine in docketLines)
						{
							docketDocFactory.AddFetchHint(StorageMainSchema.SM_ParentFK, docketLine.PK);
							docketLine.DocFactory = docketDocFactory;
						}
					}
				}
			}

			base.FetchForViewCore(businessObjects, columns);
		}
	}
}

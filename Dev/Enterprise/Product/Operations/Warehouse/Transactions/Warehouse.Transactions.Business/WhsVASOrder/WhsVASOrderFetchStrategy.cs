using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsVASOrderFetchStrategy(WhsVASOrder vasOrder)
			: base(vasOrder)
		{
		}

		#region FetchForViewCore

		// Tested in VASOrderFilterControlTest.cs
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			var lineQuery = new ZQuery(WhsVASOrderLineSchema.WVL_WVO_VASOrder, VASOrder.PK);
			var requiresAreaHint = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case "Client+" + OrgHeaderSchema.Constants.OH_Code:
					case "Client+" + OrgHeaderSchema.Constants.OH_FullName:
						Factory.AddFetchHint(OrgHeaderSchema.PK, VASOrder.WVO_OH_Client);
						break;

					case nameof(WhsVASOrder.Status):
						Factory.AddFetchHint(WhsDocketSchema.PK, VASOrder.WVO_WD_TransferIntoServiceArea);
						break;

					case nameof(WhsVASOrder.WarehousePK):
					case "ServiceArea+WA_NameMultilingual":
						requiresAreaHint = true;
						break;

					case nameof(VASOrder.ProductCode):
					case nameof(VASOrder.ProductDescription):
						Factory.AddFetchHint(WhsVASOrderLineSchema.Instance, lineQuery);

						var lineSubQuery = new ZDBOnlySubQuery(typeof(WhsVASOrderLine), WhsVASOrderLineSchema.WVL_OP_Product);
						lineSubQuery.AddToFilter(lineQuery);

						var productQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
						productQuery.AddSubQuery(lineSubQuery, JoinCondition.And);

						Factory.AddFetchHint(OrgSupplierPartSchema.Instance, productQuery);
						break;

					case nameof(VASOrder.ProductQty):
						Factory.AddFetchHint(WhsVASOrderLineSchema.Instance, lineQuery);
						break;

					case nameof(WhsVASOrder.Job) + "+" + nameof(WhsVASOrder.Job.JH_ProfitLossReasonCode):
					case nameof(WhsVASOrder.Job) + "+" + nameof(WhsVASOrder.Job.JH_TotalProfitRevenueMargin):
						AddFetchHintJobHeader();
						break;

					default:
						break;
				}
			}

			if (requiresAreaHint)
			{
				Factory.AddFetchHint(WhsAreaSchema.PK, VASOrder.WVO_WA_ServiceArea);
			}
		}

		void AddFetchHintJobHeader()
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, VASOrder.PK)
				.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)
				.AddToFilter(JobHeaderSchema.JH_IsActive, true);

			Factory.AddFetchHint(JobHeaderSchema.Instance, query);
		}

		#endregion

		#region Implementation

		WhsVASOrder VASOrder
		{
			get { return (WhsVASOrder)BusinessObject; }
		}

		#endregion
	}
}

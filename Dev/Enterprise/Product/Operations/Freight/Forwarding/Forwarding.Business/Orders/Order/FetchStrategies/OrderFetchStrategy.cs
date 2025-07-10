using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrderFetchStrategy(Order order) : base(order)
		{
		}

		Order order
		{
			get { return BusinessObject as Order; }
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(), order.JD_JE);
			Factory.AddFetchHint(GenPivotSchema.Instance, GetGenPivotFetchHintQuery());
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(), order.JD_JE);
			Factory.AddFetchHint(GenPivotSchema.Instance, GetGenPivotFetchHintQuery());
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, order.PK);
		}

		ZQuery GetGenPivotFetchHintQuery()
		{
			var query = new ZQuery(GenPivotSchema.XX_Relation2ID, order.PK);
			query.AddToFilter(GenPivotSchema.XX_RelationType, ZString.Empty);
			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			if (columns.FirstOrDefault(x => IsDeclarationRequired(x.ColumnName)) != null)
			{
				Factory.AddFetchHint(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(), order.JD_JE);
			}

			var orderLineRequired = false;
			var processTasksRequired = false;
			var buyerAddressRequired = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case Order.Schema.JD_Calc_InnerPacks:
					case Order.Schema.JD_Calc_InnerPackType:
					case Order.Schema.JD_Calc_LineCount:
					case Order.Schema.JD_Calc_OuterPacks:
					case Order.Schema.JD_Calc_TotalQuantity:
					case Order.Schema.JD_Calc_TotalQuantityInvoiced:
					case Order.Schema.JD_Calc_TotalQuantityReceived:
					case Order.Schema.JD_Calc_TotalQuantityRemaining:
						orderLineRequired = true;
						break;

					case Order.Schema.JD_Milestone_A_CAV:
					case Order.Schema.JD_Milestone_A_CCC:
					case Order.Schema.JD_Milestone_A_CLR:
					case Order.Schema.JD_Milestone_A_DCA:
					case Order.Schema.JD_Milestone_A_DCF:
					case Order.Schema.JD_Milestone_A_EXW:
					case Order.Schema.JD_Milestone_A_GIW:
					case Order.Schema.JD_Milestone_E_ARV:
					case Order.Schema.JD_Milestone_E_CAV:
					case Order.Schema.JD_Milestone_E_CCC:
					case Order.Schema.JD_Milestone_E_CLR:
					case Order.Schema.JD_Milestone_E_DCA:
					case Order.Schema.JD_Milestone_E_DCF:
					case Order.Schema.JD_Milestone_E_DEP:
					case Order.Schema.JD_Milestone_E_EXW:
					case Order.Schema.JD_Milestone_E_GIW:
						processTasksRequired = true;
						break;

					case nameof(Order.JD_Calc_BuyerCode):
						buyerAddressRequired = true;
						break;
				}
			}

			if (orderLineRequired)
			{
				Factory.AddFetchHint(JobOrderLineSchema.JO_JD, order.PK);
			}

			if (processTasksRequired)
			{
				Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
			}

			if (buyerAddressRequired)
			{
				Factory.AddFetchHint(typeof(OrgAddress), order.JD_OA_BuyerAddress);
			}
		}

		bool IsDeclarationRequired(string columnName)
		{
			return columnName == Order.Schema.AttachedShipment_RS_NKServiceLevel || columnName == Order.Schema.AttachedShipment_RL_NKDestination || columnName == Order.Schema.AttachedShipment_RL_NKOrigin;
		}
	}
}

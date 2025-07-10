using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsClientParameterByWarehouseValidation : AutoWhsClientParameterByWarehouseValidation
	{
		public WhsClientParameterByWarehouseValidation(AutoWhsClientParameterByWarehouse parent)
			: base(parent)
		{
		}

		new WhsClientParameterByWarehouse Parent => (WhsClientParameterByWarehouse)base.Parent;

		protected override void CheckWY_OH_Client()
		{
			base.CheckWY_OH_Client();
			CheckDuplicates(Parent.WY_OH_ClientInfo);
		}

		protected override void CheckWY_WW_Whs()
		{
			base.CheckWY_WW_Whs();
			CheckDuplicates(Parent.WY_WW_WhsInfo);
		}

		protected override void CheckWY_ReceiveCategory()
		{
			base.CheckWY_ReceiveCategory();

			ListValidation.ErrorIfInvalidCode(Parent.WY_ReceiveCategoryInfo, WarehouseDataRegistry.Instance.ReceiveCategories.Value);
			CheckDuplicates(Parent.WY_ReceiveCategoryInfo);
		}

		void CheckDuplicates(ZPropertyInfo zPropertyInfo)
		{
			CheckDuplicatesWithReceiveCategory(zPropertyInfo);
		}

		void CheckDuplicatesWithReceiveCategory(ZPropertyInfo zPropertyInfo)
		{
			var errorMessage = Res.GetString("a6f9b971-1de1-4b6a-a816-aa1794babe39", "Warehouse and Receive Category should be unique.");
			if (Parent.Client != null && Parent.WY_WW_Whs.IsValid)
			{
				var query = new ZQuery(WhsClientParameterByWarehouseSchema.WY_OH_Client, Parent.WY_OH_Client);
				query.AddToFilter(WhsClientParameterByWarehouseSchema.WY_WW_Whs, Parent.WY_WW_Whs);
				query.AddToFilter(WhsClientParameterByWarehouseSchema.WY_ReceiveCategory, Parent.WY_ReceiveCategory);
				query.AddToFilter(WhsClientParameterByWarehouseSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				if (Parent.Factory.LoadTop1<WhsClientParameterByWarehouse>(query) != null)
				{
					zPropertyInfo.AddError(errorMessage);
				}
			}
		}

		protected override void CheckWY_PreventReceivingOvers()
		{
			base.CheckWY_PreventReceivingOvers();
			if (Parent.WY_ReceiveOverageTolerancePercent > 0 && !Parent.WY_PreventReceivingOvers)
			{
				Parent.WY_PreventReceivingOversInfo.AddError(Res.GetString("9afeae95-de0a-4981-b910-7ab9d57bf197", "Prevent Receiving Overs must be checked when Receive Overage Tolerance Percent is greater than zero."));
			}
		}

		protected override void CheckWY_ReceiveOverageTolerancePercent()
		{
			base.CheckWY_ReceiveOverageTolerancePercent();
			CompareValidation.CheckWithinRange(Parent.WY_ReceiveOverageTolerancePercentInfo, 0, 500);
		}
	}
}

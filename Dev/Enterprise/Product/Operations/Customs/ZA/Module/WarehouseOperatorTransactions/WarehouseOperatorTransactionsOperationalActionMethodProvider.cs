using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ZA.Module
{
	class WarehouseOperatorTransactionsOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			return new OperationalActionMethod[]
			{
				new ExWarehouseOperationalActionMethod(),
				new ExportNonBelnOperationalActionMethod(),
				new ExportBelnOperationalActionMethod(),
				new ExbondForUnderReceiptsOperationalActionMethod(),
				new ClearExpiredStockOperationalActionMethod(),
			};
		}
	}
}

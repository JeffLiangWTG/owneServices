using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Warehouse.Transactions.Business
{
	partial class WhsPermitWithdrawRequest : WhsPermitWithdrawalRequestDetail, IPermitWithdrawRequest
	{
		public WhsPermitWithdrawRequest(WhsOrderLine orderLine)
			: base(Argument.NotNull(orderLine, nameof(orderLine)), orderLine.WE_TransactionQuantity)
		{
			var order = Argument.NotNull(orderLine.Order, nameof(orderLine.Order));
			var warehouse = Argument.NotNull(order.Warehouse, nameof(order.Warehouse));
			var customsData = Argument.NotNull(orderLine.CustomsData, nameof(orderLine.CustomsData));

			Owner = Argument.NotNull(order.Client, nameof(order.Client)).MainAddress;
			DetailedTrackingEnabled = warehouse.WW_FTZIsDetailedTrackingEnabled;

			ProductCode = orderLine.ProductCode;
			UQ = orderLine.ProductUQ;

			Manufacturer = customsData.ManufacturerAddress;
			Tariff = customsData.WB_Tariff;
			CountryOfOrigin = customsData.WB_RN_NKCountryOfOrigin;
		}

		#region IPermitMatchingCriteria Members

		public IOrgAddress Owner { get; }

		public IOrgAddress Manufacturer { get; }

		public bool DetailedTrackingEnabled { get; }

		public ZString Tariff { get; }

		public ZString CountryOfOrigin { get; }

		public ZString ProductCode { get; }

		public ZString UQ { get; }

		#endregion
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonPickupDeliveryConfirmFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CommonPickupDeliveryConfirmFetchStrategy(CommonPickupDeliveryConfirm confirm)
			: base(confirm)
		{
			this.confirm = confirm;
		}

		readonly CommonPickupDeliveryConfirm confirm;

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(OrgAddressSchema.Constants.TableName, confirm.EU_OA_TransportProvider);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			var transportColumns = new[]
			{
				CommonPickupDeliveryConfirm.Schema.TotalBookedPackages,
				CommonPickupDeliveryConfirm.Schema.TotalBookedVolume,
				CommonPickupDeliveryConfirm.Schema.TotalBookedWeight,
				CommonPickupDeliveryConfirm.Schema.TotalPackagesUnit,
				CommonPickupDeliveryConfirm.Schema.TotalVolumeUnit,
				CommonPickupDeliveryConfirm.Schema.TotalWeightUnit,
			};

			if (columns.Any(c => transportColumns.Contains(c.ColumnName)))
			{
				Factory.AddFetchHint(JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, confirm.PK);
			}

			base.FetchForViewCore(columns);
		}
	}
}

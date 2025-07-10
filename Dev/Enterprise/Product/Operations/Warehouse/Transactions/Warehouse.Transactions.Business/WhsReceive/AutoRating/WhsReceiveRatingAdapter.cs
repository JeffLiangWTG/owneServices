using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveRatingAdapter : WhsDocketRatingAdapter<WhsReceive>
	{
		public WhsReceiveRatingAdapter(WhsReceive receive)
			: base(receive)
		{
		}

		public override AdapterType AdapterType => AdapterType.WarehouseReceipt;

		protected override ZPropertyInfo GetTransportCoOrganisationPropertyInfo() => Parent.GetTransportCoOrganisationPropertyInfo();

		protected override OrgHeader WarehouseFallbackConsignorForFilterOnlyCore
			=> Parent.LoadJobDocAddressQuickly(Parent.SupplierDocAddressRequirement.DefaultDocAddressType)?.GetOrganisation();

		protected override void AddMeasuresCore(RateableMeasureSet result, ClosureData data)
		{
			CreatePalletIDMeasure(result, data);
		}

		void CreatePalletIDMeasure(RateableMeasureSet rateableMeasures, ClosureData data)
		{
			Action<RateableMeasureSet> lazyPopulatePalletId = measures =>
			{
				var uniquePalletIds = data.Lines.
									Where(l => l.Quantity > 0 && !l.PalletID.IsEmpty).
									Select(l => l.PalletID.ToUpper()).
									Distinct();

				var docketReference = GetDocketReference(Parent);

				foreach (var palletId in uniquePalletIds)
				{
					measures.AddWarehousePalletId(Parent.WD_WW_Whs, palletId: palletId, docketReference: docketReference);
				}
			};

			rateableMeasures.CreateWarehousePalletIdList(lazyPopulatePalletId, includeDocketReference: true);
		}

		protected override IEnumerable<string> UsedChargeCodeGroups
		{
			get { return new[] { ChargeCodeGroupList.Codes.WHSInwards }; }
		}
	}
}

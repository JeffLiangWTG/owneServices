using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.eTail.Business
{
	class CheckGoodsDescriptionAction : CheckNormalPropertyAction
	{
		public CheckGoodsDescriptionAction(HVLVPreScreeningField field, HVLVConsignment consignment) : base(field, consignment)
		{
		}
		protected override IEnumerable<ZString> PropertyValues
		{
			get
			{
				yield return consignment.HVC_GoodsDescription;

				var items = consignment.Items.OfType<HVLVItem>();
				if (items != null && items.Any())
				{
					foreach (var item in items)
					{
						yield return item.HVI_GoodsDescription;
					}
				}

				var itemLines = items.SelectMany(item => item.Lines).OfType<HVLVItemLine>();
				if (itemLines != null && itemLines.Any())
				{
					foreach (var itemLine in itemLines)
					{
						yield return itemLine.HVS_GoodsDescription;
					}
				}
			}
		}
	}
}

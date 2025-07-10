using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public class NctsToCusEntryHeaderAttacher : ZRecordAttacher
	{
		public NctsToCusEntryHeaderAttacher(CusEntryHeader entry, IBusinessObjectCollection findBoxList)
			: base(null, findBoxList, ModuleIDs.Customs.EU.NctsMovementModule)
		{
			this.entry = Argument.NotNull(entry, nameof(CusEntryHeader));
		}
		readonly CusEntryHeader entry;

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			if (bizO is ICommonGoodsItemsIntegratorProvider provider)
			{
				var commonGoodsItems = provider.CommonGoodsItemsIntegrator.GetCommonGoodsItemsForIntegration();
				entry.CommonGoodsItemsIntegrator.CopyCommonGoodsItems(commonGoodsItems, 0);
			}

			return true;
		}
	}
}

using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveLookups : WhsDocketLookups
	{
		public WhsReceiveLookups(WhsReceive parent)
			: base(parent)
		{
		}

		public override OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get => ((WhsReceive)Parent).GetCarrierServiceLevels() ?? base.CarrierServiceLevels;
		}

		protected override CodeDescriptionPairList SubTypesCore => new CodeLists.ReceiveType();

		public CodeDescriptionPairList ReceiveCategories => Factory.GetCachedValue("WhsReceiveLookups|ReceiveCategories", () => WarehouseDataRegistry.Instance.ReceiveCategories.Value.GetCodeDescriptionPairList());
	}
}

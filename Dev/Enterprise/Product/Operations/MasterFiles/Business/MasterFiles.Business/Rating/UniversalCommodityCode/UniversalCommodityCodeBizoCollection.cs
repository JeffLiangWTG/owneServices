using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Rating
{
	[ModuleID(ModuleId.UniversalCommodityCode)]
	public class UniversalCommodityCodeBizoCollection : NonPersistentBusinessObjectCollection<UniversalCommodityCodeBizo>
	{
		public UniversalCommodityCodeBizoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UniversalCommodityCodeBizo(Factory);
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}

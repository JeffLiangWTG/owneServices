using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.StmMenuItem)]
	public class StmMenuItemCollection : BusinessObjectCollection<StmMenuItem>
	{
		public StmMenuItemCollection(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		public StmMenuItemCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new StmMenuItemFindBoxListProvider(this); }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new DocumentZQuery();
		}
	}
}

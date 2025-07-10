using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// StmMenuItem Collection with additional filter placed on
	/// </summary>
	[ModuleID(ModuleId.StmMenuItem)]
	public class StmMenuItemFilteredCollection : StmMenuItemCollection
	{
		public StmMenuItemFilteredCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public StmMenuItemFilteredCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return CreateAdditionalFilter();
		}
	}
}

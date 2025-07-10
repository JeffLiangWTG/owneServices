using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// StmTemplate Collection with additional filter placed on
	/// </summary>
	[ModuleID(ModuleId.DocumentTemplate)]
	public class StmTemplateFilteredCollection : StmTemplateCollection
	{
		public StmTemplateFilteredCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public StmTemplateFilteredCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return CreateAdditionalFilter();
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccTaxRate)]
	public class AccTaxRateCollectionForRegistry : AccTaxRateCollection
	{
		public AccTaxRateCollectionForRegistry(BusinessObjectFactory factory)
			: base(factory, ZString.Empty)
		{
		}

		public AccTaxRateCollectionForRegistry(BusinessObjectFactory factory, ZQuery filter, GlbCompany company)
			: base(factory, filter, company)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery();
		}

		internal ZQuery RelationshipFilterInternal => RelationshipFilter;
	}
}

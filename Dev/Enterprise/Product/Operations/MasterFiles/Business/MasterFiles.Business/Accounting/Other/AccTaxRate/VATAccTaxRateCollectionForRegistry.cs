using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AccTaxRate)]
	public class VATAccTaxRateCollectionForRegistry : VATAccTaxRateCollection
	{
		public VATAccTaxRateCollectionForRegistry(BusinessObjectFactory factory)
			: base(factory, ZString.Empty)
		{
		}

		public VATAccTaxRateCollectionForRegistry(BusinessObjectFactory factory, ZQuery filter, GlbCompany company)
			: base(factory, filter.AddToFilter(JoinCondition.And, AccTaxRateSchema.AT_TaxSystemCode, ""), company)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery();
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationCollection : TypeSafeJobDeclarationCollection
	{
		public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, companyPkToFilterOn)
		{
		}

		protected override bool AllowNewCore => false;

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new FetchStrategies.JobDeclarationCollectionFetchStrategy(this);
	}
}

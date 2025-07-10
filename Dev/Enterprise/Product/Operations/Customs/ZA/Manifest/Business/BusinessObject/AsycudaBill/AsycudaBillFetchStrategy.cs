using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaBillFetchStrategy : ASYCUDA.Business.AsycudaBillFetchStrategy
	{
		public AsycudaBillFetchStrategy(AsycudaBill bill)
			: base(bill)
		{
		}

		protected new AsycudaBill BusinessObject => (AsycudaBill)base.BusinessObject;

		protected override IEnumerable<BusinessObject> LoadChildrenForValidate() => base.LoadChildrenForValidate().Union(BusinessObject.CaseNumbers);
	}
}

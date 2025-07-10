using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTaxOrFeeType))]
	public class RefCusTaxOrFeeTypeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bo = (RefCusTaxOrFeeType)base.GetNewBusinessObjectForDeleteTest(factory);
			return bo;
		}
	}
}

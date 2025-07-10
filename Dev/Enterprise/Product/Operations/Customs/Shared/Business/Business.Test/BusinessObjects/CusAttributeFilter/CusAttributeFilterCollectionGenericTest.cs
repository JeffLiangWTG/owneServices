using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusAttributeFilterCollection<CusAttributeFilter>))]
	sealed class CusAttributeFilterCollectionGenericTest : ActiveBusinessObjectCollectionTestCase<CusAttributeFilterCollection<CusAttributeFilter>>
	{
		protected override CusAttributeFilterCollection<CusAttributeFilter> GetCollectionToTest()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			return new CusAttributeFilterCollection<CusAttributeFilter>(pivot, nameof(CusAttributeFilter.AttributeFilterName.AT1));
		}
	}
}

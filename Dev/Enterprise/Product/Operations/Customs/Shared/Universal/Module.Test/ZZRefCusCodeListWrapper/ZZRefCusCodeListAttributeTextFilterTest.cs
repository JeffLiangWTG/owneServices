using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusCodeListAttributeTextFilter))]
	public class ZZRefCusCodeListAttributeTextFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ZZRefCusCodeListAttributeTextFilter("Type", "Type");
		}
	}
}

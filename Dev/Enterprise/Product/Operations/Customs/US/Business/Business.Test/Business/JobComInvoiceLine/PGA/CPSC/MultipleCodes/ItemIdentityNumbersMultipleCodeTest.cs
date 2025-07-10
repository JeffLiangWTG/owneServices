using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ItemIdentityNumbersMultipleCode))]
	class ItemIdentityNumbersMultipleCodeTest : NonPersistentBusinessObjectTestCase
	{
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new ItemIdentityNumbersMultipleCode();
		}
	}
}

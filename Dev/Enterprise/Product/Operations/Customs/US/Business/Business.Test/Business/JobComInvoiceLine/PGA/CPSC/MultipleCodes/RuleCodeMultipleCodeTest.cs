using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RuleCodeMultipleCode))]
	class RuleCodeMultipleCodeTest : NonPersistentBusinessObjectTestCase
	{
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new RuleCodeMultipleCode();
		}
	}
}

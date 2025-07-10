using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobDeclaration))]
	public class BaseJobDeclarationICartageParentTestCase : ICartageParentTestCase
	{
		protected override ICartageParent GetNewParent()
		{
			return Factory.New<BaseJobDeclaration>();
		}
	}
}

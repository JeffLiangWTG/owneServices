using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[TestedType(typeof(InvalidCodeRule.CodeDescriptionCollectionWrapper))]
	sealed class CodeDescriptionCollectionWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invalidCodeRule = new InvalidCodeRule();
			return new InvalidCodeRule.CodeDescriptionCollectionWrapper(invalidCodeRule, Factory);
		}
	}
}

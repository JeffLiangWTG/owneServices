using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ActiveCusEntryHeaderCollection))]
	sealed class ActiveCusEntryHeaderCollectionTest : Customs.Business.Testing.ActiveCusEntryHeaderCollectionTest<ActiveCusEntryHeaderCollection>
	{
		protected override ActiveCusEntryHeaderCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new ActiveCusEntryHeaderCollection(declaration);
		}
	}
}

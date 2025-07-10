using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationFormAdaptationsProvider))]
	sealed class ConsolidatedDeclarationFormAdaptationsProviderTest : TestCaseWithFactory
	{
		public void TestEnableDocumentMenuItem()
		{
			var provider = new ConsolidatedDeclarationFormAdaptationsProvider();
			Assert("NZ Consolidated Declaration Form should display Documents menu", provider.EnableDocumentMenuItem);
		}
	}
}

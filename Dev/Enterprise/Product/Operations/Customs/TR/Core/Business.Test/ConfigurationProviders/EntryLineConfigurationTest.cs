using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(EntryLineConfiguration))]
	class EntryLineConfigurationTest : EU.Business.Testing.EntryLineConfigurationAbstractTest<EntryLineConfiguration>
	{
		public new void TestSupportingDocumentsSupport()
		{
			AssertEquals(true, configuration.SupportingDocumentsSupport(CreateDeclaration()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			configuration = new EntryLineConfiguration();
		}
		new EntryLineConfiguration configuration;
	}
}

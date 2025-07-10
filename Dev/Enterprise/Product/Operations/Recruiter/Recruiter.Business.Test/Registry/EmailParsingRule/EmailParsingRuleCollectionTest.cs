using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(EmailParsingRuleCollection))]
	public class EmailParsingRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EmailParsingRuleCollection>
	{
		public void TestAllowRemoveAllowNew()
		{
			var testCollection = GetCollectionToTest();
			Assert(!testCollection.AllowRemove);
			Assert(!testCollection.AllowNew);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override EmailParsingRuleCollection GetCollectionToTest()
		{
			return new EmailParsingRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EmailParsingRule();
		}

		#endregion Implementation
	}
}

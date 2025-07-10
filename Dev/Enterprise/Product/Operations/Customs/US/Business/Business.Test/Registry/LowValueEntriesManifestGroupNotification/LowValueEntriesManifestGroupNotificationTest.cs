using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(LowValueEntriesManifestGroupNotification))]
	sealed class LowValueEntriesManifestGroupNotificationTest : RegistryBusinessObjectTemplateTestCase<LowValueEntriesManifestGroupNotification>
	{
		public new void TestClone()
		{
			var businessObjectToClone = GetBusinessObjectToClone();
			var clone = businessObjectToClone.Clone(businessObjectToClone.CurrentFallbackLevel, businessObjectToClone.Factory);

			Assert("Clone should be a different instance.", businessObjectToClone != clone);
			AssertEquals("CurrentFallbackLevel", businessObjectToClone.CurrentFallbackLevel, clone.CurrentFallbackLevel);
			AssertEquals("CurrentFactory", businessObjectToClone.Factory, clone.Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override LowValueEntriesManifestGroupNotification GetBusinessObjectToClone()
		{
			return new LowValueEntriesManifestGroupNotification();
		}

		protected override LowValueEntriesManifestGroupNotification GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}

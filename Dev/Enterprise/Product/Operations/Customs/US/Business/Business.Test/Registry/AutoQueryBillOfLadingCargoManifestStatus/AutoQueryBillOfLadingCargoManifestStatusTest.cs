using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutoQueryBillOfLadingCargoManifestStatus))]
	sealed class AutoQueryBillOfLadingCargoManifestStatusTest : RegistryBusinessObjectTemplateTestCase<AutoQueryBillOfLadingCargoManifestStatus>
	{
		public void TestSendOnFirstSave()
		{
			var queryObject = new AutoQueryBillOfLadingCargoManifestStatus();
			queryObject.SendOnFirstSave = true;
			queryObject.UpdateEntryWithResults = true;
			queryObject.SendOnFirstSave = false;
			AssertEquals("SendOnFirstSave being set to false should set UpdateEntryWithResults to false", false, queryObject.UpdateEntryWithResults);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AutoQueryBillOfLadingCargoManifestStatus GetBusinessObjectToClone()
		{
			var result = new AutoQueryBillOfLadingCargoManifestStatus();
			result.FillWithValidTestData();
			return result;
		}

		protected override AutoQueryBillOfLadingCargoManifestStatus GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}

using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefMessagingBussCarrierInfo))]
	class RefMessagingBussCarrierInfoTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var carrierInfo = Factory.NewWithValidTestData<RefMessagingBussCarrierInfo>();

			AssertHumanReadableName("INDHL", "DHL Courier", "INDHL - DHL Courier", "Carrier - INDHL - DHL Courier");
			AssertHumanReadableName("AUABC", "ABC Company", "AUABC - ABC Company", "Carrier - AUABC - ABC Company");

			void AssertHumanReadableName(string carrierCode, string carrierName, string expectedShortName, string expectedName)
			{
				carrierInfo.ZMC_CarrierCode = carrierCode;
				carrierInfo.ZMC_CarrierName = carrierName;
				AssertEquals(expectedShortName, carrierInfo.HumanReadableShortcutName);
				AssertEquals(expectedName, carrierInfo.HumanReadableName);
			}
		}
	}
}

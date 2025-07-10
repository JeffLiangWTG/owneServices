using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefAccessorial))]
	class RefAccessorialTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var accessorialInfo = Factory.NewWithValidTestData<RefAccessorial>();

			AssertHumanReadableName("ABC", "ABC Accessorial", "ABC - ABC Accessorial", "Accessorial - ABC - ABC Accessorial");
			AssertHumanReadableName("XYZ", "XYZ Accessorial", "XYZ - XYZ Accessorial", "Accessorial - XYZ - XYZ Accessorial");

			void AssertHumanReadableName(string accessorialCode, string accessorialDescription, string expectedShortName, string expectedName)
			{
				accessorialInfo.ASI_Code = accessorialCode;
				accessorialInfo.ASI_Description = accessorialDescription;
				AssertEquals(expectedShortName, accessorialInfo.HumanReadableShortcutName);
				AssertEquals(expectedName, accessorialInfo.HumanReadableName);
			}
		}
	}
}

using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	public class DisclaimApplicablePGAsSectionLogTest : TestCase
	{
		public void TestIOperationalActionSectionLogNotify()
		{
			var sectionLog = new DisclaimApplicablePGAsSectionLog();
			((IOperationalActionSectionLog)sectionLog).Notify(OperationalActionLogErrorLevel.Warning, "Test");
			((IOperationalActionSectionLog)sectionLog).Notify(OperationalActionLogErrorLevel.Warning, "Warning");
			AssertEquals("Test\r\nWarning", sectionLog.Logs);
		}
	}
}

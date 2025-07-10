using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobServiceProcessHandlingInfoTest : TestCaseWithFactory
	{
		#region TestPopulateCascadingTargets

		public void TestPopulateCascadingTargets()
		{
			var handlingInfo = GetNewProcessHandlingInfo();

			Factory.Save();

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = AutoEvents.ServiceRequestedCode;
			}

			AssertEquals(Enumerable.Empty<CascadingLink>(), handlingInfo.GetCascadingTargets(eventLog));
		}

		#endregion

		#region Implementation

		JobServiceProcessHandlingInfo GetNewProcessHandlingInfo()
		{
			return new JobServiceProcessHandlingInfo(Factory.NewWithValidTestData<JobService>());
		}

		#endregion
	}
}

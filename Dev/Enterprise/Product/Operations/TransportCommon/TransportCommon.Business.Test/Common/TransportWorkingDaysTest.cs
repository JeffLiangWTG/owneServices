using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportCommon.Business.Testing
{
	public class TransportWorkingDaysTest : TestCaseWithFactory
	{
		#region TestGetWorkingDate

		public void TestGetWorkingDate()
		{
			var wd = TransportWorkingDays.GetWorkingDate(Factory, ZDateTime.Now, GlbDepartment.CurrentDepartment.PK);

			Assert("Must be a valid date", !wd.IsEmpty);
		}

		#endregion
	}
}

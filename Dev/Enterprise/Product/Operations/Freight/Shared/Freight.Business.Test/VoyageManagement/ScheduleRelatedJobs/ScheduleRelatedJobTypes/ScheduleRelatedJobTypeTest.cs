using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class ScheduleRelatedJobTypeTest : TestCaseWithFactory
	{
		public void TestSailingRelatedJobType()
		{
			var sailingRelatedJobType = GetScheduleRelatedJobType("A", (NoResString)"B");
			AssertEquals("ControllerID", ExpectedControllerID, sailingRelatedJobType.ControllerID);
			AssertEquals("BizOType", ExpectedBizOType, sailingRelatedJobType.BizOType.FullName);
			AssertEquals("Code", "A", sailingRelatedJobType.Code);
			AssertEquals("Description", "B", sailingRelatedJobType.Description);
		}

		protected abstract ControllerID ExpectedControllerID { get; }

		protected abstract string ExpectedBizOType { get; }

		protected abstract ScheduleRelatedJobType GetScheduleRelatedJobType(string code, MultilingualString description);
	}
}

using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class RefTimeZoneTest : EnterpriseBusinessObjectTestCase
	{
		#region Logging

		public void TestLogging()
		{
			RefTimeZoneForTest timeZone = (RefTimeZoneForTest)GetNewBusinessObject();
			Assert(timeZone.IsAutoLogged);
		}

		#endregion

		#region HumanReadableName

		public void TestHumanReadableName()
		{
			RefTimeZoneForTest timeZone = (RefTimeZoneForTest)GetNewBusinessObject();
			AssertEquals("Human Readable name is incorrect or doesn't exist", "Time Zone Details", timeZone.HumanReadableName);
		}

		#endregion

		class RefTimeZoneForTest : RefTimeZone
		{
			public RefTimeZoneForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			internal new bool IsAutoLogged => base.IsAutoLogged;
		}
	}
}

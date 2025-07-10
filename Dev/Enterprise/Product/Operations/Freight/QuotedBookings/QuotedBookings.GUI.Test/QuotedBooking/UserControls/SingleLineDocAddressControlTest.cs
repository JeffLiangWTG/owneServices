using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	class SingleLineDocAddressControlTest : TestCaseWithDummy
	{
		#region Binding
		public void TestDataSourceType()
		{
			using (SingleLineDocAddressControl control = new SingleLineDocAddressControl())
			{
				AssertEquals(typeof(JobDocAddress), control.DataSourceType);
			}
		}
		#endregion
	}
}

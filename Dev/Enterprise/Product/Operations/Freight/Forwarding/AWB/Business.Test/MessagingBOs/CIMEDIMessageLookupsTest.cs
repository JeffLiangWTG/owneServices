using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class CIMEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertNotNull("message.Lookups.MessageTypeList", message.Lookups.MessageTypeList);
		}

		#region Implementation

		CIMEDIMessage message;

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<CIMEDIMessage>();
		}

		#endregion
	}
}

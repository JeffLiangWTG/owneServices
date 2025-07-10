using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(SendACASShipmentActionMethod))]
	public class SendACASShipmentMethodTest : OperationalActionMethodTest<SendACASShipmentActionMethod>
	{
		public void TestOverrides()
		{
			AssertEquals("Send ACAS Shipment Report (US)", Method.Name);
			AssertEquals("for US air cargo imports reporting", Method.Description);
			AssertEquals(typeof(SendACASShipmentMethodApplicator), Method.NewApplicator(Factory, new DocDataObjectSendingMessageSettings()).GetType());
			Assert(Method.HasSettings);
			AssertEquals(typeof(DocDataObjectSendingMessageSettings), Method.NewSetting(Factory).GetType());
			using (var control = Method.NewSettingsControl())
			{
				AssertEquals(typeof(DocDataObjectSendingMessageSettingsControl), control.GetType());
			}
		}

		#region Implementation

		protected override SendACASShipmentActionMethod NewMethod()
		{
			return new SendACASShipmentActionMethod();
		}

		#endregion
	}
}

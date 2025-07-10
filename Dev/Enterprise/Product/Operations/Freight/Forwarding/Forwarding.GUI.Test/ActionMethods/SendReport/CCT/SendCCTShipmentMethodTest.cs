using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(SendCCTShipmentActionMethod))]
	public class SendCCTShipmentMethodTest : OperationalActionMethodTest<SendCCTShipmentActionMethod>
	{
		public void TestOverrides()
		{
			AssertEquals("Send CCT Shipment Report (BR)", Method.Name);
			AssertEquals("for Brazil air cargo imports reporting", Method.Description);
			AssertEquals(typeof(SendCCTShipmentMethodApplicator), Method.NewApplicator(Factory, new DocDataObjectSendingMessageSettings()).GetType());
			Assert(Method.HasSettings);
			AssertEquals(typeof(DocDataObjectSendingMessageSettings), Method.NewSetting(Factory).GetType());
			using (var control = Method.NewSettingsControl())
			{
				AssertEquals(typeof(DocDataObjectSendingMessageSettingsControl), control.GetType());
			}
		}

		#region Implementation

		protected override SendCCTShipmentActionMethod NewMethod()
		{
			return new SendCCTShipmentActionMethod();
		}

		#endregion
	}
}

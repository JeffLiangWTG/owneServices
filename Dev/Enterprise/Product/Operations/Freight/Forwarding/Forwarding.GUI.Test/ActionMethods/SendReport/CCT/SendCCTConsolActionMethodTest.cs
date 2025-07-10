using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(SendCCTConsolActionMethod))]
	public class SendCCTConsolActionMethodTest : OperationalActionMethodTest<SendCCTConsolActionMethod>
	{
		[RequiresSTA]
		public void TestOverrides()
		{
			AssertEquals("Send CCT House Manifest (BR)", Method.Name);
			AssertEquals("for Brazil air cargo imports", Method.Description);
			AssertEquals(typeof(SendCCTConsolMethodApplicator), Method.NewApplicator(Factory, new DocDataObjectSendingMessageSettings()).GetType());
			Assert(Method.HasSettings);
			AssertEquals(typeof(DocDataObjectSendingMessageSettings), Method.NewSetting(Factory).GetType());
			using (var control = Method.NewSettingsControl())
			{
				AssertEquals(typeof(DocDataObjectSendingMessageSettingsControl), control.GetType());
			}
		}

		#region Implementation

		protected override SendCCTConsolActionMethod NewMethod()
		{
			return new SendCCTConsolActionMethod();
		}

		#endregion
	}
}

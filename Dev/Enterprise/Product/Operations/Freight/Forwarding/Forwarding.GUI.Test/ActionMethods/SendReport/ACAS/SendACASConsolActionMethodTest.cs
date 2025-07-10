using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(SendACASConsolActionMethod))]
	public class SendACASConsolActionMethodTest : OperationalActionMethodTest<SendACASConsolActionMethod>
	{
		public void TestOverrides()
		{
			AssertEquals("Send ACAS House Checklist (US)", Method.Name);
			AssertEquals("for US air cargo imports", Method.Description);
			AssertEquals(typeof(SendACASConsolMethodApplicator), Method.NewApplicator(Factory, new DocDataObjectSendingMessageSettings()).GetType());
			Assert(Method.HasSettings);
			AssertEquals(typeof(DocDataObjectSendingMessageSettings), Method.NewSetting(Factory).GetType());
			using (var control = Method.NewSettingsControl())
			{
				AssertEquals(typeof(DocDataObjectSendingMessageSettingsControl), control.GetType());
			}
		}

		#region Implementation

		protected override SendACASConsolActionMethod NewMethod()
		{
			return new SendACASConsolActionMethod();
		}

		#endregion
	}
}

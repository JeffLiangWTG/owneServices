using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(StatementOperationalActionMethod))]
	sealed class StatementOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<StatementOperationalActionMethod>
	{
		public void TestProperties()
		{
			var method = new StatementOperationalActionMethod();
			AssertEquals("Send ACH Payment Authorization operational action", method.Name);
			AssertEquals("Send ACH Payment Authorization (US)", method.Description);
			AssertEquals(true, method.HasControl);
			AssertEquals(false, method.HasSettings);
			using (var control = method.NewGuiControl())
			{
				AssertEquals(typeof(USStatementOperationActionControl), control.GetType());
			}
		}

		protected override StatementOperationalActionMethod NewMethod() => new StatementOperationalActionMethod();
	}
}

using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(PRAActionMethodProvider))]
	public class PRAActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewMethods()
		{
			OperationalActionMethod[] methods = Provider.NewMethods(new ForwardingConsolActionSupporter());
			AssertEquals(3, methods.Length);
			AssertEquals(typeof(PRASendActionMethod), methods[0].GetType());
			AssertEquals(typeof(PRAReSendActionMethod), methods[1].GetType());
			AssertEquals(typeof(PRACancelActionMethod), methods[2].GetType());
		}

		#region Implementation

		protected override ActionMethodProviderID ID
		{
			get { return ActionMethodProviderIDs.PRAMessage; }
		}

		#endregion
	}
}

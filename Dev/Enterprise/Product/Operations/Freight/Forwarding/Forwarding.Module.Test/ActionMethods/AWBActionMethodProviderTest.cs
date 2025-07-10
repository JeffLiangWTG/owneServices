using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(AWBActionMethodProvider))]
	public class AWBActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewMethods()
		{
			OperationalActionMethod[] methods = Provider.NewMethods(new ForwardingConsolActionSupporter());
			AssertEquals(2, methods.Length);
			AssertEquals(typeof(PrintFinalMasterActionMethod), methods[0].GetType());
			AssertEquals(typeof(PrintMAWBBarcodeLabelsActionMethod), methods[1].GetType());
		}

		#region Implementation

		protected override ActionMethodProviderID ID
		{
			get { return ActionMethodProviderIDs.AWB; }
		}

		#endregion
	}
}

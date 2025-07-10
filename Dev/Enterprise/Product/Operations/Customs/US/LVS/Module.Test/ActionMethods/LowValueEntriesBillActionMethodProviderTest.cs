using Enterprise.Customs.US.LVS.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	[TestedType(typeof(LowValueEntriesBillActionMethodProvider))]
	public class LowValueEntriesBillActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewMethods()
		{
			OperationalActionMethod[] methods = Provider.NewMethods(new CusUSLVConsignmentOperationalActionSupporter());
			AssertEquals(3, methods.Length);
			AssertEquals(typeof(ADDCVDAppliesActionMethod), methods[0].GetType());
			AssertEquals(typeof(ADDCVDDoesNotApplyActionMethod), methods[1].GetType());
			AssertEquals(typeof(DisclaimApplicablePGAsActionMethod), methods[2].GetType());
		}

		#region Implementation

		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.USLowValueBill;

		#endregion
	}
}

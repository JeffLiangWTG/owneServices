using Enterprise.Freight.Agency.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(UpdateReturnByActionMethod))]
	internal class UpdateReturnByActionMethodTest : OperationalActionMethodTest<UpdateReturnByActionMethod>
	{
		public void TestApplicatorType()
		{
			OperationalActionMethodApplicator applicator = Method.NewApplicator(Factory, null);
			AssertType(typeof(UpdateReturnByApplicator), applicator);
		}

		public void TestGUI()
		{
			AssertEquals(false, Method.HasControl);
		}

		public void TestSettings()
		{
			AssertEquals(false, Method.HasSettings);
		}

		#region Implementation
		protected override UpdateReturnByActionMethod NewMethod()
		{
			return new UpdateReturnByActionMethod();
		}
		#endregion
	}
}

using Enterprise.Freight.Agency.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(UpdateDetentionDaysActionMethod))]
	internal class UpdateDetentionDaysActionMethodTest : OperationalActionMethodTest<UpdateDetentionDaysActionMethod>
	{
		public void TestApplicatorType()
		{
			OperationalActionMethodApplicator applicator = Method.NewApplicator(Factory, null);
			AssertType(typeof(UpdateDetentionDaysApplicator), applicator);
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
		protected override UpdateDetentionDaysActionMethod NewMethod()
		{
			return new UpdateDetentionDaysActionMethod();
		}
		#endregion
	}
}

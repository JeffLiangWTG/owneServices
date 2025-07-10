using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ReleaseOrderSendActionMethod))]
	internal class ReleaseOrderSendActionMethodTest : ReleaseOrderActionMethodTest<ReleaseOrderSendActionMethod>
	{
		public void TestNewApplicatorIsOfTheCorrectType()
		{
			var applicator = Method.NewApplicator(Factory, new ReleaseImportOrderSettings());
			AssertEquals(typeof(SendNZReleaseOrderActionMethodApplicator), applicator.GetType());
		}

		#region Implementation
		protected override ReleaseOrderSendActionMethod NewMethod()
		{
			return new ReleaseOrderSendActionMethod();
		}
		#endregion
	}
}

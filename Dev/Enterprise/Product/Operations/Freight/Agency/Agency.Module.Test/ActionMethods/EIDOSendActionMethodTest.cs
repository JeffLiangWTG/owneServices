using Enterprise.Freight.Agency.Business;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(EIDOSendActionMethod))]
	internal class EIDOSendActionMethodTest : EIDOBaseActionMethodTest<EIDOSendActionMethod>
	{
		public void TestNewApplicatorIsOfTheCorrectType()
		{
			OperationalActionMethodApplicator applicator = Method.NewApplicator(Factory, new ReleaseImportOrderSettings());
			AssertEquals(typeof(EIDOSendOriginalApplicator), applicator == null ? null : applicator.GetType());
		}

		#region Implementation
		protected override EIDOSendActionMethod NewMethod()
		{
			return new EIDOSendActionMethod();
		}
		#endregion
	}
}

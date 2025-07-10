using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class SecuredFreightVerificationCheckerTest : TestCaseWithFactory
	{
		public void TestRegister()
		{
			AssertNull(Factory.GetValue<ISecuredFreightVerificationChecker>());

			SecuredFreightVerificationChecker.Register(Factory);
			AssertNotNull(Factory.GetValue<ISecuredFreightVerificationChecker>());
		}
	}
}

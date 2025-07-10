namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class FreightVerifiedStub : ISecuredFreightVerificationChecker
	{
		public bool StubCalled { get; set; }

		public bool MAWBMessageDisplayed { get; set; }

		bool ISecuredFreightVerificationChecker.FreightIsVerifiedToBeSecure(bool isMAWBOverridden)
		{
			if (isMAWBOverridden)
			{ MAWBMessageDisplayed = true; }
			StubCalled = true;
			return true;
		}
	}
}

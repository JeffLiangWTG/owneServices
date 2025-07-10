using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(PartyProvider))]
sealed class PartyProviderBaseOnlyTest : PartyProviderAbstractTest<PartyProvider>
{
	public void TestOmitNameAndAddressWhenIDIsFoundTrue() => CombineAssertions(() =>
	{
		provider = new PartyProviderForTest(nctsHeader.Principal);
		AssertNotNullOrEmpty("ID should not be empty", provider.Id);
		AssertNullOrEmpty("name should be empty when ID is found", provider.Name);
		AssertNull("Address should be empty when ID is found", provider.Address);

		var jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.E2_CompanyName = "Test";
		provider = new PartyProviderForTest(jobDocAddress);

		AssertNullOrEmpty("ID should be empty", provider.Id);
		AssertNotNullOrEmpty("name should be be filled when no ID is found", provider.Name);
		AssertNotNull("Address should be filled when no ID is found", provider.Address);
	});

	class PartyProviderForTest : PartyProvider
	{
		public PartyProviderForTest(JobDocAddress jobDocAddress) : base(jobDocAddress)
		{
		}
		protected override bool OmitNameAndAddressWhenIDIsFound => true;
	}
}

using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(RepresentativeProvider))]
sealed class RepresentativeProviderTest : PartyProviderAbstractTest<RepresentativeProvider>
{
	public void TestStatus()
	{
		AssertEquals(3, Provider.Status);
	}

	public void TestConstructorNull()
	{
		AssertNull(RepresentativeProvider.New(Factory.New<JobDocAddress>()));
	}

	public new void TestAddress() => AssertNull(provider.Address);

	protected override RepresentativeProvider CreateProvider(JobDocAddress address) => RepresentativeProvider.New(CreateRepresentative());

	JobDocAddress CreateRepresentative()
	{
		var representative = nctsHeader.MovementHeader.Representative;
		var principal = nctsHeader.Principal;

		representative.E2_OA_Address = principal.E2_OA_Address;
		return representative;
	}
}

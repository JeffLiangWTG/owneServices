using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(ConsignmentProvider))]
abstract class ConsignmentProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : ConsignmentProvider
{
	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		provider = CreateProvider(nctsHeader);
	}

	protected NctsHeader nctsHeader;
	protected T provider;

	protected override T GetProvider() => provider;

	protected abstract T CreateProvider(NctsHeader nctsHeader);
}

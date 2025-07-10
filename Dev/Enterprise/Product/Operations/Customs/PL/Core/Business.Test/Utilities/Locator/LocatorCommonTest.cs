using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class LocatorCommonTest : LocatorTestBase<LocatorCommon, EDIMessage>
{
	protected override string ExpectedApplicationCodes => Constants.AllSupportedApplications;

	protected override string MessageApplicationCode => ApplicationCodeList.Codes.PLCustoms;

	protected override bool IgnoreFindByLrn => true;

	protected override BusinessObject CreateLinkedObject() => Factory.New<CusEntryHeader>();

	protected override void AllocateLRN(BusinessObject linkedObject, string lrn) { }
}

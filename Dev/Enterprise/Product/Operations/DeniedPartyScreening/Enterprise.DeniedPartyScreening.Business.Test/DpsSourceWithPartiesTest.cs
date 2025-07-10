using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.Business;

public class DpsSourceWithPartiesTest : TestCaseWithFactory
{
	public void TestGetSingleSourceList()
	{
		AssertExceptionThrown<ArgumentNullException>(() => DpsSourceWithParties.GetSingleSourceList(null, Array.Empty<ScreeningParty>()));
		AssertExceptionThrown<ArgumentNullException>(() => DpsSourceWithParties.GetSingleSourceList(Factory.New<OrgHeader>(), null));
		AssertNoExceptionThrown(() => DpsSourceWithParties.GetSingleSourceList(Factory.New<OrgHeader>(), Array.Empty<ScreeningParty>()));
	}
}

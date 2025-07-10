using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ScreeningPartyWrapper))]
	sealed class ScreeningPartyWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			ScreeningParty screeningParty = new ScreeningParty(org, "", OrgHeader.New(Factory));
			return new ScreeningPartyWrapper(screeningParty);
		}
	}
}

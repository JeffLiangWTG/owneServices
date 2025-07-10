using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ForwardingConsolExtensionsTest : TestCaseWithFactory
	{
		public void TestHasCoLoadData()
		{
			var coLoadOrg = Factory.New<OrgHeader>();
			coLoadOrg.OH_Code = "COL";
			coLoadOrg.Addresses.AddNewMainAddress();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals(false, consol.HasCoLoadData());
			consol.JK_CoLoadBookingReference = "BOOKING1";
			AssertEquals(true, consol.HasCoLoadData());

			consol.JK_CoLoadBookingReference = ZString.Empty;
			AssertEquals(false, consol.HasCoLoadData());

			consol.JK_CoLoadMasterBill = "MB1";
			AssertEquals(true, consol.HasCoLoadData());

			consol.JK_CoLoadMasterBill = ZString.Empty;
			AssertEquals(false, consol.HasCoLoadData());

			consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoadOrg.PK;
			AssertEquals(true, consol.HasCoLoadData());

			consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Invalid;
			AssertEquals(false, consol.HasCoLoadData());

			consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
			AssertEquals(false, consol.HasCoLoadData());
		}
	}
}

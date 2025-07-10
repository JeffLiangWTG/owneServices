using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class PackUnpackStatusHelperTest : TestCaseWithFactory
	{
		public void TestGetPackUnpackStatus()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			GlbBranch melbourneDepot = CreateDepot("AUMEL");

			AssertEquals("going out = pack", PackUnpackStatusHelper.PackUnpackStatus.Pack, PackUnpackStatusHelper.GetPackUnpackStatus("AUSYD", "SGSIN", Factory));
			AssertEquals("coming in = unpack", PackUnpackStatusHelper.PackUnpackStatus.Unpack, PackUnpackStatusHelper.GetPackUnpackStatus("SGSIN", "AUSYD", Factory));
			AssertEquals("they can't both be the same port", PackUnpackStatusHelper.PackUnpackStatus.InvalidSamePort, PackUnpackStatusHelper.GetPackUnpackStatus("AUSYD", "AUSYD", Factory));
			AssertEquals("none when depot exists where shipment should be packed", PackUnpackStatusHelper.PackUnpackStatus.None, PackUnpackStatusHelper.GetPackUnpackStatus("SGSIN", "AUMEL", Factory));

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "";
			AssertEquals("should barf if there's no home port to test against", PackUnpackStatusHelper.PackUnpackStatus.InvalidBranchHomePort, PackUnpackStatusHelper.GetPackUnpackStatus("AUSYD", "AUMEL", Factory));
		}

		public void TestGetPackUnpackStatusNotAtHomePort()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			AssertEquals("going out = pack", PackUnpackStatusHelper.PackUnpackStatus.Pack, PackUnpackStatusHelper.GetPackUnpackStatus("AUNTL", "SGSIN", Factory));
			AssertEquals("coming in = unpack", PackUnpackStatusHelper.PackUnpackStatus.Unpack, PackUnpackStatusHelper.GetPackUnpackStatus("SGSIN", "AUNTL", Factory));
		}

		#region Implementation

		protected ZString SavedPort;

		protected override void SetUp()
		{
			base.SetUp();
			SavedPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = SavedPort;
		}

		protected GlbBranch CreateDepot(ZString uNLOCO)
		{
			GlbBranch result = Factory.New<GlbBranch>();
			result.GB_Address1 = "Address 1 " + uNLOCO;
			result.GB_Code = "DEP";
			result.GB_BranchName = "Depot Branch " + uNLOCO;
			result.GB_RL_NKHomePort = uNLOCO;
			result.GB_OH_OrgProxy = CreateProxyOrg(uNLOCO).PK;
			return result;
		}

		protected OrgHeader CreateProxyOrg(ZString uNLOCO)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "Proxy Org Name" + uNLOCO;
			result.MainAddress.OA_Address1 = "Address 1 " + uNLOCO;
			result.OH_RL_NKClosestPort = uNLOCO;
			result.OH_IsUnpackDepot = true;
			return result;
		}

		#endregion
	}
}

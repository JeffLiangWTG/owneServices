using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Environment.Business.US.Testing
{
	public class TSAInfoTest : WhsTestCaseWithFactoryUS
	{
		#region Static Methods

		public void TestIsOrgAddressTSAKnown()
		{
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(null));

			OrgHeader org = Helper.CreateOrganization();
			OrgAddress address = org.MainAddress;
			AssertNotNull("Precondition: Address should not be null", address);
			address.OA_RL_NKRelatedPortCode = ZString.Empty;
			AssertNull("Precondition: Address related port code is null", address.RelatedPortCode);
			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Unknown);
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address));
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address, true));
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address, false));

			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Known);
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address));
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address, true));
			AssertEquals(true, TSAInfo.IsOrgAddressTSAKnown(address, false));

			AssertNotNull("Precondition: Address should not be null", address);
			address.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Enterprise.Core.Constants.CountryCodes.Australia).RL_Code;
			AssertNotNull("Precondition: Address related port code should not be null", address.RelatedPortCode);
			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Unknown);
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address));
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address, true));
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address, false));

			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Known);
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address));
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address, true));
			AssertEquals(true, TSAInfo.IsOrgAddressTSAKnown(address, false));

			address.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Enterprise.Core.Constants.CountryCodes.UnitedStates).RL_Code;
			AssertNotNull("Precondition: Address related port code should not be null", address.RelatedPortCode);
			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Unknown);
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address));
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address, true));
			AssertEquals(false, TSAInfo.IsOrgAddressTSAKnown(address, false));

			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Known);
			AssertEquals(true, TSAInfo.IsOrgAddressTSAKnown(address));
			AssertEquals(true, TSAInfo.IsOrgAddressTSAKnown(address, true));
			AssertEquals(true, TSAInfo.IsOrgAddressTSAKnown(address, false));
		}

		public void TestIsOrgHeaderTSAKnown()
		{
			AssertEquals(false, TSAInfo.IsOrgHeaderTSAKnown(null));

			OrgHeader org = Helper.CreateOrganization();
			OrgAddress address = org.MainAddress;
			AssertNotNull("Precondition: Address should not be null", address);
			address.OA_RL_NKRelatedPortCode = ZString.Empty;
			AssertNull("Precondition: Address related port code is null", address.RelatedPortCode);
			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Unknown);
			AssertEquals(false, TSAInfo.IsOrgHeaderTSAKnown(org));
			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Known);
			AssertEquals(true, TSAInfo.IsOrgHeaderTSAKnown(org));

			AssertNotNull("Precondition: Address should not be null", address);
			address.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Enterprise.Core.Constants.CountryCodes.Australia).RL_Code;
			AssertNotNull("Precondition: Address related port code should not be null", address.RelatedPortCode);
			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Unknown);
			AssertEquals(false, TSAInfo.IsOrgHeaderTSAKnown(org));
			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Known);
			AssertEquals(true, TSAInfo.IsOrgHeaderTSAKnown(org));

			address.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Enterprise.Core.Constants.CountryCodes.UnitedStates).RL_Code;
			AssertNotNull("Precondition: Address related port code should not be null", address.RelatedPortCode);
			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Unknown);
			AssertEquals(false, TSAInfo.IsOrgHeaderTSAKnown(org));
			Helper.SetOrgAddressTSAStatus(address, CodeLists.US.TSAStatus.Codes.Known);
			AssertEquals(true, TSAInfo.IsOrgHeaderTSAKnown(org));
		}

		#endregion

		#region Implementation

		#endregion
	}
}

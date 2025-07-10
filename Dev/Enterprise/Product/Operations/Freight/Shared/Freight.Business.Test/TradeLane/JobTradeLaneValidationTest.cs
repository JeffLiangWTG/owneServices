using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobTradeLaneValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEJ_Code()
		{
			TradeLane.EJ_Code = "Code";
			AssertNoErrors(TradeLane.EJ_CodeInfo);

			TradeLane.EJ_Code = ZString.Empty;
			AssertHasError(TradeLane.EJ_CodeInfo, "Please enter a " + TradeLane.EJ_CodeInfo.Description + ".");

			TradeLane.EJ_Code = "Code";
			AssertNoErrors(TradeLane.EJ_CodeInfo);
		}

		public void TestEJ_Location1()
		{
			TradeLane.EJ_Location1 = "AUSYD";
			AssertNoErrors(TradeLane.EJ_Location1Info);

			TradeLane.EJ_Location1 = ZString.Empty;
			AssertHasError(TradeLane.EJ_Location1Info, "Please enter a " + TradeLane.EJ_Location1Info.Description + ".");

			TradeLane.EJ_Location1 = "AUSYD";
			AssertNoErrors(TradeLane.EJ_Location1Info);
		}

		public void TestEJ_Location2()
		{
			TradeLane.EJ_Location2 = "AUBNE";
			AssertNoErrors(TradeLane.EJ_Location2Info);

			TradeLane.EJ_Location2 = ZString.Empty;
			AssertHasError(TradeLane.EJ_Location2Info, "Please enter a " + TradeLane.EJ_Location2Info.Description + ".");

			TradeLane.EJ_Location2 = "AUBNE";
			AssertNoErrors(TradeLane.EJ_Location2Info);
		}

		public void TestEJ_OH_RelatedOrg()
		{
			OrgHeader nonPrincipalOrg = Factory.New<OrgHeader>();
			nonPrincipalOrg.OH_IsShippingProvider = false;
			nonPrincipalOrg.OH_Code = "NonPrincipal";
			Factory.Save();
			TradeLane.EJ_OH_RelatedOrg = nonPrincipalOrg.PK;

			AssertHasError(TradeLane.EJ_OH_RelatedOrgInfo, "Enter a valid Principal.");

			OrgHeader principalOrg = Factory.New<OrgHeader>();
			principalOrg.OH_Code = "Principal";
			principalOrg.OH_IsShippingProvider = true;
			principalOrg.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();
			TradeLane.EJ_OH_RelatedOrg = principalOrg.PK;

			AssertNoErrors(TradeLane.EJ_OH_RelatedOrgInfo);
		}

		#region TradeLane

		JobTradeLane TradeLane
		{
			get { return tradeLane ?? (tradeLane = Factory.New<JobTradeLane>()); }
		}
		JobTradeLane tradeLane;

		#endregion
	}
}

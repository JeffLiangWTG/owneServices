using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class TestCFSLoadListConsolValidation : LoadListConsolTest
	{
		public void TestValidateJK_RL_NKLastForeignPort()
		{
			LoadList.JK_RL_NKLastForeignPort = ZString.Empty;
			AssertEquals("No error expected.", false, LoadList.JK_RL_NKLastForeignPortInfo.HasErrors());
		}

		public void TestValidateJK_OH_Forwarder()
		{
			LoadList.JK_OH_Forwarder = ZGuid.Empty;
			LoadList.Validation.ValidateJK_OH_Forwarder();
			AssertEquals("Error expected - empty client.", true, LoadList.JK_OH_ForwarderInfo.HasErrors());

			LoadList.JK_OH_Forwarder = ZGuid.Invalid;
			AssertEquals("Error expected - invalid client.", true, LoadList.JK_OH_ForwarderInfo.HasErrors());

			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_IsDebtor = false;
			LoadList.JK_OH_Forwarder = client.PK;
			AssertEquals("Error expected - client is not checked as receivables.", true, LoadList.JK_OH_ForwarderInfo.HasErrors());

			client.OH_IsDebtor = true;
			LoadList.JK_OH_Forwarder = ZGuid.Empty;
			LoadList.JK_OH_Forwarder = client.PK;
			AssertEquals("No Error expected - valid client is checked as receivables.", false, LoadList.JK_OH_ForwarderInfo.HasErrors());

			GlbCompany.CurrentCompany.OrgProxy.OH_IsDebtor = false;
			LoadList.JK_OH_Forwarder = GlbCompany.CurrentCompany.OrgProxy.PK;
			AssertEquals("No Error expected - valid client is checked as receivables.", false, LoadList.JK_OH_ForwarderInfo.HasErrors());

			GlbBranch.CurrentBranch.OrgProxy.OH_IsDebtor = false;
			LoadList.JK_OH_Forwarder = GlbBranch.CurrentBranch.OrgProxy.PK;
			AssertEquals("No Error expected - valid client is checked as receivables.", false, LoadList.JK_OH_ForwarderInfo.HasErrors());

			GlbCompany.CurrentCompany.Branches[0].OrgProxy.OH_IsDebtor = false;
			LoadList.JK_OH_Forwarder = GlbCompany.CurrentCompany.Branches[0].OrgProxy.PK;
			AssertEquals("No Error expected - valid client is checked as receivables.", false, LoadList.JK_OH_ForwarderInfo.HasErrors());
		}

		public void TestValidateJK_OA_CTOAddress()
		{
			LoadList.JK_OA_CTOAddress_ZAddress.IsOrgVisible = true;
			LoadList.JK_OA_CTOAddress_ZAddress.OrgPK = ZGuid.Empty;
			LoadList.JK_OA_CTOAddress = ZGuid.Empty;
			LoadList.Validation.ValidateJK_OA_CTOAddress();
			AssertEquals("No notification expected", false, LoadList.JK_OA_CTOAddressInfo.HasNotifications());

			LoadList.JK_OA_CTOAddress_ZAddress.OrgPK = ZGuid.Invalid;
			LoadList.Validation.ValidateJK_OA_CTOAddress();
			AssertEquals("Error expected", true, LoadList.JK_OA_CTOAddressInfo.HasErrors());

			LoadList.JK_OA_CTOAddress_ZAddress.OrgPK = ZGuid.NewZGuid();
			LoadList.Validation.ValidateJK_OA_CTOAddress();
			AssertEquals("Warning expected", true, LoadList.JK_OA_CTOAddressInfo.HasWarnings());

			LoadList.JK_OA_CTOAddress = ZGuid.Invalid;
			AssertEquals("Error expected", true, LoadList.JK_OA_CTOAddressInfo.HasErrors());

			LoadList.JK_OA_CTOAddress = ZGuid.NewZGuid();
			AssertEquals("No notification expected", false, LoadList.JK_OA_CTOAddressInfo.HasNotifications());
		}

		public void TestValidateJK_OA_EmptyContainerYard()
		{
			LoadList.JK_OA_EmptyContainerYard_ZAddress.IsOrgVisible = true;
			LoadList.JK_OA_EmptyContainerYard_ZAddress.OrgPK = ZGuid.Empty;
			LoadList.JK_OA_EmptyContainerYard = ZGuid.Empty;
			LoadList.Validation.ValidateJK_OA_EmptyContainerYard();
			AssertEquals("No notification expected", false, LoadList.JK_OA_EmptyContainerYardInfo.HasNotifications());

			LoadList.JK_OA_EmptyContainerYard_ZAddress.OrgPK = ZGuid.Invalid;
			LoadList.Validation.ValidateJK_OA_EmptyContainerYard();
			AssertEquals("Error expected", true, LoadList.JK_OA_EmptyContainerYardInfo.HasErrors());

			LoadList.JK_OA_EmptyContainerYard_ZAddress.OrgPK = ZGuid.NewZGuid();
			LoadList.Validation.ValidateJK_OA_EmptyContainerYard();
			AssertEquals("Warning expected", true, LoadList.JK_OA_EmptyContainerYardInfo.HasWarnings());

			LoadList.JK_OA_EmptyContainerYard = ZGuid.Invalid;
			AssertEquals("Error expected", true, LoadList.JK_OA_EmptyContainerYardInfo.HasErrors());

			LoadList.JK_OA_EmptyContainerYard = ZGuid.NewZGuid();
			AssertEquals("No notification expected", false, LoadList.JK_OA_EmptyContainerYardInfo.HasNotifications());
		}

		public void TestValidateDepotPK()
		{
			var currentCompany = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			LoadList.DepotPK = currentCompany.PK;
			AssertEquals("Error for company not set up as depot", true, LoadList.DepotPKInfo.HasErrors());

			currentCompany.OH_IsMiscFreightServices = true;
			currentCompany.OH_IsPackDepot = true;
			currentCompany.OH_IsUnpackDepot = true;
			Factory.Save();

			LoadList.DepotPK = ZGuid.Empty;
			AssertEquals("No notification expected - empty depot", false, LoadList.DepotPKInfo.HasErrors());

			LoadList.DepotPK = ZGuid.Invalid;
			AssertEquals("No notification expected - invalid depot", false, LoadList.DepotPKInfo.HasErrors());

			LoadList.DepotPK = currentCompany.PK;
			AssertEquals("No errors as company is now a valid depot", false, LoadList.DepotPKInfo.HasErrors());
		}

		public void TestValidateJK_OA_DepotAddress()
		{
			LoadList.JK_OA_DepotAddress = ZGuid.Empty;
			LoadList.Validation.ValidateJK_OA_DepotAddress();
			AssertHasError(LoadList.JK_OA_DepotAddressInfo, "Please enter a Depot Address.");

			LoadList.JK_OA_DepotAddress = ZGuid.Invalid;
			AssertHasError(LoadList.JK_OA_DepotAddressInfo, "Enter a valid selection.");

			LoadList.JK_OA_DepotAddress = ZGuid.NewZGuid();
			AssertNoError(LoadList.JK_OA_DepotAddressInfo, "Please enter a Depot Address.");
			AssertNoError(LoadList.JK_OA_DepotAddressInfo, "Enter a valid selection.");

			var currentCompany = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			LoadList.DepotPK = currentCompany.PK;

			AssertHasError(LoadList.JK_OA_DepotAddressInfo, "Enter a valid selection.");
			AssertHasError(LoadList.DepotPKInfo, "Enter a valid selection.");

			currentCompany.OH_IsMiscFreightServices = true;
			currentCompany.OH_IsPackDepot = true;
			currentCompany.OH_IsUnpackDepot = true;
			Factory.Save();

			LoadList.JK_OA_DepotAddress = currentCompany.MainAddress.PK;
			AssertEquals("No errors as company is now a valid depot", false, LoadList.JK_OA_DepotAddressInfo.HasErrors());
			AssertEquals("No errors as company is now a valid depot", false, LoadList.DepotPKInfo.HasErrors());
		}
	}
}

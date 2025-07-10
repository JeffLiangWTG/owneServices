using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestAuthorityToLeaveHelper : TestCaseWithFactory
	{
		public void TestGetConsignorAuthorityToLeave()
		{
			//Setup Consignor
			var consignorOrganisation = Factory.New<OrgHeader>();
			consignorOrganisation.OH_Code = "CNRSYD";
			var consignorDocAddress = Factory.New<JobDocAddress>();
			consignorDocAddress.E2_OA_Address = consignorOrganisation.Addresses.AddNew().PK;
			Assert(consignorDocAddress.Address != null);
			var consignorAddress = consignorDocAddress.Address;

			//Setup Consignee
			var consigneeOrganisation = Factory.New<OrgHeader>();
			consigneeOrganisation.OH_Code = "CNESYD";
			var consigneeAddress = consigneeOrganisation.Addresses.AddNew();

			//Check ATL is set to true if Consignor is a free text entry address
			consignorDocAddress.E2_AddressOverride = true;
			AssertEquals("Authority To Leave should be YES, as it Consignor is a free text address (overwritten)", true, AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeAddress));

			//Check ATL is obtained using OrgAddress (OA) if it is not a free text entry address
			consignorDocAddress.E2_AddressOverride = false;
			consignorAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;
			AssertEquals("Authority To Leave should be YES, as it was set via OrgAddress (OA)", false, AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeAddress));

			//Check ATL falls back to SupplerBuyerLink (OL) when OrgAddress (OA) is DEF --> if there is no SupplierBuyerLink, instead fallback to OrgMiscServ (OM)
			consignorAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			var getSupplierBuyerLink = AuthorityToLeaveHelper.GetSupplierBuyerLink(consigneeAddress, consignorAddress);
			Assert("No SupplierBuyerLink has been set up, therefore the link should be null", getSupplierBuyerLink == null);
			consignorOrganisation.MiscServ.OM_ConsignorAuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			AssertEquals("Since ATL on OrgAddress (OA) is DEF, and there is no SupplerBuyerLink, ATL should have defaulted back to OrgMiscServ (OM)", true, AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeAddress));

			//Setup SupplierBuyerLink
			var buyerSupplierLink = consignorOrganisation.BuyerLinks.AddNew(consigneeOrganisation);
			consigneeOrganisation.SupplierLinks.Add(buyerSupplierLink);
			getSupplierBuyerLink = AuthorityToLeaveHelper.GetSupplierBuyerLink(consignorDocAddress.Address, consigneeAddress);
			Assert("Should not be null as a SupplierBuyerLink was just added", getSupplierBuyerLink != null);

			//Check ATL falls back to SupplierBuyerLink (OL) correctly
			consignorAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			getSupplierBuyerLink.OL_AuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;
			AssertEquals("Since ATL in OrgAddress (OA) was set to DEF, should have defaulted back to the SupplierBuyerLink (OL)", false, AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeAddress));

			//Check ATL falls back to OrgMiscServ (OM) when SupplierBuyerLink (OL) is DEF
			getSupplierBuyerLink.OL_AuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			consignorOrganisation.MiscServ.OM_ConsignorAuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			AssertEquals("Since ATL in SupplierBuyerLink (OL) was set to DEF, should have defaulted back to OrgMiscServ (OM)", true, AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeAddress));

			//Check ATL falls back to registry item if OrgMiscServ (OM) is DEF
			consignorOrganisation.MiscServ.OM_ConsignorAuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			ObjectFactory.Get<TransportCommon.Integration.ITransportRegistry>().AuthorityToLeave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Since ATL in OrgMiscServ (OM) was set to DEF, should have defaulted back to registry item", false, AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeAddress));

			ObjectFactory.Get<TransportCommon.Integration.ITransportRegistry>().AuthorityToLeave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Since ATL in OrgMiscServ (OM) was set to DEF, should have defaulted back to registry item", true, AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeAddress));
		}

		public void TestGetConsignorAuthorityToLeave_YesOnSupplierLinkSucceeds()
		{
			var consignorOrganisation = Factory.New<OrgHeader>();
			consignorOrganisation.OH_Code = "CNRSYD";
			var consignorDocAddress = Factory.New<JobDocAddress>();
			consignorDocAddress.E2_OA_Address = consignorOrganisation.Addresses.AddNew().PK;
			var consignorAddress = consignorDocAddress.Address;

			var consigneeOrganisation = Factory.New<OrgHeader>();
			consigneeOrganisation.OH_Code = "CNESYD";
			var consigneeAddress = consigneeOrganisation.Addresses.AddNew();

			consignorAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			var buyerSupplierLink = consignorOrganisation.BuyerLinks.AddNew(consigneeOrganisation);
			consigneeOrganisation.SupplierLinks.Add(buyerSupplierLink);
			buyerSupplierLink.OL_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			AssertEquals("Since ATL in SupplierBuyerLink (OL) was set to YES, should have defaulted to true.", true, AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeAddress));
		}

		public void TestGetConsigneeAuthorityToLeave()
		{
			//Setup Consignor
			var consignorOrganisation = Factory.New<OrgHeader>();
			consignorOrganisation.OH_Code = "CNRSYD";
			var consignorAddress = consignorOrganisation.Addresses.AddNew();

			//Setup Consignee
			var consigneeOrganisation = Factory.New<OrgHeader>();
			consigneeOrganisation.OH_Code = "CNESYD";
			var consigneeAddress = consigneeOrganisation.Addresses.AddNew();

			//Check ATL is obtained using OrgAddress (OA)
			consigneeAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			AssertEquals("Authority To Leave should be YES, as it was set via OrgAddress (OA)", true, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consigneeAddress, consignorAddress));

			//Check ATL falls back to SupplerBuyerLink (OL) when OrgAddress (OA) is DEF --> if there is no SupplierBuyerLink, instead fallback to OrgMiscServ (OM)
			consigneeAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			var getSupplierBuyerLink = AuthorityToLeaveHelper.GetSupplierBuyerLink(consigneeAddress, consignorAddress);
			Assert("No SupplierBuyerLink has been set up, therefore the link should be null", getSupplierBuyerLink == null);
			consigneeOrganisation.MiscServ.OM_ConsigneeAuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			AssertEquals("Since ATL on OrgAddress (OA) is DEF, and there is no SupplerBuyerLink, ATL should have defaulted back to OrgMiscServ (OM)", true, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consigneeAddress, consignorAddress));

			//Setup SupplierBuyerLink
			var buyerSupplierLink = consignorOrganisation.BuyerLinks.AddNew(consigneeOrganisation);
			consigneeOrganisation.SupplierLinks.Add(buyerSupplierLink);
			getSupplierBuyerLink = AuthorityToLeaveHelper.GetSupplierBuyerLink(consignorAddress, consigneeAddress);
			Assert("Should not be null as a SupplierBuyerLink was just added", getSupplierBuyerLink != null);

			//Check ATL falls back to SupplierBuyerLink (OL) correctly
			getSupplierBuyerLink.OL_AuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;
			AssertEquals("Since ATL in OrgAddress (OA) was set to DEF, should have defaulted back to the SupplierBuyerLink (OL)", false, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consigneeAddress, consignorAddress));

			//Check ATL falls back to OrgMiscServ (OM) when SupplierBuyerLink (OL) is DEF
			getSupplierBuyerLink.OL_AuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			consignorOrganisation.MiscServ.OM_ConsignorAuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			AssertEquals("Since ATL in SupplierBuyerLink (OA) was set to DEF, should have defaulted back to OrgMiscServ (OM)", true, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consigneeAddress, consignorAddress));

			//Check ATL falls back to registry item if OrgMiscServ (OM) is DEF
			consigneeOrganisation.MiscServ.OM_ConsigneeAuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			ObjectFactory.Get<TransportCommon.Integration.ITransportRegistry>().AuthorityToLeave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Since ATL in OrgMiscServ (OM) was set to DEF, should have defaulted back to registry item", false, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consigneeAddress, consignorAddress));

			ObjectFactory.Get<TransportCommon.Integration.ITransportRegistry>().AuthorityToLeave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Since ATL in OrgMiscServ (OM) was set to DEF, should have defaulted back to registry item", true, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consigneeAddress, consignorAddress));
		}

		public void TestGetConsigneeAuthorityToLeave_YesOnSupplierLinkSucceeds()
		{
			var consignorOrganisation = Factory.New<OrgHeader>();
			consignorOrganisation.OH_Code = "CNRSYD";
			var consignorAddress = consignorOrganisation.Addresses.AddNew();

			var consigneeOrganisation = Factory.New<OrgHeader>();
			consigneeOrganisation.OH_Code = "CNESYD";
			var consigneeAddress = consigneeOrganisation.Addresses.AddNew();

			consigneeAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			var buyerSupplierLink = consignorOrganisation.BuyerLinks.AddNew(consigneeOrganisation);
			consigneeOrganisation.SupplierLinks.Add(buyerSupplierLink);
			buyerSupplierLink.OL_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			AssertEquals("Since ATL in SupplierBuyerLink (OL) was set to YES, should have defaulted to true.", true, AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consigneeAddress, consignorAddress));
		}

		public void TestGetClientAuthorityToLeave()
		{
			//Setup Client
			var clientOrganisation = Factory.New<OrgHeader>();
			clientOrganisation.OH_Code = "CLISYD";
			var clientAddress = clientOrganisation.Addresses.AddNew();

			//Check ATL is obtained using OrgAddress (OA)
			clientAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			AssertEquals("Authority To Leave should be YES, as it was set via OrgAddress (OA)", true, AuthorityToLeaveHelper.GetClientAuthorityToLeave(clientAddress));

			//Check ATL falls back to OrgMiscServ (OM) when OrgAddress (OA) is DEF
			clientAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			clientOrganisation.MiscServ.OM_CMAuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;
			AssertEquals("Since ATL in OrgAddress (OA) was set to DEF, should have defaulted back to OrgMiscServ (OM)", false, AuthorityToLeaveHelper.GetClientAuthorityToLeave(clientAddress));

			//Check ATL falls back to registry item if OrgMiscServ (OM) is DEF
			clientOrganisation.MiscServ.OM_CMAuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;
			ObjectFactory.Get<TransportCommon.Integration.ITransportRegistry>().AuthorityToLeave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Since ATL in OrgMiscServ (OM) was set to DEF, should have defaulted back to registry item", true, AuthorityToLeaveHelper.GetClientAuthorityToLeave(clientAddress));

			ObjectFactory.Get<TransportCommon.Integration.ITransportRegistry>().AuthorityToLeave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Since ATL in OrgMiscServ (OM) was set to DEF, should have defaulted back to registry item", false, AuthorityToLeaveHelper.GetClientAuthorityToLeave(clientAddress));
		}

		public void TestGetSupplierBuyerLink()
		{
			//Setup Consignor
			var consignorOrganisation = Factory.New<OrgHeader>();
			consignorOrganisation.OH_Code = "CNRSYD";
			var consignorAddress = consignorOrganisation.Addresses.AddNew();

			//Setup Consignee
			var consigneeOrganisation = Factory.New<OrgHeader>();
			consigneeOrganisation.OH_Code = "CNESYD";
			var consigneeAddress = consigneeOrganisation.Addresses.AddNew();

			//Check nothing is returned when there is no buyer link
			var getSupplierBuyerLink = AuthorityToLeaveHelper.GetSupplierBuyerLink(consigneeAddress, consignorAddress);
			Assert("No SupplierBuyerLink has been set up, therefore the link should be null", getSupplierBuyerLink == null);

			//Setup SupplierBuyerLink
			var buyerSupplierLink = consignorOrganisation.BuyerLinks.AddNew(consigneeOrganisation);
			consigneeOrganisation.SupplierLinks.Add(buyerSupplierLink);
			getSupplierBuyerLink = AuthorityToLeaveHelper.GetSupplierBuyerLink(consignorAddress, consigneeAddress);
			Assert("Should not be null as a SupplierBuyerLink was just added", getSupplierBuyerLink != null);
		}

		[ExpectNoExceptions]
		public void TestMethodsAreNullProof()
		{
			OrgAddress consignorOrgAddress = null;
			OrgAddress consigneeOrgAddress = null;
			OrgAddress clientOrgAddress = null;
			JobDocAddress consignorDocAddress = null;

			AuthorityToLeaveHelper.GetConsignorAuthorityToLeave(consignorDocAddress, consigneeOrgAddress);
			AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consigneeOrgAddress, consignorOrgAddress);
			AuthorityToLeaveHelper.GetClientAuthorityToLeave(clientOrgAddress);
			AuthorityToLeaveHelper.GetSupplierBuyerLink(consignorOrgAddress, consigneeOrgAddress);
		}
	}
}

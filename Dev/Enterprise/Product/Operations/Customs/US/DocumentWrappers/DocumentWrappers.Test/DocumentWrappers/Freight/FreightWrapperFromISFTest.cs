using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromISF))]
	sealed class FreightWrapperFromISFTest : FreightWrapperTest
	{
		public void TestOrganizationOverridenAddress()
		{
			var sellingParty = Factory.New<OrgHeader>();
			var sellingPartyAddress = Factory.New<OrgAddress>();
			sellingPartyAddress.OA_OH = sellingParty.PK;
			sellingPartyAddress.OA_Address1 = "Non-overriden selling party address";

			header.SellingParty.E2_OA_Address = sellingPartyAddress.PK;

			var wrapper = new FreightWrapperFromISF(header, Factory);
			AssertContains("When not overriden", "Non-overriden selling party address".ToUpper(), wrapper.SellingParty.CompanyNameAndAddress);

			header.SellingParty.E2_AddressOverride = true;
			header.SellingParty.E2_Address1 = "Overriden selling address";
			wrapper = new FreightWrapperFromISF(header, Factory);
			AssertContains("When overriden", "Overriden selling address".ToUpper(), wrapper.SellingParty.CompanyNameAndAddress);

			var mainShipToParty = Factory.New<OrgHeader>();
			var mainShipToPartyAddress = Factory.New<OrgAddress>();
			mainShipToPartyAddress.OA_OH = mainShipToParty.PK;
			mainShipToPartyAddress.OA_Address1 = "Non-overriden main ship to party address";

			header.MainShipToParty.E2_OA_Address = mainShipToPartyAddress.PK;

			wrapper = new FreightWrapperFromISF(header, Factory);
			AssertContains("When not overriden", "Non-overriden main ship to party address".ToUpper(), wrapper.MainShipToParty.CompanyNameAndAddress);

			header.MainShipToParty.E2_AddressOverride = true;
			header.MainShipToParty.E2_Address1 = "Overriden main ship to party";
			wrapper = new FreightWrapperFromISF(header, Factory);
			AssertContains("When overriden", "Overriden main ship to party".ToUpper(), wrapper.MainShipToParty.CompanyNameAndAddress);

			var consolidator = Factory.New<OrgHeader>();
			var consolidatorAddress = Factory.New<OrgAddress>();
			consolidatorAddress.OA_OH = consolidator.PK;
			consolidatorAddress.OA_Address1 = "Non-overriden consolidator address";

			header.Consolidator.E2_OA_Address = consolidatorAddress.PK;

			wrapper = new FreightWrapperFromISF(header, Factory);
			AssertContains("When not overriden", "Non-overriden consolidator address".ToUpper(), wrapper.Consolidator.CompanyNameAndAddress);

			header.Consolidator.E2_AddressOverride = true;
			header.Consolidator.E2_Address1 = "Overriden consolidator address";
			wrapper = new FreightWrapperFromISF(header, Factory);
			AssertContains("When overriden", "Overriden consolidator address".ToUpper(), wrapper.Consolidator.CompanyNameAndAddress);

			var stuffingLocation = Factory.New<OrgHeader>();
			var stuffingLocationAddress = Factory.New<OrgAddress>();
			stuffingLocationAddress.OA_OH = stuffingLocation.PK;
			stuffingLocationAddress.OA_Address1 = "Non-overriden stuffing location address";

			header.StuffingLocation.E2_OA_Address = stuffingLocationAddress.PK;

			wrapper = new FreightWrapperFromISF(header, Factory);
			AssertContains("When not overriden", "Non-overriden stuffing location address".ToUpper(), wrapper.StuffingLocation.CompanyNameAndAddress);

			header.StuffingLocation.E2_AddressOverride = true;
			header.StuffingLocation.E2_Address1 = "Overriden stuffing location address";
			wrapper = new FreightWrapperFromISF(header, Factory);
			AssertContains("When overriden", "Overriden stuffing location address".ToUpper(), wrapper.StuffingLocation.CompanyNameAndAddress);
		}

		public void TestOrganisations()
		{
			FreightWrapperFromISF wrapper = (FreightWrapperFromISF)GetSetupWrapperForDefaultFormatting();
			AssertEquals("MainShipToParty", "MR SHIP TO PARTY", wrapper.MainShipToParty.CompanyName);
			AssertEquals("SellingParty", "MR SELLING PARTY", wrapper.SellingParty.CompanyName);
			AssertEquals("Consolidator", "MR CONSOLIDATOR", wrapper.Consolidator.CompanyName);
			AssertEquals("StuffingLocation", "MR STUFFING LOCATION", wrapper.StuffingLocation.CompanyName);
			AssertEquals("ImportAgent", "MR IMPORTER", wrapper.ImportAgent.CompanyName);
		}

		public override void TestContainerLayoutStyle()
		{
			Assert(true); // containers on an ISF Header are a different type of object and do not have a container layout style
		}

		public void TestCustomsEntryNumber()
		{
			FreightWrapperFromISF wrapper = new FreightWrapperFromISF(header, Factory);
			header.BF_CustomsReference = "Planet Express";
			AssertEquals("Planet Express", wrapper.CustomsEntryNumber);
		}

		public override void TestBuyer()
		{
			OrgHeader buyingParty = Factory.New<OrgHeader>();
			buyingParty.OH_FullName = "MR BUYING PARTY";
			buyingParty.MainAddress.OA_Address1 = "BUYING PARTY ADDRESS 1";
			header.BuyingParty.E2_OA_Address = buyingParty.MainAddress.PK;
			FreightWrapperFromISF emptyWrapper = new FreightWrapperFromISF(header, Factory);
			AssertEquals("emptyWrapper.Buyer.CompanyName", "MR BUYING PARTY", emptyWrapper.Buyer.CompanyName);
		}

		public override void TestWrapperNotes()
		{
			header.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Some dangerous goods crap");

			FreightWrapperFromISF noteWrapper = new FreightWrapperFromISF(header, Factory);
			AssertEquals("noteWrapper.SpecialInstructions", "Some dangerous goods crap", noteWrapper.Notes[PredefinedNoteTypes.Instance.SpecialInstructions.Description].Text);
		}

		public void TestMainToPartyAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Some names";
			orgHeader.MainAddress.OA_Address1 = "Main address";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "Second address";
			orgAddress.OA_OH = orgHeader.PK;

			header.MainShipToParty.OrganisationPK = orgHeader.PK;
			header.MainShipToParty.E2_OA_Address = orgAddress.PK;

			FreightWrapperFromISF emptyWrapper = new FreightWrapperFromISF(header, Factory);

			AssertContains("Should be point to second address", "SECOND ADDRESS", emptyWrapper.MainShipToParty.CompanyNameAndAddress);
		}

		public void TestImportAgent()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			OrgAddress addressMain = importer.MainAddress;

			header.BF_OH_Importer = importer.PK;
			FreightWrapperFromISF wrapper = new FreightWrapperFromISF(header, Factory);

			ZGuid agentAddressPK = wrapper.ImportAgent.MainAddress.AddressPK;
			AssertEquals("Should point to Main address", addressMain.PK, agentAddressPK);

			OrgAddress addressCAR = importer.Addresses.AddNew();
			addressCAR.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);

			// need to regenerate wrapper as previous result is cached
			wrapper = new FreightWrapperFromISF(header, Factory);

			agentAddressPK = wrapper.ImportAgent.MainAddress.AddressPK;
			AssertEquals("Should point to Customs address", addressCAR.PK, agentAddressPK);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CusISFHeader>();
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "HouseBillHeading", "House Bill" },
					{ "JobNumberHeading", "ISF Job" },
					{ "MasterBillHeading", "Master Bill" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Buyer : MR BUYING PARTY\nBUYING PARTY ADDRESS 1\nAUSTRALIA
Consolidator : MR CONSOLIDATOR\nCONSOLIDATOR ADDRESS 1\nAUSTRALIA
ImportAgent : MR IMPORTER\nIMPORTER ADDRESS 1\nAUSTRALIA
MainShipToParty : MR SHIP TO PARTY\nSHIP TO PARTY ADDRESS 1\nAUSTRALIA
SellingParty : MR SELLING PARTY\nSELLING PARTY ADDRESS 1\nAUSTRALIA
StuffingLocation : MR STUFFING LOCATION\nSTUFFING LOCATION ADDRESS 1\nAUSTRALIA";
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusISFHeader>();
		}
		CusISFHeader header;

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "MR IMPORTER";
			importer.MainAddress.OA_Address1 = "IMPORTER ADDRESS 1";
			importer.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader shipToParty = Factory.New<OrgHeader>();
			shipToParty.OH_FullName = "MR SHIP TO PARTY";
			shipToParty.MainAddress.OA_Address1 = "SHIP TO PARTY ADDRESS 1";
			shipToParty.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader buyingParty = Factory.New<OrgHeader>();
			buyingParty.OH_FullName = "MR BUYING PARTY";
			buyingParty.MainAddress.OA_Address1 = "BUYING PARTY ADDRESS 1";
			buyingParty.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader sellingParty = Factory.New<OrgHeader>();
			sellingParty.OH_FullName = "MR SELLING PARTY";
			sellingParty.MainAddress.OA_Address1 = "SELLING PARTY ADDRESS 1";
			sellingParty.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader stuffingLocation = Factory.New<OrgHeader>();
			stuffingLocation.OH_FullName = "MR STUFFING LOCATION";
			stuffingLocation.MainAddress.OA_Address1 = "STUFFING LOCATION ADDRESS 1";
			stuffingLocation.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader consolidator = Factory.New<OrgHeader>();
			consolidator.OH_FullName = "MR CONSOLIDATOR";
			consolidator.MainAddress.OA_Address1 = "CONSOLIDATOR ADDRESS 1";
			consolidator.MainAddress.OA_RN_NKCountryCode = "AU";

			header.BF_OH_Importer = importer.PK;
			header.MainShipToParty.E2_OA_Address = shipToParty.MainAddress.PK;
			header.BuyingParty.E2_OA_Address = buyingParty.MainAddress.PK;
			header.SellingParty.E2_OA_Address = sellingParty.MainAddress.PK;
			header.StuffingLocation.E2_OA_Address = stuffingLocation.MainAddress.PK;
			header.Consolidator.E2_OA_Address = consolidator.MainAddress.PK;

			return new FreightWrapperFromISF(header, Factory);
		}

		protected override ZString ExpectedConsignorTypeDescription
		{
			get { return "Shipper"; }
		}
	}
}

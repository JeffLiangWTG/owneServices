using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class ICRBillWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new ICRBillWrapper(null);
		}

		public void TestWriteOffRequest()
		{
			Assert(!wrappedBill.WriteOffRequest); // WOF not supported from manifest
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", (ZShort)0, wrappedBill.SequenceNumber);
		}

		public void TestConsignmentValueInNZD()
		{
			Assert("ConsignmentValueInNZD should be Empty to be excluded from the message.", wrappedBill.ConsignmentValueInNZD.IsEmpty); // not required,  WOF only - probably wont support from manifest
		}

		public void TestPermits()
		{
			AssertEquals(Enumerable.Empty<ZString>(), wrappedBill.Permits);
		}

		public void TestMasterBill()
		{
			Assert(wrappedBill.MasterBill.IsEmpty);
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR, ApplicationCodeTypeList.Codes.ShippingLine);
			testBill.ABL_AMA = header.PK;
			Assert(wrappedBill.MasterBill.IsEmpty);
			header.MasterBill.ABL_BillNumber = "BILL001";
			AssertEquals("BILL001", wrappedBill.MasterBill);
		}

		public void TestConsigneeWithoutAnOrgHeader()
		{
			testBill.ABL_ConsigneeName = "TEST CONSIGNEE";
			testBill.ABL_ConsigneeCity = "AUCKLAND";
			testBill.ABL_RN_NKConsigneeCountry = "NZ";
			testBill.ABL_ConsigneeState = "NTH";
			testBill.ABL_ConsigneeStreet1 = "123 HILLY STREET";
			testBill.ABL_ConsigneeStreet2 = "LEVEL 2";
			testBill.ABL_ConsigneePostcode = "1234";
			testBill.ABL_ConsigneePhone = "9876543";
			AssertPartyDetails(wrappedBill.Consignee, "TEST CONSIGNEE", "AUCKLAND", "NZ", "NTH", "123 HILLY STREET LEVEL 2", "1234", "9876543");
			AssertSame(wrappedBill.Consignee, wrappedBill.Consignee);
		}

		public void TestConsigneeWithAnOrgHeader()
		{
			var testConsignee = Factory.NewWithValidTestData<OrgHeader>();
			testConsignee.OH_FullName = "ORG CONSIGNEE";
			var orgAddress = testConsignee.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_RN_NKCountryCode = "NZ";
			orgAddress.OA_PostCode = "203023";
			orgAddress.OA_Phone = "+4234232";
			testBill.ABL_OA_Consignee = testConsignee.MainAddress.PK;
			AssertPartyDetails(wrappedBill.Consignee, "ORG CONSIGNEE", "CITY", "NZ", "STATE", "ADDRESS 1 ADDRESS 2", "203023", "4234232");
		}

		public void TestConsignorWithoutAnOrgHeader()
		{
			testBill.ABL_ShipperName = "TEST CONSIGNOR";
			testBill.ABL_ShipperCity = "AUCKLAND";
			testBill.ABL_RN_NKShipperCountry = "NZ";
			testBill.ABL_ShipperState = "NTH";
			testBill.ABL_ShipperStreet1 = "123 HILLY STREET";
			testBill.ABL_ShipperStreet2 = "LEVEL 2";
			testBill.ABL_ShipperPostcode = "1234";
			AssertPartyDetails(wrappedBill.Consignor, "TEST CONSIGNOR", "AUCKLAND", "NZ", "NTH", "123 HILLY STREET LEVEL 2", "1234", ZString.Empty);
			AssertSame(wrappedBill.Consignor, wrappedBill.Consignor);
		}

		public void TestConsignorWithAnOrgHeader()
		{
			var testConsignor = Factory.NewWithValidTestData<OrgHeader>();
			testConsignor.OH_FullName = "ORG CONSIGNOR";
			var orgAddress = testConsignor.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_RN_NKCountryCode = "NZ";
			orgAddress.OA_PostCode = "203023";
			testBill.ABL_OA_Shipper = testConsignor.MainAddress.PK;
			AssertPartyDetails(wrappedBill.Consignor, "ORG CONSIGNOR", "CITY", "NZ", "STATE", "ADDRESS 1 ADDRESS 2", "203023", ZString.Empty);
		}

		public void TestNotifyPartyWithoutAnOrgHeader()
		{
			testBill.ABL_NotifyPartyName = "TEST NOTIFY PARTY";
			testBill.ABL_NotifyPartyCity = "AUCKLAND";
			testBill.ABL_RN_NKNotifyPartyCountry = "NZ";
			testBill.ABL_NotifyPartyState = "NTH";
			testBill.ABL_NotifyPartyStreet1 = "UNIT 1";
			testBill.ABL_NotifyPartyStreet2 = "123 Main Street";
			testBill.ABL_NotifyPartyPostcode = "4321";
			testBill.ABL_NotifyPartyPhone = "9871234";
			AssertPartyDetails(wrappedBill.NotifyParty, "TEST NOTIFY PARTY", "AUCKLAND", "NZ", "NTH", "UNIT 1 123 Main Street", "4321", "9871234");
			AssertSame(wrappedBill.NotifyParty, wrappedBill.NotifyParty);
		}

		public void TestNotifyPartyWithAnOrgHeader()
		{
			var testNotifyParty = Factory.NewWithValidTestData<OrgHeader>();
			testNotifyParty.OH_FullName = "ORG NOTIFY PARTY";
			var orgAddress = testNotifyParty.MainAddress;
			orgAddress.OA_Address1 = "ADDRESS 1";
			orgAddress.OA_Address2 = "ADDRESS 2";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "STATE";
			orgAddress.OA_RN_NKCountryCode = "NZ";
			orgAddress.OA_PostCode = "203023";
			testBill.ABL_OA_NotifyParty = testNotifyParty.MainAddress.PK;
			AssertPartyDetails(wrappedBill.NotifyParty, "ORG NOTIFY PARTY", "CITY", "NZ", "STATE", "ADDRESS 1 ADDRESS 2", "203023", ZString.Empty);
		}

		public void TestDeliveryNotifyParties()
		{
			AssertEquals(Enumerable.Empty<IOrganisationSimple>(), wrappedBill.DeliveryNotifyParties);
		}

		public void TestFreightPaymentMethod()
		{
			Assert(wrappedBill.FreightPaymentMethod.IsEmpty);
			testBill.ABL_PrepaidCollect = Core.Constants.DomesticPaymentTerms.Prepaid;
			AssertEquals(Core.Constants.DomesticPaymentTerms.Prepaid, wrappedBill.FreightPaymentMethod);
		}

		public void TestPortOfOrigin()
		{
			Assert(wrappedBill.PortOfOrigin.IsEmpty);
			testBill.ABL_RL_NKPortOfLoading = "USLAX";
			AssertEquals("USLAX", wrappedBill.PortOfOrigin);
		}

		public void TestGoodsLocation()
		{
			Assert(wrappedBill.GoodsLocation.IsEmpty);
		}

		public void TestPortOfLoading()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR, ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_RL_NKPortOfLoading = "AUADL";
			testBill.ABL_AMA = header.PK;
			AssertEquals("AUADL", wrappedBill.PortOfLoading);
		}

		public void TestBillNumber()
		{
			Assert(wrappedBill.BillNumber.IsEmpty);
			testBill.ABL_BillNumber = "BILL12";
			AssertEquals("BILL12", wrappedBill.BillNumber);
		}

		public void TestBillType()
		{
			Assert(wrappedBill.BillType.IsEmpty);
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR, ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			testBill.ABL_AMA = header.PK;
			AssertEquals(BillTypeList.Codes.HWB, wrappedBill.BillType);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(BillTypeList.Codes.BM, wrappedBill.BillType);
		}

		public void TestDeconsolidator()
		{
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.OH_FullName, wrappedBill.Deconsolidator.Name);
			AssertSame(wrappedBill.Deconsolidator, wrappedBill.Deconsolidator);
		}

		public void TestContainersSea()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR, ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			testBill.ABL_AMA = header.PK;
			var container1 = header.Containers.AddNew();
			var container2 = header.Containers.AddNew();
			var pack1 = testBill.Packs.AddNew();
			pack1.ContainerPK = container1.PK;
			testBill.Packs.AddNew();
			var pack2 = testBill.Packs.AddNew();
			pack2.ContainerPK = container1.PK;
			var pack3 = testBill.Packs.AddNew();
			pack3.ContainerPK = container2.PK;
			AssertEquals("Container Count", 2, wrappedBill.Containers.Count());
		}

		public void TestContainersAir()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR, ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			testBill.ABL_AMA = header.PK;
			Assert(!wrappedBill.Containers.Any());
			var container = header.Containers.AddNew();
			var pack1 = testBill.Packs.AddNew();
			pack1.ContainerPK = container.PK;
			Assert(!wrappedBill.Containers.Any());
		}

		public void TestPortOfDischarge()
		{
			Assert(wrappedBill.PortOfDischarge.IsEmpty);
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR, ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_RL_NKPortOfDischarge = "NZAKL";
			testBill.ABL_AMA = header.PK;
			AssertEquals("NZAKL", wrappedBill.PortOfDischarge);
		}

		public void TestHandlingInformation()
		{
			Assert(wrappedBill.HandlingInformation.IsEmpty);
		}

		public void TestMPIAccountDetails()
		{
			Assert(wrappedBill.MPIAccountDetails.IsEmpty);
		}

		public void TestConsignmentItems()
		{
			AssertEquals(0, wrappedBill.ConsignmentItems.Count());
			var pack1 = testBill.Packs.AddNew();
			pack1.APA_GoodsDescription = "STUFF";
			AssertEquals(1, wrappedBill.ConsignmentItems.Count());
			AssertEquals("STUFF", wrappedBill.ConsignmentItems.First().GoodsDescription);
		}

		public void TestDeliverToParty()
		{
			AssertNull(wrappedBill.DeliverToParty);
		}

		public void TestPremiseCode()
		{
			AssertNull(wrappedBill.TranshipmentDetails);
		}

		public void TestTranshipmentPorts()
		{
			AssertEquals(Enumerable.Empty<ZString>(), wrappedBill.TranshipmentPorts);
		}

		public void TestContainerPackLocation()
		{
			AssertEquals(Enumerable.Empty<IOrganisation>(), wrappedBill.ContainerPackingLocations);
		}

		public void TestMAFContainerDeclaration()
		{
			Assert(!wrappedBill.MAFContainerDeclaration);
		}

		public void TestMAFContainerStatements()
		{
			AssertEquals(Enumerable.Empty<ZString>(), wrappedBill.MAFContainerStatements);
		}

		public void TestMPIApprovedSystemNumbers()
		{
			AssertEquals(Enumerable.Empty<ZString>(), wrappedBill.MPIApprovedSystemNumbers);
		}

		public void TestSupportingDocuments()
		{
			AssertEquals(0, wrappedBill.SupportingDocuments.Count());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR, ApplicationCodeTypeList.Codes.ShippingLine);
			testBill = (AsycudaBill)header.Bills.AddNew();
			wrappedBill = new ICRBillWrapper(testBill);
		}

		AsycudaBill testBill;
		IICRConsignment wrappedBill;
		void AssertPartyDetails(IPartyInformation wrappedOrg, ZString name, ZString city, ZString countryCode, ZString countryRegion, ZString address, ZString postCode, ZString phoneNumber)
		{
			CombineAssertions(() =>
			{
				AssertEquals("NAME", name, wrappedOrg.Name);
				AssertEquals("CITY", city, wrappedOrg.City);
				AssertEquals("COUNTRY CODE", countryCode, wrappedOrg.CountryCode);
				AssertEquals("COUNTRY REGION", countryRegion, wrappedOrg.CountryRegion);
				AssertEquals("ADDRESS", address, wrappedOrg.Address);
				AssertEquals("POSTCODE", postCode, wrappedOrg.PostCode);
				if (!phoneNumber.IsEmpty)
				{
					AssertEquals(1, wrappedOrg.Communications.Count());
					var billCommunication = wrappedOrg.Communications.First();
					AssertEquals(CommunicationTypeList.Codes.TE, billCommunication.ContactType);
					AssertEquals("PHONE NUMBER", phoneNumber, billCommunication.ContactDetail);
				}
			});
		}
	}
}

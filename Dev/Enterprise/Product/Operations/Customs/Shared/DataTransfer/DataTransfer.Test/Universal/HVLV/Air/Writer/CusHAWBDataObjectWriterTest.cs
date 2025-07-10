using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest.Testing
{
	sealed class CusHAWBDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestCusHAWBDataMerge()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				var org3 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "SHOUSE1232",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					TransportMode = new CodeDescriptionPair() { Code = "AIR" },
					ShipmentType = new CodeDescriptionPair() { Code = "STD" },
					PortOfOrigin = new UNLOCO() { Code = "GBLON" },
					PortOfDestination = new UNLOCO() { Code = "SGSIN" },
					TotalWeight = 130m,
					TotalWeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Grams },
					TotalNoOfPieces = 150,
					GoodsDescription = "SHIPMENT GOODS",
					GoodsValue = 1504m,
					GoodsValueCurrency = new Currency() { Code = "AUD" }
				};
				dataObject.AddOrgAddress(writeManager, org1, DocAddressType.SendingForwarderAddress);
				dataObject.AddOrgAddress(writeManager, org2, DocAddressType.ReceivingForwarderAddress);
				dataObject.AddOrgAddress(writeManager, org3, DocAddressType.NotifyParty);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
				var mawb = SetupCusMAWB(Factory.New<CusMAWB>(), "MB324242", "CL32423");
				var hawb = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB3243", ZBool.True, "HB78785");
				hawb.CS_ConsignorName = org3.OH_FullName;
				hawb.CS_ConsignorStreet = org3.MainAddress.OA_Address1;
				hawb.CS_ConsignorStreet2 = org3.MainAddress.OA_Address2;
				hawb.CS_ConsignorCity = org3.MainAddress.OA_City;
				hawb.CS_ConsignorState = org3.MainAddress.OA_State;
				hawb.CS_ConsignorPostcode = org3.MainAddress.OA_PostCode;
				hawb.CS_RN_NKConsignorCountry = org3.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2);
				hawb.CS_ConsignorContactName = org3.Contacts[0].OC_ContactName;
				hawb.CS_ConsignorPhone = org3.MainAddress.OA_Phone;
				hawb.CS_OH_Consignee = org1.PK;
				hawb.CS_OH_Notify = org3.PK;
				Factory.SaveForTesting();

				var mockWriter1 = new Mock<CusHAWBDataObjectWriter>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, hawb)), new AirManifestDataObjectWriterHelper(hawb));
				mockWriter1.CallBase = true;
				mockWriter1
					.Protected()
					.Setup<bool>("ShouldKeepExistingData", ItExpr.IsAny<CusHAWB>())
					.Returns(true);
				IMergeDataObjectWriter writer1 = mockWriter1.Object;
				writer1.MergeData(dataObject, hawb);
				AssertNull("Should not have AirManifest", dataObject.GetMatchingDataSource(DataContextType.AirManifest));
				AssertNull("Should not have AirManifestLine", dataObject.GetMatchingDataSource(DataContextType.AirManifestLine));
				AssertEquals("WayBillNumber", "SHOUSE1232", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.House, dataObject.WayBillType.Code);
				AssertNotNull("TransportMode", dataObject.TransportMode);
				AssertEquals("TransportMode.Code", "AIR", dataObject.TransportMode.Code);
				AssertNotNull("ShipmentType", dataObject.ShipmentType);
				AssertEquals("ShipmentType.Code", "STD", dataObject.ShipmentType.Code);
				AssertNotNull("PortOfOrigin", dataObject.PortOfOrigin);
				AssertEquals("PortOfOrigin.Code", "GBLON", dataObject.PortOfOrigin.Code);
				AssertNotNull("PortOfDestination", dataObject.PortOfDestination);
				AssertEquals("PortOfDestination.Code", "SGSIN", dataObject.PortOfDestination.Code);
				AssertEquals("TotalWeight", 130m, dataObject.TotalWeight);
				AssertNotNull("TotalWeightUnit", dataObject.TotalWeightUnit);
				AssertEquals("TotalWeightUnit.Code", Core.Constants.Weight.Grams, dataObject.TotalWeightUnit.Code);
				AssertEquals("TotalNoOfPieces", 150, dataObject.TotalNoOfPieces);
				AssertEquals("GoodsDescription", "SHIPMENT GOODS", dataObject.GoodsDescription);
				AssertEquals("GoodsValue", 1504m, dataObject.GoodsValue);
				AssertNotNull("GoodsValueCurrency", dataObject.GoodsValueCurrency);
				AssertEquals("GoodsValueCurrency.Code", "AUD", dataObject.GoodsValueCurrency.Code);

				AssertNotNull("OrganizationAddressCollection", dataObject.OrganizationAddressCollection);
				AssertEquals("OrganizationAddressCollection.Count", 3, dataObject.OrganizationAddressCollection.Count);
				AssertOrganizationBO_WUFSHIJNB(DocAddressType.SendingForwarderAddress, dataObject);
				AssertOrganizationBO_CRAHOLSYD(DocAddressType.ReceivingForwarderAddress, dataObject);
				AssertOrganizationBO_INTHEMSYD(DocAddressType.NotifyParty, dataObject);

				var additionalBillCollection = dataObject.AdditionalBillCollection;
				AssertEquals("AdditionalBillCollection.Count", 1, additionalBillCollection.Count);
				var additionalBill = additionalBillCollection[0];
				AssertEquals("additionalBill.BillNumber", "HB3243", additionalBill.BillNumber);
				AssertNotNull("additionalBill.BillType", additionalBill.BillType);
				AssertEquals("additionalBill.BillType.Code", WayBillTypeList.Codes.MasterHouse, additionalBill.BillType.Code);
				AssertEquals("additionalBill.ParentBillNumber", "HB78785", additionalBill.ParentBillNumber);
				mockWriter1.VerifyAll();

				var mockWriter2 = new Mock<CusHAWBDataObjectWriter>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, hawb)), new AirManifestDataObjectWriterHelper(hawb));
				mockWriter2.CallBase = true;
				mockWriter2
					.Protected()
					.Setup<bool>("ShouldKeepExistingData", ItExpr.IsAny<CusHAWB>())
					.Returns(false);
				IMergeDataObjectWriter writer2 = mockWriter2.Object;
				writer2.MergeData(dataObject, hawb);
				AssertNull("Should not have AirManifest", dataObject.GetMatchingDataSource(DataContextType.AirManifest));
				AssertNull("Should not have AirManifestLine", dataObject.GetMatchingDataSource(DataContextType.AirManifestLine));
				AssertEquals("WayBillNumber", "HB3243", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.MasterHouse, dataObject.WayBillType.Code);
				AssertNotNull("TransportMode", dataObject.TransportMode);
				AssertEquals("TransportMode.Code", "AIR", dataObject.TransportMode.Code);
				AssertNotNull("ShipmentType", dataObject.ShipmentType);
				AssertEquals("ShipmentType.Code", "STD", dataObject.ShipmentType.Code);
				AssertNotNull("PortOfOrigin", dataObject.PortOfOrigin);
				AssertEquals("PortOfOrigin.Code", AirForeignPort2.RL_Code, dataObject.PortOfOrigin.Code);
				AssertNotNull("PortOfDestination", dataObject.PortOfDestination);
				AssertEquals("PortOfDestination.Code", AirLocalPort3.RL_Code, dataObject.PortOfDestination.Code);
				AssertEquals("TotalWeight", 1500.6m, dataObject.TotalWeight);
				AssertNotNull("TotalWeightUnit", dataObject.TotalWeightUnit);
				AssertEquals("TotalWeightUnit.Code", Core.Constants.Weight.Kilograms, dataObject.TotalWeightUnit.Code);
				AssertEquals("TotalNoOfPieces", 350, dataObject.TotalNoOfPieces);
				AssertEquals("GoodsDescription", "GOODS FOR TESTING", dataObject.GoodsDescription);
				AssertEquals("GoodsValue", 1304.5m, dataObject.GoodsValue);
				AssertNotNull("GoodsValueCurrency", dataObject.GoodsValueCurrency);
				AssertEquals("GoodsValueCurrency.Code", USD.RX_Code, dataObject.GoodsValueCurrency.Code);

				AssertNotNull("OrganizationAddressCollection", dataObject.OrganizationAddressCollection);
				AssertEquals("OrganizationAddressCollection.Count", 3, dataObject.OrganizationAddressCollection.Count);
				AssertAddress(DocAddressType.SendingForwarderAddress, dataObject, null, org3.OH_FullName, null,
					org3.MainAddress.OA_Address1, org3.MainAddress.OA_Address2, org3.MainAddress.OA_City, org3.MainAddress.OA_State, org3.MainAddress.OA_PostCode, org3.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2),
					org3.Contacts[0].OC_ContactName, null, null, null, org3.MainAddress.OA_Phone);
				AssertOrganizationBO_WUFSHIJNB(DocAddressType.ReceivingForwarderAddress, dataObject);
				AssertOrganizationBO_INTHEMSYD(DocAddressType.NotifyParty, dataObject);

				additionalBillCollection = dataObject.AdditionalBillCollection;
				AssertEquals("AdditionalBillCollection.Count", 1, additionalBillCollection.Count);
				additionalBill = additionalBillCollection[0];
				AssertEquals("additionalBill.BillNumber", "HB3243", additionalBill.BillNumber);
				AssertNotNull("additionalBill.BillType", additionalBill.BillType);
				AssertEquals("additionalBill.BillType.Code", WayBillTypeList.Codes.MasterHouse, additionalBill.BillType.Code);
				AssertEquals("additionalBill.ParentBillNumber", "HB78785", additionalBill.ParentBillNumber);

				dataObject = (Shipment)writer2.GetDataObject(hawb);
				AssertNotNull("Should have AirManifest", dataObject.GetMatchingDataSource(DataContextType.AirManifest));
				AssertNotNull("Should have AirManifestLine", dataObject.GetMatchingDataSource(DataContextType.AirManifestLine));
				AssertEquals("WayBillNumber", "MB324242", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.Master, dataObject.WayBillType.Code);
				AssertEquals("dataObject.SubShipmentCollection.Count", 1, dataObject.SubShipmentCollection.Count);
				dataObject = dataObject.SubShipmentCollection[0];
				AssertHVLVAirShipmentContents(dataObject, "HB3243", ZBool.True, "HB78785");
				AssertEquals("hawbSubData.OrganizationAddressCollection.Count", 3, dataObject.OrganizationAddressCollection.Count);
				AssertAddress(DocAddressType.SendingForwarderAddress, dataObject, null, org3.OH_FullName, null,
					org3.MainAddress.OA_Address1, org3.MainAddress.OA_Address2, org3.MainAddress.OA_City, org3.MainAddress.OA_State, org3.MainAddress.OA_PostCode, org3.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2),
					org3.Contacts[0].OC_ContactName, null, null, null, org3.MainAddress.OA_Phone);
				AssertOrganizationBO_WUFSHIJNB(DocAddressType.ReceivingForwarderAddress, dataObject);
				AssertOrganizationBO_INTHEMSYD(DocAddressType.NotifyParty, dataObject);

				additionalBillCollection = dataObject.AdditionalBillCollection;
				AssertEquals("AdditionalBillCollection.Count", 1, additionalBillCollection.Count);
				additionalBill = additionalBillCollection[0];
				AssertEquals("additionalBill.BillNumber", "HB3243", additionalBill.BillNumber);
				AssertNotNull("additionalBill.BillType", additionalBill.BillType);
				AssertEquals("additionalBill.BillType.Code", WayBillTypeList.Codes.MasterHouse, additionalBill.BillType.Code);
				AssertEquals("additionalBill.ParentBillNumber", "HB78785", additionalBill.ParentBillNumber);
				mockWriter2.VerifyAll();
			}
		}

		public void TestCusHAWBMappings()
		{
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var org3 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var mawb = SetupCusMAWB(Factory.New<CusMAWB>(), "MB324242", "CL32423");
			var hawb1 = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB3243", ZBool.True, ZString.Empty);
			hawb1.CS_ConsignorName = org3.OH_FullName;
			hawb1.CS_ConsignorStreet = org3.MainAddress.OA_Address1;
			hawb1.CS_ConsignorStreet2 = org3.MainAddress.OA_Address2;
			hawb1.CS_ConsignorCity = org3.MainAddress.OA_City;
			hawb1.CS_ConsignorState = org3.MainAddress.OA_State;
			hawb1.CS_ConsignorPostcode = org3.MainAddress.OA_PostCode;
			hawb1.CS_RN_NKConsignorCountry = org3.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2);
			hawb1.CS_ConsignorContactName = org3.Contacts[0].OC_ContactName;
			hawb1.CS_ConsignorPhone = org3.MainAddress.OA_Phone;
			hawb1.CS_OH_Consignee = org1.PK;
			var hawb2 = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB8956", ZBool.False, "HB3243");
			hawb2.CS_OH_Consignor = org1.PK;
			hawb2.CS_ConsigneeName = org2.OH_FullName;
			hawb2.CS_ConsigneeStreet = org2.MainAddress.OA_Address1;
			hawb2.CS_ConsigneeStreet2 = org2.MainAddress.OA_Address2;
			hawb2.CS_ConsigneeCity = org2.MainAddress.OA_City;
			hawb2.CS_ConsigneeState = org2.MainAddress.OA_State;
			hawb2.CS_ConsigneePostcode = org2.MainAddress.OA_PostCode;
			hawb2.CS_RN_NKConsigneeCountry = org2.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2);
			hawb2.CS_ConsigneeContactName = org2.Contacts[0].OC_ContactName;
			hawb2.CS_ConsigneePhone = org2.MainAddress.OA_Phone;
			hawb2.CS_CS_MasterHouseBill = hawb1.PK;
			hawb2.CS_VendorIdentifier = "EBAY";
			Factory.SaveForTesting();
			var writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = writer.GetDataObject(mawb);
			AssertEquals("mawbData.SubShipmentCollection.Count", 1, mawbData.SubShipmentCollection.Count);
			var hawbData = mawbData.SubShipmentCollection[0];
			AssertHVLVAirShipmentContents(hawbData, "HB3243", ZBool.True, null);
			AssertEquals("hawbData.OrganizationAddressCollection.Count", 3, hawbData.OrganizationAddressCollection.Count);
			AssertAddress(DocAddressType.SendingForwarderAddress, hawbData, null, org3.OH_FullName, null, org3.MainAddress.OA_Address1, org3.MainAddress.OA_Address2, org3.MainAddress.OA_City,
				org3.MainAddress.OA_State, org3.MainAddress.OA_PostCode, org3.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2), org3.Contacts[0].OC_ContactName, null, null, null, org3.MainAddress.OA_Phone);
			AssertOrganizationBO_WUFSHIJNB(DocAddressType.ReceivingForwarderAddress, hawbData);
			AssertAddress("NotifyParty", GetOrganizationAddressByType(hawbData, DocAddressType.NotifyParty), nameof(DocAddressType.NotifyParty), null, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, null, ZString.Empty);
			AssertEquals("hawbData.SubShipmentCollection.Count", 1, hawbData.SubShipmentCollection.Count);

			var hawbSubData = hawbData.SubShipmentCollection[0];
			AssertHVLVAirShipmentContents(hawbSubData, "HB8956", ZBool.False, "HB3243");
			AssertEquals("hawbSubData.OrganizationAddressCollection.Count", 3, hawbSubData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB(DocAddressType.ConsignorDocumentaryAddress, hawbSubData);
			AssertAddress(DocAddressType.ConsigneeDocumentaryAddress, hawbSubData, null, org2.OH_FullName, null, org2.MainAddress.OA_Address1, org2.MainAddress.OA_Address2, org2.MainAddress.OA_City,
				org2.MainAddress.OA_State, org2.MainAddress.OA_PostCode, org2.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2), org2.Contacts[0].OC_ContactName, null, null, null, org2.MainAddress.OA_Phone);
			AssertAddress(DocAddressType.NotifyParty, hawbSubData, null, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, null, ZString.Empty);
			AssertNull("hawbSubData.SubShipmentCollection", hawbSubData.SubShipmentCollection);
			AssertEquals("VendorIdentifier", "EBAY", hawbSubData.VendorIdentifier);
		}

		public void TestCusHawbNotifyMapping()
		{
			var wufOrg = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);

			var mawb = SetupCusMAWB(Factory.New<CusMAWB>(), "MB99997", ZString.Empty);
			var hawb1 = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB9876", ZBool.True, ZString.Empty);
			hawb1.CS_OH_Notify = wufOrg.PK;
			var hawb2 = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB1234", ZBool.True, ZString.Empty);
			hawb2.CS_NotifyName = "ENTERED NOTIFY PARTY";
			hawb2.CS_NotifyAddress1 = "NOTIFY ADDRESS 1";
			hawb2.CS_NotifyAddress2 = "NOTIFY ADDRESS 2";
			hawb2.CS_NotifySuburb = "NOTIFY SUBURB";
			hawb2.CS_NotifyState = "NOTIFY STATE";
			hawb2.CS_NotifyPostcode = "NTPCODE";
			hawb2.CS_RN_NKNotifyCountryCode = Core.Constants.CountryCodes.Jordan;
			hawb2.CS_NotifyPhone = "PH322740";
			hawb2.CS_NotifyFax = "FX322741";
			hawb2.CS_NotifyContactName = "NOTIFY CONTACT";
			Factory.SaveForTesting();
			var writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = writer.GetDataObject(mawb);

			AssertEquals("HAWB's", 2, mawbData.SubShipmentCollection.Count);
			var hawbData1 = mawbData.SubShipmentCollection[0];
			AssertOrganizationBO_WUFSHIJNB(DocAddressType.NotifyParty, hawbData1);
			var hawbData2 = mawbData.SubShipmentCollection[1];
			AssertAddress(DocAddressType.NotifyParty, hawbData2, null, "ENTERED NOTIFY PARTY", null,
				"NOTIFY ADDRESS 1", "NOTIFY ADDRESS 2", "NOTIFY SUBURB", "NOTIFY STATE", "NTPCODE", Core.Constants.CountryCodes.Jordan, "NOTIFY CONTACT", null, null, null, "PH322740");
		}

		public void TestCusMAWBDataMerge()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var org = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				var org2 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
				var mawbData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					WayBillNumber = "MB3234",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Air },
					BookingConfirmationReference = "BK32425",
					ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.AgentType.Agent },
					VoyageFlightNo = "QF324",
					Folio = "FL3",
					PortOfLoading = new UNLOCO() { Code = "AUSYD" },
					PortOfFirstArrival = new UNLOCO() { Code = "USCHI" },
					PortOfDischarge = new UNLOCO() { Code = "USLAX" },
					Branch = new Branch() { Code = "BR1" },
				};
				mawbData.SetDateCollection(() => new List<Date>(new[] { Date.New(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2012, 7, 5)) }));
				mawbData.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[]
						{
						new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
						{
							BillNumber = "BK32425",
							BillType = new WayBillType()
								{
									Code = WayBillTypeList.Codes.MasterHouse,
									Description = WayBillTypeList.Descriptions.MasterHouse
								},
							ParentBillNumber = "MB3234"
						}
					}));
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				mawbData.AddOrgAddress(writeManager, org, DocAddressType.SendingForwarderAddress);
				mawbData.AddOrgAddress(writeManager, org2, AddressTypes.ResponsibleParty);

				var depotOrg = Factory.NewWithValidTestData<OrgHeader>();
				var depotAddress2 = depotOrg.MainAddress;
				depotAddress2.FillWithValidTestData();
				depotAddress2.OA_Address1 = "DEPOT ADDRESS 2";
				mawbData.AddOrgAddress(writeManager, depotAddress2, DocAddressType.ArrivalCFSAddress);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
				var mawb = SetupCusMAWB(Factory.New<CusMAWB>(), "MB324242", "CL32423");
				mawb.Notes.AddNew(ZBool.True, "DUMMY NOTE", "HELLO WORLD");
				mawb.Notes.AddNew(ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
				var hawb1 = mawb.ChildBills.AddNew();
				hawb1.CS_HAWB = "HB24";
				var hawb2 = mawb.ChildBills.AddNew();
				hawb2.CS_HAWB = "HB89";
				var hawb3 = mawb.ChildBills.AddNew();
				hawb3.CS_HAWB = "SB243";
				hawb3.CS_MasterHouseBill = "HB89";
				hawb3.CS_CS_MasterHouseBill = hawb2.PK;
				Factory.SaveForTesting();
				var mockWriter1 = new Mock<CusMAWBDataObjectWriter>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
				mockWriter1.CallBase = true;
				mockWriter1
					.Protected()
					.Setup<bool>("ShouldKeepExistingData", ItExpr.IsAny<CusMAWB>())
					.Returns(true);
				IMergeDataObjectWriter writer1 = mockWriter1.Object;
				writer1.MergeData(mawbData, mawb);
				AssertNull("Should not have AirManifest", mawbData.GetMatchingDataSource(DataContextType.AirManifest));
				AssertEquals("mawbData.WayBillNumber", "MB3234", mawbData.WayBillNumber);
				AssertNotNull("mawbData.WayBillType", mawbData.WayBillType);
				AssertEquals("mawbData.WayBillType.Code", WayBillTypeList.Codes.Master, mawbData.WayBillType.Code);
				AssertNotNull("mawbData.TransportMode", mawbData.TransportMode);
				AssertEquals("mawbData.TransportMode.Code", Core.Constants.TransportModes.Air, mawbData.TransportMode.Code);
				AssertEquals("mawbData.BookingConfirmationReference", "BK32425", mawbData.BookingConfirmationReference);
				AssertNotNull("mawbData.ShipmentType", mawbData.ShipmentType);
				AssertEquals("mawbData.ShipmentType.Code", Core.Constants.AgentType.Agent, mawbData.ShipmentType.Code);
				AssertEquals("mawbData.VoyageFlightNo", "QF324", mawbData.VoyageFlightNo);
				AssertEquals("mawbData.Folio", "FL3", mawbData.Folio);
				AssertNotNull("mawbData.PortOfLoading", mawbData.PortOfLoading);
				AssertEquals("mawbData.PortOfLoading.Code", "AUSYD", mawbData.PortOfLoading.Code);
				AssertNotNull("mawbData.PortOfFirstArrival", mawbData.PortOfFirstArrival);
				AssertEquals("mawbData.PortOfFirstArrival.Code", "USCHI", mawbData.PortOfFirstArrival.Code);
				AssertNotNull("mawbData.PortOfDischarge", mawbData.PortOfDischarge);
				AssertEquals("mawbData.PortOfDischarge.Code", "USLAX", mawbData.PortOfDischarge.Code);
				AssertNotNull("mawbData.Branch", mawbData.Branch);
				AssertEquals("mawbData.Branch.Code", "BR1", mawbData.Branch.Code);

				AssertEquals("mawbData.AdditionalBillCollection.Count", 2, mawbData.AdditionalBillCollection.Count);
				AssertContents(mawbData.AdditionalBillCollection[0], "BK32425", new WayBillType()
				{
					Code = WayBillTypeList.Codes.MasterHouse,
					Description = WayBillTypeList.Descriptions.MasterHouse
				}, "MB3234");
				AssertContents(mawbData.AdditionalBillCollection[1], "CL32423", new WayBillType()
				{
					Code = WayBillTypeList.Codes.MasterHouse,
					Description = WayBillTypeList.Descriptions.MasterHouse
				}, "MB324242");

				AssertNotNull("mawbData.DateCollection", mawbData.DateCollection);
				AssertEquals("mawbData.DateCollection.Count", 3, mawbData.DateCollection.Count);
				AssertContents(mawbData.DateCollection[0], DateType.FirstArrivalInCountry, new ZDateTime(2012, 7, 5), ZBool.False);
				AssertContents(mawbData.DateCollection[1], DateType.LoadingDate, new ZDateTime(2012, 6, 4), ZBool.False);
				AssertContents(mawbData.DateCollection[2], DateType.DischargeDate, new ZDateTime(2012, 6, 6), ZBool.False);

				AssertEquals("mawbData.OrganizationAddressCollection.Count", 3, mawbData.OrganizationAddressCollection.Count);
				AssertOrganizationBO_CRAHOLSYD("SendingForwarderAddress", mawbData.OrganizationAddressCollection[0], nameof(DocAddressType.SendingForwarderAddress));
				AssertOrganizationBO_INTHEMSYD("SendingForwarderAddress", mawbData.OrganizationAddressCollection[1], AddressTypes.ResponsibleParty);
				AssertEquals("mawbData.AdditionalReferenceCollection.Count", 1, mawbData.AdditionalReferenceCollection.Count);
				AssertContents(mawbData.AdditionalReferenceCollection[0], "RPI2343", CodeDescriptionPairForTesting.New(Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID, Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID));
				AssertEquals("mawbData.NoteCollection.Count", 2, mawbData.NoteCollection.Count);
				AssertContents(mawbData.NoteCollection[0], ZBool.True, "DUMMY NOTE", "HELLO WORLD");
				AssertContents(mawbData.NoteCollection[1], ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");

				AssertEquals("DEPOT ADDRESS 2", mawbData.OrganizationAddressCollection[2].Address1);
				mockWriter1.VerifyAll();

				var mockWriter2 = new Mock<CusMAWBDataObjectWriter>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
				mockWriter2.CallBase = true;
				mockWriter2
					.Protected()
					.Setup<bool>("ShouldKeepExistingData", ItExpr.IsAny<CusMAWB>())
					.Returns(false);
				IMergeDataObjectWriter writer2 = mockWriter2.Object;
				writer2.MergeData(mawbData, mawb);
				AssertNull("Should not have AirManifest", mawbData.GetMatchingDataSource(DataContextType.AirManifest));
				AssertEquals("mawbData.WayBillNumber", "MB324242", mawbData.WayBillNumber);
				AssertNotNull("mawbData.WayBillType", mawbData.WayBillType);
				AssertEquals("mawbData.WayBillType.Code", WayBillTypeList.Codes.Master, mawbData.WayBillType.Code);
				AssertNotNull("mawbData.TransportMode", mawbData.TransportMode);
				AssertEquals("mawbData.TransportMode.Code", Core.Constants.TransportModes.Air, mawbData.TransportMode.Code);
				AssertEquals("mawbData.BookingConfirmationReference", "BK32425", mawbData.BookingConfirmationReference);
				AssertNotNull("mawbData.ShipmentType", mawbData.ShipmentType);
				AssertEquals("mawbData.ShipmentType.Code", Core.Constants.AgentType.Agent, mawbData.ShipmentType.Code);
				AssertEquals("mawbData.VoyageFlightNo", "QF123", mawbData.VoyageFlightNo);
				AssertEquals("mawbData.Folio", "FL324", mawbData.Folio);
				AssertNotNull("mawbData.PortOfLoading", mawbData.PortOfLoading);
				AssertEquals("mawbData.PortOfLoading.Code", AirForeignPort1.RL_Code, mawbData.PortOfLoading.Code);
				AssertNotNull("mawbData.PortOfFirstArrival", mawbData.PortOfFirstArrival);
				AssertEquals("mawbData.PortOfFirstArrival.Code", AirLocalPort1.RL_Code, mawbData.PortOfFirstArrival.Code);
				AssertNotNull("mawbData.PortOfDischarge", mawbData.PortOfDischarge);
				AssertEquals("mawbData.PortOfDischarge.Code", AirLocalPort2.RL_Code, mawbData.PortOfDischarge.Code);
				AssertNotNull("mawbData.Branch", mawbData.Branch);
				AssertEquals("mawbData.Branch.Code", USBranch.GB_Code, mawbData.Branch.Code);

				AssertEquals("mawbData.AdditionalBillCollection.Count", 2, mawbData.AdditionalBillCollection.Count);
				AssertContents(mawbData.AdditionalBillCollection[0], "BK32425", new WayBillType()
				{
					Code = WayBillTypeList.Codes.MasterHouse,
					Description = WayBillTypeList.Descriptions.MasterHouse
				}, "MB3234");
				AssertContents(mawbData.AdditionalBillCollection[1], "CL32423", new WayBillType()
				{
					Code = WayBillTypeList.Codes.MasterHouse,
					Description = WayBillTypeList.Descriptions.MasterHouse
				}, "MB324242");

				AssertNotNull("mawbData.DateCollection", mawbData.DateCollection);
				AssertEquals("mawbData.DateCollection.Count", 3, mawbData.DateCollection.Count);
				AssertContents(mawbData.DateCollection[0], DateType.LoadingDate, new ZDateTime(2012, 6, 4), ZBool.False);
				AssertContents(mawbData.DateCollection[1], DateType.FirstArrivalInCountry, new ZDateTime(2012, 6, 5), ZBool.False);
				AssertContents(mawbData.DateCollection[2], DateType.DischargeDate, new ZDateTime(2012, 6, 6), ZBool.False);

				AssertEquals("mawbData.OrganizationAddressCollection.Count", 3, mawbData.OrganizationAddressCollection.Count);
				AssertOrganizationBO_CRAHOLSYD("SendingForwarderAddress", mawbData.OrganizationAddressCollection[2], nameof(DocAddressType.SendingForwarderAddress));
				AssertOrganizationBO_WUFSHIJNB("ResponsibleParty", mawbData.OrganizationAddressCollection[1], AddressTypes.ResponsibleParty);
				AssertEquals("mawbData.AdditionalReferenceCollection.Count", 1, mawbData.AdditionalReferenceCollection.Count);
				AssertContents(mawbData.AdditionalReferenceCollection[0], "RPI2343", CodeDescriptionPairForTesting.New(Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID, Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID));
				AssertEquals("mawbData.NoteCollection.Count", 2, mawbData.NoteCollection.Count);
				AssertContents(mawbData.NoteCollection[0], ZBool.True, "DUMMY NOTE", "HELLO WORLD");
				AssertContents(mawbData.NoteCollection[1], ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");

				AssertEquals("DEPOT ADDRESS 1", mawbData.OrganizationAddressCollection[0].Address1);

				mawbData = (Shipment)writer2.GetDataObject(mawb);
				AssertNotNull("Should not have AirManifest", mawbData.GetMatchingDataSource(DataContextType.AirManifest));
				AssertHVLVAirManifestContents(mawbData, "MB324242", "CL32423");
				AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
				var hawbData1 = mawbData.SubShipmentCollection[0];
				AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
				AssertHVLVAirShipmentMasterHouse(hawbData1, null);
				AssertNull("hawbData1.SubShipmentCollection", hawbData1.SubShipmentCollection);
				var hawbData2 = mawbData.SubShipmentCollection[1];
				AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
				AssertHVLVAirShipmentMasterHouse(hawbData2, null);
				AssertEquals("hawbData2.SubShipmentCollection.Count", 1, hawbData2.SubShipmentCollection.Count);
				var hawbData2Sub = hawbData2.SubShipmentCollection[0];
				AssertEquals("hawbData2Sub.WayBillNumber", "SB243", hawbData2Sub.WayBillNumber);
				AssertHVLVAirShipmentMasterHouse(hawbData2Sub, "HB89");
				mockWriter2.VerifyAll();
			}
		}

		public void TestCusMAWBMappings()
		{
			var mawb = SetupCusMAWB(Factory.New<CusMAWB>(), "MB324242", "CL32423");
			mawb.Notes.AddNew(ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			mawb.Notes.AddNew(ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "HB24";
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB89";
			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "SB243";
			hawb3.CS_MasterHouseBill = "HB89";
			hawb3.CS_CS_MasterHouseBill = hawb2.PK;
			Factory.SaveForTesting();
			var writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = writer.GetDataObject(mawb);
			AssertHVLVAirManifestContents(mawbData, "MB324242", "CL32423");
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			var hawbData1 = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
			AssertHVLVAirShipmentMasterHouse(hawbData1, null);
			AssertNull("hawbData1.SubShipmentCollection", hawbData1.SubShipmentCollection);
			var hawbData2 = mawbData.SubShipmentCollection[1];
			AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
			AssertHVLVAirShipmentMasterHouse(hawbData2, null);
			AssertEquals("hawbData2.SubShipmentCollection.Count", 1, hawbData2.SubShipmentCollection.Count);
			var hawbData2Sub = hawbData2.SubShipmentCollection[0];
			AssertEquals("hawbData2Sub.WayBillNumber", "SB243", hawbData2Sub.WayBillNumber);
			AssertHVLVAirShipmentMasterHouse(hawbData2Sub, "HB89");

			mawb.CM_MasterHouseBill = ZString.Empty;
			Factory.SaveForTesting();
			writer = new CusMAWBDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			mawbData = writer.GetDataObject(mawb);
			AssertHVLVAirManifestContents(mawbData, "MB324242", null);
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			hawbData1 = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData1.WayBillNumber", "HB24", hawbData1.WayBillNumber);
			AssertHVLVAirShipmentMasterHouse(hawbData1, null);
			AssertNull("hawbData1.SubShipmentCollection", hawbData1.SubShipmentCollection);
			hawbData2 = mawbData.SubShipmentCollection[1];
			AssertEquals("hawbData2.WayBillNumber", "HB89", hawbData2.WayBillNumber);
			AssertHVLVAirShipmentMasterHouse(hawbData2, null);
			AssertEquals("hawbData2.SubShipmentCollection.Count", 1, hawbData2.SubShipmentCollection.Count);
			hawbData2Sub = hawbData2.SubShipmentCollection[0];
			AssertEquals("hawbData2Sub.WayBillNumber", "SB243", hawbData2Sub.WayBillNumber);
			AssertHVLVAirShipmentMasterHouse(hawbData2Sub, "HB89");
		}

		void AssertHVLVAirShipmentContents(Shipment hawbData, ZString? wayBillNumber, ZBool isMasterHouse, ZString? masterHouse)
		{
			AssertHVLVAirShipmentContents(hawbData, wayBillNumber, isMasterHouse, CodeDescriptionPairForTesting.New(AirForeignPort2.RL_Code, AirForeignPort2.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort3.RL_Code, AirLocalPort3.RL_PortName), 1500.60m, CodeDescriptionPairForTesting.New(Core.Constants.Weight.Kilograms, "Kilograms"), 350, "GOODS FOR TESTING", 1304.50m, CodeDescriptionPairForTesting.New(USD.RX_Code, USD.RX_Desc));
			AssertEquals("hawbData.AdditionalReferenceCollection.Count", 1, hawbData.AdditionalReferenceCollection.Count);
			AssertContents(hawbData.AdditionalReferenceCollection[0], "RPI9865463", CodeDescriptionPairForTesting.New(Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID, Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID));
			AssertHVLVAirShipmentMasterHouse(hawbData, masterHouse);
		}

		void AssertHVLVAirShipmentContents(Shipment hawbData, ZString? wayBillNumber, ZBool isMasterHouse, ICodeDescription portOfOrigin, ICodeDescription portOfDestination, ZDecimal? weight, ICodeDescription weightUQ, ZInt? piecesManifested, ZString? goodsDescription, ZDecimal? goodsValue, ICodeDescription goodsValueCurrency)
		{
			AssertNotNull("Precondition: mawbData", hawbData);

			CombineAssertions(delegate
			{
				AssertEquals("hawbData.WayBillNumber", wayBillNumber, hawbData.WayBillNumber);
				AssertNotNull("hawbData.WayBillType", hawbData.WayBillType);
				if (isMasterHouse)
				{
					AssertEquals("hawbData.WayBillType.Code", WayBillTypeList.Codes.MasterHouse, hawbData.WayBillType.Code);
					AssertEquals("hawbData.WayBillType.Description", WayBillTypeList.Descriptions.MasterHouse, hawbData.WayBillType.Description);
				}
				else
				{
					AssertEquals("hawbData.WayBillType.Code", WayBillTypeList.Codes.House, hawbData.WayBillType.Code);
					AssertEquals("hawbData.WayBillType.Description", WayBillTypeList.Descriptions.House, hawbData.WayBillType.Description);
				}

				AssertNotNull("hawbData.PortOfOrigin", hawbData.PortOfOrigin);
				AssertEquals("hawbData.PortOfOrigin.Code", portOfOrigin.Code, hawbData.PortOfOrigin.Code);
				AssertEquals("hawbData.PortOfOrigin.Name", portOfOrigin.Description, hawbData.PortOfOrigin.Name);
				AssertNotNull("hawbData.PortOfDestination", hawbData.PortOfDestination);
				AssertEquals("hawbData.PortOfDestination.Code", portOfDestination.Code, hawbData.PortOfDestination.Code);
				AssertEquals("hawbData.PortOfDestination.Name", portOfDestination.Description, hawbData.PortOfDestination.Name);
				AssertEquals("hawbData.TotalWeight", weight, hawbData.TotalWeight);
				AssertNotNull("hawbData.TotalWeightUnit", hawbData.TotalWeightUnit);
				AssertEquals("hawbData.TotalWeightUnit.Code", weightUQ.Code, hawbData.TotalWeightUnit.Code);
				AssertEquals("hawbData.TotalWeightUnit.Description", weightUQ.Description, hawbData.TotalWeightUnit.Description);
				AssertEquals("hawbData.TotalNoOfPieces", piecesManifested, hawbData.TotalNoOfPieces);
				AssertEquals("hawbData.GoodsDescription", goodsDescription, hawbData.GoodsDescription);
				AssertEquals("hawbData.GoodsValue", goodsValue, hawbData.GoodsValue);
				AssertNotNull("hawbData.GoodsValueCurrency", hawbData.TotalWeightUnit);
				AssertEquals("hawbData.GoodsValueCurrency.Code", goodsValueCurrency.Code, hawbData.GoodsValueCurrency.Code);
				AssertEquals("hawbData.GoodsValueCurrency.Description", goodsValueCurrency.Description, hawbData.GoodsValueCurrency.Description);
			});
		}

		void AssertHVLVAirShipmentMasterHouse(Shipment hawbData, ZString? masterHouse)
		{
			var additionalBillData = hawbData.AdditionalBillCollection.GetAdditionalBill(hawbData.WayBillNumber.GetValueOrDefault(), hawbData.WayBillType.GetCodeAsUpperCase());
			if (masterHouse.HasValue)
			{
				AssertEquals("additionalBillData.ParentBillNumber", masterHouse, additionalBillData.ParentBillNumber);
			}
			else
			{
				AssertNull("additionalBillData", additionalBillData);
			}
		}

		CusHAWB SetupCusHAWB(CusHAWB hawb, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse)
		{
			return SetupCusHAWB(hawb, wayBillNumber, isMasterHouse, masterHouse, AirForeignPort2.RL_Code, AirLocalPort3.RL_Code, 1500.60m, Core.Constants.Weight.Kilograms, 350, "GOODS FOR TESTING", 1304.50m, USD.RX_Code, "RPI9865463");
		}

		CusHAWB SetupCusHAWB(CusHAWB hawb, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse, ZString origin, ZString destination, ZDecimal weight, ZString weightUQ, ZShort piecesManifested, ZString goodsDescription, ZDecimal goodsValue, ZString goodsCurrency, ZString responsiblePartyID)
		{
			hawb.CS_HAWB = wayBillNumber;
			hawb.CS_IsMasterHouse = isMasterHouse;
			hawb.CS_MasterHouseBill = masterHouse;
			hawb.CS_RL_NKOrigin = origin;
			hawb.CS_RL_NKDestination = destination;
			hawb.CS_Weight = weight;
			hawb.CS_WeightUQ = weightUQ;
			hawb.CS_PiecesManifested = piecesManifested;
			hawb.CS_GoodsDescription = goodsDescription;
			hawb.CS_GoodsValue = goodsValue;
			hawb.CS_RX_NKGoodsCurrency = goodsCurrency;
			hawb.CS_ResponsiblePartyID = responsiblePartyID;
			return hawb;
		}

		static void AssertAddress(DocAddressType docAddressType, Shipment shipment, string organizationCode, string companyName, ZBool? addressOverride, string address1, string address2, string city, string state, string postcode, string country, string contact, string email, string fax, string mobile, string phone)
		{
			AssertAddress(docAddressType.ToString(), GetOrganizationAddressByType(shipment, docAddressType), docAddressType.ToString(), organizationCode, companyName, addressOverride, address1, address2, city, state, postcode, country, contact, email, fax, mobile, phone);
		}

		static void AssertOrganizationBO_CRAHOLSYD(DocAddressType docAddressType, Shipment shipment)
		{
			AssertOrganizationBO_CRAHOLSYD(docAddressType.ToString(), GetOrganizationAddressByType(shipment, docAddressType), docAddressType.ToString());
		}

		static void AssertOrganizationBO_INTHEMSYD(DocAddressType docAddressType, Shipment shipment)
		{
			AssertOrganizationBO_INTHEMSYD(docAddressType.ToString(), GetOrganizationAddressByType(shipment, docAddressType), docAddressType.ToString());
		}

		static void AssertOrganizationBO_WUFSHIJNB(DocAddressType docAddressType, Shipment shipment)
		{
			AssertOrganizationBO_WUFSHIJNB(docAddressType.ToString(), GetOrganizationAddressByType(shipment, docAddressType), docAddressType.ToString());
		}

		RefCurrency USD => usd ?? (usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates));
		RefCurrency usd;

		static OrganizationAddress GetOrganizationAddressByType(Shipment shipment, DocAddressType type)
		{
			return shipment.OrganizationAddressCollection.Single(org => org != null && org.AddressType.HasValue && org.AddressType.Value == type.ToString());
		}

		CusMAWB SetupCusMAWB(CusMAWB mawb, ZString wayBillNumber, ZString coloadBillNumber)
		{
			var responsibleParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			mawb.CM_MAWB = wayBillNumber;
			mawb.CM_MasterHouseBill = coloadBillNumber;
			mawb.CM_FlightNo = "QF123";
			mawb.CM_Folio = "FL324";
			mawb.CM_OH_ResponsibleParty = responsibleParty.PK;
			mawb.CM_ResponsiblePartyID = "RPI2343";
			mawb.CM_RL_NKLoadPort = AirForeignPort1.RL_Code;
			mawb.CM_RL_NKFirstArrivalPort = AirLocalPort1.RL_Code;
			mawb.CM_RL_NKDischargePort = AirLocalPort2.RL_Code;
			mawb.CM_GB = USBranch.PK;
			mawb.CM_DepartureDate = new ZDateTime(2012, 6, 4);
			mawb.CM_DateOfFirstArrival = new ZDateTime(2012, 6, 5);
			mawb.CM_ArrivalDate = new ZDateTime(2012, 6, 6);
			mawb.CM_OA_UnpackDepotAddress = DepotAddress1.PK;
			return mawb;
		}

		OrgAddress DepotAddress1
		{
			get
			{
				if (fDepotAddress1 == null)
				{
					var org = Factory.NewWithValidTestData<OrgHeader>();
					fDepotAddress1 = org.MainAddress;
					fDepotAddress1.FillWithValidTestData();
					fDepotAddress1.OA_Address1 = "DEPOT ADDRESS 1";
				}
				return fDepotAddress1;
			}
		}

		OrgAddress fDepotAddress1;

		GlbCompany USCompany
		{
			get
			{
				if (usCompany == null)
				{
					usCompany = Factory.New<GlbCompany>();
					usCompany.GC_Code = "US@";
					usCompany.GC_Name = "US Company Test";
					usCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
					usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				}
				return usCompany;
			}
		}

		GlbCompany usCompany;

		GlbBranch USBranch
		{
			get
			{
				if (usBranch == null)
				{
					usBranch = USCompany.Branches.AddNew();
					usBranch.GB_Code = "US@";
					usBranch.GB_BranchName = "US Branch Test";
					usBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				}
				return usBranch;
			}
		}

		GlbBranch usBranch;

		RefUNLOCO AirLocalPort1
		{
			get
			{
				if (airLocalPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort1;
			}
		}

		RefUNLOCO airLocalPort1;

		RefUNLOCO AirLocalPort2
		{
			get
			{
				if (airLocalPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, AirLocalPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort2;
			}
		}

		RefUNLOCO airLocalPort2;

		RefUNLOCO AirLocalPort3
		{
			get
			{
				if (airLocalPort3 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, new[] { AirLocalPort1.PK, AirLocalPort2.PK });
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort3 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort3;
			}
		}

		RefUNLOCO airLocalPort3;

		RefUNLOCO AirForeignPort1
		{
			get
			{
				if (airForeignPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airForeignPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airForeignPort1;
			}
		}

		RefUNLOCO airForeignPort1;

		RefUNLOCO AirForeignPort2
		{
			get
			{
				if (airForeignPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, AirForeignPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airForeignPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airForeignPort2;
			}
		}

		RefUNLOCO airForeignPort2;

		void AssertContents(AdditionalBill additionalBillDataObject, string billNumber, WayBillType billType, string parentBillNumber)
		{
			AssertNotNull("Precondition: additionalBillDataObject", additionalBillDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("additionalBillDataObject.BillNumber", billNumber, additionalBillDataObject.BillNumber);
				AssertNotNull("additionalBillDataObject.BillType", additionalBillDataObject.BillType);
				AssertEquals("additionalBillDataObject.BillType.Code", billType.Code, additionalBillDataObject.BillType.Code);
				AssertEquals("additionalBillDataObject.BillType.Description", billType.Description, additionalBillDataObject.BillType.Description);
				AssertEquals("additionalBillDataObject.ParentBillNumber", parentBillNumber, additionalBillDataObject.ParentBillNumber);
			});
		}

		void AssertContents(Date dateDataObject, DateType type, ZDateTime dateTime, ZBool isEstimate)
		{
			AssertNotNull("Precondition: dateDataObject", dateDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("dateDataObject.Type", type, dateDataObject.Type);
				AssertEquals("dateDataObject.Value", dateTime, dateDataObject.Value);
				AssertEquals("dateDataObject.IsEstimate", isEstimate, dateDataObject.IsEstimate);
			});
		}

		void AssertContents(Note noteData, ZBool isCustomDescription, ZString description, ZString noteText)
		{
			AssertNotNull("Precondition: noteData", noteData);
			CombineAssertions(delegate
			{
				AssertEquals("noteData.IsCustomDescription", isCustomDescription, noteData.IsCustomDescription);
				AssertEquals("noteData.Description", description, noteData.Description);
				AssertEquals("noteData.NoteText", noteText, noteData.NoteText);
			});
		}

		void AssertContents(AdditionalReference additionalReferenceDataObject, ZString referenceNumber, ICodeDescription type)
		{
			AssertNotNull("Precondition: additionalReferenceDataObject", additionalReferenceDataObject);
			CombineAssertions(delegate
			{
				AssertEquals("additionalReferenceDataObject.ReferenceNumber", referenceNumber, additionalReferenceDataObject.ReferenceNumber);
				AssertNotNull("additionalReferenceDataObject.Type", additionalReferenceDataObject.Type);
				AssertEquals("additionalReferenceDataObject.Type.Code", type.Code, additionalReferenceDataObject.Type.Code);
				AssertEquals("additionalReferenceDataObject.Type.Description", type.Description, additionalReferenceDataObject.Type.Description);
			});
		}

		void AssertHVLVAirManifestContents(Shipment mawbData, ZString? wayBillNumber, ZString? coloadBillNumber)
		{
			AssertAirCargoMasterContents(mawbData, wayBillNumber, coloadBillNumber, "QF123", "FL324", CodeDescriptionPairForTesting.New(AirForeignPort1.RL_Code, AirForeignPort1.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort2.RL_Code, AirLocalPort2.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort1.RL_Code, AirLocalPort1.RL_PortName), CodeDescriptionPairForTesting.New(USBranch.GB_Code, USBranch.GB_BranchName));
			AssertNotNull("mawbData.DateCollection", mawbData.DateCollection);
			AssertEquals("mawbData.DateCollection.Count", 3, mawbData.DateCollection.Count);
			AssertContents(mawbData.DateCollection[0], DateType.LoadingDate, new ZDateTime(2012, 6, 4), ZBool.False);
			AssertContents(mawbData.DateCollection[1], DateType.FirstArrivalInCountry, new ZDateTime(2012, 6, 5), ZBool.False);
			AssertContents(mawbData.DateCollection[2], DateType.DischargeDate, new ZDateTime(2012, 6, 6), ZBool.False);
			AssertEquals("mawbData.OrganizationAddressCollection.Count", 2, mawbData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("ResponsibleParty", mawbData.OrganizationAddressCollection[1], AddressTypes.ResponsibleParty);
			AssertEquals("mawbData.AdditionalReferenceCollection.Count", 1, mawbData.AdditionalReferenceCollection.Count);
			AssertContents(mawbData.AdditionalReferenceCollection[0], "RPI2343", CodeDescriptionPairForTesting.New(Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID, Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID));
			AssertEquals("mawbData.NoteCollection.Count", 2, mawbData.NoteCollection.Count);
			AssertContents(mawbData.NoteCollection[0], ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			AssertContents(mawbData.NoteCollection[1], ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
		}

		void AssertAirCargoMasterContents(Shipment mawbData, ZString? wayBillNumber, ZString? coloadBillNumber, ZString? flight, ZString? folio, ICodeDescription portOfLoading, ICodeDescription portOfDischarge, ICodeDescription portOfFirstArrival, ICodeDescription branch)
		{
			AssertNotNull("Precondition: mawbData", mawbData);

			CombineAssertions(delegate
			{
				AssertEquals("mawbData.WayBillNumber", wayBillNumber, mawbData.WayBillNumber);
				AssertNotNull("mawbData.WayBillType", mawbData.WayBillType);
				AssertEquals("mawbData.WayBillType.Code", WayBillTypeList.Codes.Master, mawbData.WayBillType.Code);
				AssertEquals("mawbData.WayBillType.Description", WayBillTypeList.Descriptions.Master, mawbData.WayBillType.Description);
				AssertNotNull("mawbData.TransportMode", mawbData.TransportMode);
				AssertEquals("mawbData.TransportMode.Code", Core.Constants.TransportModes.Air, mawbData.TransportMode.Code);
				AssertEquals("mawbData.TransportMode.Description", "Air Freight", mawbData.TransportMode.Description);
				if (coloadBillNumber.HasValue)
				{
					AssertNotNull("mawbData.AdditionalBillCollection", mawbData.AdditionalBillCollection);
					AssertNotNull("mawbData.AdditionalBillCollection should contain masterhouse", mawbData.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber == coloadBillNumber && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.MasterHouse && x.ParentBillNumber == wayBillNumber));
				}
				else if (mawbData.AdditionalBillCollection != null)
				{
					AssertNull("mawbData.AdditionalBillCollection should not contain masterhouse", mawbData.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber == coloadBillNumber && x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.MasterHouse && x.ParentBillNumber == wayBillNumber));
				}

				AssertEquals("mawbData.VoyageFlightNo", flight, mawbData.VoyageFlightNo);
				AssertEquals("mawbData.Folio", folio, mawbData.Folio);
				AssertNotNull("mawbData.PortOfLoading", mawbData.PortOfLoading);
				AssertEquals("mawbData.PortOfLoading.Code", portOfLoading.Code, mawbData.PortOfLoading.Code);
				AssertEquals("mawbData.PortOfLoading.Name", portOfLoading.Description, mawbData.PortOfLoading.Name);
				AssertNotNull("mawbData.PortOfDischarge", mawbData.PortOfDischarge);
				AssertEquals("mawbData.PortOfDischarge.Code", portOfDischarge.Code, mawbData.PortOfDischarge.Code);
				AssertEquals("mawbData.PortOfDischarge.Name", portOfDischarge.Description, mawbData.PortOfDischarge.Name);
				AssertNotNull("mawbData.PortOfFirstArrival", mawbData.PortOfFirstArrival);
				AssertEquals("mawbData.PortOfFirstArrival.Code", portOfFirstArrival.Code, mawbData.PortOfFirstArrival.Code);
				AssertEquals("mawbData.PortOfFirstArrival.Name", portOfFirstArrival.Description, mawbData.PortOfFirstArrival.Name);
				AssertNotNull("mawbData.Branch", mawbData.Branch);
				AssertEquals("mawbData.Branch.Code", branch.Code, mawbData.Branch.Code);
				AssertEquals("mawbData.Branch.Name", branch.Description, mawbData.Branch.Name);
			});
		}
	}
}

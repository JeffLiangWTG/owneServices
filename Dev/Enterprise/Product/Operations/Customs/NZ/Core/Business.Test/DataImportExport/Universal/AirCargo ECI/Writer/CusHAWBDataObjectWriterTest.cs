using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPairForTesting = Enterprise.Customs.DataTransfer.Universal.Testing.CodeDescriptionPairForTesting;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	partial class AirManifestDataObjectWriterTest
	{
		public void TestCusHAWBMappings()
		{
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var org3 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var mawb = SetupCusMAWB(Factory.New<CusMAWB>(), "MB324242");
			var hawb1 = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB3243");
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
			hawb1.CS_IsGSTPrePaid = "Y";
			hawb1.SetUserDefinedValue("CustomField1", (ZString)"CustomField1");
			hawb1.SetUserDefinedValue("CustomField2PART1", (ZString)"CustomField2PART1");
			hawb1.SetUserDefinedValue("CustomField2PART2", (ZString)"CustomField2PART2");

			var hawb2 = SetupCusHAWB2(mawb.ChildBills.AddNew(), "HB8956");
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
			hawb2.CS_IsGSTPrePaid = "N";
			Factory.SaveForTesting();
			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);
			AssertEquals("mawbData.SubShipmentCollection.Count", 2, mawbData.SubShipmentCollection.Count);
			var hawbData1 = mawbData.SubShipmentCollection[0];
			var hawbData2 = mawbData.SubShipmentCollection[1];
			if (hawbData2.WayBillNumber.GetValueOrDefault() == "HB3243")
			{
				hawbData1 = mawbData.SubShipmentCollection[1];
				hawbData2 = mawbData.SubShipmentCollection[0];
			}

			AssertHVLVAirShipmentContents(hawbData1, "HB3243");
			AssertEquals("hawbData.OrganizationAddressCollection.Count", 4, hawbData1.OrganizationAddressCollection.Count);
			AssertAddress("ConsignorDocumentaryAddress", GetOrganizationAddressByType(hawbData1, DocAddressType.ConsignorDocumentaryAddress), nameof(DocAddressType.ConsignorDocumentaryAddress), null, org3.OH_FullName, null,
				org3.MainAddress.OA_Address1, org3.MainAddress.OA_Address2, org3.MainAddress.OA_City, org3.MainAddress.OA_State, org3.MainAddress.OA_PostCode, org3.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2),
				org3.Contacts[0].OC_ContactName, null, null, null, org3.MainAddress.OA_Phone);
			AssertOrganizationBO_WUFSHIJNB("ConsigneeDocumentaryAddress", GetOrganizationAddressByType(hawbData1, DocAddressType.ConsigneeDocumentaryAddress), nameof(DocAddressType.ConsigneeDocumentaryAddress));
			AssertAddress("NotifyParty", GetOrganizationAddressByType(hawbData1, DocAddressType.NotifyParty), nameof(DocAddressType.NotifyParty), null, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, null, ZString.Empty);
			AssertEquals(3, hawbData1.CustomizedFieldCollection.Count);
			Assert(hawbData1.CustomizedFieldCollection.Any(x => x.Value.Equals("CustomField1")));
			Assert(hawbData1.CustomizedFieldCollection.Any(x => x.Value.Equals("CustomField2PART1")));
			Assert(hawbData1.CustomizedFieldCollection.Any(x => x.Value.Equals("CustomField2PART2")));
			AssertEquals("Y", hawbData1.AddInfoCollection.GetZStringValue("IsGSTPrePaid"));

			AssertHVLVAirShipmentContents2(hawbData2, "HB8956");
			AssertCodeDescription(hawbData2.ConsolidatedCargoStatus, new CodeDescriptionPair() { Code = LowValueConsignmentStatusList.Codes.ConsignmentHeld, Description = LowValueConsignmentStatusList.Descriptions.ConsignmentHeld });
			AssertEquals("hawbSubData.OrganizationAddressCollection.Count", 4, hawbData2.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("ConsignorDocumentaryAddress", GetOrganizationAddressByType(hawbData2, DocAddressType.ConsignorDocumentaryAddress), nameof(DocAddressType.ConsignorDocumentaryAddress));
			AssertAddress("ConsigneeDocumentaryAddress", GetOrganizationAddressByType(hawbData2, DocAddressType.ConsigneeDocumentaryAddress), nameof(DocAddressType.ConsigneeDocumentaryAddress), null, org2.OH_FullName, null,
				org2.MainAddress.OA_Address1, org2.MainAddress.OA_Address2, org2.MainAddress.OA_City, org2.MainAddress.OA_State, org2.MainAddress.OA_PostCode, org2.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2),
				org2.Contacts[0].OC_ContactName, null, null, null, org2.MainAddress.OA_Phone);
			AssertAddress("NotifyParty", GetOrganizationAddressByType(hawbData2, DocAddressType.NotifyParty), nameof(DocAddressType.NotifyParty), null, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, null, ZString.Empty);
			AssertEquals("N", hawbData2.AddInfoCollection.GetZStringValue("IsGSTPrePaid"));
		}

		public void TestExtraCusHAWBMappings()
		{
			var mawb = SetupCusMAWB(Factory.New<CusMAWB>(), "MB324242");
			var hawb = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB3243");
			var goodsLocation = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			hawb.CS_OA_GoodsLocation = goodsLocation.MainAddress.PK;
			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);
			var hawbData = mawbData.SubShipmentCollection[0];
			AssertEquals("hawbData.OrganizationAddressCollection.Count", 4, hawbData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_CRAHOLSYD("GoodsLocation", GetOrganizationAddressByType(hawbData, DocAddressType.GoodsLocation), nameof(DocAddressType.GoodsLocation));

			mawb.CM_ApplicationCode = Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "8892";
			subs.DG_Variant = "4";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			hawb.UNDGs.AddNew().LinkDefault(subs);

			manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			mawbData = (Shipment)writer.GetDataObject(mawb);
			hawbData = mawbData.SubShipmentCollection[0];
			AssertEquals("UNDG", "88924", hawbData.PackingLineCollection[0].UNDGCollection[0].UNDGCode);
		}

		public void TestGoodsDetails()
		{
			var mawb = SetupCusMAWB(Factory.New<CusMAWB>(), "MB123456");
			var hawb = SetupCusHAWB(mawb.ChildBills.AddNew(), "HB111");
			hawb.CS_GoodsDescription = "fish";
			hawb.CS_RN_NKGoodsOrigin = "AU";
			hawb.CS_GoodsValue = 1001;
			hawb.CS_RX_NKGoodsCurrency = "AUD";
			hawb.CS_PiecesManifested = 1002;
			hawb.CS_PackType = "BI";
			hawb.CS_Weight = 1003;
			hawb.CS_WeightUQ = "KG";
			hawb.CS_HarmonisedTariffNums = "1004";

			var hawbItems1 = hawb.CusHAWBItemsCollection.AddNew();
			hawbItems1.CHI_GoodsDescription = "chips";
			hawbItems1.CHI_RN_NKGoodsOrigin = "NZ";
			hawbItems1.CHI_GoodsValue = 2001;
			hawbItems1.CHI_RX_NKGoodsCurrency = "AUD";
			hawbItems1.CHI_PieceCount = 2002;
			hawbItems1.CHI_PackType = "BG";
			hawbItems1.CHI_Weight = 2003;
			hawbItems1.CHI_WeightUQ = "G";
			hawbItems1.CHI_HarmonisedTariffNums = "2004";

			var hawbItems2 = hawb.CusHAWBItemsCollection.AddNew();
			hawbItems2.CHI_GoodsDescription = "cola";
			hawbItems2.CHI_RN_NKGoodsOrigin = "US";
			hawbItems2.CHI_GoodsValue = 3001;
			hawbItems2.CHI_RX_NKGoodsCurrency = "AUD";
			hawbItems2.CHI_PieceCount = 3002;
			hawbItems2.CHI_PackType = "BX";
			hawbItems2.CHI_Weight = 3003;
			hawbItems2.CHI_WeightUQ = "LB";
			hawbItems2.CHI_HarmonisedTariffNums = "3004";

			var manager = (IShipmentDataContextManager)mawb.GetUniversalDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, mawb)));
			var mawbData = (Shipment)writer.GetDataObject(mawb);
			var hawbData = mawbData.SubShipmentCollection[0];
			AssertEquals("Three packing lines are populated, one is house bill, the other two are add. items.", 3, hawbData.PackingLineCollection.Count);

			AssertPackingLineGoodsDetails(hawbData.PackingLineCollection[0], "fish", "AU", 1001, "AUD", 1002, "BI", 1003, "KG", "1004");
			AssertPackingLineGoodsDetails(hawbData.PackingLineCollection[1], "chips", "NZ", 2001, "AUD", 2002, "BG", 2003, "G", "2004");
			AssertPackingLineGoodsDetails(hawbData.PackingLineCollection[2], "cola", "US", 3001, "AUD", 3002, "BX", 3003, "LB", "3004");
		}

		void AssertPackingLineGoodsDetails(PackingLine packingLine, ZString goodsDescription, ZString countryOfOriginCode, ZDecimal linePrice, ZString linePriceCurrencyCode, ZLong packQty, ZString packType, ZDecimal weight, ZString weightUnitCode, ZString harmonisedCode)
		{
			AssertEquals("GoodsDescription", goodsDescription, packingLine.GoodsDescription);
			AssertEquals("CountryOfOrigin", countryOfOriginCode, packingLine.CountryOfOrigin.Code);
			AssertEquals("LinePrice", linePrice, packingLine.LinePrice);
			AssertEquals("LinePriceCurrency", linePriceCurrencyCode, packingLine.LinePriceCurrency.Code);
			AssertEquals("PackQty", packQty, packingLine.PackQty);
			AssertEquals("PackType", packType, packingLine.PackType.Code);
			AssertEquals("Weight", weight, packingLine.Weight);
			AssertEquals("WeightUnit", weightUnitCode, packingLine.WeightUnit.Code);
			AssertEquals("HarmonisedCode", harmonisedCode, packingLine.HarmonisedCode);
		}

		void AssertCodeDescription(ICodeDescriptionDataObject actual, ICodeDescriptionDataObject expected)
		{
			if (expected == null)
			{
				AssertNull(actual);
			}
			else
			{
				AssertEquals("Code", expected.Code, actual.Code);
				AssertEquals("Description", expected.Description, actual.Description);
			}
		}

		void AssertHVLVAirShipmentContents(Shipment hawbData, ZString? wayBillNumber)
		{
			AssertHVLVAirShipmentContents(hawbData, wayBillNumber, CodeDescriptionPairForTesting.New(AirForeignPort2.RL_Code, AirForeignPort2.RL_PortName), CodeDescriptionPairForTesting.New(AirForeignPort1.RL_Code, AirForeignPort1.RL_PortName), 1500.60m, CodeDescriptionPairForTesting.New(Core.Constants.Weight.Kilograms, "Kilograms"), 350, "GOODS FOR TESTING", 1304.50m, CodeDescriptionPairForTesting.New(NZD.RX_Code, NZD.RX_Desc),
				CodeDescriptionPairForTesting.New(Core.Constants.CountryCodes.Singapore, "Singapore"), CodeDescriptionPairForTesting.New("EC", "Bag, plastic"),
				CodeDescriptionPairForTesting.New(AirForeignPort1.RL_Code, AirForeignPort1.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort2.RL_Code, AirLocalPort2.RL_PortName), CodeDescriptionPairForTesting.New(LowValueConsignmentStatusList.Codes.ManifestedReadyToSend, LowValueConsignmentStatusList.Descriptions.ManifestedReadyToSend));
		}

		void AssertHVLVAirShipmentContents2(Shipment hawbData, ZString? wayBillNumber)
		{
			AssertHVLVAirShipmentContents(hawbData, wayBillNumber, CodeDescriptionPairForTesting.New(AirForeignPort1.RL_Code, AirForeignPort1.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort2.RL_Code, AirLocalPort2.RL_PortName), 1000.60m, CodeDescriptionPairForTesting.New(Core.Constants.Weight.Pounds, "Pounds"), 860, "GOODS FOR TESTING 2", 150.50m, CodeDescriptionPairForTesting.New(NZD.RX_Code, NZD.RX_Desc),
				CodeDescriptionPairForTesting.New(Core.Constants.CountryCodes.Australia, "Australia"), CodeDescriptionPairForTesting.New("PK", "Package"),
				CodeDescriptionPairForTesting.New(AirForeignPort2.RL_Code, AirForeignPort2.RL_PortName), CodeDescriptionPairForTesting.New(AirLocalPort3.RL_Code, AirLocalPort3.RL_PortName), CodeDescriptionPairForTesting.New(LowValueConsignmentStatusList.Codes.ConsignmentHeld, LowValueConsignmentStatusList.Descriptions.ConsignmentHeld));
		}

		void AssertHVLVAirShipmentContents(Shipment hawbData, ZString? wayBillNumber, ICodeDescription portOfOrigin, ICodeDescription portOfDestination, ZDecimal? weight, ICodeDescription weightUQ, ZInt? piecesManifested, ZString? goodsDescription, ZDecimal? goodsValue, ICodeDescription goodsValueCurrency,
			ICodeDescription goodsCountry, ICodeDescription packageType, ICodeDescription portOfLoading, ICodeDescription portOfDischarge, ICodeDescription consolidatedCargoStatus)
		{
			AssertNotNull("Precondition: mawbData", hawbData);
			CombineAssertions(delegate
			{
				AssertEquals("hawbData.WayBillNumber", wayBillNumber, hawbData.WayBillNumber);
				AssertNotNull("hawbData.WayBillType", hawbData.WayBillType);
				AssertEquals("hawbData.WayBillType.Code", WayBillTypeList.Codes.House, hawbData.WayBillType.Code);
				AssertEquals("hawbData.WayBillType.Description", WayBillTypeList.Descriptions.House, hawbData.WayBillType.Description);

				AssertNotNull("hawbData.PortOfOrigin", hawbData.PortOfOrigin);
				AssertEquals("hawbData.PortOfOrigin.Code", portOfOrigin.Code, hawbData.PortOfOrigin.Code);
				AssertEquals("hawbData.PortOfOrigin.Name", portOfOrigin.Description, hawbData.PortOfOrigin.Name);
				AssertNotNull("hawbData.PortOfLoading", hawbData.PortOfLoading);
				AssertEquals("hawbData.PortOfLoading.Code", portOfLoading.Code, hawbData.PortOfLoading.Code);
				AssertEquals("hawbData.PortOfLoadPort.Name", portOfLoading.Description, hawbData.PortOfLoading.Name);
				AssertNotNull("hawbData.PortOfDischarge", hawbData.PortOfDischarge);
				AssertEquals("hawbData.PortOfDischarge.Code", portOfDischarge.Code, hawbData.PortOfDischarge.Code);
				AssertEquals("hawbData.PortOfDischarge.Name", portOfDischarge.Description, hawbData.PortOfDischarge.Name);
				AssertNotNull("hawbData.PortOfDestination", hawbData.PortOfDestination);
				AssertEquals("hawbData.PortOfDestination.Code", portOfDestination.Code, hawbData.PortOfDestination.Code);
				AssertEquals("hawbData.PortOfDestination.Name", portOfDestination.Description, hawbData.PortOfDestination.Name);
				AssertEquals("hawbData.TotalWeight", weight, hawbData.TotalWeight);
				AssertNotNull("hawbData.TotalWeightUnit", hawbData.TotalWeightUnit);
				AssertEquals("hawbData.TotalWeightUnit.Code", weightUQ.Code, hawbData.TotalWeightUnit.Code);
				AssertEquals("hawbData.TotalWeightUnit.Description", weightUQ.Description, hawbData.TotalWeightUnit.Description);
				AssertEquals("hawbData.TotalNoOfPieces", piecesManifested, hawbData.TotalNoOfPieces);
				AssertNotNull("hawbData.TotalNoOfPacksPackageType", hawbData.TotalNoOfPacksPackageType);
				AssertEquals("hawbData.TotalNoOfPacksPackageType.Code", packageType.Code, hawbData.TotalNoOfPacksPackageType.Code);
				AssertEquals("hawbData.TotalNoOfPacksPackageType.Description", packageType.Description, hawbData.TotalNoOfPacksPackageType.Description);
				AssertEquals("hawbData.GoodsDescription", goodsDescription, hawbData.GoodsDescription);
				AssertEquals("hawbData.GoodsValue", goodsValue, hawbData.GoodsValue);
				AssertNotNull("hawbData.GoodsValueCurrency", hawbData.TotalWeightUnit);
				AssertEquals("hawbData.GoodsValueCurrency.Code", goodsValueCurrency.Code, hawbData.GoodsValueCurrency.Code);
				AssertEquals("hawbData.GoodsValueCurrency.Description", goodsValueCurrency.Description, hawbData.GoodsValueCurrency.Description);
				AssertNotNull("hawbData.CountryOfSupply", hawbData.CountryOfSupply);
				AssertEquals("hawbData.CountryOfSupply.Code", goodsCountry.Code, hawbData.CountryOfSupply.Code);
				AssertEquals("hawbData.CountryOfSupply.Description", goodsCountry.Description, hawbData.CountryOfSupply.Name);
				AssertNotNull("hawbData.ConsolidatedCargoStatus", hawbData.ConsolidatedCargoStatus);
				AssertEquals("hawbData.ConsolidatedCargoStatus.Code", consolidatedCargoStatus.Code, hawbData.ConsolidatedCargoStatus.Code);
				AssertEquals("hawbData.ConsolidatedCargoStatus.Description", consolidatedCargoStatus.Description, hawbData.ConsolidatedCargoStatus.Description);
			});
		}

		CusHAWB SetupCusHAWB(CusHAWB hawb, ZString wayBillNumber)
		{
			return SetupCusHAWB(hawb, wayBillNumber, AirForeignPort2.RL_Code, AirLocalPort3.RL_Code, 1500.60m, Core.Constants.Weight.Kilograms, 350, "GOODS FOR TESTING", 1304.50m, NZD.RX_Code,
				Core.Constants.CountryCodes.Singapore, "EC", AirForeignPort1.RL_Code, AirLocalPort2.RL_Code, LowValueConsignmentStatusList.Codes.ManifestedReadyToSend);
		}

		CusHAWB SetupCusHAWB2(CusHAWB hawb, ZString wayBillNumber)
		{
			return SetupCusHAWB(hawb, wayBillNumber, AirForeignPort1.RL_Code, AirLocalPort2.RL_Code, 1000.60m, Core.Constants.Weight.Pounds, 860, "GOODS FOR TESTING 2", 150.50m, NZD.RX_Code,
				Core.Constants.CountryCodes.Australia, "PK", AirForeignPort2.RL_Code, AirLocalPort3.RL_Code, LowValueConsignmentStatusList.Codes.ConsignmentHeld);
		}

		CusHAWB SetupCusHAWB(CusHAWB hawb, ZString wayBillNumber, ZString origin, ZString destination, ZDecimal weight, ZString weightUQ, ZShort piecesManifested, ZString goodsDescription, ZDecimal goodsValue, ZString goodsCurrency,
			ZString goodsOrigin, ZString packType, ZString loadPort, ZString dischargePort, ZString customsStatus)
		{
			hawb.CS_HAWB = wayBillNumber;
			hawb.CS_RL_NKOrigin = origin;
			hawb.CS_RL_NKDestination = destination;
			hawb.CS_Weight = weight;
			hawb.CS_WeightUQ = weightUQ;
			hawb.CS_PiecesManifested = piecesManifested;
			hawb.CS_GoodsDescription = goodsDescription;
			hawb.CS_GoodsValue = goodsValue;
			hawb.CS_RX_NKGoodsCurrency = goodsCurrency;
			hawb.CS_RN_NKGoodsOrigin = goodsOrigin;
			hawb.CS_PackType = packType;
			hawb.CS_RL_NKLoadPort = loadPort;
			hawb.CS_RL_NKDischargePort = dischargePort;
			hawb.CS_CustomsStatus = customsStatus;
			return hawb;
		}

		#region Implementation

		RefCurrency NZD
		{
			get { return nzd ?? (nzd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.NewZealand)); }
		}
		RefCurrency nzd;

		static OrganizationAddress GetOrganizationAddressByType(Shipment shipment, DocAddressType type)
		{
			return shipment.OrganizationAddressCollection.Single(org => org != null && org.AddressType.HasValue && org.AddressType.Value == type.ToString());
		}

		protected override void SetUp()
		{
			base.SetUp();
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory.BOFactory,
				new ZArchitecture.Core.CodeDescriptionPair("EC", "Bag, plastic"),
				new ZArchitecture.Core.CodeDescriptionPair("PK",  "Package"));
		}

		#endregion
	}
}

using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	using Enterprise.UniversalDataBuss.DataObjects;
	using Enterprise.UniversalDataBuss.DataObjects.Core;
	using Enterprise.UniversalDataBuss.DataObjects.Universal;
	using Enterprise.UniversalDataBuss.Management;
	using Enterprise.UniversalDataBuss.Management.Testing;

	partial class AirManifestDataObjectReaderTest
	{
		public void TestStringValueCharacterCaseSetToUpper()
		{
			var reader = new CusHAWBDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new UniversalDataBuss.Integration.DummyLogger(), new DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper(Factory, "NZ"), Factory.New<CusMAWB>(), null, false);
			var type = typeof(CusHAWBDataObjectReader);
			var stringValueCaseProperty = type.GetProperty("StringValueCharacterCase", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			AssertEquals("StringValueCharacterCase is set to Upper", CharacterCase.Upper, stringValueCaseProperty.GetValue(reader));
		}

		public void TestGetReasonForNotAbleToUpdateFromNZCusHAWB()
		{
			var mawb = Factory.BOFactory.New<CusMAWB>();
			mawb.CM_FlightNo = "QF344";
			mawb.CM_ArrivalDate = ZDate.Today;
			mawb.CM_MAWB = "MB2343";

			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HB2343";
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;

			Factory.SaveForTesting();

			var hawbDataObject = SetupAirCargoHouse("HB2343");
			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			mawbDataObject.VoyageFlightNo = "QF344";
			mawbDataObject.SetDateCollection(() => new List<Date>());
			mawbDataObject.DateCollection.Add(new Date() { Type = DateType.DischargeDate, Value = ZDate.Today });
			mawbDataObject.PortOfLoading = new UNLOCO() { Code = "AUSYD" };
			mawbDataObject.PortOfDischarge = new UNLOCO() { Code = "NZAKL" };
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject);

			hawbDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					LinePriceCurrency = new Currency { Code = "AUD" }
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					LinePriceCurrency = new Currency { Code = "AUD" }
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					LinePriceCurrency = new Currency { Code = "NZD" }
				}
			});

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Logs", @"Successfully loaded matching CusMAWB.
Populating CusMAWB...
Successfully loaded matching CusHAWB.
Error - Cannot populate CusHAWB because:
Messaging is active for Consignment: (HAWB: HB2343)
Import failed as line price currency needs to match on all lines
Updated AirCargo Report (MAWB: MB2343 Job#: X00001000) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: MB2343 Job#: X00001000).", message.GetLogNoteText());
		}

		public void TestImportingCusHAWBData()
		{
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var hawbDataObject1 = SetupAirCargoHouse("HB2343");
			var consignorData = hawbDataObject1.AddOrgAddress(writeManager, org1, DocAddressType.ConsignorDocumentaryAddress);
			var consigneeData = SetupOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			consigneeData.Contact = "BOB THE BUILDER";
			consigneeData.Phone = "3234 23432";
			hawbDataObject1.OrganizationAddressCollection.Add(consigneeData);

			hawbDataObject1.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine1.SetUNDGCollection(() =>
			{
				var list = new List<UNDG>();
				var undg1 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
				undg1.UNDGCode = "1111";
				list.Add(undg1);
				return list;
			});
			hawbDataObject1.PackingLineCollection.Add(packingLine1);

			hawbDataObject1.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo
				{
					Key = "IsGSTPrePaid",
					Value = "Y"
				}
			});

			var hawbDataObject2 = SetupAirCargoHouse2("SB8965");
			consignorData = hawbDataObject2.AddOrgAddress(writeManager, org2, DocAddressType.ConsignorDocumentaryAddress);
			consigneeData = SetupOrganizationAddress2(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			consigneeData.Contact = "WENDY THE DESTROYER";
			consigneeData.Phone = "9685 5744";
			hawbDataObject2.OrganizationAddressCollection.Add(consigneeData);

			hawbDataObject2.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine2.SetUNDGCollection(() =>
			{
				var list = new List<UNDG>();
				var undg2 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
				undg2.UNDGCode = "1112";
				list.Add(undg2);
				return list;
			});
			hawbDataObject2.PackingLineCollection.Add(packingLine2);

			hawbDataObject2.SetAddInfoCollection(() => new List<AddInfo>
			{
				new AddInfo
				{
					Key = "IsGSTPrePaid",
					Value = "N"
				}
			});

			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject1);
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject2);
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);

			AssertNotNull("mawbBO", mawbBO);
			AssertEquals("mawbBO.ChildBills.Count", 2, mawbBO.ChildBills.Count);
			var hawbBO1 = mawbBO.ChildBills[0];
			var hawbBO2 = mawbBO.ChildBills[1];
			if (hawbBO2.CS_HAWB == "HB2343")
			{
				hawbBO1 = mawbBO.ChildBills[1];
				hawbBO2 = mawbBO.ChildBills[0];
			}

			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertCusHAWBContents(hawbBO1, "HB2343");
				AssertCusHAWBConsignor(hawbBO1, org1.OH_FullName, org1.MainAddress.OA_Address1, org1.MainAddress.OA_Address2, org1.MainAddress.OA_City, org1.MainAddress.OA_State,
					org1.MainAddress.OA_PostCode, org1.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2), "", org1.MainAddress.OA_Phone, org1.PK);
				AssertCusHAWBConsignee(hawbBO1, "In The Moment", "Unit 12, Level 3", "233 Here St", "ThereVille", "OfBliss", "1233", "AU", "BOB THE BUILDER", "3234 23432", ZGuid.Empty);
				AssertEquals("hawbBO1.UNDGs[0].Substance.DG_Code", "1111", hawbBO1.UNDGs[0].Substance.DG_Code);
				AssertEquals("Y", hawbBO1.CS_IsGSTPrePaid);

				AssertCusHAWBContents2(hawbBO2, "SB8965");
				AssertCusHAWBConsignor(hawbBO2, org2.OH_FullName, org2.MainAddress.OA_Address1, org2.MainAddress.OA_Address2, org2.MainAddress.OA_City, org2.MainAddress.OA_State,
					org2.MainAddress.OA_PostCode, org2.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2), "", org2.MainAddress.OA_Phone, org2.PK);
				AssertCusHAWBConsignee(hawbBO2, "Too Late To Apologise", "Unit 24, Level 10", "455 There St", "Big City", "Small State", "56845", "US", "WENDY THE DESTROYER", "9685 5744", ZGuid.Empty);
				AssertEquals("hawbBO2.UNDGs[0].Substance.DG_Code", "1112", hawbBO2.UNDGs[0].Substance.DG_Code);
				AssertEquals("N", hawbBO2.CS_IsGSTPrePaid);

				AssertMultilineASCIIEquals("logger.Logs", @"
No matching CusMAWB found, creating new CusMAWB.
Populating CusMAWB...
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Successfully matched organization with code 'WUFSHIJNB'.
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
No matching UNDGDataItem found, creating new UNDGDataItem.
Populating UNDGDataItem...
Added Consignment: (HAWB: HB2343) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Successfully matched organization with code 'CRAHOLSYD'.
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
No matching UNDGDataItem found, creating new UNDGDataItem.
Populating UNDGDataItem...
Added Consignment: (HAWB: SB8965) from UniversalShipment.
Added AirCargo Report (MAWB: MB2343) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: MB2343 Job#: X00001000) with 2 x UNDGDataItem, 2 x CusHAWB.
".Trim(), message.GetLogNoteText());
			});

			#endregion
		}

		public void TestImportGoodsDetails_FromSubShipment()
		{
			AssertImportGoodsDetails_FromSubShipment("MB2343A", null, new PackageType() { Code = "PK" });
			AssertImportGoodsDetails_FromSubShipment("MB2343B", null, null);
			AssertImportGoodsDetails_FromSubShipment("MB2343C", 1, null);
		}

		void AssertImportGoodsDetails_FromSubShipment(ZString mawb, ZLong? packQty, PackageType packageType)
		{
			var hawbData = SetupAirCargoHouse("HB2343");
			hawbData.GoodsDescription = "fish";
			hawbData.CountryOfSupply = new Country { Code = "AU" };
			hawbData.GoodsValue = 1001;
			hawbData.GoodsValueCurrency = new Currency { Code = "AUD" };
			hawbData.TotalNoOfPieces = 1002;
			hawbData.TotalNoOfPacksPackageType = new PackageType { Code = "BLC" };
			hawbData.TotalWeight = 1003;
			hawbData.TotalWeightUnit = new UnitOfWeight { Code = "KG" };
			var packingLineData = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GoodsDescription = "Is ignored because Pack Qty is empty",
				CountryOfOrigin = new Country() { Code = "NZ" },
				LinePrice = 2000m,
				LinePriceCurrency = new Currency() { Code = "NZD" },
				Weight = 200m,
				WeightUnit = new UnitOfWeight() { Code = "LP" },
				PackQty = packQty,
				PackType = packageType
			};
			packingLineData.SetUNDGCollection(() => new List<UNDG>(new[]
			{
				new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { UNDGCode = "1111" }
			}));
			hawbData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
			{
				packingLineData
			}));
			var mawbDataObject = SetupAirCargoMaster(mawb, ZString.Empty);
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			{
				hawbData
			});

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, mawb);
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);
			var hawbBO = mawbBO.ChildBills[0];
			AssertEquals("One house bill was imported.", 1, mawbBO.ChildBills.Count);
			AssertCusHAWBGoodsDetails(hawbBO, "FISH", "AU", 1001, "AUD", 1002, "BL", 1003, "KG", "");
		}

		public void TestImportGoodsDetails_FromPackingLines()
		{
			var hawbData = SetupAirCargoHouse("HB2343");
			hawbData.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					GoodsDescription = "fish",
					CountryOfOrigin = new Country { Code = "AU" },
					LinePrice = 1001,
					LinePriceCurrency = new Currency { Code = "AUD" },
					PackQty = 1002,
					PackType = new PackageType { Code = "BI" },
					Weight = 1003,
					WeightUnit = new UnitOfWeight { Code = "KG" },
					HarmonisedCode = "1004"
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					GoodsDescription = "chips",
					CountryOfOrigin = new Country { Code = "NZ" },
					LinePrice = 2001,
					LinePriceCurrency = new Currency { Code = "AUD" },
					PackQty = 2002,
					PackType = new PackageType { Code = "BG" },
					Weight = 2003,
					WeightUnit = new UnitOfWeight { Code = "G" },
					HarmonisedCode = "2004"
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					GoodsDescription = "cola",
					CountryOfOrigin = new Country { Code = "US" },
					LinePrice = 3001,
					LinePriceCurrency = new Currency { Code = "AUD" },
					PackQty = 3002,
					PackType = new PackageType { Code = "BX" },
					Weight = 3003,
					WeightUnit = new UnitOfWeight { Code = "LB" },
					HarmonisedCode = "3004"
				}
			});

			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			{
				hawbData
			});

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);
			var hawbBO = mawbBO.ChildBills[0];
			AssertEquals("One house bill was imported.", 1, mawbBO.ChildBills.Count);
			AssertEquals("Two CusHAWBItems were imported.", 2, hawbBO.CusHAWBItemsCollection.Count);
			AssertCusHAWBGoodsDetails(hawbBO, "FISH", "AU", 1001, "AUD", 1002, "BI", 1003, "KG", "1004");
			AssertCusHAWBItemsGoodsDetails(hawbBO.CusHAWBItemsCollection[0], "CHIPS", "NZ", 2001, "AUD", 2002, "BG", 2003, "G", "2004");
			AssertCusHAWBItemsGoodsDetails(hawbBO.CusHAWBItemsCollection[1], "COLA", "US", 3001, "AUD", 3002, "BX", 3003, "LB", "3004");
		}

		public void TestImportGoodsDetails_PackingLineShouldOverrideSubShipment()
		{
			var hawbData = SetupAirCargoHouse("HB2343");
			hawbData.GoodsDescription = "AU seafood";
			hawbData.CountryOfSupply = new Country { Code = "AU" };
			hawbData.GoodsValue = 1001;
			hawbData.GoodsValueCurrency = new Currency { Code = "AUD" };
			hawbData.TotalNoOfPieces = 1002;
			hawbData.TotalNoOfPacksPackageType = new PackageType { Code = "BI" };
			hawbData.TotalWeight = 1003;
			hawbData.TotalWeightUnit = new UnitOfWeight { Code = "KG" };
			hawbData.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					GoodsDescription = "NZ seafood",
					CountryOfOrigin = new Country { Code = "NZ" },
					LinePrice = 2001,
					LinePriceCurrency = new Currency { Code = "AUD" },
					PackQty = 2002,
					PackType = new PackageType { Code = "BG" },
					Weight = 2003,
					WeightUnit = new UnitOfWeight { Code = "G" },
					HarmonisedCode = "2004"
				}
			});

			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			{
				hawbData
			});

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);
			var hawbBO = mawbBO.ChildBills[0];
			AssertEquals("One house bill was imported.", 1, mawbBO.ChildBills.Count);
			AssertCusHAWBGoodsDetails(hawbBO, "NZ SEAFOOD", "NZ", 2001, "AUD", 2002, "BG", 2003, "G", "2004");
		}

		public void TestImportGoodsDetails_PackingLineShouldNotOverrideSubShipment_IfInvalid()
		{
			var hawbData = SetupAirCargoHouse("HB2343");
			hawbData.GoodsDescription = "AU seafood";
			hawbData.CountryOfSupply = new Country { Code = "AU" };
			hawbData.GoodsValue = 1001;
			hawbData.GoodsValueCurrency = new Currency { Code = "AUD" };
			hawbData.TotalNoOfPieces = 1002;
			hawbData.TotalNoOfPacksPackageType = new PackageType { Code = "BI" };
			hawbData.TotalWeight = 1003;
			hawbData.TotalWeightUnit = new UnitOfWeight { Code = "KG" };
			hawbData.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					GoodsDescription = "NZ seafood",
					CountryOfOrigin = new Country { Code = "NZ" },
					LinePrice = 2001,
					LinePriceCurrency = new Currency { Code = "AUD" },
					PackQty = null,
					PackType = null,
					Weight = 2003,
					WeightUnit = new UnitOfWeight { Code = "G" },
					HarmonisedCode = "2004"
				}
			});

			var mawbDataObject = SetupAirCargoMaster("MB2343", ZString.Empty);
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			{
				hawbData
			});

			var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
			var mawbBO = Factory.LoadTop1<CusMAWB>(mawbQuery);
			var hawbBO = mawbBO.ChildBills[0];
			AssertEquals("One house bill was imported.", 1, mawbBO.ChildBills.Count);
			AssertCusHAWBGoodsDetails(hawbBO, "AU SEAFOOD", "AU", 1001, "AUD", 1002, "BI", 1003, "KG", null);
		}

		protected Shipment SetupAirCargoHouse(ZString? wayBillNumber)
		{
			return SetupAirCargoHouse(wayBillNumber, new UNLOCO() { Code = AirForeignPort2.RL_Code }, new UNLOCO() { Code = AirLocalPort3.RL_Code },
				1500.60m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, 350, "GOODS FOR TESTING", 1304.50m, LocalCurrency,
				new Country() { Code = Core.Constants.CountryCodes.NewZealand }, null,
				new UNLOCO() { Code = AirForeignPort1.RL_Code }, new UNLOCO() { Code = AirLocalPort1.RL_Code });
		}

		protected Shipment SetupAirCargoHouse2(ZString? wayBillNumber)
		{
			return SetupAirCargoHouse(wayBillNumber, new UNLOCO() { Code = AirForeignPort1.RL_Code }, new UNLOCO() { Code = AirLocalPort1.RL_Code },
				850.60m, new UnitOfWeight() { Code = Core.Constants.Weight.Pounds }, 350, "GOODS FOR TESTING 2", 1880.50m, ForeignCurrency,
				new Country() { Code = Core.Constants.CountryCodes.Australia }, new PackageType() { Code = "XJ" },
				new UNLOCO() { Code = AirForeignPort2.RL_Code }, new UNLOCO() { Code = AirLocalPort2.RL_Code });
		}

		protected Shipment SetupAirCargoHouse(ZString? wayBillNumber, UNLOCO portOfOrigin, UNLOCO portOfDestination, ZDecimal? weight, UnitOfWeight weightUQ, ZInt? piecesManifested, ZString? goodsDescription, ZDecimal? goodsValue, Currency goodsValueCurrency,
			Country goodsOrigin, PackageType packType, UNLOCO loadPort, UNLOCO dischargePort)
		{
			var result = SetupAirCargoHouse(wayBillNumber, ZBool.False, null, portOfOrigin, portOfDestination, weight, weightUQ, piecesManifested, goodsDescription, goodsValue, goodsValueCurrency, null);
			result.CountryOfSupply = goodsOrigin;
			result.TotalNoOfPacksPackageType = packType;
			result.PortOfLoading = loadPort;
			result.PortOfDischarge = dischargePort;
			result.SetCustomizedFieldCollection(() => new List<CustomizedField>
			{
				CustomizedField.New("CustomField1", (ZString)"CustomField1"),
				CustomizedField.New("CustomField2PART1", (ZString)"CustomField2PART1"),
				CustomizedField.New("CustomField2PART2", (ZString)"CustomField2PART2")
			});
			return result;
		}

		protected void AssertCusHAWBContents(CusHAWB hawbBO, ZString wayBillNumber)
		{
			AssertCusHAWBContents(hawbBO, wayBillNumber, AirForeignPort2.RL_Code, AirLocalPort3.RL_Code,
				1500.60m, Core.Constants.Weight.Kilograms, 350, "GOODS FOR TESTING", 1304.50m, Enterprise.Customs.NZ.Business.Declaration.JobDeclaration.LocalCurrencyConstantCode,
				Core.Constants.CountryCodes.NewZealand, "", AirForeignPort1.RL_Code, AirLocalPort1.RL_Code,
				"CustomField1", "CustomField2PART1", "CustomField2PART2");
		}

		protected void AssertCusHAWBContents2(CusHAWB hawbBO, ZString wayBillNumber)
		{
			AssertCusHAWBContents(hawbBO, wayBillNumber, AirForeignPort1.RL_Code, AirLocalPort1.RL_Code,
				850.60m, Core.Constants.Weight.Pounds, 350, "GOODS FOR TESTING 2", 1880.50m, ForeignCurrencyBO.RX_Code,
				Core.Constants.CountryCodes.Australia, "XJ", AirForeignPort2.RL_Code, AirLocalPort2.RL_Code,
				"CustomField1", "CustomField2PART1", "CustomField2PART2");
		}

		protected void AssertCusHAWBContents(CusHAWB hawbBO, ZString wayBillNumber, ZString origin, ZString destination, ZDecimal weight, ZString weightUQ, ZShort piecesManifested, ZString goodsDescription, ZDecimal goodsValue, ZString goodsCurrency, ZString goodsOrigin,
			ZString packType, ZString loadPort, ZString dischargePort, ZString customsField1, ZString customsField2Part1, ZString customsField2Part2)
		{
			AssertCusHAWBContents(hawbBO, wayBillNumber, ZBool.False, ZString.Empty, origin, destination, weight, weightUQ, piecesManifested, goodsDescription, goodsValue, goodsCurrency, ZString.Empty);
			AssertEquals("hawbBO.CS_RN_NKGoodsOrigin", goodsOrigin, hawbBO.CS_RN_NKGoodsOrigin);
			AssertEquals("hawbBO.CS_PackType", packType, hawbBO.CS_PackType);
			AssertEquals("hawbBO.CS_RL_NKLoadPort", loadPort, hawbBO.CS_RL_NKLoadPort);
			AssertEquals("hawbBO.CS_RL_NKDischargePort", dischargePort, hawbBO.CS_RL_NKDischargePort);
			AssertEquals("hawbBO.CustomsFields1", customsField1, hawbBO.GetUserDefinedValue<ZString>("CustomField1"));
			AssertEquals("hawbBO.CustomsFields2Part1", customsField2Part1, hawbBO.GetUserDefinedValue<ZString>("CustomField2PART1"));
			AssertEquals("hawbBO.CustomsFields2Part2", customsField2Part2, hawbBO.GetUserDefinedValue<ZString>("CustomField2PART2"));
		}

		void AssertCusHAWBGoodsDetails(CusHAWB hawbBO, ZString goodsDescription, ZString goodsOrigin, ZDecimal goodsValue, ZString goodsCurrency, ZShort piecesManifested, ZString packType, ZDecimal weight, ZString weightUQ, ZString harmonisedTariffNums)
		{
			AssertEquals("CS_GoodsDescription", goodsDescription, hawbBO.CS_GoodsDescription);
			AssertEquals("CS_RN_NKGoodsOrigin", goodsOrigin, hawbBO.CS_RN_NKGoodsOrigin);
			AssertEquals("CS_GoodsValue", goodsValue, hawbBO.CS_GoodsValue);
			AssertEquals("CS_RX_NKGoodsCurrency", goodsCurrency, hawbBO.CS_RX_NKGoodsCurrency);
			AssertEquals("CS_PiecesManifested", piecesManifested, hawbBO.CS_PiecesManifested);
			AssertEquals("CS_PackType", packType, hawbBO.CS_PackType);
			AssertEquals("CS_Weight", weight, hawbBO.CS_Weight);
			AssertEquals("CS_WeightUQ", weightUQ, hawbBO.CS_WeightUQ);
			AssertEquals("CS_HarmonisedTariffNums", harmonisedTariffNums, hawbBO.CS_HarmonisedTariffNums);
		}

		void AssertCusHAWBItemsGoodsDetails(CusHAWBItems cusHawbItemsBO, ZString goodsDescription, ZString goodsOrigin, ZDecimal goodsValue, ZString goodsCurrency, ZShort piecesManifested, ZString packType, ZDecimal weight, ZString weightUQ, ZString harmonisedTariffNums)
		{
			AssertEquals("CHI_GoodsDescription", goodsDescription, cusHawbItemsBO.CHI_GoodsDescription);
			AssertEquals("CHI_RN_NKGoodsOrigin", goodsOrigin, cusHawbItemsBO.CHI_RN_NKGoodsOrigin);
			AssertEquals("CHI_GoodsValue", goodsValue, cusHawbItemsBO.CHI_GoodsValue);
			AssertEquals("CHI_RX_NKGoodsCurrency", goodsCurrency, cusHawbItemsBO.CHI_RX_NKGoodsCurrency);
			AssertEquals("CHI_PieceCount", piecesManifested, cusHawbItemsBO.CHI_PieceCount);
			AssertEquals("CHI_PackType", packType, cusHawbItemsBO.CHI_PackType);
			AssertEquals("CHI_Weight", weight, cusHawbItemsBO.CHI_Weight);
			AssertEquals("CHI_WeightUQ", weightUQ, cusHawbItemsBO.CHI_WeightUQ);
			AssertEquals("CHI_HarmonisedTariffNums", harmonisedTariffNums, cusHawbItemsBO.CHI_HarmonisedTariffNums);
		}

		[TestDate(2019, 11, 5)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPassHVLVDataToCustoms()
		{
			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\AirCargo ECI\TestFiles\Pass HVLV data to customs.xml"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawb = new CusMAWB.Loader(Factory.BOFactory).FindFirstMatchingMAWB("04420191031");
			AssertNotNull(mawb);
			AssertEquals(2, mawb.ChildBills.Count);

			var hawb1 = mawb.ChildBills[0];
			AssertEquals("APPLEHOUSE", hawb1.CS_HAWB);
			AssertEquals("CHILD APPLE 1", hawb1.CS_GoodsDescription);
			AssertEquals(11.11m, hawb1.CS_GoodsValue);
			AssertEquals("AUD", hawb1.CS_RX_NKGoodsCurrency);
			AssertEquals("BG", hawb1.CS_PackType);
			AssertEquals(1, hawb1.CusHAWBItemsCollection.Count);
			AssertEquals("CHILD APPLE 2", hawb1.CusHAWBItemsCollection[0].CHI_GoodsDescription);

			var hawb2 = mawb.ChildBills[1];
			AssertEquals("BANANAHOUSE", hawb2.CS_HAWB);
			AssertEquals("CHILD BANANA 1", hawb2.CS_GoodsDescription);
			AssertEquals(22.22m, hawb2.CS_GoodsValue);
			AssertEquals("GBP", hawb2.CS_RX_NKGoodsCurrency);
			AssertEquals("BX", hawb2.CS_PackType);
			AssertEquals(1, hawb2.CusHAWBItemsCollection.Count);
			AssertEquals("CHILD BANANA 2", hawb2.CusHAWBItemsCollection[0].CHI_GoodsDescription);
		}

		#region Overrides

		protected override void AssertCusHAWBConsignee(Customs.Business.CusHAWB hawbBO, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode, ZString contactName, ZString phone, ZGuid organisationPK)
		{
			name = name.ToUpper();
			address1 = address1.ToUpper();
			address2 = address2.ToUpper();
			city = city.ToUpper();
			state = state.ToUpper();
			postCode = postCode.ToUpper();
			countryCode = countryCode.ToUpper();
			contactName = contactName.ToUpper();
			phone = phone.ToUpper();
			base.AssertCusHAWBConsignee(hawbBO, name, address1, address2, city, state, postCode, countryCode, contactName, phone, organisationPK);
		}

		protected override void AssertCusHAWBConsignor(Customs.Business.CusHAWB hawbBO, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode, ZString contactName, ZString phone, ZGuid organisationPK)
		{
			name = name.ToUpper();
			address1 = address1.ToUpper();
			address2 = address2.ToUpper();
			city = city.ToUpper();
			state = state.ToUpper();
			postCode = postCode.ToUpper();
			countryCode = countryCode.ToUpper();
			contactName = contactName.ToUpper();
			phone = phone.ToUpper();
			base.AssertCusHAWBConsignor(hawbBO, name, address1, address2, city, state, postCode, countryCode, contactName, phone, organisationPK);
		}

		protected override void AssertCusHAWBContents(Customs.Business.CusHAWB hawbBO, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse, ZString origin, ZString destination, ZDecimal weight, ZString weightUQ, ZShort piecesManifested, ZString goodsDescription, ZDecimal goodsValue, ZString goodsCurrency, ZString responsiblePartyID)
		{
			wayBillNumber = wayBillNumber.ToUpper();
			masterHouse = masterHouse.ToUpper();
			weightUQ = weightUQ.ToUpper();
			goodsDescription = goodsDescription.ToUpper();
			goodsCurrency = goodsCurrency.ToUpper();
			responsiblePartyID = responsiblePartyID.ToUpper();
			base.AssertCusHAWBContents(hawbBO, wayBillNumber, isMasterHouse, masterHouse, origin, destination, weight, weightUQ, piecesManifested, goodsDescription, goodsValue, goodsCurrency, responsiblePartyID);
		}

		#endregion
	}
}

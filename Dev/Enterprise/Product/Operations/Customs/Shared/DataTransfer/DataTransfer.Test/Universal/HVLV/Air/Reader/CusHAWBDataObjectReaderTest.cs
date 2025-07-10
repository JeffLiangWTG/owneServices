using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest.Testing
{
	sealed class CusHAWBDataObjectReaderTest : AirManifestDataObjectReaderTestHelper
	{
		public void TestStringValueCharacterCaseIsUpper()
		{
			var characterCasingProperty = typeof(CusHAWBDataObjectReader).GetProperty(
				"StringValueCharacterCase",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);

			var mawb = Factory.New<CusMAWB>();
			var mawbHelper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);

			var reader = new CusHAWBDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), null,
				logger, mawbHelper, mawb, null, true);
			var characterCasingValue = characterCasingProperty.GetValue(reader);

			AssertEquals(CharacterCase.Upper, characterCasingValue);
		}

		public void TestSkipFindingExisingHousebillIfCreatingNewFromHVLV()
		{
			var hawbDataObject = SetupAirCargoHouse("hb2343", ZBool.False, null);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "hb2343";
			var mawbHelper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var reader = new CusHAWBDataObjectReader(hawbDataObject, null, logger, mawbHelper, mawb, null, true) as ITopLevelDataObjectReader;
			Assert("precondition", !mawb.IsInDatabase);
			AssertNull("Should not find anything", reader.GetExistingBusinessObject());
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRulesIsSealed()
		{
			var type = typeof(CusHAWBDataObjectReader);
			var method = type.GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			Assert(@"GetExistingBusinessObjectUsingModuleSpecificBusinessRules should be sealed.
If it is needed to be overridden, please make sure Finding existing housebill is skipped when XUS is from HVLV Shipment and masterbill is new.",
method.IsFinal);
		}

		public void TestCS_IsHVLV_True()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var mawbDataObject = SetupAirCargoMaster("MB2343", null);
				mawbDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo
				{
					EventType = new CodeDescriptionPair { Code = Events.HVLVReadyCode }
				});
				mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>
				{
					SetupAirCargoHouse("MH1", ZBool.True, null),
					SetupAirCargoHouse("MH2", ZBool.True, null)
				});

				var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
				var mawbBO = new BusinessObjectFactory().LoadTop1<CusMAWB>(mawbQuery);

				CombineAssertions(() =>
				{
					AssertNotNull("mawbBO", mawbBO);
					var masterHouseBO1 = mawbBO.ChildBills.OfType<CusHAWB>().First(x => x.CS_HAWB == "MH1");
					AssertEquals("MH1.CS_IsHVLV", true, masterHouseBO1.CS_IsHVLV);

					var masterHouseBO2 = mawbBO.ChildBills.OfType<CusHAWB>().First(x => x.CS_HAWB == "MH2");
					AssertEquals("MH2.CS_IsHVLV", true, masterHouseBO2.CS_IsHVLV);

					AssertMultilineASCIIEquals("logger.Logs", @"
Added Air Cargo House (HAWB: MH1) from UniversalShipment.
Added Air Cargo House (HAWB: MH2) from UniversalShipment.
Added AirCargo Report (MAWB: MB2-343) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: MB2-343) with 2 x CusHAWB.
				".Trim(), serviceTaskLog.ToString());
				});

				mawbDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo
				{
					EventType = new CodeDescriptionPair { Code = "" }
				});
				message = GetQueuedUniversalShipmentMessage(mawbDataObject);
				serviceTaskLog = new ServiceTaskLogForTesting();
				manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
				mawbBO = new BusinessObjectFactory().LoadTop1<CusMAWB>(mawbQuery);

				CombineAssertions(() =>
				{
					AssertNotNull("mawbBO", mawbBO);
					var masterHouseBO1 = mawbBO.ChildBills.OfType<CusHAWB>().First(x => x.CS_HAWB == "MH1");
					AssertEquals("MH1.CS_IsHVLV", true, masterHouseBO1.CS_IsHVLV);
					AssertEquals("MH1.CS_RL_NKOrigin", AirForeignPort2.RL_Code, masterHouseBO1.CS_RL_NKOrigin);
					AssertEquals("MH1.CS_RL_NKDestination", AirLocalPort3.RL_Code, masterHouseBO1.CS_RL_NKDestination);

					var masterHouseBO2 = mawbBO.ChildBills.OfType<CusHAWB>().First(x => x.CS_HAWB == "MH2");
					AssertEquals("MH2.CS_IsHVLV", true, masterHouseBO2.CS_IsHVLV);
					AssertEquals("MH2.CS_RL_NKOrigin", AirForeignPort2.RL_Code, masterHouseBO2.CS_RL_NKOrigin);
					AssertEquals("MH2.CS_RL_NKDestination", AirLocalPort3.RL_Code, masterHouseBO2.CS_RL_NKDestination);

					AssertMultilineASCIIEquals("logger.Logs", @"
Updated Air Cargo House (HAWB: MH1) from UniversalShipment.
Updated Air Cargo House (HAWB: MH2) from UniversalShipment.
Updated AirCargo Report (MAWB: MB2-343) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: MB2-343) with 2 x CusHAWB.
				".Trim(), serviceTaskLog.ToString());
				});
			}
		}

		public void TestCS_IsHVLV_False()
		{
			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.Code)) // need to clear user context and cached values to emulate a service task.
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var mawbDataObject = SetupAirCargoMaster("MB2343", null);
				mawbDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo
				{
					EventType = new CodeDescriptionPair { Code = "" }
				});
				mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>
				{
					SetupAirCargoHouse("MH1", ZBool.True, null),
					SetupAirCargoHouse("MH2", ZBool.True, null)
				});

				var message = GetQueuedUniversalShipmentMessage(mawbDataObject);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
				var mawbBO = new BusinessObjectFactory().LoadTop1<CusMAWB>(mawbQuery);

				CombineAssertions(() =>
				{
					AssertNotNull("mawbBO", mawbBO);
					var masterHouseBO1 = mawbBO.ChildBills.OfType<CusHAWB>().First(x => x.CS_HAWB == "MH1");
					AssertEquals("MH1.CS_IsHVLV", false, masterHouseBO1.CS_IsHVLV);
					AssertEquals("MH1.CS_RL_NKOrigin", AirForeignPort2.RL_Code, masterHouseBO1.CS_RL_NKOrigin);
					AssertEquals("MH1.CS_RL_NKDestination", AirLocalPort3.RL_Code, masterHouseBO1.CS_RL_NKDestination);

					var masterHouseBO2 = mawbBO.ChildBills.OfType<CusHAWB>().First(x => x.CS_HAWB == "MH2");
					AssertEquals("MH2.CS_IsHVLV", false, masterHouseBO2.CS_IsHVLV);
					AssertEquals("MH2.CS_RL_NKOrigin", AirForeignPort2.RL_Code, masterHouseBO2.CS_RL_NKOrigin);
					AssertEquals("MH2.CS_RL_NKDestination", AirLocalPort3.RL_Code, masterHouseBO2.CS_RL_NKDestination);

					AssertMultilineASCIIEquals("logger.Logs", @"
Added Air Cargo House (HAWB: MH1) from UniversalShipment.
Added Air Cargo House (HAWB: MH2) from UniversalShipment.
Added AirCargo Report (MAWB: MB2-343) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: MB2-343) with 2 x CusHAWB.
				".Trim(), serviceTaskLog.ToString());
				});

				mawbDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo
				{
					EventType = new CodeDescriptionPair { Code = Events.HVLVReadyCode }
				});

				message = GetQueuedUniversalShipmentMessage(mawbDataObject);
				serviceTaskLog = new ServiceTaskLogForTesting();
				manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				mawbQuery = new ZQuery(CusMAWBSchema.CM_MAWB, "MB2343");
				mawbBO = new BusinessObjectFactory().LoadTop1<CusMAWB>(mawbQuery);

				CombineAssertions(() =>
				{
					AssertNotNull("mawbBO", mawbBO);
					var masterHouseBO1 = mawbBO.ChildBills.OfType<CusHAWB>().First(x => x.CS_HAWB == "MH1");
					AssertEquals("MH1.CS_IsHVLV", true, masterHouseBO1.CS_IsHVLV);

					var masterHouseBO2 = mawbBO.ChildBills.OfType<CusHAWB>().First(x => x.CS_HAWB == "MH2");
					AssertEquals("MH2.CS_IsHVLV", true, masterHouseBO2.CS_IsHVLV);

					AssertMultilineASCIIEquals("logger.Logs", @"
Updated Air Cargo House (HAWB: MH1) from UniversalShipment.
Updated Air Cargo House (HAWB: MH2) from UniversalShipment.
Updated AirCargo Report (MAWB: MB2-343) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: MB2-343) with 2 x CusHAWB.
				".Trim(), serviceTaskLog.ToString());
				});
			}
		}

		public void TestCS_IsHVLV_ImportHVLVShipperConsolidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var mawbThatShouldBeIgnored = Factory.New<CusMAWB>();
				mawbThatShouldBeIgnored.CM_MAWB = "08123232322";
				Factory.SaveForTesting();
				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalHVLVAirTestFiles("UniversalShipment For HVLV Shipper Consolidation.xml")).Replace("<Code>HLR</Code>", "<Code></Code>"));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					var query = new ZQuery(CusMAWBSchema.CM_MAWB, "08123232322");
					query.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, "HB1");
					var mawbs = Factory.Load<CusMAWB>(query);
					AssertEquals("mawbs.Length", 1, mawbs.Length);
					var mawb = mawbs[0];
					AssertNotEquals("HVLV Shipper Consolidation should be direct match", mawbThatShouldBeIgnored, mawb);
					var hawbs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CM, mawb.PK));
					AssertEquals("hawbs.Length", 9, hawbs.Length);

					var hawbs1 = hawbs.Where(h => h.CS_HAWB == "1").ToArray();
					var hawbs2 = hawbs.Where(h => h.CS_HAWB == "6").ToArray();

					AssertEquals(1, hawbs1.Length);
					AssertEquals(1, hawbs2.Length);

					AssertEquals("hawb1.CS_IsHVLV", true, hawbs1[0].CS_IsHVLV);
					AssertEquals("hawb2.CS_IsHVLV", true, hawbs2[0].CS_IsHVLV);
				});
			}
		}

		public void TestMatchHAWBIgnoringCase()
		{
			var hawbDataObject = SetupAirCargoHouse("hb2343", ZBool.False, null);
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "hb2343";
			var mawbHelper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			var reader = new CusHAWBDataObjectReader(hawbDataObject, null, logger, mawbHelper, mawb, null, false);
			AssertEquals(hawb, reader.ReadIntoBusinessObject());
		}

		public void TestImportingCusHAWB_HandleUnmatchOrganisation()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
			var hawbDataObject = SetupAirCargoHouse("HB2343", ZBool.False, null);
			var consignorData = SetupOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress));
			consignorData.Contact = "BOB THE BUILDER";
			consignorData.Phone = "3234 23432";
			var consigneeData = SetupOrganizationAddress2(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			consigneeData.Contact = "WENDY THE DESTROYER";
			consigneeData.Phone = "9685 5744";
			hawbDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] { consignorData, consigneeData }));
			var mawb = Factory.New<CusMAWB>();
			var mawbHelper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Eritrea);
			var reader = new CusHAWBDataObjectReader(hawbDataObject, null, logger, mawbHelper, mawb, null, false);
			var hawbBO = reader.ReadIntoBusinessObject();

			AssertNotNull(hawbBO);

			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertCusHAWBContents(hawbBO, "HB2343", ZBool.False, null);
				AssertEquals("CS_CM", mawb.PK, hawbBO.CS_CM);
				AssertEquals("CS_CS_MasterHouseBill", ZGuid.Empty, hawbBO.CS_CS_MasterHouseBill);
				AssertCusHAWBConsignor(hawbBO, "IN THE MOMENT", "UNIT 12, LEVEL 3", "233 HERE ST", "THEREVILLE", "OFBLISS", "1233", "AU", "BOB THE BUILDER", "3234 23432", ZGuid.Empty);
				AssertCusHAWBConsignee(hawbBO, "TOO LATE TO APOLOGISE", "UNIT 24, LEVEL 10", "455 THERE ST", "BIG CITY", "SMALL STATE", "56845", "US", "WENDY THE DESTROYER", "9685 5744", ZGuid.Empty);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
Information - Added House Bill from UniversalShipment.
				".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestImportingCusHAWBData_ImportGoodsLocationFromMAWB()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			var mawbGoodsLocation = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress;
			Factory.SaveForTesting();

			var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");
			var mawbLocationAddress = SetupOrganizationAddress(nameof(DocAddressType.GoodsLocation), mawbGoodsLocation.OA_Code, mawbGoodsLocation.Header.OH_Code, mawbGoodsLocation.Header.OH_FullName, mawbGoodsLocation.OA_Address1, mawbGoodsLocation.OA_Address2,
				mawbGoodsLocation.OA_City, mawbGoodsLocation.OA_State, mawbGoodsLocation.OA_PostCode, UNLOCO.New(mawbGoodsLocation.EffectiveRelatedPortCode), Country.New(mawbGoodsLocation.RelatedCountry), mawbGoodsLocation.Header.Contacts[0].OC_ContactName, mawbGoodsLocation.OA_Phone);
			mawbDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			mawbDataObject.OrganizationAddressCollection.Add(mawbLocationAddress);

			var hawbDataObject = SetupAirCargoHouse("HB2343", ZBool.True, null);

			var mawb = Factory.New<CusMAWB>();
			var mawbHelper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.NewZealand);
			var reader = new CusHAWBDataObjectReader(hawbDataObject, mawbDataObject, logger, mawbHelper, mawb, null, false);
			var hawbBO = reader.ReadIntoBusinessObject();

			AssertNotNull(hawbBO);
			AssertEquals("hawbBO.CS_OA_GoodsLocation", mawbGoodsLocation.PK, hawbBO.CS_OA_GoodsLocation);
		}

		public void TestImportingCusHAWBData()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var goodLocation = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress;
			Factory.SaveForTesting();

			var hawbDataObject = SetupAirCargoHouse("HB2343", ZBool.True, null);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var receivingAgent = hawbDataObject.AddOrgAddress(writeManager, org1, DocAddressType.ReceivingForwarderAddress);
			var sendingAgent = SetupOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress));
			sendingAgent.Contact = "BOB THE BUILDER";
			sendingAgent.Phone = "3234 23432";
			hawbDataObject.OrganizationAddressCollection.Add(sendingAgent);

			var goodLocationAddress = SetupOrganizationAddress(nameof(DocAddressType.GoodsLocation), goodLocation.OA_Code, goodLocation.Header.OH_Code, goodLocation.Header.OH_FullName, goodLocation.OA_Address1, goodLocation.OA_Address2,
				goodLocation.OA_City, goodLocation.OA_State, goodLocation.OA_PostCode, UNLOCO.New(goodLocation.EffectiveRelatedPortCode), Country.New(goodLocation.RelatedCountry), goodLocation.Header.Contacts[0].OC_ContactName, goodLocation.OA_Phone);
			hawbDataObject.OrganizationAddressCollection.Add(goodLocationAddress);

			hawbDataObject.TotalNoOfPacksPackageType = new PackageType() { Code = "AE" };

			var subHawbDataObject = SetupAirCargoHouse("SB8965", ZBool.False, "HB2343");
			var consignorData = subHawbDataObject.AddOrgAddress(writeManager, org2, DocAddressType.ConsignorDocumentaryAddress);
			var consigneeData = SetupOrganizationAddress2(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			consigneeData.Contact = "WENDY THE DESTROYER";
			consigneeData.Phone = "9685 5744";
			subHawbDataObject.OrganizationAddressCollection.Add(consigneeData);
			subHawbDataObject.VendorIdentifier = "AMAZON";

			hawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			hawbDataObject.SubShipmentCollection.Add(subHawbDataObject);
			var mawb = Factory.New<CusMAWB>();
			var mawbHelper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Eritrea);
			var reader = new CusHAWBDataObjectReader(hawbDataObject, null, logger, mawbHelper, mawb, null, false);
			var hawbBO = reader.ReadIntoBusinessObject();

			AssertNotNull(hawbBO);

			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertCusHAWBContents(hawbBO, "HB2343", ZBool.True, null);
				AssertEquals("CS_CM", mawb.PK, hawbBO.CS_CM);
				AssertEquals("CS_CS_MasterHouseBill", ZGuid.Empty, hawbBO.CS_CS_MasterHouseBill);
				AssertCusHAWBConsignee(hawbBO, org1.OH_FullName, org1.MainAddress.OA_Address1, org1.MainAddress.OA_Address2, org1.MainAddress.OA_City, org1.MainAddress.OA_State,
					org1.MainAddress.OA_PostCode, org1.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2), "", org1.MainAddress.OA_Phone, org1.PK);
				AssertCusHAWBConsignor(hawbBO, "IN THE MOMENT", "UNIT 12, LEVEL 3", "233 HERE ST", "THEREVILLE", "OFBLISS", "1233", "AU", "BOB THE BUILDER", "3234 23432", ZGuid.Empty);
				AssertEquals("hawbBO.CS_OA_GoodsLocation", goodLocation.PK, hawbBO.CS_OA_GoodsLocation);
				AssertEquals("hawbBO.CS_PackType", "AE", hawbBO.CS_PackType);

				var subHawbBOs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CS_MasterHouseBill, hawbBO.PK));
				AssertEquals("subHawbBOs.Length", 1, subHawbBOs.Length);
				var subHawbBO = subHawbBOs[0];
				AssertCusHAWBContents(subHawbBO, "SB8965", ZBool.False, "HB2343");
				AssertEquals("CS_CM", mawb.PK, subHawbBO.CS_CM);
				AssertCusHAWBConsignor(subHawbBO, org2.OH_FullName, org2.MainAddress.OA_Address1, org2.MainAddress.OA_Address2, org2.MainAddress.OA_City, org2.MainAddress.OA_State,
					org2.MainAddress.OA_PostCode, org2.MainAddress.EffectiveRelatedPortCode.RL_Code.Left(2), "", org2.MainAddress.OA_Phone, org2.PK);
				AssertCusHAWBConsignee(subHawbBO, "TOO LATE TO APOLOGISE", "UNIT 24, LEVEL 10", "455 THERE ST", "BIG CITY", "SMALL STATE", "56845", "US", "WENDY THE DESTROYER", "9685 5744", ZGuid.Empty);
				AssertEquals("CS_VendorIdentifier", "AMAZON", subHawbBO.CS_VendorIdentifier);
				subHawbBOs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CS_MasterHouseBill, subHawbBO.PK));
				AssertEquals("subHawbBOs.Length", 0, subHawbBOs.Length);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Matching 'GoodsLocation':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Warning - Matching 'SendingForwarderAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Matching 'ReceivingForwarderAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Warning - Matching 'ConsigneeDocumentaryAddress':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
Information - Added House Bill from UniversalShipment.
Information - Added House Bill from UniversalShipment.
				".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestShouldNotImportWhenThereAreMultipleSubHousesForSingleHAWB()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
			var hawbDataObject = SetupAirCargoHouse("HB2343", ZBool.True, null);
			hawbDataObject.GoodsDescription = "HELLO";
			var subHawbDataObject1 = SetupAirCargoHouse("SB8965", ZBool.False, null);
			subHawbDataObject1.GoodsDescription = "HI";

			hawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			hawbDataObject.SubShipmentCollection.Add(subHawbDataObject1);
			var mawb = Factory.New<CusMAWB>();
			var mawbHelper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Eritrea);
			var reader = new CusHAWBDataObjectReader(hawbDataObject, null, logger, mawbHelper, mawb, null, true);
			var hawbBO = reader.ReadIntoBusinessObject();
			mawb.ChildBills.Load();
			AssertEquals("hawbBO.CS_GoodsDescription", "HELLO", hawbBO.CS_GoodsDescription);
			var subHawbBOs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CS_MasterHouseBill, hawbBO.PK));
			AssertEquals("subHawbBOs.Length", 1, subHawbBOs.Length);
			var subHawbBO = subHawbBOs[0];
			AssertEquals("subHawbBO.CS_GoodsDescription", "HI", subHawbBO.CS_GoodsDescription);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Added House Bill from UniversalShipment.
Information - Added House Bill from UniversalShipment.
			".Trim(), logger.Logs);

			hawbBO.CS_GoodsDescription = "BYE";
			subHawbBO.CS_GoodsDescription = "CIAO";
			var subHawbDataObject2 = SetupAirCargoHouse("SB6359", ZBool.False, null);
			subHawbDataObject2.GoodsDescription = "YO";
			hawbDataObject.SubShipmentCollection.Add(subHawbDataObject2);
			logger.ClearLogs();
			reader = new CusHAWBDataObjectReader(hawbDataObject, null, logger, mawbHelper, mawb, null, false, true);
			var hawbBO2 = reader.ReadIntoBusinessObject();
			AssertEquals(hawbBO, hawbBO2);
			AssertEquals("hawbBO.CS_GoodsDescription", "BYE", hawbBO.CS_GoodsDescription);
			AssertEquals("subHawbBO.CS_GoodsDescription", "CIAO", subHawbBO.CS_GoodsDescription);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusHAWB.
Error - Cannot populate CusHAWB because:
Only one Sub Bill (SubShipmentCollection element) per House Bill is allowed for 'AirManifestLine' importation.
			".Trim(), logger.Logs);

			reader = new CusHAWBDataObjectReader(hawbDataObject, null, logger, mawbHelper, mawb, null, false, false);
			hawbBO2 = reader.ReadIntoBusinessObject();
			AssertEquals(hawbBO, hawbBO2);
			AssertEquals("hawbBO.CS_GoodsDescription", "HELLO", hawbBO.CS_GoodsDescription);
			subHawbBOs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CS_MasterHouseBill, hawbBO.PK));
			AssertEquals("subHawbBOs.Length", 2, subHawbBOs.Length);
			var subHawbBO1 = subHawbBOs[0];
			var subHawbBO2 = subHawbBOs[1];
			if (subHawbBO2 == subHawbBO)
			{
				subHawbBO1 = subHawbBOs[1];
				subHawbBO2 = subHawbBOs[0];
			}
			else
			{
				AssertEquals(subHawbBO, subHawbBO1);
			}
			AssertEquals("subHawbBO1.CS_GoodsDescription", "HI", subHawbBO1.CS_GoodsDescription);
			AssertEquals("subHawbBO2.CS_GoodsDescription", "YO", subHawbBO2.CS_GoodsDescription);
		}

		public void TestStringValueCharacterCaseIsUpper_CusMAWBDataObjectReader()
		{
			var characterCasingProperty = typeof(CusMAWBDataObjectReader).GetProperty(
				"StringValueCharacterCase",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);

			var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");

			var reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory, "ABC");
			var characterCasingValue = characterCasingProperty.GetValue(reader);

			AssertEquals(CharacterCase.Upper, characterCasingValue);
		}

		public void TestDontMatchCusMAWBWithEmptyMasterHouseBill()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var mawbShouldNotMatch = Factory.New<CusMAWB>();
				mawbShouldNotMatch.CM_MAWB = "12555550810";
				mawbShouldNotMatch.CM_ApplicationCode = "CMR";
				Factory.SaveForTesting();

				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalHVLVAirTestFiles("HVLVShipment2.xml")));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				mawbShouldNotMatch.Reload();
				AssertEquals("this MAWB should not be matched", ZString.Empty, mawbShouldNotMatch.CM_MasterHouseBill);

				var query = new ZQuery(CusMAWBSchema.CM_MAWB, "12555550810");
				query.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, "H0810B");
				var mawbs = Factory.Load<CusMAWB>(query);
				AssertEquals("mawbs.Length", 1, mawbs.Length);
			}
		}

		public void TestImportHVLVShipperConsolidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var mawbThatShouldBeIgnored = Factory.New<CusMAWB>();
				mawbThatShouldBeIgnored.CM_MAWB = "08123232322";
				Factory.SaveForTesting();
				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalHVLVAirTestFiles("UniversalShipment For HVLV Shipper Consolidation.xml")));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
Added Air Cargo House (HAWB: 1) from UniversalShipment.
Added Air Cargo House (HAWB: 2) from UniversalShipment.
Added Air Cargo House (HAWB: 3) from UniversalShipment.
Added Air Cargo House (HAWB: 4) from UniversalShipment.
Added Air Cargo House (HAWB: 8) from UniversalShipment.
Added Air Cargo House (HAWB: 9) from UniversalShipment.
Added Air Cargo House (HAWB: 5) from UniversalShipment.
Added Air Cargo House (HAWB: 6) from UniversalShipment.
Added Air Cargo House (HAWB: 7) from UniversalShipment.
Added AirCargo Report (MAWB: 081-23232322 MHB: HB1) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: 081-23232322 MHB: HB1) with 9 x CusHAWB.
".Trim(), serviceTaskLog.ToString());

					var logNoteText = message.GetLogNoteText();
					AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching CusMAWB found, creating new CusMAWB.
Populating CusMAWB...
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Successfully matched organization with code 'BARSOU'.
Successfully matched organization with code 'ABABEU'.
Skipped Organization Match on Company [Gagan gogia].
Added Air Cargo House (HAWB: 1) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Successfully matched organization with code 'BARSOU'.
Successfully matched organization with code 'ABABEU'.
Skipped Organization Match on Company [dillen ozdemir].
Added Air Cargo House (HAWB: 2) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Successfully matched organization with code 'ABABEU'.
Skipped Organization Match on Company [Sean Supierz].
Added Air Cargo House (HAWB: 3) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Successfully matched organization with code 'ABABEU'.
Skipped Organization Match on Company [amanda takasch].
Added Air Cargo House (HAWB: 4) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Successfully matched organization with code 'ABABEU'.
Skipped Organization Match on Company [Micaela Alcaino].
Added Air Cargo House (HAWB: 8) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Successfully matched organization with code 'ABABEU'.
Skipped Organization Match on Company [Rebecca Erhart].
Added Air Cargo House (HAWB: 9) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Successfully matched organization with code 'ABABEU'.
Skipped Organization Match on Company [Darryl Frost].
Added Air Cargo House (HAWB: 5) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Successfully matched organization with code 'ABABEU'.
Skipped Organization Match on Company [Blueprint Productions].
Added Air Cargo House (HAWB: 6) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Successfully matched organization with code 'ABABEU'.
Skipped Organization Match on Company [rajnish sharma].
Added Air Cargo House (HAWB: 7) from UniversalShipment.
Added AirCargo Report (MAWB: 081-23232322 MHB: HB1) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: 081-23232322 MHB: HB1) with 9 x CusHAWB.
".Trim(), logNoteText);

					var query = new ZQuery(CusMAWBSchema.CM_MAWB, "08123232322");
					query.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, "HB1");
					var mawbs = Factory.Load<CusMAWB>(query);
					AssertEquals("mawbs.Length", 1, mawbs.Length);
					var mawb = mawbs[0];
					AssertNotEquals("HVLV Shipper Consolidation should be direct match", mawbThatShouldBeIgnored, mawb);
					var hawbs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_CM, mawb.PK));
					AssertEquals("hawbs.Length", 9, hawbs.Length);

					var hawbs1 = hawbs.Where(h => h.CS_HAWB == "1").ToArray();
					var hawbs2 = hawbs.Where(h => h.CS_HAWB == "6").ToArray();

					AssertEquals(1, hawbs1.Length);
					AssertEquals(1, hawbs2.Length);

					AssertEquals("hawb1.CS_ConsigneeName", "GAGAN GOGIA", hawbs1[0].CS_ConsigneeName);
					AssertEquals("hawb1.CS_ConsignorName", "BARTON SOUND SYSTEMS", hawbs1[0].CS_ConsignorName);

					AssertEquals("hawb2.CS_ConsigneeName", "BLUEPRINT PRODUCTIONS", hawbs2[0].CS_ConsigneeName);
					AssertEquals("hawb2.CS_ConsignorName", "EFORCITY1", hawbs2[0].CS_ConsignorName);
				});
			}
		}

		public void TestImportHVLVShipperConsolidationWithCoLoadConsol()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var mawbThatShouldBeIgnored = Factory.New<CusMAWB>();
				mawbThatShouldBeIgnored.CM_MAWB = "08155550423";
				mawbThatShouldBeIgnored.CM_MasterHouseBill = "H0423SUB";
				Factory.SaveForTesting();
				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(TestFileHelper.GetPathForUniversalHVLVAirTestFiles("CoLoadHVLVShipment.xml")));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);

					var query = new ZQuery(CusMAWBSchema.CM_MAWB, "08155550423");
					query.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, "H0423HVLVA");
					var mawbs = Factory.Load<CusMAWB>(query);
					AssertEquals("mawbs.Length", 1, mawbs.Length);
					var mawb = mawbs[0];
					AssertNotEquals("HVLV Shipper Consolidation should be direct match", mawbThatShouldBeIgnored, mawb);
					AssertEquals("no hAWB", 0, mawbThatShouldBeIgnored.ChildBills.Count);
				});
			}
		}

		public void TestResponsiblePartyIsNotClearedIfNotSpecified()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
			var responsibleParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB2343";
			mawb.CM_ApplicationCode = "ABC";
			mawb.CM_MasterHouseBill = "CL343";
			mawb.CM_OH_ResponsibleParty = responsibleParty.PK;
			mawb.CM_ResponsiblePartyID = "ADS12312";
			Factory.SaveForTesting();
			var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");
			var mock = new Mock<CusMAWBDataObjectReader>(mawbDataObject, null, logger, Factory, (ZString)"ABC", false);
			mock
				.Protected()
				.Setup<CusMAWB>("GetExistingBusinessObjectUsingModuleSpecificBusinessRules")
				.Returns(mawb);
			var reader = mock.Object;
			var mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawb, mawbBO);
			AssertEquals("mawbBO.CM_OH_ResponsibleParty", responsibleParty.PK, mawbBO.CM_OH_ResponsibleParty);
			AssertEquals("mawbBO.CM_ResponsiblePartyID", "ADS12312", mawbBO.CM_ResponsiblePartyID);
			mock.VerifyAll();
		}

		public void TestResponsiblePartyPKIsClearedWhenSpecifyOnlyID()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
			var responsibleParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB2343";
			mawb.CM_ApplicationCode = "ABC";
			mawb.CM_MasterHouseBill = "CL343";
			mawb.CM_OH_ResponsibleParty = responsibleParty.PK;
			mawb.CM_ResponsiblePartyID = "ADS12312";
			Factory.SaveForTesting();
			var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");
			mawbDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>(new[] { SetupAdditionalReference(null, null, "RIP3423", new EntryType() { Code = Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID }) }));
			var mock = new Mock<CusMAWBDataObjectReader>(mawbDataObject, null, logger, Factory, (ZString)"ABC", false);
			mock.CallBase = true;
			mock
				.Protected()
				.Setup<CusMAWB>("GetExistingBusinessObjectUsingModuleSpecificBusinessRules")
				.Returns(mawb);
			var reader = mock.Object;
			var mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawb, mawbBO);
			AssertEquals("mawbBO.CM_OH_ResponsibleParty", ZGuid.Empty, mawbBO.CM_OH_ResponsibleParty);
			AssertEquals("mawbBO.CM_ResponsiblePartyID", "RIP3423", mawbBO.CM_ResponsiblePartyID);
			mock.VerifyAll();
		}

		public void TestResponsiblePartyIsNotImportedIfItDoesNotMatchID()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
			var responsibleParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var responsibleParty2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB2343";
			mawb.CM_ApplicationCode = "ABC";
			mawb.CM_MasterHouseBill = "CL343";
			mawb.CM_OH_ResponsibleParty = responsibleParty2.PK;
			Factory.SaveForTesting();
			var writeManager = new DataWritingManager(new ActionInfo(null, mawb));
			var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");
			mawbDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>(new[] { SetupAdditionalReference(null, null, "RIP3423", new EntryType() { Code = Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID }) }));
			mawbDataObject.AddOrgAddress(writeManager, responsibleParty, AddressTypes.ResponsibleParty);
			var mockReader1 = new Mock<CusMAWBDataObjectReader>(mawbDataObject, null, logger, Factory, (ZString)"ABC", false);
			mockReader1.CallBase = true;
			mockReader1
				.Protected()
				.Setup<ZString?>("GetResponsiblePartyID", ItExpr.IsAny<ZGuid>(), ItExpr.IsAny<ZString?>())
				.Returns((ZString?)null);
			mockReader1
				.Protected()
				.Setup<CusMAWB>("GetExistingBusinessObjectUsingModuleSpecificBusinessRules")
				.Returns(mawb);
			var reader = mockReader1.Object;
			var mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawb, mawbBO);
			AssertEquals("mawbBO.CM_OH_ResponsibleParty", ZGuid.Empty, mawbBO.CM_OH_ResponsibleParty);
			AssertEquals("mawbBO.CM_ResponsiblePartyID", "RIP3423", mawbBO.CM_ResponsiblePartyID);
			mockReader1.VerifyAll();

			mawb.CM_OH_ResponsibleParty = responsibleParty2.PK;
			Factory.SaveForTesting();
			var mockReader2 = new Mock<CusMAWBDataObjectReader>(mawbDataObject, null, logger, Factory, (ZString)"ABC", false);
			mockReader2.CallBase = true;
			mockReader2
				.Protected()
				.Setup<ZString?>("GetResponsiblePartyID", ItExpr.IsAny<ZGuid>(), ItExpr.IsAny<ZString?>())
				.Returns((ZString?)"DS342");
			mockReader2
				.Protected()
				.Setup<CusMAWB>("GetExistingBusinessObjectUsingModuleSpecificBusinessRules")
				.Returns(mawb);
			reader = mockReader2.Object;
			mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawb, mawbBO);
			AssertEquals("mawbBO.CM_OH_ResponsibleParty", ZGuid.Empty, mawbBO.CM_OH_ResponsibleParty);
			AssertEquals("mawbBO.CM_ResponsiblePartyID", "RIP3423", mawbBO.CM_ResponsiblePartyID);
			mockReader2.VerifyAll();

			mawb.CM_OH_ResponsibleParty = responsibleParty2.PK;
			Factory.SaveForTesting();
			var mockReader3 = new Mock<CusMAWBDataObjectReader>(mawbDataObject, null, logger, Factory, (ZString)"ABC", false);
			mockReader3.CallBase = true;
			mockReader3
				.Protected()
				.Setup<ZString?>("GetResponsiblePartyID", ItExpr.IsAny<ZGuid>(), ItExpr.IsAny<ZString?>())
				.Returns((ZString?)"RIP3423");
			mockReader3
				.Protected()
				.Setup<CusMAWB>("GetExistingBusinessObjectUsingModuleSpecificBusinessRules")
				.Returns(mawb);
			reader = mockReader3.Object;
			mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals(mawb, mawbBO);
			AssertEquals("mawbBO.CM_OH_ResponsibleParty", responsibleParty.PK, mawbBO.CM_OH_ResponsibleParty);
			AssertEquals("mawbBO.CM_ResponsiblePartyID", "RIP3423", mawbBO.CM_ResponsiblePartyID);
			mockReader3.VerifyAll();
		}

		public void TestImportingCusMAWBData()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
			var responsibleParty = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var depotAddress = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress;
			var goodLocation = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress;
			Factory.SaveForTesting();
			var mawbDataObject = SetupAirCargoMaster("MB2343", "CL343");
			mawbDataObject.Branch = Branch.New(CurrentCompanySecondBranch);

			mawbDataObject.SetDateCollection(() => new List<Date>());
			mawbDataObject.DateCollection.Add(Date.New(DateType.LoadingDate, ZBool.True, new ZDateTime(2010, 1, 4)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.LoadingDate, ZBool.False, new ZDateTime(2010, 1, 3)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.True, new ZDateTime(2010, 1, 6)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2010, 1, 5)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.DischargeDate, ZBool.True, new ZDateTime(2010, 1, 7)));
			mawbDataObject.DateCollection.Add(Date.New(DateType.DischargeDate, ZBool.False, new ZDateTime(2010, 1, 8)));

			mawbDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			mawbDataObject.NoteCollection.Add(SetupNote());
			mawbDataObject.NoteCollection.Add(SetupNote2());

			mawbDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			mawbDataObject.AdditionalReferenceCollection.Add(SetupAdditionalReference());
			mawbDataObject.AdditionalReferenceCollection.Add(SetupAdditionalReference(null, null, "RIP3423", new EntryType() { Code = Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID }));

			mawbDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			mawbDataObject.OrganizationAddressCollection.Add(SetupOrganizationAddress(AddressTypes.Forwarder));
			var responsiblePartyData = SetupOrganizationAddress(AddressTypes.ResponsibleParty, null, null, responsibleParty.OH_FullName, responsibleParty.MainAddress.OA_Address1, responsibleParty.MainAddress.OA_Address2,
				responsibleParty.MainAddress.OA_City, responsibleParty.MainAddress.OA_State, responsibleParty.MainAddress.OA_PostCode, UNLOCO.New(responsibleParty.MainAddress.EffectiveRelatedPortCode), Country.New(responsibleParty.MainAddress.RelatedCountry), responsibleParty.Contacts[0].OC_ContactName, responsibleParty.MainAddress.OA_Phone);
			mawbDataObject.OrganizationAddressCollection.Add(responsiblePartyData);
			var depotAddressData = SetupOrganizationAddress(nameof(DocAddressType.ArrivalCFSAddress), depotAddress.OA_Code, depotAddress.Header.OH_Code, depotAddress.Header.OH_FullName, depotAddress.OA_Address1, depotAddress.OA_Address2,
				depotAddress.OA_City, depotAddress.OA_State, depotAddress.OA_PostCode, UNLOCO.New(depotAddress.EffectiveRelatedPortCode), Country.New(depotAddress.RelatedCountry), depotAddress.Header.Contacts[0].OC_ContactName, depotAddress.OA_Phone);
			mawbDataObject.OrganizationAddressCollection.Add(depotAddressData);
			var goodLocationAddress = SetupOrganizationAddress(nameof(DocAddressType.GoodsLocation), goodLocation.OA_Code, goodLocation.Header.OH_Code, goodLocation.Header.OH_FullName, goodLocation.OA_Address1, goodLocation.OA_Address2,
				goodLocation.OA_City, goodLocation.OA_State, goodLocation.OA_PostCode, UNLOCO.New(goodLocation.EffectiveRelatedPortCode), Country.New(goodLocation.RelatedCountry), goodLocation.Header.Contacts[0].OC_ContactName, goodLocation.OA_Phone);
			mawbDataObject.OrganizationAddressCollection.Add(goodLocationAddress);

			var hawbDataObject = SetupAirCargoHouse("HB2343", ZBool.True, null);
			var subHawbDataObject = SetupAirCargoHouse("SB8965", ZBool.False, "HB2343");

			hawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			hawbDataObject.SubShipmentCollection.Add(subHawbDataObject);

			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject);

			var reader = new CusMAWBDataObjectReader(mawbDataObject, null, logger, Factory, "ABC");
			var mawbBO = reader.ReadIntoBusinessObject();

			AssertNotNull(mawbBO);

			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertContents(mawbBO, "MB2343", "CL343");
				AssertEquals("mawbBO.CM_ApplicationCode", "ABC", mawbBO.CM_ApplicationCode);
				AssertEquals("mawbBO.CM_IsCTOMAWB", ZBool.False, mawbBO.CM_IsCTOMAWB);
				AssertEquals("mawbBO.CM_DepartureDate", new ZDateTime(2010, 1, 3), mawbBO.CM_DepartureDate);
				AssertEquals("mawbBO.CM_DateOfFirstArrival", new ZDateTime(2010, 1, 5), mawbBO.CM_DateOfFirstArrival);
				AssertEquals("mawbBO.CM_ArrivalDate", new ZDateTime(2010, 1, 8), mawbBO.CM_ArrivalDate);
				AssertEquals("mawbBO.CM_ResponsiblePartyID", "RIP3423", mawbBO.CM_ResponsiblePartyID);
				AssertEquals("mawbBO.CM_OH_ResponsibleParty", responsibleParty.PK, mawbBO.CM_OH_ResponsibleParty);
				AssertEquals("mawbBO.CM_OA_UnpackDepotAddress", depotAddress.PK, mawbBO.CM_OA_UnpackDepotAddress);
				AssertEquals("mawbBO.CM_OA_GoodsLocation", goodLocation.PK, mawbBO.CM_OA_GoodsLocation);

				Assert("Should reload ChildBills in FillHAWBs.", mawbBO.ChildBillsIsLoaded);
				AssertEquals("mawbBO.ChildBills.Count", 2, mawbBO.ChildBills.Count);

				var hawbBO = mawbBO.ChildBills[0];
				var subhawbBO = mawbBO.ChildBills[1];
				if (subhawbBO.CS_CS_MasterHouseBill.IsEmpty)
				{
					hawbBO = mawbBO.ChildBills[1];
					subhawbBO = mawbBO.ChildBills[0];
				}
				AssertEquals("hawbBO.CS_HAWB", "HB2343", hawbBO.CS_HAWB);
				AssertEquals("hawbBO.CS_CS_MasterHouseBill", ZGuid.Empty, hawbBO.CS_CS_MasterHouseBill);
				AssertEquals("subhawbBO.CS_HAWB", "SB8965", subhawbBO.CS_HAWB);
				AssertEquals("subhawbBO.CS_CS_MasterHouseBill", hawbBO.PK, subhawbBO.CS_CS_MasterHouseBill);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusMAWB found, creating new CusMAWB.
Information - Populating CusMAWB...
Information - Matching 'ResponsibleParty':- Matched to 'WUFSHIJNB' address 'Level 2, Building G' with a score of 250.
Information - Matching 'ArrivalCFSAddress':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Information - Matching 'GoodsLocation':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: WORM EATER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Matching 'GoodsLocation':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Matching 'GoodsLocation':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - Added House Bill from UniversalShipment.
Information - Added House Bill from UniversalShipment.
Information - Added Master Bill from UniversalShipment.
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestShouldNotImportWhenThereAreMultipleHousesForSingleHAWB()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
			var mawbDataObject = SetupAirCargoMaster("MB-23 43", null);
			mawbDataObject.Folio = "HELLO";
			var hawbDataObject1 = SetupAirCargoHouse("HB1", ZBool.False, null);
			hawbDataObject1.GoodsDescription = "HI";

			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject1);
			var mock1 = new Mock<CusMAWBDataObjectReader>(mawbDataObject, null, logger, Factory, (ZString)"ABC", true);
			mock1.CallBase = true;
			var reader = mock1.Object;
			var mawbBO = reader.ReadIntoBusinessObject();
			AssertEquals("mawbBO.CM_Folio", "HELLO", mawbBO.CM_Folio);
			mawbBO.ChildBills.Load();
			AssertEquals("mawbBO.ChildBills.Count", 1, mawbBO.ChildBills.Count);
			var hawbBO = mawbBO.ChildBills[0];
			AssertEquals("hawbBO.CS_GoodsDescription", "HI", hawbBO.CS_GoodsDescription);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusMAWB found, creating new CusMAWB.
Information - Populating CusMAWB...
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Added House Bill from UniversalShipment.
Information - Added Master Bill from UniversalShipment.
			".Trim(), logger.Logs);
			mock1.VerifyAll();

			mawbBO.CM_Folio = "BYE";
			hawbBO.CS_GoodsDescription = "CIAO";
			Factory.SaveForTesting();
			var subHawbDataObject2 = SetupAirCargoHouse("HB2", ZBool.False, null);
			subHawbDataObject2.GoodsDescription = "YO";
			mawbDataObject.SubShipmentCollection.Add(subHawbDataObject2);
			logger.ClearLogs();
			var mock2 = new Mock<CusMAWBDataObjectReader>(mawbDataObject, null, logger, Factory, (ZString)"ABC", true);
			mock2.CallBase = true;
			mock2
				.Protected()
				.Setup<CusMAWB>("GetExistingBusinessObjectUsingModuleSpecificBusinessRules")
				.Returns(mawbBO);
			reader = mock2.Object;
			var mawbBO2 = reader.ReadIntoBusinessObject();
			AssertEquals(mawbBO, mawbBO2);
			AssertEquals("mawbBO.CS_Folio", "BYE", mawbBO.CM_Folio);
			mawbBO.ChildBills.Load();
			AssertEquals("mawbBO.ChildBills.Count", 1, mawbBO.ChildBills.Count);
			AssertEquals(hawbBO, mawbBO.ChildBills[0]);
			AssertEquals("hawbBO.CS_GoodsDescription", "CIAO", hawbBO.CS_GoodsDescription);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusMAWB.
Error - Cannot populate CusMAWB because:
Only one House Bill (SubShipmentCollection element) is allowed for 'AirManifestLine' importation.
			".Trim(), logger.Logs);
			mock2.VerifyAll();

			logger.ClearLogs();
			var mock3 = new Mock<CusMAWBDataObjectReader>(mawbDataObject, null, logger, Factory, (ZString)"ABC", false);
			mock3.CallBase = true;
			mock3
				.Protected()
				.Setup<CusMAWB>("GetExistingBusinessObjectUsingModuleSpecificBusinessRules")
				.Returns(mawbBO);
			reader = mock3.Object;
			mawbBO2 = reader.ReadIntoBusinessObject();
			AssertEquals(mawbBO, mawbBO2);
			AssertEquals("mawbBO.CS_Folio", "HELLO", mawbBO.CM_Folio);
			mawbBO.ChildBills.Load();
			AssertEquals("mawbBO.ChildBills.Count", 2, mawbBO.ChildBills.Count);
			var hawbBO1 = mawbBO.ChildBills[0];
			var hawbBO2 = mawbBO.ChildBills[1];
			if (hawbBO2 == hawbBO)
			{
				hawbBO1 = mawbBO.ChildBills[1];
				hawbBO2 = mawbBO.ChildBills[0];
			}
			else
			{
				AssertEquals(hawbBO, hawbBO1);
			}
			AssertEquals("hawbBO1.CS_GoodsDescription", "HI", hawbBO1.CS_GoodsDescription);
			AssertEquals("hawbBO2.CS_GoodsDescription", "YO", hawbBO2.CS_GoodsDescription);
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusMAWB.
Information - Populating CusMAWB...
Information - Successfully loaded matching CusHAWB.
Information - Populating CusHAWB...
Information - Updated House Bill from UniversalShipment.
Information - No matching CusHAWB found, creating new CusHAWB.
Information - Populating CusHAWB...
Information - Added House Bill from UniversalShipment.
Information - Updated Master Bill from UniversalShipment.
			".Trim(), logger.Logs);

			mock3.VerifyAll();
		}

		public void TestImportingSingleHAWBDoesNotDeleteExistingOnes()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Eritrea);
			var newFactory = new BusinessObjectFactory();
			var existingMAWB = newFactory.New<CusMAWB>();
			existingMAWB.CM_ApplicationCode = "CMR";
			existingMAWB.CM_MAWB = "MB2343";
			existingMAWB.CM_MasterHouseBill = ZString.Empty;
			var existingMAWBHAWB = existingMAWB.ChildBills.AddNew();
			existingMAWBHAWB.CS_HAWB = "HB2343";
			existingMAWBHAWB.CS_IsMasterHouse = ZBool.True;
			newFactory.Save();

			var mawbDataObject = SetupAirCargoMaster("MB-23 43", null);
			var hawbDataObject = SetupAirCargoHouse("HB5983", ZBool.False, null);
			mawbDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			mawbDataObject.SubShipmentCollection.Add(hawbDataObject);

			var mock1 = new Mock<CusMAWBDataObjectReader>(mawbDataObject, null, logger, Factory, (ZString)"CMR", true);
			mock1.CallBase = true;
			mock1
				.Protected()
				.Setup<CusMAWB>("GetExistingBusinessObjectUsingModuleSpecificBusinessRules")
				.Returns(Factory.BOFactory.Load<CusMAWB>(existingMAWB.PK));
			var reader = mock1.Object;
			var mawbBO = reader.ReadIntoBusinessObject();
			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertEquals(existingMAWB.PK, mawbBO.PK);
				mawbBO.ChildBills.Load();
				AssertEquals("mawbBO.ChildBills.Count", 2, mawbBO.ChildBills.Count);
				var hawbBO1 = mawbBO.ChildBills[0];
				var hawbBO2 = mawbBO.ChildBills[1];
				if (hawbBO2.PK == existingMAWBHAWB.PK)
				{
					hawbBO1 = mawbBO.ChildBills[1];
					hawbBO2 = mawbBO.ChildBills[0];
				}
				else
				{
					AssertEquals(hawbBO1.PK, existingMAWBHAWB.PK);
				}
				existingMAWBHAWB.Reload();
				AssertEquals("existingMAWBHAWB.IsDeleted", false, existingMAWBHAWB.IsDeleted);
				AssertCusHAWBContents(hawbBO2, "HB5983", ZBool.False, null);
				hawbBO2.Delete();
			});

			mock1.VerifyAll();
			#endregion

			var mock2 = new Mock<CusMAWBDataObjectReader>(mawbDataObject, null, logger, Factory, (ZString)"CMR", false);
			mock2.CallBase = true;
			mock2
				.Protected()
				.Setup<CusMAWB>("GetExistingBusinessObjectUsingModuleSpecificBusinessRules")
				.Returns(Factory.BOFactory.Load<CusMAWB>(existingMAWB.PK));
			reader = mock2.Object;
			mawbBO = reader.ReadIntoBusinessObject();
			#region Check Contents of mawb Business Object

			CombineAssertions(delegate
			{
				AssertEquals(existingMAWB.PK, mawbBO.PK);
				mawbBO.ChildBills.Load();
				AssertEquals("mawbBO.ChildBills.Count", 1, mawbBO.ChildBills.Count);
				existingMAWBHAWB.Reload();
				AssertEquals("existingMAWBHAWB.IsDeleted", false, existingMAWBHAWB.IsDeleted);
				AssertCusHAWBContents(mawbBO.ChildBills[0], "HB5983", ZBool.False, null);
			});

			mock2.VerifyAll();
			#endregion
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}

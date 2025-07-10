using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondBill))]
	sealed class CusInBondBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniversalCopy()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])typeof(CusInBondBill).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false);
			AssertEquals(1, ignoreElementAttributes.Length);
			AssertEquals(6, ignoreElementAttributes[0].ElementNames.Count);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusAddInfos, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusCodeDatas, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(Universal.Constants.Header.UniversalCopyIgnoreElement.CusInbondBillAddRefs, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondBill.Schema.B0_MessageStatus, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondBill.Schema.B0_ReleaseStatus, ignoreElementAttributes[0].ElementNames);
			AssertCollectionContains(CusInBondBill.Schema.B0_ReleaseStatusDate, ignoreElementAttributes[0].ElementNames);

			var additionalReferences = typeof(CusInBondBill).GetProperty(nameof(CusInBondBill.AdditionalReferences), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(additionalReferences, typeof(UniversalCopyCollectionEntityAttribute)));

			var docAddresses = typeof(CusInBondBill).GetProperty(nameof(CusInBondBill.DocAddresses), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			Assert(Attribute.IsDefined(docAddresses, typeof(UniversalCopySplitCollectionAttribute)));
		}

		public void TestIMessageAttachee()
		{
			bill.B0_MessageStatus = "AAV";
			bill.Messages.AddNew(typeof(EDIMessage));
			header.BH_JobReference = "123456";
			header.Logs.AddNew();
			var iMessageAttachee = bill as IMessageAttachee;
			AssertEquals("AAV", iMessageAttachee.MessageStatus);
			AssertEquals(bill.Messages, iMessageAttachee.Messages);
			AssertEquals(header.Branch, iMessageAttachee.Branch);
			AssertEquals(bill.Header, iMessageAttachee.TopLevelBusinessObject);
			AssertEquals("123456", iMessageAttachee.TopLevelBizObjReferenceNumber);
			AssertEquals(header.Logs, iMessageAttachee.TopLevelBusinessObjectLogs);
		}

		public void TestIResetToOriginal()
		{
			bill.B0_MessageStatus = "AAV";
			bill.B0_MasterBillNumber = "123456";
			var iResetToOriginal = bill as IResetToOriginal;
			AssertEquals(ZString.Empty, iResetToOriginal.InBondNumber);
			AssertEquals(ZString.Empty, iResetToOriginal.MovementDescription);
			AssertEquals("AAV", iResetToOriginal.CustomsStatus);
			AssertEquals("123456", iResetToOriginal.BillNumber);
			AssertEquals(ZString.Empty, iResetToOriginal.ContainerNumber);
			AssertEquals("Bills of Lading", iResetToOriginal.Level);
			Assert(bill.IsResetableToOriginal);
			iResetToOriginal.ResetStatus("");
			AssertEquals(ZString.Empty, bill.B0_MessageStatus);
		}

		public void TestMasterBillCarrierHasNumericBillPrefix()
		{
			header.BH_ImportTransportMode = "40";
			var usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "A8"));
			if (usCarrier == null)
			{
				usCarrier = Factory.New<USCarrierCombined>();
				usCarrier.UI_Code = "A8";
				usCarrier.UI_ModeOfTransportation = "40";
				usCarrier.UI_Name = "Test Carrier";
			}

			usCarrier.UI_AirwayBillPrefix = "WMF";
			bill.B0_IssuerCode = usCarrier.UI_Code;
			Assert(!bill.MasterBillCarrierHasNumericBillPrefix);
			usCarrier.UI_Code = "A9";
			usCarrier.UI_AirwayBillPrefix = "123";
			bill.B0_IssuerCode = usCarrier.UI_Code;
			Assert(bill.MasterBillCarrierHasNumericBillPrefix);
		}

		public void TestIsAir()
		{
			header.BH_ImportTransportMode = "10";
			Assert(!bill.IsAir);
			header.BH_ImportTransportMode = "40";
			Assert(bill.IsAir);
		}

		public void TestProperties()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				header.BH_ImportTransportMode = "40";
				header.MovementHeader.BM_InBondCarrierSCAC = "EFGH";
				bill.B0_IssuerCode = "ABCD";
				bill.B0_MasterBillNumber = "12345";
				bill.B0_HouseBillNumber = "54321";
				AssertEquals("HumanReadableName", "Bill Of Lading 54321 (ABCD 12345)", bill.HumanReadableName);
				header.BH_ImportTransportMode = "10";
				AssertEquals("HumanReadableName", "Bill Of Lading 54321 (ABCD 12345)", bill.HumanReadableName);
				bill.B0_HouseBillNumber = "";
				AssertEquals("HumanReadableName", "Bill Of Lading ABCD 12345", bill.HumanReadableName);
				header.BH_FTZMove = true;
				AssertEquals("Foreign Port Of Lading should be set to '99999' for FTZ/Warehouse move. This is the only circumstance when ‘99999’ may be used.", CusInBondBill.FTZForeignPortOfLading, bill.B0_PortOfLadingKCode);
				AssertEquals("EFGH", bill.B0_IssuerCode);
				header.BH_FTZMove = false;
				bill.B0_PortOfLadingKCode = "62000";
				header.BH_FTZMove = true;
				AssertEquals("Foreign Port Of Lading should not be changed to '99999', because value already entered, validation provided instead.", "62000", bill.B0_PortOfLadingKCode);
			}
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CusInBondBillLookups), bill.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusInBondBillValidation), bill.Validation.GetType());
		}

		public void TestWeight()
		{
			bill.B0_Weight = 1234567;
			bill.B0_WeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals(new ZWeight(1234567, Core.Constants.Weight.Pounds), bill.Weight);
		}

		public void TestVolume()
		{
			bill.B0_Volume = 1234567;
			bill.B0_VolumeUQ = Core.Constants.Volume.CubicFeet;
			AssertEquals(new ZVolume(1234567, Core.Constants.Volume.CubicFeet), bill.Volume);
		}

		public void TestBillUniqueCode()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				header.BH_ImportTransportMode = "40";
				bill.B0_IssuerCode = "ABCD";
				bill.B0_MasterBillNumber = "12345";
				bill.B0_HouseBillNumber = "54321";
				AssertEquals("Bill Unique Code", "54321 (ABCD 12345)", bill.BillUniqueCode);
				header.BH_ImportTransportMode = "10";
				AssertEquals("Bill Unique Code", "54321 (ABCD 12345)", bill.BillUniqueCode);
				bill.B0_HouseBillNumber = "";
				AssertEquals("Bill Unique Code", "ABCD 12345", bill.BillUniqueCode);
				header.BH_FTZMove = true;
				AssertEquals("Bill Unique Code", "12345", bill.BillUniqueCode);
			}
		}

		public void TestSetIssuerCodeForBillIssuerWithFTZMove()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = false;
			header.BH_CarrierSCAC = "SCAC";
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "";
			var moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondCarrierSCAC = "ABCD";
			AssertEquals("SCAC", header.BH_CarrierSCAC);
			AssertEquals("ABCD", moveHeader1.BM_InBondCarrierSCAC);
			AssertEquals("", bill1.B0_IssuerCode);
			header.BH_FTZMove = true;
			AssertEquals("ABCD", moveHeader1.BM_InBondCarrierSCAC);
			AssertEquals("should be same as SCAC", "ABCD", bill1.B0_IssuerCode);
		}

		public void TestDeleteBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var masterBill0 = declaration.Bills.AddNew();
			masterBill0.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill0.CU_BillNum = "MBTEST1";
			var houseBill0 = declaration.Bills.AddNew();
			houseBill0.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill0.CU_CU_ParentBill = masterBill0.PK;
			houseBill0.CU_BillNum = "HBTEST1";
			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill1.CU_BillNum = "MBTEST2";
			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill1.PK;
			houseBill1.CU_BillNum = "HBTEST2";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = declaration.TablePrefix;
			var synchronizer = (CusInBondHeaderDeclarationSynchronizer)header.Synchroniser;
			synchronizer.Synchronise(true);
			AssertEquals(2, header.Bills.Count);
			AssertEquals("MBTEST1", header.Bills[0].B0_MasterBillNumber);
			Assert("Bill can't be deleted.", !header.Bills[0].CanDelete);
			AssertEquals(string.Format("This bill cannot be deleted {0}.", ValidationConstants.Synchronize.SynchronizedFromParent(JobDeclarationSchema.Constants.TableName)), header.Bills[0].ReasonForNotAbleToDelete);
			AssertEquals("MBTEST2", header.Bills[1].B0_MasterBillNumber);
			Assert("Bill can't be deleted.", !header.Bills[1].CanDelete);
			AssertEquals(string.Format("This bill cannot be deleted {0}.", ValidationConstants.Synchronize.SynchronizedFromParent(JobDeclarationSchema.Constants.TableName)), header.Bills[1].ReasonForNotAbleToDelete);
			header.BH_OverrideFreightDefaults = true;
			Assert("Bill can be deleted.", header.Bills[0].CanDelete);
			Assert("Bill can be deleted.", header.Bills[1].CanDelete);
			header.BH_OverrideFreightDefaults = false;
			header.BH_ParentID = ZGuid.Empty;
			Assert("Bill can be deleted.", header.Bills[0].CanDelete);
			Assert("Bill can be deleted.", header.Bills[1].CanDelete);
			header.BH_ParentID = declaration.PK;
			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MBTEST3";
			synchronizer.Synchronise(true);
			AssertEquals(3, header.Bills.Count);
			AssertEquals("MBTEST3", header.Bills[2].B0_MasterBillNumber);
			Assert("Bill can't be deleted.", !header.Bills[2].CanDelete);
			AssertEquals(string.Format("This bill cannot be deleted {0}.", ValidationConstants.Synchronize.SynchronizedFromParent(JobDeclarationSchema.Constants.TableName)), header.Bills[2].ReasonForNotAbleToDelete);
			masterBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals(2, header.Bills.Count);
			AssertEquals("MBTEST1", header.Bills[0].B0_MasterBillNumber);
			Assert("Bill can't be deleted.", !header.Bills[0].CanDelete);
			AssertEquals("MBTEST2", header.Bills[1].B0_MasterBillNumber);
			Assert("Bill can't be deleted.", !header.Bills[1].CanDelete);
			masterBill2.Delete();
			AssertEquals(2, header.Bills.Count);
			AssertEquals("MBTEST1", header.Bills[0].B0_MasterBillNumber);
			Assert("Bill can't be deleted.", !header.Bills[0].CanDelete);
			AssertEquals("MBTEST2", header.Bills[1].B0_MasterBillNumber);
			Assert("Bill can't be deleted.", !header.Bills[1].CanDelete);
		}

		public void TestICargoManifestStatusQueryData()
		{
			header.BH_ImportTransportMode = "40";
			bill.B0_IssuerCode = "ABCD";
			bill.B0_MasterBillNumber = "12345";
			bill.B0_HouseBillNumber = "HouseBill";
			bill.B0_HouseBillIssuerCode = "HBIC";
			var billWrapper = new CusInBondBillCargoManifestStatusQueryWrapper(bill, true, true);
			var iCargoManifestStatusQueryData = (ICargoManifestStatusQueryData)billWrapper;
			AssertEquals("Bill Of Lading HouseBill (ABCD 12345)", iCargoManifestStatusQueryData.HumanFriendlyReference);
			AssertEquals("", iCargoManifestStatusQueryData.EntryOrInBondNumber);
			AssertEquals("12345", iCargoManifestStatusQueryData.MasterAirWayBillNumber);
			AssertEquals("12345", iCargoManifestStatusQueryData.BillNumber);
			AssertEquals("HouseBill", iCargoManifestStatusQueryData.HouseAirWayBillNumber);
			AssertEquals("ABCD", iCargoManifestStatusQueryData.BillIssuerCode);
			AssertEquals(CargoManifestQueryActionType.MAWB, iCargoManifestStatusQueryData.QueryActionType);
			Assert(iCargoManifestStatusQueryData.IsRelevantFor(CargoManifestStatusQueryActionList.Codes.HAWB));
			header.BH_ImportTransportMode = "10";
			AssertEquals("HBIC", iCargoManifestStatusQueryData.BillIssuerCode);
			AssertEquals("HouseBill", iCargoManifestStatusQueryData.BillNumber);
			bill.B0_HouseBillNumber = "";
			Assert(!iCargoManifestStatusQueryData.IsRelevantFor(CargoManifestStatusQueryActionList.Codes.HAWB));
		}

		public void TestProcessInBondDepartureBillDeleteResponseMessage()
		{
			var header = Factory.New<CusInBondHeader>();
			var move = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			var detail = header.MovementDetails.AddNew();
			detail.B9_B0 = bill.PK;
			detail.B9_BM = move.PK;
			bill.B0_MasterBillNumber = "ST08161905";
			bill.B0_MessageStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			MQEDIMessage sent = Factory.New<MQEDIMessage>();
			sent.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			sent.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransaction;
			sent.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
			sent.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sent.EM_Status = MQEDIMessage.Status.Sent;
			sent.EM_MessageNum = "EDIEDIDAT_1";
			sent.EM_LinkTable = CusInBondBill.Schema.TableName;
			sent.EM_LinkUniqueID = bill.PK;
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse;
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.InBondDepartureDelete;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = MQEDIMessage.Status.Queued;
			message.EM_MessageNum = "EDIEDIDAT_1";
			message.EM_MessageText = "B011101SV9QT                                               EDIEDIDAT_1          " +
				"10B62001003343   MDRL2704413800001000020-064032900YN                            " +
				"30D 0001MDRLMST08161905                                                         " +
				"9502209 INBOND DELETED                                                          " +
				"Y  1101SV9QT00003";
			var processor = new InBondProcessor();
			processor.Message = message;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var messageFactory = new ABIMessageProcessorFactoryTest().GetFactoryInstance();
			messageFactory.ProcessMessage(message);
			processor.Process();
			AssertEquals(ImportMessageStatusList.Codes.ClearDepartureWithdraw, bill.B0_MessageStatus);
			AssertEquals(CusInBondBill.Schema.TableName, message.EM_LinkTable);
			AssertEquals(bill.PK, message.EM_LinkUniqueID);
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
		}

		public void TestICusInbondBillAddRefTypeSupporter()
		{
			var supporter = bill as ICusInbondBillAddRefTypeSupporter;
			AssertEquals(typeof(CusInbondBillAddRef), supporter.AddRefType);
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => bill;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MoveDetails.AddNew();
			moveDetail.B9_BM = moveHeader.PK;
			return bill;
		}

		CusInBondHeader header;
		CusInBondBill bill;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			bill = header.Bills.AddNew();
			var moveDetail = bill.MoveDetails.AddNew();
			moveDetail.B9_BM = moveHeader.PK;
		}
	}
}

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWMessageHelper))]
	sealed class TWMessageHelperTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestNewOutgoingHelper()
		{
			var factory = new BusinessObjectFactory();
			var testMessage = factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = "ECD";
			NUnit.Framework.Assert.That(TWMessageHelper.NewOutgoingHelper(testMessage), NUnit.Framework.Is.TypeOf<N5203MessageHelper>());
			testMessage.EM_MessageType = "ICD";
			NUnit.Framework.Assert.That(TWMessageHelper.NewOutgoingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX5105MessageHelper>());
			testMessage.EM_MessageType = "CAA";
			NUnit.Framework.Assert.That(TWMessageHelper.NewOutgoingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX5105MessageHelper>());
			testMessage.EM_MessageType = "AMD";
			NUnit.Framework.Assert.That(TWMessageHelper.NewOutgoingHelper(testMessage), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.MessageProcessors.TWMessageHelper)));
		}

		[ExpectNoExceptions]
		public void TestNewIncomingHelper()
		{
			var factory = new BusinessObjectFactory();
			var testMessage = factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = MessageTypeList.Codes._402;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX402MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.ARM;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX5106MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.ERM;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<N5204MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.IEM;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<N5109MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.IRM;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<N5116MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.RFM;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<N5107MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.UHC;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<N5168MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.ECD;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWCustomsDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.ICD;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWCustomsDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.ADM;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWCustomsDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.IEA;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWCustomsDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.FHM;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWCustomsDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.TAD;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<N5111MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.TPC;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<N5110MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.TRN;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<N5302MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes.FHR;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<N5108MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._302;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX302MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._102;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX102MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._202;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX202MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._602;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX602MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._901;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX901MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._902;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX902MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._903;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX903MessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._32A;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<NX302_AXMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._101;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWControllingAgencyDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._201;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWControllingAgencyDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._207;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWControllingAgencyDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._301;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWControllingAgencyDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._31A;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWControllingAgencyDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._31D;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWControllingAgencyDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._401;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWControllingAgencyDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._601;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWControllingAgencyDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = MessageTypeList.Codes._603;
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.TypeOf<TWControllingAgencyDeliveryNotificationMessageHelper>());
			testMessage.EM_MessageType = "AMD";
			NUnit.Framework.Assert.That(TWMessageHelper.NewIncomingHelper(testMessage), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.MessageProcessors.TWMessageHelper)));
		}

		[ExpectNoExceptions]
		public void TestLookForCusEntryHeader()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var org2Code = orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress = orgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			factory.Save();
			var dec1 = factory.New<JobDeclaration>();
			dec1.JE_CustomsOffice = "AA";
			var entry1 = dec1.CusEntryInstruction;
			entry1.CEI_DateForDuty = new ZDateTime(2019, 01, 01);
			entry1.CEI_Style = "B1";
			entry1.CEI_CustomsOffice = "AA";
			entry1.CEI_OA_Warehouse2 = orgHeader.MainAddress.PK;
			var cusHead1 = factory.NewWithValidTestData<CusEntryHeader>();
			cusHead1.CH_CEI_Instruction = entry1.PK;
			var cusNum1 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "AAG2082348";
			var dec2 = factory.New<JobDeclaration>();
			dec2.JE_CustomsOffice = "AA";
			var entry2 = dec2.CusEntryInstruction;
			entry2.CEI_DateForDuty = new ZDateTime(2020, 01, 01);
			entry2.CEI_Style = "B1";
			entry2.CEI_CustomsOffice = "AA";
			entry2.CEI_OA_Warehouse2 = orgHeader.MainAddress.PK;
			var cusHead2 = factory.NewWithValidTestData<CusEntryHeader>();
			cusHead2.CH_CEI_Instruction = entry2.PK;
			var cusNum2 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = cusHead2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = SharedJobMessageTypeList.Codes.Export;
			cusNum2.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = "AAG2092348";
			var dec3 = factory.New<JobDeclaration>();
			dec3.JE_CustomsOffice = "AA";
			var entry3 = dec3.CusEntryInstruction;
			entry3.CEI_DateForDuty = new ZDateTime(2021, 01, 01);
			entry3.CEI_Style = "B1";
			entry3.CEI_CustomsOffice = "AA";
			entry3.CEI_OA_Warehouse2 = orgHeader.MainAddress.PK;
			var cusHead3 = factory.NewWithValidTestData<CusEntryHeader>();
			cusHead3.CH_CEI_Instruction = entry3.PK;
			var cusNum3 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum3.CE_ParentID = cusHead3.PK;
			cusNum3.CE_Category = "XXX";
			cusNum3.CE_EntryType = SharedJobMessageTypeList.Codes.Export;
			cusNum3.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum3.CE_EntryNum = "AAG2102348";
			factory.Save();
			var newfactory = new BusinessObjectFactory();
			var testcusHead = TWMessageHelper.LookForCusEntryHeader(newfactory, cusNum1.CE_EntryNum, SharedJobMessageTypeList.Codes.Import);
			NUnit.Framework.Assert.That(testcusHead.PK, NUnit.Framework.Is.EqualTo(cusHead1.PK));
			testcusHead = TWMessageHelper.LookForCusEntryHeader(newfactory, cusNum2.CE_EntryNum, SharedJobMessageTypeList.Codes.Import);
			NUnit.Framework.Assert.That(testcusHead, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeader)));
			testcusHead = TWMessageHelper.LookForCusEntryHeader(newfactory, cusNum2.CE_EntryNum, SharedJobMessageTypeList.Codes.Export);
			NUnit.Framework.Assert.That(testcusHead.PK, NUnit.Framework.Is.EqualTo(cusHead2.PK));
			testcusHead = TWMessageHelper.LookForCusEntryHeader(newfactory, cusNum1.CE_EntryNum, "");
			NUnit.Framework.Assert.That(testcusHead, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeader)));
			testcusHead = TWMessageHelper.LookForCusEntryHeader(newfactory, cusNum2.CE_EntryNum, "");
			NUnit.Framework.Assert.That(testcusHead, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeader)));
			testcusHead = TWMessageHelper.LookForCusEntryHeader(newfactory, cusNum3.CE_EntryNum, SharedJobMessageTypeList.Codes.Export);
			NUnit.Framework.Assert.That(testcusHead, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeader)));
			testcusHead = TWMessageHelper.LookForCusEntryHeader(newfactory, cusNum3.CE_EntryNum, "");
			NUnit.Framework.Assert.That(testcusHead, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusEntryHeader)));
		}

		[ExpectNoExceptions]
		public void TestLookForCusInBondHeader()
		{
			var factory = new BusinessObjectFactory();
			var cusInBondHead1 = factory.NewWithValidTestData<CusInBondHeader>();
			cusInBondHead1.ReceiptOffice = "AA";
			cusInBondHead1.UnladingOffice = "BB";
			cusInBondHead1.TW_BoxNumber = "123";
			var cusNum1 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusInBondHead1.PK;
			cusNum1.CE_EntryType = "TRS";
			cusNum1.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "INNO01";
			var cusInBondHead2 = factory.NewWithValidTestData<CusInBondHeader>();
			cusInBondHead2.ReceiptOffice = "AA";
			cusInBondHead2.UnladingOffice = "BB";
			cusInBondHead2.TW_BoxNumber = "123";
			var cusNum2 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = cusInBondHead2.PK;
			cusNum2.CE_EntryType = "TRS";
			cusNum2.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = "INNO02";
			var cusInBondHead3 = factory.NewWithValidTestData<CusInBondHeader>();
			cusInBondHead3.ReceiptOffice = "AA";
			cusInBondHead3.UnladingOffice = "BB";
			cusInBondHead3.TW_BoxNumber = "123";
			var cusNum3 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum3.CE_ParentID = cusInBondHead3.PK;
			cusNum3.CE_EntryType = "XXX";
			cusNum3.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum3.CE_EntryNum = "INNO03";
			cusInBondHead3.AllocateEntryNumber();
			factory.Save();
			var newfactory = new BusinessObjectFactory();
			var testcusHead = TWMessageHelper.LookForCusInBondHeader(newfactory, cusInBondHead1.EntryNumber);
			NUnit.Framework.Assert.That(testcusHead.PK, NUnit.Framework.Is.EqualTo(cusInBondHead1.PK));
			NUnit.Framework.Assert.That(testcusHead.CusEntryNumber.PK, NUnit.Framework.Is.EqualTo(cusNum1.PK));
			testcusHead = TWMessageHelper.LookForCusInBondHeader(newfactory, cusInBondHead2.EntryNumber);
			NUnit.Framework.Assert.That(testcusHead.PK, NUnit.Framework.Is.EqualTo(cusInBondHead2.PK));
			NUnit.Framework.Assert.That(testcusHead.CusEntryNumber.PK, NUnit.Framework.Is.EqualTo(cusNum2.PK));
			testcusHead = TWMessageHelper.LookForCusInBondHeader(newfactory, cusInBondHead3.EntryNumber);
			NUnit.Framework.Assert.That(testcusHead.PK, NUnit.Framework.Is.EqualTo(cusInBondHead3.PK));
			NUnit.Framework.Assert.That(testcusHead.CusEntryNumber.PK, NUnit.Framework.Is.Not.EqualTo(cusNum3.PK));
		}

		[ExpectNoExceptions]
		public void TestLookForAsycudaManifestHeader()
		{
			var factory = new BusinessObjectFactory();
			var header1 = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_Voyage = "123";
			header1.AMA_TransportMode = "AIR";
			header1.AMA_JobReference = ZString.Empty;
			header1.MasterBill.ABL_E_ARV = new ZDateTime(2023, 11, 7);
			header1.AMA_MasterBill = "456";

			var header2 = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_Voyage = "123";
			header2.AMA_TransportMode = "AIR";
			header2.AMA_JobReference = ZString.Empty;
			header2.MasterBill.ABL_E_ARV = new ZDateTime(2023, 11, 8);
			header2.AMA_MasterBill = "456";

			var header3 = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header3.AMA_Voyage = "123";
			header3.AMA_TransportMode = "AIR";
			header3.AMA_JobReference = ZString.Empty;
			header3.AMA_MasterBill = "456";
			factory.Save();

			var newfactory = new BusinessObjectFactory();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(TWMessageHelper.LookForAsycudaManifestHeader(newfactory, "123", "456", "AIR").PK, NUnit.Framework.Is.EqualTo(header2.PK));
				NUnit.Framework.Assert.That(TWMessageHelper.LookForAsycudaManifestHeader(newfactory, "321", "456", "AIR"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.ManifestBase.AsycudaManifestHeader)), "The corresponding AMA_Voyage cannot be found - should be [null]");
				NUnit.Framework.Assert.That(TWMessageHelper.LookForAsycudaManifestHeader(newfactory, "123", "789", "AIR"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.ManifestBase.AsycudaManifestHeader)), "The corresponding ABL_BillNumber cannot be found - should be [null]");
				NUnit.Framework.Assert.That(TWMessageHelper.LookForAsycudaManifestHeader(newfactory, "123", "456", "SEA"), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.ManifestBase.AsycudaManifestHeader)), "The corresponding AMA_TransportMode cannot be found - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestLookForCusTWControllingMessageHeader()
		{
			var factory = new BusinessObjectFactory();
			var dec1 = factory.New<JobDeclaration>();
			dec1.JE_CustomsOffice = "AA";
			var entry1 = dec1.CusEntryInstruction;
			var controllingMessageHeader1 = entry1.ControllingMessageHeaders.AddNew();
			controllingMessageHeader1.TW1_FunctionalReferenceId = "96944490002307040001";
			var cusHead1 = factory.NewWithValidTestData<CusEntryHeader>();
			cusHead1.CH_CEI_Instruction = entry1.PK;
			var cusNum1 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "AAG2082348";
			var dec2 = factory.New<JobDeclaration>();
			dec2.JE_CustomsOffice = "AA";
			var entry2 = dec2.CusEntryInstruction;
			var controllingMessageHeader2 = entry2.ControllingMessageHeaders.AddNew();
			controllingMessageHeader2.TW1_FunctionalReferenceId = "96944490002307040002";
			var cusHead2 = factory.NewWithValidTestData<CusEntryHeader>();
			cusHead2.CH_CEI_Instruction = entry2.PK;
			var cusNum2 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = cusHead2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = SharedJobMessageTypeList.Codes.Export;
			cusNum2.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = "AAG2092349";
			var dec3 = factory.New<JobDeclaration>();
			dec3.JE_CustomsOffice = "AA";
			var entry3 = dec3.CusEntryInstruction;
			var controllingMessageHeader3 = entry3.ControllingMessageHeaders.AddNew();
			controllingMessageHeader3.TW1_FunctionalReferenceId = "96944490002307040003";
			var cusHead3 = factory.NewWithValidTestData<CusEntryHeader>();
			cusHead3.CH_CEI_Instruction = entry3.PK;
			var cusNum3 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum3.CE_ParentID = cusHead3.PK;
			cusNum3.CE_Category = "XXX";
			cusNum3.CE_EntryType = SharedJobMessageTypeList.Codes.Export;
			cusNum3.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum3.CE_EntryNum = "AAG2102347";
			factory.Save();
			var newfactory = new BusinessObjectFactory();
			var controllingMessageHeader = TWMessageHelper.LookForCusTWControllingMessageHeader(newfactory, "96944490002307040002", "AAG2092349");
			NUnit.Framework.Assert.That(controllingMessageHeader.PK, NUnit.Framework.Is.EqualTo(controllingMessageHeader2.PK));

			controllingMessageHeader = TWMessageHelper.LookForCusTWControllingMessageHeader(newfactory, "96944490002307040002", "AAG2102347");
			NUnit.Framework.Assert.That(controllingMessageHeader, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)));

			controllingMessageHeader = TWMessageHelper.LookForCusTWControllingMessageHeader(newfactory, "96944490002307040002");
			NUnit.Framework.Assert.That(controllingMessageHeader.PK, NUnit.Framework.Is.EqualTo(controllingMessageHeader2.PK));
		}

		[ExpectNoExceptions]
		public void TestLookForCusTWControllingMessageHeader_Declaration()
		{
			var factory = new BusinessObjectFactory();
			var dec1 = factory.New<JobDeclaration>();
			dec1.JE_CustomsOffice = "AA";
			var entry1 = dec1.CusEntryInstruction;
			var controllingMessageHeader1 = entry1.ControllingMessageHeaders.AddNew();
			controllingMessageHeader1.TW1_FunctionalReferenceId = "96944490002311030001";
			var cusHead1 = factory.NewWithValidTestData<CusEntryHeader>();
			cusHead1.CH_CEI_Instruction = entry1.PK;
			var cusNum1 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = dec1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "CA  1245600096";
			var dec2 = factory.New<JobDeclaration>();
			dec2.JE_CustomsOffice = "AA";
			var entry2 = dec2.CusEntryInstruction;
			var controllingMessageHeader2 = entry2.ControllingMessageHeaders.AddNew();
			controllingMessageHeader2.TW1_FunctionalReferenceId = "96944490002311030002";
			var cusHead2 = factory.NewWithValidTestData<CusEntryHeader>();
			cusHead2.CH_CEI_Instruction = entry2.PK;
			var cusNum2 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = dec2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = SharedJobMessageTypeList.Codes.Export;
			cusNum2.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = "CA  1245600097";
			factory.Save();
			var newfactory = new BusinessObjectFactory();
			var controllingMessageHeader = TWMessageHelper.LookForCusTWControllingMessageHeader(newfactory, "96944490002311030001", "CA  1245600096");
			NUnit.Framework.Assert.That(controllingMessageHeader.PK, NUnit.Framework.Is.EqualTo(controllingMessageHeader1.PK));

			controllingMessageHeader = TWMessageHelper.LookForCusTWControllingMessageHeader(newfactory, "96944490002311030002", "CA  1245600096");
			NUnit.Framework.Assert.That(controllingMessageHeader, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)));

			controllingMessageHeader = TWMessageHelper.LookForCusTWControllingMessageHeader(newfactory, "96944490002311030002", "CA  1245600097");
			NUnit.Framework.Assert.That(controllingMessageHeader.PK, NUnit.Framework.Is.EqualTo(controllingMessageHeader2.PK));
		}

		[ExpectNoExceptions]
		public void TestLookForAsycudaManifestBillsWithHeaderPK()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = ZString.Empty;
			var bill1 = header.Bills.AddNew();
			var cusNum1 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = bill1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "FHM";
			cusNum1.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "INNO01";

			var bill2 = header.Bills.AddNew();
			var cusNum2 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = bill2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = "FHM";
			cusNum2.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = "INNO01";

			var header2 = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = ZString.Empty;
			var bill3 = header2.Bills.AddNew();
			var cusNum3 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum3.CE_ParentID = bill3.PK;
			cusNum3.CE_Category = "CUS";
			cusNum3.CE_EntryType = "FHM";
			cusNum3.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum3.CE_EntryNum = "INNO01";

			var bill4 = header.Bills.AddNew();
			var cusNum4 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum4.CE_ParentID = bill4.PK;
			cusNum4.CE_Category = "CUS";
			cusNum4.CE_EntryType = "FHM";
			cusNum4.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum4.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum4.CE_EntryNum = "INNO02";
			factory.Save();

			var newfactory = new BusinessObjectFactory();
			CombineAssertions("functionalReferenceID is not empty", () =>
			{
				var entryNums = TWMessageHelper.LookForAsycudaManifestBillsWithHeaderPK(newfactory, "INNO01", header.PK);
				NUnit.Framework.Assert.That(entryNums.Length, NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(entryNums.Where(x => x.PK == bill1.PK).Count(), NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(entryNums.Where(x => x.PK == bill2.PK).Count(), NUnit.Framework.Is.EqualTo(1));
			});
			CombineAssertions("functionalReferenceID is empty", () =>
			{
				var entryNums = TWMessageHelper.LookForAsycudaManifestBillsWithHeaderPK(newfactory, ZString.Empty, header.PK);
				NUnit.Framework.Assert.That(entryNums.Length, NUnit.Framework.Is.EqualTo(3));
				NUnit.Framework.Assert.That(entryNums.Where(x => x.PK == bill1.PK).Count(), NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(entryNums.Where(x => x.PK == bill2.PK).Count(), NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(entryNums.Where(x => x.PK == bill4.PK).Count(), NUnit.Framework.Is.EqualTo(1));
			});
		}

		[ExpectNoExceptions]
		public void TestLookForAsycudaManifestBills()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var cusNum1 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = bill1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "FHM";
			cusNum1.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "INNO01";

			var bill2 = header.Bills.AddNew();
			var cusNum2 = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum2.CE_ParentID = bill2.PK;
			cusNum2.CE_Category = "CUS";
			cusNum2.CE_EntryType = "FHM";
			cusNum2.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum2.CE_EntryNum = "INNO01";
			factory.Save();

			var newfactory = new BusinessObjectFactory();
			var testBills = TWMessageHelper.LookForAsycudaManifestBills(newfactory, cusNum2.CE_EntryNum);
			NUnit.Framework.Assert.That(testBills.Select(c => c.PK), NUnit.Framework.Is.EquivalentTo(new ZGuid[] { bill1.PK, bill2.PK }));
		}

		[ExpectNoExceptions]
		public void TestGetMessageTypeByCode()
		{
			var expectedMessageTypes = new Dictionary<string, string>()
			{
				{ "N5108", "FHR" },
				{ "NX5106", "ARM" },
				{ "N5204", "ERM" },
				{ "N5109", "IEM" },
				{ "N5116", "IRM" },
				{ "N5107", "RFM" },
				{ "N5168", "UHC" },
				{ "N5110", "TPC" },
				{ "N5111", "TAD" },
				{ "N5302", "TRN" },
				{ "NX202", "202" },
				{ "NX302", "302" },
				{ "NX302_AX", "32A" },
				{ "NX302_DN", "32D" },
				{ "NX402", "402" },
				{ "NX602", "602" },
				{ "NX901", "901" },
				{ "NX902", "902" },
				{ "NX903", "903" }
			};
			foreach (var expectedMessageType in expectedMessageTypes)
			{
				var messageCode = expectedMessageType.Key;
				var status = expectedMessageType.Value;
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeByCode(messageCode), NUnit.Framework.Is.EqualTo(status), string.Format(CultureInfo.InvariantCulture, "{0} is {1}", messageCode, status));
			}
		}

		[ExpectNoExceptions]
		public void TestGetMessageTypeByDatamodelAndValidMessageXML()
		{
			var expectedMessageTypes = new Dictionary<string, string>()
			{
				{ "N5108.N5108.xml", "FHR" },
				{ "NX5106.xml", "ARM" },
				{ "N5204.xml", "ERM" },
				{ "N5109.xml", "IEM" },
				{ "N5116.xml", "IRM" },
				{ "N5107.xml", "RFM" },
				{ "N5168.xml", "UHC" },
				{ "N5110.xml", "TPC" },
				{ "N5111.xml", "TAD" },
				{ "N5302.xml", "TRN" },
				{ "NX102.xml", "102" },
				{ "NX202.xml", "202" },
				{ "NX302.xml", "302" },
				{ "NX302_AX.xml", "32A" },
				{ "NX302_DN.xml", "32D" },
				{ "NX402.xml", "402" },
				{ "NX602.xml", "602" },
				{ "NX901.xml", "901" }
			};
			foreach (var expectedMessageType in expectedMessageTypes)
			{
				var messageCode = expectedMessageType.Key;
				var status = expectedMessageType.Value;
				var xml = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile." + messageCode);
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeByXml(xml), NUnit.Framework.Is.EqualTo(status).Using(CustomComparers.TypeComparison), string.Format(CultureInfo.InvariantCulture, "{0} is {1}", messageCode, status));
				NUnit.Framework.Assert.That(TWMessageHelper.ValidMessageXML(xml), NUnit.Framework.Is.EqualTo(string.Empty), string.Format(CultureInfo.InvariantCulture, "{0} is Valid", messageCode));
			}

			NUnit.Framework.Assert.That(TWMessageHelper.ValidMessageXML("<A>A</A>"), NUnit.Framework.Is.EqualTo(string.Empty), "<A></A> is Valid");
			NUnit.Framework.Assert.That(TWMessageHelper.ValidMessageXML(""), NUnit.Framework.Is.EqualTo("The Message XML is Empty"), "'' is Empty");
			NUnit.Framework.Assert.That(TWMessageHelper.ValidMessageXML("<A>A</B>"), NUnit.Framework.Is.EqualTo("The Message XML is not a valid XML"), "<A>A</B> is not a valid XML");
		}

		[ExpectNoExceptions]
		public void TestGetMessageTypeDescription()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes.ARM), NUnit.Framework.Is.EqualTo("Agency Response Message"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes.ERM), NUnit.Framework.Is.EqualTo("Export Release Notice"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes.IEM), NUnit.Framework.Is.EqualTo("Examination Required Notice"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes.IRM), NUnit.Framework.Is.EqualTo("Import Goods Release Notice"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes.RFM), NUnit.Framework.Is.EqualTo("Required Formalities Message"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes.TAD), NUnit.Framework.Is.EqualTo("Treasury Account Deposit Receipt"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes.TPC), NUnit.Framework.Is.EqualTo("Tax Payment Certificate"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes.TRN), NUnit.Framework.Is.EqualTo("Transshipment/Transit Permit"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes.UHC), NUnit.Framework.Is.EqualTo("Unable to Handle Container"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes._302), NUnit.Framework.Is.EqualTo("Reply Message for Inspection Application"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes._402), NUnit.Framework.Is.EqualTo("Reply Message for Quarantine Application"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes._602), NUnit.Framework.Is.EqualTo("Reply Message from FDA for Inspection Application"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes._32D), NUnit.Framework.Is.EqualTo("Reply Message for Wine Import Application"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes._902), NUnit.Framework.Is.EqualTo("Notice from Licensing Agency"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes._903), NUnit.Framework.Is.EqualTo("Error or Irregularity Notice"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes._901), NUnit.Framework.Is.EqualTo("Reply Message from Licensing Agency"));
				NUnit.Framework.Assert.That(TWMessageHelper.GetMessageTypeDescription(MessageTypeList.Codes._102), NUnit.Framework.Is.EqualTo("Reply Message for Certificate of Origin Application"));
			});
		}

		[ExpectNoExceptions]
		public void TestCreateTWMessageInfoFromTWMessage()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_Code = "111";
			var extPassword = factory.New<GlbExternalPassword>();
			extPassword.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			extPassword.GP_MailBoxID = "123-3";
			extPassword.GP_UserID = "111";
			extPassword.GP_GS = staff.PK;
			extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = "111";
			declaration.JE_CustomsProfile = "123-3";
			declaration.JE_MessageType = "EXP";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "222";
			var twMessage1 = cusEntryHeader.Messages.AddNew();
			twMessage1.EM_ApplicationReference = "TWC";
			twMessage1.EM_MessageType = "ICS";
			var twMessageInfo = TWMessageHelper.CreateTWMessageInfoFromTWMessage(twMessage1);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(twMessageInfo.MailBox, NUnit.Framework.Is.EqualTo("TWC").Using(CustomComparers.TypeComparison), "MailBox");
				NUnit.Framework.Assert.That(twMessageInfo.MessageType, NUnit.Framework.Is.EqualTo("ICS").Using(CustomComparers.TypeComparison), "Message Type");
				NUnit.Framework.Assert.That(twMessageInfo.EntryNumber, NUnit.Framework.Is.EqualTo("222").Using(CustomComparers.TypeComparison), "Entry Number");
				NUnit.Framework.Assert.That(twMessageInfo.EntryNumberType, NUnit.Framework.Is.EqualTo("EXP").Using(CustomComparers.TypeComparison), "Entry Number Type");
				NUnit.Framework.Assert.That(twMessageInfo.StaffCode, NUnit.Framework.Is.EqualTo("111").Using(CustomComparers.TypeComparison), "Staff Code");
				NUnit.Framework.Assert.That(twMessageInfo.CompanyID, NUnit.Framework.Is.EqualTo(GlbCompany.CurrentCompany.GC_Code), "Company ID");
				NUnit.Framework.Assert.That(twMessageInfo.PasswordType, NUnit.Framework.Is.EqualTo(PasswordTypesList.Codes.TVA).Using(CustomComparers.TypeComparison), "Password Type");
			});

			var twMessage2 = factory.New<TWMessage>();
			twMessage2.EM_ApplicationReference = "AAA";
			twMessage2.EM_MessageType = "ECD";
			var twMessageInfo2 = TWMessageHelper.CreateTWMessageInfoFromTWMessage(twMessage2);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(twMessageInfo2.MailBox, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison), "MailBox");
				NUnit.Framework.Assert.That(twMessageInfo2.MessageType, NUnit.Framework.Is.EqualTo("ECD").Using(CustomComparers.TypeComparison), "Message Type");
				NUnit.Framework.Assert.That(twMessageInfo2.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty), "Entry Number");
				NUnit.Framework.Assert.That(twMessageInfo2.EntryNumberType, NUnit.Framework.Is.EqualTo(ZString.Empty), "Entry Number Type");
				NUnit.Framework.Assert.That(twMessageInfo2.StaffCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "Staff Code");
				NUnit.Framework.Assert.That(twMessageInfo2.CompanyID, NUnit.Framework.Is.EqualTo(ZString.Empty), "Company ID");
				NUnit.Framework.Assert.That(twMessageInfo2.PasswordType, NUnit.Framework.Is.EqualTo(ZString.Empty), "Password Type");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateTWMessageInfoFromTWMessage_NxmMessage()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_Code = "111";
			var extPassword = factory.New<GlbExternalPassword>();
			extPassword.GP_PasswordType = PasswordTypesList.Codes.NXM;
			extPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			extPassword.GP_MailBoxID = "123-3";
			extPassword.GP_UserID = "111";
			extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declaration = factory2.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = "111";
			declaration.JE_CustomsProfile = "123-3";
			declaration.JE_MessageType = "EXP";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "222";
			var twMessage = cusEntryHeader.Messages.AddNew();
			twMessage.EM_ApplicationReference = "TWC";
			AssertLicensingMessageInfo(twMessage, MessageTypeList.Codes._101, "101", "123-3");
			AssertLicensingMessageInfo(twMessage, MessageTypeList.Codes._201, "201", "123-3");
			AssertLicensingMessageInfo(twMessage, MessageTypeList.Codes._207, "207", "123-3");
			AssertLicensingMessageInfo(twMessage, MessageTypeList.Codes._301, "301", "123-3");
			AssertLicensingMessageInfo(twMessage, MessageTypeList.Codes._31A, "31A", "123-3");
			AssertLicensingMessageInfo(twMessage, MessageTypeList.Codes._31D, "31D", "123-3");
			AssertLicensingMessageInfo(twMessage, MessageTypeList.Codes._401, "401", "123-3");
			AssertLicensingMessageInfo(twMessage, MessageTypeList.Codes._601, "601", "123-3");
			AssertLicensingMessageInfo(twMessage, MessageTypeList.Codes._603, "603", "123-3");
		}

		[ExpectNoExceptions]
		void AssertLicensingMessageInfo(TWMessage message, ZString originalMessageType, ZString expectedMessageType, ZString expectedMailBox)
		{
			message.EM_MessageType = originalMessageType;
			var twMessageInfo = TWMessageHelper.CreateTWMessageInfoFromTWMessage(message);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(twMessageInfo.MailBox, NUnit.Framework.Is.EqualTo(expectedMailBox), "MailBox");
				NUnit.Framework.Assert.That(twMessageInfo.MessageType, NUnit.Framework.Is.EqualTo(expectedMessageType), "Message Type");
				NUnit.Framework.Assert.That(twMessageInfo.EntryNumber, NUnit.Framework.Is.EqualTo("222").Using(CustomComparers.TypeComparison), "Entry Number");
				NUnit.Framework.Assert.That(twMessageInfo.EntryNumberType, NUnit.Framework.Is.EqualTo("EXP").Using(CustomComparers.TypeComparison), "Entry Number Type");
				NUnit.Framework.Assert.That(twMessageInfo.StaffCode, NUnit.Framework.Is.EqualTo("111").Using(CustomComparers.TypeComparison), "Staff Code");
				NUnit.Framework.Assert.That(twMessageInfo.CompanyID, NUnit.Framework.Is.EqualTo(GlbCompany.CurrentCompany.GC_Code), "Company ID");
				NUnit.Framework.Assert.That(twMessageInfo.PasswordType, NUnit.Framework.Is.EqualTo(ZString.Empty), "Password Type");
			});
		}

		[ExpectNoExceptions]
		public void TestSerialize()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.New<GlbCompany>();
			company.GC_Code = "CO1";
			company.Branches.AddNew().GB_Code = "BR1";
			GlbCompany.CurrentCompany.GC_Code = company.GC_Code;
			var staff = factory.New<GlbStaff>();
			staff.GS_Code = "111";
			var extPassword = factory.New<GlbExternalPassword>();
			extPassword.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			extPassword.GP_MailBoxID = "123-3";
			extPassword.GP_UserID = "111";
			extPassword.GP_GS = staff.PK;
			extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_GS_NKCusAgent = "111";
			declaration.JE_CustomsProfile = "123-3";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "222";
			var twMessage1 = cusEntryHeader.Messages.AddNew();
			twMessage1.EM_ApplicationReference = "TWC";
			twMessage1.EM_MessageType = "ICS";
			var twMessageInfo = TWMessageHelper.Serialize(TWMessageHelper.CreateTWMessageInfoFromTWMessage(twMessage1));
			NUnit.Framework.Assert.That(twMessageInfo, NUnit.Framework.Is.EqualTo("<TWMessageInfo>\r\n  <MailBox>TWC</MailBox>\r\n  <MessageType>ICS</MessageType>\r\n  <EntryNumber>222</EntryNumber>\r\n  <EntryNumberType>EXP</EntryNumberType>\r\n  <StaffCode>111</StaffCode>\r\n  <CompanyID>CO1</CompanyID>\r\n  <PasswordType>TVA</PasswordType>\r\n</TWMessageInfo>").Using(CustomComparers.TypeComparison), "Serialize");

			var twMessage2 = cusEntryHeader.Messages.AddNew();
			twMessage2.EM_ApplicationReference = "TW  C";
			twMessage2.EM_MessageType = "ICS";
			var twMessageInfo2 = TWMessageHelper.Serialize(TWMessageHelper.CreateTWMessageInfoFromTWMessage(twMessage2), true);
			NUnit.Framework.Assert.That(twMessageInfo2, NUnit.Framework.Is.EqualTo("<TWMessageInfo><MailBox>TW  C</MailBox><MessageType>ICS</MessageType><EntryNumber>222</EntryNumber><EntryNumberType>EXP</EntryNumberType><StaffCode>111</StaffCode><CompanyID>CO1</CompanyID><PasswordType>TVA</PasswordType></TWMessageInfo>").Using(CustomComparers.TypeComparison), "Serialize should remove the indent and line break, but it needs keep the space of values.");
		}

		[ExpectNoExceptions]
		public void TestFillEntryNumberPlaceHolder()
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message1 = entryHeader.Messages.AddNew();
			message1.EM_MessageText = "<A><!-- placeholder:EntryNumber --></A>";
			TWMessageHelper.FillMessagePlaceHolder(message1, "<!-- placeholder:EntryNumber -->", "AA");
			NUnit.Framework.Assert.That(message1.EM_MessageText, NUnit.Framework.Is.EqualTo("<A>AA</A>").Using(CustomComparers.TypeComparison));
			message1.EM_MessageText = "<A>&lt;&lt;FUNCTIONAL REFERENCE ID PLACE HOLDER&gt;&gt;</A>";
			TWMessageHelper.FillMessagePlaceHolder(message1, "<<FUNCTIONAL REFERENCE ID PLACE HOLDER>>", "CC");
			NUnit.Framework.Assert.That(message1.EM_MessageText, NUnit.Framework.Is.EqualTo("<A>CC</A>").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUpdateAsycudaHeaderMessageStatusFromBills()
		{
			var factory = new BusinessObjectFactory();
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = manifestHeader.Bills.AddNew();
			bill1.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Acknowledged;
			var bill2 = manifestHeader.Bills.AddNew();
			bill2.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Acknowledged;
			var bill3 = manifestHeader.Bills.AddNew();
			bill3.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Acknowledged;
			TWMessageHelper.UpdateAsycudaHeaderMessageStatusFromBills(manifestHeader);
			NUnit.Framework.Assert.That(manifestHeader.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Acknowledged).Using(CustomComparers.TypeComparison), "all bill of ABL_MessageStatus are ACK");

			bill1.ABL_MessageStatus = TWMessageStatusCodeList.Codes.AwaitingResponse;
			bill2.ABL_MessageStatus = TWMessageStatusCodeList.Codes.AwaitingResponse;
			bill3.ABL_MessageStatus = TWMessageStatusCodeList.Codes.AwaitingResponse;
			TWMessageHelper.UpdateAsycudaHeaderMessageStatusFromBills(manifestHeader);
			NUnit.Framework.Assert.That(manifestHeader.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.AwaitingResponse).Using(CustomComparers.TypeComparison), "all bill of ABL_MessageStatus are AWR");

			bill1.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Sent;
			bill2.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Sent;
			bill3.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Sent;
			TWMessageHelper.UpdateAsycudaHeaderMessageStatusFromBills(manifestHeader);
			NUnit.Framework.Assert.That(manifestHeader.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Sent).Using(CustomComparers.TypeComparison), "all bill of ABL_MessageStatus are SNT");

			bill1.ABL_MessageStatus = TWMessageStatusCodeList.Codes.Unknown;
			TWMessageHelper.UpdateAsycudaHeaderMessageStatusFromBills(manifestHeader);
			NUnit.Framework.Assert.That(manifestHeader.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.Unknown).Using(CustomComparers.TypeComparison), "The ABL_MessageStatus of one of the bills is UNK");

			bill2.ABL_MessageStatus = TWMessageStatusCodeList.Codes.NotSent;
			TWMessageHelper.UpdateAsycudaHeaderMessageStatusFromBills(manifestHeader);
			NUnit.Framework.Assert.That(manifestHeader.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.NotSent).Using(CustomComparers.TypeComparison), "The ABL_MessageStatus of one of the bills is NOT");

			bill3.ABL_MessageStatus = TWMessageStatusCodeList.Codes.TransmissionError;
			TWMessageHelper.UpdateAsycudaHeaderMessageStatusFromBills(manifestHeader);
			NUnit.Framework.Assert.That(manifestHeader.AMA_MessageStatus, NUnit.Framework.Is.EqualTo(TWMessageStatusCodeList.Codes.TransmissionError).Using(CustomComparers.TypeComparison), "The ABL_MessageStatus of one of the bills is ERR");
		}

		[ExpectNoExceptions]
		public void TestUpdateAsycudaHeaderCustomsStatusFromBills()
		{
			var factory = new BusinessObjectFactory();
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillStatus = Constants.CustomsManifestStatus.AP;
			var bill2 = manifestHeader.Bills.AddNew();
			bill2.ABL_BillStatus = Constants.CustomsManifestStatus.AP;
			var bill3 = manifestHeader.Bills.AddNew();
			bill3.ABL_BillStatus = Constants.CustomsManifestStatus.AP;
			TWMessageHelper.UpdateAsycudaHeaderCustomsStatusFromBills(manifestHeader);
			var registrationEntryNumber = CusEntryNumber.LoadOrCreate(manifestHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, manifestHeader.AMA_RN_NKCountry);
			NUnit.Framework.Assert.That(registrationEntryNumber.CE_EntryStatus, NUnit.Framework.Is.EqualTo(Constants.CustomsManifestStatus.AP).Using(CustomComparers.TypeComparison), "all bill of ABL_BillStatus are AP");

			bill1.ABL_BillStatus = Constants.CustomsManifestStatus.AK;
			TWMessageHelper.UpdateAsycudaHeaderCustomsStatusFromBills(manifestHeader);
			NUnit.Framework.Assert.That(registrationEntryNumber.CE_EntryStatus, NUnit.Framework.Is.EqualTo(Constants.CustomsManifestStatus.AK).Using(CustomComparers.TypeComparison), "The ABL_BillStatus of one of the bills is AK");

			bill2.ABL_BillStatus = Constants.CustomsManifestStatus.EX;
			TWMessageHelper.UpdateAsycudaHeaderCustomsStatusFromBills(manifestHeader);
			NUnit.Framework.Assert.That(registrationEntryNumber.CE_EntryStatus, NUnit.Framework.Is.EqualTo(Constants.CustomsManifestStatus.EX).Using(CustomComparers.TypeComparison), "The ABL_BillStatus of one of the bills is EX");

			bill3.ABL_BillStatus = Constants.CustomsManifestStatus.RE;
			TWMessageHelper.UpdateAsycudaHeaderCustomsStatusFromBills(manifestHeader);
			NUnit.Framework.Assert.That(registrationEntryNumber.CE_EntryStatus, NUnit.Framework.Is.EqualTo(Constants.CustomsManifestStatus.RE).Using(CustomComparers.TypeComparison), "The ABL_BillStatus of one of the bills is RE");
		}
	}
}

using System.Globalization;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business.Testing
{
	static class TestMessageFactory
	{
		public static TWMessage GetIncomingCustomsDeliveryNotificationEDIMessage(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "AA";
			var entryInstruction1 = declaration.CusEntryInstruction;
			entryInstruction1.CEI_Style = "B1";
			entryInstruction1.CEI_CustomsOffice = "AA";
			entryInstruction1.CEI_DateForDuty = new ZDateTime(2019, 01, 01);
			entryInstruction1.CEI_BoxNumber = "123";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction1.PK;
			entryHeader.CH_JE = declaration.PK;
			var entryNumber = factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			entryNumber.CE_EntryNum = "AAB1082348";
			factory.Save();

			var messageText = TWMessageProcessorTest.GetTWNotification("SNT", "IMP", entryHeader.EntryNumber, "NUM1");
			return CreateMessage(factory, messageText, MessageTypeList.Codes.ICD);
		}

		public static TWMessage GetIncomingTranshipmentProcessorEDIMessage(BusinessObjectFactory factory)
		{
			var cusInBondHead = factory.NewWithValidTestData<CusInBondHeader>();
			cusInBondHead.ReceiptOffice = "AA";
			cusInBondHead.UnladingOffice = "BB";
			cusInBondHead.TW_BoxNumber = "123";
			var entryNumber = factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = cusInBondHead.PK;
			entryNumber.CE_EntryType = "TRS";
			entryNumber.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			entryNumber.CE_EntryNum = "AAB1082348";
			factory.Save();

			return CreateMessage(factory, GetMessageText("N5302.xml", cusInBondHead.EntryNumber, null, "C1", null, null), MessageTypeList.Codes.TRN);
		}

		public static TWMessage GetIncomingManifestMessageProcessorEDIMessage(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Voyage = "5X 0059";
			header.AMA_TransportMode = "AIR";
			header.AMA_MasterBill = "406-51754603";

			var bill = header.Bills.AddNew();
			var cusNum = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = bill.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "FHM";
			cusNum.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryNum = "11233527SW1812100001";
			cusNum.CE_EntryStatus = ZString.Empty;
			factory.Save();

			var testMessage = factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "TWIN1";
			testMessage.EM_MessageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML("Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.N5108.N5108.xml");
			testMessage.EM_MessageType = "FHR";

			return testMessage;
		}

		public static TWMessage GetIncomingManifestDeliveryNotificationProcessorEDIMessage(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cusNum = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = bill.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "FHM";
			cusNum.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryNum = "1234567";
			cusNum.CE_EntryStatus = ZString.Empty;
			factory.Save();

			return CreateMessage(factory, TWMessageProcessorTest.GetTWNotification("SNT", "FHM", "1234567", "NUM1"), "FCF");
		}

		public static TWMessage GetIncomingProcessTransferApplicationFromEHubNotificationEDIMessage(BusinessObjectFactory factory)
		{
			var cusInBondHead = factory.NewWithValidTestData<CusInBondHeader>();
			cusInBondHead.ReceiptOffice = "AA";
			cusInBondHead.UnladingOffice = "BB";
			cusInBondHead.TW_BoxNumber = "123";
			var cusNum = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = cusInBondHead.PK;
			cusNum.CE_EntryType = "TRS";
			cusNum.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryNum = "INNO1";
			factory.Save();

			return CreateMessage(factory, TWMessageProcessorTest.GetTWNotification("SNT", "TRS", cusInBondHead.EntryNumber, "00000000001004230159"), MessageTypeList.Codes.TRA);
		}

		public static TWMessage GetIncomingControllingAgencyNotificationProcessorEDIMessage(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader1.TW1_FunctionalReferenceId = "22099131002309220001";
			var messageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader2.TW1_FunctionalReferenceId = "23322708001106180001";
			factory.Save();

			return CreateMessage(factory, TWMessageProcessorTest.GetTWNotification("SNT", "NXM", "22099131002309220001", "00000000001305020580", "101"), "101");
		}

		public static (EDIMessage IncomingMessage, CusTWControllingMessageHeader ControllingMessageHeader) GetIncomingLicensingMessageProcessorEDIMessage(BusinessObjectFactory factory, ZString controllingMessageType, ZString functionalReferenceId, ZString entryNum, ZString eMMessageType, ZString xmlFileName, ZString expectedStatus)
		{
			var decl = factory.New<JobDeclaration>();
			decl.JE_CustomsOffice = "AA";
			var entryInstruction = decl.CusEntryInstruction;
			var controllingMessageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = controllingMessageType;
			controllingMessageHeader.TW1_FunctionalReferenceId = functionalReferenceId;
			var entryHeader = decl.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			if (!entryNum.IsEmpty)
			{
				var entryNumber = factory.NewWithValidTestData<CusEntryNumber>();
				entryNumber.CE_ParentID = entryHeader.PK;
				entryNumber.CE_Category = "CUS";
				entryNumber.CE_EntryType = decl.JE_MessageType;
				entryNumber.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
				entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
				entryNumber.CE_EntryNum = entryNum;
			}
			factory.Save();

			var testMessage = factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "TWIN1";
			testMessage.EM_MessageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML($"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.{xmlFileName}.xml");
			testMessage.EM_MessageType = eMMessageType;

			return (testMessage, controllingMessageHeader);
		}

		public static TWMessage GetIncomingCustomsDeclarationMessageProcessorEDIMessage(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "AA";

			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = "B1";
			entryInstruction.CEI_CustomsOffice = "AA";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 01, 01);
			entryInstruction.CEI_BoxNumber = "123";

			var cusHead = declaration.CustomsEntryHeaders.AddNew();
			cusHead.CH_CEI_Instruction = entryInstruction.PK;
			cusHead.CH_JE = declaration.PK;
			var cusNum = factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = cusHead.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "IMP";
			cusNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryNum = "AAB1082348";
			factory.Save();

			return CreateMessage(factory, GetMessageText("N5110.xml", cusNum.CE_EntryNum, null, null, null, null), "TPC");
		}

		static string GetMessageText(string name, string number, string procedure, string nameCode, string borderTransportMeans, string validationCode, string releaseDateTime = null, string dueDateTime = null)
		{
			var messageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile." + name);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(messageText);
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace("a", xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);
			if (!string.IsNullOrEmpty(number))
			{
				SetNodeValueByPath(xmlDocument, "{0}Response/{0}Declaration/{0}ID", number, nameSpace);
				SetNodeValueByPath(xmlDocument, "{0}Declaration/{0}ID", number, nameSpace);
			}

			if (procedure != null)
			{
				SetNodeValueByPath(xmlDocument, "{0}Response/{0}Declaration/{0}GovernmentProcedure/{0}tw_TransportTypeCode", procedure, nameSpace);
				SetNodeValueByPath(xmlDocument, "{0}Declaration/{0}GovernmentProcedure/{0}tw_TransportTypeCode", procedure, nameSpace);
			}

			if (nameCode != null)
			{
				SetNodeValueByPath(xmlDocument, "{0}Response/{0}Status/{0}NameCode", nameCode, nameSpace);
				SetNodeValueByPath(xmlDocument, "{0}Response/{0}Declaration/{0}GoodsShipment/{0}GovernmentAgencyGoodsItem/{0}Status/{0}NameCode", nameCode, nameSpace);
			}

			if (borderTransportMeans != null)
			{
				SetNodeValueByPath(xmlDocument, "{0}Response/{0}Declaration/{0}BorderTransportMeans/{0}TypeCode", borderTransportMeans, nameSpace);
			}

			if (validationCode != null)
			{
				SetNodeValueByPath(xmlDocument, "{0}Response/{0}Declaration/{0}GoodsShipment/{0}GovernmentAgencyGoodsItem/{0}Error/{0}ValidationCode", validationCode, nameSpace);
			}

			if (releaseDateTime != null)
			{
				SetNodeValueByPath(xmlDocument, "{0}Response/{0}Status/{0}ReleaseDateTime", releaseDateTime, nameSpace);
			}

			if (dueDateTime != null)
			{
				SetNodeValueByPath(xmlDocument, "{0}Response/{0}Declaration/{0}DutyTaxFee/{0}Payment/{0}DueDateTime", dueDateTime, nameSpace);
			}

			return xmlDocument.OuterXml;
		}

		static void SetNodeValueByPath(XmlDocument xmlDocument, string xpathFormat, string value, XmlNamespaceManager nameSpace)
		{
			var node = xmlDocument.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, xpathFormat, "a:"), nameSpace);
			if (node != null)
			{
				node.InnerText = value;
			}
		}

		static TWMessage CreateMessage(BusinessObjectFactory factory, string messageText, string messageType)
		{
			var testMessage = factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "TWC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageNum = "TWIN1";
			testMessage.EM_MessageText = messageText;
			testMessage.EM_MessageType = messageType;
			return testMessage;
		}
	}
}

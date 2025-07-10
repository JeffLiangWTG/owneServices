using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX5901MessageSendingObject))]
	sealed class NX5901MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestSerializeToMessageString()
		{
			var message = MessageSendingObject;
			var entryInstruction = declaration.CusEntryInstruction;
			cusHead.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_Style = "G1";
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			message.SupportingDocuments.RemoveAndDeleteAll();
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), MessageConstants.DocumentTypes.PKL);
			var doc2 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), MessageConstants.DocumentTypes.TDM);
			MessageSendingObject.ContactOffice = "CO1";
			var docLine1 = messageSendingObject.SupportingDocuments.AddNew();
			docLine1.DocumentNo = "DocumentNo1";
			docLine1.Remarks = "Remarks1";
			docLine1.EDoc = doc1.UniqueKey;
			docLine1.ControllingAgency = "C1";
			var docLine2 = messageSendingObject.SupportingDocuments.AddNew();
			docLine2.DocumentNo = "DocumentNo2";
			docLine2.Remarks = "Remarks2";
			docLine2.EDoc = doc2.UniqueKey;
			docLine2.ControllingAgency = "C2";
			var xml = message.SerializeToMessageString();
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:FunctionalReferenceID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo(MessageConstants.FunctionalReferenceIDPlaceHolder));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:ID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("NO1"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:TypeCode", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("G1"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:ContactOffice/a:ID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("CO1"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)));
			var nodes = xmlDocument.SelectNodes("a:Declaration/a:GoodsShipment/a:AdditionalDocument", nameSpace).Cast<XmlNode>();
			NUnit.Framework.Assert.That(nodes.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:ID", nameSpace)[0].InnerText == "DocumentNo1"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:tw_Content", nameSpace)[0].InnerText == "Remarks1"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:tw_ImageFileFormat", nameSpace)[0].InnerText == "PDF"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:tw_ImageFileName", nameSpace)[0].InnerText == "sample.pdf"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:tw_SizeMeasure", nameSpace)[0].InnerText == "28451"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:TypeCode", nameSpace)[0].InnerText == "1"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:ResponsibleGovernmentAgency/a:ID", nameSpace)[0].InnerText == "C1"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:ID", nameSpace)[0].InnerText == "DocumentNo2"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:tw_Content", nameSpace)[0].InnerText == "Remarks2"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:tw_ImageFileFormat", nameSpace)[0].InnerText == "XLS"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:tw_ImageFileName", nameSpace)[0].InnerText == "Test.xls"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:tw_SizeMeasure", nameSpace)[0].InnerText == "13824"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:TypeCode", nameSpace)[0].InnerText == "5"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(nodes.Cast<XmlNode>().Any(x => x.SelectNodes("a:ResponsibleGovernmentAgency/a:ID", nameSpace)[0].InnerText == "C2"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure/a:tw_TransportTypeCode", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:ResponsibleGovernmentAgency/a:ID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("CU"));
			message = new NX5901MessageSendingObject(cusHead);
			message.SupportingDocuments.RemoveAndDeleteAll();
			declaration.JE_MessageType = ZString.Empty;
			entryInstruction.CEI_Style = ZString.Empty;
			xml = message.SerializeToMessageString();
			xmlDocument.LoadXml(xml);
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:FunctionalReferenceID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo(MessageConstants.FunctionalReferenceIDPlaceHolder));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:ID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("NO1"));
			NUnit.Framework.Assert.That(string.IsNullOrEmpty(xmlDocument.SelectSingleNode("a:Declaration/a:TypeCode", nameSpace).InnerText), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:ContactOffice", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:ResponsibleGovernmentAgency/a:ID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("CU"));
		}

		[ExpectNoExceptions]
		public void TestData()
		{
			INX5901Declaration message = MessageSendingObject;
			NUnit.Framework.Assert.That(message.FunctionalReferenceID, NUnit.Framework.Is.EqualTo(MessageConstants.FunctionalReferenceIDPlaceHolder).Using(CustomComparers.TypeComparison));
			cusHead.EntryNumber = "";
			NUnit.Framework.Assert.That(message.ID, NUnit.Framework.Is.EqualTo(MessageConstants.EntryNumberPlaceHolder).Using(CustomComparers.TypeComparison));
			cusHead.EntryNumber = "NO1";
			NUnit.Framework.Assert.That(message.ID, NUnit.Framework.Is.EqualTo("NO1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.TypeCode, NUnit.Framework.Is.EqualTo("G2").Using(CustomComparers.TypeComparison));
			var entryInstruction = declaration.CusEntryInstruction;
			cusHead.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_Style = "G1";
			NUnit.Framework.Assert.That(message.TypeCode, NUnit.Framework.Is.EqualTo("G1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(IAdditionalDocument)));
			NUnit.Framework.Assert.That(message.ContactOffice, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.GoodsShipment, NUnit.Framework.Is.Not.EqualTo(default(IGoodsShipment)));
			NUnit.Framework.Assert.That(message.GoodsShipment.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(2));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			NUnit.Framework.Assert.That(message.GovernmentProcedure.TransportTypeCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			NUnit.Framework.Assert.That(message.GovernmentProcedure.TransportTypeCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			declaration.JE_MessageType = ZString.Empty;
			NUnit.Framework.Assert.That(message.GovernmentProcedure, NUnit.Framework.Is.EqualTo(default(IGovernmentProcedure)));
			NUnit.Framework.Assert.That(message.PreviousDocument, NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
			NUnit.Framework.Assert.That(message.ResponsibleGovernmentAgency, NUnit.Framework.Is.EqualTo("CU").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMessageType()
		{
			NUnit.Framework.Assert.That(MessageSendingObject.MessageType, NUnit.Framework.Is.EqualTo(MessageTypeList.Codes.ADM).Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			return new NX5901MessageSendingObject(entryHeader);
		}

		NX5901MessageSendingObject MessageSendingObject
		{
			get
			{
				if (messageSendingObject == null)
				{
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
					declaration.JE_GS_NKCusAgent = "TT";
					declaration.JE_CustomsProfile = "123-3";
					cusHead = Factory.NewWithValidTestData<CusEntryHeader>();
					cusHead.CH_JE = declaration.PK;
					cusHead.EntryNumber = "NO1";
					cusHead.CH_Status = "AWO";
					cusHead.CH_MessageType = "";
					var cusEntryInstruction = declaration.CusEntryInstruction;
					cusHead.CH_CEI_Instruction = cusEntryInstruction.PK;
					cusEntryInstruction.CEI_Style = "G2";
					var staff = Factory.NewWithValidTestData<GlbStaff>();
					staff.GS_Code = "TT";
					var company = GlbCompany.CurrentCompany;
					var extPassword1 = Factory.New<GlbExternalPassword>();
					extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
					extPassword1.GP_GC = company.PK;
					extPassword1.GP_MailBoxID = "123-3";
					extPassword1.GP_UserID = "001";
					extPassword1.GP_GS = staff.PK;
					extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
					messageSendingObject = new NX5901MessageSendingObject(cusHead);
					messageSendingObject.ContactOffice = "A";
					var docLine1 = messageSendingObject.SupportingDocuments.AddNew();
					var docLine2 = messageSendingObject.SupportingDocuments.AddNew();
				}

				return messageSendingObject;
			}
		}

		NX5901MessageSendingObject messageSendingObject;
		JobDeclaration declaration;
		CusEntryHeader cusHead;
	}
}

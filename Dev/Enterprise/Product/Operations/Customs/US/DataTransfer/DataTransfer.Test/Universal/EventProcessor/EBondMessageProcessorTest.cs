using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class EBondMessageProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendMessageToBondStatusNotificationGroup()
		{
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "jason@test.email.com";
			USCustomsDataRegistry.Instance.BondStatusNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupZZ1.PK.ToGuid());
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = EBondMssageProcessorFactoryTest.CreateDeclarationForEBondMessage(Factory);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.SubstitutionBond;
			declaration.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.BondRejectedByCBPSeeAttachedErrors;
			var document = EBondMssageProcessorFactoryTest.GetEBondInterchangeBodyXml();
			var message = Factory.New<EBondEDIMessage>();
			message.EM_MessageNum = "EBOND190320";
			message.EM_MessageText = document.GetElementsByTagName("UniversalEvent")[0].OuterXml;
			Factory.Save();
			var logInformation = new LoggingInformation();
			var tracker = new XmlSessionTracker(logInformation);
			AssertNull("Pre condition", declaration.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).FirstOrDefault());
			var processor = ObjectFactory.New<IEBondMessageProcessor>(tracker);
			processor.Process(Factory, logInformation, message.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Logger", @"Match Declaration B00001398 from the message EBOND190320.
Update bond data on Declaration B00001398 from the message EBOND190320.
Create email to group BondStatusNotificationGroup from the message EBOND190320.", tracker.ToString());
				AssertEquals("Should update EM_Status.", EDIMessage.Status.Received, message.EM_Status);
				AssertEquals("Should update US_BondDesignationCode.", BondDesignationCodeList.Codes.BasicBond, declaration.US_BondDesignationCode);
				AssertEquals("Should update US_InsuranceDisposition.", InsuranceDispositionCodeList.Codes.AcceptedByCBP, declaration.US_InsuranceDisposition);
				AssertEquals("Should update EM_ApplicationReference.", Constants.EBond.ApplicationReferences.Accepted, message.EM_ApplicationReference);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains("jason@test.email.com"));
				var expectedHtmlContent = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\US\DataTransfer\DataTransfer.Test\Testing\USEBondEmailContent.html"));
				AssertEquals("Body Of Html.", expectedHtmlContent, email.Body);
				var eBondLog = declaration.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).FirstOrDefault();
				AssertNotNull(eBondLog);
				AssertEquals("|MST=eBond Status|RES=B06 Test Surety Response Description", eBondLog.SL_Reference);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "JOE";
			staff1.GS_LoginName = "joe";
			staff1.GS_EmailAddress = "joe.bloggs@wisetechglobal.com";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "JO2";
			staff2.GS_EmailAddress = "jo2.bloggs@wisetechglobal.com";
			staff2.GS_LoginName = "jo2";
			var declaration = EBondMssageProcessorFactoryTest.CreateDeclarationForEBondMessage(Factory);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.SubstitutionBond;
			declaration.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.BondRejectedByCBPSeeAttachedErrors;
			var sentMessage1 = Factory.NewWithValidTestData<EBondEDIMessage>();
			sentMessage1.EM_ApplicationCode = ApplicationCodeList.Codes.USeBond;
			sentMessage1.EM_MessageType = EDIInterchangeTypeList.Codes.XDC;
			sentMessage1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			sentMessage1.EM_Status = EDIMessage.Status.Sent;
			sentMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage1.EM_LinkTable = CusEntryHeader.Schema.TableName;
			sentMessage1.EM_LinkUniqueID = declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;
			sentMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			sentMessage1.EM_SystemCreateUser = staff1.GS_Code;
			var sentMessage2 = Factory.NewWithValidTestData<EBondEDIMessage>();
			sentMessage2.EM_ApplicationCode = ApplicationCodeList.Codes.USeBond;
			sentMessage2.EM_MessageType = EDIInterchangeTypeList.Codes.XDC;
			sentMessage2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			sentMessage2.EM_Status = EDIMessage.Status.Sent;
			sentMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage2.EM_LinkTable = CusEntryHeader.Schema.TableName;
			sentMessage2.EM_LinkUniqueID = declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;
			sentMessage2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			sentMessage2.EM_SystemCreateUser = staff2.GS_Code;
			var document = EBondMssageProcessorFactoryTest.GetEBondInterchangeBodyXml();
			var message = Factory.NewWithValidTestData<EBondEDIMessage>();
			message.EM_MessageNum = "EBOND190320";
			message.EM_MessageText = document.GetElementsByTagName("UniversalEvent")[0].OuterXml;
			var sentMessage3 = Factory.NewWithValidTestData<EBondEDIMessage>();
			sentMessage3.EM_ApplicationCode = ApplicationCodeList.Codes.USeBond;
			sentMessage3.EM_MessageType = EDIInterchangeTypeList.Codes.XDC;
			sentMessage3.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			sentMessage3.EM_Status = EDIMessage.Status.Sent;
			sentMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage3.EM_LinkTable = CusEntryHeader.Schema.TableName;
			sentMessage3.EM_LinkUniqueID = declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;
			sentMessage3.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(1);
			sentMessage3.EM_SystemCreateUser = staff2.GS_Code;
			Factory.Save();
			var logInformation = new LoggingInformation();
			var tracker = new XmlSessionTracker(logInformation);
			AssertNull("Pre condition", declaration.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).FirstOrDefault());
			var processor = ObjectFactory.New<IEBondMessageProcessor>(tracker);
			processor.Process(Factory, logInformation, message.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Logger", @"Match Declaration B00001398 from the message EBOND190320.
Update bond data on Declaration B00001398 from the message EBOND190320.
Create email to joe.bloggs@wisetechglobal.com from the message EBOND190320.", tracker.ToString());
				AssertEquals("Should update EM_Status.", EDIMessage.Status.Received, message.EM_Status);
				AssertEquals("Should update US_BondDesignationCode.", BondDesignationCodeList.Codes.BasicBond, declaration.US_BondDesignationCode);
				AssertEquals("Should update US_InsuranceDisposition.", InsuranceDispositionCodeList.Codes.AcceptedByCBP, declaration.US_InsuranceDisposition);
				AssertEquals("Should update EM_ApplicationReference.", Constants.EBond.ApplicationReferences.Accepted, message.EM_ApplicationReference);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains("joe.bloggs@wisetechglobal.com"));
				var expectedHtmlContent = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\US\DataTransfer\DataTransfer.Test\Testing\USEBondEmailContent.html"));
				AssertEquals("Body Of Html.", expectedHtmlContent, email.Body);
				var eBondLog = declaration.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).FirstOrDefault();
				AssertNotNull(eBondLog);
				AssertEquals("|MST=eBond Status|RES=B06 Test Surety Response Description", eBondLog.SL_Reference);
			});
		}
	}
}

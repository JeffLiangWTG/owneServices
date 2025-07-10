using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	public class eBondEventProcessorTest : TestCaseWithFactory
	{
		public void TestProcessor()
		{
			CreateTestMessage();
			var declaration = CreateJobDeclarationForSendMessage();
			AssertEquals(BondDispositionCodeList.Codes.CVB, declaration.US_BondDispositionCode);
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var list = Env.OutgoingCustomsMailManager.EmailsCreated;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(declaration.CusAgent.GS_EmailAddress));
			AssertNotNull(email);
			AssertEquals(true, email.Body.Contains(string.Format("<a href=\"{0}\">{1}</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration), declaration.JobNumber)));
			AssertContains("principal name", "enb", email.Subject.ToLower());
			declaration.Messages.Reload(true);
			AssertEquals("message linking to a job", 1, declaration.Messages.Count);
			var newFactory = new BusinessObjectFactory();
			var reloadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("message has been saved", 1, reloadedDeclaration.Messages.Count);
			var log = reloadedDeclaration.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.MessageReceived.ToString());
			AssertNotNull(log);
			AssertEquals("UpdateJobDeclaration", BondDispositionCodeList.Codes.ENB, reloadedDeclaration.US_BondDispositionCode);
			AssertEquals("UpdateJobDeclaration", "123456789", reloadedDeclaration.US_CBPBondNo);
			AssertEquals("UpdateJobDeclaration", 12345m, reloadedDeclaration.US_BondAmount);
		}

		void CreateTestMessage()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_MessageText = "B018888XJ5BS                                               286                  " +
				"B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " +
				"B2THIS IS A TEST                                                                " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
				"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
				"20107XJ5C1234578                                                                " +
				"3034 NN-NNNNNNNXXPrincipal Name                                                 " +
				"35EI YYDDPP-NNNNNCo-principal Name1                                             " +
				"35EI YYDDPP-NNNNNCo-principal Name2                                             " +
				"35EI YYDDPP-NNNNNCo-principal Name3                                             " +
				"36ANI111-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI222-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI333-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI444-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI555-NN-NNNN Bond User Name                          D112714121014          " +
				"40123777-88-9999Surety Name                              0000000005             " +
				"45111111-88-9999Surety Name                              0000000005             " +
				"45222222-88-9999Surety Name                              0000000005             " +
				"45333333-88-9999Surety Name                              0000000005             " +
				"Y  8888XJ5WR00005";
			Factory.Save();
		}

		public void TestEmailAddressFromRegistryBondStatusNotificationGroupWhenNoLinkJob()
		{
			var groupZZ5 = Factory.New<GlbGroup>();
			groupZZ5.GG_Code = "ZZ5";
			var staffZ1 = groupZZ5.Staff.AddNew();
			staffZ1.GS_Code = "Z5";
			staffZ1.GS_LoginName = "zT5";
			staffZ1.GS_EmailAddress = "test55@pretend.email.com";
			USCustomsDataRegistry.Instance.BondStatusNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupZZ5.PK.ToGuid());
			Factory.Save();
			var declaration = CreateJobDeclarationForSendMessage();
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_MessageText = "B018888XJ5BS                                                                    " +
				"B1A00000001 ETS CONT BOND IS SCHEDULED TO BE TERMINATED  1 0626191547           " +
				"10T8             031517000343838      071219A00000001                           " +
				"20107XJ5C5553333                                                                " +
				"Y  8888XJ5WR00002";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var emailNull = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(declaration.CusAgent.GS_EmailAddress));
			AssertNull(emailNull);
			var list = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("An email sent.", 1, list.Count);
			AssertEquals("Email address should get from the Registry BondStatusNotificationGroup.", true, list[0].Recipients.Contains("test55@pretend.email.com"));
		}

		[TestDate(2019, 11, 28)]
		public void TestAddSingleOrgCodeToEmailResponse()
		{
			var declaration = CreateJobDeclarationForSendMessage();
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_MessageText = "B018888XJ5BS                                                                    " +
				"B1A00000001 ETS CONT BOND IS SCHEDULED TO BE TERMINATED  1 0626191547           " +
				"10T8             031517000343838      071219A00000001                           " +
				"20107XJ5C1234578                                                                " +
				"Y  8888XJ5WR00002";
			var org01 = CreateImporterOrgHeaderWithBondDetail("ORG001", "Organisation 001", "A00000001", ApplicationCodeList.Codes.UsaInBond);
			var org02 = CreateImporterOrgHeaderWithBondDetail("ORG002", "Organisation 002", "A00000001", ApplicationCodeList.Codes.Unknown);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			RunUMIMessageProcesserManually();
			var list = Env.OutgoingCustomsMailManager.EmailsCreated;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(declaration.CusAgent.GS_EmailAddress));
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertContains("Org Code in Subject.", "for " + org01.OH_Code, email.Subject);
				AssertContains("Contains Org01.", string.Format("<td>Importer Name</td><td>{0}</td>", org01.OH_FullName), email.Body);
				AssertNotContains("No Org02.", org02.OH_FullName, email.Body);
				message.Reload();
				declaration.Messages.Reload(true);
				AssertEquals("Declaration has one linked message.", 1, declaration.Messages.Count);
				AssertEquals("Message links to JobDeclaration.", JobDeclaration.Schema.TableName, message.EM_LinkTable);
				AssertEquals("Message belongs to declaration.", declaration.PK, message.EM_LinkUniqueID);
			});
		}

		[TestDate(2019, 11, 28)]
		public void TestAddMultipleOrgCodesToEmailResponse()
		{
			var declaration = CreateJobDeclarationForSendMessage();
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_MessageText = "B018888XJ5BS                                                                    " +
				"B1A00000001 ETS CONT BOND IS SCHEDULED TO BE TERMINATED  1 0626191547           " +
				"10T8             031517000343838      071219A00000001                           " +
				"20107XJ5C1234578                                                                " +
				"Y  8888XJ5WR00002";
			var org01 = CreateImporterOrgHeaderWithBondDetail("ORG001", "Organisation 001", "A00000001", ApplicationCodeList.Codes.UsaInBond);
			var org02 = CreateImporterOrgHeaderWithBondDetail("ORG002", "Organisation 002", "A00000001", ApplicationCodeList.Codes.UsaInBond);
			var org03 = CreateImporterOrgHeaderWithBondDetail("ORG003", "Organisation 003", "A00000001", ApplicationCodeList.Codes.Unknown);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			RunUMIMessageProcesserManually();
			var list = Env.OutgoingCustomsMailManager.EmailsCreated;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(declaration.CusAgent.GS_EmailAddress));
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertContains("Org Code in Subject.", "for Multiple Organisations", email.Subject);
				AssertContains("Contains Org01.", string.Format("<td>Importer Name</td><td>{0}</td>", org01.OH_FullName), email.Body);
				AssertContains("Contains Org02.", string.Format("<td>Importer Name</td><td>{0}</td>", org02.OH_FullName), email.Body);
				AssertNotContains("No Org03.", org03.OH_FullName, email.Body);
				declaration.Messages.Reload(true);
				message.Reload();
				AssertEquals("Declaration has one linked message.", 1, declaration.Messages.Count);
				AssertEquals("Message links to JobDeclaration.", JobDeclaration.Schema.TableName, message.EM_LinkTable);
				AssertEquals("Message belongs to declaration.", declaration.PK, message.EM_LinkUniqueID);
			});
		}

		[TestDate(2019, 11, 28)]
		public void TestAddSingleOrgCodeToEmailResponseWithoutEntryNumber()
		{
			var groupZZ5 = Factory.New<GlbGroup>();
			groupZZ5.GG_Code = "ZZ5";
			var staffZ1 = groupZZ5.Staff.AddNew();
			staffZ1.GS_Code = "Z5";
			staffZ1.GS_LoginName = "zT5";
			staffZ1.GS_EmailAddress = "test55@pretend.email.com";
			USCustomsDataRegistry.Instance.BondStatusNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupZZ5.PK.ToGuid());
			var declaration = CreateJobDeclarationForSendMessage();
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_MessageText = "B018888XJ5BS                                                                    " +
				"B1A00000001 ETS CONT BOND IS SCHEDULED TO BE TERMINATED  1 0626191547           " +
				"10T8             031517000343838      071219A00000001                           " +
				"20107                                                                           " +
				"Y  8888XJ5WR00002";
			var org01 = CreateImporterOrgHeaderWithBondDetail("ORG001", "Organisation 001", "A00000001", ApplicationCodeList.Codes.UsaInBond);
			var org02 = CreateImporterOrgHeaderWithBondDetail("ORG002", "Organisation 002", "A00000001", ApplicationCodeList.Codes.Unknown);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			RunUMIMessageProcesserManually();
			var list = Env.OutgoingCustomsMailManager.EmailsCreated;
			var emailNull = list.FirstOrDefault(x => x.Recipients.Contains(org01.Addresses[0].OA_Email));
			CombineAssertions(() =>
			{
				AssertNull(emailNull);
				var email = list.FirstOrDefault(x => x.Recipients.Contains("test55@pretend.email.com"));
				AssertContains("Org Code in Subject.", "for " + org01.OH_Code, email.Subject);
				AssertContains("Contains Org01.", string.Format("<td>Importer Name</td><td>{0}</td>", org01.OH_FullName), email.Body);
				AssertNotContains("No Org02.", org02.OH_FullName, email.Body);
				AssertEquals("Linked Parent Table", OrgHeader.Schema.TableName, message.EM_LinkTable);
				AssertEquals("Linked Organization.", org01.PK, message.EM_LinkUniqueID);
			});
		}

		[TestDate(2019, 11, 28)]
		public void TestAddMultipleOrgCodesToEmailResponseWithoutEntryNumber()
		{
			var groupZZ5 = Factory.New<GlbGroup>();
			groupZZ5.GG_Code = "ZZ5";
			var staffZ1 = groupZZ5.Staff.AddNew();
			staffZ1.GS_Code = "Z5";
			staffZ1.GS_LoginName = "zT5";
			staffZ1.GS_EmailAddress = "test55@pretend.email.com";
			USCustomsDataRegistry.Instance.BondStatusNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupZZ5.PK.ToGuid());
			var declaration = CreateJobDeclarationForSendMessage();
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_MessageText = "B018888XJ5BS                                                                    " +
				"B1A00000001 ETS CONT BOND IS SCHEDULED TO BE TERMINATED  1 0626191547           " +
				"10T8             031517000343838      071219A00000001                           " +
				"20107                                                                           " +
				"Y  8888XJ5WR00002";
			var org01 = CreateImporterOrgHeaderWithBondDetail("ORG001", "Organisation 001", "A00000001", ApplicationCodeList.Codes.UsaInBond);
			var org02 = CreateImporterOrgHeaderWithBondDetail("ORG002", "Organisation 002", "A00000001", ApplicationCodeList.Codes.UsaInBond);
			var org03 = CreateImporterOrgHeaderWithBondDetail("ORG003", "Organisation 003", "A00000001", ApplicationCodeList.Codes.Unknown);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			RunUMIMessageProcesserManually();
			var list = Env.OutgoingCustomsMailManager.EmailsCreated;
			var emailNull = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(org01.Addresses[0].OA_Email));
			CombineAssertions(() =>
			{
				AssertNull(emailNull);
				var email = list.FirstOrDefault(x => x.Recipients.Contains("test55@pretend.email.com"));
				AssertContains("Org Code in Subject.", "for Multiple Organisations", email.Subject);
				AssertContains("Contains Org01.", string.Format("<td>Importer Name</td><td>{0}</td>", org01.OH_FullName), email.Body);
				AssertContains("Contains Org02.", string.Format("<td>Importer Name</td><td>{0}</td>", org02.OH_FullName), email.Body);
				AssertNotContains("No Org03.", org03.OH_FullName, email.Body);
				AssertEquals("Linked Parent Table", OrgHeader.Schema.TableName, message.EM_LinkTable);
				AssertCollectionContains("Linked Organization.", message.EM_LinkUniqueID.ToString(), new string[] { org01.PK.ToString(), org02.PK.ToString() });
			});
		}

		JobDeclaration CreateJobDeclarationForSendMessage()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Z8";
			staff.GS_EmailAddress = "dummy@email.com";
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = "07";
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DecEntryNumber = "C1234578";
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CVB;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.JE_GS_NKCusAgent = "Z8";
			Factory.Save();
			return declaration;
		}

		OrgHeader CreateImporterOrgHeaderWithBondDetail(string orgCode, string orgName, string bondNumber, string applicationCode)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = orgCode;
			org.OH_FullName = orgName;
			var bond = org.CusBondDetails.AddNew();
			bond.PW_BondNumber = bondNumber;
			bond.PW_ApplicationCode = applicationCode;
			var address = org.Addresses[0];
			address.OA_Address1 = "Test Address";
			address.OA_Email = orgCode + "@test.com";
			return org;
		}

		void RunUMIMessageProcesserManually()
		{
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("Generate new message", universalEventMessage);
			var serviceTaskLog = new UniversalDataBuss.Management.Testing.ServiceTaskLogForTesting();
			var manager = new UniversalDataBuss.Management.UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(universalEventMessage);
		}

		ZQuery GetMessageQuery()
		{
			var messageQuery = new ZQuery();
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Internal);
			messageQuery.AddToFilter(EDIMessageSchema.EM_GB, GlbBranch.CurrentBranch.PK);
			messageQuery.AddToFilter(EDIMessageSchema.EM_GE, GlbDepartment.CurrentDepartment.PK);
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
			messageQuery.MaximumRows = 100;
			return messageQuery;
		}
	}
}

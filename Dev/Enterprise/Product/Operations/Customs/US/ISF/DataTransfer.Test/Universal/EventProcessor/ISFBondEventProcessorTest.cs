using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Testing
{
	sealed class ISFBondEventProcessorTest : TestCaseWithFactory
	{
		public void TestProcessor()
		{
			CreateTestMessage();
			var cusISFHeader = CreateCusISFHeaderForSendMessage();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) =>
			{
				return emailToMatched.Subject.StartsWith("Customs eBond Status Notification");
			}));
			AssertNotNull(email);
			AssertEquals(true, email.Body.Contains(string.Format("<a href=\"{0}\">{1}</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(cusISFHeader), cusISFHeader.BF_JobReference)));
			AssertContains("principal name", "enb", email.Subject.ToLower());
			cusISFHeader.Messages.Reload(true);
			AssertEquals("message linking to a job", 1, cusISFHeader.Messages.Count);
			var newFactory = new BusinessObjectFactory();
			var newDeclaration = newFactory.Load(typeof(JobDeclaration), cusISFHeader.PK);
			AssertEquals("message has been saved", 1, cusISFHeader.Messages.Count);
			cusISFHeader.Logs.GetAllLogs().Reload(true);
			var log = cusISFHeader.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.MessageReceived.ToString());
			AssertNotNull(log);
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
			message.EM_MessageText = "B004701739BS                                                                    " +
				"B116S006BLS ENB NEW BOND HAS BEEN ADDED IN ACE           1 1107161717           " +
				"10B916      10000110716000118005110716      16S006BLS                           " +
				"124701739                                                                       " +
				"202  73994506084190                                                             " +
				"30EI 90 - 107748100CADENCE INSOLES LLC                                          " +
				"40856143 - 48 - 7151LEXON INSURANCE COMPANY                      10000          " +
				"Y  4701739BS00006                                                               ";
			Factory.Save();
		}

		CusISFHeader CreateCusISFHeaderForSendMessage()
		{
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "jason@test.email.com";
			USCustomsDataRegistry.Instance.BondStatusNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupZZ1.PK.ToGuid());
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var cusISFHeader = Factory.NewWithValidTestData<CusISFHeader>();
			cusISFHeader.BF_BondType = BondTypeList.Codes.SingleTransactionBond;
			cusISFHeader.BF_CustomsReference = "739-94506084190";
			cusISFHeader.BF_JobReference = "ISFT234322";
			Factory.Save();
			return cusISFHeader;
		}
	}
}

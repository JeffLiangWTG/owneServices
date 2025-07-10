using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ZA.ServiceTasks.Testing
{
	[TestedType(typeof(MessageSenderService))]
	sealed class MessageSenderServiceTest : ServiceTaskTestCase<MessageSenderService>
	{
		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestRunTask()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "ZA1";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AIR";
			company.GC_OH_OrgProxy = org.PK;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "AAA";
			Factory.Save();
			Env.Registry.ZACustoms.SetIsTestMode(branch, true);
			EDIMessage message1;
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.FillWithValidTestData();
				declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, "1234");
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.FillWithValidTestData();
				message1 = entryHeader.Messages.AddNew(typeof(CUSDECEDIMessage));
				message1.EM_LinkedObject = entryHeader;
				message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
				message1.EM_GB = GlbBranch.CurrentBranch.PK;
				message1.EM_MessageText = "UNH+1+CUSDEC:D:96B:UN:ZZZ01'";
				message1.EM_MessageOwner = "";
				message1.EM_IsTestMessage = true;
				message1.EM_MessageNum = "1";
				Factory.Save();
			}

			var task = new MessageSenderService();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
			CombineAssertions(() =>
			{
				var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("NumberOfInterchanges", 1, interchangesCreated.Length);
				var interchange = interchangesCreated[0];
				AssertEquals("Header", "UNB+UNOB:4+1234::SENDERID:SENDERSUBID+SARSDECT+20151030:0936+1++CUSDEC++1+TRADINGPARTNER+1'\n", interchange.EI_HeaderText);
				AssertEquals("Footer", "UNZ+1+1'", interchange.EI_FooterText);
				AssertEquals("Body", "UNH+1+CUSDEC:D:96B:UN:ZZZ01'\n", interchange.EI_BodyText);
				AssertEquals("Status", "HQU", interchange.EI_Status);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"ZA Customs messages outbound",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.SouthAfricanCustoms),
				};
			}
		}
	}
}

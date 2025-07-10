using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(US1ServiceTask))]
	sealed class US1ServiceTaskTest : ServiceTaskTestCase<US1ServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("US1", "United States ABI Interchange Inbound", "USC");
		}

		public void TestProcess()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "K#@";
			company.GC_Name = "TEST COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B$#";
			branch.GB_BranchName = "BKD NAME";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var incomingInterchange = Factory.New<CBPEDIInterchange>();
			incomingInterchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Receive;
			incomingInterchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			incomingInterchange.EI_From = "USC";
			incomingInterchange.EI_To = "XJ5";
			incomingInterchange.EI_HeaderText = "A3901SV9CAREDI08160601               89";
			incomingInterchange.EI_BodyText = "B018888XJ5ER                                               ~11417               " +
				"E08888XJ5 70022021         ENTRY HAS BEEN DELETED AS REQUESTED     01           " +
				"Y  8888XJ5ER00001                                                               ";
			incomingInterchange.EI_FooterText = "Z3901SV9CAREDI08160601               89";
			incomingInterchange.EI_Status = CBPEDIInterchange.Status.Queued;
			incomingInterchange.EI_GB = branch.PK;
			Factory.Save();
			AssertEquals(CBPEDIInterchange.Status.Queued, incomingInterchange.EI_Status);
			var serviceTask = new US1ServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			incomingInterchange.Reload();
			AssertEquals(CBPEDIInterchange.Status.Received, incomingInterchange.EI_Status);
			AssertEquals(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse, incomingInterchange.EI_InterchangeType);
			AssertEquals(1, incomingInterchange.ContainedMessages.Count);
			var incomingMessage = incomingInterchange.ContainedMessages[0];
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.USCustomsImport, incomingMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", ApplicationIdentifierCodeList.Codes.EntrySummaryResponse, incomingMessage.EM_MessageType);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, incomingMessage.EM_Status);
			AssertEquals("EM_GB", branch.PK, incomingMessage.EM_GB);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"US Customs Interchanges Inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.USCustomsImport),
				};
			}
		}
	}
}

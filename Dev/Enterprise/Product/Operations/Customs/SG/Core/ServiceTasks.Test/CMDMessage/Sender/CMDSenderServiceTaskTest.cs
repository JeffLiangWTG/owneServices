using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.SG.V4.ServiceTasks.CMDMessage.Testing
{
	[TestedType(typeof(CMDSenderServiceTask))]
	sealed class CMDSenderServiceTaskTest : ServiceTaskTestCase<CMDSenderServiceTask>
	{
		public void TestRunTask()
		{
			CMDSenderServiceTask task = new CMDSenderServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
		}

		[ExpectNoExceptions]
		public void TestServiceItteratesOverMultipleSGCompanies()
		{
			var sgCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			sgCompany1.GC_Code = "SG1";
			sgCompany1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AIR";
			sgCompany1.GC_OH_OrgProxy = org1.PK;
			var branch1 = sgCompany1.Branches.AddNew();
			branch1.GB_Code = "AAA";
			var sgCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			sgCompany2.GC_Code = "SG2";
			sgCompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "SEA";
			sgCompany2.GC_OH_OrgProxy = org2.PK;
			var branch2 = sgCompany2.Branches.AddNew();
			branch2.GB_Code = "SSS";
			var sgInactiveCompany = Factory.NewWithValidTestData<GlbCompany>();
			sgInactiveCompany.GC_Code = "SG3";
			sgInactiveCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "INA";
			sgInactiveCompany.GC_OH_OrgProxy = org2.PK;
			var branch3 = sgInactiveCompany.Branches.AddNew();
			branch3.GB_Code = "III";
			branch3.GB_IsActive = false;
			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_Code = "USA";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "CHI";
			usCompany.GC_OH_OrgProxy = org1.PK;
			var branch4 = usCompany.Branches.AddNew();
			branch4.GB_Code = "CHI";
			Factory.Save();
			var task = new CMDSenderServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"Cargo Manifest Declaration (SG) interchanges outbound",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.SingaporeCMD,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued),
				};
			}
		}
	}
}

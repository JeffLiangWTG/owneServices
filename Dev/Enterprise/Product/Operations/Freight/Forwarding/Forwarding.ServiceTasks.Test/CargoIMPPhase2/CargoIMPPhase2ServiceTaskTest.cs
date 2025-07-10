using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2.Test
{
	[TestedType(typeof(CargoIMPPhase2ServiceTask))]
	class CargoIMPPhase2ServiceTaskTest : ServiceTaskTestCase<CargoIMPPhase2ServiceTask>
	{
		public void TestRunTask()
		{
			CargoIMPPhase2ServiceTask task = new CargoIMPPhase2ServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
		}

		public void TestRunTaskForSeveralCompanies()
		{
			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "111";
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "aaa";
			company1.GC_OH_OrgProxy = org1.PK;
			GlbBranch branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "222";
			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "333";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "bbb";
			company1.GC_OH_OrgProxy = org2.PK;
			GlbBranch branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "444";
			branch2.GB_IsActive = false;
			Factory.Save();
			CargoIMPPhase2InterchangeSenderForTest.RegisterThisSubTypeOverride();
			CargoIMPPhase2ServiceTask task = new CargoIMPPhase2ServiceTask();
			TestServiceLogger log = InitialiseAndRunTaskSchedule(task);
			AssertContains("222", log.ToString());
			AssertNotContains("444", log.ToString());
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "CI2", hostedServiceAttribute.Code);
				AssertEquals("Description", "CargoIMP Phase 2 Messaging", hostedServiceAttribute.Description);
				AssertEquals("Category", "FRT", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "5minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
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
						"Cargo IMP Phase 2",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CargoIMPPhase2),
				};
			}
		}

		class CargoIMPPhase2InterchangeSenderForTest : CargoIMPPhase2InterchangeSender
		{
			#region Construction

			protected CargoIMPPhase2InterchangeSenderForTest(ILogger logger) : base(logger)
			{
			}

			public new static CargoIMPPhase2InterchangeSenderForTest New(ILogger logger)
			{
				return new CargoIMPPhase2InterchangeSenderForTest(logger);
			}

			public static void RegisterThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = new NewDelegate(New);
			}

			#endregion Construction

			protected override void Execute(CancellationToken token)
			{
				Logger.Log(LogType.Information, GlbBranch.CurrentBranch.GB_Code);
			}

			protected override bool SendInterchange(EDIInterchange interchange)
			{
				throw new System.NotImplementedException();
			}

			protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}

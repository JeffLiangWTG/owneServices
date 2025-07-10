using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMP.Test
{
	[TestedType(typeof(CargoIMPSenderServiceTask))]
	internal class CargoIMPSenderServiceTaskTest : ServiceTaskTestCase<CargoIMPSenderServiceTask>
	{
		public void TestRunTask()
		{
			CargoIMPSenderServiceTask task = new CargoIMPSenderServiceTask();
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
			CargoIMPMessageSenderForTest.RegisterThisSubTypeOverride();
			CargoIMPSenderServiceTask task = new CargoIMPSenderServiceTask();
			TestServiceLogger log = InitialiseAndRunTaskSchedule(task);
			AssertContains("222", log.ToString());
			AssertNotContains("444", log.ToString());
		}

		public void TestServiceTaskAvoidsGlbBranchCache()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			Factory.Save();
			InitialiseAndRunTaskSchedule(new CargoIMPSenderServiceTask());
			TestConnection.Command($"UPDATE dbo.GlbBranch SET GB_ISActive = 0, GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_PK = '{branch.PK}'").ExecuteScalar();
			AssertNoExceptionThrown("Caching on GlbBranch will prevent changes from propagating if the values are not fetched from the DB directly.", () => InitialiseAndRunTaskSchedule(new CargoIMPSenderServiceTask()));
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "CIS", hostedServiceAttribute.Code);
				AssertEquals("Description", "CargoIMP Message Sender", hostedServiceAttribute.Description);
				AssertEquals("Category", "CIM", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("MaximumPeriod", "15minutes", hostedServiceAttribute.MaximumPeriod);
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
						EDIInterchangeSchema.Constants.TableName,
						"Cargo IMP",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
						EDIInterchangeSchema.Constants.EI_IsActive + "=" + "Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CIM),
				};
			}
		}

		class CargoIMPMessageSenderForTest : CargoIMPMessageSender
		{
			#region Construction

			protected CargoIMPMessageSenderForTest(ILogger logger) : base(logger)
			{
			}

			public static new CargoIMPMessageSenderForTest New(ILogger logger)
			{
				return new CargoIMPMessageSenderForTest(logger);
			}

			public static void RegisterThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = new NewDelegate(New);
			}

			#endregion Construction

			protected override void ProcessCore(CancellationToken token)
			{
				Logger.Log(LogType.Debug, GlbBranch.CurrentBranch.GB_Code);
			}
		}
	}
}

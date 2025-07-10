#if NETFRAMEWORK
using IActionResult = System.Web.Http.IHttpActionResult;
#elif NET
using Microsoft.AspNetCore.Mvc;
#endif
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.Accounting
{
	class AccountingBillingControllerTest : TestCaseWithFactory
	{
		public void TestCreateJobHeader_NonexistentOperationsJob()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var result =
				GetController()
					.CreateJobHeader(
						Guid.NewGuid(),
						JobShipmentSchema.Constants.Prefix,
						GlbStaff.CurrentUser.PK.ToGuid(),
						GlbBranch.CurrentBranch.PK.ToGuid(),
						GlbDepartment.CurrentDepartment.PK.ToGuid(),
						Guid.NewGuid())
					.GetResult();

			AssertEquals(
				"Should return 400 status code when operation failed",
				(int)HttpStatusCode.BadRequest,
				result.GetStatusCode());

			AssertEquals(
				"Should return message related to the condition",
				MessageHelper.DecorateAsResponse(HttpStatusCode.BadRequest, "Cannot load/create job header."),
				result.GetMessage(HttpStatusCode.BadRequest));

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("No job is created when an invalid business object PK is passed in", 0, jobCount);
		}

		public void TestCreateJobHeader_InvalidOperationsJob()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var operationsJob = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var result =
				GetController()
					.CreateJobHeader(
						operationsJob.PK.ToGuid(),
						operationsJob.TablePrefix,
						GlbStaff.CurrentUser.PK.ToGuid(),
						GlbBranch.CurrentBranch.PK.ToGuid(),
						GlbDepartment.CurrentDepartment.PK.ToGuid(),
						Guid.NewGuid())
					.GetResult();

			AssertEquals(
				"Should return 400 status code when operation failed",
				(int)HttpStatusCode.BadRequest,
				result.GetStatusCode());

			AssertEquals(
				"Should return message related to the condition",
				MessageHelper.DecorateAsResponse(HttpStatusCode.BadRequest, "Cannot load/create job header."),
				result.GetMessage(HttpStatusCode.BadRequest));

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("No job is created when an invalid operation is passed in", 0, jobCount);
		}

		public void TestCreateJobHeader_InvalidTablePrefix()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var localClient = Factory.NewWithValidTestData<OrgAddress>();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var invalidTablePrefix = "IVPX";
			var result =
				GetController()
					.CreateJobHeader(
						operationsJob.PK.ToGuid(),
						invalidTablePrefix,
						GlbStaff.CurrentUser.PK.ToGuid(),
						GlbBranch.CurrentBranch.PK.ToGuid(),
						GlbDepartment.CurrentDepartment.PK.ToGuid(),
						localClient.PK.ToGuid())
					.GetResult();

			AssertEquals(
				"Should return 400 status code when operation failed",
				(int)HttpStatusCode.BadRequest,
				result.GetStatusCode());

			AssertEquals(
				"Should return message related to the condition",
				MessageHelper.DecorateAsResponse(HttpStatusCode.BadRequest, invalidTablePrefixAcceptedMessage),
				result.GetMessage(HttpStatusCode.BadRequest));

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("No job is created when an invalid business object is introduced", 0, jobCount);
		}

		public void TestCreateJobHeader_ShouldSetLocalCharges()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var localClient = Factory.NewWithValidTestData<OrgAddress>();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var result =
				GetController()
					.CreateJobHeader
					(
						operationsJob.PK.ToGuid()
						, operationsJob.TablePrefix
						, GlbStaff.CurrentUser.PK.ToGuid()
						, GlbBranch.CurrentBranch.PK.ToGuid()
						, GlbDepartment.CurrentDepartment.PK.ToGuid()
						, localClient.PK.ToGuid()
					)
					.GetResult();

			Assert(result.GetStatusCode() == (int)HttpStatusCode.OK);

			AssertNull(result.GetContent(HttpStatusCode.OK));

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Job is created when valid operation and job id passed in", 1, jobCount);

			var jobHeader = Factory.Load<JobHeader>(new ZQuery())[0];
			AssertEquals(localClient.PK, jobHeader.JH_OA_LocalChargesAddr);
		}

		public void TestPostRevenue_NonexistentOperationsJob()
		{
			TestMethodOverNonexistentOperationsJob(GetController().PostRevenue);
		}

		public void TestPostRevenue_InvalidOperationsJob()
		{
			TestMethodOverInvalidOperationsJob(GetController().PostRevenue);
		}

		public void TestPostRevenue_InvalidTablePrefix()
		{
			TestMethodOverInvalidTablePrefix(GetController().PostRevenue);
		}

		public void TestPostRevenue_WithoutNotificationErrors()
		{
			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<IRevenuePosterCreator>(creator);
			TestMethodWithoutNotificationErrors(GetController().PostRevenue, creator);
		}

		public void TestPostRevenue_WithNotificationErrors()
		{
			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<IRevenuePosterCreator>(creator);
			TestMethodWithNotificationErrors(GetController().PostRevenue, creator);
		}

		public void TestPostCost_NonexistentOperationsJob()
		{
			TestMethodOverNonexistentOperationsJob(GetController().PostCost);
		}

		public void TestPostCost_InvalidOperationsJob()
		{
			TestMethodOverInvalidOperationsJob(GetController().PostCost);
		}

		public void TestPostCost_InvalidTablePrefix()
		{
			TestMethodOverInvalidTablePrefix(GetController().PostCost);
		}

		public void TestPostCost_WithoutNotificationErrors()
		{
			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<ICostPosterCreator>(creator);
			TestMethodWithoutNotificationErrors(GetController().PostCost, creator);
		}

		public void TestPostCost_WithNotificationErrors()
		{
			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<ICostPosterCreator>(creator);
			TestMethodWithNotificationErrors(GetController().PostCost, creator);
		}

		public void TestPostOverseasAgentCharges_NonexistentOperationsJob()
		{
			TestMethodOverNonexistentOperationsJob(GetController().PostOverseasAgentCharges);
		}

		public void TestPostOverseasAgentCharges_InvalidOperationsJob()
		{
			TestMethodOverInvalidOperationsJob(GetController().PostOverseasAgentCharges);
		}

		public void TestPostOverseasAgentCharges_InvalidTablePrefix()
		{
			TestMethodOverInvalidTablePrefix(GetController().PostOverseasAgentCharges);
		}

		public void TestPostOverseasAgentCharges_WithoutNotificationErrors()
		{
			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<IPostOverseasAgentChargesProcessorCreator>(creator);
			TestMethodWithoutNotificationErrors(GetController().PostOverseasAgentCharges, creator);
		}

		public void TestPostOverseasAgentCharges_WithNotificationErrors()
		{
			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<IPostOverseasAgentChargesProcessorCreator>(creator);
			TestMethodWithNotificationErrors(GetController().PostOverseasAgentCharges, creator);
		}

		public void TestSplitApportionAmount_ConsolCostNotFound()
		{
			var result =
				GetController()
					.SplitApportionAmount(
						Guid.NewGuid(),
						GlbStaff.CurrentUser.PK.ToGuid(),
						GlbBranch.CurrentBranch.PK.ToGuid(),
						GlbDepartment.CurrentDepartment.PK.ToGuid())
					.GetResult();

			AssertEquals(
				"Should return 400 status code when operation failed",
				(int)HttpStatusCode.BadRequest,
				result.GetStatusCode());

			AssertEquals(
				"Should return message related to the condition",
				MessageHelper.DecorateAsResponse(HttpStatusCode.BadRequest, "Consol Cost could not be found."),
				result.GetMessage(HttpStatusCode.BadRequest));
		}

		public void TestSplitApportionAmount()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			Factory.Save();
			var apps = new ApportionmentListing(Factory, consol);
			try
			{
				var consolCost = apps.CostsCollection.TryAddNew();
				var testObjectCreator = new TestObjectCreator(Factory);
				consolCost.E6_AC_ChargeCode = testObjectCreator.DSBChargeCode.PK;
				consolCost.E6_OSCostAmount = 5163.96;
				consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				Factory.Save();
				AssertEquals(consolCost.ApportionmentCharges.Count, 2);
				AssertEquals(2581.98m, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals(2581.98m, consolCost.ApportionmentCharges[1].JR_OSCostAmt);
				TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobConsolCost
SET
	E6_OSCostAmount = 300,
	E6_ApportionmentMethod = '{0}',
	E6_SystemLastEditTimeUtc = GETUTCDATE(),
	E6_SystemLastEditUser = '~BP'
WHERE
	E6_PK = '{1}'", AllocationMethod.Shipment, consolCost.PK));
				var result =
					GetController()
						.SplitApportionAmount(
							consolCost.PK.ToGuid(),
							GlbStaff.CurrentUser.PK.ToGuid(),
							GlbBranch.CurrentBranch.PK.ToGuid(),
							GlbDepartment.CurrentDepartment.PK.ToGuid())
						.GetResult();

				AssertEquals(
					"Should return 200 when operation succeeded",
					(int)HttpStatusCode.OK,
					result.GetStatusCode());

				AssertNull(result.GetContent(HttpStatusCode.OK));

				var reloadedConsolCost = Factory.Load<JobConsolCost>(consolCost.PK);
				AssertEquals(reloadedConsolCost.ApportionmentCharges.Count, 2);
				AssertEquals(150m, reloadedConsolCost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals(150m, reloadedConsolCost.ApportionmentCharges[1].JR_OSCostAmt);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		#region Internals 

		AccountingBillingController GetController() => ControllerHelper.GetController<AccountingBillingController, AccountingBillingService>();

		void TestMethodOverNonexistentOperationsJob(Func<Guid, string, Guid, Guid, Guid, IActionResult> method)
		{
			var invalidJobID = Guid.NewGuid();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();

			var result =
				method(
						invalidJobID,
						operationsJob.TablePrefix,
						GlbStaff.CurrentUser.PK.ToGuid(),
						GlbBranch.CurrentBranch.PK.ToGuid(),
						GlbDepartment.CurrentDepartment.PK.ToGuid())
					.GetResult();

			var status = result.GetStatusCode();

			AssertEquals(
				"Should return 400 status code when operation failed",
				(int)HttpStatusCode.BadRequest,
				status);

			AssertEquals(
				"Should return message related to the condition",
				MessageHelper.DecorateAsResponse(HttpStatusCode.BadRequest, string.Format(nonExistentOperationJobAcceptedMessageTemplate, operationsJob.TablePrefix, invalidJobID)),
				result.GetMessage(HttpStatusCode.BadRequest));
		}

		void TestMethodOverInvalidOperationsJob(Func<Guid, string, Guid, Guid, Guid, IActionResult> method)
		{
			var operationsJob = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var result =
				method(
						operationsJob.PK.ToGuid(),
						operationsJob.TablePrefix,
						GlbStaff.CurrentUser.PK.ToGuid(),
						GlbBranch.CurrentBranch.PK.ToGuid(),
						GlbDepartment.CurrentDepartment.PK.ToGuid())
					.GetResult();

			AssertEquals(
				"Should return 400 status code when operation failed",
				(int)HttpStatusCode.BadRequest,
				result.GetStatusCode());

			AssertEquals(
				"Should return message related to the condition",
				MessageHelper.DecorateAsResponse(HttpStatusCode.BadRequest, invalidOperaitonJobAcceptedMessage),
				result.GetMessage(HttpStatusCode.BadRequest));
		}

		void TestMethodOverInvalidTablePrefix(Func<Guid, string, Guid, Guid, Guid, IActionResult> method)
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var invalidTablePrefix = "IVPX";
			var result =
				method(
						operationsJob.PK.ToGuid(),
						invalidTablePrefix,
						GlbStaff.CurrentUser.PK.ToGuid(),
						GlbBranch.CurrentBranch.PK.ToGuid(),
						GlbDepartment.CurrentDepartment.PK.ToGuid())
					.GetResult();

			AssertEquals(
				"Should return 400 status code when operation failed",
				(int)HttpStatusCode.BadRequest,
				result.GetStatusCode());

			AssertEquals(
				"Should return message related to the condition",
				MessageHelper.DecorateAsResponse(HttpStatusCode.BadRequest, invalidTablePrefixAcceptedMessage),
				result.GetMessage(HttpStatusCode.BadRequest));
		}

		void TestMethodWithoutNotificationErrors(Func<Guid, string, Guid, Guid, Guid, IActionResult> method, TestPosterCreator creator)
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Some Warning"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Information, "Some Information"));

			var result =
				method(
						operationsJob.PK.ToGuid(),
						operationsJob.TablePrefix,
						GlbStaff.CurrentUser.PK.ToGuid(),
						GlbBranch.CurrentBranch.PK.ToGuid(),
						GlbDepartment.CurrentDepartment.PK.ToGuid())
					.GetResult();

			AssertEquals(
				"Should return 200 when operation succeeded",
				(int)HttpStatusCode.OK,
				result.GetStatusCode());

			AssertNull(result.GetContent(HttpStatusCode.OK));
		}

		void TestMethodWithNotificationErrors(Func<Guid, string, Guid, Guid, Guid, IActionResult> method, TestPosterCreator creator)
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Some Warning"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "First Error"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Information, "Some Information"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "Second Error"));

			var result =
				method(
						operationsJob.PK.ToGuid(),
						operationsJob.TablePrefix,
						GlbStaff.CurrentUser.PK.ToGuid(),
						GlbBranch.CurrentBranch.PK.ToGuid(),
						GlbDepartment.CurrentDepartment.PK.ToGuid())
					.GetResult();

			AssertEquals(
				"Should return 200 status code when operation failed",
				(int)HttpStatusCode.OK,
				result.GetStatusCode());

			AssertEquals(
				"Should return message related to the condition",
				MessageHelper.DecorateAsResponse(HttpStatusCode.OK, "First Error; Second Error"),
				result.GetMessage(HttpStatusCode.OK));
		}

		const string nonExistentOperationJobAcceptedMessageTemplate = "Operations job with Table-prefix '{0}' and PK '{1}' could not be found.";
		const string invalidOperaitonJobAcceptedMessage = "Operation job is not valid.";
		const string invalidTablePrefixAcceptedMessage = "Table prefix is invalid.";

		class TestPosterCreator :
			IRevenuePosterCreator,
			ICostPosterCreator,
			IPostOverseasAgentChargesProcessorCreator
		{
			public TestProcessor Processor => processor ?? (processor = new TestProcessor());
			TestProcessor processor;

			public IWorkflowProvider LastUsedProvider { get; private set; }

			public IProcessor CreateRevenuePoster(IWorkflowProvider provider)
			{
				return GetPoster(provider);
			}

			public IProcessor CreateCostPoster(IWorkflowProvider provider)
			{
				return GetPoster(provider);
			}

			public IProcessor CreateOverseasAgentChargesPoster(IWorkflowProvider provider)
			{
				return GetPoster(provider);
			}

			IProcessor GetPoster(IWorkflowProvider provider)
			{
				LastUsedProvider = provider;
				return Processor;
			}
		}

		class TestProcessor : IProcessor
		{
			public List<INotification> ProcessNotifications => processNotifications ?? (processNotifications = new List<INotification>());
			List<INotification> processNotifications;

			public void Process(INotifications notifications, CancellationToken token = new CancellationToken())
			{
				notifications.AddRange(ProcessNotifications);
			}
		}

		#endregion
	}
}

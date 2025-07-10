using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.CustomsMessaging.Testing
{
	[TestedType(typeof(AutoSendCustomsMessagingServiceTask))]
	class AutoSendCustomsMessagingServiceTaskTest : ServiceTaskTestCase<AutoSendCustomsMessagingServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		[ExpectNoExceptions]
		public void TestFirstActiveBranchIsNullException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var company = GlbCompany.CurrentCompany;
				foreach (var branch in company.ActiveBranches)
				{
					branch.GB_IsActive = false;
				}
				AssertNull(company.FirstActiveBranch);

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_DeclarationReference = "B00001001";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1234567890";
				CustomsStmProcessQueueLoader.New(declaration, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, "SEM");
				Factory.Save();

				var serviceTask = new AutoSendCustomsMessagingServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);
				AssertContains("serviceTask.ServiceLogger", $"SED message has been sent to customs for Job:B00001001", serviceTask.ServiceLogger.ToString());
				Factory.Save();
			}
		}

		public void TestSendCustomsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_DeclarationReference = "B00001001";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "1234567890";
				CustomsStmProcessQueueLoader.New(declaration, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, "SEM");
				Factory.Save();

				var serviceTask = new AutoSendCustomsMessagingServiceTask();
				InitialiseTaskSchedule(serviceTask);
				RunTaskSchedule(serviceTask);
				AssertContains("serviceTask.ServiceLogger", $"SED message has been sent to customs for Job:B00001001", serviceTask.ServiceLogger.ToString());
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var declarationLoaded = newFactory.Load<BaseJobDeclaration>(declaration.PK);
				AssertEquals(1, declarationLoaded.ActiveEntryHeaders.Count);
				AssertEquals(1, declarationLoaded.ActiveEntryHeaders[0].Messages.Count);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						StmProcessQueueSchema.Constants.TableName,
						"Auto Send Customs Messaging",
						StmProcessQueueSchema.Constants.SW_ApplicationCode + "=" + CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging,
						StmProcessQueueSchema.Constants.SW_JobTypeCode + "=" + CustomsStmProcessQueueLoader.Constants.JobTypeCode),
				};
			}
		}
	}
}

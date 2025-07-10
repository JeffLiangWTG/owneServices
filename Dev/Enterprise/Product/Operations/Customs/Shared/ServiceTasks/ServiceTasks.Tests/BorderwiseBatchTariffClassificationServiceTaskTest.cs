using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	[TestedType(typeof(BorderWiseBatchTariffClassificationServiceTask))]
	public class BorderWiseBatchTariffClassificationServiceTaskTest : ServiceTaskTestCase<BorderWiseBatchTariffClassificationServiceTask>
	{
		protected override void SetUpCore()
		{
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient = true;
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassification = true;
		}

		public void TestRunTask_Should_SkipRetrieveAndProcessTariffClassificationFromBorderWise_When_NotProductionSystemAndRegistryIsFalse()
		{
			// Arrange
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassificationServiceTask = false;

			// Act
			var serviceTask = new BorderWiseBatchTariffClassificationServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);

			// Assert
			AssertNotContains("serviceTask.ServiceLogger", $"Started retrieving completed tariff classification jobs from BorderWise", serviceTask.ServiceLogger.ToString());
		}

		public void TestRunTask_Should_RunRetrieveAndProcessTariffClassificationFromBorderWise_When_NotProductionSystemAndRegistryIsTrue()
		{
			// Arrange
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassificationServiceTask = true;
			var auBranch = CreateCompanyAndBranch("AU1", Core.Constants.CountryCodes.Australia);
			var declaration = CreateJobDeclaration(auBranch);
			var borderWiseInvoiceLine1 = new BorderWiseInvoiceLine("2901.24.00", "06", declaration.FilteredInvoiceLines[0].PK.ToGuid(), 2, declaration.Invoices[0].PK.ToGuid(), "456", "test1");

			var borderWiseTariffExchangeModelV2 = new BorderWiseTariffExchangeModelV2()
			{
				JobPk = declaration.PK.ToGuid(),
				JobNumber = declaration.JobNumber,
				JobType = "Customs Declaration",
				BranchPk = auBranch.PK.ToGuid(),
			};

			borderWiseTariffExchangeModelV2.BorderWiseInvoiceLines.Add(borderWiseInvoiceLine1);
			var httpMessageHandlerMock = SetUpHttpMessage(borderWiseTariffExchangeModelV2);

			// Act
			var serviceTask = new BorderWiseBatchTariffClassificationServiceTask();
			serviceTask.HttpMessageHandler = httpMessageHandlerMock.Object;
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);

			// Assert
			AssertContains("serviceTask.ServiceLogger", $"Started retrieving completed tariff classification jobs from BorderWise", serviceTask.ServiceLogger.ToString());
		}

		public void TestRetrieveAndProcessTariffClassificationFromBorderWise_Should_UpdateCustomDeclaration()
		{
			// Arrange
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassificationServiceTask = false;
			var auBranch = CreateCompanyAndBranch("AU1", Core.Constants.CountryCodes.Australia);
			var declaration = CreateJobDeclaration(auBranch);
			var borderWiseInvoiceLine1 = new BorderWiseInvoiceLine("2901.24.00", "06", declaration.FilteredInvoiceLines[0].PK.ToGuid(), 2, declaration.Invoices[0].PK.ToGuid(), "456", "test1");

			var borderWiseTariffExchangeModelV2 = new BorderWiseTariffExchangeModelV2()
			{
				JobPk = declaration.PK.ToGuid(),
				JobNumber = declaration.JobNumber,
				JobType = "Customs Declaration",
				BranchPk = auBranch.PK.ToGuid(),
			};

			borderWiseTariffExchangeModelV2.BorderWiseInvoiceLines.Add(borderWiseInvoiceLine1);
			var httpMessageHandlerMock = SetUpHttpMessage(borderWiseTariffExchangeModelV2);

			// Act
			var serviceTask = new BorderWiseBatchTariffClassificationServiceTask();
			serviceTask.OverriddenValue_ForTest.Value = true;
			serviceTask.HttpMessageHandler = httpMessageHandlerMock.Object;
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);

			// Assert
			var newFactory = new BusinessObjectFactory();
			var declarationLoaded = newFactory.Load<BaseJobDeclaration>(declaration.PK);
			AssertEquals("2901.24.00 06", declarationLoaded.FilteredInvoiceLines[0].JI_Tariff);
			AssertEquals("test1", declarationLoaded.FilteredInvoiceLines[0].JI_Description);
			AssertContains("serviceTask.ServiceLogger", $"Completed updating invoice lines for JobNumber {declarationLoaded?.JobNumber} with JobPK: {declarationLoaded.PK}.", serviceTask.ServiceLogger.ToString());
		}

		public void TestRetrieveAndProcessTariffClassificationFromBorderWise_Should_UpdateCommercialInvoice()
		{
			// Arrange
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableMultilineTariffClassificationServiceTask = false;
			var auBranch = CreateCompanyAndBranch("AU1", Core.Constants.CountryCodes.Australia);
			var invoiceHeader = CreateCommercialInvoice(auBranch);
			var borderWiseInvoiceLine1 = new BorderWiseInvoiceLine("2901.24.00", "06", invoiceHeader.InvoiceLines[0].PK.ToGuid(), 2, invoiceHeader.InvoiceLines[0].PK.ToGuid(), "456", "test1");

			var borderWiseTariffExchangeModelV2 = new BorderWiseTariffExchangeModelV2()
			{
				JobPk = invoiceHeader.PK.ToGuid(),
				JobNumber = invoiceHeader.JobNumber,
				JobType = "Commercial Invoice",
				BranchPk = auBranch.PK.ToGuid(),
			};

			borderWiseTariffExchangeModelV2.BorderWiseInvoiceLines.Add(borderWiseInvoiceLine1);
			var httpMessageHandlerMock = SetUpHttpMessage(borderWiseTariffExchangeModelV2);

			// Act
			var serviceTask = new BorderWiseBatchTariffClassificationServiceTask();
			serviceTask.OverriddenValue_ForTest.Value = true;
			serviceTask.HttpMessageHandler = httpMessageHandlerMock.Object;
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);

			// Assert
			var newFactory = new BusinessObjectFactory();
			var declarationLoaded = newFactory.Load<BaseJobComInvoiceHeader>(invoiceHeader.PK);
			AssertEquals("2901.24.00 06", declarationLoaded.InvoiceLines[0].JI_Tariff);
			AssertEquals("test1", declarationLoaded.InvoiceLines[0].JI_Description);
			AssertContains("serviceTask.ServiceLogger", $"Completed updating invoice lines for JobNumber {declarationLoaded?.JobNumber} with JobPK: {declarationLoaded.PK}.", serviceTask.ServiceLogger.ToString());
		}

		Mock<HttpMessageHandler> SetUpHttpMessage(BorderWiseTariffExchangeModelV2 borderWiseTariffExchangeModelV2)
		{
			// Arrange
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			var completedTariffExchangeJobs = new List<BorderWiseTariffExchangeModelV2>()
			{
				new BorderWiseTariffExchangeModelV2()
				{
					JobPk = borderWiseTariffExchangeModelV2.JobPk,
					JobNumber = borderWiseTariffExchangeModelV2.JobNumber
				}
			};
			var completedTariffExchangeJobsResponse = BorderWiseBatchTariffClassificationServiceTask.SerializeAsJson(completedTariffExchangeJobs);
			var tariffExchangeJobsResponse = BorderWiseBatchTariffClassificationServiceTask.SerializeAsJson(borderWiseTariffExchangeModelV2);

			httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
			"SendAsync",
			ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.Contains("/api/tariff-classification-job/completed-tariff-exchange-jobs")),
			ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(completedTariffExchangeJobsResponse)
			});

			httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
			"SendAsync",
			ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.Contains($"/api/tariff-classification-job/tariff-exchange-job")),
			ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(tariffExchangeJobsResponse)
			});

			httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
			"SendAsync",
			ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.Contains($"/api/tariff-classification-job/update-job-status")),
			ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
			{
				StatusCode = HttpStatusCode.OK
			});

			httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
			"SendAsync",
			ItExpr.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri.Contains($"/api/tariff-classification-job/delete-job")),
			ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
			{
				StatusCode = HttpStatusCode.OK
			});

			return httpMessageHandlerMock;
		}

		BaseJobDeclaration CreateJobDeclaration(GlbBranch glbBranch)
		{
			BaseJobDeclaration declaration;

			using (Environment.DisposableEnvironment.ForBranch(glbBranch.PK.ToGuid()))
			{
				var factory = new BusinessObjectFactory();
				declaration = factory.New<BaseJobDeclaration>();

				var invoiceHeader = declaration.Invoices.AddNew();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "1234.56.78 01";
				invoiceLine1.JI_Description = "test1";
				invoiceLine1.JI_Calc_Invoice = "123";

				factory.Save();
			}

			return declaration;
		}

		BaseJobComInvoiceHeader CreateCommercialInvoice(GlbBranch glbBranch)
		{
			BaseJobComInvoiceHeader invoiceHeader;

			using (Environment.DisposableEnvironment.ForBranch(glbBranch.PK.ToGuid()))
			{
				var factory = new BusinessObjectFactory();
				invoiceHeader = factory.New<BaseJobComInvoiceHeader>();

				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "1234.56.78 01";
				invoiceLine1.JI_Description = "test1";
				invoiceLine1.JI_Calc_Invoice = "123";

				factory.Save();
			}

			return invoiceHeader;
		}

		GlbBranch CreateCompanyAndBranch(ZString code, ZString countryCode)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = code;
			company.GC_Name = "TEST COMP " + code;
			company.GC_RN_NKCountryCode = countryCode;
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch = company.Branches.AddNew();
			branch.GB_Code = code;
			branch.GB_BranchName = "TEST BRANCH " + code;
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			Factory.Save();

			return branch;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	[TestedType(typeof(ContainerDetentionAdviceServiceTask))]
	internal class ContainerDetentionAdviceServiceTaskTest : ServiceTaskTestCase<ContainerDetentionAdviceServiceTask>
	{
		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Auto)]
		public void TestBlankDetentionAdvicesWithContainer()
		{
			var company = GlbCompany.CurrentCompany;
			var today = DateTime.Today;
			DocumentsDataRegistry.Instance.UseNewDocBuilderForwardingDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var delivery = AgencyRegistry.Instance.DetentionAdviceBehaviour.Value;
			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.AgencyDtnAdvice));
			filter.AddToFilter(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Documents);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuName, "Detention Advice");
			NewContainer(EmailClient, "FAKE4100011", "V1", "NLAMS", "AUBNE", today);
			Factory.Save();
			AssertPrintJobCounts("precondition: email", EmailClient, 0, 0, 0);
			var adviceProvider = new ContainerDetentionAdviceProvider(company, today);
			AssertEquals("Precondition: ContainerDetentionAdviceProvider.Count", 1, adviceProvider.Count());
			var advice = adviceProvider.FirstOrDefault();
			DocumentCommand command = advice.Factory.LoadTop1<DocumentCommand>(filter);
			command.Parent = advice;
			DocAutoDelivery autoDelivery = new DocAutoDelivery();
			UserControlProviderList providerList = new UserControlProviderList();
			DocDeliveryContactCollection contacts = autoDelivery.GetDeliveryContacts(command, advice.DocumentSupporter);
			var container = advice.Containers[0];
			container.JC_EmptyReturnedBy = ZDateTime.Empty; // simulate invalid data
			using (DocumentPack pack = new DocumentPack(command, advice, providerList, command))
			{
				DeliveryInstructions instructions = new DeliveryInstructions(pack);
				instructions.AllowAutoDelivery = true;
				instructions.Destination = DeliveryInstructionDestination.Auto;
				instructions.PrinterDelivery.PrintQueuePK = delivery.Printer;
				instructions.Recipients.RemoveAll();
				instructions.Recipients.AddRange(contacts);
				AssertEquals("Precondition: JC_EmptyReturnedBy", ZDateTime.Empty, container.JC_EmptyReturnedBy);
				using (DocumentPrintSet set = new DocumentPrintSet(command, providerList))
				{
					set.Run(instructions);
				}

				AssertPrintJobCounts("email", EmailClient, 0, 1, 0);
				AssertEquals("Row Count for Business Object DataSource", 1, pack.GetRowCountForBusinessObjectDataSource()); // blank advice has zero row count
			}
		}

		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Auto)]
		public void TestBlankDetentionAdvicesWithMovement()
		{
			var company = GlbCompany.CurrentCompany;
			var today = DateTime.Today;
			DocumentsDataRegistry.Instance.UseNewDocBuilderForwardingDocuments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var delivery = AgencyRegistry.Instance.DetentionAdviceBehaviour.Value;
			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.AgencyDtnAdvice));
			filter.AddToFilter(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Documents);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuName, "Detention Advice");
			OrgHeader depot = Factory.New<OrgHeader>();
			depot.OH_Code = "AUSYD1111";
			depot.OH_RL_NKClosestPort = "AUSYD";
			NewMovement(EmailClient, "Stock1", depot, Enterprise.Core.Constants.ContainerOwnership.Codes.Leased, ContainerMovementTypes.Codes.YardGateOut, today);
			Factory.Save();
			AssertPrintJobCounts("precondition: email", EmailClient, 0, 0, 0);
			var adviceProvider = new ContainerDetentionAdviceProvider(company, today);
			AssertEquals("Precondition: ContainerDetentionAdviceProvider.Count", 1, adviceProvider.Count());
			var advice = adviceProvider.FirstOrDefault();
			DocumentCommand command = advice.Factory.LoadTop1<DocumentCommand>(filter);
			command.Parent = advice;
			DocAutoDelivery autoDelivery = new DocAutoDelivery();
			UserControlProviderList providerList = new UserControlProviderList();
			DocDeliveryContactCollection contacts = autoDelivery.GetDeliveryContacts(command, advice.DocumentSupporter);
			using (DocumentPack pack = new DocumentPack(command, advice, providerList, command))
			{
				DeliveryInstructions instructions = new DeliveryInstructions(pack);
				instructions.AllowAutoDelivery = true;
				instructions.Destination = DeliveryInstructionDestination.Auto;
				instructions.PrinterDelivery.PrintQueuePK = delivery.Printer;
				instructions.Recipients.RemoveAll();
				instructions.Recipients.AddRange(contacts);
				instructions.ExcludeDocumentsWhichContainNoBusinessObjectDataRows = true;
				using (DocumentPrintSet set = new DocumentPrintSet(command, providerList))
				{
					set.Run(instructions);
				}

				AssertEquals("Row Count for Business Object DataSource", 0, pack.GetRowCountForBusinessObjectDataSource());
				AssertPrintJobCounts("email", EmailClient, 0, 0, 0);
			}
		}

		public void TestIsRegistered()
		{
			HostedServiceAttribute att = TestHelper.GetAttributeRegistring<ContainerDetentionAdviceServiceTask>();
			AssertNotNull("Should have found an attribute", att);
			CombineAssertions(delegate
			{
				AssertEquals("Category", ServiceTaskConstants.Category, att.Category);
				AssertEquals("Code", ContainerDetentionAdviceServiceTask.Code, att.Code);
				AssertEquals("Description", ContainerDetentionAdviceServiceTask.Description, att.Description);
				AssertEquals("CanRunInAnyBranch", true, att.CanRunInAnyBranch);
			});
		}

		[ExpectNoExceptions]
		public void TestOptions()
		{
			var attribute = GetHostedServiceAttributes().FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("DefaultScheduleRunEvery", "1week", attribute.DefaultScheduleRunEvery);
				AssertContainsExactElementsInAnyOrder("DefaultScheduleDaysOfWeek", new[] { DayOfWeek.Saturday }, attribute.DefaultScheduleDaysOfWeek);
				AssertEquals("DefaultScheduleStartAtLocal", "6hours", attribute.DefaultScheduleStartAtLocal);
			});
		}

		public void TestMinimumPeriod()
		{
			var attributes = typeof(ContainerDetentionAdviceServiceTask).Assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false);
			var attribute = Array.Find((HostedServiceAttribute[])attributes, a => a.TypeName == typeof(ContainerDetentionAdviceServiceTask).FullName);
			AssertEquals("Minimum Period should be 1 Day", "1day", attribute.MinimumPeriod);
		}

		[ExpectNoExceptions]
		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Auto)]
		public void TestRun_NoClient()
		{
			var start = ZDateTime.Now;
			var today = ZDateTime.Today;
			NewContainer(null, "FAKE4100011", "V1", "NLAMS", "AUBNE", today);
			var proxy = NewClientContact(ContactNotifyModes.Email);
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "AUC";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = proxy.PK;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "AUB";
			branch.GB_BranchName = "Test Branch";
			Factory.Save();
			AssertPrintJobCounts("precondition: proxy", proxy, 0, 0, 0);
			var task = new ContainerDetentionAdviceServiceTask();
			task.ServiceLogger = Logger;
			task.RunTask();
			AssertEquals("ContactEmailIsEmpty_WhenDeliveryMethodIsEmail", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Auto)]
		public void TestRun_Auto()
		{
			ZDateTime start = ZDateTime.Now;
			ZDateTime today = ZDateTime.Today;
			NewContainer(EmailClient, "FAKE4100011", "V1", "NLAMS", "AUBNE", today);
			NewContainer(EmailClient, "FAKE4100027", "V2", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100032", "V3", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100048", "V4", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100053", "V5", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100069", "V6", "NLAMS", "AUBNE", today);
			Factory.Save();
			AssertPrintJobCounts("precondition: email", EmailClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: fax", FaxClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: print", PrintClient, 0, 0, 0);
			ContainerDetentionAdviceServiceTask task = new ContainerDetentionAdviceServiceTask();
			task.ServiceLogger = Logger;
			task.RunTask();
			AssertMultilineASCIIEquals("", "", Logger.ToString());
			AssertPrintJobCounts("email", EmailClient, 0, 1, 0);
			AssertPrintJobCounts("fax", FaxClient, 0, 0, 1);
			AssertPrintJobCounts("print", PrintClient, 1, 0, 0);
		}

		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Print)]
		public void TestRun_Print()
		{
			ZDateTime start = ZDateTime.Now;
			ZDateTime today = ZDateTime.Today;
			NewContainer(EmailClient, "FAKE4100011", "V1", "NLAMS", "AUBNE", today);
			NewContainer(EmailClient, "FAKE4100027", "V2", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100032", "V3", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100048", "V4", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100053", "V5", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100069", "V6", "NLAMS", "AUBNE", today);
			Factory.Save();
			AssertPrintJobCounts("precondition: email", EmailClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: fax", FaxClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: print", PrintClient, 0, 0, 0);
			ContainerDetentionAdviceServiceTask task = new ContainerDetentionAdviceServiceTask();
			task.ServiceLogger = Logger;
			task.RunTask();
			AssertMultilineASCIIEquals("", "", Logger.ToString());
			AssertPrintJobCounts("precondition: email", EmailClient, 1, 0, 0);
			AssertPrintJobCounts("precondition: fax", FaxClient, 1, 0, 0);
			AssertPrintJobCounts("precondition: print", PrintClient, 1, 0, 0);
		}

		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Notify)]
		public void TestRun_Notify()
		{
			Factory.Save();
			ZDateTime start = ZDateTime.Now;
			ZDateTime today = ZDateTime.Today;
			NewContainer(EmailClient, "FAKE4100011", "V1", "NLAMS", "AUBNE", today);
			NewContainer(EmailClient, "FAKE4100027", "V2", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100032", "V3", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100048", "V4", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100053", "V5", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100069", "V6", "NLAMS", "AUBNE", today);
			Factory.Save();
			AssertPrintJobCounts("precondition: email", EmailClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: fax", FaxClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: print", PrintClient, 0, 0, 0);
			ContainerDetentionAdviceServiceTask task = new ContainerDetentionAdviceServiceTask();
			task.ServiceLogger = Logger;
			task.RunTask();
			AssertMultilineASCIIEquals("", "", Logger.ToString());
			AssertPrintJobCounts("email", EmailClient, 0, 1, 0);
			AssertPrintJobCounts("fax", FaxClient, 0, 1, 0);
			AssertPrintJobCounts("print", PrintClient, 0, 1, 0);
		}

		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Auto, SendToGroup = true)]
		public void TestRun_Auto_Group()
		{
			ZDateTime start = ZDateTime.Now;
			ZDateTime today = ZDateTime.Today;
			NewContainer(EmailClient, "FAKE4100011", "V1", "NLAMS", "AUBNE", today);
			NewContainer(EmailClient, "FAKE4100027", "V2", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100032", "V3", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100048", "V4", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100053", "V5", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100069", "V6", "NLAMS", "AUBNE", today);
			Factory.Save();
			AssertPrintJobCounts("precondition: email", EmailClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: fax", FaxClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: print", PrintClient, 0, 0, 0);
			ContainerDetentionAdviceServiceTask task = new ContainerDetentionAdviceServiceTask();
			task.ServiceLogger = Logger;
			task.RunTask();
			AssertMultilineASCIIEquals("", "", Logger.ToString());
			AssertPrintJobCounts("email", EmailClient, 0, 2, 0);
			AssertPrintJobCounts("fax", FaxClient, 0, 1, 1);
			AssertPrintJobCounts("print", PrintClient, 1, 1, 0);
		}

		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Print, SendToGroup = true)]
		public void TestRun_Print_Group()
		{
			ZDateTime start = ZDateTime.Now;
			ZDateTime today = ZDateTime.Today;
			NewContainer(EmailClient, "FAKE4100011", "V1", "NLAMS", "AUBNE", today);
			NewContainer(EmailClient, "FAKE4100027", "V2", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100032", "V3", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100048", "V4", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100053", "V5", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100069", "V6", "NLAMS", "AUBNE", today);
			Factory.Save();
			AssertPrintJobCounts("precondition: email", EmailClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: fax", FaxClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: print", PrintClient, 0, 0, 0);
			ContainerDetentionAdviceServiceTask task = new ContainerDetentionAdviceServiceTask();
			task.ServiceLogger = Logger;
			task.RunTask();
			AssertMultilineASCIIEquals("", "", Logger.ToString());
			AssertPrintJobCounts("email", EmailClient, 1, 1, 0);
			AssertPrintJobCounts("fax", FaxClient, 1, 1, 0);
			AssertPrintJobCounts("print", PrintClient, 1, 1, 0);
		}

		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Notify, SendToGroup = true)]
		public void TestRun_Notify_Group()
		{
			Factory.Save();
			ZDateTime start = ZDateTime.Now;
			ZDateTime today = ZDateTime.Today;
			NewContainer(EmailClient, "FAKE4100011", "V1", "NLAMS", "AUBNE", today);
			NewContainer(EmailClient, "FAKE4100027", "V2", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100032", "V3", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100048", "V4", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100053", "V5", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100069", "V6", "NLAMS", "AUBNE", today);
			Factory.Save();
			AssertPrintJobCounts("precondition: email", EmailClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: fax", FaxClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: print", PrintClient, 0, 0, 0);
			ContainerDetentionAdviceServiceTask task = new ContainerDetentionAdviceServiceTask();
			task.ServiceLogger = Logger;
			task.RunTask();
			AssertMultilineASCIIEquals("", "", Logger.ToString());
			AssertPrintJobCounts("email", EmailClient, 0, 1, 0);
			AssertPrintJobCounts("fax", FaxClient, 0, 1, 0);
			AssertPrintJobCounts("print", PrintClient, 0, 1, 0);
		}

		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Print, PrinterName = "Printer")]
		public void TestRun_MissingPrinter()
		{
			foreach (StmPrintQueue printer in Factory.Load<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_DisplayName, "Printer")))
			{
				printer.Delete();
			}

			Factory.Save();
			ZDateTime start = ZDateTime.Now;
			ZDateTime today = ZDateTime.Today;
			NewContainer(EmailClient, "FAKE4100011", "V1", "NLAMS", "AUBNE", today);
			NewContainer(EmailClient, "FAKE4100027", "V2", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100032", "V3", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100048", "V4", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100053", "V5", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100069", "V6", "NLAMS", "AUBNE", today);
			Factory.Save();
			AssertPrintJobCounts("precondition: email", EmailClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: fax", FaxClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: print", PrintClient, 0, 0, 0);
			ContainerDetentionAdviceServiceTask task = new ContainerDetentionAdviceServiceTask();
			task.ServiceLogger = Logger;
			task.RunTask();
			string expectedLog = string.Format("Error - Unable to find an approperate printer to print to. Please check the '{0}' registry option.\n", ((IRegistryItemInternals)AgencyRegistry.Instance.DetentionAdviceBehaviour).Location) + string.Format("Error - Unable to find an approperate printer to print to. Please check the '{0}' registry option.\n", ((IRegistryItemInternals)AgencyRegistry.Instance.DetentionAdviceBehaviour).Location) + "";
			AssertMultilineASCIIEquals("", expectedLog, Logger.ToString());
			AssertPrintJobCounts("email", EmailClient, 0, 0, 0);
			AssertPrintJobCounts("fax", FaxClient, 0, 0, 0);
			AssertPrintJobCounts("print", PrintClient, 0, 0, 0);
		}

		[DetentionAdviceDeliveryConfiguration(DeliveryMode = DetentionAdviceDeliveryMode.Codes.Notify, GroupName = "GRP")]
		public void TestRun_MissingNotifyGroup()
		{
			foreach (GlbGroup group in Factory.Load<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "GRP")))
			{
				group.Delete();
			}

			Factory.Save();
			ZDateTime start = ZDateTime.Now;
			ZDateTime today = ZDateTime.Today;
			NewContainer(EmailClient, "FAKE4100011", "V1", "NLAMS", "AUBNE", today);
			NewContainer(EmailClient, "FAKE4100027", "V2", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100032", "V3", "NLAMS", "AUBNE", today);
			NewContainer(FaxClient, "FAKE4100048", "V4", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100053", "V5", "NLAMS", "AUBNE", today);
			NewContainer(PrintClient, "FAKE4100069", "V6", "NLAMS", "AUBNE", today);
			Factory.Save();
			AssertPrintJobCounts("precondition: email", EmailClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: fax", FaxClient, 0, 0, 0);
			AssertPrintJobCounts("precondition: print", PrintClient, 0, 0, 0);
			ContainerDetentionAdviceServiceTask task = new ContainerDetentionAdviceServiceTask();
			task.ServiceLogger = Logger;
			task.RunTask();
			string expectedLog = string.Format("Error - Unable to find an approperate notification group to send to. Please check the '{0}' registry option.\n", ((IRegistryItemInternals)AgencyRegistry.Instance.DetentionAdviceBehaviour).Location) + string.Format("Error - Unable to find an approperate notification group to send to. Please check the '{0}' registry option.\n", ((IRegistryItemInternals)AgencyRegistry.Instance.DetentionAdviceBehaviour).Location) + "";
			AssertMultilineASCIIEquals("", expectedLog, Logger.ToString());
			AssertPrintJobCounts("email", EmailClient, 0, 0, 0);
			AssertPrintJobCounts("fax", FaxClient, 0, 0, 0);
			AssertPrintJobCounts("print", PrintClient, 0, 0, 0);
		}

		public void TestRun_CompanyWithBranchSafe()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "AUC";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			Factory.Save();
			ContainerDetentionAdviceServiceTask task = new ContainerDetentionAdviceServiceTask();
			task.ServiceLogger = Logger;
			AssertNoExceptionThrown(task.RunTask);
		}

		public void TestFindCompanyWithDisabledBranch()
		{
			ContainerDetentionAdviceServiceTask task = new ContainerDetentionAdviceServiceTask();
			GlbCompany glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany1.GC_Code = "t1";
			glbCompany1.GC_RN_NKCountryCode = "US";
			Factory.Save();
			GlbCompany glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "t2";
			glbCompany2.GC_RN_NKCountryCode = "US";
			GlbBranch branch2 = glbCompany2.Branches.AddNew();
			branch2.GB_GC = glbCompany2.PK;
			branch2.GB_Code = "Ts2";
			branch2.GB_IsActive = false;
			Factory.Save();
			task.ServiceLogger = Logger;
			AssertNoExceptionThrown(task.RunTask);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		void AssertPrintJobCounts(string message, OrgHeader client, int printJobs, int emailJobs, int faxJobs)
		{
			SortedList<string, int> expected = new SortedList<string, int>();
			if (printJobs > 0)
			{
				expected.Add("PRN", printJobs);
			}

			if (emailJobs > 0)
			{
				expected.Add("EML", emailJobs);
			}

			if (faxJobs > 0)
			{
				expected.Add("FAX", faxJobs);
			}

			SortedList<string, int> actual = CountPrintJobs(client);
			AssertEquals(message, CountsAsString(expected), CountsAsString(actual));
		}

		SortedList<string, int> CountPrintJobs(BusinessObject parent)
		{
			const string sql = "select SP_JobType, COUNT(*) as num " + "from dbo.StmPrintJob " + "where SP_ParentGuid = @parentPK and SP_ParentTableName = @parentTableName " + // not having a space here caused it to merge with the next word
 "group by SP_JobType " + "";
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@parentPK", parent.PK, StmPrintJobSchema.SP_ParentGuid);
			parameters.Add("@parentTableName", parent.TableName, StmPrintJobSchema.SP_ParentTableName);
			DynamicBusinessObjectCollection list = new DynamicBusinessObjectCollection(Factory);
			list.Load(sql, parameters);
			SortedList<string, int> counts = new SortedList<string, int>();
			foreach (DynamicBusinessObject dbo in list)
			{
				counts.Add(new ZString(dbo[StmPrintJobSchema.Constants.SP_JobType]), new ZInt(dbo["num"]));
			}

			return counts;
		}

		string CountsAsString(SortedList<string, int> counts)
		{
			StringBuilder builder = new StringBuilder();
			using (IEnumerator<KeyValuePair<string, int>> enumerator = counts.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					builder.Append(enumerator.Current.Key);
					builder.Append(" = ");
					builder.Append(enumerator.Current.Value);
					while (enumerator.MoveNext())
					{
						builder.Append(", ");
						builder.Append(enumerator.Current.Key);
						builder.Append(" = ");
						builder.Append(enumerator.Current.Value);
					}
				}
			}

			return builder.ToString();
		}

		BillOfLadingContainer NewContainer(OrgHeader client, string containerNum, string shipmentNum, string load, string discharge, ZDateTime today)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_AvailabilityDate = today.AddDays(-1);
			voyage.GenerateSailings();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = shipmentNum;
			bill.JS_RL_NKOrigin = load;
			bill.JS_RL_NKDestination = discharge;
			bill.JS_JX = voyage.Sailings[0].PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = containerNum;
			container.JC_EmptyReturnedBy = today.AddDays(9);
			if (client != null)
			{
				JobHeader header = Factory.NewJobForTesting<JobHeader>();
				header.JH_GC = GlbCompany.CurrentCompany.PK;
				header.JH_GB = GlbBranch.CurrentBranch.PK;
				header.JH_GE = GlbDepartment.CurrentDepartment.PK;
				header.JH_ParentID = container.Booking.PK;
				header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				header.Parent = container.Booking;
				header.JH_JobNum = container.Booking.JS_UniqueConsignRef;
				header.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			}

			return container;
		}

		ContainerMovement NewMovement(OrgHeader client, string containerNum, OrgHeader depot, string ownerType, string movementType, ZDateTime today)
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = containerNum;
			stock.R6_OwnerType = ownerType;
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementType = movementType;
			movement.E9_MovementDate = today.AddDays(-1);
			movement.E9_OA_Depot = depot.MainAddress.PK;
			movement.E9_OH_ResponsibleParty = client.PK;
			return movement;
		}

		OrgHeader EmailClient
		{
			get
			{
				return emailClient ?? (emailClient = NewClientContact(ContactNotifyModes.Email));
			}
		}

		OrgHeader emailClient;
		OrgHeader FaxClient
		{
			get
			{
				return faxClient ?? (faxClient = NewClientContact(ContactNotifyModes.Fax));
			}
		}

		OrgHeader faxClient;
		OrgHeader PrintClient
		{
			get
			{
				return printClient ?? (printClient = NewClientContact(ContactNotifyModes.Print));
			}
		}

		OrgHeader printClient;
		OrgHeader NewClientContact(string deliveryMethod)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = "Client" + deliveryMethod;
			OrgContact contact = result.Contacts.AddNew();
			contact.OC_ContactName = deliveryMethod;
			contact.OC_Fax = "131314";
			contact.OC_Email = "unit.test@cw1.com";
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DeliverBy = deliveryMethod;
			document.OD_DocumentGroup = ContactType.All.Code;
			return result;
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			RawDataRegistry.Instance.EnglishSpelling.SetValue(Guid.Parse("22c79b3e-cd3e-4ca1-8fc9-6da7ab1bd061"), Guid.Empty, Guid.Empty, Core.Constants.Languages.EnglishAmerican);
		}

		LoggerForTest Logger
		{
			get
			{
				return logger ?? (logger = new LoggerForTest());
			}
		}

		LoggerForTest logger;
		class LoggerForTest : ILogger
		{
			#region ILogger Members
			public void Log(LogType type, string message, Exception ex)
			{
				messages.Add(new KeyValuePair<LogType, string>(type, message));
			}

			public void Log(LogType type, string message)
			{
				messages.Add(new KeyValuePair<LogType, string>(type, message));
			}

			#endregion
			public override string ToString()
			{
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < messages.Count; i++)
				{
					builder.Append(messages[i].Key);
					builder.Append(" - ");
					builder.Append(messages[i].Value);
					builder.AppendLine();
				}

				return builder.ToString();
			}

			readonly List<KeyValuePair<LogType, string>> messages = new List<KeyValuePair<LogType, string>>();
		}
		#endregion
	}
}

using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	public abstract class WorkItemCommonTest<T> : EnterpriseBusinessObjectTestCase
		where T : WorkItemCommon
	{
		#region TestAutoLogging

		public void TestIsAutoAdminBusinessObjectLoggerEnabled()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				// EDI has its own configuration on EdiAutoLoggedTablesDefaultConfigValues.cs, so we need to override the default value
				var defaultValues = SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.DefaultValue;
				using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValues))
				{
					AssertEquals(true, CachedWorkItem.IsAutoAdminBusinessObjectLoggerEnabled);
				}
			}
		}

		public void TestAuditStmALogDeciderConfig()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var registryValue = ObjectFactory.Get<ISystemDataRegistry>().AuditLogsEnabledFor(WorkItemSchema.Constants.TableName);
				Assert("Should find record for audit logs", registryValue.HasValue);
				AssertEquals((false, false, false), registryValue.Value);
			}
		}

		#endregion

		#region TestOnSaving

		public void TestOnSaving()
		{
			var biz1 = (WorkItemCommon)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			var jobHeader1 = new JobHeader.Loader(biz1).TryLoadOrCreate();
			Factory.Save();

			var biz2 = (WorkItemCommon)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			var jobHeader2 = new JobHeader.Loader(biz2).TryLoadOrCreate();
			Factory.Save();

			AssertEquals("WI00000001", biz1.WKI_WorkItemNumber);
			AssertEquals("WI00000002", biz2.WKI_WorkItemNumber);
			AssertEquals(jobHeader1.JH_JobNum, biz1.WKI_WorkItemNumber);
			AssertEquals(jobHeader2.JH_JobNum, biz2.WKI_WorkItemNumber);
		}

		#endregion

		#region TestAllowInvoiceDeletion

		public void TestAllowInvoiceDeletion()
		{
			IJobHeaderParent bizo = CachedWorkItem;
			AssertEquals(ExpectedAllowInvoiceDeletion, bizo.AllowInvoiceDeletion);
		}
		public virtual bool ExpectedAllowInvoiceDeletion { get { return true; } }

		#endregion

		#region TestCreatedTimeAsText

		[TestUtcOffset(7, 0, 0)]
		public void TestCreatedTimeAsText()
		{
			var workItem = (WorkItemCommon)GetNewBusinessObject();
			workItem.WKI_SystemCreateTimeUtc = ZDateTime.Now;
			AssertEquals(workItem.WKI_SystemCreateTimeUtc.AddHours(7).ToString(DateTimeFormatStrings.LongTimeFormat), workItem.CreatedTimeAsText);
		}

		#endregion

		#region TestInvoicingSupporter

		public void TestInvoicingSupporter()
		{
			var workItem = (WorkItemCommon)GetNewBusinessObject();
			var testJob = (IJobInvoicingPlugIn)workItem;
			AssertType(ExpectedJobInvoicingSupporterType, testJob.InvoicingSupporter);
		}

		public virtual Type ExpectedJobInvoicingSupporterType { get { return typeof(WorkItemInvoicingSupporter); } }

		#endregion

		#region TestIHaveServices

		public void TestIHaveServices()
		{
			var workItem = (WorkItemCommon)GetNewBusinessObject();
			IHaveServices iHaveServices = workItem;

			AssertEquals("", iHaveServices.ContainerMode);
			AssertEquals("", iHaveServices.TransportMode);
			AssertEquals(0, iHaveServices.DependentServiceParents.Length);
			AssertEquals(workItem, iHaveServices.ServiceParent);
			AssertEquals(workItem.TablePrefix, iHaveServices.TableCode);
		}

		public virtual void TestServices()
		{
			var workItem = (WorkItemCommon)GetNewBusinessObject();
			JobService service = Factory.New<JobService>();
			service.ES_ParentID = workItem.PK;
			service.ES_ParentTableCode = workItem.TablePrefix;

			AssertCollectionContains("Collection was not loaded and/or the relationship filter is incorrect.", service, workItem.Services);
			AssertEquals(typeof(JobService), workItem.Services.TypeOfElements);
			AssertEquals(true, workItem.IsRegisteredEditableChildObject(workItem.Services));
		}

		public void TestServiceBranch() => TestServiceBranchCore();

		protected virtual void TestServiceBranchCore()
		{
			var workItem = (WorkItemCommon)GetNewBusinessObject();
			var iHaveServices = (IHaveServices)workItem;
			AssertNull("No service branch", iHaveServices.ServiceBranch);

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			workItem.WKI_GB_AssignedBranch = branch.PK;
			AssertEquals("Service branch", branch.PK, iHaveServices.ServiceBranch.PK);
		}

		#endregion

		#region CachedWorkItem

		protected WorkItemCommon CachedWorkItem
		{
			get { return (WorkItemCommon)CachedBusinessObject; }
		}

		#endregion

		#region TestJob

		public void TestJob()
		{
			var workItem = (WorkItemCommon)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			AssertNull(workItem.Job);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = workItem.PK;
			job.JH_ParentTableCode = WorkItemSchema.Constants.Prefix;
			AssertNotNull(workItem.Job);
		}

		#endregion

		#region TestAttribute

		public void TestBusinessObjectAttribute()
		{
			var workItem = (WorkItemCommon)GetNewBusinessObject();
			var codePropertyAttribute = (Attribute[])workItem.GetType().GetCustomAttributes(typeof(CodePropertyAttribute), true);

			AssertEquals("You should implement [CodeProperty(BizObj.Schema.XX_CodePropertyName)]", 1, codePropertyAttribute.Length);
			AssertEquals("Code Property Attribute", ExpectedCodePropertyAttribute, ((ZPropertyAttribute)codePropertyAttribute[0]).PropertyName);

			var descriptionPropertyAttribute = (Attribute[])workItem.GetType().GetCustomAttributes(typeof(DescriptionPropertyAttribute), true);
			AssertEquals("You should implement [DescriptionProperty(BizObj.Schema.XX_DescriptionPropertyName)]", 1, descriptionPropertyAttribute.Length);
			AssertEquals("Description Property Attribute", ExpectedDescriptionPropertyAttribute, ((ZPropertyAttribute)descriptionPropertyAttribute[0]).PropertyName);
		}

		public void TestHtmlProperty()
		{
			var workItem = (WorkItemCommon)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, workItem.WKI_Details);
			AssertEquals(ZBlob.Empty, workItem.WKI_Details_HTML);

			workItem.WKI_Details_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(workItem.WKI_Details.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", workItem.WKI_Details_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var workItem = (WorkItemCommon)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, workItem.WKI_Details);
			AssertEquals(ZBlob.Empty, workItem.WKI_Details_HTML);

			workItem.WKI_Details = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", workItem.WKI_Details_HTML.ToUTF8());

			workItem.WKI_Details = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", workItem.WKI_Details_HTML.ToUTF8());
		}

		protected virtual string ExpectedCodePropertyAttribute => "WKI_WorkItemNumber";
		protected virtual string ExpectedDescriptionPropertyAttribute => "WKI_Summary";

		#endregion

		#region TestILocation

		public void TestILocationMembers()
		{
			var workItem = (WorkItemCommon)GetNewBusinessObject();
			workItem.WKI_PortOrCountry = ZString.Empty;
			var location = (ILocation)workItem;

			AssertNotNull("Precondition: WorkItem should not be null", workItem);
			AssertNotNull("Precondition: Location should not be null", location);
			AssertNull("CityTown should be null", location.CityTown);
			AssertEquals("Code should be empty", ZString.Empty, location.Code);
			AssertEquals("Description should be empty", ZString.Empty, location.Description);
			AssertNull("Country should be null", location.Country);
			AssertNull("IATACityCode should be null", location.IATACityCode);
			AssertEquals("IsActive should be false", false, location.IsActive);
			AssertNull("State should be null", location.State);
			AssertNull("UNLOCO should be null", location.UNLOCO);
			AssertEquals("Zones should be empty", 0, location.Zones.Length);

			workItem.WKI_PortOrCountry = "AUSYD";

			AssertEquals("CityTown", "SYDNEY", location.CityTown.Description);
			AssertEquals("Code", "AUSYD", location.Code);
			AssertEquals("Description", "Sydney", location.Description);
			AssertEquals("Country", "AU", location.Country.Code);
			AssertEquals("IATACityCode", "SYD", location.IATACityCode.Code);
			AssertEquals("IsActive", true, location.IsActive);
			AssertEquals("State", "NSW", location.State.RW_Code);
			AssertEquals("UNLOCO", "Sydney", location.UNLOCO.Description);
			AssertEquals("Zones items count", 3, location.Zones.Length);

			workItem.WKI_PortOrCountry = "AU";

			AssertNull("CityTown should be null", location.CityTown);
			AssertEquals("Code", "AU", location.Code);
			AssertEquals("Description", "Australia", location.Description);
			AssertEquals("Country", "AU", location.Country.Code);
			AssertNull("IATACityCode should be null", location.IATACityCode);
			AssertEquals("IsActive should match", true, location.IsActive);
			AssertNull("State should be null", location.State);
			AssertNull("UNLOCO should be null", location.UNLOCO);
			AssertEquals("Zones items count", 1, location.Zones.Length);
		}

		#endregion
	}
}

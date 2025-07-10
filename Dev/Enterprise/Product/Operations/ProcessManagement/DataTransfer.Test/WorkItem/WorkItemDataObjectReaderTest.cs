using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	class WorkItemDataObjectReaderTest : ActivityDataObjectReaderTestCase<WorkItemDataObjectReader, WorkItem>
	{
		public void TestPopulateDataObject_NoDepartmentElement()
		{
			var activity = CreateActivityWithContact();
			var businessObject = ReadBusinessObject(activity);

			AssertEquals("No department was specified, which is fine, so this field should simply be empty. SAD!", ZGuid.Empty, businessObject.WKI_GE_AssignedDepartment);
		}

		public void TestPopulateDataObject_DepartmentNameNotProvided()
		{
			var currentDepartment = GlbDepartment.CurrentDepartment;
			var activity = CreateActivityWithContact();
			activity.Department = new Department { Code = currentDepartment.GE_Code };
			WorkItem workItem = null;

			AssertNoExceptionThrown("Providing XML with a company that only has a Code specified shouldn't throw an exception. SAD!", () => workItem = ReadBusinessObject(activity));
			AssertEquals(currentDepartment.GE_Desc, workItem.AssignedDepartment.GE_Desc);
		}

		public void TestPopulateDataObject_NoCountryElement()
		{
			var activity = CreateActivityWithContact();
			var businessObject = ReadBusinessObject(activity);

			AssertEquals("No country was specified, which is fine, so this field should simply be empty. SAD!", ZString.Empty, businessObject.WKI_PortOrCountry);
		}

		public void TestPopulateDataObject_NoCompanyElement()
		{
			var activity = CreateActivityWithContact();
			var businessObject = ReadBusinessObject(activity);

			AssertEquals("No company was specified, which is fine, so this field should simply be empty. SAD!", ZGuid.Empty, businessObject.WKI_GC_AssignedCompany);
		}

		public void TestPopulateDataObject_CompanyNameNotProvided()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var activity = CreateActivityWithContact();
			activity.Company = new Company { Code = currentCompany.GC_Code };
			WorkItem workItem = null;

			AssertNoExceptionThrown("Providing XML with a company that only has a Code specified shouldn't throw an exception. SAD!", () => workItem = ReadBusinessObject(activity));
			AssertEquals(currentCompany.CompanyName, workItem.AssignedCompany.CompanyName);
		}

		protected override void AssertDescription(WorkItem businessObject)
		{
			AssertEquals("Lonely Gal Margarita Mix for One", businessObject.WKI_Details.ToUTF8());
		}

		protected override void AssertBranch(WorkItem businessObject)
		{
			Assert("Field is not used for Work Items.", true);
		}

		protected override void AssertDepartment(WorkItem businessObject)
		{
			AssertEquals("DEP", businessObject.AssignedDepartment.GE_Code);
		}

		protected override void AssertCompany(WorkItem businessObject)
		{
			AssertEquals("COM", businessObject.AssignedCompany.GC_Code);
		}

		protected override void AssertClient1(WorkItem businessObject)
		{
			Assert("Field is not used for Work Items.", true);
		}

		protected override void AssertClient2(WorkItem businessObject)
		{
			Assert("Field is not used for Work Items.", true);
		}

		protected override void AssertCountry(WorkItem businessObject)
		{
			AssertEquals("LB", businessObject.WKI_PortOrCountry);
		}

		protected override void AssertProjectManager(WorkItem businessObject)
		{
			Assert("Field is not used for Work Items.", true);
		}

		protected override IEnumerable<BusinessObject> GetRelatedItemsForTestsCore(BusinessObjectFactory factory, OrgContact contact)
		{
			var project = ProcessMgmtTestHelper.CreateProject(factory, "Yeah, I'd like to order one large person with extra people please.", client: contact);
			project.WKP_ProjectNumber = "PROJECTNUM";
			yield return project;

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(factory, "Doctors. No, no, no, no. Architects. And dentists on half.");
			ticket.WKR_RequestNumber = "TICKETNUM";
			ticket.WKR_OC_Client = contact.PK;
			yield return ticket;
		}

		protected override IEnumerable<string> ExpectedRelatedItemDescriptions => new[] { "Yeah, I'd like to order one large person with extra people please.", "Doctors. No, no, no, no. Architects. And dentists on half." };

		protected override string ExpectedValidRelatedItemTypes => "[Customer Service Ticket], [Project]";

		protected override ZPropertyInfo GetStatusPropertyInfo(WorkItem workItem)
		{
			return workItem.WKI_StatusInfo;
		}

		protected override ZString GetCreateUserCode(WorkItem workItem)
		{
			return workItem.WKI_SystemCreateUser;
		}

		protected override DataContextType ExpectedDataContextType => DataContextType.WorkItem;
	}
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	class ProjectDataObjectReaderTest : ActivityDataObjectReaderTestCase<ProjectDataObjectReader, Project>
	{
		public void TestReadActivity_WithNullClientInformation()
		{
			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = new DataContext() };
			activity.DataContext.AddDataTarget(DataContextType.Project, null);
			activity.Summary = "This is not a test";

			var project = ReadBusinessObject(activity);

			AssertEquals(ZGuid.Empty, project.WKP_OC_Contact);
			AssertEquals(ZGuid.Empty, project.WKP_OA_ClientAddress);
		}

		public void TestReadActivity_WithClientAddressSpecified_ButNoContactSpecified()
		{
			var contact = ProcessMgmtTestHelper.CreateOrganizationAndContact(BizoFactory);
			var address = contact.Header.MainAddress;
			BizoFactory.Save();

			var organizationAddress = OrganizationAddressHelper.GetAddressDataObject(address, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, address)), nameof(ActivityOrganizationAddressType.Client));

			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = new DataContext() };
			activity.DataContext.AddDataTarget(DataContextType.Project, null);
			activity.Summary = "This is not a test";
			activity.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { organizationAddress });

			var project = ReadBusinessObject(activity);

			AssertEquals(ZGuid.Empty, project.WKP_OC_Contact);
			AssertEquals(address.PK, project.WKP_OA_ClientAddress);
		}

		protected override void AssertDescription(Project businessObject)
		{
			AssertEquals("Lonely Gal Margarita Mix for One", businessObject.WKP_Details.ToUTF8());
		}

		protected override void AssertSelectionCriterion4(Project project)
		{
			AssertEquals("Selection criterion 4 is not used for Projects.", ZString.Empty, project.SelectionCriterion4);
			AssertEquals("Priority not stored in selection criterion 4 for Projects.", "DDD", project.WKP_Priority);
		}

		protected override void AssertSelectionCriterion5(Project project)
		{
			AssertEquals("Selection criterion 5 is not used for Projects.", ZString.Empty, project.SelectionCriterion5);
		}

		protected override void AssertBranch(Project businessObject)
		{
			Assert("Field is not used for Projects.", true);
		}

		protected override void AssertDepartment(Project businessObject)
		{
			Assert("Field is not used for Projects.", true);
		}

		protected override void AssertCompany(Project businessObject)
		{
			Assert("Field is not used for Projects.", true);
		}

		protected override void AssertClient1(Project businessObject)
		{
			AssertEquals("Jan Michael Vincent", businessObject.Contact.Name);
			AssertEquals("123 Canada Street", businessObject.ClientAddress.Address1);
		}

		protected override void AssertClient2(Project businessObject)
		{
			AssertEquals("Mrs Sullivan", businessObject.TechnicalContact.Name);
		}

		protected override void AssertCountry(Project businessObject)
		{
			Assert("Field is not used for Projects.", true);
		}

		protected override void AssertProjectManager(Project businessObject)
		{
			AssertEquals("Janet", businessObject.ProjectManager.GS_FullName);
		}

		protected override IEnumerable<BusinessObject> GetRelatedItemsForTestsCore(BusinessObjectFactory factory, OrgContact contact)
		{
			var project = ProcessMgmtTestHelper.CreateProject(factory, "Yeah, I'd like to order one large phone with extra phones please.", client: contact);
			project.WKP_ProjectNumber = "PROJECTNUM";
			yield return project;

			var workitem = ProcessMgmtTestHelper.CreateWorkItem(factory, "Cellphone. No, no, no, no. Rotary. And pay phone on half.");
			workitem.WKI_WorkItemNumber = "TICKETNUM";
			yield return workitem;
		}

		protected override IEnumerable<string> ExpectedRelatedItemDescriptions => new[] { "Yeah, I'd like to order one large phone with extra phones please.", "Cellphone. No, no, no, no. Rotary. And pay phone on half." };

		protected override string ExpectedValidRelatedItemTypes => "[Project], [Work Item]";

		protected override ZPropertyInfo GetStatusPropertyInfo(Project businessObject)
		{
			return businessObject.WKP_StatusInfo;
		}

		protected override IEnumerable<ZString> FilterOutNotesWeDontCareAboutForThisTest(IEnumerable<ZString> allNotes)
		{
			return base.FilterOutNotesWeDontCareAboutForThisTest(allNotes).Where(x => !x.Contains("Project Created"));
		}

		protected override ZString GetCreateUserCode(Project project)
		{
			return project.WKP_SystemCreateUser;
		}

		protected override DataContextType ExpectedDataContextType => DataContextType.Project;
	}
}

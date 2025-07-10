using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer
{
	public class ProjectDataObjectWriter : ProcessManagementActivityDataObjectWriter<Project>
	{
		public ProjectDataObjectWriter(IDataWritingManager writeManager, bool shouldIncludeRelatedItems)
			: base(writeManager, shouldIncludeRelatedItems)
		{
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.Project;
		}

		protected override void PopulateDataObjectCore(Project project, Activity activity)
		{
			PopulateFields(activity, project);
			PopulateClients(activity, project);
			PopulateProjectManager(activity, project);
		}

		static void PopulateFields(Activity activity, Project project)
		{
			activity.Summary = project.WKP_Summary;
			activity.Description = project.WKP_Details.ToUTF8();
			activity.Status = GetCodeDescriptionPair(project.WKP_Status, project.Lookups.StatusList);
			activity.SelectionCriterion1 = GetCodeDescriptionPair(project.WKP_Type, project.Lookups.AllTypes);
			activity.SelectionCriterion2 = GetCodeDescriptionPair(project.WKP_SubType, project.Lookups.AllSubtypes);
			activity.SelectionCriterion3 = GetCodeDescriptionPair(project.WKP_Module, project.Lookups.AllModules);
			activity.SelectionCriterion4 = GetCodeDescriptionPair(project.WKP_Priority, project.Lookups.AllPriorities);
		}

		void PopulateClients(Activity activity, Project project)
		{
			var contact = project.Contact;
			var address = project.ClientAddress;

			if (contact != null && address == null)
			{
				address = contact.Header.MainAddress;
			}

			if (address != null)
			{
				AddClient(activity, address, contact, ActivityOrganizationAddressType.Client);
			}

			var technicalContact = project.TechnicalContact;

			if (technicalContact != null)
			{
				AddClient(activity, technicalContact.Header.MainAddress, technicalContact, ActivityOrganizationAddressType.TechnicalClient);
			}
		}

		void AddClient(Activity activity, OrgAddress address, OrgContact contact, ActivityOrganizationAddressType type)
		{
			OrganizationAddress GetUniversalAddress() => ActivityOrganizationHelper.GetAddress(address, writeManager, type, contact);

			if (activity.OrganizationAddressCollection == null)
			{
				activity.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { GetUniversalAddress() });
			}
			else
			{
				activity.OrganizationAddressCollection.Add(GetUniversalAddress());
			}
		}

		static void PopulateProjectManager(Activity activity, Project project)
		{
			var manager = project.ProjectManager;

			if (manager != null)
			{
				activity.ProjectManager = Staff.New(manager);
			}
		}

		protected override SchemaStringColumn CreatedByColumn => WorkProjectSchema.WKP_SystemCreateUser;
	}
}

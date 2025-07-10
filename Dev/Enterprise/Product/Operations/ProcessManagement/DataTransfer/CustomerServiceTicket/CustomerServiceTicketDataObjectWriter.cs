using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer
{
	public class CustomerServiceTicketDataObjectWriter : ProcessManagementActivityDataObjectWriter<WorkRequest>
	{
		public CustomerServiceTicketDataObjectWriter(IDataWritingManager writeManager, bool shouldIncludeRelatedItems)
			: base(writeManager, shouldIncludeRelatedItems)
		{
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.CustomerServiceTicket;
		}

		protected override void PopulateDataObjectCore(WorkRequest ticket, Activity activity)
		{
			PopulateFields(activity, ticket);
			PopulateBranch(activity, ticket);
			PopulateDepartment(activity, ticket);
			PopulateClient(activity, ticket);
		}

		static void PopulateFields(Activity activity, WorkRequest ticket)
		{
			activity.Summary = ticket.WKR_Summary;
			activity.Description = ticket.WKR_Description;
			activity.Status = GetCodeDescriptionPair(ticket.WKR_Status, ticket.Lookups.StatusList);
			activity.SelectionCriterion1 = GetCodeDescriptionPair(ticket.WKR_SelectionCriteria1, ticket.Lookups.SelectionCriterion1Values);
			activity.SelectionCriterion2 = GetCodeDescriptionPair(ticket.WKR_SelectionCriteria2, ticket.Lookups.SelectionCriterion2Values);
			activity.SelectionCriterion3 = GetCodeDescriptionPair(ticket.WKR_SelectionCriteria3, ticket.Lookups.SelectionCriterion3Values);
			activity.SelectionCriterion4 = GetCodeDescriptionPair(ticket.WKR_SelectionCriteria4, ticket.Lookups.SelectionCriterion4Values);
			activity.SelectionCriterion5 = GetCodeDescriptionPair(ticket.WKR_SelectionCriteria5, ticket.Lookups.SelectionCriterion5Values);
			activity.Location = new CodeDescriptionPair5Char { Code = ticket.WKR_RN_NKCountry, Description = ticket.Country?.Description ?? ZString.Empty };
		}

		static void PopulateBranch(Activity activity, WorkRequest ticket)
		{
			var branch = ticket.Branch;

			if (branch != null)
			{
				activity.Branch = Branch.New(branch);
			}
		}

		static void PopulateDepartment(Activity activity, WorkRequest ticket)
		{
			var department = ticket.Department;

			if (department != null)
			{
				activity.Department = Department.New(department);
			}
		}

		void PopulateClient(Activity activity, WorkRequest ticket)
		{
			var contact = ticket.Client;
			var orgHeader = contact.Header;
			var addressElement = ActivityOrganizationHelper.GetAddress(orgHeader, writeManager, ActivityOrganizationAddressType.Client, contact);

			activity.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(1) { addressElement });
		}

		protected override SchemaStringColumn CreatedByColumn => WorkRequestSchema.WKR_SystemCreateUser;
	}
}

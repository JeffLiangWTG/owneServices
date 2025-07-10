using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer
{
	public class WorkItemDataObjectWriter : ProcessManagementActivityDataObjectWriter<WorkItem>
	{
		public WorkItemDataObjectWriter(IDataWritingManager writeManager, bool shouldIncludeRelatedItems)
			: base(writeManager, shouldIncludeRelatedItems)
		{
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.WorkItem;
		}

		protected override void PopulateDataObjectCore(WorkItem workItem, Activity activity)
		{
			PopulateFields(activity, workItem);
			PopulateDepartment(activity, workItem);
			PopulateCompany(activity, workItem);
		}

		void PopulateFields(Activity activity, WorkItem workItem)
		{
			activity.Summary = workItem.WKI_Summary;
			activity.Description = workItem.WKI_Details.ToUTF8();
			activity.Status = GetCodeDescriptionPair(workItem.WKI_Status, workItem.Lookups.StatusList);
			activity.SelectionCriterion1 = GetCodeDescriptionPair(workItem.WKI_WorkItemType, workItem.Lookups.AllTypes);
			activity.SelectionCriterion2 = GetCodeDescriptionPair(workItem.WKI_WorkItemArea, workItem.Lookups.AllAreas);
			activity.SelectionCriterion3 = GetCodeDescriptionPair(workItem.WKI_ActivityType, workItem.Lookups.AllActivityTypes);
			activity.SelectionCriterion4 = GetCodeDescriptionPair(workItem.WKI_ActivitySubtype, workItem.Lookups.AllActivitySubtypes);
			activity.SelectionCriterion5 = GetCodeDescriptionPair(workItem.WKI_Priority, workItem.Lookups.AllPriorities);

			var locationDescription = workItem.WKI_PortOrCountry.IsEmpty ? ZString.Empty : workItem.Lookups.Locations.GetDescriptionFromCode(workItem.WKI_PortOrCountry);
			activity.Location = new CodeDescriptionPair5Char { Code = workItem.WKI_PortOrCountry, Description = locationDescription };
		}

		static void PopulateDepartment(Activity activity, WorkItem workItem)
		{
			var department = workItem.AssignedDepartment;

			if (department != null)
			{
				activity.Department = Department.New(department);
			}
		}

		static void PopulateCompany(Activity activity, WorkItem workItem)
		{
			var company = workItem.AssignedCompany;

			if (company != null)
			{
				activity.Company = Company.New(company);
			}
		}

		protected override SchemaStringColumn CreatedByColumn => WorkItemSchema.WKI_SystemCreateUser;
	}
}

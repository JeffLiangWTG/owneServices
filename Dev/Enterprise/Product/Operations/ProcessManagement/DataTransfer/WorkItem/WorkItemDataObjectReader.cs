using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer
{
	public class WorkItemDataObjectReader : ProcessManagementActivityDataObjectReader<WorkItem>
	{
		public WorkItemDataObjectReader(Activity dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		protected override void PopulateFields(WorkItem businessObject)
		{
			SetValue(businessObject, WorkItemSchema.WKI_Summary, dataObject.Summary);
			SetValue(businessObject, WorkItemSchema.WKI_Details, ZBlob.FromUTF8(dataObject.Description ?? ZString.Empty));
			SetValue(businessObject, WorkItemSchema.WKI_WorkItemType, dataObject.SelectionCriterion1?.Code);
			SetValue(businessObject, WorkItemSchema.WKI_WorkItemArea, dataObject.SelectionCriterion2?.Code);
			SetValue(businessObject, WorkItemSchema.WKI_ActivityType, dataObject.SelectionCriterion3?.Code);
			SetValue(businessObject, WorkItemSchema.WKI_ActivitySubtype, dataObject.SelectionCriterion4?.Code);
			SetValue(businessObject, WorkItemSchema.WKI_Priority, dataObject.SelectionCriterion5?.Code);
		}

		public override DataContextType DataContextType => DataContextType.WorkItem;

		protected override SchemaGuidColumn CompanyColumn => WorkItemSchema.WKI_GC_AssignedCompany;

		protected override SchemaStringColumn LocationColumn => WorkItemSchema.WKI_PortOrCountry;

		protected override SchemaGuidColumn DepartmentColumn => WorkItemSchema.WKI_GE_AssignedDepartment;
	}
}

using CargoWise.Schema;
using Enterprise.ProcessManagement.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer
{
	public class CustomerServiceTicketDataObjectReader : ProcessManagementActivityDataObjectReader<WorkRequest>
	{
		public CustomerServiceTicketDataObjectReader(Activity dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.CustomerServiceTicket;

		protected override void PopulateFields(WorkRequest ticket)
		{
			SetValue(ticket, WorkRequestSchema.WKR_Summary, dataObject.Summary);
			SetValue(ticket, WorkRequestSchema.WKR_Description, dataObject.Description);
			SetValue(ticket, WorkRequestSchema.WKR_SelectionCriteria1, dataObject.SelectionCriterion1?.Code);
			SetValue(ticket, WorkRequestSchema.WKR_SelectionCriteria2, dataObject.SelectionCriterion2?.Code);
			SetValue(ticket, WorkRequestSchema.WKR_SelectionCriteria3, dataObject.SelectionCriterion3?.Code);
			SetValue(ticket, WorkRequestSchema.WKR_SelectionCriteria4, dataObject.SelectionCriterion4?.Code);
			SetValue(ticket, WorkRequestSchema.WKR_SelectionCriteria5, dataObject.SelectionCriterion5?.Code);
			SetValue(ticket, WorkRequestSchema.WKR_RN_NKCountry, dataObject.Location?.Code);
		}

		protected override SchemaGuidColumn BranchColumn => WorkRequestSchema.WKR_GB_Branch;
		protected override SchemaGuidColumn DepartmentColumn => WorkRequestSchema.WKR_GE_Department;
		protected override SchemaStringColumn LocationColumn => WorkRequestSchema.WKR_RN_NKCountry;
		protected override SchemaGuidColumn Client1Column => WorkRequestSchema.WKR_OC_Client;

		protected override ActivityOrganizationAddressType Client1AddressType => ActivityOrganizationAddressType.Client;
	}
}

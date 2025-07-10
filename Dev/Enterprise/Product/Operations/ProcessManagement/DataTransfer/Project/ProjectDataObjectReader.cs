using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer
{
	public class ProjectDataObjectReader : ProcessManagementActivityDataObjectReader<Project>
	{
		public ProjectDataObjectReader(Activity dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.Project;

		protected override void PopulateFields(Project project)
		{
			SetValue(project, WorkProjectSchema.WKP_Summary, dataObject.Summary);
			SetValue(project, WorkProjectSchema.WKP_Details, ZBlob.FromUTF8(dataObject.Description ?? ZString.Empty));
			SetValue(project, WorkProjectSchema.WKP_Type, dataObject.SelectionCriterion1?.Code);
			SetValue(project, WorkProjectSchema.WKP_SubType, dataObject.SelectionCriterion2?.Code);
			SetValue(project, WorkProjectSchema.WKP_Module, dataObject.SelectionCriterion3?.Code);
			SetValue(project, WorkProjectSchema.WKP_Priority, dataObject.SelectionCriterion4?.Code);
			SetValue(project, WorkProjectSchema.WKP_GS_NKProjectManager, dataObject.ProjectManager?.Code);
		}

		protected override void SetAddress(Project project, OrgAddress address, ActivityOrganizationAddressType addressType)
		{
			base.SetAddress(project, address, addressType);

			if (addressType == ActivityOrganizationAddressType.Client)
			{
				project.WKP_OA_ClientAddress = address.PK;
			}
		}

		protected override SchemaGuidColumn Client1Column => WorkProjectSchema.WKP_OC_Contact;
		protected override SchemaGuidColumn Client2Column => WorkProjectSchema.WKP_OC_TechnicalContact;
		protected override ActivityOrganizationAddressType Client1AddressType => ActivityOrganizationAddressType.Client;
		protected override ActivityOrganizationAddressType Client2AddressType => ActivityOrganizationAddressType.TechnicalClient;
		protected override bool IsClient1Required => false;
		protected override bool IsClient2Required => false;
	}
}

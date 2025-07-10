using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkRequestLookups : AutoWorkRequestLookups
	{
		public WorkRequestLookups(AutoWorkRequest parent)
			: base(parent)
		{
		}

		public new WorkRequest Parent
		{
			get { return (WorkRequest)base.Parent; }
		}

		public ICodeDescriptionPairList StatusList => Factory.GetCachedValue<TicketStatusList>();

		public ICodeDescriptionPairList SelectionCriterion1Values => ProcessManagementRegistry.Instance.SelectionCriterion1Values.Value;
		public ICodeDescriptionPairList SelectionCriterion2Values => ProcessManagementRegistry.Instance.SelectionCriterion2Values.Value;
		public ICodeDescriptionPairList SelectionCriterion3Values => ProcessManagementRegistry.Instance.SelectionCriterion3Values.Value;
		public ICodeDescriptionPairList SelectionCriterion4Values => ProcessManagementRegistry.Instance.SelectionCriterion4Values.Value;
		public ICodeDescriptionPairList SelectionCriterion5Values => ProcessManagementRegistry.Instance.SelectionCriterion5Values.Value;

		public OrgHeaderCollection Organisations => Factory.GetCachedValue("OrgHeaderCollection", delegate { return new OrgHeaderCollection(Factory); });

		public OrgContactCollection ClientsFilteredByOrganisation
		{
			get
			{
				var organisation = Factory.Load<OrgHeader>(Parent.OrganisationPK);

				return organisation != null
				? new OrgContactCollection(Factory, new ZQuery(OrgContactSchema.OC_OH, organisation.PK))
				{
					FilterBusinessObjectDefaults = { new FilterBusinessObjectDefault("Organisation", "Property", organisation.PK, false) }
				}
				: new OrgContactCollection(Factory);
			}
		}
	}
}

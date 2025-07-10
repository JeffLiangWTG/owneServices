using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OpportunityProcessTasksCollection : ProcessTaskCollection
	{
		public OpportunityProcessTasksCollection(OrgOpportunity opportunity) : base(opportunity)
		{
		}

		public new OpportunityProcessTasks this[int index]
		{
			get { return (OpportunityProcessTasks)Elements[index]; }
		}

		public new OpportunityProcessTasks AddNew()
		{
			return (OpportunityProcessTasks)base.AddNew();
		}

		OrgOpportunity ParentOpportunity
		{
			get { return (OrgOpportunity)base.Parent; }
		}

		#region Contacts and Addresses

		public override bool SupportsContactAndAddress
		{
			get { return true; }
		}

		public override ZString OriginCountry
		{
			get { return Country; }
		}

		public override ZString DestinationCountry
		{
			get { return Country; }
		}

		ZString Country
		{
			get { return ParentOpportunity.Header != null ? ParentOpportunity.Header.CountryCode : ZString.Empty; }
		}

		#endregion

		#region Default Values

		protected internal override void SetDefaultsForNewChildCore(IWorkflowProviderCollection collection, ProcessTask task, bool defaultAssignedStaff)
		{
			base.SetDefaultsForNewChildCore(collection, task, defaultAssignedStaff);

			task.P9_ParentID = ParentOpportunity.PK;
			task.P9_ParentTableCode = OrgOpportunitySchema.Constants.Prefix;
			task.OrganisationPK = ParentOpportunity.P8_OH;
			task.P9_OA = ParentOpportunity.P8_OA;
			task.P9_OC = ParentOpportunity.P8_OC;
			if (task.P9_GS_NKAssignedStaffMember.IsEmpty && task.P9_G4_RequiredCapability.IsEmpty)
			{
				task.P9_GS_NKAssignedStaffMember = ParentOpportunity.P8_GS_NKPrimarySalesPerson;
			}
		}

		#endregion
	}
}

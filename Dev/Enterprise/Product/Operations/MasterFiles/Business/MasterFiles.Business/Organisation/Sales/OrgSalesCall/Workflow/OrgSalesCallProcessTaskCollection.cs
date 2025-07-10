namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallProcessTaskCollection : ProcessTaskCollection
	{
		public OrgSalesCallProcessTaskCollection(OrgSalesCall parent)
			: base(parent)
		{
		}

		public new OrgSalesCallProcessTask this[int index]
		{
			get { return (OrgSalesCallProcessTask)Elements[index]; }
		}

		public new OrgSalesCallProcessTask AddNew()
		{
			return (OrgSalesCallProcessTask)base.AddNew();
		}

		new OrgSalesCall Parent
		{
			get { return (OrgSalesCall)base.Parent; }
		}

		public override bool SupportsContactAndAddress
		{
			get { return false; }
		}

		#region Default Values

		protected internal override void SetDefaultsForNewChildCore(IWorkflowProviderCollection collection, ProcessTask task, bool defaultAssignedStaff)
		{
			base.SetDefaultsForNewChildCore(collection, task, defaultAssignedStaff);

			task.OrganisationPK = Parent.OQ_OH;
			var org = Parent.Header;

			if (!Parent.OQ_OA_LocationAddress.IsEmpty)
			{
				task.P9_OA = Parent.OQ_OA_LocationAddress;
			}
			else if (org != null && org.MainAddress != null)
			{
				task.P9_OA = org.MainAddress.PK;
			}

			if (!task.P9_ParentTemplateID.IsEmpty && Count == 0 // first template task
				&& task.P9_GS_NKAssignedStaffMember.IsEmpty
				&& !Parent.OQ_GS_NKSalesRep.IsEmpty)
			{
				task.P9_GS_NKAssignedStaffMember = Parent.OQ_GS_NKSalesRep;
			}
		}

		#endregion
	}
}

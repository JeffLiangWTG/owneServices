using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class LockComplianceBook : LockReleaseComplianceBook
	{
		public LockComplianceBook(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override bool IsLock
		{
			get { return true; }
		}

		protected override void OnFactorySaving()
		{
			if (ComplianceSequenceBo != null)
			{
				ComplianceSequenceBo.XD_LockBy = GlbStaff.CurrentUser.PK;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				ComplianceSequenceBo.Logs.AddNew(Events.EditedARecord, "Compliance book locked");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			base.OnFactorySaving();
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class ReleaseComplianceBook : LockReleaseComplianceBook
	{
		public ReleaseComplianceBook(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override bool IsLock
		{
			get { return false; }
		}

		protected override void OnFactorySaving()
		{
			if (ComplianceSequenceBo != null)
			{
				ComplianceSequenceBo.XD_LockBy = ZGuid.Empty;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				ComplianceSequenceBo.Logs.AddNew(Events.EditedARecord, "Compliance book released");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			base.OnFactorySaving();
		}
	}
}

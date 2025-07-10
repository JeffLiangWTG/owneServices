using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class ReleaseComplianceBookController : ZSingletonController
	{
		public ReleaseComplianceBookController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccComplianceSequence;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ReleaseComplianceBook; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ReleaseComplianceBook); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new LockReleaseComplianceBookForm((ReleaseComplianceBook)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed ? Env.Security.ComplianceSequencesModifyLockRelease : Env.Security.ComplianceSequencesModifyReleaseOtherStaff; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new ReleaseComplianceBook(Factory);
		}
	}
}

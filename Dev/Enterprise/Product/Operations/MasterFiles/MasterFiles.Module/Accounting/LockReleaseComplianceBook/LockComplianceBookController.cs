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
	public class LockComplianceBookController : ZSingletonController
	{
		public LockComplianceBookController()
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
			get { return ControllerIDs.LockComplianceBook; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(LockComplianceBook); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new LockReleaseComplianceBookForm((LockComplianceBook)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ComplianceSequencesModifyLockRelease; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new LockComplianceBook(Factory);
		}
	}
}

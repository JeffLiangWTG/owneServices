using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccComplianceSequnceBulkController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccComplianceSequenceBulkForm((AccComplianceSequenceBulkCreator)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccComplianceSequenceBulk; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccComplianceSequenceBulkCreator); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new AccComplianceSequenceBulkCreator(Factory);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ComplianceSequencesActionsBulkCreate; }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}

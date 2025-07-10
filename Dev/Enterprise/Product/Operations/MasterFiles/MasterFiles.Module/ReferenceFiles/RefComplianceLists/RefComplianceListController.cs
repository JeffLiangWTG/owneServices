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
	public class RefComplianceListController : ZController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.RefComplianceList; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefComplianceList; }
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefComplianceList);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.RefComplianceListView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.RefComplianceListEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefComplianceListForm((RefComplianceList)businessEntity);
		}
	}
}

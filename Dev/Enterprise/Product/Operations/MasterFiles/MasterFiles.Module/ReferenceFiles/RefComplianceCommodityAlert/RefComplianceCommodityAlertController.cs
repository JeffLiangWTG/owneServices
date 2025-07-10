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
	public class RefComplianceCommodityAlertController : ZController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.RefComplianceCommodityAlert; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefComplianceCommodityAlert; }
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefComplianceCommodityAlert);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.RefComplianceCommodityAlertView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.RefComplianceCommodityAlertEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefComplianceCommodityAlertForm((RefComplianceCommodityAlert)businessEntity);
		}
	}
}

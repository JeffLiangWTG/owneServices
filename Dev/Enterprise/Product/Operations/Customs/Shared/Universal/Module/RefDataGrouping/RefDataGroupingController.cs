using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class RefDataGroupingController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.Universal.RefDataGrouping;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.RefDataGrouping;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefDataGrouping);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlobalCodesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlobalCodesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlobalCodesEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlobalCodesDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}
	}
}

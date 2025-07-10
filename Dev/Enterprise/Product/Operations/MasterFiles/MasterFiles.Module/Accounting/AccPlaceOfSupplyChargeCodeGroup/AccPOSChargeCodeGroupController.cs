using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccPOSChargeCodeGroupController : ZController
	{
		public AccPOSChargeCodeGroupController() : base()
		{
		}

		public override ControllerID ID => ControllerIDs.AccPlaceOfSupplyChargeCodeGroup;

		protected override IZForm GetForm(IBusiness businessEntity) => new AccPOSChargeCodeGroupForm((AccPOSChargeCodeGroup)businessEntity);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.AccPlaceOfSupplyChargeCodeGroup;

		public override Type TypeOfTopLevelBusinessObject => typeof(AccPOSChargeCodeGroup);

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (!sourceEntity.CanDelete)
			{
				Globals.Message.Show(sourceEntity.ReasonForNotAbleToDelete);
			}
			else
			{
				base.ShowDeleteForm(sourceEntity);
			}
			return LastShownForm;
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			// No template copying supported at the moment
			LastShownForm = null;
			return null;
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.POSChargeCodeGroupsDelete;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.POSChargeCodeGroupsModify;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.POSChargeCodeGroupsNew;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.POSChargeCodeGroups;

		#endregion
	}
}

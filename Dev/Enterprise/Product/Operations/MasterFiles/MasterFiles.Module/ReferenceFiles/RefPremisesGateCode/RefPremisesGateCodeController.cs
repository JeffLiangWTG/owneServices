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
	public class RefPremisesGateCodeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefPremisesGateCode; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefPremisesGateCode; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefPremisesGateCode); }
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			IZForm resultForm;
			if (!sourceEntity.CanDelete)
			{
				Globals.Message.Show(sourceEntity.ReasonForNotAbleToDelete);
				resultForm = LastShownForm;
			}
			else
			{
				resultForm = base.ShowDeleteForm(sourceEntity);
			}
			return resultForm;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefPremisesGateCodeForm((RefPremisesGateCode)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.PremisesGateCodeModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PremisesGateCodeModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PremisesGateCodeModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PremisesGateCode; }
		}

#if DEBUG
		#region For Testing

		internal SecurityCheckpoint CheckPointForDeleteForTesting => CheckPointForDelete;

		internal SecurityCheckpoint CheckPointForEditForTesting => CheckPointForEdit;

		internal SecurityCheckpoint CheckPointForNewForTesting => CheckPointForNew;

		internal SecurityCheckpoint CheckPointForViewForTesting => CheckPointForView;

		#endregion
#endif
	}
}

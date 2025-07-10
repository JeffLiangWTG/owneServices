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
	public class RefPackTypeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefPackType); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefPackTypeForm((RefPackType)businessEntity);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.RefPackTypeModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.RefPackTypeModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.RefPackTypeModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.RefPackType; }
		}

		#endregion

#if DEBUG
		#region For Testing

		internal SecurityCheckpoint CheckPointForEditForTesting => CheckPointForEdit;

		internal SecurityCheckpoint CheckPointForDeleteForTesting => CheckPointForDelete;

		internal SecurityCheckpoint CheckPointForNewForTesting => CheckPointForNew;

		internal SecurityCheckpoint CheckPointForViewForTesting => CheckPointForView;

		#endregion
#endif

		#region IDs

		public override ControllerID ID
		{
			get { return ControllerIDs.RefPackType; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefPackType; }
		}

		#endregion
	}
}

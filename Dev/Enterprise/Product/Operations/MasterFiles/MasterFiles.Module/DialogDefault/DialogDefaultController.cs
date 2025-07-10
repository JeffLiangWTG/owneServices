using System;
using CargoWise.EntityFramework;
using Enterprise.Core.DialogDefault;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	class DialogDefaultController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.DialogDefault; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DialogDefault; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmDialogDefault); }
		}

		#region Checkpoints

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return GetCheckPointForView(bizObject);
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject sourceEntity)
		{
			return GetCheckPointForView(sourceEntity);
		}

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject sourceEntity)
		{
			var defaults = (StmDialogDefault)sourceEntity;
			return defaults == null || defaults.SDD_Owner != Env.CurrentUser.PK ?
				Env.Security.CanCreateAndModifyGlobalDialogDefaults :
				Env.Security.CanModifyOwnDefaults;
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return new SecurityCheckpoint("DialogDefault", ResString.GetMultilingualString("8E6B3F1F-34CA-4CBE-AEF0-CE7C2F938EBE", "You cannot directly create a new default"), null, Env.Instance.Security, false); }
		}

		#endregion

		#region Implemented

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var defaults = (StmDialogDefault)businessEntity;
			defaults.TryToFormatXml = true;

			return new DialogDefaultEditForm((StmDialogDefault)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DialogDefault; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}

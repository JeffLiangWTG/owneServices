using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbGroupController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GlbGroupController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlbGroup; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbGroup; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbGroup); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			GlbGroup group = (GlbGroup)businessEntity;
			return new GlbGroupForm(group);
		}

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GroupsModify; }
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject sourceEntity)
		{
			if (sourceEntity == null)
			{
				return Env.Security.GroupsModify;
			}
			else if ((sourceEntity as GlbGroup).IsFixedDatabaseAccessGroup)
			{
				return new ReadOnlySecurityCheckpoint((sourceEntity as GlbGroup).GG_Code, ResString.GetMultilingualString("1f4f5f05-75dd-4b7d-bcd6-3b5066fe6ca8", "Cannot edit database access group."));
			}

			return ((GlbGroup)sourceEntity).IsCurrentUserGroupOwnerForThisGroup
				? Env.Security.FindOrCreateGroupOwnerSecurityCheckpoint(sourceEntity.PK.ToGuid())
				: Env.Security.GroupsModify;
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { throw new NotSupportedException("Access depends on two checkpoints"); }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GroupsView; }
		}

		class ReadOnlySecurityCheckpoint : SecurityCheckpoint
		{
			public ReadOnlySecurityCheckpoint(string code, MultilingualString displayText) : base(code, displayText, null, null, false) { }

			public override void ShowError() => Globals.Message.Show(DisplayText);

			public override bool IsAllowed => false;
		}

		#endregion
	}
}

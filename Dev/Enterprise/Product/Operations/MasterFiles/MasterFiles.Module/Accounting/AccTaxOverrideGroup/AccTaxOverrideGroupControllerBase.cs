using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public abstract class AccTaxOverrideGroupControllerBase : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected AccTaxOverrideGroupControllerBase()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccTaxOverrideGroup;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccTaxOverrideGroup); }
		}

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

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TaxOverrideGroupsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.TaxOverrideGroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TaxOverrideGroupsNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TaxOverrideGroups; }
		}

		protected override SecurityCheckpoint GetCheckPointForCopy(BusinessObject inMemorySourceEntity)
		{
			return Env.Security.TaxOverrideGroupsCopy;
		}

		#endregion
	}
}

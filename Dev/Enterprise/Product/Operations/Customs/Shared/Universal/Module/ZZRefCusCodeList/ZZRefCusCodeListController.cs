using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusCodeListController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.Universal.ZZRefCusCodeList; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.Universal.ZZRefCusCodeList; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ZZRefCusCodeListCombined); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GlobalCodesDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GlobalCodesEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GlobalCodesNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GlobalCodesView; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCusCodeListForm((ZZRefCusCodeListCombined)businessEntity);
		}

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get
			{
				return base.AlreadyDeletedOrIrreversiblyChangedMessageCore + System.Environment.NewLine + Res.GetString("{530E875A-14B4-417B-9F71-CC99B4CD61F0}", "If you have just created a user-editable copy of a system-generated record, please re-run your search query and try again.");
			}
		}
	}
}

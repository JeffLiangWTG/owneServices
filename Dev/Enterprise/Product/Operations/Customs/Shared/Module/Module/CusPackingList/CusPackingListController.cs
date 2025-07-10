using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class CusPackingListController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.CusPackingList;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CusPackingList;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusPackingList);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsPackingListView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsPackingListEdit;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsPackingListEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GUI.PackingListForm((CusPackingList)businessEntity);
		}
	}
}

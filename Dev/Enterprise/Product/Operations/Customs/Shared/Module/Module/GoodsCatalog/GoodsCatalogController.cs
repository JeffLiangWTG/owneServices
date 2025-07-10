using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class GoodsCatalogController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.GoodsCatalog;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.GoodsCatalog;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject => typeof(BaseCusGoodsCatalog);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GoodsCatalogView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GoodsCatalogNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GoodsCatalogEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GoodsCatalogDelete;

		protected override IZForm GetForm(IBusiness businessEntity) => new CusGoodsCatalogForm((BaseCusGoodsCatalog)businessEntity);
	}
}

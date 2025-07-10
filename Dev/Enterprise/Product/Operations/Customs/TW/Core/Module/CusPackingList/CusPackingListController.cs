using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Module
{
	class CusPackingListController : Customs.Module.CusPackingListController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusPackingList);

		protected override IZForm GetForm(IBusiness businessEntity) => new PackingListForm((CusPackingList)businessEntity);
	}
}

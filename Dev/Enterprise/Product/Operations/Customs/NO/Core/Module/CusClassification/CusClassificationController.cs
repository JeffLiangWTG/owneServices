using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NO.Business;
using Enterprise.Customs.NO.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Module
{
	public class CusClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusClassification);

		protected override IZForm GetForm(IBusiness businessEntity) => new CusClassificationForm((CusClassification)businessEntity);
	}
}

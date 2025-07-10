using System;
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.Customs._CustomsTemplate_.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs._CustomsTemplate_.Module
{
	public class CusClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusClassification);

		protected override IZForm GetForm(IBusiness businessEntity) => new CusClassificationForm((CusClassification)businessEntity);
	}
}

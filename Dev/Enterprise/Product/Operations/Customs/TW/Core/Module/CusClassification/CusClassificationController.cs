using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Module
{
	public class CusClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusClassification); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusClassificationForm((CusClassification)businessEntity);
		}
	}
}

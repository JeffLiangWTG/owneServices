using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
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

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SingleTariffClassification; }
		}
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	public class ExportClassificationController : Customs.Module.ExportClassificationController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusClassification);

		protected override IZForm GetForm(IBusiness businessEntity) => new ExportClassificationForm((CusClassification)businessEntity);
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Module
{
	public class GuaranteesController : Customs.Module.GuaranteesController
	{
		public GuaranteesController() : base()
		{
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusGuaranteeHeader);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GuaranteeForm((CusGuaranteeHeader)businessEntity);
		}
	}
}


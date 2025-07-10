using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.Module
{
	public class ConsolidatedDeclarationController : Customs.Module.ConsolidatedDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(ConsolidatedDeclaration);

		protected override IZForm CreateEditForm(IBusiness businessEntity)
		{
			return new ConsolidatedDeclarationForm(businessEntity as ConsolidatedDeclaration, new ConsolidatedDeclarationFormAdaptationsProvider());
		}
	}
}

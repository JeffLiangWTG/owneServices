using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business;

namespace Enterprise.Customs.NZ.Module
{
	public class ConsolidatedDeclarationModule : Customs.Module.ConsolidatedDeclarationModule
	{
		protected override IBusinessObjectCollection GetNewGridCollection() => new Customs.Business.ConsolidatedDeclarationCollection<ConsolidatedDeclaration>(Factory, Customs.Business.ConsolidatedDeclaration.ApplicationCodes.TSW);
	}
}

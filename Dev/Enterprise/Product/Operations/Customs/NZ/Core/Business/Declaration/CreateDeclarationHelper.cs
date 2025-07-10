using System;

namespace Enterprise.Customs.NZ.Business
{
	public class CreateDeclarationHelper : Customs.Business.CreateDeclarationHelper, Integration.Customs.NZ.ICreateDeclarationHelper
	{
		protected override bool HasMoreThanOneTypeOfJobDeclarationPerShipment
		{
			get { return true; }
		}

		protected override Type GetTypeForNewJobDeclaration()
		{
			return typeof(Declaration.JobDeclaration);
		}
	}
}

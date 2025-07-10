using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	public class JobDeclarationControllerTest : EU.Module.Testing.JobDeclarationControllerTest
	{
		public override Type ControllerToBashType => typeof(JobDeclarationController);

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var dec = base.GetBusinessObjectWithoutValidationErrors() as JobDeclaration;
			dec.ZG_CountryOfSupply = Core.Constants.CountryCodes.Turkey;
			dec.ZG_ShippingCountry = Core.Constants.CountryCodes.Turkey;
			dec.DischargeOffice = "TR210300";
			dec.DischargePlace = "Place Name";
			return dec;
		}
	}
}


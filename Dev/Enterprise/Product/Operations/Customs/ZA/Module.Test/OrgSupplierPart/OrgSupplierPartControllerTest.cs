using System;
using Enterprise.Customs.ZA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	sealed class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;

		protected override Type GetBusinessObjectType() => typeof(OrgSupplierPart);

		public override Type ControllerToBashType => typeof(OrgSupplierPartController);
	}
}

using System;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	sealed class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		public override Type ControllerToBashType => typeof(OrgSupplierPartController);

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override Type GetBusinessObjectType() => typeof(OrgSupplierPart);
	}
}

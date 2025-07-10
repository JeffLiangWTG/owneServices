using System;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(ExportClassificationController))]
	sealed class ExportClassificationControllerTest : Customs.Module.Testing.ExportClassificationControllerTest
	{
		public override Type ControllerToBashType => typeof(ExportClassificationController);

		protected override Type GetBusinessObjectType() => typeof(CusClassification);

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}

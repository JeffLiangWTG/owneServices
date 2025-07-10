using System;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(ImportClassificationController))]
	sealed class ImportClassificationControllerTest : Customs.Module.Testing.ImportClassificationControllerTest
	{
		public override Type ControllerToBashType => typeof(ImportClassificationController);

		protected override Type GetBusinessObjectType() => typeof(CusClassification);

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}

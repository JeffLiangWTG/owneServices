using System;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(JobDeclarationShipmentController))]
	sealed class JobDeclarationShipmentControllerTest : Customs.Module.Testing.JobDeclarationShipmentControllerTest
	{
		public override Type ControllerToBashType => typeof(JobDeclarationShipmentController);
	}
}

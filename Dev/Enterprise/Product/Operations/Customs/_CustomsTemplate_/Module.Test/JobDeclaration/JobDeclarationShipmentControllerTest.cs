using System;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.Module.Testing
{
	[TestedType(typeof(JobDeclarationShipmentController))]
	class JobDeclarationShipmentControllerTest : Customs.Module.Testing.JobDeclarationShipmentControllerTest
	{
		public override Type ControllerToBashType => typeof(JobDeclarationShipmentController);
	}
}

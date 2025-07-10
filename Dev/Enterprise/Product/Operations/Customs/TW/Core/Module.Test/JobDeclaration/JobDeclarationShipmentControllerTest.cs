using System;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(JobDeclarationShipmentController))]
	public sealed class JobDeclarationShipmentControllerTest : Customs.Module.Testing.JobDeclarationShipmentControllerTest
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(JobDeclarationShipmentController);
			}
		}
	}
}

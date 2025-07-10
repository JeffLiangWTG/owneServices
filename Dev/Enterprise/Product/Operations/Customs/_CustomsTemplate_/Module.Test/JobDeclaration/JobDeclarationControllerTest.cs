using System;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
	{
		public override Type ControllerToBashType => typeof(JobDeclarationController);
	}
}

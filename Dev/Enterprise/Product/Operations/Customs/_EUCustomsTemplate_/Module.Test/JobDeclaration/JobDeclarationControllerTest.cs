using System;
using NUnit.Framework;

namespace Enterprise.Customs._EUCustomsTemplate_.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	sealed class JobDeclarationControllerTest : EU.Module.Testing.JobDeclarationControllerTest
	{
		public override Type ControllerToBashType => typeof(JobDeclarationController);
	}
}

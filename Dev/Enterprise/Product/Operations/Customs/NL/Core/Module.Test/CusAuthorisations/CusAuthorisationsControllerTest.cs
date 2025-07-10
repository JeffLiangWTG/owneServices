using System;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Module.Testing;

[TestedType(typeof(CusAuthorisationsController))]
class CusAuthorisationsControllerTest : Customs.Module.Testing.CusAuthorisationsControllerTest
{
	public override Type ControllerToBashType => typeof(CusAuthorisationsController);
}

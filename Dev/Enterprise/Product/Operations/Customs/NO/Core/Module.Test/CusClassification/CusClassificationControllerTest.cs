using System;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing
{
	[TestedType(typeof(CusClassificationController))]
	sealed class CusClassificationControllerTest : Customs.Module.Testing.SingleTariffClassificationControllerTest
	{
		public override Type ControllerToBashType => typeof(CusClassificationController);
	}
}

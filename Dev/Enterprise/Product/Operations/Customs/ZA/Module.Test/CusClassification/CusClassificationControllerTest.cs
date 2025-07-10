using System;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CusClassificationController))]
	sealed class CusClassificationControllerTest : Customs.Module.Testing.SingleTariffClassificationControllerTest
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(CusClassificationController);
			}
		}
	}
}

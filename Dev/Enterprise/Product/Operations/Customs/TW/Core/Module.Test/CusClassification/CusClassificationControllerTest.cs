using System;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(CusClassificationController))]
	public sealed class CusClassificationControllerTest : Customs.Module.Testing.SingleTariffClassificationControllerTest
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

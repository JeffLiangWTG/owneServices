using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusRulingController))]
	internal class ZZRefCusRulingControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.Universal.ZZRefCusRuling;
		}

		public override Type ControllerToBashType
		{
			get
			{
				return typeof(ZZRefCusRulingController);
			}
		}
	}
}

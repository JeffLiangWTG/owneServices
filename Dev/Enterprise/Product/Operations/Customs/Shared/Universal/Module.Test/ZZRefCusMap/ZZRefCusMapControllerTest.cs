using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusMapController))]
	public class ZZRefCusMapControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.Universal.ZZRefCusMap;
		}

		public override Type ControllerToBashType
		{
			get
			{
				return typeof(ZZRefCusMapController);
			}
		}
	}
}

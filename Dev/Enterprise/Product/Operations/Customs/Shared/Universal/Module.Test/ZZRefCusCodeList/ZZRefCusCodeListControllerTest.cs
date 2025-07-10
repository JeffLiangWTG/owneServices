using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusCodeListController))]
	public class ZZRefCusCodeListControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.Universal.ZZRefCusCodeList;
		}

		public override Type ControllerToBashType
		{
			get
			{
				return typeof(ZZRefCusCodeListController);
			}
		}
	}
}

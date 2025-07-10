using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USCRuleUserControlTest))]
	sealed class USCRuleUserControlTest : CustomsUserControlBasherAbstractTest
	{
		public void TestGridId()
		{
			using (var control = new USCRuleUserControl())
			{
				var grid = control.FindSingle<ZModuleButtonGrid>("TariffsModuleButtonGrid");
				AssertEquals("02d8b038-2fcc-4d6a-98c1-a40d16a5d521", grid.GridId);
			}
		}

		public override ZString MessageTypeForFormBashing => ZString.Empty;

		public override Type FormToBashType => typeof(ZForm);

		protected override Type UserControlToBashType => typeof(USCRuleUserControlTest);
	}
}

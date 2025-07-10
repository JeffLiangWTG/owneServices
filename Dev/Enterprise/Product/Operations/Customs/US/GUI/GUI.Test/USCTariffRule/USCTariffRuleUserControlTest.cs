using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USCTariffRuleUserControlTest))]
	sealed class USCTariffRuleUserControlTest : CustomsUserControlBasherAbstractTest
	{
		public void TestGridId()
		{
			using (var control = new USCTariffRuleUserControl())
			{
				var grid = control.FindSingle<ZGrid>("ExceptionsGrid");
				AssertEquals("8053b13a-7ef7-407e-ace3-fe7c3d78c54c", grid.GridId);
			}
		}

		public override ZString MessageTypeForFormBashing => ZString.Empty;

		public override Type FormToBashType => typeof(ZForm);

		protected override Type UserControlToBashType => typeof(USCTariffRuleUserControlTest);
	}
}

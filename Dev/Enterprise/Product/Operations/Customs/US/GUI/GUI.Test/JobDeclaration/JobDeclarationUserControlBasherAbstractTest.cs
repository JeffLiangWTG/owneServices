using System;

namespace Enterprise.Customs.US.GUI
{
	abstract class JobDeclarationUserControlBasherAbstractTest : Testing.CustomsUserControlBasherAbstractTest
	{
		protected override Type UserControlToBashType => typeof(USJobDeclarationUserControl);
	}
}

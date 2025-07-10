using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Module.Testing
{
	[TestedType(typeof(PalletTransactionController))]
	internal class PalletTransactionControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PalletTransaction;
		}
	}
}

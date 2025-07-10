using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(SalesProductController))]
	public class SalesProductControllerTest : ZControllerBasherTest
	{
		#region Standard Overrides

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.SalesProduct;
		}

		#endregion
	}
}

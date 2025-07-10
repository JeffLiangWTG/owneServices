using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccTaxOverrideGroupController))]
	sealed class AccTaxOverrideGroupControllerTest : AccTaxOverrideGroupControllerBaseTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccTaxOverrideGroup;
		}
	}
}

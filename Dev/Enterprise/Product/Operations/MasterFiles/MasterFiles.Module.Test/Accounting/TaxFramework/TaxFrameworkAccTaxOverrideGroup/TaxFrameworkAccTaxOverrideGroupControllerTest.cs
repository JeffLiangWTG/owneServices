using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(TaxFrameworkAccTaxOverrideGroupController))]
	sealed class TaxFrameworkAccTaxOverrideGroupControllerTest : AccTaxOverrideGroupControllerBaseTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.TaxFrameworkAccTaxOverrideGroup;
		}
	}
}

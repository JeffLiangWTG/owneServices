using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgSecondaryTypeModuleFilter))]
	sealed class OrgSecondaryTypeModuleFilterTest : ModuleTextFilterTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgSecondaryTypeModuleFilter("Test", (value) => { return new ZQuery(); }, new CodeDescriptionPairList());
		}

		#endregion
	}
}

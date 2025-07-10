using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SailingScheduleDefaultFilterProvider))]
	sealed class SailingScheduleDefaultFilterProviderSeaTest : SailingScheduleDefaultFilterProviderTest
	{
		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobSeaSailing; }
		}

		#endregion
	}
}

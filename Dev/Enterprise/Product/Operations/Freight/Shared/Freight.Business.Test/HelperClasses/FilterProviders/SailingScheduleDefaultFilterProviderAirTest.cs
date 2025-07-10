using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SailingScheduleDefaultFilterProvider))]
	sealed class SailingScheduleDefaultFilterProviderAirTest : SailingScheduleDefaultFilterProviderTest
	{
		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobAirSailing; }
		}

		#endregion
	}
}

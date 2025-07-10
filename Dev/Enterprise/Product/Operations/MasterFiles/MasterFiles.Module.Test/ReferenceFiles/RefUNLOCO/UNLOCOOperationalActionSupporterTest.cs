using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UNLOCOOperationalActionSupporter))]
	sealed class UNLOCOOperationalActionSupporterTest : OperationalActionSupporterTest<UNLOCOOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefUNLOCO; }
		}

		public override bool ShouldSupportDocuments
		{
			get { return false; }
		}
	}
}

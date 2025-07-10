using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.ProcessManagement.ProcessTasks.Testing
{
	[TestedType(typeof(ProcessTasksOperationalActionSupporter))]
	sealed class ProcessTasksOperationalActionSupporterTest : OperationalActionSupporterTest<ProcessTasksOperationalActionSupporter>
	{
		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ProcessTasks; }
		}

		public override bool ShouldSupportDocuments
		{
			get { return false; }
		}

		#endregion
	}
}

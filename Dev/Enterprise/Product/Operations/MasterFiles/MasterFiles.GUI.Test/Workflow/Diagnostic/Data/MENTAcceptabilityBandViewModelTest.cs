using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Workflow.Test
{
	[TestedType(typeof(StmJobQueueViewModel))]
	sealed class MENTAcceptabilityBandViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new StmJobQueueViewModel(null);
		}
	}
}

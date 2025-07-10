using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Workflow.Test
{
	[TestedType(typeof(WTELogViewModelCollection))]
	sealed class WTELogViewModelCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WTELogViewModelCollection>
	{
		protected override WTELogViewModelCollection GetCollectionToTest()
		{
			return new WTELogViewModelCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WTELogViewModel(new BusinessObjectFactory().New<ProcessTask>(), null);
		}
	}
}

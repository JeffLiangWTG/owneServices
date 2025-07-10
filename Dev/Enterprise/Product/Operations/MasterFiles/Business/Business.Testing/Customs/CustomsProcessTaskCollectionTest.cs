using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Customs.Testing
{
	[TestedType(typeof(ProcessTaskCollection<ProcessTask, BusinessObject>))]
	sealed class CustomsProcessTaskCollectionTest : ProcessTaskCollectionTest<ProcessTaskCollection<ProcessTask, BusinessObject>>
	{
		protected override ProcessTaskCollection<ProcessTask, BusinessObject> GetCollectionToTestCore()
		{
			var parent = Factory.New<OrgOpportunity>();
			return new ProcessTaskCollection<ProcessTask, BusinessObject>(parent);
		}
	}
}

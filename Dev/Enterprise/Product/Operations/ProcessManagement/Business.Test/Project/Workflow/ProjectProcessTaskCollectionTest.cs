using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectProcessTaskCollection))]
	class ProjectProcessTaskCollectionTest : ProcessTaskCollectionTest<ProjectProcessTaskCollection>
	{
		protected override ProjectProcessTaskCollection GetCollectionToTestCore()
		{
			var project = Factory.New<Project>();
			return new ProjectProcessTaskCollection(project);
		}
	}
}

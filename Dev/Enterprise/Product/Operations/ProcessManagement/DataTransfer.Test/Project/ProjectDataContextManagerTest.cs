using System;
using Enterprise.ProcessManagement.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	[TestedType(typeof(ProjectDataContextManager))]
	class ProjectDataContextManagerTest : ActivityDataContextManagerTestCase<ProjectDataContextManager, Project>
	{
		protected override DataContextType ExpectedDataContextType => DataContextType.Project;
		protected override Type ExpectedDataObjectWriterType => typeof(ProjectDataObjectWriter);
	}
}

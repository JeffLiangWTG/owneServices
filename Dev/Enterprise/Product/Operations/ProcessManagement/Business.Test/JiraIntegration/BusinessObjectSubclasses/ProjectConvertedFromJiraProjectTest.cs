using System;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectConvertedFromJiraProject))]
	class ProjectConvertedFromJiraProjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.Project);
	}

	[TestedType(typeof(ProjectConvertedFromJiraProject))]
	class ProjectConvertedFromJiraProjectRelatedItemSourceTest : WorkTaskRelatedItemSourceTestCase
	{
	}

	[TestedType(typeof(ProjectConvertedFromJiraProject))]
	class ProjectConvertedFromJiraProjectRelatedItemTest : ProjectRelatedItemTestCase
	{
	}
}

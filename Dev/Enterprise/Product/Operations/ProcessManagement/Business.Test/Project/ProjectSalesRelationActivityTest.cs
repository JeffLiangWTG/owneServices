using CargoWise.Schema;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(Project))]
	sealed class ProjectSalesRelationActivityTest : SalesRelationActivityTestCase<Project>
	{
		protected override ITableSchema TableSchema
		{
			get { return WorkProjectSchema.Instance; }
		}

		protected override Project GetNewActivity()
		{
			return Factory.NewWithValidTestData<Project>();
		}
	}

	[TestedType(typeof(Project))]
	sealed class ProjectRelatableActivityTest : RelatableActivityTestCase<Project>
	{
		protected override Project GetNewActivity()
		{
			return Factory.NewWithValidTestData<Project>();
		}
	}
}

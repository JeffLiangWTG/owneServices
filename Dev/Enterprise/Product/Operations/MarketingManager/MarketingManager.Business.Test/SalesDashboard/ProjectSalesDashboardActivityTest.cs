using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Integration;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ProjectSalesDashboardActivity))]
	sealed class ProjectSalesDashboardActivityTest : SalesDashboardActivityTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var parent = Factory.NewWithValidTestData(ObjectFactory.GetType<IProject>());
			var activity = Factory.New<ProjectSalesDashboardActivity>();
			activity.VSA_ParentId = parent.PK;
			return activity;
		}
	}
}

using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(RelatedContainersOfJobDeclarationFilter))]
	sealed class RelatedContainersOfJobDeclarationFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFilter()
		{
			var jobDeclaration1 = Factory.New<BaseJobDeclaration>();
			var cusContainer1 = jobDeclaration1.CusContainers.AddNew();
			var jobContainer1 = Factory.New<CommonContainer>();
			jobContainer1.JC_ContainerNum = "CON001";
			cusContainer1.CO_JC = jobContainer1.PK;
			var cusContainer2 = jobDeclaration1.CusContainers.AddNew();
			var jobContainer2 = Factory.New<CommonContainer>();
			jobContainer2.JC_ContainerNum = "CON002";
			cusContainer2.CO_JC = jobContainer2.PK;
			var jobDeclaration2 = Factory.New<BaseJobDeclaration>();
			var cusContainer3 = jobDeclaration2.CusContainers.AddNew();
			var jobContainer3 = Factory.New<CommonContainer>();
			jobContainer3.JC_ContainerNum = "CON001";
			cusContainer3.CO_JC = jobContainer3.PK;
			var cusContainer4 = jobDeclaration2.CusContainers.AddNew();
			var jobContainer4 = Factory.New<CommonContainer>();
			jobContainer4.JC_ContainerNum = "XXX002";
			cusContainer4.CO_JC = jobContainer4.PK;
			var jobDeclaration3 = Factory.New<BaseJobDeclaration>();
			var cusContainer5 = jobDeclaration3.CusContainers.AddNew();
			var jobContainer5 = Factory.New<CommonContainer>();
			jobContainer5.JC_ContainerNum = "XXX001";
			cusContainer5.CO_JC = jobContainer5.PK;
			var cusContainer6 = jobDeclaration3.CusContainers.AddNew();
			var jobContainer6 = Factory.New<CommonContainer>();
			jobContainer6.JC_ContainerNum = "XXX002";
			cusContainer6.CO_JC = jobContainer6.PK;
			var jobDeclaration4 = Factory.New<BaseJobDeclaration>();
			jobDeclaration4.CusContainers.RemoveAndDeleteAll();
			Factory.Save();
			var filterStripBiz0 = new JobDeclarationFilterBusinessObject();
			var relatedContainerFilter = (RelatedContainersOfJobDeclarationFilter)filterStripBiz0["Related Containers"];
			relatedContainerFilter.IsActive = true;
			relatedContainerFilter.SelectedFilters.AddTextFilterStrip("Container #", "CON");
			relatedContainerFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var jobDeclarations = new BaseJobDeclarationCollection(Factory);
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobDeclaration1, jobDeclaration2 }, jobDeclarations);
			relatedContainerFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobDeclaration1, jobDeclaration4 }, jobDeclarations);
			relatedContainerFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			jobDeclarations.Load(filterStripBiz0.Filter);
			var pks = jobDeclarations.Select(j => j.PK);
			AssertContainsExactElementsInAnyOrder(new[] { jobDeclaration3.PK, jobDeclaration4.PK }, pks);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RelatedContainersOfJobDeclarationFilter("moo", () => new ArrayList());
		}
	}
}

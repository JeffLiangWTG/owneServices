using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskTemplateCollection))]
	sealed class ProcessTaskTemplateCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCompanyFilter()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			GlbCompany company2 = Factory.New<GlbCompany>();
			ProcessTaskTemplate template1 = Factory.New<ProcessTaskTemplate>();

			ProcessTaskTemplate template2 = Factory.New<ProcessTaskTemplate>();
			template2.P0_GC = company2.PK;

			ProcessTaskTemplate template3 = Factory.New<ProcessTaskTemplate>();
			template3.P0_GC = ZGuid.Empty;

			ProcessTaskTemplateCollection templates = new ProcessTaskTemplateCollection(Factory);
			templates.Load();
			AssertEquals("2 items only", 2, templates.Count);
			AssertCollectionContains("Correct Items", template1, templates);
			AssertCollectionContains("Correct Items", template3, templates);

			template2.P0_GC = GlbCompany.CurrentCompany.PK;
			templates.Load();
			AssertEquals("All items now exist", 3, templates.Count);
		}

		public void TestCollectionRespectsProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			AssertEquals("PRE: We're in CW Next Mode", "CargoWise", BrandingFactory.Instance.ProductName);

			var templates = MakeAProcessTaskTemplateForEveryWorkflowType();
			var myTemplatesQuery = new ZQuery(ProcessTaskTemplateSchema.PK, templates.Select(tem => tem.PK));

			Factory.Save();

			var collection = new ProcessTaskTemplateCollection(Factory, myTemplatesQuery);
			collection.Load();

			AssertEquals("PRE: We have all our templates in CW Next Mode", templates.Count, collection.ToArray().Length);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			collection = new ProcessTaskTemplateCollection(Factory, myTemplatesQuery);
			collection.Load();
			var pWiseWorkflowDescriptorCodes = ObjectFactory.Get<IWorkflowDescriptorList>()
				.Cast<CodeDescriptionPair>()
				.Select(pair => WorkflowDescriptors.Instance.TryGetValueSafe(pair.Code))
				.Where(desc => WorkflowDescriptorsTest.ProductivityWiseWorkflowTypes.Contains(desc.GetType().FullName))
				.Select(riptor => riptor.Code)
				.ToList();

			var productiveTemplates = templates.Where(tem => WorkflowDescriptors.IsAllowedForProductivityWise(tem.P0_ProcessType));

			CombineAssertions("We correctly cut freight-related module templates in PW mode", () =>
			{
				AssertEquals(WorkflowDescriptorsTest.ProductivityWiseWorkflowTypes.Length, collection.ToArray().Length);
				AssertContainsExactElementsInAnyOrder(productiveTemplates, collection.ToArray());
			});

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			collection = new ProcessTaskTemplateCollection(Factory, myTemplatesQuery);
			collection.Load();

			AssertEquals("We have all our templates in CW Next Mode", templates.Count, collection.ToArray().Length);
		}

		List<ProcessTaskTemplate> MakeAProcessTaskTemplateForEveryWorkflowType()
		{
			var templates = new List<ProcessTaskTemplate>();
			var workflowTypes = ObjectFactory.Get<IWorkflowDescriptorList>()
				.Cast<CodeDescriptionPair>()
				.Select(pair => WorkflowDescriptors.Instance.TryGetValueSafe(pair.Code))
				.OrderBy(d => d.GetType().FullName);

			foreach (var workflowType in workflowTypes)
			{
				var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = workflowType.Code;

				templates.Add(template);
			}

			AssertEquals(workflowTypes.Count(), templates.Count);

			return templates;
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ProcessTaskTemplateCollection(Factory);
		}

		#endregion
	}
}

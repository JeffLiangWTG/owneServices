using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestsSubclassesOf(typeof(IWorkTaskRelatedItemSource))]
	public abstract class WorkTaskRelatedItemSourceTestCase : TestCaseWithFactory
	{
		public void TestRelatedItems_AttachAndDetach()
		{
			var source = GetNewSourceBusinessObject();

			foreach (var info in source.SupportedRelatedItemModules.Where(x => x.AllowAttach))
			{
				using (var module = ZFilterModule.GetZFilterModule(info.ModuleID))
				{
					var relatedItem = Factory.New(module.TypeOfTopLevelBusinessObject);
					relatedItem.FillWithValidTestData();

					AssertNoExceptionThrown(() => source.RelatedItems.Add(relatedItem));
					AssertNoExceptionThrown(Factory.Save);
					AssertContainsExactElementsInAnyOrder(new[] { ((IWorkTaskRelatedItem)relatedItem).Number }, source.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.Number));

					AssertNoExceptionThrown(() => source.RelatedItems.Remove(relatedItem));
					AssertNoExceptionThrown(Factory.Save);
					AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), source.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.Number));
				}
			}

			Assert(true); // If none of the modules support attaching
		}

		public void TestRelatedItems_AddNew()
		{
			var source = GetNewSourceBusinessObject();
			var relatedItems = new List<IWorkTaskRelatedItem>();

			foreach (var info in source.SupportedRelatedItemModules.Where(x => x.AllowNew))
			{
				IWorkTaskRelatedItem relatedItem = null;
				var controller = ZControllerFactory.Create(info.ControllerID);

				AssertNoExceptionThrown(() => relatedItem = (IWorkTaskRelatedItem)source.RelatedItems.AddNew(controller.TypeOfTopLevelBusinessObject));

				source.PopulateNewRelatedItem(info.Type, relatedItem);
				relatedItems.Add(relatedItem);

				AssertNoExceptionThrown(Factory.Save);
				AssertContainsExactElementsInAnyOrder(relatedItems.Select(x => x.Number), source.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.Number));
			}

			Assert(true); // If none of the modules support New
		}

		protected virtual IWorkTaskRelatedItemSource GetNewSourceBusinessObject()
		{
			return (IWorkTaskRelatedItemSource)Factory.NewWithValidTestData(TestedTypeHelper.GetTestedType(GetType()));
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ContainerToSelectFromForPrinting))]
	sealed class ContainerToSelectFromForPrintingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestChangingJC_Calc_PrintDocumentForContainerDoesNotMarkBOsForSave()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			Factory.Save();

			Assert("Factory.Save has just occured, no changes should be recognized", !container.HasChanges);
			ContainerToSelectFromForPrinting containerForPrinting = new ContainerToSelectFromForPrinting(container);
			containerForPrinting.JC_Calc_PrintDocumentForContainer = !containerForPrinting.JC_Calc_PrintDocumentForContainer;
			Assert("Changing JC_Calc_PrintDocumentForContainer should not be recognized as a change to the related business objects", !container.HasChanges);
		}

		public void TestChangingContainerNumberForContainerDoesNotMarkBOsForSave()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			Factory.Save();

			Assert("Factory.Save has just occured, no changes should be recognized", !container.HasChanges);
			ContainerToSelectFromForPrinting containerForPrinting = new ContainerToSelectFromForPrinting(container);
			containerForPrinting.ContainerNumber = "AAAA00001";
			Assert("Changing ContainerNumber should not be recognized as a change to the related business objects", !container.HasChanges);
		}

		public void TestFCLContainersShouldPrintByDefault()
		{
			CommonContainer container1 = Factory.New<CommonContainer>();
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			CommonContainer container2 = Factory.New<CommonContainer>();
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;

			Assert("FCL container should bet set to print by default", new ContainerToSelectFromForPrinting(container1).JC_Calc_PrintDocumentForContainer);
			Assert("Non-FCL container should not be set to print by default", !new ContainerToSelectFromForPrinting(container2).JC_Calc_PrintDocumentForContainer);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			return new ContainerToSelectFromForPrinting(container);
		}
	}
}

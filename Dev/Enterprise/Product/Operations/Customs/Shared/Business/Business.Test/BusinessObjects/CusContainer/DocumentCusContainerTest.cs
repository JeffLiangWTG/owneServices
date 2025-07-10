using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DocumentCusContainer))]
	sealed class DocumentCusContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentCusContainer(Factory);
		}

		public void TestProperties()
		{
			BaseCusContainer cusContainer = Factory.New<BaseCusContainer>();
			DocumentCusContainer documentCusContainer = new DocumentCusContainer(Factory);
			documentCusContainer.Container = cusContainer;
			AssertEquals("Print Container default", true, documentCusContainer.PrintContainer);
		}

		public void TestGridColumnProperties()
		{
			BaseCusContainer cusContainer = Factory.New<BaseCusContainer>();
			cusContainer.CO_ContainerNumber = "COOO1234567";
			cusContainer.CO_FCL_LCL_AIR = "FCL";
			cusContainer.CO_Seal = "222";
			cusContainer.CO_Weight = ZDecimal.Parse("2.4");
			cusContainer.CO_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20RE").PK;

			DocumentCusContainer documentCusContainer = new DocumentCusContainer(Factory);
			documentCusContainer.Container = cusContainer;
			AssertEquals("Container number", "COOO1234567", cusContainer.CO_ContainerNumber);
			AssertEquals("Container fcl/lcl", "FCL", cusContainer.CO_FCL_LCL_AIR);
			AssertEquals("Container seal", "222", cusContainer.CO_Seal);
			AssertEquals("Container weight", ZDecimal.Parse("2.4"), cusContainer.CO_Weight);
			AssertEquals("Container code", "20RE", cusContainer.Container.RC_Code);
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	sealed class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseCusContainerTypeDecider to include a decider for this class", Factory.New(typeof(BaseCusContainer)).GetType() == typeof(CusContainer));
		}

		public void TestDeclaration()
		{
			CusContainer container = declaration.CusContainers.AddNew();
			AssertEquals(declaration, container.Declaration);
		}

		public void TestLookupsCachesInstance()
		{
			CusContainerLookups lookup1 = container.Lookups;
			CusContainerLookups lookup2 = container.Lookups;
			AssertEquals(lookup2, lookup1);
		}

		public void TestIsContainerNumberToBeAdvisedAlwaysTrueForNonExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			CusContainer container = declaration.CusContainers.AddNew();

			container.CO_ContainerNumber = "AB";
			Assert(container.IsContainerToBeAdvised);

			container.CO_ContainerNumber = "TBA1";
			Assert(container.IsContainerToBeAdvised);
		}

		public void TestIsContainerNumberToBeAdvisedForExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			CusContainer container = declaration.CusContainers.AddNew();
			Assert(!container.IsContainerToBeAdvised);

			container.CO_ContainerNumber = "TBAT";
			Assert("Container Number should be shown", container.IsContainerToBeAdvised);

			container.CO_ContainerNumber = "TBA";
			Assert("Container Number should be shown", container.IsContainerToBeAdvised);

			container.CO_ContainerNumber = "TR123456789";
			Assert("Container Number should be shown", container.IsContainerToBeAdvised);

			container.CO_ContainerNumber = "TBA56";
			Assert("Container Number should be shown", !container.IsContainerToBeAdvised);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
			declaration.Importer.OH_FullName = "Test Importer";
			declaration.Importer.MainAddress.OA_Address1 = "Importers Address";
			declaration.Importer.OH_RL_NKClosestPort = "AUSYD";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			CusContainer result = declaration.CusContainers.AddNew();
			return result;
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
		{
			ICustomLabelsProvider result = new BaseCusContainer.CustomLabelsProvider(((CusContainer)bO).Declaration);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			container = declaration.CusContainers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			CusContainer result = declaration.CusContainers.AddNew();
			Customs.Business.Testing.TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(result.Factory);
			return result;
		}

		JobDeclaration declaration;
		CusContainer container;
	}

	sealed class CusContainer_DataProviderTest : TestCaseWithFactory
	{
		public void TestIContainerInformation()
		{
			var testObject = Factory.New<CusContainer>();
			testObject.CO_ContainerNumber = "COCN";
			testObject.CO_Seal = "COSL";
			testObject.CO_SecondSeal = "COSSL";
			testObject.CO_FCL_LCL_AIR = "FCL";

			var test = testObject as IContainerInformation;
			AssertEquals("NONU-COCN", test.ContainerNumber);
			AssertEquals("COSL", test.FirstSealNumber);
			AssertEquals("COSSL", test.SecondSealNumber);
			AssertEquals("8", test.ContainerMode);

			testObject.CO_FCL_LCL_AIR = "LCL";
			AssertEquals("7", test.ContainerMode);

			testObject.CO_FCL_LCL_AIR = "EMP";
			AssertEquals("4", test.ContainerMode);

			testObject.CO_FCL_LCL_AIR = "FCG";
			AssertEquals("5", test.ContainerMode);

			testObject.CO_FCL_LCL_AIR = "";
			AssertEquals("", test.ContainerMode);

			testObject.CO_ContainerNumber = "APLU1234564";
			AssertEquals("APLU1234564", test.ContainerNumber);

			testObject.CO_ContainerNumber = "PLTT7500126";
			AssertEquals("NONU-PLTT7500126", test.ContainerNumber);
		}
	}
}

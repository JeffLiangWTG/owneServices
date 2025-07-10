using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusContainer))]
	sealed class DocCusContainerTest : DocBaseCusContainerAbstractTest<CusContainer, DocCusContainer>
	{
		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var container = Factory.New<CusContainer>();

			DocCusContainer cusContainerWrapper = DocCusContainer.New(container, declaration, Factory);
			cusContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertNotNull("Declaration wrapper should exist within the container wrapper", cusContainerWrapper.Declaration);
			AssertEquals("Document direction of declaration should match document direction of container", cusContainerWrapper.DocumentDirection, cusContainerWrapper.Declaration.DocumentDirection);
		}

		public void TestTotalAllocatedJobPackages()
		{
			var declaration = Factory.New<JobDeclaration>();

			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "APLU11222555";

			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "APLU11339484";

			Package packLine1 = declaration.Packages.AddNew();
			packLine1.CW_PackQty = 10;
			packLine1.CW_ContainerNoOrEquipmentNo = "APLU11222555";

			Package packLine2 = declaration.Packages.AddNew();
			packLine2.CW_PackQty = 18;
			packLine2.CW_ContainerNoOrEquipmentNo = "APLU11222555";

			Package packLine3 = declaration.Packages.AddNew();
			packLine3.CW_PackQty = 35;
			packLine3.CW_ContainerNoOrEquipmentNo = "APLU11339484";

			DocCusContainer container1Wrapper = DocCusContainer.New(container1, declaration, Factory);
			AssertEquals("Total Package qty for Container 1", 28, container1Wrapper.TotalAllocatedJobPackages);
			DocCusContainer container2Wrapper = DocCusContainer.New(container2, declaration, Factory);
			AssertEquals("Package qty Container 2", 35, container2Wrapper.TotalAllocatedJobPackages);
		}

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.UnitedStates; }
		}

		protected override DocCusContainer CreateContainerWrapper(CusContainer containerInternal, Enterprise.Customs.Business.BaseJobDeclaration declarationInternal)
		{
			return DocCusContainer.New(containerInternal, (JobDeclaration)declarationInternal, Factory);
		}
	}
}

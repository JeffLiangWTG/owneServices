using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusContainer))]
	[CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.SouthAfrica)]
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

		public void TestIsContainerToBeAdvised()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;

			CusContainer container = declaration.CusContainers.AddNew();
			DocCusContainer cusContainerWrapper = DocCusContainer.New(container, Factory);

			Assert("Container should be shown", cusContainerWrapper.IsContainerToBeAdvised);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			container.CO_ContainerNumber = "TBA1";
			Assert("Container should not be shown", !cusContainerWrapper.IsContainerToBeAdvised);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.SouthAfrica; }
		}

		protected override DocCusContainer CreateContainerWrapper(CusContainer containerInternal, Enterprise.Customs.Business.BaseJobDeclaration declarationInternal)
		{
			return DocCusContainer.New(containerInternal, (JobDeclaration)declarationInternal, Factory);
		}

		#endregion
	}
}

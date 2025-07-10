using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers.Testing
{
	[TestedType(typeof(DocDeclaration))]
	sealed class DocJobDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
	{
		public void TestDocDelcarationForAI()
		{
			DocDeclaration declarationWrapper = DocDeclaration.New(Declaration, Factory);
			AssertNotNull("Can create AI wrapper", declarationWrapper);
		}

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes._TemplateCountryName_; }
		}
	}
}

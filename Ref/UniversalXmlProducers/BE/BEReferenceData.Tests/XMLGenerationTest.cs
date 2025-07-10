using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	sealed class XMLGenerationTest
	{
		[Test]
		public void TestReplaceHexadecimalSymbols()
		{
			var textWithUnkownCharacter = "EG\u0002Andorra";

			Assert.That(XMLGeneration.ReplaceHexadecimalSymbols(textWithUnkownCharacter), Is.EqualTo("EGAndorra"));
		}
	}
}

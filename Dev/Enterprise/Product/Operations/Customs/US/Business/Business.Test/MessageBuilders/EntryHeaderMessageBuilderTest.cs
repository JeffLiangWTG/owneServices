using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class EntryHeaderMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGeneratorUseEntryFilerCodeFromDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("JD9");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var mock = new Mock<EntryHeaderMessageBuilder<ABIInputBlockControlGenerator>>(entryHeader);

			var generator = new ABIInputBlockControlGenerator(entryHeader);
			mock.Protected().Setup<ABIInputBlockControlGenerator>("GetNewInputBlockControlGenerator").Returns(generator);
			var message = mock.Object.PopulateMessage();
			AssertEquals("B018888XJ5", message.EM_MessageText.Left(10));
		}
	}
}

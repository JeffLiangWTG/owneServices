using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.Testing
{
	public abstract class MessageBuilderFromEntryHeaderTest : MessageBuilderTest
	{
		public abstract void TestEmptyCusEntryHeaderThrowsNoExceptions();
		protected abstract void SetupJobDeclarationAndTestCreator();

		protected TestDeclarationCreator decCreator;
		protected JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			SetupJobDeclarationAndTestCreator();
			AssertNotNull("Should be instantiated in SetupJobDeclarationAndTestCreator()", declaration);
			AssertNotNull("Should be instantiated in SetupJobDeclarationAndTestCreator()", decCreator);
		}
	}
}

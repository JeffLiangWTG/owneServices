using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	public class InvoiceLineCompleteCollectionTestOnDeclarationWithEntryInstruction : InvoiceLineCompleteCollectionTest
	{
		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			return Factory.New<BaseJobDeclarationWithEntryInstructions>();
		}
	}
}

using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionAgreementConflictEmailDocumentParserTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestParse()
		{
			var loserCommissionAgreement = Factory.New<OrgCommissionAgreement>();
			var loserCommissionAgreementItem = loserCommissionAgreement.ProductItems.AddNew();
			var winnerCommissionAgreement = Factory.New<OrgCommissionAgreement>();
			var winnerCommissionAgreementItem1 = winnerCommissionAgreement.ProductItems.AddNew();
			var winnerCommissionAgreementItem2 = winnerCommissionAgreement.ProductItems.AddNew();

			var conflicts = new[] { new CommissionAgreementItemConflict(winnerCommissionAgreementItem1, loserCommissionAgreementItem), new CommissionAgreementItemConflict(winnerCommissionAgreementItem2, loserCommissionAgreementItem) };
			var emailCreator = new CommissionAgreementConflictEmailCreator(Factory, winnerCommissionAgreement, loserCommissionAgreement, conflicts);
			var parser = new CommissionAgreementConflictEmailDocumentParser(Factory);
			parser.Parse(emailCreator, "");
		}
	}
}

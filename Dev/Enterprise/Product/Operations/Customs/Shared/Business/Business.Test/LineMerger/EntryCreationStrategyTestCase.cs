using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class EntryCreationStrategyTestCase : TestCaseWithFactory
	{
		public void TestGetKeyForLine_NoMerge()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			EntryCreationStrategy strategy = new EntryCreationStrategy(Declaration);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.PK) > -1);
		}

		public void TestGetKeyForLine_NotMergeUsingProductNumberInDescription()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription;
			EntryCreationStrategy strategy = new EntryCreationStrategy(Declaration);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.PK) > -1);
		}

		public void TestGetKeyForLine_Tariff()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			EntryCreationStrategy strategy = new EntryCreationStrategy(Declaration);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.JI_Tariff) > -1);
		}

		public void TestGetKeyForLine_Classification()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			EntryCreationStrategy strategy = new EntryCreationStrategy(Declaration);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.JI_Tariff) > -1);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.Classification.CC_LookupCode) > -1);
		}

		public void TestGetKeyForLine_ClassificationUsingClassificationDescriptionAlways()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways;
			EntryCreationStrategy strategy = new EntryCreationStrategy(Declaration);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.JI_Tariff) > -1);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.Classification.CC_LookupCode) > -1);
		}

		public void TestGetKeyForLine_PartNumber()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			EntryCreationStrategy strategy = new EntryCreationStrategy(Declaration);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.JI_Tariff) > -1);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.Classification.CC_LookupCode) > -1);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.JI_PartNo) > -1);
		}

		public void TestGetKeyForLine_PartNumberUsingProductNumberInDescription()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription;
			EntryCreationStrategy strategy = new EntryCreationStrategy(Declaration);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.JI_Tariff) > -1);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.Classification.CC_LookupCode) > -1);
			Assert(strategy.GetKeyForLine(Line).IndexOf(line.JI_PartNo) > -1);
		}

		BaseJobDeclaration declaration;
		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<BaseJobDeclaration>();
				}

				return declaration;
			}
		}

		BaseJobComInvoiceLine line;
		BaseJobComInvoiceLine Line
		{
			get
			{
				if (line == null)
				{
					line = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
					line.JI_PartNo = "PART";
					BaseCusClassification classification = Factory.New<BaseCusClassification>();
					classification.CC_Description = "CLASS";
					classification.CC_LookupCode = "LOOKUP";
					line.JI_CC = classification.PK;
				}

				return line;
			}
		}
	}
}

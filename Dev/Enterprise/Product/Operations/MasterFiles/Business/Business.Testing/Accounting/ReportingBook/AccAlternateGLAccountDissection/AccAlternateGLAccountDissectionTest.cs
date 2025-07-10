using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateGLAccountDissection))]
	sealed class AccAlternateGLAccountDissectionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAttributeDescription()
		{
			var alternateGLAccountDissection = Factory.NewWithValidTestData<AccAlternateGLAccountDissection>();
			alternateGLAccountDissection.ADC_Attribute = "ORG";
			AssertEquals(alternateGLAccountDissection.AttributeDescription, "The GL Balance will be dissected based on organization code recorded against accounting transactions");

			alternateGLAccountDissection.ADC_Attribute = "OCG";
			AssertEquals(alternateGLAccountDissection.AttributeDescription, "The GL Balance will be dissected based on the consolidation category classification group code (TPY/INT) of organization recorded against accounting transactions");

			alternateGLAccountDissection.ADC_Attribute = "TIC";
			AssertEquals(alternateGLAccountDissection.AttributeDescription, "The GL Balance will be dissected based on tax id recorded against accounting transactions differentiating tax ids without extra tax and tax ids with extra tax reported separately");

			alternateGLAccountDissection.ADC_Attribute = "LFO";
			AssertEquals(alternateGLAccountDissection.AttributeDescription, "The GL Balance will be dissected based on location of organization recorded against accounting transactions is Local or Foreign entity");

			alternateGLAccountDissection.ADC_Attribute = "LFE";
			AssertEquals(alternateGLAccountDissection.AttributeDescription, "The GL Balance will be dissected based on location of organization recorded against accounting transactions is Local, Within EU or Outside EU");

			alternateGLAccountDissection.ADC_Attribute = "SPR";
			AssertEquals(alternateGLAccountDissection.AttributeDescription, "The GL Balance will be dissected based on whether the accounting transactions relates to sales/purchases or sales/purchases returns");
		}

		public void TestCannotDelete()
		{
			var dissection = GetDissection();
			AssertEquals(true, dissection.CanDelete);

			LinkChartToAccount(dissection);
			Factory.Save();
			AssertEquals(false, dissection.CanDelete);
			AssertEquals("You cannot delete the attribute of this Chart because there is at least one Alternate Account relating to the existing attribute combinations.\r\nYou can delete all Alternate Accounts currently linked to the Parent Account<" + dissection.GLHeader.AccountNum + ">, then adjust the dissection configuration.\r\nOtherwise, please create a new Alternate Chart and new set of Alternate Accounts.", dissection.ReasonForNotAbleToDelete);
		}

		public void TestOnLoaded()
		{
			var dissection = GetDissection();
			LinkChartToAccount(dissection);
			Factory.Save();
			var originalHasLinkedBeforeLoad = dissection.OriginalHasLinkedAlternateAccounts;
			dissection = Factory.Load<AccAlternateGLAccountDissection>(dissection.PK);
			var originalHasLinkedAfterLoad = dissection.OriginalHasLinkedAlternateAccounts;
			AssertEquals(originalHasLinkedBeforeLoad, originalHasLinkedAfterLoad);
		}

		public void TestOnSaved()
		{
			var dissection = GetDissection();
			var originalHasLinkedBeforeSave = dissection.OriginalHasLinkedAlternateAccounts;
			LinkChartToAccount(dissection);
			Factory.Save();
			var originalHasLinkedAfterSave = dissection.OriginalHasLinkedAlternateAccounts;
			AssertNotEquals(originalHasLinkedBeforeSave, originalHasLinkedAfterSave);
		}

		public void TestRunPreSaveValidationCore()
		{
			var dissection = GetDissection();
			var originalHasLinkedBeforeSave = dissection.OriginalHasLinkedAlternateAccounts;
			LinkChartToAccount(dissection);
			dissection.RunPreSaveValidation();
			var originalHasLinkedAfterSave = dissection.OriginalHasLinkedAlternateAccounts;
			AssertNotEquals(originalHasLinkedBeforeSave, originalHasLinkedAfterSave);
		}

		AccAlternateGLAccountDissection GetDissection()
		{
			var accGlHeader = AccountingTestObjectCreator.CreateAccGLHeader("99.99.9999");
			return accGlHeader.AlternateGLAccountDissections.AddNew();
		}

		void LinkChartToAccount(AccAlternateGLAccountDissection dissection)
		{
			if (dissection.ADC_AAC_AlternateChart.IsEmpty)
			{
				var chart = AccountingTestObjectCreator.CreateAlternateChart("CC1");
				dissection.ADC_AAC_AlternateChart = chart.PK;
			}

			if (dissection.ADC_Attribute.IsEmpty)
			{
				dissection.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR;
			}

			Factory.Save();
			var account = AccountingTestObjectCreator.CreateAccAlternateGlAccount(dissection.ADC_AAC_AlternateChart,dissection.GLHeader.AccountNum);
			AccountingTestObjectCreator.CreateAccAlternateGlAccountAttribute(account,dissection.ADC_AG_GLHeader,attribute: dissection.ADC_Attribute);
		}

		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ??= new AccountingTestObjectCreator(Factory);

		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}

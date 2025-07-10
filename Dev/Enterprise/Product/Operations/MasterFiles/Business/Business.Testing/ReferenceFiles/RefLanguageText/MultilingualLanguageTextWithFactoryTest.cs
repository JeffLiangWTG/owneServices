using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MultilingualLanguageTextWithFactoryTest : TestCaseWithFactory
	{
		public void TestNoExtraFactoryIsCreated()
		{
			var lang1 = Factory.New<RefLanguageText>();
			lang1.RLT_ColumnName = "ANY";
			lang1.RLT_ParentTableCode = "RN";
			lang1.RLT_Text = "ANY";
			lang1.RLT_IsSystem = true;
			lang1.RLT_IsClientOverridden = true;
			lang1.RLT_Language = "DE";
			lang1.RLT_ParentId = Guid.NewGuid();
			Factory.Save();

			var countBefore = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Length;

			var multilingual = MultilingualLanguageText.GetMultilingualText(lang1.PK.ToString(), "PT", "ANY", "Text", Factory);

			multilingual.ToString("DE");
			multilingual.ToString("DE");
			multilingual.ToString("DE");

			var countAfter = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Length;
			Assert("Should not have created a Factory instance", countAfter == countBefore);
		}
	}
}

using System;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccountingCodeDescriptionWithGroupRegistryItem<ConsolidatedAccountingCategoryCollection, ConsolidatedAccountingCategoryItem>))]
	sealed class ConsolidatedAccountingCategoryRegistryItemTest : AccountingCodeDescriptionWithGroupRegistryItemTest<ConsolidatedAccountingCategoryCollection, ConsolidatedAccountingCategoryItem>
	{
		protected override ConsolidatedAccountingCategoryCollection GetDefaultValue()
		{
			var result = new ConsolidatedAccountingCategoryCollection();
			result.Add(Constants.AccountsCategory.Unrelated, (NoResString)"Unrelated company", ConsolidatedAccountingCategoryClassList.Codes.ThirdParty);
			result.Add(Constants.AccountsCategory.WhollyOwned, (NoResString)"Wholly owned subsidiary");
			result.Add(Constants.AccountsCategory.MinorityWithReporting, (NoResString)"Minority Interest with reporting");

			return result;
		}

		protected override Func<CodeDescriptionWithGroupCollection, ConsolidatedAccountingCategoryCollection> ValueConveter => ConsolidatedAccountingCategoryCollection.Converter;
	}
}

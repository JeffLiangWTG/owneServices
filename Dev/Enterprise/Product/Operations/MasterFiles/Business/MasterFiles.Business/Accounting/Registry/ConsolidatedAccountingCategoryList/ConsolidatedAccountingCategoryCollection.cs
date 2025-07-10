using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public sealed class ConsolidatedAccountingCategoryCollection : CodeDescriptionWithGroupCollection
	{
		ConsolidatedAccountingCategoryCollection(CodeDescriptionWithGroupCollection list)
			: base(list, GetConsolidatedAccountingCategoryClassList(), ConsolidatedAccountingCategoryClassList.Codes.Intercompany, 0)
		{
		}

		public ConsolidatedAccountingCategoryCollection()
			: base(null, GetConsolidatedAccountingCategoryClassList(), ConsolidatedAccountingCategoryClassList.Codes.Intercompany, 0)
		{
		}

		public static ConsolidatedAccountingCategoryCollection Converter(CodeDescriptionWithGroupCollection originalValue)
			=> new ConsolidatedAccountingCategoryCollection(originalValue);

		public static ConsolidatedAccountingCategoryCollection GetDefaultValue()
		{
			var defaultValue = new ConsolidatedAccountingCategoryCollection();
			defaultValue.Add(Constants.AccountsCategory.Unrelated, ResString.GetMultilingualString("Accounting|ConsolidatedAccountingCategoryList|Unrelated", "Unrelated company"), ConsolidatedAccountingCategoryClassList.Codes.ThirdParty);
			defaultValue.Add(Constants.AccountsCategory.WhollyOwned, ResString.GetMultilingualString("Accounting|ConsolidatedAccountingCategoryList|WhollyOwned", "Wholly owned subsidiary"));
			defaultValue.Add(Constants.AccountsCategory.MinorityWithReporting, ResString.GetMultilingualString("Accounting|ConsolidatedAccountingCategoryList|MinorityWithReporting", "Minority Interest with reporting"));
			defaultValue.Add(Constants.AccountsCategory.MinorityWithNoReporting, ResString.GetMultilingualString("Accounting|ConsolidatedAccountingCategoryList|MinorityWithNoReporting", "Minority Interest with no reporting"));
			defaultValue.Add(Constants.AccountsCategory.RelatedMinorityShareholder, ResString.GetMultilingualString("Accounting|ConsolidatedAccountingCategoryList|RelatedMinorityShareholder", "Related minority shareholder"));
			defaultValue.Add(Constants.AccountsCategory.RelatedMajorityShareholder, ResString.GetMultilingualString("Accounting|ConsolidatedAccountingCategoryList|RelatedMajorityShareholder", "Related Majority Shareholder"));
			defaultValue.Add(Constants.AccountsCategory.RelatedWhollyOwningShareholder, ResString.GetMultilingualString("Accounting|ConsolidatedAccountingCategoryList|RelatedWhollyOwningShareholder", "Related Wholly Owning Shareholder"));
			defaultValue.Add(Constants.AccountsCategory.GroupCompanyRelatedMinority, ResString.GetMultilingualString("Accounting|ConsolidatedAccountingCategoryList|GroupCompanyRelatedMinority", "Group Company Related Minority (no direct ownership)"));
			defaultValue.Add(Constants.AccountsCategory.GroupCompanyRelatedMajority, ResString.GetMultilingualString("Accounting|ConsolidatedAccountingCategoryList|GroupCompanyRelatedMajority", "Group Company Related Majority (no direct ownership)"));

			foreach (ConsolidatedAccountingCategoryItem item in defaultValue)
			{
				item.SetOriginalValue(item);
			}

			return defaultValue;
		}

		public new ConsolidatedAccountingCategoryItem this[int i]
			=> (ConsolidatedAccountingCategoryItem)base[i];

		public new ConsolidatedAccountingCategoryItem AddNew()
			=> (ConsolidatedAccountingCategoryItem)base.AddNew();

		public static MultilingualString GetGroupColumnCaption() => ResString.GetMultilingualString("Accounting|ConsolidatedAccountingCategoryList|Class", "Class");

		static ConsolidatedAccountingCategoryClassList GetConsolidatedAccountingCategoryClassList()
		{
			var result = new ConsolidatedAccountingCategoryClassList();
			result.RemoveCode(ConsolidatedAccountingCategoryClassList.Codes.All);
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var codeDescriptionWithGroupCollection = new ConsolidatedAccountingCategoryCollection();
			codeDescriptionWithGroupCollection.CurrentFallbackLevel = fallbackLevel;
			return codeDescriptionWithGroupCollection;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new ConsolidatedAccountingCategoryItem();

		protected override void FillNewElementFromCodeDescriptionPairCore(ICodeDescription pair, RegistryBusinessObject child)
		{
			base.FillNewElementFromCodeDescriptionPairCore(pair, child);

			if (pair is ICodeDescriptionWithGroup pairWithGroup
				&& child is ConsolidatedAccountingCategoryItem childListValueItem)
			{
				childListValueItem.SetOriginalValue(pairWithGroup);
			}
		}

		public void OnUpdateAction()
		{
			foreach (ConsolidatedAccountingCategoryItem item in this)
			{
				item.SetOriginalValue(item);
			}
		}
	}
}
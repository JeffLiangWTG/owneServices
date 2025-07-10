using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public static class RegistryItemExtensions
	{
		public static bool IsRegistryItemOverriden(this IRegistryItemInternals registryItem, Guid companyPK)
		{
			return registryItem?.GetCurrentValueToUse(companyPK, Guid.Empty, Guid.Empty) != ValueToUse.DefaultValue;
		}

		public static bool IsRegistryItemConfiguredInCustomsInterface(this IRegistryItemWithOtherChangedItems registryItem, Guid companyPK)
		{
			return registryItem != null &&
				(registryItem.OtherChangedItems != null && registryItem.OtherChangedItems.Any(item => item.Name == CustomsDataRegistry.Instance.LocalCountryCustomsInterface.Name && item.IsRegistryItemOverriden(companyPK)) ||
				(registryItem.OtherChangedItems == null || !registryItem.OtherChangedItems.Any(item => item.Name == CustomsDataRegistry.Instance.LocalCountryCustomsInterface.Name && !item.IsRegistryItemOverriden(companyPK))) &&
				IntegratedCountryHelper.IsCustomsInterfaceActivatedByCompany(companyPK));
		}

		public static ZString LocationOfLocalCountryCustomsInterface
		{
			get { return ((IRegistryItemInternals)CustomsDataRegistry.Instance.LocalCountryCustomsInterface).Location; }
		}

		public static void AddErrorIfCustomsInterfaceConfigured(this IRegistryItem registryItem, Guid companyPK)
		{
			var registryItemWithOtherChangedItems = registryItem as IRegistryItemWithOtherChangedItems;
			if (registryItemWithOtherChangedItems != null && registryItemWithOtherChangedItems.IsRegistryItemOverriden(companyPK) &&
				registryItemWithOtherChangedItems.IsRegistryItemConfiguredInCustomsInterface(companyPK))
			{
				AddErrorForRegistryItem(companyPK);
			}
		}

		static void AddErrorForRegistryItem(Guid companyPK)
		{
			var companyCode = (string)Db.Connection.ExecuteScalar("select GC_Code from dbo.GlbCompany where GC_PK = '" + companyPK.ToString() + "'"); // This is the registry so we don't want to use business objects here
			throw new RegistryValidationException(CannotConfigureRegistryForCompany(companyCode));
		}

		public static string CannotConfigureRegistryForCompany(ZString companyCode)
		{
			return Res.GetString("9b9ee078-1fdf-4f69-b7c8-774ad0f07dfb", "Cannot configure for company {0} because a configuration already exists under '{1}' for the same company.", companyCode, LocationOfLocalCountryCustomsInterface);
		}

		public static bool IsRegistryItemConfiguredInMutuallyExclusiveItems(this IRegistryItemWithOtherChangedItems registryItem, Guid companyPK, bool isSavedRegistryItem, ZString[] keysOfMutualExclusiveRegistryItems)
		{
			return registryItem != null &&
				(registryItem.OtherChangedItems != null && registryItem.OtherChangedItems.Any(item => keysOfMutualExclusiveRegistryItems.Contains(item.Name) && item.IsRegistryItemOverriden(companyPK))) ||
				(registryItem.OtherChangedItems == null || !registryItem.OtherChangedItems.Any(item => keysOfMutualExclusiveRegistryItems.Contains(item.Name) && !item.IsRegistryItemOverriden(companyPK))) &&
				isSavedRegistryItem;
		}
	}
}

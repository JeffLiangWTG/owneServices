using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	sealed class RegistryItemExtensionsTest : TestCaseWithFactory
	{
		public void TestAddErrorIfCustomsInterfaceConfigured()
		{
			var registryItem = new LocalCountryCustomsInterfaceMutualExclusiveStringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System) as IRegistryItemInternals;
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "GC1";
			company.Branches.AddNew().GB_Code = "GB1";
			Factory.Save();
			var dataType = registryItem.DataType;
			registryItem.SetCurrentValueToUse(company.PK.ToGuid(), Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			registryItem.SetProposedValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "aaa");
			AssertNoExceptionThrown(delegate
			{
				dataType.Validate(registryItem, "def", company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			});

			var errorMessage = RegistryItemExtensions.CannotConfigureRegistryForCompany("GC1");
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			var registryItemCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface as IRegistryItemInternals;
			registryItemCustomsInterface.SetCurrentValueToUse(company.PK.ToGuid(), Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			registryItemCustomsInterface.SetProposedValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			((IRegistryItemWithOtherChangedItems)registryItem).OtherChangedItems = new[] { registryItemCustomsInterface };
			AssertExceptionThrown(typeof(RegistryValidationException), errorMessage, delegate
			{
				dataType.Validate(registryItem, "abc", company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			});

			((IRegistryItemWithOtherChangedItems)registryItem).OtherChangedItems = null;
			registryItemCustomsInterface.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			AssertExceptionThrown(typeof(RegistryValidationException), errorMessage, delegate
			{
				dataType.Validate(registryItem, "ghi", company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			});
		}
	}
}

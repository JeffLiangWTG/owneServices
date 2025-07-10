using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(LocalCountryCustomsInterfaceRegistryItemDataType))]
	sealed class LocalCountryCustomsInterfaceRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<LocalCountryCustomsInterfaceRegistryItemDataType>
	{
		public void TestValidateCore_ABM()
		{
			AssertEquals("PreCondition:IntegratedCountryHelper.CustomsWareInstallations(BE)", true, IntegratedCountryHelper.CustomsWareInstallations(Core.Constants.CountryCodes.Belgium));
			var factory = new BusinessObjectFactory();
			var abmCompany = factory.New<GlbCompany>();
			abmCompany.GC_Code = "BE1";
			abmCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			abmCompany.Branches.AddNew().GB_Code = "GB3";
			factory.Save();

			var registryItem = new LocalCountryCustomsInterfaceRegistryItem("", null, null, null, RegistryStorageFlags.Company) as IRegistryItemInternals;
			var dataType = registryItem.DataType;
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;

			var abmErrorMessage = LocalCountryCustomsInterfaceRegistryItemDataType.CannotConfigureInterfaceForCompany("BE1", "ABM");
			var abmRegistryItem = CustomsDataRegistry.Instance.CustomsWareCompany as IRegistryItemInternals;
			((IRegistryItemWithOtherChangedItems)registryItem).OtherChangedItems = new[] { abmRegistryItem };
			registryItem.SetCurrentValueToUse(abmCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			registryItem.SetProposedValue(abmCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("CustomsInterface activated; OtherChangedItem = ABMInterface but not activated and not overridden", () => dataType.Validate(registryItem, customsInterface, abmCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

				abmRegistryItem.SetCurrentValueToUse(abmCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
				abmRegistryItem.SetProposedValue(abmCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "aaa");
				AssertExceptionThrown<RegistryValidationException>("CustomsInterface activated; OtherChangedItem = ABMInterface which is overridden but activated(not saved)", abmErrorMessage, () => dataType.Validate(registryItem, customsInterface, abmCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

				((IRegistryItemWithOtherChangedItems)registryItem).OtherChangedItems = null;
				abmRegistryItem.SetValue(abmCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TEST");
				AssertExceptionThrown<RegistryValidationException>("CustomsInterface activated; otherchanged items NULL but ABMInterface activated and saved", abmErrorMessage, () => dataType.Validate(registryItem, customsInterface, abmCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

				abmRegistryItem.SetValue(abmCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
				AssertNoExceptionThrown("CustomsInterface activated; otherchanged items NULL and ABMInterface not activatedand not saved", () => dataType.Validate(registryItem, customsInterface, abmCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			});
		}

		protected override string ExpectedEditorName => "LocalCountryCustomsInterfaceRegistryItemEditor";

		protected override LocalCountryCustomsInterfaceRegistryItemDataType GetNewDataType() => new LocalCountryCustomsInterfaceRegistryItemDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Builtin;
			customsInterface.InterfaceType = LocalCountryCustomsInterfaceTypeCodeList.Codes.WiseTechCustoms;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(customsInterface, new LocalCountryCustomsInterfaceRegistryItemDataType().Serialise(customsInterface))
			};
		}
	}
}

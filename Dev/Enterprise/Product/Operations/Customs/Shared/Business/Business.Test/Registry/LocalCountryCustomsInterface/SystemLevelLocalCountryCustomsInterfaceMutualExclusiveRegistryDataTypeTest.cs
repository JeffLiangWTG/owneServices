using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestsSubclassesOf(typeof(SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryDataType))]
	public abstract class SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryDataTypeTest : RegistryDataTypeTestCase<SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryDataType>
	{
		public void TestValidate()
		{
			var registryItem = GetRegistryItem() as IRegistryItemInternals;
			var factory = new BusinessObjectFactory();
			var company = factory.New<GlbCompany>();
			company.GC_Code = "GC1";
			company.GC_RN_NKCountryCode = GetCountryCode();
			company.Branches.AddNew().GB_Code = "GB1";
			factory.Save();
			var dataType = registryItem.DataType;
			registryItem.SetCurrentValueToUse(Guid.Empty, Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			registryItem.SetProposedValue(Guid.Empty, Guid.Empty, Guid.Empty, "aaa");
			AssertNoExceptionThrown(delegate
			{
				dataType.Validate(registryItem, "def", company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			});

			var errorMessage = GetRegistryDataType().CannotConfigureRegistryForCountry("GC1");
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "123";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			var registryItemCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface as IRegistryItemInternals;
			registryItemCustomsInterface.SetCurrentValueToUse(company.PK.ToGuid(), Guid.Empty, Guid.Empty, ValueToUse.ProposedValue);
			registryItemCustomsInterface.SetProposedValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			((IRegistryItemWithOtherChangedItems)registryItem).OtherChangedItems = new[] { registryItemCustomsInterface };
			AssertExceptionThrown(typeof(RegistryValidationException), errorMessage, delegate
			{
				dataType.Validate(registryItem, "aaa", company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			});

			((IRegistryItemWithOtherChangedItems)registryItem).OtherChangedItems = null;
			registryItemCustomsInterface.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			AssertExceptionThrown(typeof(RegistryValidationException), errorMessage, delegate
			{
				dataType.Validate(registryItem, "abc", company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			});
		}

		protected override SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryDataType GetNewDataType()
		{
			return GetRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB("abc", Encoding.Unicode.GetBytes("abc")),
				new ValidSampleAndBinaryValueInDB("def", Encoding.Unicode.GetBytes("def")),
			};
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected abstract SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryDataType GetRegistryDataType();
		protected abstract SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryItem GetRegistryItem();
		protected abstract ZString GetCountryCode();
	}
}

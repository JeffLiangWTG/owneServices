using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EPaymentConfigurationRegistryDataType))]
	sealed class EPaymentConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EPaymentConfigurationRegistryDataType>
	{
		#region Implementation

		public void TestValidationWithEmptyData()
		{
			var dataType = GetNewDataType();
			var registryItem = new EPaymentConfigurationRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);

			var collection1 = new EPaymentConfigurationCollection();

			void action(EPaymentConfigurationCollection value)
			{
				registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection1);
				dataType.Validate(registryItem, collection1, GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			}

			AssertExceptionThrown<RegistryValidationException>(() => action(collection1));
		}

		protected override string ExpectedEditorName => "EPaymentConfigurationRegistryItemEditor";

		protected override EPaymentConfigurationRegistryDataType GetNewDataType() => new EPaymentConfigurationRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collectionForAUCompany = new EPaymentConfigurationCollection();
			var configForAUCompany = collectionForAUCompany.AddNew();
			configForAUCompany.CountryCode = Core.Constants.CountryCodes.Australia;
			configForAUCompany.CountryDescription = "Australia";
			configForAUCompany.OFXEPaymentEnabled = true;

			var byteArray1 = DataType.Serialise(collectionForAUCompany);

			var collectionForLKCompany = new EPaymentConfigurationCollection();
			var configForLKCompany = collectionForLKCompany.AddNew();
			configForLKCompany.CountryCode = Core.Constants.CountryCodes.SriLanka;
			configForLKCompany.CountryDescription = "Sri Lanka";
			configForLKCompany.OFXEPaymentEnabled = false;

			var byteArray2 = DataType.Serialise(collectionForLKCompany);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collectionForAUCompany, byteArray1)
				, new ValidSampleAndBinaryValueInDB(collectionForLKCompany, byteArray2)
			};
		}

		#endregion
	}
}

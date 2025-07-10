using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(ASNRefreshOptionsConfig))]
	sealed class ASNRefreshOptionsConfigTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateASNRefreshDefaults()
		{
			var collection = new ASNRefreshOptionsConfigCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var config1 = collection.AddNew();
			config1.FieldType = "XXX";
			AssertHasErrorContaining(config1.FieldTypeInfo, ListValidation.InvalidCodeError);

			config1.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification;
			AssertNoErrorContaining(config1.FieldTypeInfo, ListValidation.InvalidCodeError);

			var config2 = collection.AddNew();
			config2.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification;
			AssertHasError(config2.FieldTypeInfo, ASNRefreshOptionsConfig.DuplicatedCodesError);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			var config = new ASNRefreshOptionsConfig(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			config.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification;
			return config;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ASNRefreshOptionsConfig();
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		#endregion
	}
}

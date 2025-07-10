using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using DefaultsOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(IMProductValueDefaultOption))]
	sealed class IMProductValueDefaultOptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFieldTypes()
		{
			var defaultOption = new IMProductValueDefaultOption(Factory);
			var list = defaultOption.FieldTypes;

			AssertEquals(4, list.Count);
			Assert(list.ContainsCode(DefaultsOptions.Codes.Classification));
			Assert(list.ContainsCode(DefaultsOptions.Codes.CountryOfOrigin));
			Assert(list.ContainsCode(DefaultsOptions.Codes.Preference));
			Assert(list.ContainsCode(DefaultsOptions.Codes.Tariff));
		}
	}
}

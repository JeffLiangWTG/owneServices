using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	public class ApplicationBusinessProviderTest : TestCaseWithFactory
	{
		public void TestGetApplicationBusinessProvider()
		{
			var provider = UniversalReferenceBusinessProvider.GetProvider(Factory, "");
			AssertType<UniversalReferenceBusinessProvider>(provider);
			AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short, provider.TariffDateTimeFormat);
			CombineAssertions(() =>
			{
				var mappings = ObjectFactory.Get<Hashtable>("UniversalReferenceBusinessProvider");
				foreach (DictionaryEntry mapping in mappings)
				{
					var country = mapping.Key.ToString();
					var providerFromMapping = UniversalReferenceBusinessProvider.GetProvider(Factory, country);
					AssertNotNull("Mapping: " + country, providerFromMapping);
					if (country != Constants.UniversalReferenceBusinessProvider.Default)
					{
						AssertEquals("Expecting a derived type for Mapping: " + country + ", " + ((ObjectHandle)mapping.Value).GetObjectType(), "UniversalReferenceBusinessProvider", providerFromMapping.GetType().BaseType.Name);
					}
				}
			}

			);
		}
	}
}

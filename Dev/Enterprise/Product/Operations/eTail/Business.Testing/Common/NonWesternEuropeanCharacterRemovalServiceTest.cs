using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.eTail.Business.Testing
{
	public class NonWesternEuropeanCharacterRemovalServiceTest : TestCaseWithFactory
	{
		public void TestRemoveNonWesternEuropeanCharactersWhenFactoryHasNonWesternEuropeanCharacterRemovalService_HVLVConsignment()
		{
			var consignment = SetupBizoWithNonWesternEuropeanCharacters<HVLVConsignment>();
			AssertNonWesternEuropeanCharactersRemoved(consignment);
		}

		public void TestRemoveNonWesternEuropeanCharactersWhenFactoryHasNonWesternEuropeanCharacterRemovalService_HVLVItem()
		{
			var item = SetupBizoWithNonWesternEuropeanCharacters<HVLVItem>();
			AssertNonWesternEuropeanCharactersRemoved(item);
		}

		public void TestRemoveNonWesternEuropeanCharactersWhenFactoryHasNonWesternEuropeanCharacterRemovalService_HVLVItemLine()
		{
			var itemLine = SetupBizoWithNonWesternEuropeanCharacters<HVLVItemLine>();
			AssertNonWesternEuropeanCharactersRemoved(itemLine);
		}

		void AssertNonWesternEuropeanCharactersRemoved<T>(T bizo) where T : BusinessObject
		{
			var schema = BusinessObjectFactory.GetTableSchemaFromType(typeof(T));
			CombineAssertions(() =>
			{
				foreach (var column in schema.All)
				{
					if (column.SqlDbType == System.Data.SqlDbType.NVarChar)
					{
						var stringValue = bizo[column] as ZString?;
						Assert(stringValue.Value.IsLettersAndNumbersOnlyOrEmpty);
					}
				}
			});
		}

		T SetupBizoWithNonWesternEuropeanCharacters<T>() where T : BusinessObject
		{
			var result = Factory.NewWithValidTestData<T>();

			var schema = BusinessObjectFactory.GetTableSchemaFromType(typeof(T));
			foreach (var column in schema.All)
			{
				if (column.SqlDbType == System.Data.SqlDbType.NVarChar)
				{
					result[column] = "吃面";
				}
			}

			return result;
		}

		protected override void SetUp()
		{
			Factory.ServiceContainer.AddService(new NonWesternEuropeanCharactersRemovalService());
		}

		protected override void TearDown()
		{
			Factory.ServiceContainer.RemoveService<NonWesternEuropeanCharactersRemovalService>();
		}
	}
}

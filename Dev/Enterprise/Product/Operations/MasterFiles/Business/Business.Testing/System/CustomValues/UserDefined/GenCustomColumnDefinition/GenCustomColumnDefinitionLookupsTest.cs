using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class GenCustomColumnDefinitionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypes()
		{
			var types = Factory.New<GenCustomColumnDefinition>().Lookups.Types;
			Assert(!types.ContainsCode(AddOnColumnDataType.Codes.Guid));
			Assert(!types.ContainsCode(AddOnColumnDataType.Codes.Byte));
			Assert(!types.ContainsCode(AddOnColumnDataType.Codes.Short));
			Assert(!types.ContainsCode(AddOnColumnDataType.Codes.Date));
		}
	}
}

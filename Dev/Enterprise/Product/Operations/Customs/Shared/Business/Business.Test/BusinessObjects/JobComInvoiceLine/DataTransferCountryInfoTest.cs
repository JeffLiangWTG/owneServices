using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(DataTransferCountryInfo))]
	sealed class DataTransferCountryInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new DataTransferCountryInfo("EU", "European Union");

		public void TestConstructor()
		{
			var dataTransferCountryInfo = (DataTransferCountryInfo)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("Code of DataTransferCountryInfo should refer to EU", "EU", dataTransferCountryInfo.RN_Code);
				AssertEquals("Description of DataTransferCountryInfo should refer to European Union", "European Union", dataTransferCountryInfo.RN_Desc);
			});
		}
	}
}

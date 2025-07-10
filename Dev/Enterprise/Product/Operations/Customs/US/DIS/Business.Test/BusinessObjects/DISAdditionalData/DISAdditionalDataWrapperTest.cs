using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISAdditionalDataWrapperTest : TestCaseWithFactory
	{
		public void TestIDISAdditionalData()
		{
			var additionalData = new DISAdditionalData(Factory);
			additionalData.Name = "Invoice Amount";
			additionalData.Data = "2500.50 USD";
			var iAdditionalData = (IDISAdditionalData)new DISAdditionalDataWrapper(additionalData);
			AssertEquals("Invoice Amount", iAdditionalData.FieldName);
			AssertEquals("2500.50 USD", iAdditionalData.Value);
		}
	}
}

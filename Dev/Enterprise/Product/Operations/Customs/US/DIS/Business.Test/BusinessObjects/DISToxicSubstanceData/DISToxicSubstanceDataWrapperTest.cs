using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISToxicSubstanceDataWrapperTest : TestCaseWithFactory
	{
		public void TestIDISToxicSubstanceData()
		{
			var data = new DISToxicSubstanceData(Factory);
			data.CASNumbers.AddNew().Number = "2";
			data.CASNumbers.AddNew().Number = "1";
			data.EPARegistrationNumber = "234890342";
			data.EPAProducerEstNumber = "324789";
			var iData = (IDISToxicSubstanceData)new DISToxicSubstanceDataWrapper(data);
			AssertEquals("1, 2", iData.CASNumber);
			AssertEquals("234890342", iData.EPARegoNumber);
			AssertEquals("324789", iData.EPAProducerEstNumber);
		}
	}
}

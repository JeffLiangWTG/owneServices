using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class IncotermCodeListTest : TestCaseWithFactory
	{
		public void TestCodeDescriptionsPairs()
		{
			var incotermList = new NOIncotermCodeList();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 11, incotermList.Count);
				AssertEquals("Code 'CFR'", "Cost and Freight", incotermList.GetDescriptionFromCode("CFR"));
				AssertEquals("Code 'CIF'", "Cost, Insurance and Freight", incotermList.GetDescriptionFromCode("CIF"));
				AssertEquals("Code 'CIP'", "Carriage and Insurance Paid To", incotermList.GetDescriptionFromCode("CIP"));
				AssertEquals("Code 'CPT'", "Carriage Paid To", incotermList.GetDescriptionFromCode("CPT"));
				AssertEquals("Code 'DAP'", "Delivered At Place", incotermList.GetDescriptionFromCode("DAP"));
				AssertEquals("Code 'DDP'", "Delivered Duty Paid", incotermList.GetDescriptionFromCode("DDP"));
				AssertEquals("Code 'DPU'", "Delivered At Place Unloaded", incotermList.GetDescriptionFromCode("DPU"));
				AssertEquals("Code 'EXW'", "Ex Works", incotermList.GetDescriptionFromCode("EXW"));
				AssertEquals("Code 'FAS'", "Free Alongside Ship", incotermList.GetDescriptionFromCode("FAS"));
				AssertEquals("Code 'FCA'", "Free Carrier (seller is responsible for origin, buyer for loading)", incotermList.GetDescriptionFromCode("FCA"));
				AssertEquals("Code 'FOB'", "Free On Bord", incotermList.GetDescriptionFromCode("FOB"));
			});
		}
	}
}

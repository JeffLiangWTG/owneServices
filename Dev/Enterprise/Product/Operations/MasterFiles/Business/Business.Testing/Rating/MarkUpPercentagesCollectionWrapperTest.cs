using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Testing
{
	[TestedType(typeof(MarkUpPercentagesCollectionWrapper))]
	sealed class MarkUpPercentagesCollectionWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			MarkUpPercentagesCollectionWrapper bizO = new MarkUpPercentagesCollectionWrapper("");
			return bizO;
		}

		public void TestMarkUpPercentagesCollectionWrapper()
		{
			string value1 = "AUSYD|1.1;SEA|USLAX|1.2;AUMEL|1.3";
			string value2 = "ALL|AUSYD|1.1|0|0;ALL|AUMEL|1.3|0|0;FCL|USSFO|2.4|0|0";
			string value3 = "AUSYD|1.1;11.11";
			string value4 = "ALL|AUSYD|1.1|0|0;ALL||11.11|0|0";
			string value5 = "AIR|AUSYD|5|100|1";
			string value6 = "AIR|AUSYD|5|100|1;FCL|AUSYD|0|0|500";

			MarkUpPercentagesCollectionWrapper bizO = new MarkUpPercentagesCollectionWrapper(value1);
			AssertEquals("Mode[0]", "ALL", bizO.MarkUpPercentages[0].Mode);
			AssertEquals("Location[0]", "AUSYD", bizO.MarkUpPercentages[0].Location);
			AssertEquals("Percentage[0]", 1.1m, bizO.MarkUpPercentages[0].Percentage);
			AssertEquals("Mode[1]", "SEA", bizO.MarkUpPercentages[1].Mode);
			AssertEquals("Location[1]", "USLAX", bizO.MarkUpPercentages[1].Location);
			AssertEquals("Percentage[1]", 1.2m, bizO.MarkUpPercentages[1].Percentage);
			AssertEquals("Mode[2]", "ALL", bizO.MarkUpPercentages[2].Mode);
			AssertEquals("Location[2]", "AUMEL", bizO.MarkUpPercentages[2].Location);
			AssertEquals("Percentage[2]", 1.3m, bizO.MarkUpPercentages[2].Percentage);

			bizO.MarkUpPercentages.Remove(bizO.MarkUpPercentages[1]);
			MarkUpPercentage elem = bizO.MarkUpPercentages.AddNew();
			elem.Mode = "FCL";
			elem.Location = "USSFO";
			elem.Percentage = 2.4m;
			AssertEquals("Value", value2, bizO.MarkUpPercentages.ToString());

			bizO = new MarkUpPercentagesCollectionWrapper(value3);
			AssertEquals("Mode[0]", "ALL", bizO.MarkUpPercentages[0].Mode);
			AssertEquals("Location[0]", "AUSYD", bizO.MarkUpPercentages[0].Location);
			AssertEquals("Percentage[0]", 1.1m, bizO.MarkUpPercentages[0].Percentage);
			AssertEquals("Mode[1]", "ALL", bizO.MarkUpPercentages[1].Mode);
			AssertEquals("Location[1]", "", bizO.MarkUpPercentages[1].Location);
			AssertEquals("Percentage[1]", 11.11m, bizO.MarkUpPercentages[1].Percentage);

			AssertEquals("Value", value4, bizO.MarkUpPercentages.ToString());

			bizO = new MarkUpPercentagesCollectionWrapper(value5);
			AssertEquals("Mode[0]", "AIR", bizO.MarkUpPercentages[0].Mode);
			AssertEquals("Location[0]", "AUSYD", bizO.MarkUpPercentages[0].Location);
			AssertEquals("Percentage[0]", 5m, bizO.MarkUpPercentages[0].Percentage);
			AssertEquals("Minimum[0]", 100m, bizO.MarkUpPercentages[0].Minimum);
			AssertEquals("PerUnit[0]", 1m, bizO.MarkUpPercentages[0].PerUnit);

			elem = bizO.MarkUpPercentages.AddNew();
			elem.Mode = "FCL";
			elem.Location = "AUSYD";
			elem.PerUnit = 500m;
			AssertEquals("Value", value6, bizO.MarkUpPercentages.ToString());
		}
	}
}

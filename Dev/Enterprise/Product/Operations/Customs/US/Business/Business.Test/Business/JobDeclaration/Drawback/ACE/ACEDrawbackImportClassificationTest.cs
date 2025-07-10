using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEDrawbackImportClassificationTest : TestCaseWithFactory
	{
		public void TestImportClassification()
		{
			var importClassification = new ACEDrawbackImportClassification("920100101234", "TEST IMPORT CLASSIFICATION", 100m, "KG", 101m, 10m, 15m);
			var classification = (IACEDrawbackImportClassification)importClassification;
			AssertEquals("920100101234", classification.HTSNumber);
			AssertEquals("TEST IMPORT CLASSIFICATION", classification.DescriptionText);
			var exportQuantityAndUnit = classification.ExportQuantityAndUnit;
			AssertEquals(100m, exportQuantityAndUnit.Quantity);
			AssertEquals("KG", exportQuantityAndUnit.UnitOfMeasure);
			AssertEquals(101m, exportQuantityAndUnit.AllowableQuantity);
			AssertEquals(10m, exportQuantityAndUnit.GoodsValuePerUnit);
			AssertEquals(15m, exportQuantityAndUnit.SubstitutedValuePerUnit);

			importClassification = new ACEDrawbackImportClassification("920100101234", "1234567890ABCDEFGHIJ1234567890ABCDEFGHIJ1234567890ABCFEFGHIJ", 100m, "KG", 101m, 10m, 15m);
			classification = importClassification;
			AssertEquals("1234567890ABCDEFGHIJ1234567890ABCDEFGHIJ1234567890", classification.DescriptionText);
		}
	}
}

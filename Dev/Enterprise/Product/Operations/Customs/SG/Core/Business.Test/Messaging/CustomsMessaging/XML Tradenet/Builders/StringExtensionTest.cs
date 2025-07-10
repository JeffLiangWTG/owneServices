using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	class StringExtensionTest : TestCaseWithFactory
	{
		public void TestSplitIntoArray()
		{
			var text = new ZString("Anglo-EASTERN INTERNATIONAL ENGINEERING MANUFACTURING and PROJECT MANAGEMENT SERVICES PTE. Limited");
			var array = text.SplitIntoArray(35, 2);
			AssertEquals("Should only get the array in max elements count.", 2, array.Length);
			AssertEquals("ANGLO-EASTERN INTERNATIONAL ENGINEE", array[0]);
			AssertEquals("RING MANUFACTURING AND PROJECT MANA", array[1]);
		}

		public void TestGetReferenceNumber()
		{
			AssertEquals("Should be empty with a null UniqueReferenceNumber.", string.Empty, StringExtension.GetReferenceNumber(null));
			var uniqueReferenceNumber = new UniqueReferenceNumber()
			{ Date = "20200701", ID = "WTGB0000200701", SequenceNumeric = "105" };
			AssertEquals("WTGB0000200701202007010105", uniqueReferenceNumber.GetReferenceNumber());
		}
	}
}

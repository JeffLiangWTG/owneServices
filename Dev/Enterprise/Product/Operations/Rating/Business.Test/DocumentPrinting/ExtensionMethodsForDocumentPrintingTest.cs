using System.Linq;
using Enterprise.Rating.Business.RatingEnums;

namespace Enterprise.Rating.Business.Testing
{
	class ExtensionMethodsForDocumentPrintingTest : RatingTestCase
	{
		public void TestGetContainersInSameClassForEntryType()
		{
			Helper.Containers["40GP"].RC_FreightRateClass = "40";
			Helper.Containers["40HC"].RC_FreightRateClass = "40";

			Helper.Containers["20GP"].RC_HandlingRateClass = "GP";
			Helper.Containers["40GP"].RC_HandlingRateClass = "GP";

			var freightContainersInSameClass = Helper.Containers["40GP"].GetContainersInSameClassForEntryType(EntryTypes.Freight);
			var destinationContainersInSameClass = Helper.Containers["40GP"].GetContainersInSameClassForEntryType(EntryTypes.Destination);
			var originCainersInSameClass = Helper.Containers["40GP"].GetContainersInSameClassForEntryType(EntryTypes.Origin);

			AssertEquals(1, freightContainersInSameClass.Count);
			AssertEquals(Helper.Containers["40HC"].PK, freightContainersInSameClass.First().PK);

			AssertEquals(1, destinationContainersInSameClass.Count);
			AssertEquals(Helper.Containers["20GP"].PK, destinationContainersInSameClass.First().PK);

			AssertEquals(1, originCainersInSameClass.Count);
			AssertEquals(Helper.Containers["20GP"].PK, originCainersInSameClass.First().PK);
		}
	}
}

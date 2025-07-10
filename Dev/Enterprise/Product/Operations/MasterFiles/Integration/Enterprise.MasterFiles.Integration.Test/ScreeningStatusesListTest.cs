using System.Reflection;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Integration.Testing
{
	public class ScreeningStatusesListTest : TestCase
	{
		readonly int numberOfCodesInShared = typeof(DeniedPartyConstants.ScreeningStatus.Codes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Length;

		public void TestNumberOfStaticItemsInScreeningStatusesList()
		{
			var numberOfCodes = typeof(ScreeningStatusesList.Codes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Length;
			var numberOfDescriptions = typeof(ScreeningStatusesList.Descriptions).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Length;

			AssertEquals(numberOfCodesInShared, numberOfCodes);
			AssertEquals(numberOfCodesInShared, numberOfDescriptions);
		}

		public void TestNumberOfInstanceItemsInScreeningStatusesList()
		{
			var instance = new ScreeningStatusesList();

			AssertEquals(numberOfCodesInShared, instance.Count);
		}
	}
}

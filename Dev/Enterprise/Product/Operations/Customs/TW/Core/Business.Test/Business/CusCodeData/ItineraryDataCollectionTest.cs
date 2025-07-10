using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ItineraryDataCollection))]
	sealed class ItineraryDataCollectionTest : CusCodeDataCollectionTest<ItineraryData>
	{
		[ExpectNoExceptions]
		public void TestMaxAllowed()
		{
			var testCollection = GetCusCodeDataCollection();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testCollection.AllowNew, NUnit.Framework.Is.True, "Collection is empty");

				for (var i = 0; i < 98; i++)
				{
					testCollection.AddNew();
					NUnit.Framework.Assert.That(testCollection.AllowNew, NUnit.Framework.Is.True, $"Currently, collection has {testCollection.Count} elements");
				}

				testCollection.AddNew();
				NUnit.Framework.Assert.That(!testCollection.AllowNew, NUnit.Framework.Is.True, $"Currently, collection has {testCollection.Count} elements");
			});
		}

		protected override CusCodeDataCollection<ItineraryData> GetCusCodeDataCollection()
		{
			return new ItineraryDataCollection(Declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<ItineraryData>();
			result.CY_ParentID = Declaration.PK;
			result.CY_ParentTableCode = Declaration.TablePrefix;
			return result;
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;
	}
}

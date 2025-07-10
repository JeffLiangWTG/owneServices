using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(ClientRateProcessTaskCollection))]
	public class ClientRatesProcessTaskCollectionTest : RatingHeaderProcessTaskCollectionTest<ClientRate, ClientRateProcessTaskCollection>
	{
		public void TestAdditionalFilter()
		{
			var processTaskCollection = new ClientRateProcessTaskCollectionForTest(RatingHeader);
			var expectedFilter = new ZQuery(ProcessTasksSchema.P9_ParentTableCode, RatingHeaderSchema.Constants.Prefix);
			AssertCollectionContains(expectedFilter, processTaskCollection.AdditionalFilter.GetCompositeParts());
		}

		#region Implementation

		protected override ClientRateProcessTaskCollection GetCollectionToTestCore()
		{
			return new ClientRateProcessTaskCollection(RatingHeader);
		}

		class ClientRateProcessTaskCollectionForTest : ClientRateProcessTaskCollection
		{
			public ClientRateProcessTaskCollectionForTest(ClientRate clientRate)
				: base(clientRate)
			{
			}

			public new ZQuery AdditionalFilter
			{
				get { return base.AdditionalFilter; }
			}
		}

		#endregion
	}
}

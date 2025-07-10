using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Test
{
	[TestedType(typeof(DuplicationModelDetailCollection))]
	public class DuplicationModelDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DuplicationModelDetailCollection>
	{
		public void TestAllowNew()
		{
			var result = new DuplicationModelDetailCollection();
			AssertEquals(false, result.AllowNew);
		}

		protected override DuplicationModelDetailCollection GetCollectionToTest()
		{
			return new DuplicationModelDetailCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var models = new[]
			{
				new DeduplicationPresenterModel
				{
					ChildGroupNameForType = string.Empty,
					ChildDisplayNameForMasterColumns = "Name",
					ChildDisplayNameForTargetColumns = "Name"
				}
			};

			return DuplicationModelDetail.GetDuplicationModelDetails(models).Single().MasterModels.Single();
		}
	}
}

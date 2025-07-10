using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RelatedChildActivityPivotCollectionForTest : RelatedChildActivityPivotCollection
	{
		public RelatedChildActivityPivotCollectionForTest(IRelatableActivity master)
			: base(master)
		{
		}

		protected override IEnumerable<ZString> GetSuperAndSubActivityTablePrefixes()
		{
			yield return DummyBizoSchema.Constants.Prefix;
		}

		protected override void RelinkRelatedSuperAndSubActivitiesCore(IEnumerable<IRelatableActivity> activities)
		{
			base.RelinkRelatedSuperAndSubActivitiesCore(activities);
			TotalRelinkRelatedSuperAndSubActivitiesCalls++;
		}

		public int TotalRelinkRelatedSuperAndSubActivitiesCalls;
	}
}

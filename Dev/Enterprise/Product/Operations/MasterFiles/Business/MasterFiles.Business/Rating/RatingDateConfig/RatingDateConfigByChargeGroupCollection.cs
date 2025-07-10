using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Rating;

namespace Enterprise.MasterFiles.Business
{
	public class RatingDateConfigByChargeGroupCollection : NonPersistentBusinessObjectCollection<RatingDateConfigByChargeGroup>
	{
		public RatingDateConfigByChargeGroupCollection(RatingDateConfigCollection ratingDateConfigCollection)
			: base(ratingDateConfigCollection.Factory)
		{
			this.ratingDateConfigCollection = ratingDateConfigCollection;
		}

		readonly RatingDateConfigCollection ratingDateConfigCollection;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new RatingDateConfigByChargeGroup(ratingDateConfigCollection);
	}
}

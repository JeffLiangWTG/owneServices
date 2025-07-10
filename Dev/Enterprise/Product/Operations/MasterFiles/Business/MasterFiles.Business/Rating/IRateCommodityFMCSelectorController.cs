using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IRateCommodityFMCSelectorController
	{
		/// <summary>
		/// The Factory should be a new factory. Created just for this selection popup.
		/// The selection and updating will work on the first adapter that the rating
		/// supporter provides that is "Revenue"
		/// </summary>
		void SelectAndUpdate(
			BusinessObjectFactory factory,
			IRatingSupporter ratingSupporter,
			DetailedGoodsDescriptionProxy detailedGoodsInfo);
	}

	public class DetailedGoodsDescriptionProxy
	{
		public bool IsEmpty { get; set; }
		public bool IsAvailable { get; set; }
	}
}

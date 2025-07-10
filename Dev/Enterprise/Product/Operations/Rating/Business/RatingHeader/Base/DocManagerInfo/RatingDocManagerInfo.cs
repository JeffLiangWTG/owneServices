using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Provides information about Related Business Objects related to a RatingHeader.
	/// Used for eDocs.
	/// </summary>
	public class RatingDocManagerInfo : DocManagerInfo
	{
		public RatingDocManagerInfo(RatingHeader bizO, string docManagerCode)
			: base(bizO, docManagerCode)
		{
		}

		protected RatingHeader RatingHeader
		{
			get { return (RatingHeader)BusinessEntity; }
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			return RatingHeader.Header != null ? new BusinessObject[] { RatingHeader.Header } : System.Array.Empty<BusinessObject>();
		}
	}
}


using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RatingDateConfigCollectionViewModel : NonPersistentBusinessObjectCollection<RatingDateConfigViewModel>
	{
		public RatingDateConfigCollectionViewModel(RatingDateConfigCollection ratingDateConfigCollection, string chargeGroup)
			: base(ratingDateConfigCollection.Factory)
		{
			this.ratingDateConfigCollection = ratingDateConfigCollection;
			this.chargeGroup = chargeGroup;
		}

		readonly RatingDateConfigCollection ratingDateConfigCollection;
		readonly string chargeGroup;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var ratingDateConfig = ratingDateConfigCollection.AddNew();
			ratingDateConfig.RDT_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			ratingDateConfig.RDT_ParentID = ratingDateConfigCollection.Master.PK;
			ratingDateConfig.RDT_GC_Company = GlbCompany.CurrentCompany.PK;
			ratingDateConfig.RDT_ChargeGroup = chargeGroup;
			return new RatingDateConfigViewModel(ratingDateConfig);
		}

		#endregion
	}
}

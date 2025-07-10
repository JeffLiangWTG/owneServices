using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class FDARelatedBillsGenPivotValidation : GenPivotValidation
	{
		public FDARelatedBillsGenPivotValidation(FDARelatedBillsGenPivot parent)
			: base(parent)
		{
		}

		protected override void CheckXX_Relation2ID()
		{
			base.CheckXX_Relation2ID();
			ValidateDuplicatePivot();
		}

		void ValidateDuplicatePivot()
		{
			var bill = Parent.Relation2Object;
			if (bill != null && !bill.IsDeleted)
			{
				var fda = Parent.Relation1Object;
				if (fda != null && !fda.IsDeleted)
				{
					var query = new ZQuery(GenPivotSchema.XX_Relation1ID, fda.PK);
					query.AddToFilter(GenPivotSchema.XX_RelationType, Parent.XX_RelationType);
					query.FetchOnlyFromLocalCache = !fda.IsInDatabase || !bill.IsInDatabase;
					if (Parent.Factory.Load<FDARelatedBillsGenPivot>(query).Any(x => x.XX_Relation2ID == bill.PK && x.PK != Parent.PK)) // need to load all matching to force fetch hint and utilise factory cached
					{
						Parent.XX_Relation2IDInfo.AddError(ResString.GetMultilingualString("USFDARelatedBill|IsForFDALineError", "The following unexpected errors were encountered:\r\nDuplicate pivots for the same related bill ({0}) exists.\r\nA possible fix for this is to untick and re-tick the related bill.", bill.CU_BillUniqueCode));
					}

					var declaration = fda.Declaration;
					if (declaration != null && declaration.US_EnableSPN && fda.BillsForFDALine.Count > 1)
					{
						Parent.XX_Relation2IDInfo.AddMessageError(OnlyOneBillCanBeSelectedForPriorNotice);
					}
				}
			}
		}

		protected new FDARelatedBillsGenPivot Parent
		{
			get { return (FDARelatedBillsGenPivot)base.Parent; }
		}

		internal const string OnlyOneBillCanBeSelectedForPriorNotice = "Customs only allow the selection of one bill per FDA Line for Stand Alone Prior Notice.";
	}
}

// Testing is done in FDARelatedBill

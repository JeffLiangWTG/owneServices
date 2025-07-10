using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class FDARelatedContainersGenPivotValidation : GenPivotValidation
	{
		public FDARelatedContainersGenPivotValidation(FDARelatedContainersGenPivot parent)
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
			var containerPivot = Parent.Relation2Object;
			if (containerPivot != null && !containerPivot.IsDeleted)
			{
				var fda = Parent.Relation1Object as IFDARelatedContainer;
				if (fda != null && !fda.IsDeleted)
				{
					var query = new ZQuery(GenPivotSchema.XX_Relation1ID, fda.PK);
					query.AddToFilter(GenPivotSchema.XX_RelationType, Parent.XX_RelationType);
					query.FetchOnlyFromLocalCache = !fda.IsInDatabase || !containerPivot.IsInDatabase;
					if (Parent.Factory.Load<FDARelatedContainersGenPivot>(query).Any(x => x.XX_Relation2ID == containerPivot.PK && x.PK != Parent.PK)) // need to load all matching to force fetch hint and utilise factory cached
					{
						Parent.XX_Relation2IDInfo.AddError(ResString.GetMultilingualString("USFDARelatedContainer|IsForFDALineError", "The following unexpected errors were encountered:\r\nDuplicate pivots for the same related container ({0}) exists.\r\nA possible fix for this is to untick and re-tick the related container.", ((IContainerNumber)Parent).ContainerEquipmentID));
					}
				}
			}
		}

		protected new FDARelatedContainersGenPivot Parent
		{
			get { return (FDARelatedContainersGenPivot)base.Parent; }
		}
	}
}

// Testing is done in FDARelatedContainer

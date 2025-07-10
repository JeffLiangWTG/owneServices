using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class PGARelatedContainersGenPivotValidation : GenPivotValidation
	{
		public PGARelatedContainersGenPivotValidation(PGARelatedContainersGenPivot parent)
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
			var pivot = Parent.Relation2Object;
			if (pivot != null && !pivot.IsDeleted)
			{
				var pga = Parent.Relation1Object;
				if (pga != null && !pga.IsDeleted)
				{
					var query = new ZQuery(GenPivotSchema.XX_Relation1ID, pga.PK);
					query.AddToFilter(GenPivotSchema.XX_RelationType, Parent.XX_RelationType);
					query.FetchOnlyFromLocalCache = !pga.IsInDatabase || !pivot.IsInDatabase;
					if (Parent.Factory.Load<PGARelatedContainersGenPivot>(query).Any(x => x.XX_Relation2ID == pivot.PK && x.PK != Parent.PK)) // need to load all matching to force fetch hint and utilise factory cached
					{
						Parent.XX_Relation2IDInfo.AddError(ResString.GetMultilingualString("USPGARelatedContainer|IsForPGALineError", "The following unexpected errors were encountered:\r\nDuplicate pivots for the same related container ({0}) exists.\r\nA possible fix for this is to untick and re-tick the related container.", ((IContainerNumber)Parent).ContainerEquipmentID));
					}
				}
			}
		}

		protected new PGARelatedContainersGenPivot Parent
		{
			get { return (PGARelatedContainersGenPivot)base.Parent; }
		}
	}
}

// Testing is done in RelatedContainer

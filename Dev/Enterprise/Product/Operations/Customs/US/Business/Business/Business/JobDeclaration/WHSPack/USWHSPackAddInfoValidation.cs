//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSWHSPackAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSWHSPackAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using System;
	using System.Linq;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business.MultiLineAddInfos;
	using Enterprise.ZArchitecture.Schema;

	public class USWHSPackAddInfoValidation : AutoUSWHSPackAddInfoValidation
	{
		public USWHSPackAddInfoValidation(AutoUSWHSPackAddInfo parent)
			: base(parent)
		{
			if (!(parent is USWHSPackAddInfo))
			{
				throw new ArgumentException("Parent should be USWHSPackAddInfo");
			}
		}

		protected new USWHSPackAddInfo Parent
		{
			get { return (USWHSPackAddInfo)base.Parent; }
		}

		protected override void CheckUS_PackageReference()
		{
			base.CheckUS_PackageReference();
			if (IsInwardBondedWarehousingEnabled)
			{
				var whsPack = Parent.Parent;
				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, whsPack.B7_ParentID);
				query.AddToFilter(CusAddInfoSchema.PK, SQLComparisonOperator.NotEqual, whsPack.PK);
				query.AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USWHSPack);
				var otherWHSPacks = whsPack.Factory.Load<WHSPack>(query);
				var packageReference = Parent.US_PackageReference;
				if (otherWHSPacks.Any(x => x.US_PackageReference == packageReference))
				{
					Parent.US_PackageReferenceInfo.AddError(ValidationConstants.WHSPack.DuplicatePackageReference);
				}
			}
		}

		protected override void CheckUS_PackageQty()
		{
			base.CheckUS_PackageQty();
			if (Parent.US_PackageQty <= 0 && IsInwardBondedWarehousingEnabled)
			{
				Parent.US_PackageQtyInfo.AddMessageError(ValidationConstants.WHSPack.PackageQtyIsRequired);
			}
		}

		bool IsInwardBondedWarehousingEnabled
		{
			get
			{
				var declaration = Parent.Declaration;
				return declaration != null && declaration.IsInwardBondedWarehousingEnabled;
			}
		}
	}
}

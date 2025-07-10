//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsProductStyleSizeValidation
//
//    This class should be used for overriding validation in AutoWhsProductStyleSizeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsProductStyleSizeValidation : AutoWhsProductStyleSizeValidation
	{
		public WhsProductStyleSizeValidation(AutoWhsProductStyleSize parent) : base(parent)
		{
		}

		protected override void CheckWSZ_Size()
		{
			base.CheckWSZ_Size();
			MandatoryValidation.CheckEntered(Parent.WSZ_SizeInfo);

			if (!Parent.WSZ_SizeInfo.HasErrors()
				&& Parent.ProductStyle != null
				&& Parent.ProductStyle.Sizes.Any(styleSize => styleSize.WSZ_Size.ToUpper() == Parent.WSZ_Size.ToUpper() && styleSize.PK != Parent.PK))
			{
				Parent.WSZ_SizeInfo.AddError(Res.GetString("WhsProductStyleSizeValidation|SameSize", "Product Style Size '{0}' is already on the list.", Parent.WSZ_Size));
			}
		}

		protected new WhsProductStyleSize Parent => (WhsProductStyleSize)base.Parent;

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> info.Name != WhsProductStyleSizeSchema.Constants.WSZ_WST_ProductStyle && base.ShouldValidateFKToCancelledRecord(info);

		#endregion
	}
}

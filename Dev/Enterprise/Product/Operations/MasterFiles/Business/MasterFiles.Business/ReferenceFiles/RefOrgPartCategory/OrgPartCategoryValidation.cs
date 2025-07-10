//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgPartCategoryValidation
//
//    This class should be used for overriding validation in AutoOrgPartCategoryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
namespace Enterprise.MasterFiles.Business
{
	public class OrgPartCategoryValidation : AutoOrgPartCategoryValidation
	{
		public OrgPartCategoryValidation(AutoOrgPartCategory parent)
			: base(parent)
		{
		}

		#region CheckOPC_OPC_Parent

		protected override void CheckOPC_OPC_Parent()
		{
			base.CheckOPC_OPC_Parent();

			if (!Parent.OPC_OPC_Parent.IsEmpty && !Parent.OPC_OPC_Parent.IsMissing && Parent.OPC_OPC_Parent.IsValid)
			{
				var currentCategory = (OrgPartCategory)Parent;
				var parentCategory = Parent.Parent;

				if (ProductCategoryHelper.IsCategroyASubCategoryOfCurrentCategory(parentCategory, currentCategory))
				{
					Parent.OPC_OPC_ParentInfo.AddError(ResString.GetMultilingualString("1D74C5A6-7033-4270-AB60-AE02B9DD3049", "Invalid parent category."));
				}
			}
		}

		#endregion

		#region CheckOPC_CategoryDescription

		protected override void CheckOPC_CategoryDescription()
		{
			base.CheckOPC_CategoryDescription();
			TranslatableDataFieldAttribute.Validate(Parent.OPC_CategoryDescriptionInfo);
		}

		#endregion

		#region ProductCategoryHelper

		protected OrgPartProductCategoryHelper ProductCategoryHelper
		{
			get { return categoryHelper ?? (categoryHelper = new OrgPartProductCategoryHelper(Parent.Factory)); }
		}
		OrgPartProductCategoryHelper categoryHelper;

		#endregion
	}
}

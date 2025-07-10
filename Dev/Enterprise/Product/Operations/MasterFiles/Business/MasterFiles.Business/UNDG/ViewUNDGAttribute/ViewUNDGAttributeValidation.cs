//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewUNDGAttributeValidation
//
//    This class should be used for overriding validation in AutoViewUNDGAttributeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;

	public class ViewUNDGAttributeValidation : AutoViewUNDGAttributeValidation
	{
		public ViewUNDGAttributeValidation(AutoViewUNDGAttribute parent) : base(parent)
		{
		}

		new ViewUNDGAttribute Parent
		{
			get { return (ViewUNDGAttribute)base.Parent; }
		}

		#region DA_Language

		protected override void CheckDA_Language()
		{
			base.CheckDA_Language();
			if (Parent.DA_Index.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.DA_LanguageInfo);
				ListValidation.ErrorIfInvalidCode(Parent.DA_LanguageInfo);

				if (Parent.IsValidEnglishSubstance && (!Parent.IsInDatabase || (Parent.IsInDatabase && Parent.DA_LanguageInfo.HasChanges)))
				{
					Parent.DA_LanguageInfo.AddError(Res.GetString("918e8a0e-115b-423d-99ae-3e828b51c6be", "You cannot add new English Dangerous Goods Details for system defined dangerous goods. You can only use this module to create localized language descriptions."));
				}
			}
		}

		#endregion

		#region DA_Descriptor

		protected override void CheckDA_Descriptor()
		{
			base.CheckDA_Descriptor();
			if (Parent.DA_Index.IsEmpty)
			{
				if (Parent.DA_Descriptor.Trim().IsEmpty)
				{
					Parent.DA_DescriptorInfo.AddError(string.Format("Please enter a value for {0} language. Please select {0} in the Language Filter field. Tab Name is: {1}", Parent.DA_Language, Parent.TypeDescription));
				}
			}
		}

		#endregion
	}
}

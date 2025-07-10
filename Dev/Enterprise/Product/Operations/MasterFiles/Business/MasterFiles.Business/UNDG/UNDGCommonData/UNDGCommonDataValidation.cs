using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGCommonDataValidation : AutoUNDGCommonDataValidation
	{
		public UNDGCommonDataValidation(AutoUNDGCommonData parent) : base(parent)
		{
		}

		#region DC_Language

		protected override void CheckDC_Language()
		{
			base.CheckDC_Language();
			MandatoryValidation.CheckEntered(Parent.DC_LanguageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DC_LanguageInfo);
			if (Parent.DC_Language == Core.Constants.Languages.English && (!Parent.IsInDatabase || (Parent.IsInDatabase && Parent.DC_LanguageInfo.HasChanges)))
			{
				Parent.DC_LanguageInfo.AddError(Res.GetString("f51f7d4d-bbff-465e-9508-20b395f72947", "You cannot add new English Dangerous Goods Details. You can only use this module to create localized language descriptions."));
			}
		}

		#endregion

		#region DC_Descriptor

		protected override void CheckDC_Descriptor()
		{
			base.CheckDC_Descriptor();
			MandatoryValidation.CheckEntered(Parent.DC_DescriptorInfo);
		}

		#endregion

		#region DC_Type

		protected override void CheckDC_Type()
		{
			base.CheckDC_Type();
			MandatoryValidation.CheckEntered(Parent.DC_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DC_TypeInfo);
		}

		#endregion
	}
}

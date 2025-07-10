//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenCustomAddOnValueValidation
//
//    This class should be used for overriding validation in AutoGenCustomAddOnValueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GenCustomAddOnValueValidation : AutoGenCustomAddOnValueValidation
	{
		public GenCustomAddOnValueValidation(AutoGenCustomAddOnValue parent)
			: base(parent)
		{
		}

		protected override void CheckXV_Name()
		{
			base.CheckXV_Name();
			MandatoryValidation.CheckEntered(Parent.XV_NameInfo, (IMultilingualString)ResString.GetMultilingualString("0379fb26-f50b-4c22-b2ac-a82358afc474", "Custom Field Name"));
		}

		protected override void CheckXV_Type()
		{
			base.CheckXV_Type();
			MandatoryValidation.CheckEntered(Parent.XV_TypeInfo, (IMultilingualString)ResString.GetMultilingualString("97b3a9e9-582c-4705-8cfe-c45c205d4c05", "'{0}' custom field value type", Parent.XV_Name));
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			if (info == Parent.XV_XR_RuleInfo)
			{
				//There is custom behaviour for when a rule has been marked as inactive. See UserDefinedPropertyDefinition.Validate
				return false;
			}

			return base.ShouldValidateFKToCancelledRecord(info);
		}
	}
}

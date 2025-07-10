using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccGroupsValidation : AutoAccGroupsValidation
	{
		public AccGroupsValidation(AutoAccGroups parent) : base(parent)
		{
		}

		protected override void CheckAR_Code()
		{
			base.CheckAR_Code();
			MandatoryValidation.CheckEntered(Parent.AR_CodeInfo);
			if (!Parent.AR_CodeInfo.HasErrors())
			{
				CheckAR_CodeIsAlphaNumeric();
			}
		}

		void CheckAR_CodeIsAlphaNumeric()
		{
			Regex nonAlphaNumericRegex = new Regex(@"^[a-z0-9]+\s*$", RegexOptions.IgnoreCase);
			if (!nonAlphaNumericRegex.IsMatch(Parent.AR_Code))
			{
				Parent.AR_CodeInfo.AddError(Res.GetString("4b82c671-65f3-4961-b3fc-94ff2e8ee6fb", "{0} is not a valid Code.", Parent.AR_Code));
			}
		}

		protected override void CheckAR_Desc()
		{
			base.CheckAR_Desc();
			MandatoryValidation.CheckEntered(Parent.AR_DescInfo);
			TranslatableDataFieldAttribute.Validate(Parent.AR_DescInfo);
		}
	}
}

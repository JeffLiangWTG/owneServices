//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusClassificationValidation
//
//    This class should be used for overriding validation in AutoCusClassificationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Customs.Business
{
	public class CusClassificationValidation : AutoCusClassificationValidation
	{
		public CusClassificationValidation(AutoCusClassification parent) : base(parent)
		{
		}

		public new BaseCusClassification Parent
		{
			get { return base.Parent as BaseCusClassification; }
		}

		protected override void CheckCC_LookupCode()
		{
			base.CheckCC_LookupCode();
			if (Parent.CC_LookupCode.IsEmpty)
			{
				Parent.CC_LookupCodeInfo.AddError(Res.GetString("2d34c5f7-29d8-4f77-ae31-94a397e4d01b", "Please enter a Lookup Code."));
			}
			else
			{
				Parent.Lookups.Classifications.Load(Parent.ClassTypeLookupCodeFilter);
				if (Parent.Lookups.Classifications.Count > 1)
				{
					Parent.CC_LookupCodeInfo.AddError(Res.GetString("b7738e8d-0fa4-447f-b66b-4f2f9ff50f97", "There is already a record with Lookup Code, {0}", Parent.CC_LookupCode));
				}
			}
		}

		protected override void CheckCC_TariffNum()
		{
			base.CheckCC_TariffNum();
			MandatoryValidation.CheckEntered(Parent.CC_TariffNumInfo);
		}

		protected override void CheckCC_Description()
		{
			base.CheckCC_Description();
			MandatoryValidation.CheckEntered(Parent.CC_DescriptionInfo);
		}
		protected override void CheckCC_IsActive()
		{
			base.CheckCC_IsActive();
			var parent = Parent;
			if (!parent.CC_IsActive && parent.IsInDatabase && parent.CC_IsActiveInfo.HasChanges)
			{
				if (!Env.Security.CusClassificationDeactivate.IsAllowed)
				{
					Parent.CC_IsActiveInfo.AddError(Res.GetString("4628C8CD-A833-4A5E-AED2-D0206A41BF77", "You don't have rights to deactivate classifications."));
				}
			}
		}
	}
}

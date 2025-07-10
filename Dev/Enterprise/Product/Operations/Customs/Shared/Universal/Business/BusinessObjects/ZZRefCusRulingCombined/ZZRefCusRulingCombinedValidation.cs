//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusRulingCombinedValidation
//
//    This class should be used for overriding validation in AutoZZRefCusRulingCombinedValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;

	public class ZZRefCusRulingCombinedValidation : AutoZZRefCusRulingCombinedValidation
	{
		public ZZRefCusRulingCombinedValidation(AutoZZRefCusRulingCombined parent) : base(parent)
		{
		}

		protected new ZZRefCusRulingCombined Parent
		{
			get { return (ZZRefCusRulingCombined)base.Parent; }
		}

		protected override void CheckZZX_RulingType()
		{
			base.CheckZZX_RulingType();
			if (!Parent.IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZX_RulingTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ZZX_RulingTypeInfo, Parent.Lookups.RulingTypeList);
			}
		}

		protected override void CheckZZX_RulingNumber()
		{
			base.CheckZZX_RulingNumber();
			if (!Parent.IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZX_RulingNumberInfo);
			}
		}

		protected override void CheckZZX_Description()
		{
			base.CheckZZX_Description();
			if (!Parent.IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZX_DescriptionInfo);
			}
		}

		protected override void CheckZZX_OA_AppliesTo()
		{
			base.CheckZZX_OA_AppliesTo();
			if (!Parent.IsSystem
				&& !Parent.ZZX_RulingNumber.IsEmpty && !Parent.ZZX_RulingNumberInfo.HasErrors()
				&& !Parent.ZZX_RulingType.IsEmpty && !Parent.ZZX_RulingTypeInfo.HasErrors()
				&& !Parent.ZZX_StartDate.IsEmpty && !Parent.ZZX_StartDateInfo.HasErrors())
			{
				var rulingFilter = new ZQuery(ZZRefCusRulingCombinedSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				rulingFilter.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				rulingFilter.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_RulingNumber, Parent.ZZX_RulingNumber);
				rulingFilter.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_RulingType, Parent.ZZX_RulingType);
				rulingFilter.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_StartDate, Parent.ZZX_StartDate);

				if (!Parent.ZZX_OA_AppliesTo.IsEmpty)
				{
					rulingFilter.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_OA_AppliesTo, Parent.ZZX_OA_AppliesTo);
				}
				else
				{
					rulingFilter.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_OA_AppliesTo, null);
					rulingFilter.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_SystemCreateTimeUtc, SQLComparisonOperator.NotEqual, null);
				}

				if (Parent.Factory.LoadTop1<ZZRefCusRulingCombined>(rulingFilter) != null)
				{
					Parent.ZZX_OA_AppliesToInfo.AddError(ResString.GetMultilingualString
						("F28A4BAD-DBEC-4CE3-A6CF-A3EB6DFC636C", "Duplicate {0}, {1}, {2} and {3} is not allowed.",
						Parent.ZZX_RulingNumberInfo.HumanReadableName, Parent.ZZX_RulingTypeInfo.HumanReadableName, Parent.ZZX_OA_AppliesToInfo.HumanReadableName, Parent.ZZX_StartDateInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckZZX_StartDate()
		{
			base.CheckZZX_StartDate();
			if (!Parent.IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZX_StartDateInfo);
				TypeValidation.CheckValidSmallDateTime(Parent.ZZX_StartDateInfo);
				if (Parent.ZZX_StartDate > Parent.ZZX_EndDate)
				{
					Parent.ZZX_StartDateInfo.AddError(StartDateCannotBeAfterEndDate);
				}
			}
		}

		protected override void CheckZZX_StartDateIsNotEmpty()
		{
		}

		protected override void CheckZZX_StartDateIsValidZDateRange()
		{
		}

		protected override void CheckZZX_StartDateIsValidZDate()
		{
		}

		internal static string StartDateCannotBeAfterEndDate
		{
			get { return ResString.GetMultilingualString("D83B9F84-AF7B-4B0C-B24D-C91F7D95C9FA", "Start Date cannot be after End Date."); }
		}

		protected override void CheckZZX_EndDate()
		{
			base.CheckZZX_EndDate();
			if (!Parent.IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZX_EndDateInfo);
				TypeValidation.CheckValidSmallDateTime(Parent.ZZX_EndDateInfo);
				ValidateZZX_StartDate();
			}
		}

		protected override void CheckZZX_EndDateIsNotEmpty()
		{
		}

		protected override void CheckZZX_EndDateIsValidZDate()
		{
		}

		protected override void CheckZZX_EndDateIsValidZDateRange()
		{
		}
	}
}

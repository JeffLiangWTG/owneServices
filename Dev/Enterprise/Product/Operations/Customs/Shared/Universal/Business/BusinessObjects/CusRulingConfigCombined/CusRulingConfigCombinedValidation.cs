//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusRulingConfigCombinedValidation
//
//    This class should be used for overriding validation in AutoZZRefCusRulingConfigCombinedValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public class CusRulingConfigCombinedValidation : AutoCusRulingConfigCombinedValidation
	{
		public CusRulingConfigCombinedValidation(AutoCusRulingConfigCombined parent) : base(parent)
		{
		}

		protected new CusRulingConfigCombined Parent
		{
			get { return (CusRulingConfigCombined)base.Parent; }
		}

		protected override void CheckZZY_Category()
		{
			base.CheckZZY_Category();
			if (!Parent.IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZY_CategoryInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ZZY_CategoryInfo, Parent.Lookups.RulingConfigCategoryList);
			}
		}

		protected override void CheckZZY_Type()
		{
			base.CheckZZY_Type();
			if (!Parent.IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZY_TypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ZZY_TypeInfo, Parent.Lookups.RulingConfigTypeList);
			}
		}

		protected override void CheckZZY_Rate()
		{
			base.CheckZZY_Rate();
			if (!Parent.IsSystem && !Parent.IsRateReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.ZZY_RateInfo);
			}
		}

		#region Validation Methods

		protected void ValidateCategoryAndTypeUniqueness(ZPropertyInfo info, IEnumerable<CusRulingConfigCombined> configs, string description = null)
		{
			if (!info.Value.IsEmpty)
			{
				if (configs?.Any(x => x.PK != Parent.PK && x.ZZY_Category == Parent.ZZY_Category && x.ZZY_Type == Parent.ZZY_Type) ?? ZBool.False)
				{
					info.AddError(description ?? ResString.GetMultilingualString("0AFD6819-8543-4F01-B134-D26ED3990435", "Duplicate configuration records with the same Category and Type are not permitted."));
				}
			}
		}

		protected void ValidateTwoTypesCouldNotBeSelectedBoth(ZPropertyInfo typeInfo, ZString category, ZString firstType, ZString secondType, IEnumerable<CusRulingConfigCombined> configs, string description = null)
		{
			if (Parent.ZZY_Type == firstType || Parent.ZZY_Type == secondType)
			{
				if (configs?.Any(x => x.ZZY_Category == category && x.ZZY_Type != Parent.ZZY_Type && (x.ZZY_Type == secondType || x.ZZY_Type == firstType)) ?? ZBool.False)
				{
					typeInfo.AddError(description ?? ResString.GetMultilingualString("4D1631DA-0C34-4C25-8BBE-28BE2125F8FD", "{0} and {1} options cannot both be selected with same Category {2}.", firstType, secondType, Parent.CategoryDescription));
				}
			}
		}

		protected void ValidateOtherRulingConfigs(Action<CusRulingConfigCombined> validateAction, IEnumerable<CusRulingConfigCombined> configs)
		{
			configs?.Where(x => x.PK != Parent.PK).ForEach(validateAction);
		}

		#endregion
	}
}

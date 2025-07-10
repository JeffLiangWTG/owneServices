//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefShippingLineValidation
//
//    This class should be used for overriding validation in AutoRefShippingLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public class RefShippingLineValidation : AutoRefShippingLineValidation
	{
		public RefShippingLineValidation(AutoRefShippingLine parent)
			: base(parent)
		{
		}

		protected new RefShippingLine Parent => (RefShippingLine)base.Parent;

		public static string IsActiveErrorMessage => Res.GetString("949D39B5-E53F-43A9-9CE7-CB72352A826E", "All non-system created reference files will be marked as In-Active. If you wish to register a new carrier, please raise a CR8 service request.");

		protected override void CheckRSL_IsActive()
		{
			base.CheckRSL_IsActive();
			ValidateIsActive(Parent.RSL_IsSystem, Parent.RSL_IsActive, Parent.RSL_IsActiveInfo);
		}

		void ValidateIsActive(bool isSystem, bool isActive, ZPropertyInfo info)
		{
			if (!isSystem && isActive)
			{
				info.AddError(IsActiveErrorMessage);
			}
		}

		protected override void CheckRSL_StandardCarrierAlphaCode()
		{
			base.CheckRSL_StandardCarrierAlphaCode();
			if (Parent.RSL_IsSystem || !Parent.RSL_StandardCarrierAlphaCode.IsEmpty)
			{
				ValidateTextLength(AutoRefShippingLine.Schema.RSL_StandardCarrierAlphaCodeMaxLength, Parent.RSL_StandardCarrierAlphaCodeInfo);
			}
			EnglishStrictCharactersValidation.ErrorIfNotEnglish(Parent.RSL_StandardCarrierAlphaCodeInfo);
		}

		protected override void CheckRSL_CarrierName()
		{
			base.CheckRSL_CargoWiseOneCode();
			MandatoryValidation.CheckEntered(Parent.RSL_CarrierNameInfo);
		}

		protected override void CheckRSL_CargoWiseOneCode()
		{
			base.CheckRSL_CargoWiseOneCode();
			MandatoryValidation.CheckEntered(Parent.RSL_CargoWiseOneCodeInfo);
			ValidateTextLength(AutoRefShippingLine.Schema.RSL_CargoWiseOneCodeMaxLength, Parent.RSL_CargoWiseOneCodeInfo);
			EnglishStrictCharactersValidation.ErrorIfNotEnglish(Parent.RSL_CargoWiseOneCodeInfo);
			ValidateDuplicateRSL_CargoWiseOneCode();
		}

		void ValidateTextLength(int standardLength, ZPropertyInfo info)
		{
			if (((ZString)info.Value).Length != standardLength)
			{
				info.AddError(Res.GetString("821DD5D5-FE1B-434C-AE18-0AED4E35BF97", "{0} must have {1} characters.", info.HumanReadableName, standardLength));
			}
		}

		void ValidateDuplicateRSL_CargoWiseOneCode()
		{
			var query = new ZQuery(RefShippingLineSchema.RSL_CargoWiseOneCode, Parent.RSL_CargoWiseOneCode);
			query.AddToFilter(JoinCondition.And, RefShippingLineSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(JoinCondition.And, RefShippingLineSchema.RSL_IsActive, SQLComparisonOperator.Equal, ZBool.True);
			if (Parent.Factory.ExistsInDatabase(nameof(RefShippingLine), query) && Parent.RSL_IsActive)
			{
				Parent.RSL_CargoWiseOneCodeInfo.AddError(ResString.GetMultilingualString("93ACA7DA-973E-456F-A301-47B4CE73BD5C",
					"{0} duplicates are allowed only if the shipping line is inactive.", Parent.RSL_CargoWiseOneCodeInfo.HumanReadableName));
			}
		}

		#region Implementation

		protected override void CheckRSL_CargoWiseOneCodeIsWesternEuropean() { }
		protected override void CheckRSL_StandardCarrierAlphaCodeIsWesternEuropean() { }

		#endregion
	}
}

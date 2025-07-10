using System.Collections.Generic;
//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusCodeListCombinedValidation
//
//    This class should be used for overriding validation in AutoZZRefCusCodeListCombinedValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using ZArchitecture.Schema;

	public class ZZRefCusCodeListCombinedValidation : AutoZZRefCusCodeListCombinedValidation
	{
		public ZZRefCusCodeListCombinedValidation(AutoZZRefCusCodeListCombined parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckMultipleIdenticalAttributeValueRecords();
		}

		protected new ZZRefCusCodeListCombined Parent => (ZZRefCusCodeListCombined)base.Parent;

		protected override void CheckZZD_IsSystem()
		{
			base.CheckZZD_IsSystem();
			ValidateZZD_Code();
			ValidateZZD_CodeType();
			ValidateZZD_Description();
			ValidateZZD_CountryOrGrouping();
			ValidateZZD_StartDate();
			ValidateZZD_EndDate();
		}

		protected override void CheckZZD_CodeType()
		{
			base.CheckZZD_CodeType();
			if (!Parent.ZZD_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZD_CodeTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ZZD_CodeTypeInfo, Parent.Lookups.CodeTypeList);
				ValidateZZD_CountryOrGrouping();
				ValidateMandatoryAttributeExists();
				ValidateCodeTypeReadOnly();
			}
		}

		void ValidateCodeTypeReadOnly()
		{
			if (!Parent.ZZD_IsSystem)
			{
				if (Parent.CusCodeType?.ZZK_IsReadonly ?? false)
				{
					Parent.ZZD_CodeTypeInfo.AddError(CodeTypeIsNotValidForReadOnly(Parent.ZZD_CodeType));
				}
			}
		}

		void ValidateMandatoryAttributeExists()
		{
			if (!Parent.ZZD_CodeTypeInfo.HasErrors())
			{
				var attributeNames = Parent.GetCodeListAttributeNames()?.Where(x => x.ZXE_IsMandatory && Parent.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().All(y => !y.ZZE_ZXE_NKName.EqualsIgnoringCase(x.ZXE_Name)));
				if (attributeNames != null && attributeNames.Any())
				{
					Parent.ZZD_CodeTypeInfo.AddError(MandatoryAttributeNameRequired(attributeNames));
				}
			}
		}

		string MandatoryAttributeNameRequired(IEnumerable<RefCusCodeListAttributeName> attributeNames)
		{
			var descriptions = string.Join("', '", attributeNames.Select(x => x.ZXE_Description));
			return Res.GetString("{BE6E0615-F697-40BF-8385-4133B8116DC1}", "Mandatory attribute name '{0}' is required.", descriptions);
		}

		protected override void CheckZZD_CountryOrGrouping()
		{
			base.CheckZZD_CountryOrGrouping();
			if (!Parent.ZZD_IsSystem)
			{
				if (Parent.ZZD_CountryOrGrouping != Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping)
				{
					MandatoryValidation.CheckEntered(Parent.ZZD_CountryOrGroupingInfo);
					ListValidation.ErrorIfInvalidCode(Parent.ZZD_CountryOrGroupingInfo, Parent.Lookups.CountryOrGroupingList);
				}
				ValidateTransportModes();
				ValidateZZD_Code();
			}
		}

		internal static string CodeTypeIsNotValidForReadOnly(ZString codeType)
		{
			return Res.GetString("{fc0bb8fe-72b3-40f8-b526-12dafdce3ba4}", "This List Type '{0}' is read only.", codeType);
		}

		protected override void CheckZZD_Code()
		{
			base.CheckZZD_Code();
			if (!Parent.ZZD_IsSystem)
			{
				var code = Parent.ZZD_Code;
				var codeType = Parent.ZZD_CodeType;
				var country = Parent.ZZD_CountryOrGrouping;
				var query = new ZQuery(ZZRefCusCodeListSchema.ZZD_Code, code);
				query.AddToFilter(ZZRefCusCodeListSchema.ZZD_CodeType, codeType);
				query.AddToFilter(ZZRefCusCodeListSchema.ZZD_CountryOrGrouping, country);
				query.AddToFilter(ZZRefCusCodeListSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var matchingBO = Parent.Factory.LoadTop1<Internal.ZZRefCusCodeList>(query);
				if (matchingBO != null && matchingBO.PK != Parent.PK)
				{
					Parent.ZZD_CodeInfo.AddError(Res.GetString("A95CD0E2-3AB4-4BD7-B738-7AE82C4A102A", "This Code '{0}', Code Type '{1}' and Country/Region or Grouping '{2}' already exists.", code, codeType, country));
				}

				MandatoryValidation.CheckEntered(Parent.ZZD_CodeInfo);

				var cusCodeType = Parent.CusCodeType;
				if (cusCodeType != null)
				{
					var maxLength = cusCodeType.ZZK_MaxLength;
					if (maxLength > 0 && code.Length > maxLength)
					{
						Parent.ZZD_CodeInfo.AddError(Res.GetString("16D67179-9E7E-41B2-9B56-02AAE364A767", "This Code length exceeds the max length {0}.", maxLength));
					}
				}
			}
		}

		protected override void CheckZZD_Description()
		{
			base.CheckZZD_Description();
			if (!Parent.ZZD_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZD_DescriptionInfo);
			}
		}

		protected override void CheckZZD_StartDate()
		{
			base.CheckZZD_StartDate();
			if (!Parent.ZZD_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZD_StartDateInfo);
				TypeValidation.CheckValidSmallDateTime(Parent.ZZD_StartDateInfo);
				if (Parent.ZZD_StartDate > Parent.ZZD_EndDate)
				{
					Parent.ZZD_StartDateInfo.AddError(StartDateCannotBeAfterEndDate);
				}
			}
		}

		protected override void CheckZZD_StartDateIsNotEmpty()
		{
		}

		protected override void CheckZZD_StartDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckZZD_StartDateIsValidZDateTime()
		{
		}

		internal static string StartDateCannotBeAfterEndDate
		{
			get { return ResString.GetMultilingualString("{4E5A4997-3067-4663-A35F-6EEBB8253FA7}", "Start Date cannot be after End Date."); }
		}

		protected override void CheckZZD_EndDate()
		{
			base.CheckZZD_EndDate();
			if (!Parent.ZZD_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZD_EndDateInfo);
				TypeValidation.CheckValidSmallDateTime(Parent.ZZD_EndDateInfo);
				ValidateZZD_StartDate();
			}
		}

		protected override void CheckZZD_EndDateIsNotEmpty()
		{
		}

		protected override void CheckZZD_EndDateIsValidZDateTime()
		{
		}

		protected override void CheckZZD_EndDateIsValidZDateTimeRange()
		{
		}

		#region Validate Transport Modes

		void ValidateTransportModes()
		{
			ValidateZZD_IsAir();
			ValidateZZD_IsFix();
			ValidateZZD_IsInw();
			ValidateZZD_IsMai();
			ValidateZZD_IsRai();
			ValidateZZD_IsRoa();
			ValidateZZD_IsSea();
		}

		protected override void CheckZZD_IsAir()
		{
			base.CheckZZD_IsAir();
			EnsureSupportTransportModes(Parent.ZZD_IsAirInfo);
		}

		protected override void CheckZZD_IsFix()
		{
			base.CheckZZD_IsFix();
			EnsureSupportTransportModes(Parent.ZZD_IsFixInfo);
		}

		protected override void CheckZZD_IsInw()
		{
			base.CheckZZD_IsInw();
			EnsureSupportTransportModes(Parent.ZZD_IsInwInfo);
		}

		protected override void CheckZZD_IsMai()
		{
			base.CheckZZD_IsMai();
			EnsureSupportTransportModes(Parent.ZZD_IsMaiInfo);
		}

		protected override void CheckZZD_IsRai()
		{
			base.CheckZZD_IsRai();
			EnsureSupportTransportModes(Parent.ZZD_IsRaiInfo);
		}

		protected override void CheckZZD_IsRoa()
		{
			base.CheckZZD_IsRoa();
			EnsureSupportTransportModes(Parent.ZZD_IsRoaInfo);
		}

		protected override void CheckZZD_IsSea()
		{
			base.CheckZZD_IsSea();
			EnsureSupportTransportModes(Parent.ZZD_IsSeaInfo);
		}

		void EnsureSupportTransportModes(ZPropertyInfo propertyInfo)
		{
			var codeType = Parent.ZZD_CodeType;
			var country = Parent.ZZD_CountryOrGrouping;

			if (!Parent.ZZD_IsSystem && (ZBool)propertyInfo.Value
				&& !RefTransportModesHelper.ExistsTransportModesForThatCodeTypeAndCountryOrGroupInZZDatabase(Parent.Factory, codeType, country))
			{
				propertyInfo.AddError(DoesNotSupportTransportModes(codeType, country));
			}
		}

		internal static string DoesNotSupportTransportModes(ZString codeType, ZString country)
		{
			return Res.GetString("bc70f682-a711-4ac4-afcd-aa1e95a4ec26", "List Type {0}/{1} does not support Transport Modes", codeType, country);
		}

		#endregion

		void CheckMultipleIdenticalAttributeValueRecords() => new RefCusCodeListMultipleIdenticalAttributeValueRecordsValidator(Parent).CheckMultipleIdenticalAttributeValueRecords();
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCarrierCombinedValidation
//
//    This class should be used for overriding validation in AutoZZRefCarrierCombinedValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Schema;

	public class ZZRefCarrierCombinedValidation : AutoZZRefCarrierCombinedValidation
	{
		public ZZRefCarrierCombinedValidation(AutoZZRefCarrierCombined parent)
			: base(parent)
		{ }

		protected new ZZRefCarrierCombined Parent => (ZZRefCarrierCombined)base.Parent;

		protected override void CheckZZ4_Code()
		{
			base.CheckZZ4_Code();
			if (!Parent.ZZ4_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZ4_CodeInfo);
				var uniqueError = ValidateUnique(Parent.ZZ4_CountryOrGrouping, Parent.ZZ4_Code);
				if (!uniqueError.IsEmpty)
				{
					Parent.ZZ4_CodeInfo.AddError(uniqueError);
				}
				ValidateMandatoryAttributes();
			}
		}

		protected override void CheckZZ4_CountryOrGrouping()
		{
			base.CheckZZ4_CountryOrGrouping();
			if (!Parent.ZZ4_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZ4_CountryOrGroupingInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ZZ4_CountryOrGroupingInfo, Parent.Lookups.CountryOrGroupingList);

				var uniqueError = ValidateUnique(Parent.ZZ4_CountryOrGrouping, Parent.ZZ4_Code);
				if (!uniqueError.IsEmpty)
				{
					Parent.ZZ4_CountryOrGroupingInfo.AddError(uniqueError);
				}

				ValidateZZ4_IsAir();
				ValidateZZ4_IsSea();
				ValidateZZ4_IsRoad();
				ValidateZZ4_IsRail();
			}
		}

		ZString ValidateUnique(ZString country, ZString code)
		{
			ZString result = ZString.Empty;
			if (!country.IsEmpty && !code.IsEmpty)
			{
				var query = new ZQuery(ZZRefCarrierSchema.ZZ4_Code, code);
				query.AddToFilter(ZZRefCarrierSchema.ZZ4_CountryOrGrouping, country);
				query.AddToFilter(ZZRefCarrierSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var matchingBO = Parent.Factory.LoadTop1<Internal.ZZRefCarrier>(query);
				if (matchingBO != null && matchingBO.PK != Parent.PK)
				{
					result = Res.GetString("F833726A-34AB-40D5-97DA-46263770A24A", "This Code '{0}' and Country/Region or Grouping '{1}' already exists.", code, country);
				}
			}

			return result;
		}

		protected override void CheckZZ4_Description()
		{
			base.CheckZZ4_Description();
			if (!Parent.ZZ4_IsSystem)
			{
				MandatoryValidation.CheckEntered(Parent.ZZ4_DescriptionInfo);
			}
		}

		protected override void CheckZZ4_IsAir()
		{
			base.CheckZZ4_IsAir();
			ValidateTransportModeAndAttributes(Parent.ZZ4_IsAirInfo, RefTransportModeList.Codes.AIR);
		}

		protected override void CheckZZ4_IsSea()
		{
			base.CheckZZ4_IsSea();
			ValidateTransportModeAndAttributes(Parent.ZZ4_IsSeaInfo, RefTransportModeList.Codes.SEA);
		}

		protected override void CheckZZ4_IsRail()
		{
			base.CheckZZ4_IsRail();
			ValidateTransportModeAndAttributes(Parent.ZZ4_IsRailInfo, RefTransportModeList.Codes.RAI);
		}

		protected override void CheckZZ4_IsRoad()
		{
			base.CheckZZ4_IsRoad();
			ValidateTransportModeAndAttributes(Parent.ZZ4_IsRoadInfo, RefTransportModeList.Codes.ROA);
		}

		void ValidateTransportModeAndAttributes(ZPropertyInfo targetInfo, ZString attributeName)
		{
			if (!Parent.ZZ4_IsSystem)
			{
				var country = Parent.ZZ4_CountryOrGrouping;
				var isTransportModeSelected = (ZBool)targetInfo.Value;
				var existedAttribute = RefCarrierHelper.GetAttributes(Parent.Factory, country).Any(x => x.ZZG_Name.EqualsIgnoringCase(attributeName));

				if (existedAttribute)
				{
					if (!isTransportModeSelected && Parent.Attributes.HasAttribute(attributeName))
					{
						targetInfo.AddError(Res.GetString("152EE5E8-5254-4EEF-9560-68D7A3B92776", "{0} Transport Modes should be selected when there is {0} attribute for {1}", attributeName, country));
					}
					if (isTransportModeSelected && !Parent.Attributes.HasAttribute(attributeName))
					{
						targetInfo.AddError(Res.GetString("188CCB67-9180-4AE0-BB34-A42F88D14AF7", "There should be {0} attribute when {0} Transport Modes is selected for {1}", attributeName, country));
					}
				}
			}
		}

		void ValidateMandatoryAttributes()
		{
			var country = Parent.ZZ4_CountryOrGrouping;
			var mandatoryAttributes = RefCarrierHelper.GetConfig(country)?.MandatoryAttributes;
			var existingAttributes = Parent.Attributes.Select(a => a.ZZG_Name);
			if (mandatoryAttributes != null && !mandatoryAttributes.Intersect(existingAttributes).Any())
			{
				Parent.ZZ4_CodeInfo.AddError(Res.GetString("E5630743-1447-4DF9-BB86-4BA6F794D657", "At least one attribute must be selected."));
			}
		}
	}
}

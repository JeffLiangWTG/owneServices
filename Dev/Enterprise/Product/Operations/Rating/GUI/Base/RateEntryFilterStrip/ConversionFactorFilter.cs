using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI
{
	public class ConversionFactorFilter : ModuleTextFilter
	{
		readonly ConversionFactorViewModel conversionFactorForBinding;
		delegate ZQuery GetConversionFactorQuery(ConversionFactor value);

		public ConversionFactorFilter()
			: base((ZString)RateLineModuleFilters.Constants.Codes.ConversionFactor, GetStaticDummyQueryForConstructor, GetLookupsList)
		{
			QueryDelegate = (GetConversionFactorQuery)GetQuery;
			ErrorOnCodeNotPresent = false;
			MaxLength = 15;
			conversionFactorForBinding = new ConversionFactorViewModel(p => new ConversionFactorLookups(p), p => new ConversionFactorValidationAllowingBlank(p));
		}

		static IList GetLookupsList()
			=> new ConversionFactorLookups(null).ConversionFactors;

		static ZQuery GetQuery(ConversionFactor value)
		{
			var query = new ZQuery(RateLinesSchema.TL_ConversionFactor, value.Factor);
			query.AddToFilter(RateLinesSchema.TL_FactorNumerator, value.NumeratorUnit);
			query.AddToFilter(RateLinesSchema.TL_FactorDenominator, value.DenominatorUnit);
			return query;
		}

		static ZQuery GetStaticDummyQueryForConstructor(ZString s) => null;

		public override bool HasComparisonOperator => false;

		protected override object[] QueryDelegateParameters => new object[] { conversionFactorForBinding.ConversionFactor };

		public override ZString Property
		{
			get => conversionFactorForBinding.ConversionFactorString;
			set
			{
				conversionFactorForBinding.ConversionFactorString = value;
				PropertyInfo.RefreshBinding();
			}
		}

		public new ZPropertyInfo PropertyInfo
			=> GetWrappedZPropertyInfo(nameof(PropertyInfo), x => conversionFactorForBinding.ConversionFactorStringInfo);

		class ConversionFactorValidationAllowingBlank : ConversionFactorValidation
		{
			public ConversionFactorValidationAllowingBlank(ConversionFactorViewModel parent) : base(parent)
			{
			}

			protected override void CheckConversionFactorString()
			{
				if (!Parent.ConversionFactorString.IsEmpty)
				{
					base.CheckConversionFactorString();
				}
			}
		}
	}
}
